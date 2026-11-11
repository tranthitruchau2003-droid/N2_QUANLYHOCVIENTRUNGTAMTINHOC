using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using N2_QUANLYHOCVIENTRUNGTAMTINHOC.DAL;

namespace N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.GiangVien
{
    public partial class QuanLyGiangVien : System.Web.UI.Page
    {
        private string TuKhoaHienTai
        {
            get { return ViewState["TuKhoaTimGV"]?.ToString() ?? ""; }
            set { ViewState["TuKhoaTimGV"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            txtEmail.Attributes.Add("readonly", "readonly");
            txtEmail.Attributes.Add("onfocus", "this.removeAttribute('readonly');");

            if (!IsPostBack)
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
                           WHERE (@kw = '' OR gv.magiangvien LIKE @kw OR gv.hoten LIKE @kw OR gv.dienthoai LIKE @kw OR gv.chuyenmon LIKE @kw)
                           ORDER BY gv.magiangvien DESC";
            gvGV.DataSource = kncsdl.DocDuLieu(sql, new[] { new SqlParameter("@kw", string.IsNullOrEmpty(kw) ? "" : "%" + kw + "%") });
            gvGV.DataBind();

            TaiGoiYTimKiem();
        }

        private void TaiGoiYTimKiem()
        {
            DataTable dt = kncsdl.DocDuLieu("SELECT magiangvien, hoten, dienthoai FROM GIANGVIEN ORDER BY magiangvien");
            StringBuilder sb = new StringBuilder();
            foreach (DataRow r in dt.Rows)
            {
                sb.AppendFormat("<button type=\"button\" class=\"list-group-item list-group-item-action py-2 px-2 small goi-y-item\" data-value=\"{0}\"><strong>{0}</strong> - {1} ({2})</button>",
                    r["magiangvien"], r["hoten"], r["dienthoai"]);
            }
            litGoiYGV.Text = sb.ToString();
        }

        private void HienThongBao(string msg, bool laLoi = false)
        {
            pnlTB.Visible = true;
            pnlTB.CssClass = laLoi
                ? "alert alert-danger text-danger fw-semibold alert-dismissible fade show shadow"
                : "alert alert-success text-success fw-semibold alert-dismissible fade show shadow";
            litTB.Text = msg;
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
                    ? "SELECT COUNT(*) FROM GIANGVIEN WHERE LOWER(LTRIM(RTRIM(email))) = LOWER(@em) AND magiangvien <> @ma"
                    : "SELECT COUNT(*) FROM GIANGVIEN WHERE LOWER(LTRIM(RTRIM(email))) = LOWER(@em)";
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
            if (!KiemTraDuLieu(false)) return;

            SinhMaGVMoi();
            string sql = "INSERT INTO GIANGVIEN(magiangvien, hoten, dienthoai, email, chuyenmon, hocvi) VALUES(@ma, @ten, @dt, @em, @cm, @hv)";
            SqlParameter[] p = {
                new SqlParameter("@ma", txtMaGV.Text.Trim()),
                new SqlParameter("@ten", txtHoTen.Text.Trim()),
                new SqlParameter("@dt", txtSDT.Text.Trim()),
                new SqlParameter("@em", txtEmail.Text.Trim()),
                new SqlParameter("@cm", txtChuyenMon.Text.Trim()),
                new SqlParameter("@hv", txtHocVi.Text.Trim())
            };
            kncsdl.ThucThiLenh(sql, p);
            HienThongBao("Thêm giảng viên thành công!");
            gvGV.PageIndex = 0;
            TaiGV();
            LamMoiForm();
        }

        protected void btnSua_Click(object sender, EventArgs e)
        {
            int check = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM GIANGVIEN WHERE magiangvien=@ma", new[] { new SqlParameter("@ma", txtMaGV.Text.Trim()) }));
            if (check == 0)
            {
                HienThongBao("Giảng viên này chưa có trong danh sách. Hãy bấm Thêm!", true);
                return;
            }

            if (!KiemTraDuLieu(true)) return;

            string sql = "UPDATE GIANGVIEN SET hoten=@ten, dienthoai=@dt, email=@em, chuyenmon=@cm, hocvi=@hv WHERE magiangvien=@ma";
            SqlParameter[] p = {
                new SqlParameter("@ma", txtMaGV.Text.Trim()),
                new SqlParameter("@ten", txtHoTen.Text.Trim()),
                new SqlParameter("@dt", txtSDT.Text.Trim()),
                new SqlParameter("@em", txtEmail.Text.Trim()),
                new SqlParameter("@cm", txtChuyenMon.Text.Trim()),
                new SqlParameter("@hv", txtHocVi.Text.Trim())
            };
            kncsdl.ThucThiLenh(sql, p);
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
                    txtEmail.Text = r["email"].ToString();
                    txtChuyenMon.Text = r["chuyenmon"].ToString();
                    txtHocVi.Text = r["hocvi"].ToString();
                    pnlTB.Visible = false;
                }
            }
        }

        protected void btnXoaChinhThuc_Click(object sender, EventArgs e)
        {
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
                    HienThongBao("Đã xóa giảng viên thành công!");

                    if (gvGV.Rows.Count == 1 && gvGV.PageIndex > 0)
                    {
                        gvGV.PageIndex--;
                    }
                    TaiGV();
                    LamMoiForm();
                }
                catch (Exception ex)
                {
                    HienThongBao("Lỗi khi xóa: " + ex.Message, true);
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