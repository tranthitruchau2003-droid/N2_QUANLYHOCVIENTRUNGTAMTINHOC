using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using N2_QUANLYHOCVIENTRUNGTAMTINHOC.DAL;

namespace N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.KyThi
{
    public partial class QuanLyKyThi : System.Web.UI.Page
    {
        private bool LaGiangVien
        {
            get
            {
                string q = Session["Quyen"]?.ToString() ?? "";
                return q.Equals("GiangVien", StringComparison.OrdinalIgnoreCase) || q.Equals("Giảng viên", StringComparison.OrdinalIgnoreCase);
            }
        }

        private bool LaHocVien
        {
            get
            {
                string q = Session["Quyen"]?.ToString() ?? "";
                return q.Equals("HocVien", StringComparison.OrdinalIgnoreCase) || q.Equals("Học viên", StringComparison.OrdinalIgnoreCase);
            }
        }

        private string MaNguoiDung => Session["TenDangNhap"]?.ToString() ?? "";

        private string TuKhoaKT
        {
            get { return ViewState["TuKhoaKT"]?.ToString() ?? ""; }
            set { ViewState["TuKhoaKT"] = value; }
        }

        private string TuKhoaTG
        {
            get { return ViewState["TuKhoaTG"]?.ToString() ?? ""; }
            set { ViewState["TuKhoaTG"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                pnlTB.Visible = false;
            }
            else
            {
                TuKhoaKT = "";
                TuKhoaTG = "";
                txtTimKT.Text = "";
                txtTimTG.Text = "";

                ddlKhoaHoc.DataSource = kncsdl.DocDuLieu("SELECT makhoahoc, tenkhoahoc FROM KHOAHOC ORDER BY tenkhoahoc");
                ddlKhoaHoc.DataTextField = "tenkhoahoc"; ddlKhoaHoc.DataValueField = "makhoahoc"; ddlKhoaHoc.DataBind();

                if (LaHocVien)
                {
                    divFormKyThi.Visible = false;
                    pnlKhuVucNhapDiem.Visible = false;
                    btnMoNhapHangLoat.Visible = false;
                    divFormNhapDiem.Attributes["class"] = "col-12";
                    litTieuDeDiem.Text = "Bảng Điểm & Lịch Thi Các Môn Đã Đăng Ký Của Tôi";
                    gvThamGia.Columns[0].Visible = false;
                    gvThamGia.Columns[1].Visible = false;
                    gvThamGia.Columns[gvThamGia.Columns.Count - 1].Visible = false;
                }
                else if (LaGiangVien)
                {
                    divFormKyThi.Visible = false;
                    divFormNhapDiem.Attributes["class"] = "col-12";
                    litTieuDeDiem.Text = "Nhập Điểm thi & Tính ĐTB (Dành cho Lớp Giảng viên phụ trách)";
                }

                KhoiTaoNamThi();
                SinhMaKTMoi();
                SinhMaTGMoi();
                TaiDuLieu();
                TaiHocVienTheoKyThi();
                TinhDiemChuyenCanTuDiemDanh();
            }
        }

        private void KhoiTaoNamThi()
        {
            int namHienTai = DateTime.Now.Year;
            ddlNamThi.Items.Clear();
            for (int y = namHienTai - 1; y <= namHienTai + 1; y++)
            {
                ddlNamThi.Items.Add(new ListItem(y.ToString(), y.ToString()));
            }
            ddlNamThi.SelectedValue = namHienTai.ToString();
        }

        private void SinhMaKTMoi()
        {
            int soLuong = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM KYTHI")) + 1;
            string maKT = "KT" + soLuong.ToString("D2");
            while (Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM KYTHI WHERE makythi=@ma", new[] { new SqlParameter("@ma", maKT) })) > 0)
            {
                soLuong++;
                maKT = "KT" + soLuong.ToString("D2");
            }
            txtMaKT.Text = maKT;
        }

        private void SinhMaTGMoi()
        {
            int soLuong = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM THAM_GIA")) + 1;
            string maTG = "TG" + soLuong.ToString("D2");
            while (Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM THAM_GIA WHERE matg=@ma", new[] { new SqlParameter("@ma", maTG) })) > 0)
            {
                soLuong++;
                maTG = "TG" + soLuong.ToString("D2");
            }
            txtMaTG.Text = maTG;
        }

        private void TaiHocVienTheoKyThi()
        {
            string maKT = ddlKyThi.SelectedValue;
            if (string.IsNullOrEmpty(maKT))
            {
                ddlHocVien.Items.Clear();
                return;
            }

            string sql = @"SELECT DISTINCT hv.mahocvien, hv.mahocvien + ' - ' + hv.hoten AS HienThi
                           FROM HOCVIEN hv
                           JOIN DANGKY dk ON hv.mahocvien = dk.mahocvien
                           JOIN KYTHI kt ON dk.makhoahoc = kt.makhoahoc
                           LEFT JOIN LOPHOC l ON dk.malop = l.malop
                           WHERE kt.makythi = @kt
                             AND (@isGV = 0 OR l.magiangvien = @user)
                           ORDER BY HienThi";
            SqlParameter[] p = {
                new SqlParameter("@kt", maKT),
                new SqlParameter("@isGV", LaGiangVien ? 1 : 0),
                new SqlParameter("@user", MaNguoiDung)
            };
            DataTable dt = kncsdl.DocDuLieu(sql, p);
            ddlHocVien.DataSource = dt;
            ddlHocVien.DataTextField = "HienThi";
            ddlHocVien.DataValueField = "mahocvien";
            ddlHocVien.DataBind();
        }

        private double TinhDiemCC(string maHV, string maKT)
        {
            if (string.IsNullOrEmpty(maHV) || string.IsNullOrEmpty(maKT)) return 10;

            string sql = @"SELECT 
                            SUM(CASE WHEN dd.trangthai = N'Vắng không phép' THEN 1 ELSE 0 END) AS VangKP,
                            SUM(CASE WHEN dd.trangthai = N'Vắng phép' THEN 1 ELSE 0 END) AS VangP
                           FROM DIEMDANH dd
                           JOIN LOPHOC l ON dd.malop = l.malop
                           JOIN KYTHI kt ON l.makhoahoc = kt.makhoahoc
                           WHERE dd.mahocvien = @hv AND kt.makythi = @kt
                             AND (@isGV = 0 OR l.magiangvien = @user)";
            SqlParameter[] p = {
                new SqlParameter("@hv", maHV),
                new SqlParameter("@kt", maKT),
                new SqlParameter("@isGV", LaGiangVien ? 1 : 0),
                new SqlParameter("@user", MaNguoiDung)
            };
            DataTable dt = kncsdl.DocDuLieu(sql, p);
            double diemCC = 10;
            if (dt.Rows.Count > 0)
            {
                int kp = dt.Rows[0]["VangKP"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["VangKP"]) : 0;
                int vp = dt.Rows[0]["VangP"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["VangP"]) : 0;
                diemCC = Math.Max(0, 10 - kp * 2 - vp * 1);
            }
            return diemCC;
        }

        // Quy đổi ngược chính xác Điểm thi (80%) từ Điểm Trung Bình và Điểm Chuyên Cần (20%)
        private double QuyDoiDiemThiTuDTB(double diemTB, double diemCC)
        {
            double diemThi = Math.Round((diemTB - diemCC * 0.2) / 0.8, 1);
            if (diemThi < 0) diemThi = 0;
            if (diemThi > 10) diemThi = 10;
            return diemThi;
        }

        // Tính Điểm Trung Bình chính xác từ Điểm Chuyên Cần (20%) và Điểm Thi (80%)
        private double TinhDiemTrungBinh(double diemCC, double diemThi)
        {
            return Math.Round(diemCC * 0.2 + diemThi * 0.8, 1, MidpointRounding.AwayFromZero);
        }

        private void TinhDiemChuyenCanTuDiemDanh()
        {
            double diemCC = TinhDiemCC(ddlHocVien.SelectedValue, ddlKyThi.SelectedValue);
            txtDiemCC.Text = diemCC.ToString("0.##", CultureInfo.InvariantCulture);
        }

        protected void ddlKyThi_SelectedIndexChanged(object sender, EventArgs e)
        {
            TaiHocVienTheoKyThi();
            TinhDiemChuyenCanTuDiemDanh();
        }

        protected void ddlDiemDanh_SelectedIndexChanged(object sender, EventArgs e)
        {
            TinhDiemChuyenCanTuDiemDanh();
        }

        private void TaiDuLieu()
        {
            if (!LaGiangVien && !LaHocVien)
            {
                TaiDanhSachKyThi();
            }

            string prevKT = ddlKyThi.SelectedValue;
            string sqlKT = @"SELECT kt.makythi, kt.makythi + N' - ' + kh.tenkhoahoc + N' (' + kt.nienkhoa + N')' AS HienThi 
                             FROM KYTHI kt 
                             JOIN KHOAHOC kh ON kt.makhoahoc = kh.makhoahoc 
                             WHERE (@isGV = 0 OR EXISTS (SELECT 1 FROM LOPHOC l WHERE l.makhoahoc = kt.makhoahoc AND l.magiangvien = @user))
                             ORDER BY kt.makythi DESC";
            SqlParameter[] pKT = {
                new SqlParameter("@isGV", LaGiangVien ? 1 : 0),
                new SqlParameter("@user", MaNguoiDung)
            };
            ddlKyThi.DataSource = kncsdl.DocDuLieu(sqlKT, pKT);
            ddlKyThi.DataTextField = "HienThi"; ddlKyThi.DataValueField = "makythi"; ddlKyThi.DataBind();
            if (!string.IsNullOrEmpty(prevKT) && ddlKyThi.Items.FindByValue(prevKT) != null)
                ddlKyThi.SelectedValue = prevKT;

            TaiDanhSachThamGia();
            TaiGoiYTimKiem();
        }

        private void TaiDanhSachKyThi()
        {
            string tuKhoa = TuKhoaKT;
            string sql = @"SELECT kt.*, kh.tenkhoahoc 
                           FROM KYTHI kt 
                           JOIN KHOAHOC kh ON kt.makhoahoc = kh.makhoahoc
                           WHERE (@kw = '' OR kt.makythi LIKE @kw OR kh.tenkhoahoc LIKE @kw OR kt.nienkhoa LIKE @kw)
                           ORDER BY kt.makythi DESC";
            SqlParameter[] p = { new SqlParameter("@kw", string.IsNullOrEmpty(tuKhoa) ? "" : "%" + tuKhoa + "%") };
            gvKyThi.DataSource = kncsdl.DocDuLieu(sql, p);
            gvKyThi.DataBind();
        }

        private void TaiDanhSachThamGia()
        {
            string tuKhoa = TuKhoaTG;
            string ketQua = ddlLocKetQua.SelectedValue;

            if (LaHocVien)
            {
                string sqlHV = @"SELECT ISNULL(tg.matg, '') AS matg,
                                        hv.hoten,
                                        kh.tenkhoahoc,
                                        ISNULL(kt.makythi, N'Chưa xếp lịch thi') AS makythi,
                                        kt.ngaythi,
                                        CASE WHEN tg.diem IS NULL THEN '' ELSE CAST(tg.diem AS NVARCHAR(20)) END AS diem,
                                        ISNULL(tg.ketqua, '') AS ketqua
                                 FROM DANGKY dk
                                 JOIN HOCVIEN hv ON dk.mahocvien = hv.mahocvien
                                 JOIN KHOAHOC kh ON dk.makhoahoc = kh.makhoahoc
                                 LEFT JOIN KYTHI kt ON kh.makhoahoc = kt.makhoahoc
                                 LEFT JOIN THAM_GIA tg ON tg.mahocvien = dk.mahocvien AND tg.makythi = kt.makythi
                                 WHERE dk.mahocvien = @user
                                 ORDER BY dk.ngaydangky DESC";
                gvThamGia.DataSource = kncsdl.DocDuLieu(sqlHV, new[] { new SqlParameter("@user", MaNguoiDung) });
                gvThamGia.DataBind();
            }
            else
            {
                string sql = @"SELECT tg.matg, hv.hoten, kh.tenkhoahoc, tg.makythi, kt.ngaythi, tg.diem, tg.ketqua
                               FROM THAM_GIA tg 
                               JOIN HOCVIEN hv ON tg.mahocvien = hv.mahocvien
                               JOIN KYTHI kt ON tg.makythi = kt.makythi
                               JOIN KHOAHOC kh ON kt.makhoahoc = kh.makhoahoc
                               WHERE (@kw = '' OR tg.matg LIKE @kw OR hv.hoten LIKE @kw OR tg.makythi LIKE @kw OR kh.tenkhoahoc LIKE @kw)
                                 AND (@kq = '' OR tg.ketqua = @kq)
                                 AND (@isGV = 0 OR EXISTS (
                                     SELECT 1 FROM DANGKY dk 
                                     JOIN LOPHOC l ON dk.malop = l.malop 
                                     WHERE dk.mahocvien = tg.mahocvien AND l.makhoahoc = kt.makhoahoc AND l.magiangvien = @user
                                 ))
                               ORDER BY tg.matg DESC";
                SqlParameter[] p = {
                    new SqlParameter("@kw", string.IsNullOrEmpty(tuKhoa) ? "" : "%" + tuKhoa + "%"),
                    new SqlParameter("@kq", ketQua ?? ""),
                    new SqlParameter("@isGV", LaGiangVien ? 1 : 0),
                    new SqlParameter("@user", MaNguoiDung)
                };
                gvThamGia.DataSource = kncsdl.DocDuLieu(sql, p);
                gvThamGia.DataBind();
            }
        }

        private void TaiGoiYTimKiem()
        {
            if (!LaGiangVien && !LaHocVien)
            {
                DataTable dtKT = kncsdl.DocDuLieu("SELECT kt.makythi, kt.nienkhoa, kh.tenkhoahoc FROM KYTHI kt JOIN KHOAHOC kh ON kt.makhoahoc = kh.makhoahoc");
                StringBuilder sbKT = new StringBuilder();
                foreach (DataRow r in dtKT.Rows)
                {
                    sbKT.AppendFormat("<button type=\"button\" class=\"list-group-item list-group-item-action py-2 px-2 small goi-y-item\" data-value=\"{0}\"><strong>{0}</strong> - {1} ({2})</button>",
                        r["makythi"], r["tenkhoahoc"], r["nienkhoa"]);
                }
                litGoiYKT.Text = sbKT.ToString();
            }

            string sqlTG = @"SELECT tg.matg, tg.makythi, hv.hoten 
                             FROM THAM_GIA tg 
                             JOIN HOCVIEN hv ON tg.mahocvien = hv.mahocvien
                             JOIN KYTHI kt ON tg.makythi = kt.makythi
                             WHERE (@isGV = 0 OR EXISTS (
                                 SELECT 1 FROM DANGKY dk 
                                 JOIN LOPHOC l ON dk.malop = l.malop 
                                 WHERE dk.mahocvien = tg.mahocvien AND l.makhoahoc = kt.makhoahoc AND l.magiangvien = @user
                             ))";
            DataTable dtTG = kncsdl.DocDuLieu(sqlTG, new[] {
                new SqlParameter("@isGV", LaGiangVien ? 1 : 0),
                new SqlParameter("@user", MaNguoiDung)
            });
            StringBuilder sbTG = new StringBuilder();
            foreach (DataRow r in dtTG.Rows)
            {
                sbTG.AppendFormat("<button type=\"button\" class=\"list-group-item list-group-item-action py-2 px-2 small goi-y-item\" data-value=\"{1}\"><strong>{0}</strong> - {1} ({2})</button>",
                    r["matg"], r["hoten"], r["makythi"]);
            }
            litGoiYTG.Text = sbTG.ToString();
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
            ddlDotThi.SelectedIndex = 0;
            if (ddlNamThi.Items.FindByValue(DateTime.Now.Year.ToString()) != null)
                ddlNamThi.SelectedValue = DateTime.Now.Year.ToString();
            txtNgayThi.Text = "";
            SinhMaKTMoi();
        }

        private void LamMoiFormTG()
        {
            txtDiem.Text = "";
            SinhMaTGMoi();
            TinhDiemChuyenCanTuDiemDanh();
        }

        protected void btnTaoKyThi_Click(object sender, EventArgs e)
        {
            if (LaGiangVien || LaHocVien)
            {
                HienThongBao("Bạn không có quyền tạo kỳ thi!", true);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNgayThi.Text))
            {
                HienThongBao("Vui lòng chọn Ngày thi!", true);
                return;
            }

            try
            {
                SinhMaKTMoi();
                string sql = "INSERT INTO KYTHI(makythi, nienkhoa, ngaythi, makhoahoc) VALUES(@ma, @nk, @ngay, @kh)";
                SqlParameter[] p = {
                    new SqlParameter("@ma", txtMaKT.Text.Trim()),
                    new SqlParameter("@nk", $"{ddlDotThi.SelectedValue} - {ddlNamThi.SelectedValue}"),
                    new SqlParameter("@ngay", txtNgayThi.Text),
                    new SqlParameter("@kh", ddlKhoaHoc.SelectedValue)
                };
                kncsdl.ThucThiLenh(sql, p);
                HienThongBao("Đã tạo kỳ thi mới thành công!");
                gvKyThi.PageIndex = 0;
                TaiDuLieu();
                TaiHocVienTheoKyThi();
                LamMoiFormKT();
            }
            catch (Exception ex) { HienThongBao("Lỗi khi tạo kỳ thi: " + ex.Message, true); }
        }

        protected void btnCapNhatKT_Click(object sender, EventArgs e)
        {
            if (LaGiangVien || LaHocVien) return;

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
                    new SqlParameter("@nk", $"{ddlDotThi.SelectedValue} - {ddlNamThi.SelectedValue}"),
                    new SqlParameter("@ngay", txtNgayThi.Text),
                    new SqlParameter("@kh", ddlKhoaHoc.SelectedValue)
                };
                kncsdl.ThucThiLenh(sql, p);
                HienThongBao("Cập nhật kỳ thi thành công!");
                TaiDuLieu();
                TaiHocVienTheoKyThi();
                LamMoiFormKT();
            }
            catch (Exception ex) { HienThongBao("Lỗi cập nhật: " + ex.Message, true); }
        }

