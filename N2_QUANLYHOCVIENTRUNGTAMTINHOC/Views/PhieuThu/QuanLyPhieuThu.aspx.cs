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

namespace N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.PhieuThu
{
    public partial class QuanLyPhieuThu : System.Web.UI.Page
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
            get { return ViewState["TuKhoaTimPT"]?.ToString() ?? ""; }
            set { ViewState["TuKhoaTimPT"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                TuKhoaHienTai = "";
                txtTim.Text = "";
                TaiDropdownDangKy();
                TaiDanhSachPhieuThu();
                SinhMaPTMoi();

                string maDK = Request.QueryString["madk"];
                if (!string.IsNullOrEmpty(maDK))
                {
                    TaiDropdownDangKy(maDK);
                    if (ddlDangKy.Items.FindByValue(maDK) != null)
                        ddlDangKy.SelectedValue = maDK;
                }
            }
        }

        private void SinhMaPTMoi()
        {
            int soLuong = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM PHIEUTHU")) + 1;
            string maPT = "PT" + soLuong.ToString("D2");
            while (Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM PHIEUTHU WHERE maphieuthu=@ma", new[] { new SqlParameter("@ma", maPT) })) > 0)
            {
                soLuong++;
                maPT = "PT" + soLuong.ToString("D2");
            }
            txtMaPT.Text = maPT;
        }

        private void TaiDropdownDangKy(string giuMaDK = "")
        {
            string sqlDK = @"SELECT dk.madangky, 
                                    dk.madangky + N' - ' + hv.hoten + N' (Còn nợ: ' + FORMAT(kh.hocphi - ISNULL(SUM(CASE WHEN ISNULL(pt.noidungthu, N'') NOT LIKE N'Chờ xác nhận%' THEN pt.sotienthu ELSE 0 END), 0), 'N0') + N'đ)' AS HienThi
                             FROM DANGKY dk 
                             JOIN HOCVIEN hv ON dk.mahocvien = hv.mahocvien 
                             JOIN KHOAHOC kh ON dk.makhoahoc = kh.makhoahoc
                             LEFT JOIN PHIEUTHU pt ON dk.madangky = pt.madangky
                             WHERE (ISNULL(dk.trangthailop, N'') NOT LIKE N'%Hủy lớp%' AND ISNULL(dk.trangthaithanhtoan, N'') NOT IN (N'Chờ hoàn tiền', N'Đã hoàn tiền'))
                                OR dk.madangky = @giu
                             GROUP BY dk.madangky, hv.hoten, kh.hocphi, dk.trangthailop, dk.trangthaithanhtoan
                             HAVING ((ISNULL(dk.trangthailop, N'') NOT LIKE N'%Hủy lớp%' AND ISNULL(dk.trangthaithanhtoan, N'') NOT IN (N'Chờ hoàn tiền', N'Đã hoàn tiền'))
                                     AND (kh.hocphi - ISNULL(SUM(CASE WHEN ISNULL(pt.noidungthu, N'') NOT LIKE N'Chờ xác nhận%' THEN pt.sotienthu ELSE 0 END), 0)) > 0)
                                 OR dk.madangky = @giu
                             ORDER BY dk.madangky DESC";
            ddlDangKy.DataSource = kncsdl.DocDuLieu(sqlDK, new[] { new SqlParameter("@giu", giuMaDK ?? "") });
            ddlDangKy.DataTextField = "HienThi";
            ddlDangKy.DataValueField = "madangky";
            ddlDangKy.DataBind();
        }

        private void TaiDanhSachPhieuThu()
        {
            string kw = TuKhoaHienTai;
            string sqlPT = @"SELECT pt.*, hv.hoten FROM PHIEUTHU pt 
                             JOIN DANGKY dk ON pt.madangky=dk.madangky 
                             JOIN HOCVIEN hv ON dk.mahocvien=hv.mahocvien
                             WHERE (@kw = '' OR pt.maphieuthu COLLATE Latin1_General_CI_AI LIKE @kw 
                                             OR hv.hoten COLLATE Latin1_General_CI_AI LIKE @kw 
                                             OR pt.noidungthu COLLATE Latin1_General_CI_AI LIKE @kw 
                                             OR dk.madangky COLLATE Latin1_General_CI_AI LIKE @kw)
                             ORDER BY CASE WHEN ISNULL(pt.noidungthu, N'') LIKE N'Chờ xác nhận%' THEN 0 ELSE 1 END, pt.ngaythu DESC, pt.maphieuthu DESC";
            gvPhieuThu.DataSource = kncsdl.DocDuLieu(sqlPT, new[] { new SqlParameter("@kw", string.IsNullOrEmpty(kw) ? "" : "%" + kw + "%") });
            gvPhieuThu.DataBind();

            TaiGoiYTimKiem();
        }

        private void TaiGoiYTimKiem()
        {
            DataTable dt = kncsdl.DocDuLieu(@"SELECT pt.maphieuthu, hv.hoten FROM PHIEUTHU pt 
                                              JOIN DANGKY dk ON pt.madangky=dk.madangky 
                                              JOIN HOCVIEN hv ON dk.mahocvien=hv.mahocvien 
                                              ORDER BY pt.maphieuthu DESC");
            StringBuilder sb = new StringBuilder();
            foreach (DataRow r in dt.Rows)
            {
                sb.AppendFormat("<button type=\"button\" class=\"list-group-item list-group-item-action py-2 px-2 small goi-y-item\" data-value=\"{0}\"><strong>{0}</strong> - {1}</button>",
                    HttpUtility.HtmlAttributeEncode(r["maphieuthu"].ToString()),
                    HttpUtility.HtmlEncode(r["hoten"].ToString()));
            }
            litGoiYPT.Text = sb.ToString();
        }

        private void DongBoCongNoDangKy(string maDK, SqlConnection conn, SqlTransaction tran)
        {
            string sql = @"UPDATE DANGKY 
                           SET sotiendong = CASE 
                                   WHEN ISNULL(trangthaithanhtoan, N'') = N'Đã hoàn tiền' OR ISNULL(trangthailop, N'') LIKE N'%Đã hoàn tiền%' THEN 0
                                   ELSE ISNULL((SELECT SUM(sotienthu) FROM PHIEUTHU WHERE madangky = @dk AND ISNULL(noidungthu, N'') NOT LIKE N'Chờ xác nhận%'), 0)
                               END,
                               trangthaithanhtoan = CASE 
                                   WHEN ISNULL(trangthaithanhtoan, N'') IN (N'Chờ hoàn tiền', N'Đã hoàn tiền') OR ISNULL(trangthailop, N'') LIKE N'%Chờ hoàn%' OR ISNULL(trangthailop, N'') LIKE N'%Đã hoàn tiền%' THEN trangthaithanhtoan
                                   WHEN EXISTS (SELECT 1 FROM PHIEUTHU WHERE madangky = @dk AND ISNULL(noidungthu, N'') LIKE N'Chờ xác nhận%') THEN N'Chờ xác nhận'
                                   WHEN ISNULL((SELECT SUM(sotienthu) FROM PHIEUTHU WHERE madangky = @dk AND ISNULL(noidungthu, N'') NOT LIKE N'Chờ xác nhận%'), 0) <= 0 THEN N'Chưa thanh toán'
                                   WHEN ISNULL((SELECT SUM(sotienthu) FROM PHIEUTHU WHERE madangky = @dk AND ISNULL(noidungthu, N'') NOT LIKE N'Chờ xác nhận%'), 0) >= (SELECT hocphi FROM KHOAHOC WHERE makhoahoc = DANGKY.makhoahoc) THEN N'Đã hoàn tất'
                                   ELSE N'Đóng một phần'
                               END
                           WHERE madangky = @dk";
            SqlCommand cmd = new SqlCommand(sql, conn, tran);
            cmd.Parameters.AddWithValue("@dk", maDK);
            cmd.ExecuteNonQuery();
        }

        private void HienThongBao(string msg, bool laLoi = false)
        {
            pnlTB.Visible = true;
            pnlTB.CssClass = laLoi
                ? "alert alert-danger text-danger fw-semibold alert-dismissible fade show shadow"
                : "alert alert-success text-success fw-semibold alert-dismissible fade show shadow";
            litTB.Text = HttpUtility.HtmlEncode(msg);
        }

        private bool LaySoTienHopLe(out decimal soTien)
        {
            soTien = 0;
            string raw = txtSoTien.Text.Replace(",", "").Replace(".", "").Trim();
            if (string.IsNullOrEmpty(raw) || !decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out soTien) || soTien <= 0)
            {
                HienThongBao("Vui lòng nhập Số tiền thu hợp lệ (lớn hơn 0)!", true);
                txtSoTien.Focus();
                return false;
            }
            return true;
        }

        private void LamMoiForm()
        {
            txtSoTien.Text = "";
            txtNoiDung.Text = "";
            ddlPhuongThuc.SelectedIndex = 0;
            TaiDropdownDangKy();
            SinhMaPTMoi();
        }

        protected void btnLapPhieu_Click(object sender, EventArgs e)
        {
            if (!LaQuanTriHoacNV) return;

            if (string.IsNullOrEmpty(ddlDangKy.SelectedValue))
            {
                HienThongBao("Không có hồ sơ đăng ký nào đang nợ học phí!", true);
                return;
            }

            decimal soTien;
            if (!LaySoTienHopLe(out soTien)) return;

            string maDK = ddlDangKy.SelectedValue;

            string ttLop = kncsdl.LayGiaTriDon("SELECT ISNULL(trangthailop, N'') FROM DANGKY WHERE madangky=@dk", new[] { new SqlParameter("@dk", maDK) })?.ToString() ?? "";
            if (ttLop.Contains("Hủy lớp"))
            {
                HienThongBao("Hồ sơ này thuộc lớp đã hủy, không thể lập phiếu thu học phí!", true);
                return;
            }

            object conNoObj = kncsdl.LayGiaTriDon(@"SELECT (kh.hocphi - ISNULL((SELECT SUM(sotienthu) FROM PHIEUTHU WHERE madangky=@dk AND ISNULL(noidungthu, N'') NOT LIKE N'Chờ xác nhận%'), 0))
                                                   FROM DANGKY dk JOIN KHOAHOC kh ON dk.makhoahoc = kh.makhoahoc WHERE dk.madangky=@dk",
                new[] { new SqlParameter("@dk", maDK) });
            decimal conNo = conNoObj != null ? Convert.ToDecimal(conNoObj) : 0;
            if (soTien > conNo)
            {
                HienThongBao($"Số tiền thu ({soTien:N0} đ) không được vượt quá số còn nợ ({conNo:N0} đ)!", true);
                return;
            }

            SinhMaPTMoi();

            using (SqlConnection conn = new SqlConnection(kncsdl.ChuoiKetNoi))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                try
                {
                    SqlCommand cmdPT = new SqlCommand(@"INSERT INTO PHIEUTHU(maphieuthu, ngaythu, sotienthu, nguoithu, phuongthucthanhtoan, noidungthu, madangky)
                                                        VALUES(@ma, GETDATE(), @tien, @nguoi, @pt, @nd, @dk)", conn, tran);
                    cmdPT.Parameters.AddWithValue("@ma", txtMaPT.Text.Trim());
                    cmdPT.Parameters.AddWithValue("@tien", soTien);
                    cmdPT.Parameters.AddWithValue("@nguoi", Session["HoTen"]?.ToString() ?? "Nhân viên");
                    cmdPT.Parameters.AddWithValue("@pt", ddlPhuongThuc.SelectedValue);
                    cmdPT.Parameters.AddWithValue("@nd", string.IsNullOrWhiteSpace(txtNoiDung.Text) ? "Thu học phí trực tiếp" : txtNoiDung.Text.Trim());
                    cmdPT.Parameters.AddWithValue("@dk", maDK);
                    cmdPT.ExecuteNonQuery();

                    DongBoCongNoDangKy(maDK, conn, tran);

                    tran.Commit();
                    kncsdl.GhiNhatKy(Session["TenDangNhap"]?.ToString(), "Lập phiếu thu", $"Mã PT: {txtMaPT.Text.Trim()} - Số tiền: {soTien:N0}đ");
                    HienThongBao("Lập phiếu thu và cập nhật công nợ thành công!");
                    gvPhieuThu.PageIndex = 0;
                    TaiDanhSachPhieuThu();
                    LamMoiForm();
                }
                catch
                {
                    tran.Rollback();
                    HienThongBao("Lỗi khi lập phiếu thu. Vui lòng thử lại!", true);
                }
            }
        }

        protected void btnSua_Click(object sender, EventArgs e)
        {
            if (!LaQuanTriHoacNV) return;

            string maPT = txtMaPT.Text.Trim();
            DataTable dtCu = kncsdl.DocDuLieu("SELECT sotienthu, madangky, phuongthucthanhtoan FROM PHIEUTHU WHERE maphieuthu=@ma", new[] { new SqlParameter("@ma", maPT) });
            if (dtCu.Rows.Count == 0)
            {
                HienThongBao("Phiếu thu này chưa có trong danh sách. Hãy bấm Lập phiếu!", true);
                return;
            }

            if (Convert.ToDecimal(dtCu.Rows[0]["sotienthu"]) < 0 || dtCu.Rows[0]["phuongthucthanhtoan"].ToString() == "Hoàn tiền học phí")
            {
                HienThongBao("Không được phép sửa phiếu chi hoàn tiền học phí!", true);
                return;
            }

            decimal soTienMoi;
            if (!LaySoTienHopLe(out soTienMoi)) return;

            string maDKCu = dtCu.Rows[0]["madangky"].ToString();
            string maDKMoi = ddlDangKy.SelectedValue;

            object conNoObj = kncsdl.LayGiaTriDon(@"SELECT (kh.hocphi - ISNULL((SELECT SUM(sotienthu) FROM PHIEUTHU WHERE madangky=@dk AND maphieuthu<>@pt AND ISNULL(noidungthu, N'') NOT LIKE N'Chờ xác nhận%'), 0))
                                                   FROM DANGKY dk JOIN KHOAHOC kh ON dk.makhoahoc = kh.makhoahoc WHERE dk.madangky=@dk",
                new[] { new SqlParameter("@dk", maDKMoi), new SqlParameter("@pt", maPT) });
            decimal conNoToiDa = conNoObj != null ? Convert.ToDecimal(conNoObj) : 0;
            if (soTienMoi > conNoToiDa)
            {
                HienThongBao($"Số tiền sửa ({soTienMoi:N0} đ) không được vượt quá công nợ tối đa ({conNoToiDa:N0} đ)!", true);
                return;
            }

            using (SqlConnection conn = new SqlConnection(kncsdl.ChuoiKetNoi))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                try
                {
                    SqlCommand cmdPT = new SqlCommand(@"UPDATE PHIEUTHU SET sotienthu=@tien, phuongthucthanhtoan=@pt, noidungthu=@nd, madangky=@dk WHERE maphieuthu=@ma", conn, tran);
                    cmdPT.Parameters.AddWithValue("@ma", maPT);
                    cmdPT.Parameters.AddWithValue("@tien", soTienMoi);
                    cmdPT.Parameters.AddWithValue("@pt", ddlPhuongThuc.SelectedValue);
                    cmdPT.Parameters.AddWithValue("@nd", txtNoiDung.Text.Trim());
                    cmdPT.Parameters.AddWithValue("@dk", maDKMoi);
                    cmdPT.ExecuteNonQuery();

                    DongBoCongNoDangKy(maDKCu, conn, tran);
                    if (maDKMoi != maDKCu)
                        DongBoCongNoDangKy(maDKMoi, conn, tran);

                    tran.Commit();
                    kncsdl.GhiNhatKy(Session["TenDangNhap"]?.ToString(), "Sửa phiếu thu", $"Mã PT: {maPT} - Số tiền mới: {soTienMoi:N0}đ");
                    HienThongBao("Cập nhật phiếu thu thành công!");
                    TaiDanhSachPhieuThu();
                    LamMoiForm();
                }
                catch
                {
                    tran.Rollback();
                    HienThongBao("Lỗi khi cập nhật phiếu thu!", true);
                }
            }
        }

