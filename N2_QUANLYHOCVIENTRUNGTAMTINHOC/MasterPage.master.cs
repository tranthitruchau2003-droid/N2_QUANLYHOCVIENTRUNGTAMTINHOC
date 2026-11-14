using N2_QUANLYHOCVIENTRUNGTAMTINHOC.DAL;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

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
                Response.Redirect("~/Login.aspx", true);
                return;
            }

            string quyen = ChuanHoaQuyen(Session["Quyen"]);
            if (string.IsNullOrEmpty(quyen))
            {
                Session.Clear();
                Response.Redirect("~/Login.aspx", true);
                return;
            }

            Session["Quyen"] = quyen;

            if (Session["BatBuocDoiMK"] != null && (bool)Session["BatBuocDoiMK"])
            {
                ContentPlaceHolder mainContent = (ContentPlaceHolder)FindControl("MainContent");
                if (mainContent != null)
                {
                    mainContent.Controls.Clear();
                }
            }
            else
            {
                PhanQuyenMenu(quyen);
            }
        }

        private void PhanQuyenMenu(string quyen)
        {
            // Thiết lập hiển thị menu minh bạch và chính xác theo từng phân quyền
            bool isDashboard = (quyen == "Admin" || quyen == "NhanVien");
            bool isHocVienMenu = (quyen == "Admin" || quyen == "NhanVien");
            bool isKhoaHocMenu = (quyen == "Admin" || quyen == "NhanVien");
            bool isGiangVienMenu = (quyen == "Admin" || quyen == "NhanVien");
            bool isLopHocMenu = (quyen == "Admin" || quyen == "NhanVien" || quyen == "GiangVien" || quyen == "HocVien");
            bool isDangKyMenu = (quyen == "Admin" || quyen == "NhanVien" || quyen == "HocVien");
            bool isPhieuThuMenu = (quyen == "Admin" || quyen == "NhanVien");
            bool isKyThiMenu = (quyen == "Admin" || quyen == "NhanVien" || quyen == "GiangVien" || quyen == "HocVien");
            bool isChungChiMenu = (quyen == "Admin" || quyen == "NhanVien" || quyen == "HocVien");
            bool isBaoCaoMenu = (quyen == "Admin" || quyen == "NhanVien");
            bool isCaiDatMenu = (quyen == "Admin");

            if (liDashboard != null) liDashboard.Visible = isDashboard;
            if (liHocVien != null) liHocVien.Visible = isHocVienMenu;
            if (liKhoaHoc != null) liKhoaHoc.Visible = isKhoaHocMenu;
            if (liGiangVien != null) liGiangVien.Visible = isGiangVienMenu;
            if (liLopHoc != null) liLopHoc.Visible = isLopHocMenu;
            if (liDangKy != null) liDangKy.Visible = isDangKyMenu;
            if (liPhieuThu != null) liPhieuThu.Visible = isPhieuThuMenu;
            if (liKyThi != null) liKyThi.Visible = isKyThiMenu;
            if (liChungChi != null) liChungChi.Visible = isChungChiMenu;
            if (liBaoCao != null) liBaoCao.Visible = isBaoCaoMenu;
            if (liCaiDat != null) liCaiDat.Visible = isCaiDatMenu;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["TenDangNhap"] == null) return;

            string quyen = ChuanHoaQuyen(Session["Quyen"]);

            if (!IsPostBack)
            {
                pnlLoiDoiMK.CssClass = "alert alert-danger py-2 small fw-semibold mb-2 d-none";
                litTenTrungTam.Text = HttpUtility.HtmlEncode(ConfigurationManager.AppSettings["TenTrungTam"] ?? "TRUNG TÂM TIN HỌC NGÔI SAO VIỆT");
                string hoTenAnToan = HttpUtility.HtmlEncode(Session["HoTen"]?.ToString() ?? "");
                string quyenAnToan = HttpUtility.HtmlEncode(quyen);
                litNguoiDung.Text = string.Format("{0} ({1})", hoTenAnToan, quyenAnToan);

                if (Session["TB_ThanhCong"] != null)
                {
                    string msg = Session["TB_ThanhCong"].ToString();
                    Session.Remove("TB_ThanhCong");
                    ThongBaoPopup(msg, false);
                }
            }

            // Không đăng ký lại script mở modal trong postback AJAX (modal đang mở sẵn)
            if (!LaAsyncPostBack() && Session["BatBuocDoiMK"] != null && (bool)Session["BatBuocDoiMK"])
            {
                ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), "force_change_pw",
                    "moModalDoiMKBatBuoc();", true);
            }
        }

        private bool LaAsyncPostBack()
        {
            ScriptManager sm = ScriptManager.GetCurrent(this.Page);
            return sm != null && sm.IsInAsyncPostBack;
        }

        /// <summary>Chuyển trang an toàn cho cả postback thường lẫn postback AJAX.</summary>
        private void ChuyenTrang(string url)
        {
            if (LaAsyncPostBack())
            {
                ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), "mk_redirect",
                    $"window.location.href = '{HttpUtility.JavaScriptStringEncode(url)}';", true);
            }
            else
            {
                Response.Redirect(url, false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        protected void btnDangXuat_Click(object sender, EventArgs e)
        {
            string user = Session["TenDangNhap"]?.ToString();
            if (!string.IsNullOrEmpty(user))
                kncsdl.GhiNhatKy(user, "Đăng xuất", "Người dùng đăng xuất khỏi hệ thống");

            Session.Clear();
            Session.Abandon();
            if (Request.Cookies["ASP.NET_SessionId"] != null)
            {
                HttpCookie myCookie = new HttpCookie("ASP.NET_SessionId", "");
                myCookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(myCookie);
            }
            Response.Redirect("~/Login.aspx", true);
        }

        /// <summary>
        /// Lấy mật khẩu hiện tại đang lưu trong CSDL (và SĐT mặc định với GV/HV).
        /// Trả về false nếu không tìm thấy tài khoản.
        /// </summary>
        private bool LayMatKhauHienTai(string quyen, string user, out string mkDangLuu, out string sdtMacDinh)
        {
            mkDangLuu = "";
            sdtMacDinh = "";
            DataTable dt;

            if (quyen == "GiangVien")
            {
                dt = kncsdl.DocDuLieu(
                    "SELECT ISNULL(NULLIF(matkhau, ''), dienthoai) AS mk, dienthoai FROM GIANGVIEN WHERE magiangvien=@u",
                    new[] { new SqlParameter("@u", user) });
            }
            else if (quyen == "HocVien")
            {
                dt = kncsdl.DocDuLieu(
                    "SELECT ISNULL(NULLIF(matkhau, ''), dienthoai) AS mk, dienthoai FROM HOCVIEN WHERE mahocvien=@u",
                    new[] { new SqlParameter("@u", user) });
            }
            else
            {
                dt = kncsdl.DocDuLieu(
                    "SELECT matkhau AS mk FROM TAIKHOAN WHERE tendangnhap=@u AND trangthai=1",
                    new[] { new SqlParameter("@u", user) });
            }

            if (dt == null || dt.Rows.Count == 0) return false;

            mkDangLuu = Convert.ToString(dt.Rows[0]["mk"])?.Trim() ?? "";
            if (dt.Columns.Contains("dienthoai"))
                sdtMacDinh = Convert.ToString(dt.Rows[0]["dienthoai"])?.Trim() ?? "";
            return true;
        }

        protected void btnLuuMatKhau_Click(object sender, EventArgs e)
        {
            string user = Session["TenDangNhap"]?.ToString();
            string quyen = ChuanHoaQuyen(Session["Quyen"]);
            string mkCu = txtMKCu.Text.Trim();
            string mkMoi = txtMKMoi.Text.Trim();
            string mkXN = txtMKXacNhan.Text.Trim();

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(quyen))
            {
                ChuyenTrang(ResolveUrl("~/Login.aspx"));
                return;
            }

            // ----- 1. Kiểm tra dữ liệu nhập -----
            if (string.IsNullOrEmpty(mkCu)) { ThongBaoPopup("Vui lòng nhập Mật khẩu hiện tại!", true, "txtMKCu"); return; }
            if (string.IsNullOrEmpty(mkMoi)) { ThongBaoPopup("Vui lòng nhập Mật khẩu mới!", true, "txtMKMoi"); return; }
            if (mkMoi.Contains(" ")) { ThongBaoPopup("Mật khẩu mới không được chứa khoảng trắng!", true, "txtMKMoi"); return; }
            if (mkMoi.Length < 6 || mkMoi.Length > 50) { ThongBaoPopup("Mật khẩu mới phải từ 6 đến 50 ký tự!", true, "txtMKMoi"); return; }
            if (string.Equals(mkMoi, mkCu, StringComparison.Ordinal)) { ThongBaoPopup("Mật khẩu mới không được trùng với mật khẩu hiện tại!", true, "txtMKMoi"); return; }
            if (string.IsNullOrEmpty(mkXN)) { ThongBaoPopup("Vui lòng nhập Xác nhận mật khẩu mới!", true, "txtMKXacNhan"); return; }
            if (!string.Equals(mkMoi, mkXN, StringComparison.Ordinal)) { ThongBaoPopup("Xác nhận mật khẩu mới không khớp!", true, "txtMKXacNhan"); return; }

            try
            {
                // ----- 2. KIỂM TRA MẬT KHẨU CŨ với CSDL -----
                string mkDangLuu, sdtMacDinh;
                if (!LayMatKhauHienTai(quyen, user, out mkDangLuu, out sdtMacDinh))
                {
                    ThongBaoPopup("Không tìm thấy tài khoản hoặc tài khoản đã bị khóa!", true);
                    return;
                }

                if (!string.Equals(mkDangLuu, mkCu, StringComparison.Ordinal))
                {
                    ThongBaoPopup("Mật khẩu hiện tại không chính xác!", true, "txtMKCu");
                    return;
                }

                // GV/HV: mật khẩu mới không được trùng SĐT mặc định
                if ((quyen == "GiangVien" || quyen == "HocVien")
                    && !string.IsNullOrEmpty(sdtMacDinh)
                    && string.Equals(mkMoi, sdtMacDinh, StringComparison.Ordinal))
                {
                    ThongBaoPopup("Mật khẩu mới không được trùng với Số điện thoại mặc định!", true, "txtMKMoi");
                    return;
                }

                // ----- 3. Cập nhật mật khẩu mới -----
                int kq;
                if (quyen == "GiangVien")
                {
                    kq = kncsdl.ThucThiLenh("UPDATE GIANGVIEN SET matkhau=@mkm WHERE magiangvien=@u",
                        new[] { new SqlParameter("@mkm", mkMoi), new SqlParameter("@u", user) });
                }
                else if (quyen == "HocVien")
                {
                    kq = kncsdl.ThucThiLenh("UPDATE HOCVIEN SET matkhau=@mkm WHERE mahocvien=@u",
                        new[] { new SqlParameter("@mkm", mkMoi), new SqlParameter("@u", user) });
                }
                else
                {
                    kq = kncsdl.ThucThiLenh("UPDATE TAIKHOAN SET matkhau=@mkm WHERE tendangnhap=@u",
                        new[] { new SqlParameter("@mkm", mkMoi), new SqlParameter("@u", user) });
                }

                if (kq > 0)
                {
                    pnlLoiDoiMK.CssClass = "alert alert-danger py-2 small fw-semibold mb-2 d-none";
                    Session.Remove("BatBuocDoiMK");
                    kncsdl.GhiNhatKy(user, "Đổi mật khẩu", "Người dùng cập nhật mật khẩu mới");
                    Session["TB_ThanhCong"] = "Đổi mật khẩu thành công! Bạn đã có thể sử dụng hệ thống.";

                    // Thành công: mới đóng pop-up bằng cách tải lại trang (thông báo thành công hiện sau khi tải lại)
                    ChuyenTrang(Request.RawUrl);
                    return;
                }
                else
                {
                    ThongBaoPopup("Không thể cập nhật mật khẩu lúc này. Vui lòng thử lại!", true);
                }
            }
            catch (Exception)
            {
                ThongBaoPopup("Có lỗi xảy ra khi cập nhật mật khẩu. Vui lòng thử lại!", true);
            }
        }

        /// <summary>
        /// laLoiTrongModalDoiMK = true : hiện lỗi NGAY TRONG pop-up Đổi mật khẩu (pop-up giữ nguyên, không đóng/mở lại).
        /// laLoiTrongModalDoiMK = false: hiện thông báo chung (vd: đổi mật khẩu thành công sau khi tải lại trang).
        /// </summary>
        private void ThongBaoPopup(string msg, bool laLoiTrongModalDoiMK = false, string focusId = null)
        {
            if (laLoiTrongModalDoiMK)
            {
                pnlLoiDoiMK.CssClass = "alert alert-danger py-2 small fw-semibold mb-2";
                litLoiDoiMK.Text = HttpUtility.HtmlEncode(msg);

                if (LaAsyncPostBack())
                {
                    // Chỉ vùng báo lỗi được cập nhật; đưa con trỏ vào ô cần sửa
                    if (!string.IsNullOrEmpty(focusId))
                    {
                        string fid = HttpUtility.JavaScriptStringEncode(focusId);
                        ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), "mk_focus_" + Guid.NewGuid().ToString("N"),
                            $"(function(){{ var e = document.getElementById('{fid}'); if (e) {{ e.focus(); if (e.select) e.select(); }} }})();", true);
                    }
                    return;
                }

                // Dự phòng khi không phải postback AJAX: mở lại modal kèm thông báo lỗi
                string boolBatBuocLoi = (Session["BatBuocDoiMK"] != null && (bool)Session["BatBuocDoiMK"]) ? "true" : "false";
                ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), "mk_msg_" + Guid.NewGuid().ToString("N"),
                    $"hienThiThongBaoMaster('{HttpUtility.JavaScriptStringEncode(msg)}', true, {boolBatBuocLoi});", true);
                return;
            }

            pnlLoiDoiMK.CssClass = "alert alert-danger py-2 small fw-semibold mb-2 d-none";
            string safeMsg = HttpUtility.JavaScriptStringEncode(msg);
            string boolBatBuoc = (Session["BatBuocDoiMK"] != null && (bool)Session["BatBuocDoiMK"]) ? "true" : "false";
            string script = $"hienThiThongBaoMaster('{safeMsg}', false, {boolBatBuoc});";
            ScriptManager.RegisterStartupScript(this.Page, this.Page.GetType(), "mk_msg_" + Guid.NewGuid().ToString("N"), script, true);
        }
    }
}