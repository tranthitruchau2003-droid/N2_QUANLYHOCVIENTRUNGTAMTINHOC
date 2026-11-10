using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using N2_QUANLYHOCVIENTRUNGTAMTINHOC.DAL;

namespace N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.KhoaHoc
{
    public partial class QuanLyKhoaHoc : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) TaiDuLieu();
        }

        private void TaiDuLieu(string kw = "")
        {
            string sql = "SELECT * FROM KHOAHOC WHERE (@kw='' OR tenkhoahoc LIKE @kw OR makhoahoc LIKE @kw)";
            gvKhoaHoc.DataSource = kncsdl.DocDuLieu(sql, new[] { new SqlParameter("@kw", string.IsNullOrEmpty(kw) ? "" : "%" + kw + "%") });
            gvKhoaHoc.DataBind();
        }

        protected void btnThem_Click(object sender, EventArgs e)
        {
            string sql = "INSERT INTO KHOAHOC(makhoahoc, tenkhoahoc, hocphi, thoigianhoc, mota, trangthaikhoahoc) VALUES(@ma, @ten, @hp, @tg, @mt, @tt)";
            SqlParameter[] p = {
                new SqlParameter("@ma", txtMaKH.Text.Trim()),
                new SqlParameter("@ten", txtTenKH.Text.Trim()),
                new SqlParameter("@hp", decimal.Parse(string.IsNullOrEmpty(txtHocPhi.Text) ? "0" : txtHocPhi.Text)),
                new SqlParameter("@tg", txtThoiGian.Text.Trim()),
                new SqlParameter("@mt", txtMoTa.Text.Trim()),
                new SqlParameter("@tt", ddlTrangThai.SelectedValue)
            };
            kncsdl.ThucThiLenh(sql, p);
            pnlTB.Visible = true; litTB.Text = "Đã thêm khóa học thành công!";
            TaiDuLieu();
        }

        protected void btnSua_Click(object sender, EventArgs e)
        {
            string sql = "UPDATE KHOAHOC SET tenkhoahoc=@ten, hocphi=@hp, thoigianhoc=@tg, mota=@mt, trangthaikhoahoc=@tt WHERE makhoahoc=@ma";
            SqlParameter[] p = {
                new SqlParameter("@ma", txtMaKH.Text.Trim()),
                new SqlParameter("@ten", txtTenKH.Text.Trim()),
                new SqlParameter("@hp", decimal.Parse(string.IsNullOrEmpty(txtHocPhi.Text) ? "0" : txtHocPhi.Text)),
                new SqlParameter("@tg", txtThoiGian.Text.Trim()),
                new SqlParameter("@mt", txtMoTa.Text.Trim()),
                new SqlParameter("@tt", ddlTrangThai.SelectedValue)
            };
            kncsdl.ThucThiLenh(sql, p);
            pnlTB.Visible = true; litTB.Text = "Cập nhật khóa học thành công!";
            TaiDuLieu();
        }

        protected void gvKhoaHoc_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            string ma = e.CommandArgument.ToString();
            if (e.CommandName == "Chon")
            {
                DataTable dt = kncsdl.DocDuLieu("SELECT * FROM KHOAHOC WHERE makhoahoc=@ma", new[] { new SqlParameter("@ma", ma) });
                if (dt.Rows.Count > 0)
                {
                    DataRow r = dt.Rows[0];
                    txtMaKH.Text = r["makhoahoc"].ToString();
                    txtTenKH.Text = r["tenkhoahoc"].ToString();
                    txtHocPhi.Text = r["hocphi"].ToString();
                    txtThoiGian.Text = r["thoigianhoc"].ToString();
                    txtMoTa.Text = r["mota"].ToString();
                    ddlTrangThai.SelectedValue = r["trangthaikhoahoc"].ToString();
                }
            }
            else if (e.CommandName == "Xoa")
            {
                try
                {
                    kncsdl.ThucThiLenh("DELETE FROM KHOAHOC WHERE makhoahoc=@ma", new[] { new SqlParameter("@ma", ma) });
                    pnlTB.Visible = true; litTB.Text = "Xóa khóa học thành công!";
                    TaiDuLieu();
                }
                catch { pnlTB.Visible = true; litTB.Text = "Không thể xóa khóa học đã có lớp hoặc học viên đăng ký!"; }
            }
        }

        protected void btnTim_Click(object sender, EventArgs e) => TaiDuLieu(txtTim.Text.Trim());
    }
}