using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Web.UI.WebControls;
using N2_QUANLYHOCVIENTRUNGTAMTINHOC.DAL;

namespace N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.PhieuThu
{
    public partial class QuanLyPhieuThu : System.Web.UI.Page
    {
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
                TaiDuLieu();
                SinhMaPTMoi();

                string maDK = Request.QueryString["madk"];
                if (!string.IsNullOrEmpty(maDK) && ddlDangKy.Items.FindByValue(maDK) != null)
                {
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

        private void TaiDuLieu()
        {
            string sqlDK = @"SELECT dk.madangky, dk.madangky + N' - ' + hv.hoten + N' (Còn nợ: ' + FORMAT(kh.hocphi - dk.sotiendong, 'N0') + N'đ)' AS HienThi
                             FROM DANGKY dk JOIN HOCVIEN hv ON dk.mahocvien=hv.mahocvien JOIN KHOAHOC kh ON dk.makhoahoc=kh.makhoahoc";
            ddlDangKy.DataSource = kncsdl.DocDuLieu(sqlDK);
            ddlDangKy.DataTextField = "HienThi";
            ddlDangKy.DataValueField = "madangky";
            ddlDangKy.DataBind();

            string kw = TuKhoaHienTai;
            string sqlPT = @"SELECT pt.*, hv.hoten FROM PHIEUTHU pt 
                             JOIN DANGKY dk ON pt.madangky=dk.madangky 
                             JOIN HOCVIEN hv ON dk.mahocvien=hv.mahocvien
                             WHERE (@kw = '' OR pt.maphieuthu LIKE @kw OR hv.hoten LIKE @kw OR pt.noidungthu LIKE @kw)
                             ORDER BY pt.ngaythu DESC, pt.maphieuthu DESC";
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
                    r["maphieuthu"], r["hoten"]);
            }
            litGoiYPT.Text = sb.ToString();
        }

        private void HienThongBao(string msg, bool laLoi = false)
        {
            pnlTB.Visible = true;
            pnlTB.CssClass = laLoi
                ? "alert alert-danger text-danger fw-semibold alert-dismissible fade show shadow"
                : "alert alert-success text-success fw-semibold alert-dismissible fade show shadow";
            litTB.Text = msg;
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
            if (ddlDangKy.Items.Count > 0) ddlDangKy.SelectedIndex = 0;
            SinhMaPTMoi();
        }

