using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI.WebControls;
using N2_QUANLYHOCVIENTRUNGTAMTINHOC.DAL;
using System.Text;

namespace N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.KyThi
{
    public partial class QuanLyKyThi : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ddlKhoaHoc.DataSource = kncsdl.DocDuLieu("SELECT makhoahoc, tenkhoahoc FROM KHOAHOC");
                ddlKhoaHoc.DataTextField = "tenkhoahoc"; ddlKhoaHoc.DataValueField = "makhoahoc"; ddlKhoaHoc.DataBind();

                ddlHocVien.DataSource = kncsdl.DocDuLieu("SELECT mahocvien, hoten FROM HOCVIEN");
                ddlHocVien.DataTextField = "hoten"; ddlHocVien.DataValueField = "mahocvien"; ddlHocVien.DataBind();

                TaiDuLieu();
            }
        }

        private void TaiDuLieu()
        {
            TaiDanhSachKyThi(txtTimKT.Text.Trim());

            ddlKyThi.DataSource = kncsdl.DocDuLieu("SELECT makythi, makythi + ' (' + nienkhoa + ')' AS HienThi FROM KYTHI");
            ddlKyThi.DataTextField = "HienThi"; ddlKyThi.DataValueField = "makythi"; ddlKyThi.DataBind();

            TaiDanhSachThamGia(txtTimTG.Text.Trim(), ddlLocKetQua.SelectedValue);

            TaiGoiYTimKiem(); 
        }

        private void TaiGoiYTimKiem()
        {
            // 1. Gợi ý cho Kỳ thi
            DataTable dtKT = kncsdl.DocDuLieu("SELECT kt.makythi, kt.nienkhoa, kh.tenkhoahoc FROM KYTHI kt JOIN KHOAHOC kh ON kt.makhoahoc = kh.makhoahoc");
            StringBuilder sbKT = new StringBuilder();
            foreach (DataRow r in dtKT.Rows)
            {
                sbKT.AppendFormat("<button type=\"button\" class=\"list-group-item list-group-item-action py-2 px-2 small goi-y-item\" data-value=\"{0}\"><strong>{0}</strong> - {1} ({2})</button>",
                    r["makythi"], r["tenkhoahoc"], r["nienkhoa"]);
            }
            litGoiYKT.Text = sbKT.ToString();

            // 2. Gợi ý cho Tham gia
            DataTable dtTG = kncsdl.DocDuLieu("SELECT tg.matg, tg.makythi, hv.hoten FROM THAM_GIA tg JOIN HOCVIEN hv ON tg.mahocvien = hv.mahocvien");
            StringBuilder sbTG = new StringBuilder();
            foreach (DataRow r in dtTG.Rows)
            {
                sbTG.AppendFormat("<button type=\"button\" class=\"list-group-item list-group-item-action py-2 px-2 small goi-y-item\" data-value=\"{1}\"><strong>{0}</strong> - {1} ({2})</button>",
                    r["matg"], r["hoten"], r["makythi"]);
            }
            litGoiYTG.Text = sbTG.ToString();
        }

        private void TaiDanhSachKyThi(string tuKhoa = "")
        {
            string sql = @"SELECT kt.*, kh.tenkhoahoc 
                           FROM KYTHI kt 
                           JOIN KHOAHOC kh ON kt.makhoahoc = kh.makhoahoc
                           WHERE (@kw = '' OR kt.makythi LIKE @kw OR kh.tenkhoahoc LIKE @kw OR kt.nienkhoa LIKE @kw)
                           ORDER BY kt.makythi DESC";
            SqlParameter[] p = { new SqlParameter("@kw", string.IsNullOrEmpty(tuKhoa) ? "" : "%" + tuKhoa + "%") };
            gvKyThi.DataSource = kncsdl.DocDuLieu(sql, p);
            gvKyThi.DataBind();
        }

        private void TaiDanhSachThamGia(string tuKhoa = "", string ketQua = "")
        {
            string sql = @"SELECT tg.*, hv.hoten 
                           FROM THAM_GIA tg 
                           JOIN HOCVIEN hv ON tg.mahocvien = hv.mahocvien
                           WHERE (@kw = '' OR tg.matg LIKE @kw OR hv.hoten LIKE @kw OR tg.makythi LIKE @kw)
                             AND (@kq = '' OR tg.ketqua = @kq)
                           ORDER BY tg.matg DESC";
            SqlParameter[] p = {
                new SqlParameter("@kw", string.IsNullOrEmpty(tuKhoa) ? "" : "%" + tuKhoa + "%"),
                new SqlParameter("@kq", ketQua ?? "")
            };
            gvThamGia.DataSource = kncsdl.DocDuLieu(sql, p);
            gvThamGia.DataBind();
        }

        private void HienThongBao(string msg, bool laLoi = false)
        {
            pnlTB.Visible = true;
            pnlTB.CssClass = laLoi
                ? "alert alert-danger text-danger fw-semibold alert-dismissible fade show shadow"
                : "alert alert-success text-success fw-semibold alert-dismissible fade show shadow";
            litTB.Text = msg;
        }

        private void LamMoiFormKT()
        {
            txtMaKT.Text = txtNienKhoa.Text = txtNgayThi.Text = "";
            txtMaKT.ReadOnly = false;
        }

        protected void btnTaoKyThi_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaKT.Text) || string.IsNullOrWhiteSpace(txtNgayThi.Text))
            {
                HienThongBao("Vui lòng nhập đầy đủ Mã kỳ thi và Ngày thi!", true);
                return;
            }

            try
            {
                int check = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM KYTHI WHERE makythi=@ma", new[] { new SqlParameter("@ma", txtMaKT.Text.Trim()) }));
                if (check > 0)
                {
                    HienThongBao("Mã kỳ thi này đã tồn tại, vui lòng nhập mã khác!", true);
                    return;
                }

                string sql = "INSERT INTO KYTHI(makythi, nienkhoa, ngaythi, makhoahoc) VALUES(@ma, @nk, @ngay, @kh)";
                SqlParameter[] p = {
                    new SqlParameter("@ma", txtMaKT.Text.Trim()),
                    new SqlParameter("@nk", txtNienKhoa.Text.Trim()),
                    new SqlParameter("@ngay", txtNgayThi.Text),
                    new SqlParameter("@kh", ddlKhoaHoc.SelectedValue)
                };
                kncsdl.ThucThiLenh(sql, p);
                HienThongBao("Đã tạo kỳ thi mới thành công!");
                TaiDuLieu();
                LamMoiFormKT();
            }
            catch (Exception ex) { HienThongBao("Lỗi khi tạo kỳ thi: " + ex.Message, true); }
        }

        protected void btnCapNhatKT_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaKT.Text) || string.IsNullOrWhiteSpace(txtNgayThi.Text))
            {
                HienThongBao("Vui lòng chọn kỳ thi cần sửa và nhập đầy đủ Ngày thi!", true);
                return;
            }

            try
            {
                int check = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM KYTHI WHERE makythi=@ma", new[] { new SqlParameter("@ma", txtMaKT.Text.Trim()) }));
                if (check == 0)
                {
                    HienThongBao("Kỳ thi này chưa có trong danh sách. Hãy bấm 'Tạo Kỳ thi'!", true);
                    return;
                }

                string sql = "UPDATE KYTHI SET nienkhoa=@nk, ngaythi=@ngay, makhoahoc=@kh WHERE makythi=@ma";
                SqlParameter[] p = {
                    new SqlParameter("@ma", txtMaKT.Text.Trim()),
                    new SqlParameter("@nk", txtNienKhoa.Text.Trim()),
                    new SqlParameter("@ngay", txtNgayThi.Text),
                    new SqlParameter("@kh", ddlKhoaHoc.SelectedValue)
                };
                kncsdl.ThucThiLenh(sql, p);
                HienThongBao("Cập nhật kỳ thi thành công!");
                TaiDuLieu();
                LamMoiFormKT();
            }
            catch (Exception ex) { HienThongBao("Lỗi cập nhật: " + ex.Message, true); }
        }

        protected void btnLamMoiKT_Click(object sender, EventArgs e)
        {
            LamMoiFormKT();
            txtTimKT.Text = "";
            TaiDanhSachKyThi();
            pnlTB.Visible = false;
        }

        protected void gvKyThi_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ChonKT")
            {
                string ma = e.CommandArgument.ToString();
                DataTable dt = kncsdl.DocDuLieu("SELECT * FROM KYTHI WHERE makythi=@ma", new[] { new SqlParameter("@ma", ma) });
                if (dt.Rows.Count > 0)
                {
                    DataRow r = dt.Rows[0];
                    txtMaKT.Text = r["makythi"].ToString();
                    txtMaKT.ReadOnly = true;
                    ddlKhoaHoc.SelectedValue = r["makhoahoc"].ToString();
                    txtNienKhoa.Text = r["nienkhoa"].ToString();
                    if (r["ngaythi"] != DBNull.Value)
                        txtNgayThi.Text = Convert.ToDateTime(r["ngaythi"]).ToString("yyyy-MM-dd");
                    pnlTB.Visible = false;
                }
            }
        }

        protected void btnXoaKTChinhThuc_Click(object sender, EventArgs e)
        {
            string ma = hfMaKTXoa.Value;
            if (!string.IsNullOrEmpty(ma))
            {
                try
                {
                    int countTG = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM THAM_GIA WHERE makythi=@ma", new[] { new SqlParameter("@ma", ma) }));
                    if (countTG > 0)
                    {
                        HienThongBao("Không thể xóa kỳ thi này vì đã có học viên tham gia thi!", true);
                        return;
                    }
                    kncsdl.ThucThiLenh("DELETE FROM KYTHI WHERE makythi=@ma", new[] { new SqlParameter("@ma", ma) });
                    HienThongBao("Đã xóa kỳ thi thành công.");
                    TaiDuLieu();
                    LamMoiFormKT();
                }
                catch (Exception ex) { HienThongBao("Lỗi khi xóa: " + ex.Message, true); }
            }
        }

        protected void btnTimKT_Click(object sender, EventArgs e)
        {
            TaiDanhSachKyThi(txtTimKT.Text.Trim());
            pnlTB.Visible = false;
        }

        protected void btnTimTG_Click(object sender, EventArgs e)
        {
            TaiDanhSachThamGia(txtTimTG.Text.Trim(), ddlLocKetQua.SelectedValue);
            pnlTB.Visible = false;
        }

        private void LamMoiFormTG()
        {
            txtMaTG.Text = txtDiem.Text = "";
            txtMaTG.ReadOnly = false;
        }

        protected void btnThemTG_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaTG.Text) || string.IsNullOrWhiteSpace(txtDiem.Text))
            {
                HienThongBao("Vui lòng nhập đầy đủ Mã TG và Điểm số!", true);
                return;
            }

            try
            {
                // Kiểm tra trùng Mã TG
                int check = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM THAM_GIA WHERE matg=@ma", new[] { new SqlParameter("@ma", txtMaTG.Text.Trim()) }));
                if (check > 0)
                {
                    HienThongBao("Mã tham gia (Mã TG) này đã tồn tại, vui lòng nhập mã khác!", true);
                    return;
                }

                double diem = double.Parse(txtDiem.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                if (diem < 0 || diem > 10)
                {
                    HienThongBao("Điểm số phải nằm trong khoảng từ 0 đến 10!", true);
                    return;
                }
                string ketQua = diem >= 5.0 ? "Đạt" : "Không đạt";

                string sql = "INSERT INTO THAM_GIA(matg, mahocvien, makythi, diem, ketqua) VALUES(@ma, @hv, @kt, @diem, @kq)";
                SqlParameter[] p = {
                    new SqlParameter("@ma", txtMaTG.Text.Trim()),
                    new SqlParameter("@hv", ddlHocVien.SelectedValue),
                    new SqlParameter("@kt", ddlKyThi.SelectedValue),
                    new SqlParameter("@diem", diem),
                    new SqlParameter("@kq", ketQua)
                };
                kncsdl.ThucThiLenh(sql, p);
                HienThongBao($"Thêm điểm thi thành công ({diem} - {ketQua})!");
                TaiDuLieu();
                LamMoiFormTG();
            }
            catch (Exception ex) { HienThongBao("Lỗi khi thêm điểm: " + ex.Message, true); }
        }

        protected void btnCapNhatTG_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaTG.Text) || string.IsNullOrWhiteSpace(txtDiem.Text))
            {
                HienThongBao("Vui lòng chọn bản ghi cần sửa và nhập Điểm số!", true);
                return;
            }

            try
            {
                int check = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM THAM_GIA WHERE matg=@ma", new[] { new SqlParameter("@ma", txtMaTG.Text.Trim()) }));
                if (check == 0)
                {
                    HienThongBao("Mã tham gia này chưa có trong danh sách. Hãy bấm Thêm mới!", true);
                    return;
                }

                double diem = double.Parse(txtDiem.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                if (diem < 0 || diem > 10)
                {
                    HienThongBao("Điểm số phải nằm trong khoảng từ 0 đến 10!", true);
                    return;
                }
                string ketQua = diem >= 5.0 ? "Đạt" : "Không đạt";

                string sql = "UPDATE THAM_GIA SET mahocvien=@hv, makythi=@kt, diem=@diem, ketqua=@kq WHERE matg=@ma";
                SqlParameter[] p = {
                    new SqlParameter("@ma", txtMaTG.Text.Trim()),
                    new SqlParameter("@hv", ddlHocVien.SelectedValue),
                    new SqlParameter("@kt", ddlKyThi.SelectedValue),
                    new SqlParameter("@diem", diem),
                    new SqlParameter("@kq", ketQua)
                };
                kncsdl.ThucThiLenh(sql, p);
                HienThongBao($"Cập nhật điểm thi thành công ({diem} - {ketQua})!");
                TaiDuLieu();
                LamMoiFormTG();
            }
            catch (Exception ex) { HienThongBao("Lỗi cập nhật điểm: " + ex.Message, true); }
        }

        protected void btnLamMoiTG_Click(object sender, EventArgs e)
        {
            LamMoiFormTG();
            txtTimTG.Text = "";
            ddlLocKetQua.SelectedIndex = 0;
            TaiDanhSachThamGia();
            pnlTB.Visible = false;
        }

        protected void gvThamGia_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ChonTG")
            {
                string ma = e.CommandArgument.ToString();
                DataTable dt = kncsdl.DocDuLieu("SELECT * FROM THAM_GIA WHERE matg=@ma", new[] { new SqlParameter("@ma", ma) });
                if (dt.Rows.Count > 0)
                {
                    DataRow r = dt.Rows[0];
                    txtMaTG.Text = r["matg"].ToString();
                    txtMaTG.ReadOnly = true;
                    ddlHocVien.SelectedValue = r["mahocvien"].ToString();
                    ddlKyThi.SelectedValue = r["makythi"].ToString();
                    txtDiem.Text = r["diem"].ToString();
                    pnlTB.Visible = false;
                }
            }
        }

        protected void btnXoaTGChinhThuc_Click(object sender, EventArgs e)
        {
            string ma = hfMaTGXoa.Value;
            if (!string.IsNullOrEmpty(ma))
            {
                try
                {
                    kncsdl.ThucThiLenh("DELETE FROM THAM_GIA WHERE matg=@ma", new[] { new SqlParameter("@ma", ma) });
                    HienThongBao("Đã xóa bản ghi điểm thi thành công.");
                    TaiDuLieu();
                    LamMoiFormTG();
                }
                catch (Exception ex) { HienThongBao("Lỗi khi xóa: " + ex.Message, true); }
            }
        }
    }
}