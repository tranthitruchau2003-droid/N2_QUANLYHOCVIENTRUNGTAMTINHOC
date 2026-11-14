using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using N2_QUANLYHOCVIENTRUNGTAMTINHOC.DAL;

namespace N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.ChungChi
{
    public partial class QuanLyChungChi : System.Web.UI.Page
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
            get { return ViewState["TuKhoaTimCC"]?.ToString() ?? ""; }
            set { ViewState["TuKhoaTimCC"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (MasterPage.ChuanHoaQuyen(Session["Quyen"]) == "HocVien")
            {
                divFormCapCC.Visible = false;
                pnlTimKiemCC.Visible = false;
                divDanhSachCC.Attributes["class"] = "col-12";
                litTieuDeCC.Text = "Chứng chỉ cá nhân của tôi";
            }

            if (!IsPostBack)
            {
                TuKhoaHienTai = "";
                txtTimCC.Text = "";
                txtNgayCap.Text = DateTime.Now.ToString("yyyy-MM-dd");
                TaiBoLocForm();
                TaiHocVienDat();
                SinhMaVaSoHieuMoi();
                CapNhatXepLoaiTuDong();
                TaiDanhSach();
            }
        }

        private void TaiBoLocForm()
        {
            ddlLocKhoaHoc.DataSource = kncsdl.DocDuLieu("SELECT makhoahoc, tenkhoahoc FROM KHOAHOC ORDER BY tenkhoahoc");
            ddlLocKhoaHoc.DataTextField = "tenkhoahoc";
            ddlLocKhoaHoc.DataValueField = "makhoahoc";
            ddlLocKhoaHoc.DataBind();
            ddlLocKhoaHoc.Items.Insert(0, new ListItem("-- Tất cả khóa --", ""));

            ddlLocNam.DataSource = kncsdl.DocDuLieu("SELECT DISTINCT RIGHT(LTRIM(RTRIM(nienkhoa)), 4) AS Nam FROM KYTHI WHERE ISNUMERIC(RIGHT(LTRIM(RTRIM(nienkhoa)), 4)) = 1 ORDER BY Nam DESC");
            ddlLocNam.DataTextField = "Nam";
            ddlLocNam.DataValueField = "Nam";
            ddlLocNam.DataBind();
            ddlLocNam.Items.Insert(0, new ListItem("-- Tất cả --", ""));
        }

        private void SinhMaVaSoHieuMoi()
        {
            int soLuong = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM CHUNGCHI")) + 1;
            string maCC = "CC" + soLuong.ToString("D2");
            while (Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM CHUNGCHI WHERE machungchi=@ma", new[] { new SqlParameter("@ma", maCC) })) > 0)
            {
                soLuong++;
                maCC = "CC" + soLuong.ToString("D2");
            }
            txtMaCC.Text = maCC;

            int nam = DateTime.Now.Year;
            int sttSH = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM CHUNGCHI")) + 1;
            string soHieu = $"TTTH-{nam}-{sttSH:D4}";
            while (Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM CHUNGCHI WHERE sohieuchungchi=@sh", new[] { new SqlParameter("@sh", soHieu) })) > 0)
            {
                sttSH++;
                soHieu = $"TTTH-{nam}-{sttSH:D4}";
            }
            txtSoHieu.Text = soHieu;
        }

        private void CapNhatXepLoaiTuDong()
        {
            if (string.IsNullOrEmpty(ddlThamGia.SelectedValue)) return;
            object val = kncsdl.LayGiaTriDon("SELECT diem FROM THAM_GIA WHERE matg=@tg", new[] { new SqlParameter("@tg", ddlThamGia.SelectedValue) });
            if (val != null && val != DBNull.Value)
            {
                decimal diem = Convert.ToDecimal(val);
                if (diem >= 9.0m) ddlXepLoai.SelectedValue = "Xuất sắc";
                else if (diem >= 8.0m) ddlXepLoai.SelectedValue = "Giỏi";
                else if (diem >= 6.5m) ddlXepLoai.SelectedValue = "Khá";
                else ddlXepLoai.SelectedValue = "Trung bình";
            }
        }

        protected void ddlLocForm_SelectedIndexChanged(object sender, EventArgs e)
        {
            TaiHocVienDat();
            CapNhatXepLoaiTuDong();
        }

        protected void ddlThamGia_SelectedIndexChanged(object sender, EventArgs e)
        {
            CapNhatXepLoaiTuDong();
        }

        private void TaiHocVienDat(string giuMaTG = "")
        {
            string maKH = ddlLocKhoaHoc.SelectedValue;
            string nam = ddlLocNam.SelectedValue;

            string sql = @"SELECT tg.matg, 
                                  hv.hoten + N' - ' + kh.tenkhoahoc + N' (' + kt.nienkhoa + N' - Điểm: ' + CAST(tg.diem AS NVARCHAR(10)) + N')' AS HienThi
                           FROM THAM_GIA tg
                           JOIN HOCVIEN hv ON tg.mahocvien = hv.mahocvien
                           JOIN KYTHI kt ON tg.makythi = kt.makythi
                           JOIN KHOAHOC kh ON kt.makhoahoc = kh.makhoahoc
                           WHERE ((tg.ketqua = N'Đạt'
                                   AND EXISTS (
                                       SELECT 1 FROM DANGKY dk
                                       WHERE dk.mahocvien = tg.mahocvien AND dk.makhoahoc = kt.makhoahoc
                                         AND ISNULL(dk.trangthailop, N'') NOT LIKE N'%Hủy lớp%'
                                         AND ISNULL(dk.trangthaithanhtoan, N'') NOT IN (N'Chờ hoàn tiền', N'Đã hoàn tiền')
                                         AND ISNULL((SELECT SUM(pt.sotienthu) FROM PHIEUTHU pt WHERE pt.madangky = dk.madangky AND ISNULL(pt.noidungthu, N'') NOT LIKE N'Chờ xác nhận%'), 0) >= kh.hocphi
                                   ))
                                  OR tg.matg = @giuMaTG)
                             AND (@maKH = '' OR kt.makhoahoc = @maKH)
                             AND (@nam = '' OR kt.nienkhoa LIKE @namLike)
                           ORDER BY hv.hoten";
            SqlParameter[] p = {
                new SqlParameter("@giuMaTG", giuMaTG ?? ""),
                new SqlParameter("@maKH", maKH ?? ""),
                new SqlParameter("@nam", nam ?? ""),
                new SqlParameter("@namLike", string.IsNullOrEmpty(nam) ? "" : "%" + nam + "%")
            };
            ddlThamGia.DataSource = kncsdl.DocDuLieu(sql, p);
            ddlThamGia.DataTextField = "HienThi";
            ddlThamGia.DataValueField = "matg";
            ddlThamGia.DataBind();
        }

        private void TaiDanhSach()
        {
            string tuKhoa = TuKhoaHienTai;
            string xepLoai = ddlLocXepLoai.SelectedValue;
            bool isHV = MasterPage.ChuanHoaQuyen(Session["Quyen"]) == "HocVien";
            string user = Session["TenDangNhap"]?.ToString() ?? "";

            string sql = @"SELECT cc.*, hv.hoten, tg.diem, kh.tenkhoahoc, kt.nienkhoa
                           FROM CHUNGCHI cc
                           JOIN THAM_GIA tg ON cc.matg = tg.matg
                           JOIN HOCVIEN hv ON tg.mahocvien = hv.mahocvien
                           JOIN KYTHI kt ON tg.makythi = kt.makythi
                           JOIN KHOAHOC kh ON kt.makhoahoc = kh.makhoahoc
                           WHERE (@kw = '' OR cc.sohieuchungchi COLLATE Latin1_General_CI_AI LIKE @kw 
                                           OR hv.hoten COLLATE Latin1_General_CI_AI LIKE @kw 
                                           OR kh.tenkhoahoc COLLATE Latin1_General_CI_AI LIKE @kw 
                                           OR cc.machungchi COLLATE Latin1_General_CI_AI LIKE @kw)
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
                    HttpUtility.HtmlAttributeEncode(Convert.ToString(r["sohieuchungchi"])),
                    HttpUtility.HtmlEncode(Convert.ToString(r["hoten"])),
                    HttpUtility.HtmlEncode(Convert.ToString(r["tenkhoahoc"])));
            }
            litGoiYCC.Text = sb.ToString();
        }

        private void HienThongBao(string msg, bool laLoi = false)
        {
            pnlTB.Visible = true;
            pnlTB.CssClass = laLoi
                ? "alert alert-danger text-danger fw-semibold alert-dismissible fade show shadow"
                : "alert alert-success text-success fw-semibold alert-dismissible fade show shadow";
            litTB.Text = HttpUtility.HtmlEncode(msg);
        }

        private void LamMoiForm()
        {
            txtNgayCap.Text = DateTime.Now.ToString("yyyy-MM-dd");
            ddlLocKhoaHoc.SelectedIndex = 0;
            ddlLocNam.SelectedIndex = 0;
            TaiHocVienDat();
            if (ddlThamGia.Items.Count > 0) ddlThamGia.SelectedIndex = 0;
            SinhMaVaSoHieuMoi();
            CapNhatXepLoaiTuDong();
        }

        protected void btnCapCC_Click(object sender, EventArgs e)
        {
            if (!LaQuanTriHoacNV)
            {
                HienThongBao("Bạn không có quyền cấp chứng chỉ!", true);
                return;
            }

            if (string.IsNullOrEmpty(ddlThamGia.SelectedValue))
            {
                HienThongBao("Vui lòng chọn Học viên đủ điều kiện!", true);
                return;
            }

            try
            {
                string maTG = ddlThamGia.SelectedValue;
                int checkTG = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM CHUNGCHI WHERE matg=@tg", new[] { new SqlParameter("@tg", maTG) }));
                if (checkTG > 0)
                {
                    HienThongBao("Học viên ở kỳ thi này đã được cấp chứng chỉ rồi!", true);
                    return;
                }

                SinhMaVaSoHieuMoi();
                string sql = "INSERT INTO CHUNGCHI(machungchi, ngaycap, sohieuchungchi, xeploai, matg) VALUES(@ma, @nc, @sh, @xl, @tg)";
                SqlParameter[] p = {
                    new SqlParameter("@ma", txtMaCC.Text.Trim()),
                    new SqlParameter("@nc", string.IsNullOrEmpty(txtNgayCap.Text) ? DateTime.Now.ToString("yyyy-MM-dd") : txtNgayCap.Text),
                    new SqlParameter("@sh", txtSoHieu.Text.Trim()),
                    new SqlParameter("@xl", ddlXepLoai.SelectedValue),
                    new SqlParameter("@tg", maTG)
                };
                kncsdl.ThucThiLenh(sql, p);
                kncsdl.GhiNhatKy(Session["TenDangNhap"]?.ToString(), "Cấp chứng chỉ", $"Mã CC: {txtMaCC.Text.Trim()} - Số hiệu: {txtSoHieu.Text.Trim()}");
                HienThongBao("Cấp chứng chỉ thành công!");
                gvChungChi.PageIndex = 0;
                TaiDanhSach();
                LamMoiForm();
            }
            catch { HienThongBao("Không thể cấp chứng chỉ lúc này. Vui lòng kiểm tra lại dữ liệu!", true); }
        }

        protected void btnCapNhatCC_Click(object sender, EventArgs e)
        {
            if (!LaQuanTriHoacNV)
            {
                HienThongBao("Bạn không có quyền cập nhật chứng chỉ!", true);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtMaCC.Text) || string.IsNullOrWhiteSpace(txtSoHieu.Text))
            {
                HienThongBao("Vui lòng chọn chứng chỉ cần sửa!", true);
                return;
            }

            try
            {
                string maCC = txtMaCC.Text.Trim();
                DataTable dtGoc = kncsdl.DocDuLieu("SELECT matg FROM CHUNGCHI WHERE machungchi=@ma", new[] { new SqlParameter("@ma", maCC) });
                if (dtGoc.Rows.Count == 0)
                {
                    HienThongBao("Chứng chỉ này chưa có trong danh sách. Hãy bấm Cấp Chứng chỉ!", true);
                    return;
                }

                // Tránh rơi về người đầu danh sách nếu dropdown trống; đồng thời chặn gán sang người khác đã có chứng chỉ
                string maTGGoc = dtGoc.Rows[0]["matg"].ToString();
                string maTGMoi = !string.IsNullOrEmpty(ddlThamGia.SelectedValue) ? ddlThamGia.SelectedValue : maTGGoc;
                if (maTGMoi != maTGGoc)
                {
                    int daCoCCKhac = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM CHUNGCHI WHERE matg=@tg AND machungchi<>@ma",
                        new[] { new SqlParameter("@tg", maTGMoi), new SqlParameter("@ma", maCC) }));
                    if (daCoCCKhac > 0)
                    {
                        HienThongBao("Học viên được chọn đã có chứng chỉ khác rồi!", true);
                        return;
                    }
                }

