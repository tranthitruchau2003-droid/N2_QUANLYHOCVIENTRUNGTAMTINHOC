using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using N2_QUANLYHOCVIENTRUNGTAMTINHOC.DAL;

namespace N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.KyThi
{
    public partial class QuanLyKyThi : System.Web.UI.Page
    {
        private string QuyenHienTai => MasterPage.ChuanHoaQuyen(Session["Quyen"]);
        private bool LaGiangVien => QuyenHienTai == "GiangVien";
        private bool LaHocVien => QuyenHienTai == "HocVien";
        private bool LaQuanTriHoacNV => QuyenHienTai == "Admin" || QuyenHienTai == "NhanVien";
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
                           JOIN KHOAHOC kh ON dk.makhoahoc = kh.makhoahoc
                           JOIN KYTHI kt ON dk.makhoahoc = kt.makhoahoc
                           LEFT JOIN LOPHOC l ON dk.malop = l.malop
                           WHERE kt.makythi = @kt
                             AND ISNULL(dk.trangthailop, N'') NOT LIKE N'%Hủy lớp%'
                             AND ISNULL(dk.trangthaithanhtoan, N'') NOT IN (N'Chờ hoàn tiền', N'Đã hoàn tiền')
                             AND ISNULL((SELECT SUM(pt.sotienthu) FROM PHIEUTHU pt WHERE pt.madangky = dk.madangky AND ISNULL(pt.noidungthu, N'') NOT LIKE N'Chờ xác nhận%'), 0) >= kh.hocphi
                             AND (SELECT COUNT(*) FROM DIEMDANH dd WHERE dd.mahocvien = hv.mahocvien AND dd.malop = dk.malop AND dd.trangthai IN (N'Vắng phép', N'Vắng không phép')) < 3
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

