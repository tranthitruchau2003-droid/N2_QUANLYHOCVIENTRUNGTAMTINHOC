using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.UI.WebControls;
using N2_QUANLYHOCVIENTRUNGTAMTINHOC.DAL;

namespace N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.CaiDat
{
    public partial class CaiDatHeThong : System.Web.UI.Page
    {
        private bool XemCaTuan
        {
            get { return ViewState["XemCaTuan"] != null && (bool)ViewState["XemCaTuan"]; }
            set { ViewState["XemCaTuan"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (MasterPage.ChuanHoaQuyen(Session["Quyen"]) != "Admin")
            {
                Response.Redirect("~/Dashboard.aspx", true);
                return;
            }

            if (IsPostBack)
            {
                pnlTB.Visible = false;
            }
            else
            {
                XemCaTuan = false;
                string folder = Server.MapPath("~/App_Data/Backup");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                txtThuMucBackup.Text = folder;
                TaiDanhSachFileBak();
                DonDepVaTaiNhatKy();
            }
        }

        private void TaiDanhSachFileBak()
        {
            ddlFileBak.Items.Clear();
            string folder = txtThuMucBackup.Text;
            if (Directory.Exists(folder))
            {
                foreach (string f in Directory.GetFiles(folder, "*.bak"))
                {
                    string name = Path.GetFileName(f);
                    ddlFileBak.Items.Add(new ListItem(name, name));
                }
            }
        }

        private void DonDepVaTaiNhatKy()
        {
            kncsdl.ThucThiLenh("DELETE FROM NHATKY_HETHONG WHERE thoigian < DATEADD(DAY, -30, GETDATE())");

            if (XemCaTuan)
            {
                DataTable dtTuan = kncsdl.DocDuLieu(@"SELECT * FROM NHATKY_HETHONG 
                                                      WHERE thoigian >= DATEADD(DAY, -7, GETDATE()) 
                                                      ORDER BY thoigian DESC, manhatky DESC");
                gvNhatKy.DataSource = dtTuan;
                gvNhatKy.DataBind();
                btnXemThemNhatKy.Text = "Thu gọn (8 dòng mới nhất)";
            }
            else
            {
                DataTable dtGanDay = kncsdl.DocDuLieu("SELECT TOP 8 * FROM NHATKY_HETHONG ORDER BY thoigian DESC, manhatky DESC");
                gvNhatKy.DataSource = dtGanDay;
                gvNhatKy.DataBind();
                btnXemThemNhatKy.Text = "Xem thêm (Cả tuần)";
            }
        }

        protected void btnXemThemNhatKy_Click(object sender, EventArgs e)
        {
            XemCaTuan = !XemCaTuan;
            DonDepVaTaiNhatKy();
        }

        protected void btnBackup_Click(object sender, EventArgs e)
        {
            try
            {
                string folder = Server.MapPath("~/App_Data/Backup");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                string dbName = new SqlConnectionStringBuilder(kncsdl.ChuoiKetNoi).InitialCatalog;
                string filePath = Path.Combine(folder, $"{dbName}_{DateTime.Now:yyyyMMdd_HHmmss}.bak");

                string sql = $"BACKUP DATABASE [{dbName}] TO DISK = @path WITH FORMAT, INIT";
                kncsdl.ThucThiLenh(sql, new[] { new SqlParameter("@path", filePath) });

                kncsdl.GhiNhatKy(Session["TenDangNhap"]?.ToString(), "Sao lưu CSDL", Path.GetFileName(filePath));
                HienTB("Sao lưu cơ sở dữ liệu thành công: " + Path.GetFileName(filePath));
                TaiDanhSachFileBak();
                DonDepVaTaiNhatKy();
            }
            catch
            {
                HienTB("Không thể sao lưu CSDL. Vui lòng kiểm tra quyền ghi thư mục trên SQL Server!", true);
            }
        }

        protected void btnRestore_Click(object sender, EventArgs e)
        {
            if (ddlFileBak.Items.Count == 0 || string.IsNullOrEmpty(ddlFileBak.SelectedValue))
            {
                HienTB("Vui lòng chọn file sao lưu (.bak)!", true);
                return;
            }

            string folder = Server.MapPath("~/App_Data/Backup");
            string safeFileName = Path.GetFileName(ddlFileBak.SelectedValue);
            string filePath = Path.Combine(folder, safeFileName);
            string dbName = new SqlConnectionStringBuilder(kncsdl.ChuoiKetNoi).InitialCatalog;

            try
            {
                string sql = $@"USE master;
                                ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                                RESTORE DATABASE [{dbName}] FROM DISK = @path WITH REPLACE;";
                kncsdl.ThucThiLenh(sql, new[] { new SqlParameter("@path", filePath) });

                kncsdl.GhiNhatKy(Session["TenDangNhap"]?.ToString(), "Phục hồi CSDL", safeFileName);
                HienTB("Phục hồi dữ liệu thành công từ bản sao lưu: " + safeFileName);
                DonDepVaTaiNhatKy();
            }
            catch
            {
                HienTB("Lỗi khi phục hồi CSDL. Vui lòng kiểm tra file sao lưu và thử lại!", true);
            }
            finally
            {
                try
                {
                    kncsdl.ThucThiLenh($"USE master; ALTER DATABASE [{dbName}] SET MULTI_USER;");
                }
                catch { }
            }
        }

        private void HienTB(string msg, bool laLoi = false)
        {
            pnlTB.Visible = true;
            pnlTB.CssClass = laLoi
                ? "alert alert-danger text-danger fw-semibold alert-dismissible fade show shadow"
                : "alert alert-success text-success fw-semibold alert-dismissible fade show shadow";
            litTB.Text = HttpUtility.HtmlEncode(msg);
        }
    }
}