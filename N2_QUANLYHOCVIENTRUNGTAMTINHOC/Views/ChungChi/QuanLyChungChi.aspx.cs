using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI.WebControls;
using N2_QUANLYHOCVIENTRUNGTAMTINHOC.DAL;

namespace N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.ChungChi
{
    public partial class QuanLyChungChi : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["Quyen"] != null && Session["Quyen"].ToString() == "HocVien")
            {
                // Ẩn hoàn toàn khung bên trái và khung tìm kiếm khi là Học viên
                divFormCapCC.Visible = false;
                pnlTimKiemCC.Visible = false;
                divDanhSachCC.Attributes["class"] = "col-12";
                litTieuDeCC.Text = "Chứng chỉ cá nhân của tôi";
            }

            if (!IsPostBack)
            {
                txtNgayCap.Text = DateTime.Now.ToString("yyyy-MM-dd");
                TaiHocVienDat();
                TaiDanhSach();
            }
        }

        private void TaiHocVienDat()
        {
            string sql = @"SELECT tg.matg, 
                                  hv.hoten + N' - ' + kh.tenkhoahoc + N' (' + kt.nienkhoa + N' - Điểm: ' + CAST(tg.diem AS NVARCHAR(10)) + N')' AS HienThi
                           FROM THAM_GIA tg
                           JOIN HOCVIEN hv ON tg.mahocvien = hv.mahocvien
                           JOIN KYTHI kt ON tg.makythi = kt.makythi
                           JOIN KHOAHOC kh ON kt.makhoahoc = kh.makhoahoc
                           WHERE tg.ketqua = N'Đạt'
                           ORDER BY hv.hoten";
            ddlThamGia.DataSource = kncsdl.DocDuLieu(sql);
            ddlThamGia.DataTextField = "HienThi";
            ddlThamGia.DataValueField = "matg";
            ddlThamGia.DataBind();
        }

        private void TaiDanhSach(string tuKhoa = "", string xepLoai = "")
        {
            bool isHV = Session["Quyen"] != null && Session["Quyen"].ToString() == "HocVien";
            string user = Session["TenDangNhap"]?.ToString() ?? "";

            string sql = @"SELECT cc.*, hv.hoten, tg.diem, kh.tenkhoahoc, kt.nienkhoa
                           FROM CHUNGCHI cc
                           JOIN THAM_GIA tg ON cc.matg = tg.matg
                           JOIN HOCVIEN hv ON tg.mahocvien = hv.mahocvien
                           JOIN KYTHI kt ON tg.makythi = kt.makythi
                           JOIN KHOAHOC kh ON kt.makhoahoc = kh.makhoahoc
                           WHERE (@kw = '' OR cc.sohieuchungchi LIKE @kw OR hv.hoten LIKE @kw OR kh.tenkhoahoc LIKE @kw OR cc.machungchi LIKE @kw)
                             AND (@xl = '' OR cc.xeploai = @xl)
                             AND (@isHV = 0 OR tg.mahocvien = @user)
                           ORDER BY cc.ngaycap DESC, cc.machungchi DESC";
            SqlParameter[] p = {
                new SqlParameter("@kw", string.IsNullOrEmpty(tuKhoa) ? "" : "%" + tuKhoa + "%"),
                new SqlParameter("@xl", xepLoai ?? ""),
                new SqlParameter("@isHV", isHV ? 1 : 0),
                new SqlParameter("@user", user)
            };
            DataTable dt = kncsdl.DocDuLieu(sql, p);
            gvChungChi.DataSource = dt;
            gvChungChi.DataBind();

            if (!isHV) TaiGoiYTimKiem();
        }

        private void TaiGoiYTimKiem()
        {
            string sql = @"SELECT cc.sohieuchungchi, hv.hoten, kh.tenkhoahoc
                           FROM CHUNGCHI cc
                           JOIN THAM_GIA tg ON cc.matg = tg.matg
                           JOIN HOCVIEN hv ON tg.mahocvien = hv.mahocvien
                           JOIN KYTHI kt ON tg.makythi = kt.makythi
                           JOIN KHOAHOC kh ON kt.makhoahoc = kh.makhoahoc";
            DataTable dt = kncsdl.DocDuLieu(sql);
            StringBuilder sb = new StringBuilder();
            foreach (DataRow r in dt.Rows)
            {
                sb.AppendFormat("<button type=\"button\" class=\"list-group-item list-group-item-action py-2 px-2 small goi-y-item\" data-value=\"{0}\"><strong>{0}</strong> - {1} ({2})</button>",
                    r["sohieuchungchi"], r["hoten"], r["tenkhoahoc"]);
            }
            litGoiYCC.Text = sb.ToString();
        }

        private void HienThongBao(string msg, bool laLoi = false)
        {
            pnlTB.Visible = true;
            pnlTB.CssClass = laLoi
                ? "alert alert-danger text-danger fw-semibold alert-dismissible fade show shadow"
                : "alert alert-success text-success fw-semibold alert-dismissible fade show shadow";
            litTB.Text = msg;
        }

        private void LamMoiForm()
        {
            txtMaCC.Text = txtSoHieu.Text = "";
            txtMaCC.ReadOnly = false;
            txtNgayCap.Text = DateTime.Now.ToString("yyyy-MM-dd");
            ddlXepLoai.SelectedIndex = 0;
        }

        protected void btnCapCC_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaCC.Text) || string.IsNullOrWhiteSpace(txtSoHieu.Text) || string.IsNullOrEmpty(ddlThamGia.SelectedValue))
            {
                HienThongBao("Vui lòng nhập đầy đủ Mã chứng chỉ, Số hiệu và chọn Học viên!", true);
                return;
            }

            try
            {
                int checkMa = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM CHUNGCHI WHERE machungchi=@ma", new[] { new SqlParameter("@ma", txtMaCC.Text.Trim()) }));
                if (checkMa > 0)
                {
                    HienThongBao("Mã chứng chỉ này đã tồn tại!", true);
                    return;
                }

                int checkTG = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM CHUNGCHI WHERE matg=@tg", new[] { new SqlParameter("@tg", ddlThamGia.SelectedValue) }));
                if (checkTG > 0)
                {
                    HienThongBao("Học viên ở kỳ thi này đã được cấp chứng chỉ rồi!", true);
                    return;
                }

                string sql = "INSERT INTO CHUNGCHI(machungchi, ngaycap, sohieuchungchi, xeploai, matg) VALUES(@ma, @nc, @sh, @xl, @tg)";
                SqlParameter[] p = {
                    new SqlParameter("@ma", txtMaCC.Text.Trim()),
                    new SqlParameter("@nc", string.IsNullOrEmpty(txtNgayCap.Text) ? DateTime.Now.ToString("yyyy-MM-dd") : txtNgayCap.Text),
                    new SqlParameter("@sh", txtSoHieu.Text.Trim()),
                    new SqlParameter("@xl", ddlXepLoai.SelectedValue),
                    new SqlParameter("@tg", ddlThamGia.SelectedValue)
                };
                kncsdl.ThucThiLenh(sql, p);
                HienThongBao("Cấp chứng chỉ thành công!");
                TaiDanhSach(txtTimCC.Text.Trim(), ddlLocXepLoai.SelectedValue);
                LamMoiForm();
            }
            catch (Exception ex) { HienThongBao("Lỗi cấp chứng chỉ: " + ex.Message, true); }
        }

        protected void btnCapNhatCC_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaCC.Text) || string.IsNullOrWhiteSpace(txtSoHieu.Text))
            {
                HienThongBao("Vui lòng chọn chứng chỉ cần sửa!", true);
                return;
            }

            try
            {
                int check = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM CHUNGCHI WHERE machungchi=@ma", new[] { new SqlParameter("@ma", txtMaCC.Text.Trim()) }));
                if (check == 0)
                {
                    HienThongBao("Chứng chỉ này chưa có trong danh sách. Hãy bấm Cấp Chứng chỉ!", true);
                    return;
                }

                string sql = "UPDATE CHUNGCHI SET ngaycap=@nc, sohieuchungchi=@sh, xeploai=@xl, matg=@tg WHERE machungchi=@ma";
                SqlParameter[] p = {
                    new SqlParameter("@ma", txtMaCC.Text.Trim()),
                    new SqlParameter("@nc", string.IsNullOrEmpty(txtNgayCap.Text) ? DateTime.Now.ToString("yyyy-MM-dd") : txtNgayCap.Text),
                    new SqlParameter("@sh", txtSoHieu.Text.Trim()),
                    new SqlParameter("@xl", ddlXepLoai.SelectedValue),
                    new SqlParameter("@tg", ddlThamGia.SelectedValue)
                };
                kncsdl.ThucThiLenh(sql, p);
                HienThongBao("Cập nhật chứng chỉ thành công!");
                TaiDanhSach(txtTimCC.Text.Trim(), ddlLocXepLoai.SelectedValue);
                LamMoiForm();
            }
            catch (Exception ex) { HienThongBao("Lỗi cập nhật: " + ex.Message, true); }
        }

        protected void btnLamMoiCC_Click(object sender, EventArgs e)
        {
            LamMoiForm();
            txtTimCC.Text = "";
            ddlLocXepLoai.SelectedIndex = 0;
            TaiDanhSach();
            pnlTB.Visible = false;
        }

        protected void gvChungChi_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ChonCC")
            {
                string ma = e.CommandArgument.ToString();
                DataTable dt = kncsdl.DocDuLieu("SELECT * FROM CHUNGCHI WHERE machungchi=@ma", new[] { new SqlParameter("@ma", ma) });
                if (dt.Rows.Count > 0)
                {
                    DataRow r = dt.Rows[0];
                    txtMaCC.Text = r["machungchi"].ToString();
                    txtMaCC.ReadOnly = true;
                    txtSoHieu.Text = r["sohieuchungchi"].ToString();
                    ddlXepLoai.SelectedValue = r["xeploai"].ToString();
                    ddlThamGia.SelectedValue = r["matg"].ToString();
                    if (r["ngaycap"] != DBNull.Value)
                        txtNgayCap.Text = Convert.ToDateTime(r["ngaycap"]).ToString("yyyy-MM-dd");
                    pnlTB.Visible = false;
                }
            }
        }

        protected void btnXoaCCChinhThuc_Click(object sender, EventArgs e)
        {
            string ma = hfMaCCXoa.Value;
            if (!string.IsNullOrEmpty(ma))
            {
                try
                {
                    kncsdl.ThucThiLenh("DELETE FROM CHUNGCHI WHERE machungchi=@ma", new[] { new SqlParameter("@ma", ma) });
                    HienThongBao("Đã xóa chứng chỉ thành công.");
                    TaiDanhSach(txtTimCC.Text.Trim(), ddlLocXepLoai.SelectedValue);
                    LamMoiForm();
                }
                catch (Exception ex) { HienThongBao("Lỗi khi xóa: " + ex.Message, true); }
            }
        }

        protected void btnTimCC_Click(object sender, EventArgs e)
        {
            TaiDanhSach(txtTimCC.Text.Trim(), ddlLocXepLoai.SelectedValue);
            pnlTB.Visible = false;
        }
    }
}