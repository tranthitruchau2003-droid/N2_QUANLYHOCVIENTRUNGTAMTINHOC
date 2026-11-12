using System;
using System.Collections.Generic;
using System.Data;
using N2_QUANLYHOCVIENTRUNGTAMTINHOC.DAL;

namespace N2_QUANLYHOCVIENTRUNGTAMTINHOC
{
    public partial class Dashboard : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string quyen = Session["Quyen"]?.ToString() ?? "";
            if (quyen.Equals("GiangVien", StringComparison.OrdinalIgnoreCase) ||
                quyen.Equals("Giảng viên", StringComparison.OrdinalIgnoreCase) ||
                quyen.Equals("HocVien", StringComparison.OrdinalIgnoreCase) ||
                quyen.Equals("Học viên", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Views/LopHoc/QuanLyLopHoc.aspx");
                return;
            }

            if (!IsPostBack) TaiDuLieuDashboard();
        }

        private void TaiDuLieuDashboard()
        {
            litTongHV.Text = Convert.ToString(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM HOCVIEN") ?? 0);
            litTongKH.Text = Convert.ToString(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM KHOAHOC") ?? 0);
            litTongLop.Text = Convert.ToString(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM LOPHOC") ?? 0);
            litTongGV.Text = Convert.ToString(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM GIANGVIEN") ?? 0);

            decimal doanhThu = Convert.ToDecimal(kncsdl.LayGiaTriDon("SELECT ISNULL(SUM(sotienthu),0) FROM PHIEUTHU") ?? 0);
            litDoanhThu.Text = doanhThu.ToString("N0") + " đ";

            string sqlNo = @"SELECT ISNULL(SUM(kh.hocphi - dk.sotiendong), 0)
                             FROM DANGKY dk JOIN KHOAHOC kh ON dk.makhoahoc = kh.makhoahoc
                             WHERE kh.hocphi > dk.sotiendong";
            decimal congNo = Convert.ToDecimal(kncsdl.LayGiaTriDon(sqlNo) ?? 0);
            litCongNo.Text = congNo.ToString("N0") + " đ";

            // Bảng lớp sắp khai giảng
            gvLopMoi.DataSource = kncsdl.DocDuLieu(@"SELECT TOP 5 l.malop, l.tenlop, l.ngaykhaigiang, g.hoten 
                                                     FROM LOPHOC l JOIN GIANGVIEN g ON l.magiangvien = g.magiangvien 
                                                     ORDER BY l.ngaykhaigiang DESC");
            gvLopMoi.DataBind();

            // Bảng đăng ký gần đây
            gvDangKyMoi.DataSource = kncsdl.DocDuLieu(@"SELECT TOP 5 dk.madangky, hv.hoten, kh.tenkhoahoc, dk.sotiendong 
                                                        FROM DANGKY dk 
                                                        JOIN HOCVIEN hv ON dk.mahocvien = hv.mahocvien
                                                        JOIN KHOAHOC kh ON dk.makhoahoc = kh.makhoahoc
                                                        ORDER BY dk.ngaydangky DESC");
            gvDangKyMoi.DataBind();

            // Biểu đồ doanh thu theo tháng
            DataTable dtDT = kncsdl.DocDuLieu(@"SELECT FORMAT(ngaythu, 'MM/yyyy') AS Thang, SUM(sotienthu) AS Tong 
                                                FROM PHIEUTHU GROUP BY FORMAT(ngaythu, 'MM/yyyy') ORDER BY MIN(ngaythu)");
            List<string> lThang = new List<string>();
            List<string> lTien = new List<string>();
            foreach (DataRow r in dtDT.Rows)
            {
                lThang.Add(r["Thang"].ToString());
                lTien.Add(r["Tong"].ToString());
            }
            hfThangDoanhThu.Value = string.Join("|", lThang);
            hfGiaTriDoanhThu.Value = string.Join("|", lTien);

            // Biểu đồ học viên theo khóa học
            DataTable dtKH = kncsdl.DocDuLieu(@"SELECT kh.tenkhoahoc, COUNT(dk.madangky) AS SL 
                                                FROM KHOAHOC kh LEFT JOIN DANGKY dk ON kh.makhoahoc = dk.makhoahoc 
                                                GROUP BY kh.tenkhoahoc");
            List<string> lTenKH = new List<string>();
            List<string> lSL = new List<string>();
            foreach (DataRow r in dtKH.Rows)
            {
                lTenKH.Add(r["tenkhoahoc"].ToString());
                lSL.Add(r["SL"].ToString());
            }
            hfTenKhoaHoc.Value = string.Join("|", lTenKH);
            hfSoLuongHV.Value = string.Join("|", lSL);
        }
    }
}