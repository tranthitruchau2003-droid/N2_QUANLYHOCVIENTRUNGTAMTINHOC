using System;
using System.Data.SqlClient;
using System.Globalization;
using N2_QUANLYHOCVIENTRUNGTAMTINHOC.DAL;

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
            gvKyThi.DataSource = kncsdl.DocDuLieu("SELECT kt.*, kh.tenkhoahoc FROM KYTHI kt JOIN KHOAHOC kh ON kt.makhoahoc=kh.makhoahoc");
            gvKyThi.DataBind();

            ddlKyThi.DataSource = kncsdl.DocDuLieu("SELECT makythi, makythi + ' (' + nienkhoa + ')' AS HienThi FROM KYTHI");
            ddlKyThi.DataTextField = "HienThi"; ddlKyThi.DataValueField = "makythi"; ddlKyThi.DataBind();

            gvThamGia.DataSource = kncsdl.DocDuLieu("SELECT tg.*, hv.hoten FROM THAM_GIA tg JOIN HOCVIEN hv ON tg.mahocvien=hv.mahocvien");
            gvThamGia.DataBind();
        }

        protected void btnTaoKyThi_Click(object sender, EventArgs e)
        {
            string sql = "INSERT INTO KYTHI(makythi, nienkhoa, ngaythi, makhoahoc) VALUES(@ma, @nk, @ngay, @kh)";
            SqlParameter[] p = {
                new SqlParameter("@ma", txtMaKT.Text.Trim()),
                new SqlParameter("@nk", txtNienKhoa.Text.Trim()),
                new SqlParameter("@ngay", txtNgayThi.Text),
                new SqlParameter("@kh", ddlKhoaHoc.SelectedValue)
            };
            kncsdl.ThucThiLenh(sql, p);
            pnlTB.Visible = true; litTB.Text = "Đã tạo kỳ thi mới!";
            TaiDuLieu();
        }

        protected void btnLuuDiem_Click(object sender, EventArgs e)
        {
            double diem = double.Parse(txtDiem.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            string ketQua = diem >= 5.0 ? "Đạt" : "Không đạt";

            string sql = @"IF EXISTS (SELECT 1 FROM THAM_GIA WHERE matg=@ma)
                           UPDATE THAM_GIA SET diem=@diem, ketqua=@kq WHERE matg=@ma
                           ELSE
                           INSERT INTO THAM_GIA(matg, mahocvien, makythi, diem, ketqua) VALUES(@ma, @hv, @kt, @diem, @kq)";
            SqlParameter[] p = {
                new SqlParameter("@ma", txtMaTG.Text.Trim()),
                new SqlParameter("@hv", ddlHocVien.SelectedValue),
                new SqlParameter("@kt", ddlKyThi.SelectedValue),
                new SqlParameter("@diem", diem),
                new SqlParameter("@kq", ketQua)
            };
            kncsdl.ThucThiLenh(sql, p);
            pnlTB.Visible = true; litTB.Text = $"Đã lưu điểm ({diem}) - Kết quả: {ketQua}!";
            TaiDuLieu();
        }
    }
}