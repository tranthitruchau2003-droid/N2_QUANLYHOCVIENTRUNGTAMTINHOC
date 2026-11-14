using System;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using System.Web.UI;
using N2_QUANLYHOCVIENTRUNGTAMTINHOC.DAL;

namespace N2_QUANLYHOCVIENTRUNGTAMTINHOC
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack && Session["TenDangNhap"] != null)
            {
                ChuyenHuongTheoQuyen(Session["Quyen"]?.ToString());
            }
        }

        private void ChuyenHuongTheoQuyen(string rawQuyen)
        {
            string q = MasterPage.ChuanHoaQuyen(rawQuyen);
            string url = "~/Dashboard.aspx";
            if (q == "GiangVien") url = "~/Views/LopHoc/QuanLyLopHoc.aspx";
            else if (q == "HocVien") url = "~/Views/DangKy/QuanLyDangKy.aspx";

            ScriptManager.RegisterStartupScript(this, GetType(), "redirect", $"window.location.href = '{ResolveUrl(url)}';", true);
        }

        protected void btnDangNhap_Click(object sender, EventArgs e)
        {
            string u = txtTaiKhoan.Text.Trim();
            string p = txtMatKhau.Text.Trim();
            if (string.IsNullOrEmpty(u) || string.IsNullOrEmpty(p))
            {
                HienLoi("Nhập đầy đủ Tên đăng nhập và Mật khẩu!");
                return;
            }

            string ip = Request.UserHostAddress ?? "unknown";
            string cacheKey = "LoginFail_" + u.ToLowerInvariant() + "_" + ip;
            int soLanSai = HttpRuntime.Cache[cacheKey] != null ? (int)HttpRuntime.Cache[cacheKey] : 0;
            if (soLanSai >= 5)
            {
                HienLoi("Tài khoản tạm khóa 5 phút do nhập sai mật khẩu quá 5 lần!");
                return;
            }

            try
            {
                // 1. Kiểm tra Admin / Nhân viên
                DataTable dtAdmin = kncsdl.DocDuLieu(
                    "SELECT * FROM TAIKHOAN WHERE tendangnhap=@u AND trangthai=1",
                    new[] { new SqlParameter("@u", u) });

                if (dtAdmin.Rows.Count > 0)
                {
                    string mkDB = dtAdmin.Rows[0]["matkhau"]?.ToString() ?? "";
                    if (string.Equals(mkDB, p, StringComparison.Ordinal))
                    {
                        HttpRuntime.Cache.Remove(cacheKey);
                        string quyen = MasterPage.ChuanHoaQuyen(dtAdmin.Rows[0]["quyen"]);
                        if (string.IsNullOrEmpty(quyen)) quyen = "NhanVien";

                        Session["TenDangNhap"] = dtAdmin.Rows[0]["tendangnhap"].ToString();
                        Session["HoTen"] = dtAdmin.Rows[0]["hoten"].ToString();
                        Session["Quyen"] = quyen;
                        Session.Remove("BatBuocDoiMK");
                        kncsdl.GhiNhatKy(Session["TenDangNhap"].ToString(), "Đăng nhập hệ thống", $"Đăng nhập quyền {quyen} từ IP {ip}");
                        ChuyenHuongTheoQuyen(quyen);
                        return;
                    }
                }

                // 2. Kiểm tra Giảng viên
                DataTable dtGV = kncsdl.DocDuLieu(
                    "SELECT * FROM GIANGVIEN WHERE LTRIM(RTRIM(dienthoai))=@u",
                    new[] { new SqlParameter("@u", u) });

                if (dtGV.Rows.Count > 0)
                {
                    string sdtGV = dtGV.Rows[0]["dienthoai"]?.ToString().Trim() ?? "";
                    string mkDB = dtGV.Rows[0]["matkhau"] != DBNull.Value ? dtGV.Rows[0]["matkhau"].ToString().Trim() : "";
                    string mkHieuLuc = string.IsNullOrEmpty(mkDB) ? sdtGV : mkDB;

                    if (string.Equals(mkHieuLuc, p, StringComparison.Ordinal))
                    {
                        HttpRuntime.Cache.Remove(cacheKey);
                        string maGV = dtGV.Rows[0]["magiangvien"].ToString();
                        bool canDoiMKLanDau = string.IsNullOrEmpty(mkDB) || string.Equals(mkDB, sdtGV, StringComparison.Ordinal);

                        Session["TenDangNhap"] = maGV;
                        Session["HoTen"] = dtGV.Rows[0]["hoten"].ToString();
                        Session["Quyen"] = "GiangVien";
                        if (canDoiMKLanDau) Session["BatBuocDoiMK"] = true;
                        else Session.Remove("BatBuocDoiMK");

                        kncsdl.GhiNhatKy(maGV, "Đăng nhập hệ thống", $"Giảng viên đăng nhập từ IP {ip}");
                        ChuyenHuongTheoQuyen("GiangVien");
                        return;
                    }
                }

                // 3. Kiểm tra Học viên
                DataTable dtHV = kncsdl.DocDuLieu(
                    "SELECT * FROM HOCVIEN WHERE mahocvien=@u OR LTRIM(RTRIM(dienthoai))=@u OR (@u <> '' AND LOWER(LTRIM(RTRIM(ISNULL(email,''))))=LOWER(@u))",
                    new[] { new SqlParameter("@u", u) });

                if (dtHV.Rows.Count > 0)
                {
                    string sdtHV = dtHV.Rows[0]["dienthoai"]?.ToString().Trim() ?? "";
                    string mkDB = dtHV.Rows[0]["matkhau"] != DBNull.Value ? dtHV.Rows[0]["matkhau"].ToString().Trim() : "";
                    string mkHieuLuc = string.IsNullOrEmpty(mkDB) ? sdtHV : mkDB;

                    if (!string.IsNullOrEmpty(mkHieuLuc) && string.Equals(mkHieuLuc, p, StringComparison.Ordinal))
                    {
                        HttpRuntime.Cache.Remove(cacheKey);
                        string maHV = dtHV.Rows[0]["mahocvien"].ToString();
                        bool canDoiMKLanDau = string.IsNullOrEmpty(mkDB) || (!string.IsNullOrEmpty(sdtHV) && string.Equals(mkDB, sdtHV, StringComparison.Ordinal));

                        Session["TenDangNhap"] = maHV;
                        Session["HoTen"] = dtHV.Rows[0]["hoten"].ToString();
                        Session["Quyen"] = "HocVien";
                        if (canDoiMKLanDau) Session["BatBuocDoiMK"] = true;
                        else Session.Remove("BatBuocDoiMK");

                        kncsdl.GhiNhatKy(maHV, "Đăng nhập hệ thống", $"Học viên đăng nhập từ IP {ip}");
                        ChuyenHuongTheoQuyen("HocVien");
                        return;
                    }
                }

                HttpRuntime.Cache.Insert(cacheKey, soLanSai + 1, null, DateTime.Now.AddMinutes(5), Cache.NoSlidingExpiration);
                HienLoi("Tên đăng nhập hoặc mật khẩu không chính xác!");
            }
            catch
            {
                HienLoi("Không thể kết nối cơ sở dữ liệu. Vui lòng thử lại sau!");
            }
        }

        protected void btnDangKyTK_Click(object sender, EventArgs e)
        {
            string hoTen = txtDKHoTen.Text.Trim();
            string sdt = txtDKDienThoai.Text.Trim();
            string email = txtDKEmail.Text.Trim();
            string matKhau = txtDKMatKhau.Text.Trim();
            string matKhauXN = txtDKMatKhauXN.Text.Trim();

            if (string.IsNullOrEmpty(hoTen) || hoTen.Length > 100)
            {
                HienLoiDKModal("Vui lòng nhập Họ và tên hợp lệ (tối đa 100 ký tự)!");
                return;
            }
            if (string.IsNullOrEmpty(sdt) || !Regex.IsMatch(sdt, @"^0\d{9}$"))
            {
                HienLoiDKModal("Số điện thoại không hợp lệ (phải gồm 10 chữ số và bắt đầu bằng số 0)!");
                return;
            }
            if (string.IsNullOrEmpty(matKhau) || matKhau.Contains(" ") || matKhau.Length < 6 || matKhau.Length > 50)
            {
                HienLoiDKModal("Mật khẩu phải từ 6 đến 50 ký tự và không chứa khoảng trắng!");
                return;
            }
            if (matKhau == sdt)
            {
                HienLoiDKModal("Mật khẩu đăng ký không được trùng với Số điện thoại!");
                return;
            }
            if (matKhau != matKhauXN)
            {
                HienLoiDKModal("Xác nhận mật khẩu không khớp!");
                return;
            }
            if (!string.IsNullOrEmpty(email) && !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                HienLoiDKModal("Định dạng Email không hợp lệ!");
                return;
            }
            if (hoTen.IndexOfAny(new[] { '<', '>' }) >= 0 || txtDKDiaChi.Text.IndexOfAny(new[] { '<', '>' }) >= 0)
            {
                HienLoiDKModal("Họ tên và Địa chỉ không được chứa ký tự đặc biệt (< hoặc >)!");
                return;
            }

            object ngaySinhParam = DBNull.Value;
            if (!string.IsNullOrEmpty(txtDKNgaySinh.Text))
            {
                DateTime ns;
                if (!DateTime.TryParse(txtDKNgaySinh.Text, out ns) || ns >= DateTime.Today || ns.Year < 1940)
                {
                    HienLoiDKModal("Ngày sinh không hợp lệ!");
                    return;
                }
                ngaySinhParam = ns;
            }

            try
            {
                int check = Convert.ToInt32(kncsdl.LayGiaTriDon(
                    "SELECT COUNT(*) FROM HOCVIEN WHERE dienthoai=@dt OR (@em <> '' AND LOWER(LTRIM(RTRIM(ISNULL(email,'')))) = LOWER(@em))",
                    new[] { new SqlParameter("@dt", sdt), new SqlParameter("@em", email) }));
                if (check > 0)
                {
                    HienLoiDKModal("Số điện thoại hoặc Email này đã được đăng ký trong hệ thống!");
                    return;
                }

                int soLuong = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM HOCVIEN")) + 1;
                string maHV = "HV" + soLuong.ToString("D2");
                while (Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM HOCVIEN WHERE mahocvien=@ma", new[] { new SqlParameter("@ma", maHV) })) > 0)
                    maHV = "HV" + (++soLuong).ToString("D2");

                string sql = "INSERT INTO HOCVIEN(mahocvien, hoten, ngaysinh, gioitinh, dienthoai, email, diachi, matkhau) VALUES(@ma, @ten, @ns, @gt, @dt, @em, @dc, @mk)";
                kncsdl.ThucThiLenh(sql, new[] {
                    new SqlParameter("@ma", maHV),
                    new SqlParameter("@ten", hoTen),
                    new SqlParameter("@ns", ngaySinhParam),
                    new SqlParameter("@gt", ddlDKGioiTinh.SelectedValue),
                    new SqlParameter("@dt", sdt),
                    new SqlParameter("@em", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email),
                    new SqlParameter("@dc", txtDKDiaChi.Text.Trim()),
                    new SqlParameter("@mk", matKhau)
                });

                Session["TenDangNhap"] = maHV;
                Session["HoTen"] = hoTen;
                Session["Quyen"] = "HocVien";
                Session.Remove("BatBuocDoiMK");
                kncsdl.GhiNhatKy(maHV, "Tự đăng ký tài khoản", $"Học viên {hoTen} tự tạo tài khoản");

                // Đăng ký thành công -> Đăng nhập ngay và chuyển hướng thẳng vào trang đăng ký học phần
                ChuyenHuongTheoQuyen("HocVien");
            }
            catch
            {
                HienLoiDKModal("Không thể tạo tài khoản lúc này. Vui lòng kiểm tra lại thông tin!");
            }
        }

        private void HienLoiDKModal(string msg)
        {
            pnlLoiDK.Visible = true;
            litLoiDK.Text = HttpUtility.HtmlEncode(msg);
            // Giữ nguyên pop-up mở bằng đoạn lệnh JavaScript an toàn sau khi UpdatePanel cập nhật
            string script = @"
                var modalEl = document.getElementById('modalDangKy');
                if (modalEl && window.bootstrap) {
                    window.bootstrap.Modal.getOrCreateInstance(modalEl).show();
                }
            ";
            ScriptManager.RegisterStartupScript(this, GetType(), "reopen_dk", script, true);
        }

        private void HienLoi(string msg)
        {
            pnlLoi.Visible = true;
            litLoi.Text = HttpUtility.HtmlEncode(msg);
        }
    }
}