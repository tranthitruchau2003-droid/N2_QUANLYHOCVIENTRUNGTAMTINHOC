using System;
using System.Data.SqlClient;
using N2_QUANLYHOCVIENTRUNGTAMTINHOC.DAL;

namespace N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.DangKy
{
    public partial class QuanLyDangKy : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                txtNgayDK.Text = DateTime.Now.ToString("yyyy-MM-dd");
                ddlHocVien.DataSource = kncsdl.DocDuLieu("SELECT mahocvien, hoten + ' (' + mahocvien + ')' AS hienthi FROM HOCVIEN");
                ddlHocVien.DataTextField = "hienthi"; ddlHocVien.DataValueField = "mahocvien"; ddlHocVien.DataBind();

                ddlKhoaHoc.DataSource = kncsdl.DocDuLieu("SELECT makhoahoc, tenkhoahoc + ' - ' + FORMAT(hocphi,'N0') + 'đ' AS hienthi FROM KHOAHOC WHERE trangthaikhoahoc=N'Đang mở'");
                ddlKhoaHoc.DataTextField = "hienthi"; ddlKhoaHoc.DataValueField = "makhoahoc"; ddlKhoaHoc.DataBind();

                TaiDanhSach();
            }
        }

        private void TaiDanhSach()
        {
            string sql = @"SELECT dk.*, hv.hoten, kh.tenkhoahoc, kh.hocphi 
                           FROM DANGKY dk JOIN HOCVIEN hv ON dk.mahocvien=hv.mahocvien 
                           JOIN KHOAHOC kh ON dk.makhoahoc=kh.makhoahoc ORDER BY dk.ngaydangky DESC";
            gvDangKy.DataSource = kncsdl.DocDuLieu(sql);
            gvDangKy.DataBind();
        }

        protected void btnDangKy_Click(object sender, EventArgs e)
        {
            // Kiểm tra trùng lặp đăng ký
            int trung = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM DANGKY WHERE mahocvien=@hv AND makhoahoc=@kh",
                new[] { new SqlParameter("@hv", ddlHocVien.SelectedValue), new SqlParameter("@kh", ddlKhoaHoc.SelectedValue) }));
            if (trung > 0)
            {
                pnlTB.Visible = true; litTB.Text = "Học viên này đã đăng ký khóa học trên rồi!";
                return;
            }

            string sql = "INSERT INTO DANGKY(madangky, ngaydangky, sotiendong, trangthaithanhtoan, mahocvien, makhoahoc) VALUES(@ma, @ngay, 0, N'Chưa thanh toán', @hv, @kh)";
            SqlParameter[] p = {
                new SqlParameter("@ma", txtMaDK.Text.Trim()),
                new SqlParameter("@ngay", txtNgayDK.Text),
                new SqlParameter("@hv", ddlHocVien.SelectedValue),
                new SqlParameter("@kh", ddlKhoaHoc.SelectedValue)
            };
            kncsdl.ThucThiLenh(sql, p);
            kncsdl.GhiNhatKy(Session["TenDangNhap"]?.ToString(), "Đăng ký khóa học", "Mã ĐK: " + txtMaDK.Text.Trim());
            pnlTB.Visible = true; litTB.Text = "Đăng ký khóa học thành công! Chuyển sang mục Phiếu thu để thu học phí.";
            TaiDanhSach();
        }
    }
}