        protected void btnLamMoi_Click(object sender, EventArgs e)
        {
            LamMoiForm();
            TuKhoaHienTai = "";
            txtTim.Text = "";
            gvPhieuThu.PageIndex = 0;
            TaiDanhSachPhieuThu();
            pnlTB.Visible = false;
        }

        protected void gvPhieuThu_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Chon")
            {
                DataTable dt = kncsdl.DocDuLieu("SELECT * FROM PHIEUTHU WHERE maphieuthu=@ma", new[] { new SqlParameter("@ma", e.CommandArgument.ToString()) });
                if (dt.Rows.Count > 0)
                {
                    DataRow r = dt.Rows[0];
                    string maDK = r["madangky"].ToString();
                    TaiDropdownDangKy(maDK);

                    txtMaPT.Text = r["maphieuthu"].ToString();
                    if (ddlDangKy.Items.FindByValue(maDK) != null)
                        ddlDangKy.SelectedValue = maDK;
                    txtSoTien.Text = r["sotienthu"] != DBNull.Value
                        ? Convert.ToDecimal(r["sotienthu"]).ToString("#,##0", CultureInfo.InvariantCulture)
                        : "0";
                    if (ddlPhuongThuc.Items.FindByValue(r["phuongthucthanhtoan"].ToString()) != null)
                        ddlPhuongThuc.SelectedValue = r["phuongthucthanhtoan"].ToString();
                    txtNoiDung.Text = r["noidungthu"].ToString();
                    pnlTB.Visible = false;
                }
            }
            else if (e.CommandName == "XemAnhDuyet")
            {
                string maPT = e.CommandArgument.ToString();
                DataTable dt = kncsdl.DocDuLieu(@"SELECT pt.*, hv.hoten 
                                                  FROM PHIEUTHU pt 
                                                  JOIN DANGKY dk ON pt.madangky = dk.madangky 
                                                  JOIN HOCVIEN hv ON dk.mahocvien = hv.mahocvien 
                                                  WHERE pt.maphieuthu = @ma", new[] { new SqlParameter("@ma", maPT) });
                if (dt.Rows.Count > 0)
                {
                    DataRow r = dt.Rows[0];
                    string nd = r["noidungthu"].ToString();
                    bool dangCho = nd.StartsWith("Chờ xác nhận");

                    hfMaPTDuyet.Value = maPT;
                    litDuyetMaPT.Text = HttpUtility.HtmlEncode(maPT);
                    litDuyetMaDK.Text = HttpUtility.HtmlEncode(r["madangky"].ToString());
                    litDuyetHocVien.Text = HttpUtility.HtmlEncode(r["hoten"].ToString());
                    litDuyetSoTien.Text = Convert.ToDecimal(r["sotienthu"]).ToString("N0") + " VNĐ";
                    litDuyetTrangThai.Text = dangCho
                        ? "<span class='badge bg-warning text-dark'>Chờ xác nhận ảnh</span>"
                        : "<span class='badge bg-success'>Đã đóng học phí</span>";

                    var match = Regex.Match(nd, @"Ảnh:\s*([^\)]+)");
                    string fileName = match.Success ? match.Groups[1].Value.Trim() : "";
                    string imgUrl = !string.IsNullOrEmpty(fileName) ? ResolveUrl("~/Uploads/BienLai/" + fileName) : "";
                    imgBienLaiDuyet.ImageUrl = imgUrl;
                    lnkXemAnhGoc.NavigateUrl = imgUrl;

                    btnXacNhanDaDong.Visible = dangCho;
                    btnTuChoiBienLai.Visible = dangCho;

                    pnlTB.Visible = false;
                    ScriptManager.RegisterStartupScript(this, GetType(), "modal_duyet", "moModalDuyetBienLai();", true);
                }
            }
        }

