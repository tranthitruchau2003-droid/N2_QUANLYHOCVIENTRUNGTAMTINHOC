using System;
using System.Data.SqlClient;
using System.IO;
using N2_QUANLYHOCVIENTRUNGTAMTINHOC.DAL;

namespace N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.CaiDat
{
    public partial class CaiDatHeThong : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["Quyen"] == null || !Session["Quyen"].ToString().Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Dashboard.aspx");
                return;
            }

            if (!IsPostBack)
            {
                string defaultFolder = Server.MapPath("~/App_Data/Backups");
                if (!Directory.Exists(defaultFolder)) Directory.CreateDirectory(defaultFolder);
                txtThuMucBackup.Text = defaultFolder;
                TaiDanhSachBackup();
                TaiNhatKy();
            }
        }

        private void TaiDanhSachBackup()
        {
            ddlFileBackup.Items.Clear();
            if (Directory.Exists(txtThuMucBackup.Text))
            {
                foreach (string file in Directory.GetFiles(txtThuMucBackup.Text, "*.bak"))
                {
                    ddlFileBackup.Items.Add(new System.Web.UI.WebControls.ListItem(Path.GetFileName(file), file));
                }
            }
        }

        private void TaiNhatKy()
        {
            gvNhatKy.DataSource = kncsdl.DocDuLieu("SELECT TOP 8 * FROM NHATKY_HETHONG ORDER BY thoigian DESC");
            gvNhatKy.DataBind();
        }

        protected void btnBackup_Click(object sender, EventArgs e)
        {
            try
            {
                string folder = txtThuMucBackup.Text.Trim();
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                string fileName = $"QuanLyDaoTao_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                string fullPath = Path.Combine(folder, fileName);

                string sql = $"BACKUP DATABASE [QuanLyDaoTao] TO DISK = @path WITH INIT, NAME = 'Full Backup QuanLyDaoTao'";
                kncsdl.ThucThiLenh(sql, new[] { new SqlParameter("@path", fullPath) });

                kncsdl.GhiNhatKy(Session["TenDangNhap"]?.ToString(), "Sao lưu DB", "File: " + fileName);
                pnlTB.Visible = true;
                litTB.Text = "Sao lưu thành công! Đường dẫn file: " + fullPath;
                TaiDanhSachBackup();
                TaiNhatKy();
            }
            catch (Exception ex)
            {
                pnlTB.Visible = true;
                litTB.Text = "Lỗi sao lưu (Kiểm tra quyền ghi của dịch vụ SQL Server tại thư mục đích): " + ex.Message;
            }
        }

        protected void btnRestore_Click(object sender, EventArgs e)
        {
            if (ddlFileBackup.SelectedItem == null || !File.Exists(ddlFileBackup.SelectedValue))
            {
                pnlTB.Visible = true; litTB.Text = "Vui lòng chọn file .bak hợp lệ để phục hồi!";
                return;
            }

            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(kncsdl.ChuoiKetNoi)
                {
                    InitialCatalog = "master"
                };

                using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
                {
                    conn.Open();
                    string sqlRestore = @"
                        ALTER DATABASE [QuanLyDaoTao] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                        RESTORE DATABASE [QuanLyDaoTao] FROM DISK = @path WITH REPLACE;
                        ALTER DATABASE [QuanLyDaoTao] SET MULTI_USER;";
                    using (SqlCommand cmd = new SqlCommand(sqlRestore, conn))
                    {
                        cmd.CommandTimeout = 300;
                        cmd.Parameters.AddWithValue("@path", ddlFileBackup.SelectedValue);
                        cmd.ExecuteNonQuery();
                    }
                }

                SqlConnection.ClearAllPools();
                kncsdl.GhiNhatKy(Session["TenDangNhap"]?.ToString(), "Phục hồi DB", "Từ file: " + Path.GetFileName(ddlFileBackup.SelectedValue));
                pnlTB.Visible = true;
                litTB.Text = "Phục hồi cơ sở dữ liệu QuanLyDaoTao thành công!";
                TaiNhatKy();
            }
            catch (Exception ex)
            {
                pnlTB.Visible = true;
                litTB.Text = "Lỗi phục hồi dữ liệu: " + ex.Message;
            }
        }

        protected void btnDoiMK_Click(object sender, EventArgs e)
        {
            string u = Session["TenDangNhap"].ToString();
            string oldHash = txtMKCu.Text.Trim();
            string newHash = txtMKMoi.Text.Trim();

            int kq = kncsdl.ThucThiLenh("UPDATE TAIKHOAN SET matkhau=@m WHERE tendangnhap=@u AND matkhau=@c",
                new[] { new SqlParameter("@m", newHash), new SqlParameter("@u", u), new SqlParameter("@c", oldHash) });

            pnlTB.Visible = true;
            litTB.Text = kq > 0 ? "Đổi mật khẩu thành công!" : "Mật khẩu hiện tại không chính xác!";
        }
    }
}