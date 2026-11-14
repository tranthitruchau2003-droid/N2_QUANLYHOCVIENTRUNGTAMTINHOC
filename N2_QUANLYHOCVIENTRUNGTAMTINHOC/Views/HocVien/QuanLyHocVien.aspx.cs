using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI.WebControls;
using N2_QUANLYHOCVIENTRUNGTAMTINHOC.DAL;
using System.Text.RegularExpressions;

namespace N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.HocVien
{
    public partial class QuanLyHocVien : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                TaiDanhSach();
                TaiGoiYTimKiem();
                SinhMaHVMoi();
            }
        }

        private void SinhMaHVMoi()
        {
            int soLuong = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM HOCVIEN")) + 1;
            string maHV = "HV" + soLuong.ToString("D2");
            while (Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM HOCVIEN WHERE mahocvien=@ma", new[] { new SqlParameter("@ma", maHV) })) > 0)
            {
                soLuong++;
                maHV = "HV" + soLuong.ToString("D2");
            }
            txtMaHV.Text = maHV;
        }

        private void TaiDanhSach(string tuKhoa = "")
        {
            string sql = "SELECT * FROM HOCVIEN WHERE (@kw = '' OR mahocvien COLLATE Latin1_General_CI_AI LIKE @kw OR hoten COLLATE Latin1_General_CI_AI LIKE @kw OR dienthoai COLLATE Latin1_General_CI_AI LIKE @kw) ORDER BY mahocvien DESC";
            SqlParameter[] p = { new SqlParameter("@kw", string.IsNullOrEmpty(tuKhoa) ? "" : "%" + tuKhoa + "%") };
            gvHocVien.DataSource = kncsdl.DocDuLieu(sql, p);
            gvHocVien.DataBind();
        }

        private void TaiGoiYTimKiem()
        {
            DataTable dt = kncsdl.DocDuLieu("SELECT mahocvien, hoten, dienthoai FROM HOCVIEN ORDER BY mahocvien DESC");
            StringBuilder sb = new StringBuilder();
            foreach (DataRow r in dt.Rows)
            {
                sb.AppendFormat("<button type=\"button\" class=\"list-group-item list-group-item-action py-2 px-2 small goi-y-item\" data-value=\"{0}\"><strong>{0}</strong> - {1} - {2}</button>",
                    System.Web.HttpUtility.HtmlAttributeEncode(Convert.ToString(r["mahocvien"])), System.Web.HttpUtility.HtmlEncode(Convert.ToString(r["hoten"])), System.Web.HttpUtility.HtmlEncode(Convert.ToString(r["dienthoai"])));
            }
            litGoiY.Text = sb.ToString();
        }

        private void HienThongBao(string msg, bool laLoi = false)
        {
            pnlThongBao.Visible = true;
            pnlThongBao.CssClass = laLoi
                ? "alert alert-danger text-danger fw-semibold alert-dismissible fade show shadow"
                : "alert alert-success text-success fw-semibold alert-dismissible fade show shadow";
            litThongBao.Text = System.Web.HttpUtility.HtmlEncode(msg);
        }

        private bool KiemTraDuLieu()
        {
            if (string.IsNullOrWhiteSpace(txtHoTen.Text))
            {
                HienThongBao("Vui lòng nhập Họ tên học viên!", true);
                return false;
            }

            string sdt = txtDienThoai.Text.Trim();
            if (string.IsNullOrEmpty(sdt) || !Regex.IsMatch(sdt, @"^0\d{9}$"))
            {
                HienThongBao("Số điện thoại không hợp lệ (phải gồm 10 chữ số và bắt đầu bằng số 0)!", true);
                txtDienThoai.Focus();
                return false;
            }

            string email = txtEmail.Text.Trim();
            if (!string.IsNullOrEmpty(email) && !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                HienThongBao("Email không hợp lệ (ví dụ: ten@gmail.com)!", true);
                txtEmail.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtDiaChi.Text))
            {
                HienThongBao("Vui lòng nhập Địa chỉ học viên!", true);
                txtDiaChi.Focus();
                return false;
            }

            return true;
        }

        protected void btnThem_Click(object sender, EventArgs e)
        {
            if (!KiemTraDuLieu()) return;

            try
            {
                string sdt = txtDienThoai.Text.Trim();
                string email = txtEmail.Text.Trim();

                int trungSDT = Convert.ToInt32(kncsdl.LayGiaTriDon(
                    "SELECT COUNT(*) FROM HOCVIEN WHERE dienthoai=@dt",
                    new[] { new SqlParameter("@dt", sdt) }));
                if (trungSDT > 0)
                {
                    HienThongBao("Số điện thoại này đã tồn tại trong hệ thống, vui lòng nhập số khác!", true);
                    txtDienThoai.Focus();
                    return;
                }

                if (!string.IsNullOrEmpty(email))
                {
                    int trungEmail = Convert.ToInt32(kncsdl.LayGiaTriDon(
                        "SELECT COUNT(*) FROM HOCVIEN WHERE LOWER(LTRIM(RTRIM(ISNULL(email,''))))=LOWER(@em)",
                        new[] { new SqlParameter("@em", email) }));
                    if (trungEmail > 0)
                    {
                        HienThongBao("Email này đã được sử dụng cho học viên khác!", true);
                        txtEmail.Focus();
                        return;
                    }
                }

                SinhMaHVMoi();

                string sql = @"INSERT INTO HOCVIEN(mahocvien, hoten, ngaysinh, gioitinh, dienthoai, email, diachi)
                               VALUES(@ma, @ten, @ns, @gt, @dt, @em, @dc)";
                SqlParameter[] p = {
                    new SqlParameter("@ma", txtMaHV.Text.Trim()),
                    new SqlParameter("@ten", txtHoTen.Text.Trim()),
                    new SqlParameter("@ns", string.IsNullOrEmpty(txtNgaySinh.Text) ? (object)DBNull.Value : txtNgaySinh.Text),
                    new SqlParameter("@gt", ddlGioiTinh.SelectedValue),
                    new SqlParameter("@dt", sdt),
                    new SqlParameter("@em", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email),
                    new SqlParameter("@dc", txtDiaChi.Text.Trim())
                };
                kncsdl.ThucThiLenh(sql, p);
                kncsdl.GhiNhatKy(Session["TenDangNhap"]?.ToString(), "Thêm học viên", "Mã HV: " + txtMaHV.Text.Trim());
                HienThongBao("Thêm học viên mới thành công!", false);

                TaiDanhSach();
                TaiGoiYTimKiem();
                LamMoiForm();
            }
            catch { HienThongBao("Không thể thêm học viên lúc này. Vui lòng kiểm tra lại thông tin!", true); }
        }

        protected void btnCapNhat_Click(object sender, EventArgs e)
        {
            if (!KiemTraDuLieu()) return;

            try
            {
                int checkExist = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM HOCVIEN WHERE mahocvien=@ma", new[] { new SqlParameter("@ma", txtMaHV.Text.Trim()) }));
                if (checkExist == 0)
                {
                    HienThongBao("Học viên này chưa có trong danh sách. Hãy bấm Thêm mới!", true);
                    return;
                }

                string sdt = txtDienThoai.Text.Trim();
                string email = txtEmail.Text.Trim();

                int trungSDT = Convert.ToInt32(kncsdl.LayGiaTriDon(
                    "SELECT COUNT(*) FROM HOCVIEN WHERE dienthoai=@dt AND mahocvien<>@ma",
                    new[] { new SqlParameter("@dt", sdt), new SqlParameter("@ma", txtMaHV.Text.Trim()) }));
                if (trungSDT > 0)
                {
                    HienThongBao("Số điện thoại này đã thuộc về học viên khác!", true);
                    txtDienThoai.Focus();
                    return;
                }

                if (!string.IsNullOrEmpty(email))
                {
                    int trungEmail = Convert.ToInt32(kncsdl.LayGiaTriDon(
                        "SELECT COUNT(*) FROM HOCVIEN WHERE LOWER(LTRIM(RTRIM(ISNULL(email,''))))=LOWER(@em) AND mahocvien<>@ma",
                        new[] { new SqlParameter("@em", email), new SqlParameter("@ma", txtMaHV.Text.Trim()) }));
                    if (trungEmail > 0)
                    {
                        HienThongBao("Email này đã thuộc về học viên khác!", true);
                        txtEmail.Focus();
                        return;
                    }
                }

                string sql = @"UPDATE HOCVIEN SET hoten=@ten, ngaysinh=@ns, gioitinh=@gt, dienthoai=@dt, email=@em, diachi=@dc
                               WHERE mahocvien=@ma";
                SqlParameter[] p = {
                    new SqlParameter("@ma", txtMaHV.Text.Trim()),
                    new SqlParameter("@ten", txtHoTen.Text.Trim()),
                    new SqlParameter("@ns", string.IsNullOrEmpty(txtNgaySinh.Text) ? (object)DBNull.Value : txtNgaySinh.Text),
                    new SqlParameter("@gt", ddlGioiTinh.SelectedValue),
                    new SqlParameter("@dt", sdt),
                    new SqlParameter("@em", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email),
                    new SqlParameter("@dc", txtDiaChi.Text.Trim())
                };
                kncsdl.ThucThiLenh(sql, p);
                kncsdl.GhiNhatKy(Session["TenDangNhap"]?.ToString(), "Sửa học viên", "Mã HV: " + txtMaHV.Text.Trim());

                HienThongBao("Cập nhật thông tin học viên thành công!", false);
                TaiDanhSach(txtTimKiem.Text.Trim());
                TaiGoiYTimKiem();
                LamMoiForm();
            }
            catch { HienThongBao("Không thể cập nhật thông tin học viên!", true); }
        }

        protected void gvHocVien_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Chon")
            {
                string ma = e.CommandArgument.ToString();
                DataTable dt = kncsdl.DocDuLieu("SELECT * FROM HOCVIEN WHERE mahocvien=@ma", new[] { new SqlParameter("@ma", ma) });
                if (dt.Rows.Count > 0)
                {
                    DataRow r = dt.Rows[0];
                    txtMaHV.Text = r["mahocvien"].ToString();
                    txtHoTen.Text = r["hoten"].ToString();
                    if (r["ngaysinh"] != DBNull.Value) txtNgaySinh.Text = Convert.ToDateTime(r["ngaysinh"]).ToString("yyyy-MM-dd");
                    ddlGioiTinh.SelectedValue = r["gioitinh"].ToString();
                    txtDienThoai.Text = r["dienthoai"].ToString();
                    txtEmail.Text = r["email"].ToString();
                    txtDiaChi.Text = r["diachi"].ToString();

                    pnlThongBao.Visible = false;
                }
            }
        }

        protected void btnXoaChinhThuc_Click(object sender, EventArgs e)
        {
            string ma = hfMaHocVienXoa.Value;
            if (!string.IsNullOrEmpty(ma))
            {
                try
                {
                    int countDK = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM DANGKY WHERE mahocvien=@ma", new[] { new SqlParameter("@ma", ma) }));
                    int countTG = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM THAM_GIA WHERE mahocvien=@ma", new[] { new SqlParameter("@ma", ma) }));
                    int countDD = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM DIEMDANH WHERE mahocvien=@ma", new[] { new SqlParameter("@ma", ma) }));
                    if (countDK > 0 || countTG > 0 || countDD > 0)
                    {
                        HienThongBao("Không thể xóa học viên này vì đã có dữ liệu Đăng ký khóa học, Điểm danh hoặc Kỳ thi!", true);
                        return;
                    }
                    kncsdl.ThucThiLenh("DELETE FROM HOCVIEN WHERE mahocvien=@ma", new[] { new SqlParameter("@ma", ma) });
                    kncsdl.GhiNhatKy(Session["TenDangNhap"]?.ToString(), "Xóa học viên", "Mã HV: " + ma);
                    HienThongBao("Đã xóa học viên thành công.", false);
                    TaiDanhSach(txtTimKiem.Text.Trim());
                    TaiGoiYTimKiem();
                    LamMoiForm();
                }
                catch
                {
                    HienThongBao("Không thể xóa học viên này do đang ràng buộc dữ liệu!", true);
                }
            }
        }

        private void LamMoiForm()
        {
            txtHoTen.Text = txtNgaySinh.Text = txtDienThoai.Text = txtEmail.Text = txtDiaChi.Text = "";
            SinhMaHVMoi();
        }

        protected void btnLamMoi_Click(object sender, EventArgs e)
        {
            LamMoiForm();
            txtTimKiem.Text = "";
            TaiDanhSach();
            pnlThongBao.Visible = false;
        }

        protected void btnTim_Click(object sender, EventArgs e)
        {
            gvHocVien.PageIndex = 0;
            TaiDanhSach(txtTimKiem.Text.Trim());
            pnlThongBao.Visible = false;
        }

        protected void gvHocVien_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvHocVien.PageIndex = e.NewPageIndex;
            TaiDanhSach(txtTimKiem.Text.Trim());
            pnlThongBao.Visible = false;
        }
    }
}