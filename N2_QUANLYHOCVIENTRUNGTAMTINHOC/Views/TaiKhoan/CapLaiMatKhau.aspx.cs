using System;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI.WebControls;
using N2_QUANLYHOCVIENTRUNGTAMTINHOC.DAL;

namespace N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.TaiKhoan
{
    public partial class CapLaiMatKhau : System.Web.UI.Page
    {
        private bool LaAdmin
        {
            get { return MasterPage.ChuanHoaQuyen(Session["Quyen"]) == "Admin"; }
        }

        private string TabHienTai
        {
            get { return ViewState["TabCapMK"]?.ToString() ?? "GV"; }
            set { ViewState["TabCapMK"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!LaAdmin)
            {
                Response.Redirect("~/Dashboard.aspx", true);
                return;
            }

            if (!IsPostBack)
            {
                TabHienTai = "GV";
                CapNhatGiaoDienTab();
                TaiDuLieuTabHienTai();
            }
        }

        private void CapNhatGiaoDienTab()
        {
            bool laGV = (TabHienTai == "GV");
            pnlGiangVien.Visible = laGV;
            pnlHocVien.Visible = !laGV;
            btnTabGV.CssClass = laGV ? "nav-link active fw-bold text-primary" : "nav-link fw-semibold text-secondary";
            btnTabHV.CssClass = !laGV ? "nav-link active fw-bold text-primary" : "nav-link fw-semibold text-secondary";
        }

        private void TaiDuLieuTabHienTai()
        {
            string kw = txtTimKiem.Text.Trim();
            SqlParameter[] p = { new SqlParameter("@kw", string.IsNullOrEmpty(kw) ? "" : "%" + kw + "%") };

            if (TabHienTai == "GV")
            {
                string sqlGV = @"SELECT magiangvien, hoten, dienthoai, email, chuyenmon
                                 FROM GIANGVIEN
                                 WHERE (@kw = '' OR magiangvien COLLATE Latin1_General_CI_AI LIKE @kw
                                                 OR hoten COLLATE Latin1_General_CI_AI LIKE @kw
                                                 OR dienthoai COLLATE Latin1_General_CI_AI LIKE @kw
                                                 OR ISNULL(email, '') COLLATE Latin1_General_CI_AI LIKE @kw)
                                 ORDER BY magiangvien DESC";
                gvGiangVien.DataSource = kncsdl.DocDuLieu(sqlGV, p);
                gvGiangVien.DataBind();
            }
            else
            {
                string sqlHV = @"SELECT mahocvien, hoten, dienthoai, email, diachi
                                 FROM HOCVIEN
                                 WHERE (@kw = '' OR mahocvien COLLATE Latin1_General_CI_AI LIKE @kw
                                                 OR hoten COLLATE Latin1_General_CI_AI LIKE @kw
                                                 OR dienthoai COLLATE Latin1_General_CI_AI LIKE @kw
                                                 OR ISNULL(email, '') COLLATE Latin1_General_CI_AI LIKE @kw)
                                 ORDER BY mahocvien DESC";
                gvHocVien.DataSource = kncsdl.DocDuLieu(sqlHV, p);
                gvHocVien.DataBind();
            }
        }

        private void HienThongBao(string msg, bool laLoi = false)
        {
            pnlThongBao.Visible = true;
            pnlThongBao.CssClass = laLoi
                ? "alert alert-danger text-danger fw-semibold alert-dismissible fade show shadow"
                : "alert alert-success text-success fw-semibold alert-dismissible fade show shadow";
            litThongBao.Text = HttpUtility.HtmlEncode(msg);
        }

        protected void btnTabGV_Click(object sender, EventArgs e)
        {
            TabHienTai = "GV";
            gvGiangVien.PageIndex = 0;
            pnlThongBao.Visible = false;
            CapNhatGiaoDienTab();
            TaiDuLieuTabHienTai();
        }

        protected void btnTabHV_Click(object sender, EventArgs e)
        {
            TabHienTai = "HV";
            gvHocVien.PageIndex = 0;
            pnlThongBao.Visible = false;
            CapNhatGiaoDienTab();
            TaiDuLieuTabHienTai();
        }

        protected void btnTim_Click(object sender, EventArgs e)
        {
            gvGiangVien.PageIndex = 0;
            gvHocVien.PageIndex = 0;
            pnlThongBao.Visible = false;
            TaiDuLieuTabHienTai();
        }

        protected void btnLamMoi_Click(object sender, EventArgs e)
        {
            txtTimKiem.Text = "";
            gvGiangVien.PageIndex = 0;
            gvHocVien.PageIndex = 0;
            pnlThongBao.Visible = false;
            TaiDuLieuTabHienTai();
        }

        protected void gvGiangVien_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvGiangVien.PageIndex = e.NewPageIndex;
            pnlThongBao.Visible = false;
            TaiDuLieuTabHienTai();
        }