        private bool KiemTraDuDieuKienDuThi(string maHV, string maKT, out string lyDo)
        {
            lyDo = "";
            DataTable dt = kncsdl.DocDuLieu(@"SELECT TOP 1 dk.madangky, dk.malop, dk.trangthailop, dk.trangthaithanhtoan, kh.hocphi,
                                                     ISNULL((SELECT SUM(pt.sotienthu) FROM PHIEUTHU pt WHERE pt.madangky = dk.madangky AND ISNULL(pt.noidungthu, N'') NOT LIKE N'Chờ xác nhận%'), 0) AS dadong
                                              FROM DANGKY dk
                                              JOIN KHOAHOC kh ON dk.makhoahoc = kh.makhoahoc
                                              JOIN KYTHI kt ON dk.makhoahoc = kt.makhoahoc
                                              WHERE dk.mahocvien = @hv AND kt.makythi = @kt
                                              ORDER BY dk.madangky DESC",
                new[] { new SqlParameter("@hv", maHV), new SqlParameter("@kt", maKT) });

            if (dt.Rows.Count == 0)
            {
                lyDo = "Học viên chưa đăng ký khóa học của kỳ thi này!";
                return false;
            }

            DataRow r = dt.Rows[0];
            string ttLop = r["trangthailop"]?.ToString() ?? "";
            string ttTT = r["trangthaithanhtoan"]?.ToString() ?? "";
            if (ttLop.Contains("Hủy lớp") || ttTT == "Chờ hoàn tiền" || ttTT == "Đã hoàn tiền")
            {
                lyDo = "Học viên thuộc lớp đã hủy hoặc đã hoàn tiền học phí, không đủ điều kiện dự thi!";
                return false;
            }

            decimal hocPhi = Convert.ToDecimal(r["hocphi"]);
            decimal daDong = Convert.ToDecimal(r["dadong"]);
            if (daDong < hocPhi)
            {
                lyDo = $"Học viên chưa đóng đủ học phí ({daDong:N0}/{hocPhi:N0} đ), không đủ điều kiện dự thi!";
                return false;
            }

            string maLop = r["malop"]?.ToString() ?? "";
            if (!string.IsNullOrEmpty(maLop))
            {
                int soVang = Convert.ToInt32(kncsdl.LayGiaTriDon(
                    "SELECT COUNT(*) FROM DIEMDANH WHERE mahocvien=@hv AND malop=@l AND trangthai IN (N'Vắng phép', N'Vắng không phép')",
                    new[] { new SqlParameter("@hv", maHV), new SqlParameter("@l", maLop) }));
                if (soVang >= 3)
                {
                    lyDo = $"Học viên đã nghỉ {soVang} buổi (>= 3 buổi), bị CẤM THI!";
                    return false;
                }
            }
            return true;
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
                           WHERE dd.mahocvien = @hv AND kt.makythi = @kt";
            SqlParameter[] p = {
                new SqlParameter("@hv", maHV),
                new SqlParameter("@kt", maKT)
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

        private bool KiemTraQuyenGiangVienTrenBanGhi(string maHV, string maKT)
        {
            if (LaQuanTriHoacNV) return true;
            if (!LaGiangVien) return false;

            string sql = @"SELECT COUNT(*) FROM DANGKY dk
                           JOIN LOPHOC l ON dk.malop = l.malop
                           JOIN KYTHI kt ON l.makhoahoc = kt.makhoahoc
                           WHERE dk.mahocvien = @hv AND kt.makythi = @kt AND l.magiangvien = @gv";
            int c = Convert.ToInt32(kncsdl.LayGiaTriDon(sql, new[] {
                new SqlParameter("@hv", maHV),
                new SqlParameter("@kt", maKT),
                new SqlParameter("@gv", MaNguoiDung)
            }));
            return c > 0;
        }

        private double LayDiemThiGoc(object valDiemThiGoc, object valDiemTB, double diemCC)
        {
            if (valDiemThiGoc != null && valDiemThiGoc != DBNull.Value)
            {
                return Convert.ToDouble(valDiemThiGoc);
            }
            if (valDiemTB != null && valDiemTB != DBNull.Value)
            {
                double dtb = Convert.ToDouble(valDiemTB);
                double suyNguoc = Math.Round((dtb - diemCC * 0.2) / 0.8, 1, MidpointRounding.AwayFromZero);
                return Math.Max(0, Math.Min(10, suyNguoc));
            }
            return 0;
        }

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
            if (LaQuanTriHoacNV)
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
                       WHERE (@kw = '' OR kt.makythi COLLATE Latin1_General_CI_AI LIKE @kw 
                                       OR kh.tenkhoahoc COLLATE Latin1_General_CI_AI LIKE @kw 
                                       OR kt.nienkhoa COLLATE Latin1_General_CI_AI LIKE @kw)
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
                       WHERE (@kw = '' OR tg.matg COLLATE Latin1_General_CI_AI LIKE @kw 
                                       OR hv.hoten COLLATE Latin1_General_CI_AI LIKE @kw 
                                       OR tg.makythi COLLATE Latin1_General_CI_AI LIKE @kw 
                                       OR kh.tenkhoahoc COLLATE Latin1_General_CI_AI LIKE @kw)
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
            if (LaQuanTriHoacNV)
            {
                DataTable dtKT = kncsdl.DocDuLieu("SELECT kt.makythi, kt.nienkhoa, kh.tenkhoahoc FROM KYTHI kt JOIN KHOAHOC kh ON kt.makhoahoc = kh.makhoahoc");
                StringBuilder sbKT = new StringBuilder();
                foreach (DataRow r in dtKT.Rows)
                {
                    string ma = HttpUtility.HtmlAttributeEncode(r["makythi"].ToString());
                    string ten = HttpUtility.HtmlEncode(r["tenkhoahoc"].ToString());
                    string nk = HttpUtility.HtmlEncode(r["nienkhoa"].ToString());
                    sbKT.AppendFormat("<button type=\"button\" class=\"list-group-item list-group-item-action py-2 px-2 small goi-y-item\" data-value=\"{0}\"><strong>{0}</strong> - {1} ({2})</button>",
                        ma, ten, nk);
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
                string maTG = HttpUtility.HtmlEncode(r["matg"].ToString());
                string hoTen = HttpUtility.HtmlAttributeEncode(r["hoten"].ToString());
                string maKT = HttpUtility.HtmlEncode(r["makythi"].ToString());
                sbTG.AppendFormat("<button type=\"button\" class=\"list-group-item list-group-item-action py-2 px-2 small goi-y-item\" data-value=\"{1}\"><strong>{0}</strong> - {1} ({2})</button>",
                    maTG, hoTen, maKT);
            }
            litGoiYTG.Text = sbTG.ToString();
        }

        private void HienThongBao(string msg, bool laLoi = false)
        {
            pnlTB.Visible = true;
            pnlTB.CssClass = laLoi
                ? "alert alert-danger text-danger fw-semibold alert-dismissible fade show shadow"
                : "alert alert-success text-success fw-semibold alert-dismissible fade show shadow";
            litTB.Text = HttpUtility.HtmlEncode(msg);
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
            if (!LaQuanTriHoacNV)
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
                string nienKhoa = $"{ddlDotThi.SelectedValue} - {ddlNamThi.SelectedValue}";

                int trung = Convert.ToInt32(kncsdl.LayGiaTriDon(
                    "SELECT COUNT(*) FROM KYTHI WHERE makhoahoc=@kh AND nienkhoa=@nk",
                    new[] { new SqlParameter("@kh", ddlKhoaHoc.SelectedValue), new SqlParameter("@nk", nienKhoa) }));
                if (trung > 0)
                {
                    HienThongBao($"Khóa học này đã có kỳ thi cho {nienKhoa}!", true);
                    return;
                }

                SinhMaKTMoi();
                string maMoi = txtMaKT.Text.Trim();
                string sql = "INSERT INTO KYTHI(makythi, nienkhoa, ngaythi, makhoahoc) VALUES(@ma, @nk, @ngay, @kh)";
                SqlParameter[] p = {
                    new SqlParameter("@ma", maMoi),
                    new SqlParameter("@nk", nienKhoa),
                    new SqlParameter("@ngay", txtNgayThi.Text),
                    new SqlParameter("@kh", ddlKhoaHoc.SelectedValue)
                };
                kncsdl.ThucThiLenh(sql, p);
                kncsdl.GhiNhatKy(MaNguoiDung, "Tạo kỳ thi", $"Mã KT: {maMoi} ({nienKhoa})");
                HienThongBao("Đã tạo kỳ thi mới thành công!");
                gvKyThi.PageIndex = 0;
                TaiDuLieu();
                if (ddlKyThi.Items.FindByValue(maMoi) != null) ddlKyThi.SelectedValue = maMoi;
                TaiHocVienTheoKyThi();
                LamMoiFormKT();
            }
            catch { HienThongBao("Không thể tạo kỳ thi lúc này. Vui lòng kiểm tra lại dữ liệu!", true); }
        }

        protected void btnCapNhatKT_Click(object sender, EventArgs e)
        {
            if (!LaQuanTriHoacNV) return;

            if (string.IsNullOrWhiteSpace(txtMaKT.Text) || string.IsNullOrWhiteSpace(txtNgayThi.Text))
            {
                HienThongBao("Vui lòng chọn kỳ thi cần sửa và nhập đầy đủ Ngày thi!", true);
                return;
            }

            try
            {
                string maKT = txtMaKT.Text.Trim();
                DataTable dtCu = kncsdl.DocDuLieu("SELECT makhoahoc FROM KYTHI WHERE makythi=@ma", new[] { new SqlParameter("@ma", maKT) });
                if (dtCu.Rows.Count == 0)
                {
                    HienThongBao("Kỳ thi này chưa có trong danh sách. Hãy bấm 'Tạo Kỳ thi'!", true);
                    return;
                }

                string khCu = dtCu.Rows[0]["makhoahoc"].ToString();
                if (khCu != ddlKhoaHoc.SelectedValue)
                {
                    int coDiem = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM THAM_GIA WHERE makythi=@ma", new[] { new SqlParameter("@ma", maKT) }));
                    if (coDiem > 0)
                    {
                        HienThongBao("Không thể đổi khóa học vì kỳ thi này đã có học viên được chấm điểm!", true);
                        return;
                    }
                }

                string sql = "UPDATE KYTHI SET nienkhoa=@nk, ngaythi=@ngay, makhoahoc=@kh WHERE makythi=@ma";
                SqlParameter[] p = {
                    new SqlParameter("@ma", maKT),
                    new SqlParameter("@nk", $"{ddlDotThi.SelectedValue} - {ddlNamThi.SelectedValue}"),
                    new SqlParameter("@ngay", txtNgayThi.Text),
                    new SqlParameter("@kh", ddlKhoaHoc.SelectedValue)
                };
                kncsdl.ThucThiLenh(sql, p);
                kncsdl.GhiNhatKy(MaNguoiDung, "Cập nhật kỳ thi", $"Mã KT: {maKT}");
                HienThongBao("Cập nhật kỳ thi thành công!");
                TaiDuLieu();
                TaiHocVienTheoKyThi();
                LamMoiFormKT();
            }
            catch { HienThongBao("Không thể cập nhật kỳ thi. Vui lòng kiểm tra lại!", true); }
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
            if (!LaQuanTriHoacNV) return;

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
                    kncsdl.GhiNhatKy(MaNguoiDung, "Xóa kỳ thi", $"Mã KT: {ma}");
                    HienThongBao("Đã xóa kỳ thi thành công.");
                    if (gvKyThi.Rows.Count == 1 && gvKyThi.PageIndex > 0) gvKyThi.PageIndex--;
                    TaiDuLieu();
                    TaiHocVienTheoKyThi();
                    LamMoiFormKT();
                }
                catch { HienThongBao("Không thể xóa kỳ thi này!", true); }
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
            if (LaHocVien) return;

            string maHV = ddlHocVien.SelectedValue;
            string maKT = ddlKyThi.SelectedValue;
            if (string.IsNullOrEmpty(maHV) || string.IsNullOrEmpty(maKT))
            {
                HienThongBao("Vui lòng chọn Kỳ thi và Học viên!", true);
                return;
            }

            if (!KiemTraQuyenGiangVienTrenBanGhi(maHV, maKT))
            {
                HienThongBao("Bạn chỉ được phép nhập điểm cho học viên thuộc lớp mình phụ trách!", true);
                return;
            }

            string lyDoKhongDuDK;
            if (!KiemTraDuDieuKienDuThi(maHV, maKT, out lyDoKhongDuDK))
            {
                HienThongBao(lyDoKhongDuDK, true);
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
                    new[] { new SqlParameter("@hv", maHV), new SqlParameter("@kt", maKT) }));
                if (daCo > 0)
                {
                    HienThongBao("Học viên này đã có điểm trong kỳ thi đang chọn! Vui lòng bấm Sửa ở bảng dưới.", true);
                    return;
                }