                string sql = "UPDATE CHUNGCHI SET ngaycap=@nc, sohieuchungchi=@sh, xeploai=@xl, matg=@tg WHERE machungchi=@ma";
                SqlParameter[] p = {
                    new SqlParameter("@ma", maCC),
                    new SqlParameter("@nc", string.IsNullOrEmpty(txtNgayCap.Text) ? DateTime.Now.ToString("yyyy-MM-dd") : txtNgayCap.Text),
                    new SqlParameter("@sh", txtSoHieu.Text.Trim()),
                    new SqlParameter("@xl", ddlXepLoai.SelectedValue),
                    new SqlParameter("@tg", maTGMoi)
                };
                kncsdl.ThucThiLenh(sql, p);
                kncsdl.GhiNhatKy(Session["TenDangNhap"]?.ToString(), "Cập nhật chứng chỉ", $"Mã CC: {maCC}");
                HienThongBao("Cập nhật chứng chỉ thành công!");
                gvChungChi.PageIndex = 0;
                TaiDanhSach();
                LamMoiForm();
            }
            catch { HienThongBao("Không thể cập nhật chứng chỉ lúc này!", true); }
        }

        protected void btnLamMoiCC_Click(object sender, EventArgs e)
        {
            LamMoiForm();
            TuKhoaHienTai = "";
            txtTimCC.Text = "";
            ddlLocXepLoai.SelectedIndex = 0;
            gvChungChi.PageIndex = 0;
            TaiDanhSach();
            pnlTB.Visible = false;
        }

        protected void gvChungChi_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ChonCC")
            {
                if (!LaQuanTriHoacNV) return;

                string ma = e.CommandArgument.ToString();
                DataTable dt = kncsdl.DocDuLieu("SELECT * FROM CHUNGCHI WHERE machungchi=@ma", new[] { new SqlParameter("@ma", ma) });
                if (dt.Rows.Count > 0)
                {
                    DataRow r = dt.Rows[0];
                    string maTG = r["matg"].ToString();
                    ddlLocKhoaHoc.SelectedIndex = 0;
                    ddlLocNam.SelectedIndex = 0;
                    TaiHocVienDat(maTG);

                    txtMaCC.Text = r["machungchi"].ToString();
                    txtSoHieu.Text = r["sohieuchungchi"].ToString();
                    ddlXepLoai.SelectedValue = r["xeploai"].ToString();
                    if (ddlThamGia.Items.FindByValue(maTG) != null)
                        ddlThamGia.SelectedValue = maTG;
                    if (r["ngaycap"] != DBNull.Value)
                        txtNgayCap.Text = Convert.ToDateTime(r["ngaycap"]).ToString("yyyy-MM-dd");
                    pnlTB.Visible = false;
                }
            }
        }

        protected void btnXoaCCChinhThuc_Click(object sender, EventArgs e)
        {
            if (!LaQuanTriHoacNV)
            {
                HienThongBao("Bạn không có quyền xóa chứng chỉ!", true);
                return;
            }

            string ma = hfMaCCXoa.Value;
            if (!string.IsNullOrEmpty(ma))
            {
                try
                {
                    kncsdl.ThucThiLenh("DELETE FROM CHUNGCHI WHERE machungchi=@ma", new[] { new SqlParameter("@ma", ma) });
                    kncsdl.GhiNhatKy(Session["TenDangNhap"]?.ToString(), "Xóa chứng chỉ", $"Mã CC: {ma}");
                    HienThongBao("Đã xóa chứng chỉ thành công.");
                    if (gvChungChi.Rows.Count == 1 && gvChungChi.PageIndex > 0)
                        gvChungChi.PageIndex--;
                    TaiDanhSach();
                    LamMoiForm();
                }
                catch { HienThongBao("Không thể xóa chứng chỉ này!", true); }
            }
        }

        protected void btnTimCC_Click(object sender, EventArgs e)
        {
            TuKhoaHienTai = txtTimCC.Text.Trim();
            gvChungChi.PageIndex = 0;
            TaiDanhSach();
            pnlTB.Visible = false;
        }

        protected void gvChungChi_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvChungChi.PageIndex = e.NewPageIndex;
            TaiDanhSach();
            pnlTB.Visible = false;
        }
    }
}