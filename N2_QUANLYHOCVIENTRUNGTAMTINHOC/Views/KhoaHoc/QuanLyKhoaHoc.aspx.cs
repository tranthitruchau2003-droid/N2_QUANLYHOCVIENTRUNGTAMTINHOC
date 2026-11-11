using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using N2_QUANLYHOCVIENTRUNGTAMTINHOC.DAL;

namespace N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.KhoaHoc
{
    public partial class QuanLyKhoaHoc : System.Web.UI.Page
    {
        private string TuKhoaHienTai
        {
            get { return ViewState["TuKhoaTim"]?.ToString() ?? ""; }
            set { ViewState["TuKhoaTim"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                TuKhoaHienTai = "";
                txtTim.Text = "";
                TaiDuLieu();
                SinhMaKHMoi();
            }
        }

        private void SinhMaKHMoi()
        {
            int soLuong = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM KHOAHOC")) + 1;
            string maKH = "KH" + soLuong.ToString("D2");
            while (Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM KHOAHOC WHERE makhoahoc=@ma", new[] { new SqlParameter("@ma", maKH) })) > 0)
            {
                soLuong++;
                maKH = "KH" + soLuong.ToString("D2");
            }
            txtMaKH.Text = maKH;
        }

        private void TaiDuLieu()
        {
            string kw = TuKhoaHienTai;
            string sql = "SELECT * FROM KHOAHOC WHERE (@kw='' OR tenkhoahoc LIKE @kw OR makhoahoc LIKE @kw) ORDER BY makhoahoc DESC";
            gvKhoaHoc.DataSource = kncsdl.DocDuLieu(sql, new[] { new SqlParameter("@kw", string.IsNullOrEmpty(kw) ? "" : "%" + kw + "%") });
            gvKhoaHoc.DataBind();

            TaiGoiYTimKiem();
        }

        private void TaiGoiYTimKiem()
        {
            DataTable dt = kncsdl.DocDuLieu("SELECT makhoahoc, tenkhoahoc FROM KHOAHOC ORDER BY makhoahoc");
            StringBuilder sbTim = new StringBuilder();
            StringBuilder sbTen = new StringBuilder();
            foreach (DataRow r in dt.Rows)
            {
                sbTim.AppendFormat("<button type=\"button\" class=\"list-group-item list-group-item-action py-2 px-2 small goi-y-item\" data-value=\"{0}\"><strong>{0}</strong> - {1}</button>",
                    r["makhoahoc"], r["tenkhoahoc"]);
                sbTen.AppendFormat("<button type=\"button\" class=\"list-group-item list-group-item-action py-2 px-2 small goi-y-item\" data-value=\"{0}\">{0}</button>",
                    r["tenkhoahoc"]);
            }
            litGoiYKH.Text = sbTim.ToString();
            litGoiYTenKH.Text = sbTen.ToString();
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
            if (string.IsNullOrWhiteSpace(txtTenKH.Text))
            {
                HienThongBao("Vui lòng nhập Tên khóa học!", true);
                txtTenKH.Focus();
                return false;
            }

            string sqlTrung = laCapNhat
                ? "SELECT COUNT(*) FROM KHOAHOC WHERE LOWER(LTRIM(RTRIM(tenkhoahoc))) = LOWER(@ten) AND makhoahoc <> @ma"
                : "SELECT COUNT(*) FROM KHOAHOC WHERE LOWER(LTRIM(RTRIM(tenkhoahoc))) = LOWER(@ten)";
            SqlParameter[] pTrung = laCapNhat
                ? new[] { new SqlParameter("@ten", txtTenKH.Text.Trim()), new SqlParameter("@ma", txtMaKH.Text.Trim()) }
                : new[] { new SqlParameter("@ten", txtTenKH.Text.Trim()) };

            int trungTen = Convert.ToInt32(kncsdl.LayGiaTriDon(sqlTrung, pTrung));
            if (trungTen > 0)
            {
                HienThongBao("Tên khóa học này đã tồn tại, vui lòng nhập tên khác!", true);
                txtTenKH.Focus();
                return false;
            }

            string thoiGian = txtThoiGian.Text.Trim();
            if (string.IsNullOrEmpty(thoiGian) || !Regex.IsMatch(thoiGian, @"\d"))
            {
                HienThongBao("Thời gian học không hợp lệ (phải có số, VD: 3 tháng, 36 buổi)!", true);
                txtThoiGian.Focus();
                return false;
            }

            return true;
        }

        private void LamMoiForm()
        {
            txtTenKH.Text = txtHocPhi.Text = txtThoiGian.Text = txtMoTa.Text = "";
            ddlTrangThai.SelectedIndex = 0;
            SinhMaKHMoi();
        }

        protected void btnThem_Click(object sender, EventArgs e)
        {
            if (!KiemTraDuLieu(false)) return;

            SinhMaKHMoi();
            string hocPhiStr = txtHocPhi.Text.Replace(",", "").Replace(".", "").Trim();
            decimal hocPhiVal = 0;
            decimal.TryParse(hocPhiStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out hocPhiVal);

            string sql = "INSERT INTO KHOAHOC(makhoahoc, tenkhoahoc, hocphi, thoigianhoc, mota, trangthaikhoahoc) VALUES(@ma, @ten, @hp, @tg, @mt, @tt)";
            SqlParameter[] p = {
                new SqlParameter("@ma", txtMaKH.Text.Trim()),
                new SqlParameter("@ten", txtTenKH.Text.Trim()),
                new SqlParameter("@hp", hocPhiVal),
                new SqlParameter("@tg", txtThoiGian.Text.Trim()),
                new SqlParameter("@mt", txtMoTa.Text.Trim()),
                new SqlParameter("@tt", ddlTrangThai.SelectedValue)
            };
            kncsdl.ThucThiLenh(sql, p);
            HienThongBao("Đã thêm khóa học thành công!");
            TaiDuLieu();
            LamMoiForm();
        }

        protected void btnSua_Click(object sender, EventArgs e)
        {
            int check = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM KHOAHOC WHERE makhoahoc=@ma", new[] { new SqlParameter("@ma", txtMaKH.Text.Trim()) }));
            if (check == 0)
            {
                HienThongBao("Khóa học này chưa có trong danh sách. Hãy bấm Thêm!", true);
                return;
            }

            if (!KiemTraDuLieu(true)) return;

            string hocPhiStr = txtHocPhi.Text.Replace(",", "").Replace(".", "").Trim();
            decimal hocPhiVal = 0;
            decimal.TryParse(hocPhiStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out hocPhiVal);

            string sql = "UPDATE KHOAHOC SET tenkhoahoc=@ten, hocphi=@hp, thoigianhoc=@tg, mota=@mt, trangthaikhoahoc=@tt WHERE makhoahoc=@ma";
            SqlParameter[] p = {
                new SqlParameter("@ma", txtMaKH.Text.Trim()),
                new SqlParameter("@ten", txtTenKH.Text.Trim()),
                new SqlParameter("@hp", hocPhiVal),
                new SqlParameter("@tg", txtThoiGian.Text.Trim()),
                new SqlParameter("@mt", txtMoTa.Text.Trim()),
                new SqlParameter("@tt", ddlTrangThai.SelectedValue)
            };
            kncsdl.ThucThiLenh(sql, p);
            HienThongBao("Cập nhật khóa học thành công!");
            TaiDuLieu();
            LamMoiForm();
        }

        protected void btnLamMoi_Click(object sender, EventArgs e)
        {
            LamMoiForm();
            TuKhoaHienTai = "";
            txtTim.Text = "";
            gvKhoaHoc.PageIndex = 0;
            TaiDuLieu();
            pnlTB.Visible = false;
        }

        protected void gvKhoaHoc_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Chon")
            {
                string ma = e.CommandArgument.ToString();
                DataTable dt = kncsdl.DocDuLieu("SELECT * FROM KHOAHOC WHERE makhoahoc=@ma", new[] { new SqlParameter("@ma", ma) });
                if (dt.Rows.Count > 0)
                {
                    DataRow r = dt.Rows[0];
                    txtMaKH.Text = r["makhoahoc"].ToString();
                    txtTenKH.Text = r["tenkhoahoc"].ToString();
                    txtHocPhi.Text = r["hocphi"].ToString();
                    txtThoiGian.Text = r["thoigianhoc"].ToString();
                    txtMoTa.Text = r["mota"].ToString();
                    ddlTrangThai.SelectedValue = r["trangthaikhoahoc"].ToString();
                    pnlTB.Visible = false;
                }
            }
        }

        protected void btnXoaChinhThuc_Click(object sender, EventArgs e)
        {
            string ma = hfMaKHXoa.Value;
            if (!string.IsNullOrEmpty(ma))
            {
                try
                {
                    kncsdl.ThucThiLenh("DELETE FROM KHOAHOC WHERE makhoahoc=@ma", new[] { new SqlParameter("@ma", ma) });
                    HienThongBao("Xóa khóa học thành công!");

                    // Nếu xóa hết dòng ở trang hiện tại thì lùi về trang trước
                    if (gvKhoaHoc.Rows.Count == 1 && gvKhoaHoc.PageIndex > 0)
                    {
                        gvKhoaHoc.PageIndex--;
                    }
                    TaiDuLieu();
                    LamMoiForm();
                }
                catch
                {
                    HienThongBao("Không thể xóa khóa học đã có lớp hoặc học viên đăng ký!", true);
                }
            }
        }

        protected void btnTim_Click(object sender, EventArgs e)
        {
            TuKhoaHienTai = txtTim.Text.Trim();
            gvKhoaHoc.PageIndex = 0;
            TaiDuLieu();
            pnlTB.Visible = false;
        }

        protected void gvKhoaHoc_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvKhoaHoc.PageIndex = e.NewPageIndex;
            TaiDuLieu();
            pnlTB.Visible = false;
        }
    }
}