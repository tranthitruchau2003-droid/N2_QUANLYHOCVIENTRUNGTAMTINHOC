using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI.WebControls;
using N2_QUANLYHOCVIENTRUNGTAMTINHOC.DAL;

namespace N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.GiangVien
{
    public partial class QuanLyGiangVien : System.Web.UI.Page
    {
        private bool LaQuanTriHoacNV
        {
            get
            {
                string q = MasterPage.ChuanHoaQuyen(Session["Quyen"]);
                return q == "Admin" || q == "NhanVien";
            }
        }

        private string TuKhoaHienTai
        {
            get { return ViewState["TuKhoaTimGV"]?.ToString() ?? ""; }
            set { ViewState["TuKhoaTimGV"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            txtEmail.Attributes.Add("readonly", "readonly");
            txtEmail.Attributes.Add("onfocus", "this.removeAttribute('readonly');");

            if (IsPostBack)
            {
                pnlTB.Visible = false;
            }
            else
            {
                TuKhoaHienTai = "";
                txtTim.Text = "";
                TaiGV();
                SinhMaGVMoi();
            }
        }

        private void SinhMaGVMoi()
        {
            int soLuong = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM GIANGVIEN")) + 1;
            string maGV = "GV" + soLuong.ToString("D2");
            while (Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM GIANGVIEN WHERE magiangvien=@ma", new[] { new SqlParameter("@ma", maGV) })) > 0)
            {
                soLuong++;
                maGV = "GV" + soLuong.ToString("D2");
            }
            txtMaGV.Text = maGV;
        }

        private void TaiGV()
        {
            string kw = TuKhoaHienTai;
            string sql = @"SELECT gv.*, (SELECT COUNT(*) FROM LOPHOC l WHERE l.magiangvien = gv.magiangvien) AS SoLop 
                           FROM GIANGVIEN gv
                           WHERE (@kw = '' OR gv.magiangvien COLLATE Latin1_General_CI_AI LIKE @kw OR gv.hoten COLLATE Latin1_General_CI_AI LIKE @kw OR gv.dienthoai COLLATE Latin1_General_CI_AI LIKE @kw OR gv.chuyenmon COLLATE Latin1_General_CI_AI LIKE @kw)
                           ORDER BY gv.magiangvien DESC";
            gvGV.DataSource = kncsdl.DocDuLieu(sql, new[] { new SqlParameter("@kw", string.IsNullOrEmpty(kw) ? "" : "%" + kw + "%") });
            gvGV.DataBind();

            TaiGoiYTimKiem();
        }

        private void TaiGoiYTimKiem()
        {
            DataTable dt = kncsdl.DocDuLieu("SELECT magiangvien, hoten, dienthoai FROM GIANGVIEN ORDER BY magiangvien DESC");
            StringBuilder sb = new StringBuilder();
            foreach (DataRow r in dt.Rows)
            {
                sb.AppendFormat("<button type=\"button\" class=\"list-group-item list-group-item-action py-2 px-2 small goi-y-item\" data-value=\"{0}\"><strong>{0}</strong> - {1} ({2})</button>",
                    HttpUtility.HtmlAttributeEncode(Convert.ToString(r["magiangvien"])),
                    HttpUtility.HtmlEncode(Convert.ToString(r["hoten"])),
                    HttpUtility.HtmlEncode(Convert.ToString(r["dienthoai"])));
            }
            litGoiYGV.Text = sb.ToString();
        }

        private void HienThongBao(string msg, bool laLoi = false)
        {
            pnlTB.Visible = true;
            pnlTB.CssClass = laLoi
                ? "alert alert-danger text-danger fw-semibold alert-dismissible fade show shadow"
                : "alert alert-success text-success fw-semibold alert-dismissible fade show shadow";
            litTB.Text = HttpUtility.HtmlEncode(msg);
        }

        private bool KiemTraDuLieu(bool laCapNhat = false)
        {
            if (string.IsNullOrWhiteSpace(txtHoTen.Text))
            {
                HienThongBao("Vui lòng nhập Họ tên giảng viên!", true);
                txtHoTen.Focus();
                return false;
            }

            string sdt = txtSDT.Text.Trim();
            if (string.IsNullOrEmpty(sdt) || !Regex.IsMatch(sdt, @"^0\d{9}$"))
            {
                HienThongBao("Số điện thoại không hợp lệ (phải gồm 10 chữ số và bắt đầu bằng số 0)!", true);
                txtSDT.Focus();
                return false;
            }

            string email = txtEmail.Text.Trim();
            if (!string.IsNullOrEmpty(email))
            {
                if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    HienThongBao("Email không hợp lệ (ví dụ: giangvien@gmail.com)!", true);
                    txtEmail.Focus();
                    return false;
                }

                string sqlTrungEmail = laCapNhat
                    ? "SELECT COUNT(*) FROM GIANGVIEN WHERE LOWER(LTRIM(RTRIM(ISNULL(email,'')))) = LOWER(@em) AND magiangvien <> @ma"
                    : "SELECT COUNT(*) FROM GIANGVIEN WHERE LOWER(LTRIM(RTRIM(ISNULL(email,'')))) = LOWER(@em)";
                SqlParameter[] pEmail = laCapNhat
                    ? new[] { new SqlParameter("@em", email), new SqlParameter("@ma", txtMaGV.Text.Trim()) }
                    : new[] { new SqlParameter("@em", email) };

                if (Convert.ToInt32(kncsdl.LayGiaTriDon(sqlTrungEmail, pEmail)) > 0)
                {
                    HienThongBao("Email này đã thuộc về giảng viên khác!", true);
                    txtEmail.Focus();
                    return false;
                }
            }

            string sqlTrungSDT = laCapNhat
                ? "SELECT COUNT(*) FROM GIANGVIEN WHERE dienthoai=@dt AND magiangvien<>@ma"
                : "SELECT COUNT(*) FROM GIANGVIEN WHERE dienthoai=@dt";
            SqlParameter[] pSDT = laCapNhat
                ? new[] { new SqlParameter("@dt", sdt), new SqlParameter("@ma", txtMaGV.Text.Trim()) }
                : new[] { new SqlParameter("@dt", sdt) };

            if (Convert.ToInt32(kncsdl.LayGiaTriDon(sqlTrungSDT, pSDT)) > 0)
            {
                HienThongBao("Số điện thoại này đã thuộc về giảng viên khác!", true);
                txtSDT.Focus();
                return false;
            }

            return true;
        }

        private void LamMoiForm()
        {
            txtHoTen.Text = txtSDT.Text = txtEmail.Text = txtChuyenMon.Text = txtHocVi.Text = "";
            SinhMaGVMoi();
        }

        protected void btnThem_Click(object sender, EventArgs e)
        {
            if (!LaQuanTriHoacNV)
            {
                HienThongBao("Bạn không có quyền thêm giảng viên!", true);
                return;
            }
            if (!KiemTraDuLieu(false)) return;

            SinhMaGVMoi();
            string maGV = txtMaGV.Text.Trim();
            string hoTen = txtHoTen.Text.Trim();

            string sql = "INSERT INTO GIANGVIEN(magiangvien, hoten, dienthoai, email, chuyenmon, hocvi) VALUES(@ma, @ten, @dt, @em, @cm, @hv)";
            SqlParameter[] p = {
                new SqlParameter("@ma", maGV),
                new SqlParameter("@ten", hoTen),
                new SqlParameter("@dt", txtSDT.Text.Trim()),
                new SqlParameter("@em", string.IsNullOrWhiteSpace(txtEmail.Text) ? (object)DBNull.Value : txtEmail.Text.Trim()),
                new SqlParameter("@cm", txtChuyenMon.Text.Trim()),
                new SqlParameter("@hv", txtHocVi.Text.Trim())
            };
            kncsdl.ThucThiLenh(sql, p);
            kncsdl.GhiNhatKy(Session["TenDangNhap"]?.ToString(), "Thêm giảng viên", $"Mã GV: {maGV} - {hoTen}");
            HienThongBao("Thêm giảng viên thành công!");
            gvGV.PageIndex = 0;
            TaiGV();
            LamMoiForm();
        }

        protected void btnSua_Click(object sender, EventArgs e)
        {
            if (!LaQuanTriHoacNV)
            {
                HienThongBao("Bạn không có quyền cập nhật giảng viên!", true);
                return;
            }

            string maGV = txtMaGV.Text.Trim();
            int check = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM GIANGVIEN WHERE magiangvien=@ma", new[] { new SqlParameter("@ma", maGV) }));
            if (check == 0)
            {
                HienThongBao("Giảng viên này chưa có trong danh sách. Hãy bấm Thêm!", true);
                return;
            }

            if (!KiemTraDuLieu(true)) return;

            string hoTen = txtHoTen.Text.Trim();
            string sql = "UPDATE GIANGVIEN SET hoten=@ten, dienthoai=@dt, email=@em, chuyenmon=@cm, hocvi=@hv WHERE magiangvien=@ma";
            SqlParameter[] p = {
                new SqlParameter("@ma", maGV),
                new SqlParameter("@ten", hoTen),
                new SqlParameter("@dt", txtSDT.Text.Trim()),
                new SqlParameter("@em", string.IsNullOrWhiteSpace(txtEmail.Text) ? (object)DBNull.Value : txtEmail.Text.Trim()),
                new SqlParameter("@cm", txtChuyenMon.Text.Trim()),
                new SqlParameter("@hv", txtHocVi.Text.Trim())
            };
            kncsdl.ThucThiLenh(sql, p);
            kncsdl.GhiNhatKy(Session["TenDangNhap"]?.ToString(), "Sửa giảng viên", $"Mã GV: {maGV} - {hoTen}");
            HienThongBao("Cập nhật giảng viên thành công!");
            TaiGV();
            LamMoiForm();
        }

        protected void btnLamMoi_Click(object sender, EventArgs e)
        {
            LamMoiForm();
            TuKhoaHienTai = "";
            txtTim.Text = "";
            gvGV.PageIndex = 0;
            TaiGV();
            pnlTB.Visible = false;
        }

        protected void gvGV_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Chon")
            {
                DataTable dt = kncsdl.DocDuLieu("SELECT * FROM GIANGVIEN WHERE magiangvien=@ma", new[] { new SqlParameter("@ma", e.CommandArgument.ToString()) });
                if (dt.Rows.Count > 0)
                {
                    DataRow r = dt.Rows[0];
                    txtMaGV.Text = r["magiangvien"].ToString();
                    txtHoTen.Text = r["hoten"].ToString();
                    txtSDT.Text = r["dienthoai"].ToString();
                    txtEmail.Text = r["email"] != DBNull.Value ? r["email"].ToString() : "";
                    txtChuyenMon.Text = r["chuyenmon"].ToString();
                    txtHocVi.Text = r["hocvi"].ToString();
                    pnlTB.Visible = false;
                }
            }
        }

        protected void btnXoaChinhThuc_Click(object sender, EventArgs e)
        {
            if (!LaQuanTriHoacNV)
            {
                HienThongBao("Bạn không có quyền xóa giảng viên!", true);
                return;
            }

            string ma = hfMaGVXoa.Value;
            if (!string.IsNullOrEmpty(ma))
            {
                try
                {
                    int soLop = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM LOPHOC WHERE magiangvien=@ma", new[] { new SqlParameter("@ma", ma) }));
                    if (soLop > 0)
                    {
                        HienThongBao("Không thể xóa giảng viên này vì đang phụ trách lớp học!", true);
                        return;
                    }

                    kncsdl.ThucThiLenh("DELETE FROM GIANGVIEN WHERE magiangvien=@ma", new[] { new SqlParameter("@ma", ma) });
                    kncsdl.GhiNhatKy(Session["TenDangNhap"]?.ToString(), "Xóa giảng viên", $"Mã GV: {ma}");
                    HienThongBao("Đã xóa giảng viên thành công!");

                    if (gvGV.Rows.Count == 1 && gvGV.PageIndex > 0)
                    {
                        gvGV.PageIndex--;
                    }
                    TaiGV();
                    LamMoiForm();
                }
                catch
                {
                    HienThongBao("Không thể xóa giảng viên này do đang ràng buộc dữ liệu!", true);
                }
            }
        }

        protected void btnTim_Click(object sender, EventArgs e)
        {
            TuKhoaHienTai = txtTim.Text.Trim();
            gvGV.PageIndex = 0;
            TaiGV();
            pnlTB.Visible = false;
        }

        protected void gvGV_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvGV.PageIndex = e.NewPageIndex;
            TaiGV();
            pnlTB.Visible = false;
        }
    }
}