                SinhMaTGMoi();
                double diemCC = TinhDiemCC(maHV, maKT);
                txtDiemCC.Text = diemCC.ToString("0.##", CultureInfo.InvariantCulture);

                double diemTB = TinhDiemTrungBinh(diemCC, diemThi);
                string ketQua = diemTB >= 5.0 ? "Đạt" : "Không đạt";

                string sql = @"INSERT INTO THAM_GIA(matg, mahocvien, makythi, diemchuyencan, diemquatrinh, diem, ketqua) 
                               VALUES(@ma, @hv, @kt, @cc, @thi, @diem, @kq)";
                SqlParameter[] p = {
                    new SqlParameter("@ma", txtMaTG.Text.Trim()),
                    new SqlParameter("@hv", maHV),
                    new SqlParameter("@kt", maKT),
                    new SqlParameter("@cc", diemCC),
                    new SqlParameter("@thi", diemThi),
                    new SqlParameter("@diem", diemTB),
                    new SqlParameter("@kq", ketQua)
                };
                kncsdl.ThucThiLenh(sql, p);
                kncsdl.GhiNhatKy(MaNguoiDung, "Nhập điểm thi", $"Mã TG: {txtMaTG.Text.Trim()} - HV {maHV} - ĐTB: {diemTB:0.0} ({ketQua})");
                HienThongBao($"Thêm điểm thành công (CC: {diemCC:0.##} | Thi: {diemThi:0.##} -> ĐTB: {diemTB:0.0} - {ketQua})!");
                gvThamGia.PageIndex = 0;
                TaiDuLieu();
                LamMoiFormTG();
            }
            catch { HienThongBao("Không thể thêm điểm lúc này. Vui lòng kiểm tra lại!", true); }
        }

        protected void btnCapNhatTG_Click(object sender, EventArgs e)
        {
            if (LaHocVien) return;

            double diemThi;
            if (string.IsNullOrWhiteSpace(txtMaTG.Text) || !double.TryParse(txtDiem.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out diemThi) || diemThi < 0 || diemThi > 10)
            {
                HienThongBao("Vui lòng chọn bản ghi cần sửa và nhập Điểm thi từ 0 đến 10!", true);
                return;
            }

            try
            {
                string maTG = txtMaTG.Text.Trim();
                DataTable dtGoc = kncsdl.DocDuLieu("SELECT mahocvien, makythi FROM THAM_GIA WHERE matg=@ma", new[] { new SqlParameter("@ma", maTG) });
                if (dtGoc.Rows.Count == 0)
                {
                    HienThongBao("Mã tham gia này chưa có trong danh sách. Hãy bấm Thêm mới!", true);
                    return;
                }

                string maHV = dtGoc.Rows[0]["mahocvien"].ToString();
                string maKT = dtGoc.Rows[0]["makythi"].ToString();

                if (!KiemTraQuyenGiangVienTrenBanGhi(maHV, maKT))
                {
                    HienThongBao("Bạn không có quyền sửa điểm của học viên ngoài lớp mình phụ trách!", true);
                    return;
                }

                double diemCC = TinhDiemCC(maHV, maKT);
                txtDiemCC.Text = diemCC.ToString("0.##", CultureInfo.InvariantCulture);

                double diemTB = TinhDiemTrungBinh(diemCC, diemThi);
                string ketQua = diemTB >= 5.0 ? "Đạt" : "Không đạt";

                int coCC = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM CHUNGCHI WHERE matg=@ma", new[] { new SqlParameter("@ma", maTG) }));
                if (coCC > 0 && ketQua == "Không đạt")
                {
                    HienThongBao("Không thể sửa điểm xuống 'Không đạt' vì học viên này đã được cấp chứng chỉ!", true);
                    return;
                }

                string sql = @"UPDATE THAM_GIA 
                               SET diemchuyencan=@cc, diemquatrinh=@thi, diem=@diem, ketqua=@kq 
                               WHERE matg=@ma";
                SqlParameter[] p = {
                    new SqlParameter("@cc", diemCC),
                    new SqlParameter("@thi", diemThi),
                    new SqlParameter("@diem", diemTB),
                    new SqlParameter("@kq", ketQua),
                    new SqlParameter("@ma", maTG)
                };
                kncsdl.ThucThiLenh(sql, p);
                kncsdl.GhiNhatKy(MaNguoiDung, "Sửa điểm thi", $"Mã TG: {maTG} - ĐTB mới: {diemTB:0.0} ({ketQua})");
                HienThongBao($"Cập nhật điểm thành công (CC: {diemCC:0.##} | Thi: {diemThi:0.##} -> ĐTB: {diemTB:0.0} - {ketQua})!");
                TaiDuLieu();
                LamMoiFormTG();
            }
            catch { HienThongBao("Không thể cập nhật điểm lúc này!", true); }
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
                    string maHV = r["mahocvien"].ToString();
                    string maKT = r["makythi"].ToString();

                    if (!KiemTraQuyenGiangVienTrenBanGhi(maHV, maKT))
                    {
                        HienThongBao("Bạn không có quyền chỉnh sửa bản ghi này!", true);
                        return;
                    }

                    txtMaTG.Text = r["matg"].ToString();
                    if (ddlKyThi.Items.FindByValue(maKT) != null)
                        ddlKyThi.SelectedValue = maKT;
                    TaiHocVienTheoKyThi();

                    if (ddlHocVien.Items.FindByValue(maHV) != null)
                        ddlHocVien.SelectedValue = maHV;

                    double diemCC = TinhDiemCC(maHV, maKT);
                    txtDiemCC.Text = diemCC.ToString("0.##", CultureInfo.InvariantCulture);

                    double diemThiGoc = LayDiemThiGoc(r["diemquatrinh"], r["diem"], diemCC);
                    txtDiem.Text = diemThiGoc.ToString("0.##", CultureInfo.InvariantCulture);

                    pnlTB.Visible = false;
                }
            }
        }

        protected void btnXoaTGChinhThuc_Click(object sender, EventArgs e)
        {
            if (LaHocVien) return;

            string ma = hfMaTGXoa.Value;
            if (!string.IsNullOrEmpty(ma))
            {
                try
                {
                    DataTable dtGoc = kncsdl.DocDuLieu("SELECT mahocvien, makythi FROM THAM_GIA WHERE matg=@ma", new[] { new SqlParameter("@ma", ma) });
                    if (dtGoc.Rows.Count == 0) return;

                    string maHV = dtGoc.Rows[0]["mahocvien"].ToString();
                    string maKT = dtGoc.Rows[0]["makythi"].ToString();

                    if (!KiemTraQuyenGiangVienTrenBanGhi(maHV, maKT))
                    {
                        HienThongBao("Bạn không có quyền xóa điểm của học viên ngoài lớp mình phụ trách!", true);
                        return;
                    }

                    int coCC = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM CHUNGCHI WHERE matg=@ma", new[] { new SqlParameter("@ma", ma) }));
                    if (coCC > 0)
                    {
                        HienThongBao("Không thể xóa điểm này vì học viên đã được cấp chứng chỉ!", true);
                        return;
                    }

                    kncsdl.ThucThiLenh("DELETE FROM THAM_GIA WHERE matg=@ma", new[] { new SqlParameter("@ma", ma) });
                    kncsdl.GhiNhatKy(MaNguoiDung, "Xóa điểm thi", $"Mã TG: {ma}");
                    HienThongBao("Đã xóa bản ghi điểm thi thành công.");
                    if (gvThamGia.Rows.Count == 1 && gvThamGia.PageIndex > 0) gvThamGia.PageIndex--;
                    TaiDuLieu();
                    LamMoiFormTG();
                }
                catch { HienThongBao("Không thể xóa bản ghi điểm thi này!", true); }
            }
        }

        protected void btnMoNhapHangLoat_Click(object sender, EventArgs e)
        {
            if (LaHocVien) return;

            string maKT = ddlKyThi.SelectedValue;
            if (string.IsNullOrEmpty(maKT))
            {
                HienThongBao("Vui lòng chọn Kỳ thi trước!", true);
                return;
            }

            litTenKyThiHL.Text = HttpUtility.HtmlEncode(ddlKyThi.SelectedItem.Text);

            string sql = @"SELECT DISTINCT hv.mahocvien, hv.hoten, tg.diem AS DiemTBCSDL, tg.diemquatrinh AS DiemThiGocCSDL
                           FROM HOCVIEN hv
                           JOIN DANGKY dk ON hv.mahocvien = dk.mahocvien
                           JOIN KHOAHOC kh ON dk.makhoahoc = kh.makhoahoc
                           JOIN KYTHI kt ON dk.makhoahoc = kt.makhoahoc
                           LEFT JOIN LOPHOC l ON dk.malop = l.malop
                           LEFT JOIN THAM_GIA tg ON tg.mahocvien = hv.mahocvien AND tg.makythi = kt.makythi
                           WHERE kt.makythi = @kt
                             AND ISNULL(dk.trangthailop, N'') NOT LIKE N'%Hủy lớp%'
                             AND ISNULL(dk.trangthaithanhtoan, N'') NOT IN (N'Chờ hoàn tiền', N'Đã hoàn tiền')
                             AND ISNULL((SELECT SUM(pt.sotienthu) FROM PHIEUTHU pt WHERE pt.madangky = dk.madangky AND ISNULL(pt.noidungthu, N'') NOT LIKE N'Chờ xác nhận%'), 0) >= kh.hocphi
                             AND (SELECT COUNT(*) FROM DIEMDANH dd WHERE dd.mahocvien = hv.mahocvien AND dd.malop = dk.malop AND dd.trangthai IN (N'Vắng phép', N'Vắng không phép')) < 3
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

                if (r["DiemTBCSDL"] != DBNull.Value || r["DiemThiGocCSDL"] != DBNull.Value)
                {
                    double diemThiGoc = LayDiemThiGoc(r["DiemThiGocCSDL"], r["DiemTBCSDL"], diemCC);
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
            if (LaHocVien) return;

            string maKT = ddlKyThi.SelectedValue;
            if (string.IsNullOrEmpty(maKT)) return;

            int daLuu = 0;
            int soLoi = 0;

            foreach (GridViewRow row in gvNhapHangLoat.Rows)
            {
                string maHV = gvNhapHangLoat.DataKeys[row.RowIndex].Value.ToString();
                if (!KiemTraQuyenGiangVienTrenBanGhi(maHV, maKT)) continue;

                string lyDo;
                if (!KiemTraDuDieuKienDuThi(maHV, maKT, out lyDo))
                {
                    soLoi++;
                    continue;
                }

                TextBox txtThi = (TextBox)row.FindControl("txtDiemThiHL");
                if (txtThi == null) continue;
                string rawVal = txtThi.Text.Trim();

                if (string.IsNullOrEmpty(rawVal)) continue;

                double diemThi;
                if (!double.TryParse(rawVal.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out diemThi) || diemThi < 0 || diemThi > 10)
                {
                    soLoi++;
                    continue;
                }

                double diemCC = TinhDiemCC(maHV, maKT);
                double diemTB = TinhDiemTrungBinh(diemCC, diemThi);
                string ketQua = diemTB >= 5.0 ? "Đạt" : "Không đạt";

                object existingMaTG = kncsdl.LayGiaTriDon("SELECT matg FROM THAM_GIA WHERE mahocvien=@hv AND makythi=@kt",
                    new[] { new SqlParameter("@hv", maHV), new SqlParameter("@kt", maKT) });

                if (existingMaTG != null && existingMaTG != DBNull.Value)
                {
                    string maTG = existingMaTG.ToString();
                    int coCC = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM CHUNGCHI WHERE matg=@ma", new[] { new SqlParameter("@ma", maTG) }));
                    if (coCC > 0 && ketQua == "Không đạt")
                    {
                        soLoi++;
                        continue;
                    }

                    kncsdl.ThucThiLenh(
                        "UPDATE THAM_GIA SET diemchuyencan=@cc, diemquatrinh=@thi, diem=@diem, ketqua=@kq WHERE matg=@ma",
                        new[] {
                            new SqlParameter("@cc", diemCC),
                            new SqlParameter("@thi", diemThi),
                            new SqlParameter("@diem", diemTB),
                            new SqlParameter("@kq", ketQua),
                            new SqlParameter("@ma", maTG)
                        });
                }
                else
                {
                    SinhMaTGMoi();
                    kncsdl.ThucThiLenh(
                        "INSERT INTO THAM_GIA(matg, mahocvien, makythi, diemchuyencan, diemquatrinh, diem, ketqua) VALUES(@ma, @hv, @kt, @cc, @thi, @diem, @kq)",
                        new[] {
                            new SqlParameter("@ma", txtMaTG.Text.Trim()),
                            new SqlParameter("@hv", maHV),
                            new SqlParameter("@kt", maKT),
                            new SqlParameter("@cc", diemCC),
                            new SqlParameter("@thi", diemThi),
                            new SqlParameter("@diem", diemTB),
                            new SqlParameter("@kq", ketQua)
                        });
                }
                daLuu++;
            }

            kncsdl.GhiNhatKy(MaNguoiDung, "Nhập điểm hàng loạt", $"Kỳ thi {maKT}: Đã lưu {daLuu} học viên");

            if (soLoi > 0)
                HienThongBao($"Đã lưu {daLuu} học viên; bỏ qua {soLoi} dòng do không đủ điều kiện thi hoặc điểm không hợp lệ!", true);
            else
                HienThongBao($"Đã lưu điểm hàng loạt chính xác cho {daLuu} học viên!");

            TaiDuLieu();
            LamMoiFormTG();
        }
    }
}