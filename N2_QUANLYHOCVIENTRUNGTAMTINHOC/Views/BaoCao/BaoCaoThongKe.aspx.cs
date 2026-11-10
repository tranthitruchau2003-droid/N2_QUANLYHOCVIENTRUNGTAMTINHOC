using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;
using N2_QUANLYHOCVIENTRUNGTAMTINHOC.DAL;

namespace N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.BaoCao
{
    public partial class BaoCaoThongKe : System.Web.UI.Page
    {
        public string JsonKHLabels = "[]";
        public string JsonKHSoLuong = "[]";
        public string JsonKHDoanhThu = "[]";
        public string JsonTNLabels = "[]";
        public string JsonTNDoanhThu = "[]";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                TaiBoLocKhoaVaLop();
                TaiNam();
                TaiTatCaDuLieu();
            }
        }

        private void TaiTatCaDuLieu()
        {
            TaiDSHocVienTheoBoLoc();
            TaiCongNo(txtTimNo.Text.Trim());
            TaiThongKeKhoaHoc();
            TaiBieuDoThangNam();
        }

        private void TaiBoLocKhoaVaLop()
        {
            DataTable dtKH = kncsdl.DocDuLieu("SELECT makhoahoc, tenkhoahoc FROM KHOAHOC ORDER BY tenkhoahoc");
            ddlLocKhoaHoc.DataSource = dtKH;
            ddlLocKhoaHoc.DataTextField = "tenkhoahoc";
            ddlLocKhoaHoc.DataValueField = "makhoahoc";
            ddlLocKhoaHoc.DataBind();
            ddlLocKhoaHoc.Items.Insert(0, new ListItem("-- Tất cả Khóa học --", ""));

            TaiDanhSachLopTheoKhoa("");
        }

        private void TaiDanhSachLopTheoKhoa(string maKH)
        {
            string sql = "SELECT malop, tenlop FROM LOPHOC WHERE (@kh = '' OR makhoahoc = @kh) ORDER BY tenlop";
            DataTable dtLop = kncsdl.DocDuLieu(sql, new[] { new SqlParameter("@kh", maKH ?? "") });
            ddlLocLopHoc.DataSource = dtLop;
            ddlLocLopHoc.DataTextField = "tenlop";
            ddlLocLopHoc.DataValueField = "malop";
            ddlLocLopHoc.DataBind();
            ddlLocLopHoc.Items.Insert(0, new ListItem("-- Tất cả Lớp học --", ""));
        }

        private DataTable LayDuLieuHocVienLoc()
        {
            string sql = @"SELECT dk.madangky, hv.mahocvien, hv.hoten, hv.dienthoai, hv.email,
                                  kh.makhoahoc, kh.tenkhoahoc,
                                  ISNULL(STUFF((SELECT ', ' + l.tenlop FROM LOPHOC l WHERE l.makhoahoc = kh.makhoahoc AND (@malop = '' OR l.malop = @malop) FOR XML PATH('')), 1, 2, ''), N'Chưa xếp lớp') AS tenlop,
                                  kh.hocphi,
                                  ISNULL(SUM(pt.sotienthu), 0) AS dadong,
                                  (kh.hocphi - ISNULL(SUM(pt.sotienthu), 0)) AS conno,
                                  CASE 
                                      WHEN (kh.hocphi - ISNULL(SUM(pt.sotienthu), 0)) <= 0 THEN N'Đã đóng đủ'
                                      WHEN ISNULL(SUM(pt.sotienthu), 0) = 0 THEN N'Chưa đóng'
                                      ELSE N'Còn nợ một phần'
                                  END AS trangthaihp
                           FROM DANGKY dk
                           JOIN HOCVIEN hv ON dk.mahocvien = hv.mahocvien
                           JOIN KHOAHOC kh ON dk.makhoahoc = kh.makhoahoc
                           LEFT JOIN PHIEUTHU pt ON dk.madangky = pt.madangky
                           WHERE (@makh = '' OR kh.makhoahoc = @makh)
                             AND (@malop = '' OR EXISTS (SELECT 1 FROM LOPHOC l2 WHERE l2.makhoahoc = kh.makhoahoc AND l2.malop = @malop))
                             AND (@kw = '' OR hv.mahocvien LIKE @kw OR hv.hoten LIKE @kw OR hv.dienthoai LIKE @kw OR dk.madangky LIKE @kw)
                           GROUP BY dk.madangky, hv.mahocvien, hv.hoten, hv.dienthoai, hv.email, kh.makhoahoc, kh.tenkhoahoc, kh.hocphi
                           HAVING (@tt = '' 
                               OR (@tt = 'DaDongDu' AND (kh.hocphi - ISNULL(SUM(pt.sotienthu), 0)) <= 0)
                               OR (@tt = 'ConNo' AND (kh.hocphi - ISNULL(SUM(pt.sotienthu), 0)) > 0)
                               OR (@tt = 'ChuaDong' AND ISNULL(SUM(pt.sotienthu), 0) = 0))
                           ORDER BY dk.madangky DESC";

            SqlParameter[] p = {
                new SqlParameter("@makh", ddlLocKhoaHoc.SelectedValue ?? ""),
                new SqlParameter("@malop", ddlLocLopHoc.SelectedValue ?? ""),
                new SqlParameter("@tt", ddlLocTrangThaiHP.SelectedValue ?? ""),
                new SqlParameter("@kw", string.IsNullOrWhiteSpace(txtTimHVLoc.Text) ? "" : "%" + txtTimHVLoc.Text.Trim() + "%")
            };
            return kncsdl.DocDuLieu(sql, p);
        }

        private void TaiDSHocVienTheoBoLoc()
        {
            gvDSHocVienLoc.DataSource = LayDuLieuHocVienLoc();
            gvDSHocVienLoc.DataBind();
        }

        protected void ddlLocKhoaHoc_SelectedIndexChanged(object sender, EventArgs e)
        {
            TaiDanhSachLopTheoKhoa(ddlLocKhoaHoc.SelectedValue);
            TaiTatCaDuLieu();
        }

        protected void BoLocHocVien_Changed(object sender, EventArgs e)
        {
            TaiTatCaDuLieu();
        }

        private void TaiNam()
        {
            DataTable dt = kncsdl.DocDuLieu("SELECT DISTINCT YEAR(ngaythu) AS Nam FROM PHIEUTHU ORDER BY Nam DESC");
            ddlNam.Items.Clear();
            if (dt.Rows.Count == 0)
            {
                ddlNam.Items.Add(DateTime.Now.Year.ToString());
            }
            else
            {
                foreach (DataRow r in dt.Rows)
                {
                    ddlNam.Items.Add(r["Nam"].ToString());
                }
            }
        }

        private DataTable LayDuLieuCongNo(string tuKhoa = "")
        {
            string sql = @"SELECT dk.madangky, hv.hoten, hv.dienthoai, kh.tenkhoahoc, kh.hocphi,
                                  ISNULL(SUM(pt.sotienthu), 0) AS dadong,
                                  (kh.hocphi - ISNULL(SUM(pt.sotienthu), 0)) AS conno
                           FROM DANGKY dk
                           JOIN HOCVIEN hv ON dk.mahocvien = hv.mahocvien
                           JOIN KHOAHOC kh ON dk.makhoahoc = kh.makhoahoc
                           LEFT JOIN PHIEUTHU pt ON dk.madangky = pt.madangky
                           WHERE (@kw = '' OR dk.madangky LIKE @kw OR hv.hoten LIKE @kw OR hv.dienthoai LIKE @kw)
                           GROUP BY dk.madangky, hv.hoten, hv.dienthoai, kh.tenkhoahoc, kh.hocphi
                           HAVING (kh.hocphi - ISNULL(SUM(pt.sotienthu), 0)) > 0
                           ORDER BY dk.madangky";
            SqlParameter[] p = { new SqlParameter("@kw", string.IsNullOrEmpty(tuKhoa) ? "" : "%" + tuKhoa + "%") };
            return kncsdl.DocDuLieu(sql, p);
        }

        private void TaiCongNo(string tuKhoa = "")
        {
            gvCongNo.DataSource = LayDuLieuCongNo(tuKhoa);
            gvCongNo.DataBind();
            TaiGoiYCongNo();
        }

        private void TaiGoiYCongNo()
        {
            DataTable dt = LayDuLieuCongNo("");
            StringBuilder sb = new StringBuilder();
            foreach (DataRow r in dt.Rows)
            {
                sb.AppendFormat("<button type=\"button\" class=\"list-group-item list-group-item-action py-2 px-2 small goi-y-item\" data-value=\"{1}\"><strong>{0}</strong> - {1} ({2})</button>",
                    r["madangky"], r["hoten"], r["dienthoai"]);
            }
            litGoiYNo.Text = sb.ToString();
        }

        private DataTable LayDuLieuThongKeKhoaHoc()
        {
            string sql = @"SELECT kh.makhoahoc, kh.tenkhoahoc,
                                  COUNT(DISTINCT dk.madangky) AS soluongdk,
                                  ISNULL(SUM(pt.sotienthu), 0) AS tongthucthu
                           FROM KHOAHOC kh
                           LEFT JOIN DANGKY dk ON kh.makhoahoc = dk.makhoahoc
                           LEFT JOIN PHIEUTHU pt ON dk.madangky = pt.madangky
                           GROUP BY kh.makhoahoc, kh.tenkhoahoc
                           ORDER BY kh.makhoahoc";
            return kncsdl.DocDuLieu(sql);
        }

        private void TaiThongKeKhoaHoc()
        {
            DataTable dt = LayDuLieuThongKeKhoaHoc();
            gvThongKeKH.DataSource = dt;
            gvThongKeKH.DataBind();

            var labels = new List<string>();
            var soLuong = new List<int>();
            var doanhThu = new List<decimal>();

            foreach (DataRow r in dt.Rows)
            {
                labels.Add(r["makhoahoc"].ToString());
                soLuong.Add(Convert.ToInt32(r["soluongdk"]));
                doanhThu.Add(Convert.ToDecimal(r["tongthucthu"]));
            }

            JavaScriptSerializer js = new JavaScriptSerializer();
            JsonKHLabels = js.Serialize(labels);
            JsonKHSoLuong = js.Serialize(soLuong);
            JsonKHDoanhThu = js.Serialize(doanhThu);
        }

        private void TaiBieuDoThangNam()
        {
            int nam = int.Parse(ddlNam.SelectedValue);
            string sql = @"SELECT MONTH(ngaythu) AS Thang, ISNULL(SUM(sotienthu), 0) AS TongTien
                           FROM PHIEUTHU
                           WHERE YEAR(ngaythu) = @nam
                           GROUP BY MONTH(ngaythu)
                           ORDER BY Thang";
            DataTable dt = kncsdl.DocDuLieu(sql, new[] { new SqlParameter("@nam", nam) });

            var labels = new List<string>();
            var doanhThu = new List<decimal>();
            foreach (DataRow r in dt.Rows)
            {
                labels.Add("Tháng " + r["Thang"] + "/" + nam);
                doanhThu.Add(Convert.ToDecimal(r["TongTien"]));
            }

            JavaScriptSerializer js = new JavaScriptSerializer();
            JsonTNLabels = js.Serialize(labels);
            JsonTNDoanhThu = js.Serialize(doanhThu);
        }

        protected void btnTimNo_Click(object sender, EventArgs e)
        {
            TaiTatCaDuLieu();
        }

        protected void ddlNam_SelectedIndexChanged(object sender, EventArgs e)
        {
            TaiTatCaDuLieu();
        }

        // XUẤT FILE EXCEL DANH SÁCH HỌC VIÊN THEO KHÓA, LỚP, ĐÃ ĐÓNG / CHƯA ĐÓNG HỌC PHÍ
        protected void btnXuatDSHocVien_Click(object sender, EventArgs e)
        {
            DataTable dt = LayDuLieuHocVienLoc();
            XuatFileExcel(dt, "DanhSach_HocVien_HocPhi_" + DateTime.Now.ToString("yyyyMMdd_HHmm"),
                "DANH SÁCH HỌC VIÊN THEO KHÓA, LỚP & TÌNH TRẠNG HỌC PHÍ",
                new[] { "Mã ĐK", "Mã HV", "Họ và tên", "Điện thoại", "Khóa học", "Lớp học", "Tổng học phí", "Đã đóng", "Còn nợ", "Trạng thái" },
                new[] { "madangky", "mahocvien", "hoten", "dienthoai", "tenkhoahoc", "tenlop", "hocphi", "dadong", "conno", "trangthaihp" });
        }

        // XUẤT FILE EXCEL CÔNG NỢ
        protected void btnXuatCongNo_Click(object sender, EventArgs e)
        {
            DataTable dt = LayDuLieuCongNo(txtTimNo.Text.Trim());
            XuatFileExcel(dt, "BaoCao_CongNo_HocVien_" + DateTime.Now.ToString("yyyyMMdd"),
                "BÁO CÁO HỌC VIÊN CÒN NỢ HỌC PHÍ",
                new[] { "Mã ĐK", "Họ tên Học viên", "Điện thoại", "Khóa học", "Tổng học phí", "Đã đóng", "Còn nợ" },
                new[] { "madangky", "hoten", "dienthoai", "tenkhoahoc", "hocphi", "dadong", "conno" });
        }

        // XUẤT FILE EXCEL THỐNG KÊ KHÓA HỌC
        protected void btnXuatExcel_Click(object sender, EventArgs e)
        {
            DataTable dt = LayDuLieuThongKeKhoaHoc();
            XuatFileExcel(dt, "ThongKe_KhoaHoc_" + DateTime.Now.ToString("yyyyMMdd"),
                "THỐNG KÊ SỐ LƯỢNG HỌC VIÊN & DOANH THU THEO KHÓA HỌC",
                new[] { "Mã KH", "Tên khóa học", "Số lượng ĐK", "Tổng thực thu (VNĐ)" },
                new[] { "makhoahoc", "tenkhoahoc", "soluongdk", "tongthucthu" });
        }

        private void XuatFileExcel(DataTable dt, string fileName, string tieuDe, string[] headers, string[] columns)
        {
            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment;filename=" + fileName + ".xls");
            Response.Charset = "";
            Response.ContentType = "application/vnd.ms-excel";
            Response.ContentEncoding = Encoding.UTF8;
            Response.BinaryWrite(Encoding.UTF8.GetPreamble());

            StringBuilder sb = new StringBuilder();
            sb.Append("<html><head><meta charset='utf-8'></head><body>");
            sb.AppendFormat("<h3 style='color:#0755A6;font-family:Arial;'>{0}</h3>", tieuDe);
            sb.AppendFormat("<p style='font-family:Arial;font-size:12px;'>Ngày xuất báo cáo: {0:dd/MM/yyyy HH:mm}</p>", DateTime.Now);
            sb.Append("<table border='1' style='border-collapse:collapse;font-family:Arial;font-size:13px;'>");
            sb.Append("<tr style='background-color:#1677D2;color:white;font-weight:bold;'>");
            foreach (string h in headers)
            {
                sb.AppendFormat("<th style='padding:6px 10px;'>{0}</th>", h);
            }
            sb.Append("</tr>");

            foreach (DataRow r in dt.Rows)
            {
                sb.Append("<tr>");
                foreach (string col in columns)
                {
                    object val = r[col];
                    if (val is decimal || val is double || val is int)
                        sb.AppendFormat("<td style='padding:5px 8px;text-align:right;'>{0:N0}</td>", val);
                    else
                        sb.AppendFormat("<td style='padding:5px 8px;mso-number-format:\"\\@\";'>{0}</td>", val);
                }
                sb.Append("</tr>");
            }
            sb.Append("</table></body></html>");

            Response.Output.Write(sb.ToString());
            Response.Flush();
            Response.End();
        }
    }
}