        protected void gvHocVien_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvHocVien.PageIndex = e.NewPageIndex;
            pnlThongBao.Visible = false;
            TaiDuLieuTabHienTai();
        }

        protected void btnLuuCapMK_Click(object sender, EventArgs e)
        {
            if (!LaAdmin) return;

            string loai = (hfLoaiTK.Value ?? "").Trim();
            string ma = (hfMaTK.Value ?? "").Trim();
            string mkMoi = txtMKMoi.Text.Trim();
            string mkXN = txtMKXacNhan.Text.Trim();

            if ((loai != "GV" && loai != "HV") || string.IsNullOrEmpty(ma))
            {
                HienThongBao("Không xác định được tài khoản cần cấp lại mật khẩu!", true);
                return;
            }
            if (string.IsNullOrEmpty(mkMoi) || mkMoi.Contains(" ") || mkMoi.Length < 6 || mkMoi.Length > 50)
            {
                HienThongBao("Mật khẩu mới phải từ 6 đến 50 ký tự và không chứa khoảng trắng!", true);
                return;
            }
            if (!string.Equals(mkMoi, mkXN, StringComparison.Ordinal))
            {
                HienThongBao("Xác nhận mật khẩu mới không khớp!", true);
                return;
            }

            try
            {
                string sql = loai == "GV"
                    ? "UPDATE GIANGVIEN SET matkhau=@mk WHERE magiangvien=@ma"
                    : "UPDATE HOCVIEN SET matkhau=@mk WHERE mahocvien=@ma";

                int kq = kncsdl.ThucThiLenh(sql, new[] {
                    new SqlParameter("@mk", MasterPage.BamMatKhau(mkMoi)),
                    new SqlParameter("@ma", ma)
                });

                if (kq > 0)
                {
                    string doiTuong = loai == "GV" ? "giảng viên" : "học viên";
                    kncsdl.GhiNhatKy(Session["TenDangNhap"]?.ToString(), $"Cấp lại mật khẩu {doiTuong}", $"Mã: {ma}");
                    HienThongBao($"Đã cấp lại mật khẩu mới cho {doiTuong} {ma} thành công!", false);
                }
                else
                {
                    HienThongBao("Không tìm thấy tài khoản để cập nhật mật khẩu!", true);
                }
            }
            catch
            {
                HienThongBao("Có lỗi xảy ra khi cấp lại mật khẩu. Vui lòng thử lại!", true);
            }
        }

        protected void btnDatVeSDT_Click(object sender, EventArgs e)
        {
            if (!LaAdmin) return;

            string loai = (hfLoaiTK.Value ?? "").Trim();
            string ma = (hfMaTK.Value ?? "").Trim();
            if ((loai != "GV" && loai != "HV") || string.IsNullOrEmpty(ma))
            {
                HienThongBao("Không xác định được tài khoản cần đặt lại mật khẩu!", true);
                return;
            }

            try
            {
                string sqlLaySDT = loai == "GV"
                    ? "SELECT dienthoai FROM GIANGVIEN WHERE magiangvien=@ma"
                    : "SELECT dienthoai FROM HOCVIEN WHERE mahocvien=@ma";
                string sdt = kncsdl.LayGiaTriDon(sqlLaySDT, new[] { new SqlParameter("@ma", ma) })?.ToString().Trim() ?? "";

                if (string.IsNullOrEmpty(sdt))
                {
                    HienThongBao("Tài khoản này chưa có Số điện thoại nên không thể đặt về SĐT mặc định!", true);
                    return;
                }

                // Đặt matkhau = NULL để đăng nhập bằng SĐT và tự động bắt buộc đổi mật khẩu mới theo luồng Login.aspx.cs
                string sqlUpdate = loai == "GV"
                    ? "UPDATE GIANGVIEN SET matkhau=NULL WHERE magiangvien=@ma"
                    : "UPDATE HOCVIEN SET matkhau=NULL WHERE mahocvien=@ma";
                int kq = kncsdl.ThucThiLenh(sqlUpdate, new[] { new SqlParameter("@ma", ma) });

                if (kq > 0)
                {
                    string doiTuong = loai == "GV" ? "giảng viên" : "học viên";
                    kncsdl.GhiNhatKy(Session["TenDangNhap"]?.ToString(), $"Đặt lại MK mặc định {doiTuong}", $"Mã: {ma}");
                    HienThongBao($"Đã đặt mật khẩu của {doiTuong} {ma} về SĐT mặc định ({sdt}). Người dùng sẽ được yêu cầu đổi MK khi đăng nhập!", false);
                }
            }
            catch
            {
                HienThongBao("Có lỗi khi đặt lại mật khẩu mặc định!", true);
            }
        }
    }
}