        protected void btnLapPhieu_Click(object sender, EventArgs e)
        {
            decimal soTien;
            if (!LaySoTienHopLe(out soTien)) return;

            SinhMaPTMoi();
            string maDK = ddlDangKy.SelectedValue;

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
                    cmdPT.Parameters.AddWithValue("@nguoi", Session["HoTen"]?.ToString() ?? "Admin");
                    cmdPT.Parameters.AddWithValue("@pt", ddlPhuongThuc.SelectedValue);
                    cmdPT.Parameters.AddWithValue("@nd", txtNoiDung.Text.Trim());
                    cmdPT.Parameters.AddWithValue("@dk", maDK);
                    cmdPT.ExecuteNonQuery();

                    SqlCommand cmdDK = new SqlCommand(@"UPDATE DANGKY 
                        SET sotiendong = sotiendong + @tien,
                            trangthaithanhtoan = CASE WHEN (sotiendong + @tien) >= (SELECT hocphi FROM KHOAHOC WHERE makhoahoc = DANGKY.makhoahoc) 
                                                      THEN N'Đã hoàn tất' ELSE N'Đóng một phần' END
                        WHERE madangky = @dk", conn, tran);
                    cmdDK.Parameters.AddWithValue("@tien", soTien);
                    cmdDK.Parameters.AddWithValue("@dk", maDK);
                    cmdDK.ExecuteNonQuery();

                    tran.Commit();
                    kncsdl.GhiNhatKy(Session["TenDangNhap"]?.ToString(), "Lập phiếu thu", $"Mã PT: {txtMaPT.Text.Trim()} - Số tiền: {soTien:N0}đ");
                    HienThongBao("Lập phiếu thu và cập nhật công nợ thành công!");
                    gvPhieuThu.PageIndex = 0;
                    TaiDuLieu();
                    LamMoiForm();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    HienThongBao("Lỗi Transaction: " + ex.Message, true);
                }
            }
        }

        protected void btnSua_Click(object sender, EventArgs e)
        {
            string maPT = txtMaPT.Text.Trim();
            DataTable dtCu = kncsdl.DocDuLieu("SELECT sotienthu, madangky FROM PHIEUTHU WHERE maphieuthu=@ma", new[] { new SqlParameter("@ma", maPT) });
            if (dtCu.Rows.Count == 0)
            {
                HienThongBao("Phiếu thu này chưa có trong danh sách. Hãy bấm Lập phiếu!", true);
                return;
            }

            decimal soTienMoi;
            if (!LaySoTienHopLe(out soTienMoi)) return;

            decimal soTienCu = Convert.ToDecimal(dtCu.Rows[0]["sotienthu"]);
            string maDKCu = dtCu.Rows[0]["madangky"].ToString();
            string maDKMoi = ddlDangKy.SelectedValue;

            using (SqlConnection conn = new SqlConnection(kncsdl.ChuoiKetNoi))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                try
                {
                    // Trừ lại số tiền cũ ở hồ sơ đăng ký cũ
                    SqlCommand cmdTru = new SqlCommand(@"UPDATE DANGKY 
                        SET sotiendong = CASE WHEN sotiendong - @tienCu < 0 THEN 0 ELSE sotiendong - @tienCu END
                        WHERE madangky = @dkCu", conn, tran);
                    cmdTru.Parameters.AddWithValue("@tienCu", soTienCu);
                    cmdTru.Parameters.AddWithValue("@dkCu", maDKCu);
                    cmdTru.ExecuteNonQuery();

                    // Cập nhật phiếu thu
                    SqlCommand cmdPT = new SqlCommand(@"UPDATE PHIEUTHU SET sotienthu=@tien, phuongthucthanhtoan=@pt, noidungthu=@nd, madangky=@dk WHERE maphieuthu=@ma", conn, tran);
                    cmdPT.Parameters.AddWithValue("@ma", maPT);
                    cmdPT.Parameters.AddWithValue("@tien", soTienMoi);
                    cmdPT.Parameters.AddWithValue("@pt", ddlPhuongThuc.SelectedValue);
                    cmdPT.Parameters.AddWithValue("@nd", txtNoiDung.Text.Trim());
                    cmdPT.Parameters.AddWithValue("@dk", maDKMoi);
                    cmdPT.ExecuteNonQuery();

                    // Cộng số tiền mới vào hồ sơ đăng ký mới và cập nhật trạng thái
                    SqlCommand cmdCong = new SqlCommand(@"UPDATE DANGKY 
                        SET sotiendong = sotiendong + @tienMoi,
                            trangthaithanhtoan = CASE WHEN (sotiendong + @tienMoi) >= (SELECT hocphi FROM KHOAHOC WHERE makhoahoc = DANGKY.makhoahoc) 
                                                      THEN N'Đã hoàn tất' ELSE N'Đóng một phần' END
                        WHERE madangky = @dkMoi", conn, tran);
                    cmdCong.Parameters.AddWithValue("@tienMoi", soTienMoi);
                    cmdCong.Parameters.AddWithValue("@dkMoi", maDKMoi);
                    cmdCong.ExecuteNonQuery();

                    tran.Commit();
                    HienThongBao("Cập nhật phiếu thu thành công!");
                    TaiDuLieu();
                    LamMoiForm();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    HienThongBao("Lỗi cập nhật: " + ex.Message, true);
                }
            }
        }

        protected void btnLamMoi_Click(object sender, EventArgs e)
        {
            LamMoiForm();
            TuKhoaHienTai = "";
            txtTim.Text = "";
            gvPhieuThu.PageIndex = 0;
            TaiDuLieu();
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
                    txtMaPT.Text = r["maphieuthu"].ToString();
                    if (ddlDangKy.Items.FindByValue(r["madangky"].ToString()) != null)
                        ddlDangKy.SelectedValue = r["madangky"].ToString();
                    txtSoTien.Text = r["sotienthu"] != DBNull.Value
                        ? Convert.ToDecimal(r["sotienthu"]).ToString("#,##0", CultureInfo.InvariantCulture)
                        : "0";
                    if (ddlPhuongThuc.Items.FindByValue(r["phuongthucthanhtoan"].ToString()) != null)
                        ddlPhuongThuc.SelectedValue = r["phuongthucthanhtoan"].ToString();
                    txtNoiDung.Text = r["noidungthu"].ToString();
                    pnlTB.Visible = false;
                }
            }
        }

        protected void btnXoaChinhThuc_Click(object sender, EventArgs e)
        {
            string maPT = hfMaPTXoa.Value;
            if (!string.IsNullOrEmpty(maPT))
            {
                DataTable dt = kncsdl.DocDuLieu("SELECT sotienthu, madangky FROM PHIEUTHU WHERE maphieuthu=@ma", new[] { new SqlParameter("@ma", maPT) });
                if (dt.Rows.Count == 0) return;

                decimal soTien = Convert.ToDecimal(dt.Rows[0]["sotienthu"]);
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

                        SqlCommand cmdDK = new SqlCommand(@"UPDATE DANGKY 
                            SET sotiendong = CASE WHEN sotiendong - @tien < 0 THEN 0 ELSE sotiendong - @tien END,
                                trangthaithanhtoan = CASE 
                                    WHEN (CASE WHEN sotiendong - @tien < 0 THEN 0 ELSE sotiendong - @tien END) = 0 THEN N'Chưa đóng'
                                    WHEN (sotiendong - @tien) >= (SELECT hocphi FROM KHOAHOC WHERE makhoahoc = DANGKY.makhoahoc) THEN N'Đã hoàn tất'
                                    ELSE N'Đóng một phần' END
                            WHERE madangky = @dk", conn, tran);
                        cmdDK.Parameters.AddWithValue("@tien", soTien);
                        cmdDK.Parameters.AddWithValue("@dk", maDK);
                        cmdDK.ExecuteNonQuery();

                        tran.Commit();
                        HienThongBao("Đã xóa phiếu thu và cập nhật lại công nợ!");
                        if (gvPhieuThu.Rows.Count == 1 && gvPhieuThu.PageIndex > 0)
                            gvPhieuThu.PageIndex--;
                        TaiDuLieu();
                        LamMoiForm();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        HienThongBao("Lỗi khi xóa: " + ex.Message, true);
                    }
                }
            }
        }

        protected void btnTim_Click(object sender, EventArgs e)
        {
            TuKhoaHienTai = txtTim.Text.Trim();
            gvPhieuThu.PageIndex = 0;
            TaiDuLieu();
            pnlTB.Visible = false;
        }

        protected void gvPhieuThu_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvPhieuThu.PageIndex = e.NewPageIndex;
            TaiDuLieu();
            pnlTB.Visible = false;
        }
    }
}