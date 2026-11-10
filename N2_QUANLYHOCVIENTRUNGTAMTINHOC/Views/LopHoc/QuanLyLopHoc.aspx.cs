using System;
using System.Data.SqlClient;
using N2_QUANLYHOCVIENTRUNGTAMTINHOC.DAL;

namespace N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.LopHoc
{
    public partial class QuanLyLopHoc : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ddlKhoaHoc.DataSource = kncsdl.DocDuLieu("SELECT makhoahoc, tenkhoahoc FROM KHOAHOC");
                ddlKhoaHoc.DataTextField = "tenkhoahoc"; ddlKhoaHoc.DataValueField = "makhoahoc"; ddlKhoaHoc.DataBind();

                ddlGiangVien.DataSource = kncsdl.DocDuLieu("SELECT magiangvien, hoten FROM GIANGVIEN");
                ddlGiangVien.DataTextField = "hoten"; ddlGiangVien.DataValueField = "magiangvien"; ddlGiangVien.DataBind();

                TaiDuLieu();
            }
        }

        private void TaiDuLieu()
        {
            gvLop.DataSource = kncsdl.DocDuLieu(@"SELECT l.*, kh.tenkhoahoc, gv.hoten 
                                                  FROM LOPHOC l JOIN KHOAHOC kh ON l.makhoahoc=kh.makhoahoc 
                                                  JOIN GIANGVIEN gv ON l.magiangvien=gv.magiangvien");
            gvLop.DataBind();

            ddlLopLich.DataSource = kncsdl.DocDuLieu("SELECT malop, tenlop FROM LOPHOC");
            ddlLopLich.DataTextField = "tenlop"; ddlLopLich.DataValueField = "malop"; ddlLopLich.DataBind();

            gvLich.DataSource = kncsdl.DocDuLieu("SELECT lh.*, l.tenlop FROM LICHHOC lh JOIN LOPHOC l ON lh.malop=l.malop");
            gvLich.DataBind();
        }

        protected void btnThemLop_Click(object sender, EventArgs e)
        {
            string sql = "INSERT INTO LOPHOC(malop, tenlop, ngaykhaigiang, makhoahoc, magiangvien) VALUES(@ma, @ten, @ngay, @kh, @gv)";
            SqlParameter[] p = {
                new SqlParameter("@ma", txtMaLop.Text.Trim()),
                new SqlParameter("@ten", txtTenLop.Text.Trim()),
                new SqlParameter("@ngay", txtNgayKG.Text),
                new SqlParameter("@kh", ddlKhoaHoc.SelectedValue),
                new SqlParameter("@gv", ddlGiangVien.SelectedValue)
            };
            kncsdl.ThucThiLenh(sql, p);
            pnlTB.Visible = true; litTB.Text = "Tạo lớp học mới thành công!";
            TaiDuLieu();
        }

        protected void btnThemLich_Click(object sender, EventArgs e)
        {
            string sql = "INSERT INTO LICHHOC(malichhoc, cahoc, phonghoc, thu, giohoc, malop) VALUES(@ma, @ca, @ph, @thu, @gio, @lop)";
            SqlParameter[] p = {
                new SqlParameter("@ma", txtMaLich.Text.Trim()),
                new SqlParameter("@ca", txtCaHoc.Text.Trim()),
                new SqlParameter("@ph", txtPhongHoc.Text.Trim()),
                new SqlParameter("@thu", txtThu.Text.Trim()),
                new SqlParameter("@gio", txtGioHoc.Text.Trim()),
                new SqlParameter("@lop", ddlLopLich.SelectedValue)
            };
            kncsdl.ThucThiLenh(sql, p);
            pnlTB.Visible = true; litTB.Text = "Đã phân bổ lịch học cho lớp!";
            TaiDuLieu();
        }
    }
}