        protected void btnLamMoiKT_Click(object sender, EventArgs e)
        {
            LamMoiFormKT();
            TuKhoaKT = "";
            txtTimKT.Text = "";
            gvKyThi.PageIndex = 0;
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
                    ddlKhoaHoc.SelectedValue = r["makhoahoc"].ToString();

                    string nk = r["nienkhoa"].ToString();
                    ddlDotThi.SelectedValue = nk.Contains("Đợt 2") ? "Đợt 2" : "Đợt 1";
                    var mNam = Regex.Match(nk, @"\b(20\d{2})\b");
                    if (mNam.Success)
                    {
                        string nam = mNam.Groups[1].Value;
                        if (ddlNamThi.Items.FindByValue(nam) == null)
                            ddlNamThi.Items.Add(new ListItem(nam, nam));
                        ddlNamThi.SelectedValue = nam;
                    }

                    if (r["ngaythi"] != DBNull.Value)
                        txtNgayThi.Text = Convert.ToDateTime(r["ngaythi"]).ToString("yyyy-MM-dd");
                    pnlTB.Visible = false;
                }
            }
        }

        protected void btnXoaKTChinhThuc_Click(object sender, EventArgs e)
        {
            if (LaGiangVien || LaHocVien) return;

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
                    if (gvKyThi.Rows.Count == 1 && gvKyThi.PageIndex > 0) gvKyThi.PageIndex--;
                    TaiDuLieu();
                    TaiHocVienTheoKyThi();
                    LamMoiFormKT();
                }
                catch (Exception ex) { HienThongBao("Lỗi khi xóa: " + ex.Message, true); }
            }
        }

        protected void btnTimKT_Click(object sender, EventArgs e)
        {
            TuKhoaKT = txtTimKT.Text.Trim();
            gvKyThi.PageIndex = 0;
            TaiDanhSachKyThi();
        }

        protected void gvKyThi_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvKyThi.PageIndex = e.NewPageIndex;
            TaiDanhSachKyThi();
        }

        protected void btnTimTG_Click(object sender, EventArgs e)
        {
            TuKhoaTG = txtTimTG.Text.Trim();
            gvThamGia.PageIndex = 0;
            TaiDanhSachThamGia();
        }

        protected void gvThamGia_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvThamGia.PageIndex = e.NewPageIndex;
            TaiDanhSachThamGia();
        }

        protected void btnThemTG_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ddlHocVien.SelectedValue) || string.IsNullOrEmpty(ddlKyThi.SelectedValue))
            {
                HienThongBao("Vui lòng chọn Kỳ thi và Học viên thuộc lớp bạn phụ trách!", true);
                return;
            }

            double diemThi;
            if (!double.TryParse(txtDiem.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out diemThi) || diemThi < 0 || diemThi > 10)
            {
                HienThongBao("Điểm thi phải là số từ 0 đến 10!", true);
                txtDiem.Focus();
                return;
            }

            try
            {
                int daCo = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM THAM_GIA WHERE mahocvien=@hv AND makythi=@kt",
                    new[] { new SqlParameter("@hv", ddlHocVien.SelectedValue), new SqlParameter("@kt", ddlKyThi.SelectedValue) }));
                if (daCo > 0)
                {
                    HienThongBao("Học viên này đã có điểm trong kỳ thi đang chọn! Vui lòng bấm Sửa ở bảng dưới.", true);
                    return;
                }

                SinhMaTGMoi();
                double diemCC = TinhDiemCC(ddlHocVien.SelectedValue, ddlKyThi.SelectedValue);
                txtDiemCC.Text = diemCC.ToString("0.##", CultureInfo.InvariantCulture);

                double diemTB = TinhDiemTrungBinh(diemCC, diemThi);
                string ketQua = diemTB >= 5.0 ? "Đạt" : "Không đạt";

                string sql = "INSERT INTO THAM_GIA(matg, mahocvien, makythi, diem, ketqua) VALUES(@ma, @hv, @kt, @diem, @kq)";
                SqlParameter[] p = {
                    new SqlParameter("@ma", txtMaTG.Text.Trim()),
                    new SqlParameter("@hv", ddlHocVien.SelectedValue),
                    new SqlParameter("@kt", ddlKyThi.SelectedValue),
                    new SqlParameter("@diem", diemTB),
                    new SqlParameter("@kq", ketQua)
                };
                kncsdl.ThucThiLenh(sql, p);
                HienThongBao($"Thêm điểm thành công (CC: {diemCC:0.##} | Thi: {diemThi:0.##} -> ĐTB: {diemTB:0.0} - {ketQua})!");
                gvThamGia.PageIndex = 0;
                TaiDuLieu();
                LamMoiFormTG();
            }
            catch (Exception ex) { HienThongBao("Lỗi khi thêm điểm: " + ex.Message, true); }
        }

        protected void btnCapNhatTG_Click(object sender, EventArgs e)
        {
            double diemThi;
            if (string.IsNullOrWhiteSpace(txtMaTG.Text) || !double.TryParse(txtDiem.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out diemThi) || diemThi < 0 || diemThi > 10)
            {
                HienThongBao("Vui lòng chọn bản ghi cần sửa và nhập Điểm thi từ 0 đến 10!", true);
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

                double diemCC = TinhDiemCC(ddlHocVien.SelectedValue, ddlKyThi.SelectedValue);
                txtDiemCC.Text = diemCC.ToString("0.##", CultureInfo.InvariantCulture);

                double diemTB = TinhDiemTrungBinh(diemCC, diemThi);
                string ketQua = diemTB >= 5.0 ? "Đạt" : "Không đạt";

                string sql = "UPDATE THAM_GIA SET mahocvien=@hv, makythi=@kt, diem=@diem, ketqua=@kq WHERE matg=@ma";
                SqlParameter[] p = {
                    new SqlParameter("@ma", txtMaTG.Text.Trim()),
                    new SqlParameter("@hv", ddlHocVien.SelectedValue),
                    new SqlParameter("@kt", ddlKyThi.SelectedValue),
                    new SqlParameter("@diem", diemTB),
                    new SqlParameter("@kq", ketQua)
                };
                kncsdl.ThucThiLenh(sql, p);
                HienThongBao($"Cập nhật điểm thành công (CC: {diemCC:0.##} | Thi: {diemThi:0.##} -> ĐTB: {diemTB:0.0} - {ketQua})!");
                TaiDuLieu();
                LamMoiFormTG();
            }
            catch (Exception ex) { HienThongBao("Lỗi cập nhật điểm: " + ex.Message, true); }
        }

        protected void btnLamMoiTG_Click(object sender, EventArgs e)
        {
            LamMoiFormTG();
            TuKhoaTG = "";
            txtTimTG.Text = "";
            ddlLocKetQua.SelectedIndex = 0;
            gvThamGia.PageIndex = 0;
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
                    string maKT = r["makythi"].ToString();
                    if (ddlKyThi.Items.FindByValue(maKT) != null)
                        ddlKyThi.SelectedValue = maKT;
                    TaiHocVienTheoKyThi();

                    string maHV = r["mahocvien"].ToString();
                    if (ddlHocVien.Items.FindByValue(maHV) != null)
                        ddlHocVien.SelectedValue = maHV;

                    double diemCC = TinhDiemCC(maHV, maKT);
                    txtDiemCC.Text = diemCC.ToString("0.##", CultureInfo.InvariantCulture);

                    // Quy đổi ngược từ ĐTB lưu trong CSDL ra đúng Điểm thi (80%) để hiển thị khi sửa
                    if (r["diem"] != DBNull.Value)
                    {
                        double diemTB = Convert.ToDouble(r["diem"]);
                        double diemThiGoc = QuyDoiDiemThiTuDTB(diemTB, diemCC);
                        txtDiem.Text = diemThiGoc.ToString("0.##", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        txtDiem.Text = "";
                    }

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
                    if (gvThamGia.Rows.Count == 1 && gvThamGia.PageIndex > 0) gvThamGia.PageIndex--;
                    TaiDuLieu();
                    LamMoiFormTG();
                }
                catch (Exception ex) { HienThongBao("Lỗi khi xóa: " + ex.Message, true); }
            }
        }

        protected void btnMoNhapHangLoat_Click(object sender, EventArgs e)
        {
            string maKT = ddlKyThi.SelectedValue;
            if (string.IsNullOrEmpty(maKT))
            {
                HienThongBao("Vui lòng chọn Kỳ thi trước!", true);
                return;
            }

            litTenKyThiHL.Text = ddlKyThi.SelectedItem.Text;

            string sql = @"SELECT DISTINCT hv.mahocvien, hv.hoten, tg.diem AS DiemTBCSDL
                           FROM HOCVIEN hv
                           JOIN DANGKY dk ON hv.mahocvien = dk.mahocvien
                           JOIN KYTHI kt ON dk.makhoahoc = kt.makhoahoc
                           LEFT JOIN LOPHOC l ON dk.malop = l.malop
                           LEFT JOIN THAM_GIA tg ON tg.mahocvien = hv.mahocvien AND tg.makythi = kt.makythi
                           WHERE kt.makythi = @kt
                             AND (@isGV = 0 OR l.magiangvien = @user)
                           ORDER BY hv.hoten";
            SqlParameter[] p = {
                new SqlParameter("@kt", maKT),
                new SqlParameter("@isGV", LaGiangVien ? 1 : 0),
                new SqlParameter("@user", MaNguoiDung)
            };
            DataTable dt = kncsdl.DocDuLieu(sql, p);
            dt.Columns.Add("DiemCC", typeof(string));
            dt.Columns.Add("DiemHienTai", typeof(string));

            foreach (DataRow r in dt.Rows)
            {
                string maHV = r["mahocvien"].ToString();
                double diemCC = TinhDiemCC(maHV, maKT);
                r["DiemCC"] = diemCC.ToString("0.##", CultureInfo.InvariantCulture);

                if (r["DiemTBCSDL"] != DBNull.Value)
                {
                    double diemTB = Convert.ToDouble(r["DiemTBCSDL"]);
                    double diemThiGoc = QuyDoiDiemThiTuDTB(diemTB, diemCC);
                    r["DiemHienTai"] = diemThiGoc.ToString("0.##", CultureInfo.InvariantCulture);
                }
                else
                {
                    r["DiemHienTai"] = "";
                }
            }

            gvNhapHangLoat.DataSource = dt;
            gvNhapHangLoat.DataBind();

            ScriptManager.RegisterStartupScript(this, GetType(), "showHL", "showModalNhapHangLoat();", true);
        }

        protected void btnLuuHangLoat_Click(object sender, EventArgs e)
        {
            string maKT = ddlKyThi.SelectedValue;
            if (string.IsNullOrEmpty(maKT)) return;

            int daLuu = 0;
            foreach (GridViewRow row in gvNhapHangLoat.Rows)
            {
                string maHV = gvNhapHangLoat.DataKeys[row.RowIndex].Value.ToString();
                TextBox txtThi = (TextBox)row.FindControl("txtDiemThiHL");

                if (txtThi == null) continue;
                string rawVal = txtThi.Text.Trim();

                object existingMaTG = kncsdl.LayGiaTriDon("SELECT matg FROM THAM_GIA WHERE mahocvien=@hv AND makythi=@kt",
                    new[] { new SqlParameter("@hv", maHV), new SqlParameter("@kt", maKT) });

                // Nếu ô trống và thí sinh này chưa từng có điểm -> bỏ qua
                if (string.IsNullOrEmpty(rawVal)) continue;

                double diemThi;
                if (!double.TryParse(rawVal.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out diemThi) || diemThi < 0 || diemThi > 10)
                    continue;

                // Tính lại điểm chuyên cần chuẩn xác trực tiếp theo từng thí sinh
                double diemCC = TinhDiemCC(maHV, maKT);
                double diemTB = TinhDiemTrungBinh(diemCC, diemThi);
                string ketQua = diemTB >= 5.0 ? "Đạt" : "Không đạt";

                if (existingMaTG != null && existingMaTG != DBNull.Value)
                {
                    kncsdl.ThucThiLenh("UPDATE THAM_GIA SET diem=@diem, ketqua=@kq WHERE matg=@ma",
                        new[] { new SqlParameter("@diem", diemTB), new SqlParameter("@kq", ketQua), new SqlParameter("@ma", existingMaTG.ToString()) });
                }
                else
                {
                    SinhMaTGMoi();
                    kncsdl.ThucThiLenh("INSERT INTO THAM_GIA(matg, mahocvien, makythi, diem, ketqua) VALUES(@ma, @hv, @kt, @diem, @kq)",
                        new[] { new SqlParameter("@ma", txtMaTG.Text.Trim()), new SqlParameter("@hv", maHV), new SqlParameter("@kt", maKT), new SqlParameter("@diem", diemTB), new SqlParameter("@kq", ketQua) });
                }
                daLuu++;
            }

            HienThongBao($"Đã lưu điểm hàng loạt chính xác cho {daLuu} học viên!");
            TaiDuLieu();
            LamMoiFormTG();
        }
    }
}