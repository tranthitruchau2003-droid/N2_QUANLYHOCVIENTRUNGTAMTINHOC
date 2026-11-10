using N2_QUANLYHOCVIENTRUNGTAMTINHOC.DAL;
using System;
using System.Configuration;
using System.Data.SqlClient;

namespace N2_QUANLYHOCVIENTRUNGTAMTINHOC
{
    public partial class MasterPage : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["TenDangNhap"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            string quyen = Session["Quyen"] != null ? Session["Quyen"].ToString().Trim() : "Admin";
            PhanQuyenMenu(quyen);

            if (!IsPostBack)
            {
                litTenTrungTam.Text = ConfigurationManager.AppSettings["TenTrungTam"];
                litNguoiDung.Text = string.Format("{0} ({1})", Session["HoTen"], quyen);
            }
        }

        private void PhanQuyenMenu(string quyen)
        {
            bool isAdmin = quyen.Equals("Admin", StringComparison.OrdinalIgnoreCase);
            bool isGV = quyen.Equals("GiangVien", StringComparison.OrdinalIgnoreCase) || quyen.Equals("Giảng viên", StringComparison.OrdinalIgnoreCase);
            bool isHV = quyen.Equals("HocVien", StringComparison.OrdinalIgnoreCase) || quyen.Equals("Học viên", StringComparison.OrdinalIgnoreCase) || quyen.Equals("SinhVien", StringComparison.OrdinalIgnoreCase);

            if (isAdmin)
            {
                // Admin thấy toàn bộ chức năng
                liCaiDat.Visible = true;
            }
            else if (isGV)
            {
                // Giảng viên: Chỉ xem Dashboard, Lớp học & Lịch học, Kỳ thi & Điểm thi
                liHocVien.Visible = false;
                liKhoaHoc.Visible = false;
                liGiangVien.Visible = false;
                liDangKy.Visible = false;
                liPhieuThu.Visible = false;
                liChungChi.Visible = false;
                liBaoCao.Visible = false;
                liCaiDat.Visible = false;
            }
            else if (isHV)
            {
                // Học viên: Xem Dashboard, Khóa học, Lớp & Lịch học, Đăng ký, Kỳ thi & Điểm, Chứng chỉ
                liHocVien.Visible = false;
                liGiangVien.Visible = false;
                liPhieuThu.Visible = false;
                liBaoCao.Visible = false;
                liCaiDat.Visible = false;
            }
            else
            {
                // Nhân viên giáo vụ: Thấy tất cả trừ Cài đặt hệ thống
                liCaiDat.Visible = false;
            }
        }

        protected void btnDangXuat_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Login.aspx");
        }

        protected void btnLuuMatKhau_Click(object sender, EventArgs e)
        {
            string user = Session["TenDangNhap"]?.ToString();
            string quyen = Session["Quyen"]?.ToString() ?? "";
            string mkCu = txtMKCu.Text.Trim();
            string mkMoi = txtMKMoi.Text.Trim();

            if (string.IsNullOrEmpty(mkMoi) || mkMoi != txtMKXacNhan.Text.Trim())
            {
                Response.Write("<script>alert('Mật khẩu mới và xác nhận mật khẩu không khớp!');</script>");
                return;
            }

            int kq = 0;
            if (quyen.Equals("GiangVien", StringComparison.OrdinalIgnoreCase))
            {
                kq = kncsdl.ThucThiLenh("UPDATE GIANGVIEN SET matkhau=@mkm WHERE magiangvien=@u AND (ISNULL(matkhau, dienthoai)=@mkc)",
                    new[] { new SqlParameter("@mkm", mkMoi), new SqlParameter("@u", user), new SqlParameter("@mkc", mkCu) });
            }
            else if (quyen.Equals("HocVien", StringComparison.OrdinalIgnoreCase))
            {
                kq = kncsdl.ThucThiLenh("UPDATE HOCVIEN SET matkhau=@mkm WHERE mahocvien=@u AND (ISNULL(matkhau, dienthoai)=@mkc)",
                    new[] { new SqlParameter("@mkm", mkMoi), new SqlParameter("@u", user), new SqlParameter("@mkc", mkCu) });
            }
            else
            {
                kq = kncsdl.ThucThiLenh("UPDATE NGUOIDUNG SET matkhau=@mkm WHERE tendangnhap=@u AND matkhau=@mkc",
                    new[] { new SqlParameter("@mkm", mkMoi), new SqlParameter("@u", user), new SqlParameter("@mkc", mkCu) });
            }

            if (kq > 0)
                Response.Write("<script>alert('Đổi mật khẩu thành công!');</script>");
            else
                Response.Write("<script>alert('Mật khẩu hiện tại không đúng!');</script>");
        }
    }
}