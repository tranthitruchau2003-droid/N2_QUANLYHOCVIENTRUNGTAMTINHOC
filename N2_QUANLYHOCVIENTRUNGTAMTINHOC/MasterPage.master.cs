using System;
using System.Configuration;

namespace N2_QUANLYHOCVIENTRUNGTAMTINHOC
{
    public partial class MasterPage : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["TenDangNhap"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                litTenTrungTam.Text = ConfigurationManager.AppSettings["TenTrungTam"];
                litNguoiDung.Text = string.Format("{0} ({1})", Session["HoTen"], Session["Quyen"]);

                // Phân quyền: Chỉ Admin mới thấy menu Cài đặt & Sao lưu
                string quyen = Session["Quyen"] != null ? Session["Quyen"].ToString() : "";
                liCaiDat.Visible = quyen.Equals("Admin", StringComparison.OrdinalIgnoreCase);
            }
        }

        protected void btnDangXuat_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/Login.aspx");
        }
    }
}