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
                txtThuMucBackup.Text = LayThuMucBackupHopLe();
                TaiDanhSachFileBak();
                DonDepVaTaiNhatKy();
            }
        }

        private string LayThuMucBackupHopLe()
        {
            string webBackupFolder = Server.MapPath("~/App_Data/Backup");
            string lowerPath = webBackupFolder.ToLowerInvariant();

            // Nếu web không nằm trong C:\Users\... (Desktop/OneDrive) thì dùng thẳng App_Data\Backup
            if (!lowerPath.Contains("\\users\\") && !lowerPath.Contains("\\desktop\\") && !lowerPath.Contains("\\onedrive\\"))
            {
                if (!Directory.Exists(webBackupFolder)) Directory.CreateDirectory(webBackupFolder);
                return webBackupFolder;
            }

            // Dùng thư mục chung trên ổ đĩa gốc (VD: C:\QuanLyDaoTao_Backup) để cả Web và SQL Server đều truy cập được
            try
            {
                string rootDrive = Path.GetPathRoot(webBackupFolder);
                string sharedFolder = Path.Combine(rootDrive, "QuanLyDaoTao_Backup");
                if (!Directory.Exists(sharedFolder)) Directory.CreateDirectory(sharedFolder);
                return sharedFolder;
            }
            catch { }

            // Dự phòng cuối cùng: dùng thư mục Backup mặc định của SQL Server
            try
            {
                object sqlBackupDir = kncsdl.LayGiaTriDon("SELECT CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS NVARCHAR(500))");
                string dir = sqlBackupDir?.ToString().Trim() ?? "";
                if (!string.IsNullOrEmpty(dir)) return dir;
            }
            catch { }

            return webBackupFolder;
        }

        private void TaiDanhSachFileBak()
        {
            ddlFileBak.Items.Clear();
            string folder = LayThuMucBackupHopLe();
            txtThuMucBackup.Text = folder;

            try
            {
                if (Directory.Exists(folder))
                {
                    foreach (string f in Directory.GetFiles(folder, "*.bak"))
                    {
                        string name = Path.GetFileName(f);
                        ddlFileBak.Items.Add(new ListItem(name, name));
                    }
                    return;
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Nếu Windows chặn Web đọc trực tiếp thư mục Program Files, nhờ SQL Server liệt kê giúp
            }

            try
            {
                DataTable dtFiles = kncsdl.DocDuLieu("EXEC master.dbo.xp_dirtree @dir, 1, 1",
                    new[] { new SqlParameter("@dir", folder) });
                foreach (DataRow r in dtFiles.Rows)
                {
                    string name = r["subdirectory"]?.ToString() ?? "";
                    if (name.EndsWith(".bak", StringComparison.OrdinalIgnoreCase))
                        ddlFileBak.Items.Add(new ListItem(name, name));
                }
            }
            catch { }
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
                string folder = LayThuMucBackupHopLe();
                txtThuMucBackup.Text = folder;
                string dbName = new SqlConnectionStringBuilder(kncsdl.ChuoiKetNoi).InitialCatalog;
                string filePath = Path.Combine(folder, $"{dbName}_{DateTime.Now:yyyyMMdd_HHmmss}.bak");

                string sql = $"BACKUP DATABASE [{dbName}] TO DISK = @path WITH FORMAT, INIT";
                kncsdl.ThucThiLenh(sql, new[] { new SqlParameter("@path", filePath) });

                kncsdl.GhiNhatKy(Session["TenDangNhap"]?.ToString(), "Sao lưu CSDL", Path.GetFileName(filePath));
                HienTB("Sao lưu cơ sở dữ liệu thành công: " + Path.GetFileName(filePath));
                TaiDanhSachFileBak();
                DonDepVaTaiNhatKy();
            }
            catch (Exception ex)
            {
                HienTB("Không thể sao lưu CSDL: " + ex.Message, true);
            }
        }

        protected void btnRestore_Click(object sender, EventArgs e)
        {
            if (ddlFileBak.Items.Count == 0 || string.IsNullOrEmpty(ddlFileBak.SelectedValue))
            {
                HienTB("Vui lòng chọn file sao lưu (.bak)!", true);
                return;
            }

            string folder = LayThuMucBackupHopLe();
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