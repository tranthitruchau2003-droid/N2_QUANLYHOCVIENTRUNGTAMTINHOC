using N2_QUANLYHOCVIENTRUNGTAMTINHOC.DAL;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;

namespace N2_QUANLYHOCVIENTRUNGTAMTINHOC
{
    public partial class MasterPage : System.Web.UI.MasterPage
    {
        public static string ChuanHoaQuyen(object rawQuyen)
        {
            string q = rawQuyen?.ToString().Trim() ?? "";
            if (q.Equals("Admin", StringComparison.OrdinalIgnoreCase) || q.Equals("Quản trị viên", StringComparison.OrdinalIgnoreCase))
                return "Admin";
            if (q.Equals("NhanVien", StringComparison.OrdinalIgnoreCase) || q.Equals("Nhân viên", StringComparison.OrdinalIgnoreCase) || q.Equals("GiaoVu", StringComparison.OrdinalIgnoreCase))
                return "NhanVien";
            if (q.Equals("GiangVien", StringComparison.OrdinalIgnoreCase) || q.Equals("Giảng viên", StringComparison.OrdinalIgnoreCase))
                return "GiangVien";
            if (q.Equals("HocVien", StringComparison.OrdinalIgnoreCase) || q.Equals("Học viên", StringComparison.OrdinalIgnoreCase) || q.Equals("SinhVien", StringComparison.OrdinalIgnoreCase))
                return "HocVien";
            return "";
        }


        protected void Page_Init(object sender, EventArgs e)
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();

