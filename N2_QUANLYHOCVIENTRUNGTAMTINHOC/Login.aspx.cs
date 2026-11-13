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

            Response.Redirect(url, false);
            Context.ApplicationInstance.CompleteRequest();
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
                DataTable dtAdmin = kncsdl.DocDuLieu(
                    "SELECT * FROM TAIKHOAN WHERE tendangnhap=@u AND matkhau=@p AND trangthai=1",
                    new[] { new SqlParameter("@u", u), new SqlParameter("@p", p) });

                if (dtAdmin.Rows.Count > 0)
                {
                    HttpRuntime.Cache.Remove(cacheKey);
                    string quyen = MasterPage.ChuanHoaQuyen(dtAdmin.Rows[0]["quyen"]);
                    if (string.IsNullOrEmpty(quyen)) quyen = "NhanVien";

                    Session["TenDangNhap"] = dtAdmin.Rows[0]["tendangnhap"].ToString();
                    Session["HoTen"] = dtAdmin.Rows[0]["hoten"].ToString();
                    Session["Quyen"] = quyen;
                    ChuyenHuongTheoQuyen(quyen);
                    return;
                }

           
                DataTable dtGV = kncsdl.DocDuLieu(
                    "SELECT * FROM GIANGVIEN WHERE (magiangvien=@u OR email=@u) AND ISNULL(NULLIF(matkhau, ''), dienthoai)=@p",
                    new[] { new SqlParameter("@u", u), new SqlParameter("@p", p) });

                if (dtGV.Rows.Count > 0)
                {
                    HttpRuntime.Cache.Remove(cacheKey);
                    string maGV = dtGV.Rows[0]["magiangvien"].ToString();
                    if (dtGV.Rows[0]["matkhau"] == DBNull.Value || string.IsNullOrEmpty(dtGV.Rows[0]["matkhau"].ToString()))
                    {
                        kncsdl.ThucThiLenh("UPDATE GIANGVIEN SET matkhau=@p WHERE magiangvien=@ma",
                            new[] { new SqlParameter("@p", p), new SqlParameter("@ma", maGV) });
                    }

                    Session["TenDangNhap"] = maGV;
                    Session["HoTen"] = dtGV.Rows[0]["hoten"].ToString();
                    Session["Quyen"] = "GiangVien";
                    ChuyenHuongTheoQuyen("GiangVien");
                    return;
                }

            
                DataTable dtHV = kncsdl.DocDuLieu(
                    "SELECT * FROM HOCVIEN WHERE (mahocvien=@u OR dienthoai=@u OR email=@u) AND ISNULL(NULLIF(matkhau, ''), dienthoai)=@p",
                    new[] { new SqlParameter("@u", u), new SqlParameter("@p", p) });

                if (dtHV.Rows.Count > 0)
                {
                    HttpRuntime.Cache.Remove(cacheKey);
                    string maHV = dtHV.Rows[0]["mahocvien"].ToString();
                    if (dtHV.Rows[0]["matkhau"] == DBNull.Value || string.IsNullOrEmpty(dtHV.Rows[0]["matkhau"].ToString()))
                    {
                        kncsdl.ThucThiLenh("UPDATE HOCVIEN SET matkhau=@p WHERE mahocvien=@ma",
                            new[] { new SqlParameter("@p", p), new SqlParameter("@ma", maHV) });
                    }

                    Session["TenDangNhap"] = maHV;
                    Session["HoTen"] = dtHV.Rows[0]["hoten"].ToString();
                    Session["Quyen"] = "HocVien";
                    ChuyenHuongTheoQuyen("HocVien");
                    return;
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

      
            if (string.IsNullOrEmpty(hoTen) || hoTen.Length > 100 || !Regex.IsMatch(sdt, @"^0\d{9}$") || matKhau.Length < 6)
            {
                HienAlertDangKy("Vui lòng nhập Họ tên hợp lệ, SĐT đủ 10 số và Mật khẩu tối thiểu 6 ký tự!");
                return;
            }

            if (!string.IsNullOrEmpty(email) && !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                HienAlertDangKy("Định dạng Email không hợp lệ!");
                return;
            }

            object ngaySinhParam = DBNull.Value;
            if (!string.IsNullOrEmpty(txtDKNgaySinh.Text))
            {
                DateTime ns;
                if (!DateTime.TryParse(txtDKNgaySinh.Text, out ns) || ns >= DateTime.Today || ns.Year < 1940)
                {
                    HienAlertDangKy("Ngày sinh không hợp lệ!");
                    return;
                }
                ngaySinhParam = ns;
            }

            try
            {
                int check = Convert.ToInt32(kncsdl.LayGiaTriDon(
                    "SELECT COUNT(*) FROM HOCVIEN WHERE dienthoai=@dt OR (@em <> '' AND LOWER(LTRIM(RTRIM(email))) = LOWER(@em))",
                    new[] { new SqlParameter("@dt", sdt), new SqlParameter("@em", email) }));
                if (check > 0)
                {
                    HienAlertDangKy("Số điện thoại hoặc Email này đã được đăng ký!");
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
                Response.Redirect("~/Views/DangKy/QuanLyDangKy.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch
            {
               
                HienAlertDangKy("Không thể tạo tài khoản lúc này. Vui lòng kiểm tra lại thông tin đã nhập!");
            }
        }

        private void HienAlertDangKy(string msg)
        {
            string safe = HttpUtility.JavaScriptStringEncode(msg);
            ScriptManager.RegisterStartupScript(this, GetType(), "dk_loi", $"alert('{safe}'); openModal('modalDangKy');", true);
        }

        private void HienLoi(string msg)
        {
            pnlLoi.Visible = true;
            litLoi.Text = HttpUtility.HtmlEncode(msg);
        }
    }
}