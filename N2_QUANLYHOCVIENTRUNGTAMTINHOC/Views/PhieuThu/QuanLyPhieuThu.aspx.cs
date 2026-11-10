using System;
using System.Data.SqlClient;
using N2_QUANLYHOCVIENTRUNGTAMTINHOC.DAL;

namespace N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.PhieuThu
{
    public partial class QuanLyPhieuThu : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) TaiDuLieu();
        }

        private void TaiDuLieu()
        {
            string sqlDK = @"SELECT dk.madangky, dk.madangky + ' - ' + hv.hoten + ' (Còn nợ: ' + FORMAT(kh.hocphi - dk.sotiendong, 'N0') + 'đ)' AS HienThi
                             FROM DANGKY dk JOIN HOCVIEN hv ON dk.mahocvien=hv.mahocvien JOIN KHOAHOC kh ON dk.makhoahoc=kh.makhoahoc";
            ddlDangKy.DataSource = kncsdl.DocDuLieu(sqlDK);
            ddlDangKy.DataTextField = "HienThi"; ddlDangKy.DataValueField = "madangky"; ddlDangKy.DataBind();

            string sqlPT = @"SELECT pt.*, hv.hoten FROM PHIEUTHU pt 
                             JOIN DANGKY dk ON pt.madangky=dk.madangky 
                             JOIN HOCVIEN hv ON dk.mahocvien=hv.mahocvien ORDER BY pt.ngaythu DESC";
            gvPhieuThu.DataSource = kncsdl.DocDuLieu(sqlPT);
            gvPhieuThu.DataBind();
        }

        protected void btnLapPhieu_Click(object sender, EventArgs e)
        {
            decimal soTien = decimal.Parse(string.IsNullOrEmpty(txtSoTien.Text) ? "0" : txtSoTien.Text);
            string maDK = ddlDangKy.SelectedValue;

            using (SqlConnection conn = new SqlConnection(kncsdl.ChuoiKetNoi))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                try
                {
                    // 1. Thêm phiếu thu
                    SqlCommand cmdPT = new SqlCommand(@"INSERT INTO PHIEUTHU(maphieuthu, ngaythu, sotienthu, nguoithu, phuongthucthanhtoan, noidungthu, madangky)
                                                        VALUES(@ma, GETDATE(), @tien, @nguoi, @pt, @nd, @dk)", conn, tran);
                    cmdPT.Parameters.AddWithValue("@ma", txtMaPT.Text.Trim());
                    cmdPT.Parameters.AddWithValue("@tien", soTien);
                    cmdPT.Parameters.AddWithValue("@nguoi", Session["HoTen"]?.ToString() ?? "Admin");
                    cmdPT.Parameters.AddWithValue("@pt", ddlPhuongThuc.SelectedValue);
                    cmdPT.Parameters.AddWithValue("@nd", txtNoiDung.Text.Trim());
                    cmdPT.Parameters.AddWithValue("@dk", maDK);
                    cmdPT.ExecuteNonQuery();

                    // 2. Cộng dồn tiền đóng & tự động cập nhật trạng thái thanh toán trong bảng DANGKY
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
                    pnlTB.Visible = true; litTB.Text = "Lập phiếu thu và cập nhật công nợ thành công!";
                    TaiDuLieu();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    pnlTB.Visible = true; litTB.Text = "Lỗi Transaction: " + ex.Message;
                }
            }
        }
    }
}