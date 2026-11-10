using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.Script.Serialization;
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
                TaiNam();
                TaiCongNo();
                TaiThongKeKhoaHoc();
                TaiBieuDoThangNam();
            }
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

        private void TaiCongNo(string tuKhoa = "")
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
            gvCongNo.DataSource = kncsdl.DocDuLieu(sql, p);
            gvCongNo.DataBind();

            TaiGoiYCongNo();
        }

        private void TaiGoiYCongNo()
        {
            string sql = @"SELECT dk.madangky, hv.hoten, hv.dienthoai
                           FROM DANGKY dk
                           JOIN HOCVIEN hv ON dk.mahocvien = hv.mahocvien
                           JOIN KHOAHOC kh ON dk.makhoahoc = kh.makhoahoc
                           LEFT JOIN PHIEUTHU pt ON dk.madangky = pt.madangky
                           GROUP BY dk.madangky, hv.hoten, hv.dienthoai, kh.hocphi
                           HAVING (kh.hocphi - ISNULL(SUM(pt.sotienthu), 0)) > 0";
            DataTable dt = kncsdl.DocDuLieu(sql);
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
            TaiCongNo(txtTimNo.Text.Trim());
            TaiThongKeKhoaHoc();
            TaiBieuDoThangNam();
        }

        protected void ddlNam_SelectedIndexChanged(object sender, EventArgs e)
        {
            TaiCongNo(txtTimNo.Text.Trim());
            TaiThongKeKhoaHoc();
            TaiBieuDoThangNam();
        }

        protected void btnXuatExcel_Click(object sender, EventArgs e)
        {
            DataTable dt = LayDuLieuThongKeKhoaHoc();
            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment;filename=ThongKe_KhoaHoc_" + DateTime.Now.ToString("yyyyMMdd") + ".xls");
            Response.Charset = "";
            Response.ContentType = "application/vnd.ms-excel";
            Response.ContentEncoding = Encoding.UTF8;
            Response.BinaryWrite(Encoding.UTF8.GetPreamble());

            StringBuilder sb = new StringBuilder();
            sb.Append("<table border='1'>");
            sb.Append("<tr style='background-color:#1677D2;color:white;font-weight:bold;'>");
            sb.Append("<th>Mã KH</th><th>Tên khóa học</th><th>Số lượng ĐK</th><th>Tổng thực thu (VNĐ)</th>");
            sb.Append("</tr>");
            foreach (DataRow r in dt.Rows)
            {
                sb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3:N0}</td></tr>",
                    r["makhoahoc"], r["tenkhoahoc"], r["soluongdk"], r["tongthucthu"]);
            }
            sb.Append("</table>");

            Response.Output.Write(sb.ToString());
            Response.Flush();
            Response.End();
        }
    }
}