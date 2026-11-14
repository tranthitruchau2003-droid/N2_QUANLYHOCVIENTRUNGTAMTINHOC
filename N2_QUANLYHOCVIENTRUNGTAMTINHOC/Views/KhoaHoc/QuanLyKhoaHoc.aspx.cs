using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
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
            if (IsPostBack)
            {
                pnlTB.Visible = false;
            }
            else
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
            string sql = @"SELECT * FROM KHOAHOC 
                           WHERE (@kw = '' 
                               OR tenkhoahoc COLLATE Vietnamese_CI_AI LIKE @kw COLLATE Vietnamese_CI_AI 
                               OR makhoahoc COLLATE Vietnamese_CI_AI LIKE @kw COLLATE Vietnamese_CI_AI) 
                           ORDER BY makhoahoc DESC";
            gvKhoaHoc.DataSource = kncsdl.DocDuLieu(sql, new[] { new SqlParameter("@kw", string.IsNullOrEmpty(kw) ? "" : "%" + kw + "%") });
            gvKhoaHoc.DataBind();

            TaiGoiYTimKiem();
        }

        private void TaiGoiYTimKiem()
        {
            DataTable dt = kncsdl.DocDuLieu("SELECT makhoahoc, tenkhoahoc FROM KHOAHOC ORDER BY makhoahoc DESC");
            StringBuilder sbTim = new StringBuilder();
            StringBuilder sbTen = new StringBuilder();
            foreach (DataRow r in dt.Rows)
            {
                sbTim.AppendFormat("<button type=\"button\" class=\"list-group-item list-group-item-action py-2 px-2 small goi-y-item\" data-value=\"{0}\"><strong>{0}</strong> - {1}</button>",
                    HttpUtility.HtmlAttributeEncode(Convert.ToString(r["makhoahoc"])), HttpUtility.HtmlEncode(Convert.ToString(r["tenkhoahoc"])));
                sbTen.AppendFormat("<button type=\"button\" class=\"list-group-item list-group-item-action py-2 px-2 small goi-y-item\" data-value=\"{0}\">{0}</button>",
                    HttpUtility.HtmlEncode(Convert.ToString(r["tenkhoahoc"])));
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
            litTB.Text = HttpUtility.HtmlEncode(msg);
        }

        private bool KiemTraDuLieu(out decimal hocPhiVal, bool laCapNhat = false)
        {
            hocPhiVal = 0;

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

            string hocPhiStr = txtHocPhi.Text.Replace(",", "").Trim();
            if (string.IsNullOrEmpty(hocPhiStr) || !decimal.TryParse(hocPhiStr, NumberStyles.Any, CultureInfo.InvariantCulture, out hocPhiVal) || hocPhiVal <= 0)
            {
                HienThongBao("Học phí không hợp lệ! Vui lòng nhập số tiền lớn hơn 0 (VD: 1500000).", true);
                txtHocPhi.Focus();
                return false;
            }

            string rawThoiGian = txtThoiGian.Text.Replace(",", ".").Trim();
            decimal soThang;
            if (!decimal.TryParse(rawThoiGian, NumberStyles.Any, CultureInfo.InvariantCulture, out soThang) || soThang <= 0)
            {
                HienThongBao("Thời gian học không hợp lệ! Vui lòng chỉ nhập số tháng (VD: 3 hoặc 2.5)", true);
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
            decimal hocPhiVal;
            if (!KiemTraDuLieu(out hocPhiVal, false)) return;

            SinhMaKHMoi();
            string rawThoiGian = txtThoiGian.Text.Replace(",", ".").Trim();
            string thoiGianHoc = rawThoiGian + " tháng";

            string sql = "INSERT INTO KHOAHOC(makhoahoc, tenkhoahoc, hocphi, thoigianhoc, mota, trangthaikhoahoc) VALUES(@ma, @ten, @hp, @tg, @mt, @tt)";
            SqlParameter[] p = {
                new SqlParameter("@ma", txtMaKH.Text.Trim()),
                new SqlParameter("@ten", txtTenKH.Text.Trim()),
                new SqlParameter("@hp", hocPhiVal),
                new SqlParameter("@tg", thoiGianHoc),
                new SqlParameter("@mt", txtMoTa.Text.Trim()),
                new SqlParameter("@tt", ddlTrangThai.SelectedValue)
            };
            kncsdl.ThucThiLenh(sql, p);
            kncsdl.GhiNhatKy(Session["TenDangNhap"]?.ToString(), "Thêm khóa học", $"Mã KH: {txtMaKH.Text.Trim()} - {txtTenKH.Text.Trim()} ({hocPhiVal:N0}đ)");
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

            decimal hocPhiVal;
            if (!KiemTraDuLieu(out hocPhiVal, true)) return;

            string rawThoiGian = txtThoiGian.Text.Replace(",", ".").Trim();
            string thoiGianHoc = rawThoiGian + " tháng";

            string sql = "UPDATE KHOAHOC SET tenkhoahoc=@ten, hocphi=@hp, thoigianhoc=@tg, mota=@mt, trangthaikhoahoc=@tt WHERE makhoahoc=@ma";
            SqlParameter[] p = {
                new SqlParameter("@ma", txtMaKH.Text.Trim()),
                new SqlParameter("@ten", txtTenKH.Text.Trim()),
                new SqlParameter("@hp", hocPhiVal),
                new SqlParameter("@tg", thoiGianHoc),
                new SqlParameter("@mt", txtMoTa.Text.Trim()),
                new SqlParameter("@tt", ddlTrangThai.SelectedValue)
            };
            kncsdl.ThucThiLenh(sql, p);
            kncsdl.GhiNhatKy(Session["TenDangNhap"]?.ToString(), "Sửa khóa học", $"Mã KH: {txtMaKH.Text.Trim()} - {txtTenKH.Text.Trim()} ({hocPhiVal:N0}đ)");
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
                    txtHocPhi.Text = r["hocphi"] != DBNull.Value
                        ? Convert.ToDecimal(r["hocphi"]).ToString("#,##0", CultureInfo.InvariantCulture)
                        : "0";

                    string tgDb = r["thoigianhoc"].ToString();
                    var m = Regex.Match(tgDb, @"\d+(?:[\.,]\d+)?");
                    txtThoiGian.Text = m.Success ? m.Value.Replace(",", ".") : "";

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
                    kncsdl.GhiNhatKy(Session["TenDangNhap"]?.ToString(), "Xóa khóa học", $"Mã KH: {ma}");
                    HienThongBao("Xóa khóa học thành công!");

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