        protected void btnXacNhanDaDong_Click(object sender, EventArgs e)
        {
            if (!LaQuanTriHoacNV) return;

            string maPT = hfMaPTDuyet.Value;
            if (string.IsNullOrEmpty(maPT)) return;

            DataTable dt = kncsdl.DocDuLieu("SELECT madangky, sotienthu, noidungthu FROM PHIEUTHU WHERE maphieuthu=@ma", new[] { new SqlParameter("@ma", maPT) });
            if (dt.Rows.Count == 0) return;

            string maDK = dt.Rows[0]["madangky"].ToString();
            decimal soTien = Convert.ToDecimal(dt.Rows[0]["sotienthu"]);
            string ndCu = dt.Rows[0]["noidungthu"].ToString();

            if (!ndCu.StartsWith("Chờ xác nhận"))
            {
                HienThongBao("Phiếu thu này đã được xử lý từ trước!", true);
                return;
            }

            object conNoObj = kncsdl.LayGiaTriDon(@"SELECT (kh.hocphi - ISNULL((SELECT SUM(sotienthu) FROM PHIEUTHU WHERE madangky=@dk AND maphieuthu<>@pt AND ISNULL(noidungthu, N'') NOT LIKE N'Chờ xác nhận%'), 0))
                                                   FROM DANGKY dk JOIN KHOAHOC kh ON dk.makhoahoc = kh.makhoahoc WHERE dk.madangky=@dk",
                new[] { new SqlParameter("@dk", maDK), new SqlParameter("@pt", maPT) });
            decimal conNoToiDa = conNoObj != null ? Convert.ToDecimal(conNoObj) : 0;

            if (soTien > conNoToiDa)
            {
                HienThongBao($"Duyệt phiếu sẽ làm tổng số tiền thu vượt quá học phí khóa học (Còn nợ: {conNoToiDa:N0} đ). Vui lòng từ chối biên lai hoặc sửa lại số tiền!", true);
                return;
            }

            string ndMoi = ndCu.Replace("Chờ xác nhận CK QR", "Đã xác nhận CK QR");
            if (ndMoi == ndCu) ndMoi = "Đã xác nhận thanh toán - " + ndCu;

            using (SqlConnection conn = new SqlConnection(kncsdl.ChuoiKetNoi))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                try
                {
                    SqlCommand cmdPT = new SqlCommand("UPDATE PHIEUTHU SET noidungthu=@nd, nguoithu=@nguoi, ngaythu=GETDATE() WHERE maphieuthu=@ma", conn, tran);
                    cmdPT.Parameters.AddWithValue("@nd", ndMoi);
                    cmdPT.Parameters.AddWithValue("@nguoi", Session["HoTen"]?.ToString() ?? "Nhân viên");
                    cmdPT.Parameters.AddWithValue("@ma", maPT);
                    cmdPT.ExecuteNonQuery();

                    DongBoCongNoDangKy(maDK, conn, tran);

                    tran.Commit();
                    kncsdl.GhiNhatKy(Session["TenDangNhap"]?.ToString(), "Xác nhận biên lai QR", $"Duyệt phiếu {maPT} ({soTien:N0}đ) cho hồ sơ {maDK}");
                    HienThongBao($"Đã xác nhận ảnh biên lai! Phiếu {maPT} ({soTien:N0} đ) đã chuyển sang trạng thái ĐÃ ĐÓNG.");
                    TaiDropdownDangKy();
                    TaiDanhSachPhieuThu();
                }
                catch
                {
                    tran.Rollback();
                    HienThongBao("Có lỗi khi xác nhận thanh toán!", true);
                }
            }
        }

        protected void btnTuChoiBienLai_Click(object sender, EventArgs e)
        {
            if (!LaQuanTriHoacNV) return;

            string maPT = hfMaPTDuyet.Value;
            if (string.IsNullOrEmpty(maPT)) return;

            DataTable dt = kncsdl.DocDuLieu("SELECT madangky FROM PHIEUTHU WHERE maphieuthu=@ma", new[] { new SqlParameter("@ma", maPT) });
            if (dt.Rows.Count == 0) return;

            string maDK = dt.Rows[0]["madangky"].ToString();

            using (SqlConnection conn = new SqlConnection(kncsdl.ChuoiKetNoi))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                try
                {
                    SqlCommand cmdXoa = new SqlCommand("DELETE FROM PHIEUTHU WHERE maphieuthu=@ma", conn, tran);
                    cmdXoa.Parameters.AddWithValue("@ma", maPT);
                    cmdXoa.ExecuteNonQuery();

                    DongBoCongNoDangKy(maDK, conn, tran);

                    tran.Commit();
                    kncsdl.GhiNhatKy(Session["TenDangNhap"]?.ToString(), "Từ chối biên lai QR", $"Từ chối phiếu chờ {maPT} của hồ sơ {maDK}");
                    HienThongBao($"Đã từ chối biên lai {maPT}. Học viên có thể gửi lại ảnh chuyển khoản khác.", true);
                    TaiDropdownDangKy();
                    TaiDanhSachPhieuThu();
                }
                catch
                {
                    tran.Rollback();
                    HienThongBao("Có lỗi khi từ chối biên lai!", true);
                }
            }
        }

        protected void btnXoaChinhThuc_Click(object sender, EventArgs e)
        {
            if (!LaQuanTriHoacNV) return;

            string maPT = hfMaPTXoa.Value;
            if (!string.IsNullOrEmpty(maPT))
            {
                DataTable dt = kncsdl.DocDuLieu("SELECT sotienthu, madangky, phuongthucthanhtoan FROM PHIEUTHU WHERE maphieuthu=@ma", new[] { new SqlParameter("@ma", maPT) });
                if (dt.Rows.Count == 0) return;

                if (Convert.ToDecimal(dt.Rows[0]["sotienthu"]) < 0 || dt.Rows[0]["phuongthucthanhtoan"].ToString() == "Hoàn tiền học phí")
                {
                    HienThongBao("Không được phép xóa phiếu chi hoàn tiền học phí!", true);
                    return;
                }

                string maDK = dt.Rows[0]["madangky"].ToString();

                using (SqlConnection conn = new SqlConnection(kncsdl.ChuoiKetNoi))
                {
                    conn.Open();
                    SqlTransaction tran = conn.BeginTransaction();
                    try
                    {
                        SqlCommand cmdXoa = new SqlCommand("DELETE FROM PHIEUTHU WHERE maphieuthu=@ma", conn, tran);
                        cmdXoa.Parameters.AddWithValue("@ma", maPT);
                        cmdXoa.ExecuteNonQuery();

                        DongBoCongNoDangKy(maDK, conn, tran);

                        tran.Commit();
                        kncsdl.GhiNhatKy(Session["TenDangNhap"]?.ToString(), "Xóa phiếu thu", $"Xóa phiếu {maPT} của hồ sơ {maDK}");
                        HienThongBao("Đã xóa phiếu thu và cập nhật lại công nợ!");
                        if (gvPhieuThu.Rows.Count == 1 && gvPhieuThu.PageIndex > 0)
                            gvPhieuThu.PageIndex--;
                        TaiDropdownDangKy();
                        TaiDanhSachPhieuThu();
                        LamMoiForm();
                    }
                    catch
                    {
                        tran.Rollback();
                        HienThongBao("Lỗi khi xóa phiếu thu!", true);
                    }
                }
            }
        }

        protected void btnTim_Click(object sender, EventArgs e)
        {
            TuKhoaHienTai = txtTim.Text.Trim();
            gvPhieuThu.PageIndex = 0;
            TaiDanhSachPhieuThu();
            pnlTB.Visible = false;
        }

        protected void gvPhieuThu_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvPhieuThu.PageIndex = e.NewPageIndex;
            TaiDanhSachPhieuThu();
            pnlTB.Visible = false;
        }
    }
}