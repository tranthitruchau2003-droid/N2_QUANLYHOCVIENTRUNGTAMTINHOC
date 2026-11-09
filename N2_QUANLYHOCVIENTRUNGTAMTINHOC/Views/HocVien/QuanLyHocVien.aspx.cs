using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI.WebControls;
using N2_QUANLYHOCVIENTRUNGTAMTINHOC.DAL;

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
            }
        }

        private void TaiDanhSach(string tuKhoa = "")
        {
            string sql = "SELECT * FROM HOCVIEN WHERE (@kw = '' OR mahocvien LIKE @kw OR hoten LIKE @kw OR dienthoai LIKE @kw) ORDER BY mahocvien DESC";
            SqlParameter[] p = { new SqlParameter("@kw", string.IsNullOrEmpty(tuKhoa) ? "" : "%" + tuKhoa + "%") };
            gvHocVien.DataSource = kncsdl.DocDuLieu(sql, p);
            gvHocVien.DataBind();
        }

        private void TaiGoiYTimKiem()
        {
            DataTable dt = kncsdl.DocDuLieu("SELECT mahocvien, hoten, dienthoai FROM HOCVIEN");
            StringBuilder sb = new StringBuilder();
            foreach (DataRow r in dt.Rows)
            {
                sb.AppendFormat("<option value=\"{0}\">{1} - {2}</option>", r["mahocvien"], r["hoten"], r["dienthoai"]);
                sb.AppendFormat("<option value=\"{0}\">{1} ({2})</option>", r["hoten"], r["mahocvien"], r["dienthoai"]);
                if (!string.IsNullOrEmpty(r["dienthoai"].ToString()))
                {
                    sb.AppendFormat("<option value=\"{0}\">{1} - {2}</option>", r["dienthoai"], r["mahocvien"], r["hoten"]);
                }
            }
            litGoiY.Text = sb.ToString();
        }

        private void HienThongBao(string msg, bool laLoi = false)
        {
            pnlThongBao.Visible = true;
            pnlThongBao.CssClass = laLoi
                ? "alert alert-danger text-danger fw-semibold alert-dismissible fade show"
                : "alert alert-success text-success fw-semibold alert-dismissible fade show";
            litThongBao.Text = msg;
        }

        protected void btnThem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaHV.Text) || string.IsNullOrWhiteSpace(txtHoTen.Text))
            {
                HienThongBao("Vui lòng nhập đầy đủ Mã học viên và Họ tên!", true);
                return;
            }

            try
            {
                string sql = @"INSERT INTO HOCVIEN(mahocvien, hoten, ngaysinh, gioitinh, dienthoai, email, diachi)
                               VALUES(@ma, @ten, @ns, @gt, @dt, @em, @dc)";
                SqlParameter[] p = {
                    new SqlParameter("@ma", txtMaHV.Text.Trim()),
                    new SqlParameter("@ten", txtHoTen.Text.Trim()),
                    new SqlParameter("@ns", string.IsNullOrEmpty(txtNgaySinh.Text) ? (object)DBNull.Value : txtNgaySinh.Text),
                    new SqlParameter("@gt", ddlGioiTinh.SelectedValue),
                    new SqlParameter("@dt", txtDienThoai.Text.Trim()),
                    new SqlParameter("@em", txtEmail.Text.Trim()),
                    new SqlParameter("@dc", txtDiaChi.Text.Trim())
                };
                kncsdl.ThucThiLenh(sql, p);
                kncsdl.GhiNhatKy(Session["TenDangNhap"]?.ToString(), "Thêm học viên", "Mã HV: " + txtMaHV.Text.Trim());
                HienThongBao("Thêm học viên mới thành công!");
                TaiDanhSach();
                TaiGoiYTimKiem();
            }
            catch (Exception ex) { HienThongBao("Lỗi khi thêm: " + ex.Message, true); }
        }

        protected void btnCapNhat_Click(object sender, EventArgs e)
        {
            try
            {
                string sql = @"UPDATE HOCVIEN SET hoten=@ten, ngaysinh=@ns, gioitinh=@gt, dienthoai=@dt, email=@em, diachi=@dc
                               WHERE mahocvien=@ma";
                SqlParameter[] p = {
                    new SqlParameter("@ma", txtMaHV.Text.Trim()),
                    new SqlParameter("@ten", txtHoTen.Text.Trim()),
                    new SqlParameter("@ns", string.IsNullOrEmpty(txtNgaySinh.Text) ? (object)DBNull.Value : txtNgaySinh.Text),
                    new SqlParameter("@gt", ddlGioiTinh.SelectedValue),
                    new SqlParameter("@dt", txtDienThoai.Text.Trim()),
                    new SqlParameter("@em", txtEmail.Text.Trim()),
                    new SqlParameter("@dc", txtDiaChi.Text.Trim())
                };
                kncsdl.ThucThiLenh(sql, p);
                kncsdl.GhiNhatKy(Session["TenDangNhap"]?.ToString(), "Sửa học viên", "Mã HV: " + txtMaHV.Text.Trim());
                HienThongBao("Cập nhật thông tin học viên thành công!");
                TaiDanhSach();
                TaiGoiYTimKiem();
            }
            catch (Exception ex) { HienThongBao("Lỗi cập nhật: " + ex.Message, true); }
        }

        protected void gvHocVien_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            string ma = e.CommandArgument.ToString();
            if (e.CommandName == "Chon")
            {
                DataTable dt = kncsdl.DocDuLieu("SELECT * FROM HOCVIEN WHERE mahocvien=@ma", new[] { new SqlParameter("@ma", ma) });
                if (dt.Rows.Count > 0)
                {
                    DataRow r = dt.Rows[0];
                    txtMaHV.Text = r["mahocvien"].ToString();
                    txtMaHV.ReadOnly = true;
                    txtHoTen.Text = r["hoten"].ToString();
                    if (r["ngaysinh"] != DBNull.Value) txtNgaySinh.Text = Convert.ToDateTime(r["ngaysinh"]).ToString("yyyy-MM-dd");
                    ddlGioiTinh.SelectedValue = r["gioitinh"].ToString();
                    txtDienThoai.Text = r["dienthoai"].ToString();
                    txtEmail.Text = r["email"].ToString();
                    txtDiaChi.Text = r["diachi"].ToString();
                }
            }
            else if (e.CommandName == "Xoa")
            {
                int countDK = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM DANGKY WHERE mahocvien=@ma", new[] { new SqlParameter("@ma", ma) }));
                int countTG = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM THAM_GIA WHERE mahocvien=@ma", new[] { new SqlParameter("@ma", ma) }));
                if (countDK > 0 || countTG > 0)
                {
                    HienThongBao("Không thể xóa học viên này vì đã có dữ liệu Đăng ký khóa học hoặc Tham gia kỳ thi!", true);
                    return;
                }
                kncsdl.ThucThiLenh("DELETE FROM HOCVIEN WHERE mahocvien=@ma", new[] { new SqlParameter("@ma", ma) });
                kncsdl.GhiNhatKy(Session["TenDangNhap"]?.ToString(), "Xóa học viên", "Mã HV: " + ma);
                HienThongBao("Đã xóa học viên thành công.");
                TaiDanhSach();
                TaiGoiYTimKiem();
            }
        }

        protected void btnLamMoi_Click(object sender, EventArgs e)
        {
            txtMaHV.Text = txtHoTen.Text = txtNgaySinh.Text = txtDienThoai.Text = txtEmail.Text = txtDiaChi.Text = "";
            txtMaHV.ReadOnly = false;
        }

        protected void btnTim_Click(object sender, EventArgs e)
        {
            gvHocVien.PageIndex = 0;
            TaiDanhSach(txtTimKiem.Text.Trim());
        }

        protected void gvHocVien_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvHocVien.PageIndex = e.NewPageIndex;
            TaiDanhSach(txtTimKiem.Text.Trim());
        }
    }
}