using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;
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
            if (MasterPage.ChuanHoaQuyen(Session["Quyen"]) != "Admin" && MasterPage.ChuanHoaQuyen(Session["Quyen"]) != "NhanVien")
            {
                Response.Redirect("~/Dashboard.aspx", true);
                return;
            }

            if (!IsPostBack)
            {
                txtTimHVLoc.Text = "";
                TaiBoLocKhoaVaLop();
                TaiNam();
                TaiGoiYTimKiem();
                TaiTatCaDuLieu();
            }
        }

        private void TaiTatCaDuLieu()
        {
            TaiDSHocVienTheoBoLoc();
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

            ddlKhoaThongKe.DataSource = dtKH;
            ddlKhoaThongKe.DataTextField = "tenkhoahoc";
            ddlKhoaThongKe.DataValueField = "makhoahoc";
            ddlKhoaThongKe.DataBind();
            ddlKhoaThongKe.Items.Insert(0, new ListItem("-- Tất cả Khóa học --", ""));

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
            // Sử dụng COLLATE Latin1_General_CI_AI để tìm kiếm không dấu và không phân biệt hoa thường
            string sql = @"SELECT dk.madangky, hv.mahocvien, hv.hoten, hv.dienthoai, hv.email,
                                  kh.makhoahoc, kh.tenkhoahoc,
                                  ISNULL(l.tenlop, N'Chưa xếp lớp') AS tenlop,
                                  kh.hocphi,
                                  CASE 
                                      WHEN ISNULL(dk.trangthailop, N'') LIKE N'%Đã hoàn tiền%' OR ISNULL(dk.trangthaithanhtoan, N'') = N'Đã hoàn tiền' THEN 0
                                      ELSE ISNULL(SUM(CASE WHEN pt.sotienthu > 0 AND ISNULL(pt.noidungthu, N'') NOT LIKE N'Chờ xác nhận%' THEN pt.sotienthu ELSE 0 END), 0)
                                  END AS dadong,
                                  CASE 
                                      WHEN ISNULL(dk.trangthailop, N'') LIKE N'%Hủy lớp%' THEN 0
                                      ELSE (kh.hocphi - ISNULL(SUM(CASE WHEN pt.sotienthu > 0 AND ISNULL(pt.noidungthu, N'') NOT LIKE N'Chờ xác nhận%' THEN pt.sotienthu ELSE 0 END), 0))
                                  END AS conno,
                                  CASE 
                                      WHEN ISNULL(dk.trangthailop, N'') LIKE N'%Đã hoàn tiền%' OR ISNULL(dk.trangthaithanhtoan, N'') = N'Đã hoàn tiền' THEN N'Đã hoàn tiền'
                                      WHEN ISNULL(dk.trangthailop, N'') LIKE N'%Chờ hoàn%' OR ISNULL(dk.trangthaithanhtoan, N'') = N'Chờ hoàn tiền' THEN N'Chưa hoàn tiền (Chờ duyệt)'
                                      WHEN ISNULL(dk.trangthailop, N'') LIKE N'%Hủy lớp%' AND ISNULL(SUM(CASE WHEN pt.sotienthu > 0 AND ISNULL(pt.noidungthu, N'') NOT LIKE N'Chờ xác nhận%' THEN pt.sotienthu ELSE 0 END), 0) > 0 THEN N'Chưa hoàn tiền'
                                      WHEN ISNULL(dk.trangthailop, N'') LIKE N'%Hủy lớp%' THEN N'Đã hủy lớp (0 đ)'
                                      WHEN (kh.hocphi - ISNULL(SUM(CASE WHEN pt.sotienthu > 0 AND ISNULL(pt.noidungthu, N'') NOT LIKE N'Chờ xác nhận%' THEN pt.sotienthu ELSE 0 END), 0)) <= 0 THEN N'Đã đóng đủ'
                                      WHEN ISNULL(SUM(CASE WHEN pt.sotienthu > 0 AND ISNULL(pt.noidungthu, N'') NOT LIKE N'Chờ xác nhận%' THEN pt.sotienthu ELSE 0 END), 0) = 0 THEN N'Chưa đóng'
                                      ELSE N'Còn nợ một phần'
                                  END AS trangthaihp
                           FROM DANGKY dk
                           JOIN HOCVIEN hv ON dk.mahocvien = hv.mahocvien
                           JOIN KHOAHOC kh ON dk.makhoahoc = kh.makhoahoc
                           LEFT JOIN LOPHOC l ON dk.malop = l.malop
                           LEFT JOIN PHIEUTHU pt ON dk.madangky = pt.madangky
                           WHERE (@makh = '' OR kh.makhoahoc = @makh)
                             AND (@malop = '' OR dk.malop = @malop)
                             AND (@kw = '' OR hv.mahocvien COLLATE Latin1_General_CI_AI LIKE @kw 
                                           OR hv.hoten COLLATE Latin1_General_CI_AI LIKE @kw 
                                           OR hv.dienthoai COLLATE Latin1_General_CI_AI LIKE @kw 
                                           OR dk.madangky COLLATE Latin1_General_CI_AI LIKE @kw)
                           GROUP BY dk.madangky, hv.mahocvien, hv.hoten, hv.dienthoai, hv.email, kh.makhoahoc, kh.tenkhoahoc, l.tenlop, kh.hocphi, dk.trangthailop, dk.trangthaithanhtoan
                           HAVING (@tt = '' 
                               OR (@tt = 'DaHoanTien' AND (ISNULL(dk.trangthailop, N'') LIKE N'%Đã hoàn tiền%' OR ISNULL(dk.trangthaithanhtoan, N'') = N'Đã hoàn tiền'))
                               OR (@tt = 'ChoHoanTien' AND (ISNULL(dk.trangthailop, N'') LIKE N'%Chờ hoàn%' OR ISNULL(dk.trangthaithanhtoan, N'') = N'Chờ hoàn tiền'))
                               OR (@tt = 'ChuaHoanTien' AND ISNULL(dk.trangthailop, N'') LIKE N'%Hủy lớp%' AND ISNULL(dk.trangthailop, N'') NOT LIKE N'%Đã hoàn tiền%' AND ISNULL(dk.trangthaithanhtoan, N'') <> N'Đã hoàn tiền' AND ISNULL(SUM(CASE WHEN pt.sotienthu > 0 AND ISNULL(pt.noidungthu, N'') NOT LIKE N'Chờ xác nhận%' THEN pt.sotienthu ELSE 0 END), 0) > 0)
                               OR (@tt = 'DaHuyLop' AND ISNULL(dk.trangthailop, N'') LIKE N'%Hủy lớp%' AND ISNULL(dk.trangthailop, N'') NOT LIKE N'%Đã hoàn tiền%' AND ISNULL(dk.trangthaithanhtoan, N'') <> N'Đã hoàn tiền' AND ISNULL(SUM(CASE WHEN pt.sotienthu > 0 AND ISNULL(pt.noidungthu, N'') NOT LIKE N'Chờ xác nhận%' THEN pt.sotienthu ELSE 0 END), 0) = 0)
                               OR (@tt = 'DaDongDu' AND ISNULL(dk.trangthailop, N'') NOT LIKE N'%Hủy lớp%' AND (kh.hocphi - ISNULL(SUM(CASE WHEN pt.sotienthu > 0 AND ISNULL(pt.noidungthu, N'') NOT LIKE N'Chờ xác nhận%' THEN pt.sotienthu ELSE 0 END), 0)) <= 0)
                               OR (@tt = 'ConNoMotPhan' AND ISNULL(dk.trangthailop, N'') NOT LIKE N'%Hủy lớp%' AND ISNULL(SUM(CASE WHEN pt.sotienthu > 0 AND ISNULL(pt.noidungthu, N'') NOT LIKE N'Chờ xác nhận%' THEN pt.sotienthu ELSE 0 END), 0) > 0 AND (kh.hocphi - ISNULL(SUM(CASE WHEN pt.sotienthu > 0 AND ISNULL(pt.noidungthu, N'') NOT LIKE N'Chờ xác nhận%' THEN pt.sotienthu ELSE 0 END), 0)) > 0)
                               OR (@tt = 'ChuaDong' AND ISNULL(dk.trangthailop, N'') NOT LIKE N'%Hủy lớp%' AND ISNULL(SUM(CASE WHEN pt.sotienthu > 0 AND ISNULL(pt.noidungthu, N'') NOT LIKE N'Chờ xác nhận%' THEN pt.sotienthu ELSE 0 END), 0) = 0))
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
            gvDSHocVienLoc.PageIndex = 0;
            TaiTatCaDuLieu();
        }

        protected void BoLocHocVien_Changed(object sender, EventArgs e)
        {
            gvDSHocVienLoc.PageIndex = 0;
            TaiTatCaDuLieu();
        }

        protected void gvDSHocVienLoc_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvDSHocVienLoc.PageIndex = e.NewPageIndex;
            TaiTatCaDuLieu();
        }

        private void TaiNam()
        {
            DataTable dt = kncsdl.DocDuLieu(@"SELECT DISTINCT YEAR(ngaythu) AS Nam FROM PHIEUTHU WHERE ngaythu IS NOT NULL
                                              UNION
                                              SELECT DISTINCT YEAR(ngaydangky) AS Nam FROM DANGKY WHERE ngaydangky IS NOT NULL
                                              ORDER BY Nam DESC");
            ddlNam.Items.Clear();
            ddlNamThongKe.Items.Clear();
            ddlNamThongKe.Items.Add(new ListItem("-- Tất cả Năm --", "0"));

            if (dt.Rows.Count == 0)
            {
                string namHT = DateTime.Now.Year.ToString();
                ddlNam.Items.Add(namHT);
                ddlNamThongKe.Items.Add(new ListItem("Năm " + namHT, namHT));
            }
            else
            {
                foreach (DataRow r in dt.Rows)
                {
                    if (r["Nam"] != DBNull.Value)
                    {
                        string nam = r["Nam"].ToString();
                        ddlNam.Items.Add(nam);
                        ddlNamThongKe.Items.Add(new ListItem("Năm " + nam, nam));
                    }
                }
            }
        }

        private void TaiGoiYTimKiem()
        {
            DataTable dt = kncsdl.DocDuLieu("SELECT mahocvien, hoten, dienthoai FROM HOCVIEN ORDER BY mahocvien DESC");
            StringBuilder sb = new StringBuilder();
            foreach (DataRow r in dt.Rows)
            {
                sb.AppendFormat("<button type=\"button\" class=\"list-group-item list-group-item-action py-2 px-2 small goi-y-item\" data-value=\"{0}\"><strong>{0}</strong> - {1} ({2})</button>",
                    HttpUtility.HtmlAttributeEncode(Convert.ToString(r["mahocvien"])),
                    HttpUtility.HtmlEncode(Convert.ToString(r["hoten"])),
                    HttpUtility.HtmlEncode(Convert.ToString(r["dienthoai"])));
            }
            litGoiY.Text = sb.ToString();
        }

        private DataTable LayDuLieuThongKeKhoaHoc()
        {
            int nam = 0;
            int.TryParse(ddlNamThongKe.SelectedValue, out nam);
            string maKH = ddlKhoaThongKe.SelectedValue ?? "";

            string sql = @"SELECT kh.makhoahoc, kh.tenkhoahoc,
                                  (SELECT COUNT(*) FROM DANGKY dk 
                                   WHERE dk.makhoahoc = kh.makhoahoc 
                                     AND ISNULL(dk.trangthailop, N'') NOT LIKE N'%Hủy lớp%' 
                                     AND (@nam = 0 OR YEAR(dk.ngaydangky) = @nam)) AS soluongdk,
                                  ISNULL((SELECT SUM(pt.sotienthu) FROM PHIEUTHU pt 
                                          JOIN DANGKY dk2 ON pt.madangky = dk2.madangky 
                                          WHERE dk2.makhoahoc = kh.makhoahoc AND (@nam = 0 OR YEAR(pt.ngaythu) = @nam)
                                            AND ISNULL(pt.noidungthu, N'') NOT LIKE N'Chờ xác nhận%'
                                            AND pt.phuongthucthanhtoan <> N'Hoàn tiền học phí'), 0) AS tongthucthu
                           FROM KHOAHOC kh
                           WHERE (@makh = '' OR kh.makhoahoc = @makh)
                           ORDER BY kh.makhoahoc";

            SqlParameter[] p = {
                new SqlParameter("@nam", nam),
                new SqlParameter("@makh", maKH)
            };
            return kncsdl.DocDuLieu(sql, p);
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

        protected void BoLocThongKe_Changed(object sender, EventArgs e)
        {
            TaiTatCaDuLieu();
        }

        private void TaiBieuDoThangNam()
        {
            int nam = DateTime.Now.Year;
            int.TryParse(ddlNam.SelectedValue, out nam);

            string sql = @"SELECT MONTH(ngaythu) AS Thang, ISNULL(SUM(sotienthu), 0) AS TongTien
                           FROM PHIEUTHU
                           WHERE YEAR(ngaythu) = @nam 
                             AND ISNULL(noidungthu, N'') NOT LIKE N'Chờ xác nhận%'
                             AND phuongthucthanhtoan <> N'Hoàn tiền học phí'
                           GROUP BY MONTH(ngaythu)
                           ORDER BY Thang";
            DataTable dt = kncsdl.DocDuLieu(sql, new[] { new SqlParameter("@nam", nam) });

            decimal[] tienTheoThang = new decimal[13];
            foreach (DataRow r in dt.Rows)
            {
                int t = Convert.ToInt32(r["Thang"]);
                if (t >= 1 && t <= 12)
                    tienTheoThang[t] = Convert.ToDecimal(r["TongTien"]);
            }

            var labels = new List<string>();
            var doanhThu = new List<decimal>();
            for (int m = 1; m <= 12; m++)
            {
                labels.Add("T" + m + "/" + nam);
                doanhThu.Add(tienTheoThang[m]);
            }

            JavaScriptSerializer js = new JavaScriptSerializer();
            JsonTNLabels = js.Serialize(labels);
            JsonTNDoanhThu = js.Serialize(doanhThu);
        }

        protected void ddlNam_SelectedIndexChanged(object sender, EventArgs e)
        {
            TaiTatCaDuLieu();
        }

        protected void btnXuatDSHocVien_Click(object sender, EventArgs e)
        {
            DataTable dt = LayDuLieuHocVienLoc();
            XuatFileExcel(dt, "DanhSach_HocVien_HocPhi_" + DateTime.Now.ToString("yyyyMMdd_HHmm"),
                "DANH SÁCH HỌC VIÊN THEO KHÓA, LỚP & TÌNH TRẠNG HỌC PHÍ",
                new[] { "Mã ĐK", "Mã HV", "Họ và tên", "Điện thoại", "Khóa học", "Lớp học", "Tổng học phí", "Đã đóng", "Còn nợ", "Trạng thái" },
                new[] { "madangky", "mahocvien", "hoten", "dienthoai", "tenkhoahoc", "tenlop", "hocphi", "dadong", "conno", "trangthaihp" });
        }

        protected void btnXuatExcel_Click(object sender, EventArgs e)
        {
            DataTable dt = LayDuLieuThongKeKhoaHoc();
            string namText = ddlNamThongKe.SelectedValue == "0" ? "TatCaNam" : "Nam" + ddlNamThongKe.SelectedValue;
            string tieuDe = ddlNamThongKe.SelectedValue == "0"
                ? "THỐNG KÊ SỐ LƯỢNG HỌC VIÊN & DOANH THU THEO KHÓA HỌC"
                : $"THỐNG KÊ SỐ LƯỢNG HỌC VIÊN & DOANH THU THEO KHÓA HỌC - NĂM {ddlNamThongKe.SelectedValue}";

            XuatFileExcel(dt, $"ThongKe_KhoaHoc_{namText}_{DateTime.Now:yyyyMMdd}",
                tieuDe,
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
            sb.AppendFormat("<h3 style='color:#0755A6;font-family:Arial;'>{0}</h3>", HttpUtility.HtmlEncode(tieuDe));
            sb.AppendFormat("<p style='font-family:Arial;font-size:12px;'>Ngày xuất báo cáo: {0:dd/MM/yyyy HH:mm}</p>", DateTime.Now);
            sb.Append("<table border='1' style='border-collapse:collapse;font-family:Arial;font-size:13px;'>");
            sb.Append("<tr style='background-color:#1677D2;color:white;font-weight:bold;'>");
            foreach (string h in headers)
            {
                sb.AppendFormat("<th style='padding:6px 10px;'>{0}</th>", HttpUtility.HtmlEncode(h));
            }
            sb.Append("</tr>");

            foreach (DataRow r in dt.Rows)
            {
                sb.Append("<tr>");
                foreach (string col in columns)
                {
                    object val = r[col];
                    if (val is decimal || val is double || val is int || val is long)
                        sb.AppendFormat("<td style='padding:5px 8px;text-align:right;'>{0:N0}</td>", val);
                    else
                        sb.AppendFormat("<td style='padding:5px 8px;mso-number-format:\"\\@\";'>{0}</td>", HttpUtility.HtmlEncode(val?.ToString() ?? ""));
                }
                sb.Append("</tr>");
            }
            sb.Append("</table></body></html>");

            Response.Output.Write(sb.ToString());
            Response.Flush();
            Response.SuppressContent = true;
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}