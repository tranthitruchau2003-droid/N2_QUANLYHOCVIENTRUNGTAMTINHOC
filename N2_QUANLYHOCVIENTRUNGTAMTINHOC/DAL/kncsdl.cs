using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace N2_QUANLYHOCVIENTRUNGTAMTINHOC.DAL
{
    public class kncsdl
    {
        public static readonly string ChuoiKetNoi = ConfigurationManager.ConnectionStrings["QuanLyDaoTao"].ConnectionString;

        // 1. Lấy dữ liệu trả về DataTable (SELECT)
        public static DataTable DocDuLieu(string sql, SqlParameter[] prms = null)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(ChuoiKetNoi))
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (prms != null) cmd.Parameters.AddRange(prms);
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            return dt;
        }

        // 2. Thực thi INSERT, UPDATE, DELETE trả về số dòng ảnh hưởng
        public static int ThucThiLenh(string sql, SqlParameter[] prms = null)
        {
            using (SqlConnection conn = new SqlConnection(ChuoiKetNoi))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (prms != null) cmd.Parameters.AddRange(prms);
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        // 3. Thực thi trả về 1 giá trị đơn (COUNT, SUM, MAX)
        public static object LayGiaTriDon(string sql, SqlParameter[] prms = null)
        {
            using (SqlConnection conn = new SqlConnection(ChuoiKetNoi))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (prms != null) cmd.Parameters.AddRange(prms);
                    object kq = cmd.ExecuteScalar();
                    return kq == DBNull.Value ? null : kq;
                }
            }
        }

        // 4. Ghi nhật ký hệ thống tự động
        public static void GhiNhatKy(string tenDangNhap, string hanhDong, string noiDung)
        {
            try
            {
                string sql = "INSERT INTO NHATKY_HETHONG(tendangnhap, hanhdong, noidung) VALUES(@u, @a, @c)";
                SqlParameter[] p = {
                    new SqlParameter("@u", tenDangNhap ?? "System"),
                    new SqlParameter("@a", hanhDong),
                    new SqlParameter("@c", noiDung)
                };
                ThucThiLenh(sql, p);
            }
            catch { /* Không làm gián đoạn luồng chính nếu log lỗi */ }
        }

        // 5. Băm mật khẩu SHA-256
        public static string BamMatKhau(string rawPassword)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawPassword));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}