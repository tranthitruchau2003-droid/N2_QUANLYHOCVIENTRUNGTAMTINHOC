using System;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Web.UI;
using N2_QUANLYHOCVIENTRUNGTAMTINHOC.DAL;

namespace N2_QUANLYHOCVIENTRUNGTAMTINHOC
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e) { }

        protected void btnDangNhap_Click(object sender, EventArgs e)
        {
            string u = txtTaiKhoan.Text.Trim();
            string p = txtMatKhau.Text.Trim();
            if (string.IsNullOrEmpty(u) || string.IsNullOrEmpty(p)) { HienLoi("Nhập đầy đủ Tên đăng nhập và Mật khẩu!"); return; }

            // 1. Kiểm tra Admin (Bảng NGUOIDUNG)
            try
            {
                DataTable dtAdmin = kncsdl.DocDuLieu("SELECT * FROM TAIKHOAN WHERE tendangnhap=@u AND matkhau=@p", new[] { new SqlParameter("@u", u), new SqlParameter("@p", p) });
                if (dtAdmin.Rows.Count > 0)
                {
                    Session["TenDangNhap"] = dtAdmin.Rows[0]["tendangnhap"].ToString();
                    Session["HoTen"] = dtAdmin.Rows[0]["hoten"].ToString();
                    Session["Quyen"] = dtAdmin.Rows[0]["quyen"].ToString();
                    Response.Redirect("~/Dashboard.aspx"); return;
                }
            }
            catch { } // Bỏ qua nếu chưa tạo bảng NGUOIDUNG

            // Admin cứng dự phòng
            if (u == "admin" && p == "123456")
            {
                Session["TenDangNhap"] = "admin"; Session["HoTen"] = "Quản trị Hệ thống"; Session["Quyen"] = "Admin";
                Response.Redirect("~/Dashboard.aspx"); return;
            }

            // 2. Kiểm tra Giảng viên (Đổi mật khẩu thì check cột matkhau, mặc định check dienthoai)
            DataTable dtGV = kncsdl.DocDuLieu("SELECT * FROM GIANGVIEN WHERE (magiangvien=@u OR email=@u) AND (ISNULL(matkhau, dienthoai)=@p OR dienthoai=@p)",
                new[] { new SqlParameter("@u", u), new SqlParameter("@p", p) });
            if (dtGV.Rows.Count > 0)
            {
                Session["TenDangNhap"] = dtGV.Rows[0]["magiangvien"].ToString();
                Session["HoTen"] = dtGV.Rows[0]["hoten"].ToString();
                Session["Quyen"] = "GiangVien";
                Response.Redirect("~/Views/LopHoc/QuanLyLopHoc.aspx"); return; // Cho GV vào thẳng trang Quản lý lớp (có bảng điểm danh)
            }

            // 3. Kiểm tra Học viên
            DataTable dtHV = kncsdl.DocDuLieu("SELECT * FROM HOCVIEN WHERE (mahocvien=@u OR dienthoai=@u OR email=@u) AND (ISNULL(matkhau, dienthoai)=@p OR dienthoai=@p)",
                new[] { new SqlParameter("@u", u), new SqlParameter("@p", p) });
            if (dtHV.Rows.Count > 0)
            {
                Session["TenDangNhap"] = dtHV.Rows[0]["mahocvien"].ToString();
                Session["HoTen"] = dtHV.Rows[0]["hoten"].ToString();
                Session["Quyen"] = "HocVien";
                Response.Redirect("~/Views/DangKy/QuanLyDangKy.aspx"); return; // HV vào thẳng trang Xếp lớp / Đăng ký
            }

            HienLoi("Tên đăng nhập hoặc mật khẩu không chính xác!");
        }

        // HÀM BỊ THIẾU GÂY LỖI CS1061: HỌC VIÊN TỰ ĐĂNG KÝ
        protected void btnDangKyTK_Click(object sender, EventArgs e)
        {
            string hoTen = txtDKHoTen.Text.Trim(); string sdt = txtDKDienThoai.Text.Trim(); string matKhau = txtDKMatKhau.Text.Trim();
            if (string.IsNullOrEmpty(hoTen) || !Regex.IsMatch(sdt, @"^0\d{9}$") || string.IsNullOrEmpty(matKhau))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "dk_loi", "alert('Vui lòng nhập Họ tên, Mật khẩu và SĐT hợp lệ (10 số)!'); openModal('modalDangKy');", true);
                return;
            }

            int check = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM HOCVIEN WHERE dienthoai=@dt OR (email <> '' AND email=@em)",
                new[] { new SqlParameter("@dt", sdt), new SqlParameter("@em", txtDKEmail.Text.Trim()) }));
            if (check > 0) { ScriptManager.RegisterStartupScript(this, GetType(), "dk_loi2", "alert('SĐT hoặc Email đã tồn tại!'); openModal('modalDangKy');", true); return; }

            int soLuong = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM HOCVIEN")) + 1;
            string maHV = "HV" + soLuong.ToString("D2");
            while (Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM HOCVIEN WHERE mahocvien=@ma", new[] { new SqlParameter("@ma", maHV) })) > 0)
                maHV = "HV" + (++soLuong).ToString("D2");

            string sql = "INSERT INTO HOCVIEN(mahocvien, hoten, ngaysinh, gioitinh, dienthoai, email, diachi, matkhau) VALUES(@ma, @ten, @ns, @gt, @dt, @em, @dc, @mk)";
            kncsdl.ThucThiLenh(sql, new[] {
                new SqlParameter("@ma", maHV), new SqlParameter("@ten", hoTen),
                new SqlParameter("@ns", string.IsNullOrEmpty(txtDKNgaySinh.Text) ? (object)DBNull.Value : txtDKNgaySinh.Text),
                new SqlParameter("@gt", ddlDKGioiTinh.SelectedValue), new SqlParameter("@dt", sdt),
                new SqlParameter("@em", txtDKEmail.Text.Trim()), new SqlParameter("@dc", txtDKDiaChi.Text.Trim()), new SqlParameter("@mk", matKhau)
            });

            Session["TenDangNhap"] = maHV; Session["HoTen"] = hoTen; Session["Quyen"] = "HocVien";
            Response.Redirect("~/Views/DangKy/QuanLyDangKy.aspx");
        }

        protected void btnTraCuuCC_Click(object sender, EventArgs e)
        {
            string sh = txtMaTraCuu.Text.Trim();
            string sql = @"SELECT hv.hoten, kh.tenkhoahoc, cc.xeploai, cc.ngaycap 
                           FROM CHUNGCHI cc 
                           JOIN THAM_GIA tg ON cc.matg = tg.matg 
                           JOIN HOCVIEN hv ON tg.mahocvien = hv.mahocvien 
                           JOIN KYTHI kt ON tg.makythi = kt.makythi 
                           JOIN KHOAHOC kh ON kt.makhoahoc = kh.makhoahoc 
                           WHERE cc.sohieuchungchi=@sh";
            DataTable dt = kncsdl.DocDuLieu(sql, new[] { new SqlParameter("@sh", sh) });

            if (dt.Rows.Count > 0)
            {
                pnlKetQuaCC.Visible = true; pnlLoiCC.Visible = false;
                litCCHoTen.Text = dt.Rows[0]["hoten"].ToString();
                litCCKhoaHoc.Text = dt.Rows[0]["tenkhoahoc"].ToString();
                litCCXepLoai.Text = dt.Rows[0]["xeploai"].ToString();
                litCCNgayCap.Text = Convert.ToDateTime(dt.Rows[0]["ngaycap"]).ToString("dd/MM/yyyy");
            }
            else
            {
                pnlKetQuaCC.Visible = false; pnlLoiCC.Visible = true;
            }
            ScriptManager.RegisterStartupScript(this, GetType(), "showTraCuu", "openModal('modalTraCuu');", true);
        }

        private void HienLoi(string msg) { pnlLoi.Visible = true; litLoi.Text = msg; }
    }
}