            if (Session["TenDangNhap"] == null)
            {
                Response.Redirect("~/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            string quyen = ChuanHoaQuyen(Session["Quyen"]);
            if (string.IsNullOrEmpty(quyen))
            {
                Session.Clear();
                Response.Redirect("~/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            Session["Quyen"] = quyen;
            KiemTraQuyenTruyCapTrang(quyen);
        }

        private void KiemTraQuyenTruyCapTrang(string quyen)
        {
            string path = Request.AppRelativeCurrentExecutionFilePath?.ToLowerInvariant() ?? "";

            if (quyen == "Admin") return;

            if (quyen == "NhanVien")
            {
                if (path.Contains("/caidathethong.aspx"))
                {
                    Response.Redirect("~/Dashboard.aspx", false);
                    Context.ApplicationInstance.CompleteRequest();
                }
            }
            else if (quyen == "GiangVien")
            {
                bool choPhepGV = path.Contains("/quanlylophoc.aspx") || path.Contains("/quanlykythi.aspx");
                if (!choPhepGV)
                {
                    Response.Redirect("~/Views/LopHoc/QuanLyLopHoc.aspx", false);
                    Context.ApplicationInstance.CompleteRequest();
                }
            }
            else if (quyen == "HocVien")
            {
                bool choPhepHV = path.Contains("/quanlylophoc.aspx") ||
                                 path.Contains("/quanlydangky.aspx") ||
                                 path.Contains("/quanlykythi.aspx") ||
                                 path.Contains("/quanlychungchi.aspx");
                if (!choPhepHV)
                {
                    Response.Redirect("~/Views/DangKy/QuanLyDangKy.aspx", false);
                    Context.ApplicationInstance.CompleteRequest();
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["TenDangNhap"] == null) return;

            string quyen = ChuanHoaQuyen(Session["Quyen"]);
            PhanQuyenMenu(quyen);

            if (!IsPostBack)
            {
                litTenTrungTam.Text = HttpUtility.HtmlEncode(ConfigurationManager.AppSettings["TenTrungTam"] ?? "TRUNG TÂM TIN HỌC NGÔI SAO VIỆT");
                string hoTenAnToan = HttpUtility.HtmlEncode(Session["HoTen"]?.ToString() ?? "");
                string quyenAnToan = HttpUtility.HtmlEncode(quyen);
                litNguoiDung.Text = string.Format("{0} ({1})", hoTenAnToan, quyenAnToan);
            }

            if (Session["BatBuocDoiMK"] != null && (bool)Session["BatBuocDoiMK"])
            {
                ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), "force_change_pw",
                    "document.addEventListener('DOMContentLoaded', function () { moModalDoiMKBatBuoc(); });", true);
            }
        }

        private void PhanQuyenMenu(string quyen)
        {
            if (quyen == "Admin")
            {
                liCaiDat.Visible = true;
            }
            else if (quyen == "GiangVien")
            {
                liDashboard.Visible = false;
                liHocVien.Visible = false;
                liKhoaHoc.Visible = false;
                liGiangVien.Visible = false;
                liDangKy.Visible = false;
                liPhieuThu.Visible = false;
                liChungChi.Visible = false;
                liBaoCao.Visible = false;
                liCaiDat.Visible = false;

                liLopHoc.Visible = true;
                liKyThi.Visible = true;
            }
            else if (quyen == "HocVien")
            {
                liDashboard.Visible = false;
                liHocVien.Visible = false;
                liKhoaHoc.Visible = false;
                liGiangVien.Visible = false;
                liPhieuThu.Visible = false;
                liBaoCao.Visible = false;
                liCaiDat.Visible = false;

                liLopHoc.Visible = true;
                liDangKy.Visible = true;
                liKyThi.Visible = true;
                liChungChi.Visible = true;
            }
            else if (quyen == "NhanVien")
            {
                liCaiDat.Visible = false;
            }
            else
            {

                liDashboard.Visible = liHocVien.Visible = liKhoaHoc.Visible = liGiangVien.Visible =
                liLopHoc.Visible = liDangKy.Visible = liPhieuThu.Visible = liKyThi.Visible =
                liChungChi.Visible = liBaoCao.Visible = liCaiDat.Visible = false;
            }
        }

        protected void btnDangXuat_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            if (Request.Cookies["ASP.NET_SessionId"] != null)
            {
                HttpCookie myCookie = new HttpCookie("ASP.NET_SessionId", "");
                myCookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(myCookie);
            }
            Response.Redirect("~/Login.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }


        protected void btnLuuMatKhau_Click(object sender, EventArgs e)
        {
            string user = Session["TenDangNhap"]?.ToString();
            string quyen = ChuanHoaQuyen(Session["Quyen"]);
            string mkCu = txtMKCu.Text.Trim();
            string mkMoi = txtMKMoi.Text.Trim();
            string mkXN = txtMKXacNhan.Text.Trim();

            if (string.IsNullOrEmpty(user))
            {
                Response.Redirect("~/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (string.IsNullOrEmpty(mkCu) || string.IsNullOrEmpty(mkMoi))
            {
                ThongBaoPopup("Vui lòng nhập đầy đủ mật khẩu hiện tại và mật khẩu mới!");
                return;
            }

            if (mkMoi.Length < 6)
            {
                ThongBaoPopup("Mật khẩu mới phải có ít nhất 6 ký tự để đảm bảo an toàn!");
                return;
            }

            if (mkMoi != mkXN)
            {
                ThongBaoPopup("Mật khẩu mới và xác nhận mật khẩu không khớp!");
                return;
            }

            if (mkMoi == mkCu)
            {
                ThongBaoPopup("Mật khẩu mới phải khác mật khẩu hiện tại!");
                return;
            }

            try
            {
                int kq = 0;
                if (quyen == "GiangVien")
                {
                    kq = kncsdl.ThucThiLenh("UPDATE GIANGVIEN SET matkhau=@mkm WHERE magiangvien=@u AND ISNULL(NULLIF(matkhau, ''), dienthoai)=@mkc",
                        new[] { new SqlParameter("@mkm", mkMoi), new SqlParameter("@u", user), new SqlParameter("@mkc", mkCu) });
                }
                else if (quyen == "HocVien")
                {
                    kq = kncsdl.ThucThiLenh("UPDATE HOCVIEN SET matkhau=@mkm WHERE mahocvien=@u AND ISNULL(NULLIF(matkhau, ''), dienthoai)=@mkc",
                        new[] { new SqlParameter("@mkm", mkMoi), new SqlParameter("@u", user), new SqlParameter("@mkc", mkCu) });
                }
                else
                {

                    kq = kncsdl.ThucThiLenh("UPDATE TAIKHOAN SET matkhau=@mkm WHERE tendangnhap=@u AND matkhau=@mkc AND trangthai=1",
                        new[] { new SqlParameter("@mkm", mkMoi), new SqlParameter("@u", user), new SqlParameter("@mkc", mkCu) });
                }

                if (kq > 0)
                {
                    Session.Remove("BatBuocDoiMK");
                    string safeMsg = HttpUtility.JavaScriptStringEncode("Đổi mật khẩu thành công! Bạn đã có thể sử dụng hệ thống.");
                    ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), "mk_ok",
                        $"alert('{safeMsg}'); window.location.href = window.location.href;", true);
                }
                else
                {
                    ThongBaoPopup("Mật khẩu hiện tại không đúng!");
                }
            }
            catch
            {
                ThongBaoPopup("Có lỗi xảy ra khi cập nhật mật khẩu. Vui lòng thử lại!");
            }
        }

        private void ThongBaoPopup(string msg)
        {
            string safeMsg = HttpUtility.JavaScriptStringEncode(msg);
            ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), "mk_msg", $"alert('{safeMsg}');", true);
        }
    }
}