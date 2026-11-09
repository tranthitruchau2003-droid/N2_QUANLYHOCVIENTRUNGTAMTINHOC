using System;
using System.Data;
using System.Data.SqlClient;
using N2_QUANLYHOCVIENTRUNGTAMTINHOC.DAL;

namespace N2_QUANLYHOCVIENTRUNGTAMTINHOC
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["TenDangNhap"] != null) Response.Redirect("~/Dashboard.aspx");
        }

        protected void btnDangNhap_Click(object sender, EventArgs e)
        {
            string user = txtTenDangNhap.Text.Trim();
            string passHash = txtMatKhau.Text.Trim();

            string sql = "SELECT * FROM TAIKHOAN WHERE tendangnhap = @u AND matkhau = @p AND trangthai = 1";
            SqlParameter[] prms = {
                new SqlParameter("@u", user),
                new SqlParameter("@p", passHash)
            };

            DataTable dt = kncsdl.DocDuLieu(sql, prms);
            if (dt.Rows.Count > 0)
            {
                Session["TenDangNhap"] = dt.Rows[0]["tendangnhap"].ToString();
                Session["HoTen"] = dt.Rows[0]["hoten"].ToString();
                Session["Quyen"] = dt.Rows[0]["quyen"].ToString();

                kncsdl.GhiNhatKy(user, "Đăng nhập", "Đăng nhập thành công vào hệ thống");
                Response.Redirect("~/Dashboard.aspx");
            }
            else
            {
                pnlLoi.Visible = true;
                litLoi.Text = "Tên đăng nhập hoặc mật khẩu không chính xác!";
            }
        }
    }
}