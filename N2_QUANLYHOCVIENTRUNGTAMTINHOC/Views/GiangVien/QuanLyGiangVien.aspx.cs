using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using N2_QUANLYHOCVIENTRUNGTAMTINHOC.DAL;

namespace N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.GiangVien
{
    public partial class QuanLyGiangVien : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) TaiGV();
        }

        private void TaiGV()
        {
            string sql = @"SELECT gv.*, (SELECT COUNT(*) FROM LOPHOC l WHERE l.magiangvien = gv.magiangvien) AS SoLop 
                           FROM GIANGVIEN gv";
            gvGV.DataSource = kncsdl.DocDuLieu(sql);
            gvGV.DataBind();
        }

        protected void btnThem_Click(object sender, EventArgs e)
        {
            string sql = "INSERT INTO GIANGVIEN VALUES(@ma, @ten, @dt, @em, @cm, @hv)";
            SqlParameter[] p = {
                new SqlParameter("@ma", txtMaGV.Text.Trim()),
                new SqlParameter("@ten", txtHoTen.Text.Trim()),
                new SqlParameter("@dt", txtSDT.Text.Trim()),
                new SqlParameter("@em", txtEmail.Text.Trim()),
                new SqlParameter("@cm", txtChuyenMon.Text.Trim()),
                new SqlParameter("@hv", txtHocVi.Text.Trim())
            };
            kncsdl.ThucThiLenh(sql, p);
            pnlTB.Visible = true; litTB.Text = "Thêm giảng viên thành công!";
            TaiGV();
        }

        protected void btnSua_Click(object sender, EventArgs e)
        {
            string sql = "UPDATE GIANGVIEN SET hoten=@ten, dienthoai=@dt, email=@em, chuyenmon=@cm, hocvi=@hv WHERE magiangvien=@ma";
            SqlParameter[] p = {
                new SqlParameter("@ma", txtMaGV.Text.Trim()),
                new SqlParameter("@ten", txtHoTen.Text.Trim()),
                new SqlParameter("@dt", txtSDT.Text.Trim()),
                new SqlParameter("@em", txtEmail.Text.Trim()),
                new SqlParameter("@cm", txtChuyenMon.Text.Trim()),
                new SqlParameter("@hv", txtHocVi.Text.Trim())
            };
            kncsdl.ThucThiLenh(sql, p);
            pnlTB.Visible = true; litTB.Text = "Cập nhật giảng viên thành công!";
            TaiGV();
        }

        protected void gvGV_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Chon")
            {
                DataTable dt = kncsdl.DocDuLieu("SELECT * FROM GIANGVIEN WHERE magiangvien=@ma", new[] { new SqlParameter("@ma", e.CommandArgument.ToString()) });
                if (dt.Rows.Count > 0)
                {
                    DataRow r = dt.Rows[0];
                    txtMaGV.Text = r["magiangvien"].ToString();
                    txtHoTen.Text = r["hoten"].ToString();
                    txtSDT.Text = r["dienthoai"].ToString();
                    txtEmail.Text = r["email"].ToString();
                    txtChuyenMon.Text = r["chuyenmon"].ToString();
                    txtHocVi.Text = r["hocvi"].ToString();
                }
            }
        }
    }
}