using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using N2_QUANLYHOCVIENTRUNGTAMTINHOC.DAL;

namespace N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.DangKy
{
    public partial class QuanLyDangKy : System.Web.UI.Page
    {
        private const int SI_SO_CHUAN = 10;

        private string TuKhoaDK
        {
            get { return ViewState["TuKhoaTimDK"]?.ToString() ?? ""; }
            set { ViewState["TuKhoaTimDK"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string maDKUrl = Request.QueryString["madk"]?.Trim() ?? "";
                TuKhoaDK = maDKUrl;
                txtTimDK.Text = maDKUrl;
                // M7: Chặn thay đổi ngày đăng ký từ Client
                txtNgayDK.Text = DateTime.Now.ToString("yyyy-MM-dd");
                txtNgayDK.ReadOnly = true;

                CauHinhTheoQuyen();
                TaiDanhMuc();
                CapNhatHocPhiTheoKhoa(ddlKhoaHoc.SelectedValue);
                TaiDuLieu();

                if (!string.IsNullOrEmpty(maDKUrl) && pnlXuLyLopThieu.Visible)
                {
                    TaiDanhSachLopBiHuy();
                    ScriptManager.RegisterStartupScript(this, GetType(), "auto_open_hoantien", "moModalLopBiHuy();", true);
                }
            }
            else
            {
                CauHinhTheoQuyen();
                txtNgayDK.Text = DateTime.Now.ToString("yyyy-MM-dd");
            }
        }

        private bool LaHocVien()
        {
            return MasterPage.ChuanHoaQuyen(Session["Quyen"]) == "HocVien";
        }

        private bool LaQuanTriHoacNV()
        {
            string q = MasterPage.ChuanHoaQuyen(Session["Quyen"]);
            return q == "Admin" || q == "NhanVien";
        }

        private bool KiemTraQuyenTrenDangKy(string maDK)
        {
            if (LaQuanTriHoacNV()) return true;
            if (!LaHocVien()) return false;
            string user = Session["TenDangNhap"]?.ToString() ?? "";
            int c = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM DANGKY WHERE madangky=@dk AND mahocvien=@hv",
                new[] { new SqlParameter("@dk", maDK), new SqlParameter("@hv", user) }));
            return c > 0;
        }

        private void CauHinhTheoQuyen()
        {
            pnlXuLyLopThieu.Visible = !LaHocVien();
        }

        private void TaiDanhMuc()
        {
            string user = Session["TenDangNhap"]?.ToString() ?? "";

            if (LaHocVien())
            {
                ddlHocVien.DataSource = kncsdl.DocDuLieu("SELECT mahocvien, mahocvien + ' - ' + hoten AS HienThi FROM HOCVIEN WHERE mahocvien=@ma", new[] { new SqlParameter("@ma", user) });
                ddlHocVien.Enabled = false;
            }
            else
            {
                ddlHocVien.DataSource = kncsdl.DocDuLieu("SELECT mahocvien, mahocvien + ' - ' + hoten AS HienThi FROM HOCVIEN ORDER BY mahocvien DESC");
                ddlHocVien.Enabled = true;
            }
            ddlHocVien.DataTextField = "HienThi";
            ddlHocVien.DataValueField = "mahocvien";
            ddlHocVien.DataBind();

            // M6: Lọc "Đang mở"
            DataTable dtKH = kncsdl.DocDuLieu("SELECT makhoahoc, tenkhoahoc FROM KHOAHOC WHERE trangthaikhoahoc = N'Đang mở' ORDER BY tenkhoahoc");
            ddlKhoaHoc.DataSource = dtKH;
            ddlKhoaHoc.DataTextField = "tenkhoahoc";
            ddlKhoaHoc.DataValueField = "makhoahoc";
            ddlKhoaHoc.DataBind();

            string locKhoaCu = ddlLocKhoaMo.SelectedValue;
            ddlLocKhoaMo.DataSource = dtKH;
            ddlLocKhoaMo.DataTextField = "tenkhoahoc";
            ddlLocKhoaMo.DataValueField = "makhoahoc";
            ddlLocKhoaMo.DataBind();
            ddlLocKhoaMo.Items.Insert(0, new ListItem("-- Tất cả Khóa học --", ""));
            if (!string.IsNullOrEmpty(locKhoaCu) && ddlLocKhoaMo.Items.FindByValue(locKhoaCu) != null)
                ddlLocKhoaMo.SelectedValue = locKhoaCu;

            TaiLopTheoKhoa(ddlKhoaHoc.SelectedValue);
        }

        private void TaiLopTheoKhoa(string maKH)
        {
            string sql = @"SELECT l.malop, 
                                  l.tenlop + N' (KG: ' + ISNULL(FORMAT(l.ngaykhaigiang, 'dd/MM/yyyy'), N'Chưa xếp') + N' - Sĩ số: ' + CAST((SELECT COUNT(*) FROM DANGKY dk WHERE dk.malop = l.malop) AS NVARCHAR(10)) + N'/10)' AS HienThi
                           FROM LOPHOC l 
                           WHERE (@kh = '' OR l.makhoahoc = @kh)
                             AND ISNULL(l.dahuy, 0) = 0
                             AND (l.ngaykhaigiang IS NULL OR l.ngaykhaigiang > CAST(GETDATE() AS DATE))
                             AND (SELECT COUNT(*) FROM DANGKY dk WHERE dk.malop = l.malop) < 10
                           ORDER BY l.ngaykhaigiang ASC";
            ddlLopHoc.DataSource = kncsdl.DocDuLieu(sql, new[] { new SqlParameter("@kh", maKH ?? "") });
            ddlLopHoc.DataTextField = "HienThi";
            ddlLopHoc.DataValueField = "malop";
            ddlLopHoc.DataBind();
        }

        private void CapNhatHocPhiTheoKhoa(string maKH)
        {
            if (string.IsNullOrEmpty(maKH))
            {
                txtSoTienDong.Text = "0";
                return;
            }
            object hp = kncsdl.LayGiaTriDon("SELECT hocphi FROM KHOAHOC WHERE makhoahoc=@ma", new[] { new SqlParameter("@ma", maKH) });
            txtSoTienDong.Text = (hp != null && hp != DBNull.Value)
                ? Convert.ToDecimal(hp).ToString("#,##0", CultureInfo.InvariantCulture)
                : "0";
        }

        private void TaiDuLieu()
        {
            string sqlLopMo = @"SELECT l.malop, l.tenlop, l.makhoahoc, kh.tenkhoahoc, kh.hocphi, kh.thoigianhoc, l.ngaykhaigiang,
                                       ISNULL(gv.hoten + ISNULL(N' (' + gv.hocvi + N')', N''), N'Chưa phân công') AS tengiangvien,
                                       ISNULL(STUFF((SELECT '; ' + lh.cahoc + ' - ' + lh.thu + ' (' + lh.giohoc + ', P.' + lh.phonghoc + ')'
                                                     FROM LICHHOC lh WHERE lh.malop = l.malop FOR XML PATH('')), 1, 2, ''), N'Đang cập nhật lịch') AS thoikhoabieu,
                                       (SELECT COUNT(*) FROM DANGKY dk WHERE dk.malop = l.malop) AS siso,
                                       CAST(ISNULL(l.dahuy, 0) AS BIT) AS dahuy,
                                       CASE 
                                           WHEN ISNULL(l.dahuy, 0) = 1 THEN N'Đã hủy lớp (thiếu sĩ số)'
                                           WHEN l.ngaykhaigiang IS NOT NULL AND l.ngaykhaigiang <= CAST(GETDATE() AS DATE) THEN N'Đã khai giảng (Đóng ĐK)'
                                           WHEN (SELECT COUNT(*) FROM DANGKY dk WHERE dk.malop = l.malop) >= 10 THEN N'Đã mở lớp (Đủ 10/10 HV)'
                                           WHEN EXISTS (SELECT 1 FROM DANGKY dk WHERE dk.malop = l.malop AND dk.trangthailop LIKE N'%đặc cách%') THEN N'Đã mở lớp (Đặc cách)'
                                           ELSE N'Chờ đủ 10 HV (' + CAST((SELECT COUNT(*) FROM DANGKY dk WHERE dk.malop = l.malop) AS NVARCHAR(10)) + N'/10)'
                                       END AS trangthailop
                                FROM LOPHOC l JOIN KHOAHOC kh ON l.makhoahoc = kh.makhoahoc
                                LEFT JOIN GIANGVIEN gv ON l.magiangvien = gv.magiangvien
                                WHERE (@kh = '' OR l.makhoahoc = @kh) ORDER BY l.malop DESC";
            DataTable dtLopMo = kncsdl.DocDuLieu(sqlLopMo, new[] { new SqlParameter("@kh", ddlLocKhoaMo.SelectedValue ?? "") });
            dtLopMo.Columns.Add("thoigianlop", typeof(string));
            dtLopMo.Columns.Add("dadongdk", typeof(bool));

            foreach (DataRow r in dtLopMo.Rows)
            {
                if (r["ngaykhaigiang"] != DBNull.Value)
                {
                    DateTime batDau = Convert.ToDateTime(r["ngaykhaigiang"]).Date;
                    DateTime ketThuc = TinhNgayKetThucLop(batDau, r["thoigianhoc"]?.ToString() ?? "");
                    r["thoigianlop"] = $"{batDau:dd/MM/yyyy} - {ketThuc:dd/MM/yyyy}";
                    r["dadongdk"] = batDau <= DateTime.Today || Convert.ToBoolean(r["dahuy"]);
                }
                else
                {
                    r["thoigianlop"] = "Chưa xác định";
                    r["dadongdk"] = Convert.ToBoolean(r["dahuy"]);
                }
            }

            gvLopMo.DataSource = dtLopMo;
            gvLopMo.DataBind();

            string sqlThieu = @"SELECT l.malop, l.tenlop + N' (' + CAST(COUNT(dk.madangky) AS NVARCHAR(10)) + N'/10 HV)' AS HienThi
                                FROM LOPHOC l LEFT JOIN DANGKY dk ON l.malop = dk.malop WHERE ISNULL(l.dahuy, 0) = 0 GROUP BY l.malop, l.tenlop HAVING COUNT(dk.madangky) > 0 AND COUNT(dk.madangky) < 10";
            ddlLopThieu.DataSource = kncsdl.DocDuLieu(sqlThieu);
            ddlLopThieu.DataTextField = "HienThi"; ddlLopThieu.DataValueField = "malop"; ddlLopThieu.DataBind();

            string sqlDich = @"SELECT l.malop, l.tenlop + N' (' + CAST(COUNT(dk.madangky) AS NVARCHAR(10)) + N'/10 HV)' AS HienThi
                               FROM LOPHOC l LEFT JOIN DANGKY dk ON l.malop = dk.malop WHERE ISNULL(l.dahuy, 0) = 0 GROUP BY l.malop, l.tenlop HAVING COUNT(dk.madangky) < 10";
            ddlLopDich.DataSource = kncsdl.DocDuLieu(sqlDich);
            ddlLopDich.DataTextField = "HienThi"; ddlLopDich.DataValueField = "malop"; ddlLopDich.DataBind();

            string user = Session["TenDangNhap"]?.ToString() ?? "";
            string kw = TuKhoaDK;

            string sqlDK = @"SELECT dk.madangky, hv.hoten, kh.tenkhoahoc, kh.hocphi,
                                    ISNULL(l.tenlop + N' (GV: ' + gv.hoten + N')', N'Chưa xếp lớp') AS tenlop,
                                    ISNULL(SUM(CASE WHEN ISNULL(pt.noidungthu, N'') NOT LIKE N'Chờ xác nhận%' THEN pt.sotienthu ELSE 0 END), 0) AS dadong,
                                    (kh.hocphi - ISNULL(SUM(CASE WHEN ISNULL(pt.noidungthu, N'') NOT LIKE N'Chờ xác nhận%' THEN pt.sotienthu ELSE 0 END), 0)) AS conno,
                                    CASE 
                                        WHEN ISNULL(dk.trangthaithanhtoan, N'') = N'Chờ xác nhận' THEN N'Chờ duyệt biên lai CK'
                                        ELSE ISNULL(dk.trangthailop, N'Chờ đủ 10 HV')
                                    END AS trangthailop,
                                    ISNULL(dk.trangthaithanhtoan, N'Chưa thanh toán') AS trangthaithanhtoan
                             FROM DANGKY dk
                             JOIN HOCVIEN hv ON dk.mahocvien = hv.mahocvien
                             JOIN KHOAHOC kh ON dk.makhoahoc = kh.makhoahoc
                             LEFT JOIN LOPHOC l ON dk.malop = l.malop
                             LEFT JOIN GIANGVIEN gv ON l.magiangvien = gv.magiangvien
                             LEFT JOIN PHIEUTHU pt ON dk.madangky = pt.madangky
                             WHERE (@isHV = 0 OR dk.mahocvien = @user)
                             GROUP BY dk.madangky, hv.hoten, kh.tenkhoahoc, kh.hocphi, l.tenlop, gv.hoten, dk.trangthailop, dk.trangthaithanhtoan
                             ORDER BY dk.madangky DESC";
            SqlParameter[] pDK = {
                new SqlParameter("@isHV", LaHocVien() ? 1 : 0),
                new SqlParameter("@user", user)
            };
            DataTable dtDK = kncsdl.DocDuLieu(sqlDK, pDK);

            if (!string.IsNullOrWhiteSpace(kw))
            {
                string kwKhongDau = kncsdl.BoDauTiengViet(kw.Trim());
                DataTable dtLoc = dtDK.Clone();
                foreach (DataRow r in dtDK.Rows)
                {
                    string gopChuoi = $"{r["madangky"]} {r["hoten"]} {r["tenkhoahoc"]} {r["tenlop"]} {r["trangthailop"]}";
                    if (kncsdl.BoDauTiengViet(gopChuoi).Contains(kwKhongDau))
                    {
                        dtLoc.ImportRow(r);
                    }
                }
                dtDK = dtLoc;
            }

            gvDangKy.DataSource = dtDK;
            gvDangKy.DataBind();

            TaiGoiYTimKiemDK(LaHocVien(), user);
        }

        private DateTime TinhNgayKetThucLop(DateTime ngayBatDau, string thoiGianHoc)
        {
            if (string.IsNullOrWhiteSpace(thoiGianHoc)) return ngayBatDau.AddMonths(2);

            var mThang = System.Text.RegularExpressions.Regex.Match(thoiGianHoc, @"(\d+(?:[\.,]\d+)?)\s*tháng", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (mThang.Success)
            {
                decimal thang;
                if (decimal.TryParse(mThang.Groups[1].Value.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out thang) && thang > 0)
                {
                    int thangNguyen = (int)Math.Floor(thang);
                    int ngayLe = (int)Math.Round((thang - thangNguyen) * 30m);
                    return ngayBatDau.AddMonths(thangNguyen).AddDays(ngayLe);
                }
            }

            var mTuan = System.Text.RegularExpressions.Regex.Match(thoiGianHoc, @"(\d+(?:[\.,]\d+)?)\s*tuần", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (mTuan.Success)
            {
                decimal tuan;
                if (decimal.TryParse(mTuan.Groups[1].Value.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out tuan) && tuan > 0)
                    return ngayBatDau.AddDays((int)Math.Round(tuan * 7m));
            }

            var mBuoi = System.Text.RegularExpressions.Regex.Match(thoiGianHoc, @"(\d+)\s*buổi", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (mBuoi.Success)
            {
                int buoi;
                if (int.TryParse(mBuoi.Groups[1].Value, out buoi) && buoi > 0)
                    return ngayBatDau.AddDays((int)Math.Ceiling(buoi / 3.0) * 7);
            }

            return ngayBatDau.AddMonths(2);
        }

        private void TaiGoiYTimKiemDK(bool isHV, string user)
        {
            string sql = @"SELECT dk.madangky, hv.hoten, ISNULL(l.tenlop, N'Chưa xếp lớp') AS tenlop
                           FROM DANGKY dk
                           JOIN HOCVIEN hv ON dk.mahocvien = hv.mahocvien
                           LEFT JOIN LOPHOC l ON dk.malop = l.malop
                           WHERE (@isHV = 0 OR dk.mahocvien = @user)
                           ORDER BY dk.madangky DESC";
            DataTable dt = kncsdl.DocDuLieu(sql, new[] { new SqlParameter("@isHV", isHV ? 1 : 0), new SqlParameter("@user", user) });
            StringBuilder sb = new StringBuilder();
            foreach (DataRow r in dt.Rows)
            {
                sb.AppendFormat("<button type=\"button\" class=\"list-group-item list-group-item-action py-2 px-2 small goi-y-item\" data-value=\"{0}\"><strong>{0}</strong> - {1} ({2})</button>",
                    HttpUtility.HtmlAttributeEncode(r["madangky"].ToString()),
                    HttpUtility.HtmlEncode(r["hoten"].ToString()),
                    HttpUtility.HtmlEncode(r["tenlop"].ToString()));
            }
            litGoiYDK.Text = sb.ToString();
        }

        private void CapNhatTrangThaiSiSoLop(string maLop)
        {
            if (string.IsNullOrEmpty(maLop)) return;
            int siSo = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM DANGKY WHERE malop=@l", new[] { new SqlParameter("@l", maLop) }));
            string tt = siSo >= SI_SO_CHUAN ? "Đã mở lớp chính thức (10/10)" : $"Chờ mở lớp ({siSo}/10 HV)";
            kncsdl.ThucThiLenh("UPDATE DANGKY SET trangthailop=@tt WHERE malop=@l AND ISNULL(trangthailop,'') NOT LIKE N'%đặc cách%'",
                new[] { new SqlParameter("@tt", tt), new SqlParameter("@l", maLop) });
        }

        protected void btnDangKy_Click(object sender, EventArgs e)
        {
            string maLop = ddlLopHoc.SelectedValue;
            string maHV = LaHocVien() ? (Session["TenDangNhap"]?.ToString() ?? "") : ddlHocVien.SelectedValue;
            if (string.IsNullOrEmpty(maLop))
            {
                HienTB("Vui lòng chọn Lớp học phần (chưa khai giảng và còn chỗ)!", true);
                return;
            }

            DataTable dtLopChon = kncsdl.DocDuLieu("SELECT makhoahoc, ISNULL(dahuy, 0) AS dahuy, ngaykhaigiang FROM LOPHOC WHERE malop=@l",
                new[] { new SqlParameter("@l", maLop) });
            if (dtLopChon.Rows.Count == 0)
            {
                HienTB("Lớp học không tồn tại!", true);
                TaiDanhMuc();
                TaiDuLieu();
                return;
            }
            if (Convert.ToBoolean(dtLopChon.Rows[0]["dahuy"]))
            {
                HienTB($"Lớp {maLop} đã bị hủy do thiếu sĩ số, không thể đăng ký!", true);
                TaiDanhMuc();
                TaiDuLieu();
                return;
            }
            string maKH = dtLopChon.Rows[0]["makhoahoc"].ToString();

            object kgObj = dtLopChon.Rows[0]["ngaykhaigiang"];
            if (kgObj != null && kgObj != DBNull.Value && Convert.ToDateTime(kgObj).Date <= DateTime.Today)
            {
                HienTB($"Lớp {maLop} đã khai giảng ngày {Convert.ToDateTime(kgObj):dd/MM/yyyy}, đã đóng cổng đăng ký!", true);
                TaiDanhMuc();
                TaiDuLieu();
                return;
            }

            // M6: Chặn đăng ký khóa Tạm ngưng
            string trangThaiKH = kncsdl.LayGiaTriDon("SELECT trangthaikhoahoc FROM KHOAHOC WHERE makhoahoc=@kh", new[] { new SqlParameter("@kh", maKH) })?.ToString() ?? "";
            if (trangThaiKH == "Tạm ngưng")
            {
                HienTB("Khóa học này đang tạm ngưng, không thể đăng ký thêm!", true);
                return;
            }

            int trungKhoa = Convert.ToInt32(kncsdl.LayGiaTriDon(
                "SELECT COUNT(*) FROM DANGKY WHERE mahocvien=@hv AND makhoahoc=@kh AND ISNULL(trangthailop, N'') NOT LIKE N'%Hủy lớp%'",
                new[] { new SqlParameter("@hv", maHV), new SqlParameter("@kh", maKH) }));
            if (trungKhoa > 0)
            {
                HienTB("Học viên đã đăng ký khóa học này rồi! Nếu muốn đổi lớp, hãy dùng chức năng Đổi Lớp.", true);
                return;
            }

            if (KiemTraTrungLichVaGoiY(maHV, maLop, maKH)) return;

            // M2: Sử dụng Transaction để ngăn vượt quá sĩ số
            using (SqlConnection conn = new SqlConnection(kncsdl.ChuoiKetNoi))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction(IsolationLevel.Serializable);
                try
                {
                    int siSoHienTai = Convert.ToInt32(new SqlCommand("SELECT COUNT(*) FROM DANGKY WITH (UPDLOCK) WHERE malop='" + maLop + "'", conn, tran).ExecuteScalar());
                    if (siSoHienTai >= SI_SO_CHUAN)
                    {
                        tran.Rollback();
                        HienTB("Lớp học này đã đủ 10/10 học viên! Vui lòng chọn lớp khác.", true);
                        return;
                    }

                    int count = Convert.ToInt32(new SqlCommand("SELECT COUNT(*) FROM DANGKY WITH (UPDLOCK)", conn, tran).ExecuteScalar()) + 1;
                    string maDK = "DK" + count.ToString("D2");
                    while (Convert.ToInt32(new SqlCommand("SELECT COUNT(*) FROM DANGKY WHERE madangky='" + maDK + "'", conn, tran).ExecuteScalar()) > 0)
                        maDK = "DK" + (++count).ToString("D2");

                    string sql = @"INSERT INTO DANGKY(madangky, ngaydangky, sotiendong, trangthaithanhtoan, mahocvien, makhoahoc, malop, trangthailop)
                                   VALUES(@ma, GETDATE(), 0, N'Chưa thanh toán', @hv, @kh, @lop, N'Chờ mở lớp')";
                    SqlCommand cmdInsert = new SqlCommand(sql, conn, tran);
                    cmdInsert.Parameters.AddWithValue("@ma", maDK);
                    cmdInsert.Parameters.AddWithValue("@hv", maHV);
                    cmdInsert.Parameters.AddWithValue("@kh", maKH);
                    cmdInsert.Parameters.AddWithValue("@lop", maLop);
                    cmdInsert.ExecuteNonQuery();

                    tran.Commit();

                    CapNhatTrangThaiSiSoLop(maLop);
                    kncsdl.GhiNhatKy(Session["TenDangNhap"]?.ToString(), "Đăng ký học phần", $"Hồ sơ {maDK}: HV {maHV} đăng ký lớp {maLop}");
                    HienTB($"Đăng ký lớp {maLop} thành công (Sĩ số: {siSoHienTai + 1}/10)! Hãy bấm nút 'Đóng HP' ở bảng bên phải để quét mã QR chuyển khoản.");
                    txtMaDK.Text = "";
                }
                catch
                {
                    tran.Rollback();
                    HienTB("Có lỗi trong quá trình xử lý đăng ký, vui lòng thử lại!", true);
                }
            }

            gvDangKy.PageIndex = 0;
            TaiDanhMuc();
            CapNhatHocPhiTheoKhoa(ddlKhoaHoc.SelectedValue);
            upPhieuDangKy.Update();
            TaiDuLieu();
        }

        protected void btnDoiLop_Click(object sender, EventArgs e)
        {
            string maDK = txtMaDK.Text.Trim();
            string maLopMoi = ddlLopHoc.SelectedValue;
            if (string.IsNullOrEmpty(maDK) || string.IsNullOrEmpty(maLopMoi))
            {
                HienTB("Vui lòng chọn nút Đổi lớp ở danh sách bên phải!", true);
                return;
            }

            if (!KiemTraQuyenTrenDangKy(maDK))
            {
                HienTB("Bạn không có quyền thao tác trên hồ sơ đăng ký này!", true);
                return;
            }

            DataTable dtLopMoi = kncsdl.DocDuLieu("SELECT makhoahoc, ISNULL(dahuy, 0) AS dahuy, ngaykhaigiang FROM LOPHOC WHERE malop=@l",
                new[] { new SqlParameter("@l", maLopMoi) });
            if (dtLopMoi.Rows.Count == 0 || Convert.ToBoolean(dtLopMoi.Rows[0]["dahuy"]))
            {
                HienTB("Lớp mới không tồn tại hoặc đã bị hủy, vui lòng chọn lớp khác!", true);
                TaiDanhMuc();
                TaiDuLieu();
                return;
            }

            // M8: Chặn đổi nếu lớp mới đã khai giảng
            object kgObjMoi = dtLopMoi.Rows[0]["ngaykhaigiang"];
            if (kgObjMoi != null && kgObjMoi != DBNull.Value && Convert.ToDateTime(kgObjMoi).Date <= DateTime.Today)
            {
                HienTB($"Lớp mới {maLopMoi} đã khai giảng, không thể đổi sang!", true);
                return;
            }

            string maKHLopMoi = dtLopMoi.Rows[0]["makhoahoc"].ToString();

            int siSo = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM DANGKY WHERE malop=@l", new[] { new SqlParameter("@l", maLopMoi) }));
            if (siSo >= SI_SO_CHUAN)
            {
                HienTB("Lớp mới đã đủ 10/10 học viên, vui lòng chọn lớp khác!", true);
                return;
            }

            DataTable dtDK = kncsdl.DocDuLieu(@"SELECT dk.mahocvien, dk.makhoahoc, dk.malop, ISNULL(dk.trangthailop, N'') AS trangthailop, 
                                                       l.ngaykhaigiang AS khcu
                                                FROM DANGKY dk LEFT JOIN LOPHOC l ON dk.malop = l.malop 
                                                WHERE dk.madangky=@ma",
                new[] { new SqlParameter("@ma", maDK) });
            if (dtDK.Rows.Count == 0) return;

            if (dtDK.Rows[0]["trangthailop"].ToString().Contains("Hủy lớp"))
            {
                HienTB("Hồ sơ thuộc lớp đã hủy không thể đổi lớp trực tiếp!", true);
                return;
            }

            // M8: Chặn đổi nếu lớp cũ đã bắt đầu học
            object khCu = dtDK.Rows[0]["khcu"];
            if (khCu != null && khCu != DBNull.Value && Convert.ToDateTime(khCu).Date <= DateTime.Today)
            {
                HienTB("Không thể đổi lớp vì lớp học cũ đã bắt đầu (khai giảng)!", true);
                return;
            }

            if (dtDK.Rows[0]["malop"]?.ToString() == maLopMoi)
            {
                HienTB("Học viên đang nằm trong lớp này, không cần chuyển!", true);
                return;
            }

            if (dtDK.Rows[0]["makhoahoc"].ToString() != maKHLopMoi)
            {
                HienTB("Chỉ được phép đổi sang lớp khác thuộc CÙNG KHÓA HỌC!", true);
                return;
            }

            string maHV = dtDK.Rows[0]["mahocvien"].ToString();
            string lopCu = dtDK.Rows[0]["malop"]?.ToString();

            if (KiemTraTrungLichVaGoiY(maHV, maLopMoi, maKHLopMoi, maDK)) return;

            kncsdl.ThucThiLenh("UPDATE DANGKY SET malop=@lop WHERE madangky=@ma",
                new[] { new SqlParameter("@lop", maLopMoi), new SqlParameter("@ma", maDK) });

            CapNhatTrangThaiSiSoLop(lopCu);
            CapNhatTrangThaiSiSoLop(maLopMoi);
            kncsdl.GhiNhatKy(Session["TenDangNhap"]?.ToString(), "Đổi lớp học", $"Hồ sơ {maDK} chuyển từ {lopCu} sang {maLopMoi}");
            HienTB($"Đã chuyển đăng ký {maDK} sang lớp {maLopMoi} thành công!");
            TaiDanhMuc();
            TaiDuLieu();
        }

        protected void gvLopMo_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "DangKyLop")
            {
                string[] parts = e.CommandArgument.ToString().Split('|');
                string maLopChon = parts[0];
                string maKHChon = parts[1];

                object kgObj = kncsdl.LayGiaTriDon("SELECT ngaykhaigiang FROM LOPHOC WHERE malop=@l", new[] { new SqlParameter("@l", maLopChon) });
                if (kgObj != null && kgObj != DBNull.Value && Convert.ToDateTime(kgObj).Date <= DateTime.Today)
                {
                    HienTB($"Lớp {maLopChon} đã đến ngày khai giảng ({Convert.ToDateTime(kgObj):dd/MM/yyyy}) nên đã đóng đăng ký!", true);
                    TaiDuLieu();
                    return;
                }

                object huyObj = kncsdl.LayGiaTriDon("SELECT ISNULL(dahuy, 0) FROM LOPHOC WHERE malop=@l", new[] { new SqlParameter("@l", maLopChon) });
                if (huyObj != null && huyObj != DBNull.Value && Convert.ToBoolean(huyObj))
                {
                    HienTB($"Lớp {maLopChon} đã bị hủy do thiếu sĩ số nên không thể đăng ký!", true);
                    TaiDuLieu();
                    return;
                }

                if (ddlKhoaHoc.Items.FindByValue(maKHChon) != null)
                    ddlKhoaHoc.SelectedValue = maKHChon;
                TaiLopTheoKhoa(maKHChon);
                if (ddlLopHoc.Items.FindByValue(maLopChon) != null)
                    ddlLopHoc.SelectedValue = maLopChon;
                CapNhatHocPhiTheoKhoa(maKHChon);

                txtMaDK.Text = "";
                upPhieuDangKy.Update();
                HienTB($"Đã chọn lớp {maLopChon}. Hãy bấm 'Xác nhận Đăng ký' ở khung bên trái!");
            }
        }

        protected void gvDangKy_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ChonDK")
            {
                string maDK = e.CommandArgument.ToString();
                if (!KiemTraQuyenTrenDangKy(maDK)) return;

                DataTable dt = kncsdl.DocDuLieu("SELECT * FROM DANGKY WHERE madangky=@m", new[] { new SqlParameter("@m", maDK) });
                if (dt.Rows.Count > 0)
                {
                    DataRow r = dt.Rows[0];
                    txtMaDK.Text = r["madangky"].ToString();
                    if (ddlHocVien.Items.FindByValue(r["mahocvien"].ToString()) != null)
                        ddlHocVien.SelectedValue = r["mahocvien"].ToString();
                    ddlKhoaHoc.SelectedValue = r["makhoahoc"].ToString();
                    TaiLopTheoKhoa(r["makhoahoc"].ToString());
                    CapNhatHocPhiTheoKhoa(r["makhoahoc"].ToString());
                    if (r["malop"] != DBNull.Value && ddlLopHoc.Items.FindByValue(r["malop"].ToString()) != null)
                        ddlLopHoc.SelectedValue = r["malop"].ToString();
                    upPhieuDangKy.Update();
                }
            }
            else if (e.CommandName == "DongHocPhi")
            {
                pnlTB.Visible = false;
                string[] parts = e.CommandArgument.ToString().Split('|');
                string maDK = parts[0];

                if (!KiemTraQuyenTrenDangKy(maDK))
                {
                    HienTB("Bạn không có quyền thao tác trên hồ sơ này!", true);
                    return;
                }

                string ttLop = kncsdl.LayGiaTriDon("SELECT ISNULL(trangthailop, N'') FROM DANGKY WHERE madangky=@dk", new[] { new SqlParameter("@dk", maDK) })?.ToString() ?? "";
                if (ttLop.Contains("Hủy lớp"))
                {
                    HienTB("Hồ sơ này thuộc lớp đã hủy, không thể đóng thêm học phí!", true);
                    return;
                }

                object conNoObj = kncsdl.LayGiaTriDon(@"SELECT (kh.hocphi - ISNULL(SUM(CASE WHEN ISNULL(pt.noidungthu, N'') NOT LIKE N'Chờ xác nhận%' THEN pt.sotienthu ELSE 0 END), 0))
                                                       FROM DANGKY dk JOIN KHOAHOC kh ON dk.makhoahoc = kh.makhoahoc
                                                       LEFT JOIN PHIEUTHU pt ON dk.madangky = pt.madangky
                                                       WHERE dk.madangky = @dk GROUP BY kh.hocphi", new[] { new SqlParameter("@dk", maDK) });
                decimal conNo = conNoObj != null ? Convert.ToDecimal(conNoObj) : Convert.ToDecimal(parts[1]);

                hfMaDKThanhToan.Value = maDK;
                hfSoTienThanhToan.Value = conNo.ToString(CultureInfo.InvariantCulture);
                litSoTienQR.Text = conNo.ToString("N0") + " VNĐ";
                litNoiDungQR.Text = "HOCPHI " + maDK;
                string qrUrl = $"https://img.vietqr.io/image/VCB-1023407856-compact2.png?amount={(long)conNo}&addInfo=HOCPHI%20{maDK}&accountName=TRUNG%20TAM%20TIN%20HOC";
                imgQRCode.ImageUrl = qrUrl;
                lnkMoAnhQR.NavigateUrl = qrUrl;
                ScriptManager.RegisterStartupScript(this, GetType(), "qr_modal", "moModalQR();", true);
            }
            else if (e.CommandName == "MoYeuCauHoanTien")
            {
                string maDK = e.CommandArgument.ToString();
                if (!KiemTraQuyenTrenDangKy(maDK))
                {
                    HienTB("Bạn không có quyền yêu cầu hoàn tiền cho hồ sơ này!", true);
                    return;
                }
                MoPopupNhapThongTinHoanTien(maDK);
            }
            else if (e.CommandName == "XemBienLaiHoanHV")
            {
                string maDK = e.CommandArgument.ToString();
                if (!KiemTraQuyenTrenDangKy(maDK)) return;

                DataTable dt = kncsdl.DocDuLieu(@"SELECT TOP 1 ngaythu, ABS(sotienthu) AS sotienhoan, noidungthu 
                                                  FROM PHIEUTHU 
                                                  WHERE madangky = @dk AND (phuongthucthanhtoan = N'Hoàn tiền học phí' OR sotienthu < 0)
                                                  ORDER BY ngaythu DESC", new[] { new SqlParameter("@dk", maDK) });
                if (dt.Rows.Count == 0)
                {
                    HienTB("Không tìm thấy chứng từ hoàn tiền cho hồ sơ này!", true);
                    return;
                }

                DataRow r = dt.Rows[0];
                string nd = r["noidungthu"].ToString();
                var match = System.Text.RegularExpressions.Regex.Match(nd, @"Ảnh:\s*([^\)]+)");
                string fileName = match.Success ? match.Groups[1].Value.Trim() : "";
                string imgUrl = !string.IsNullOrEmpty(fileName) ? ResolveUrl("~/Uploads/BienLai/" + fileName) : "";

                litHVHoanMaDK.Text = HttpUtility.HtmlEncode(maDK);
                litHVHoanSoTien.Text = Convert.ToDecimal(r["sotienhoan"]).ToString("N0") + " VNĐ";
                litHVHoanNgay.Text = r["ngaythu"] != DBNull.Value ? Convert.ToDateTime(r["ngaythu"]).ToString("dd/MM/yyyy HH:mm") : "";
                litHVHoanChiTiet.Text = HttpUtility.HtmlEncode(nd);

                imgBienLaiHoanHV.ImageUrl = imgUrl;
                imgBienLaiHoanHV.Visible = !string.IsNullOrEmpty(imgUrl);
                lnkMoAnhHoanHV.NavigateUrl = imgUrl;
                lnkMoAnhHoanHV.Visible = !string.IsNullOrEmpty(imgUrl);

                pnlTB.Visible = false;
                ScriptManager.RegisterStartupScript(this, GetType(), "modal_xem_bl_hoan", "moModalXemBienLaiHoanHV();", true);
            }
        }

        protected void btnHuyDKChinhThuc_Click(object sender, EventArgs e)
        {
            string maDK = hfMaDKHuy.Value;
            if (string.IsNullOrEmpty(maDK)) return;

            if (!KiemTraQuyenTrenDangKy(maDK))
            {
                HienTB("Bạn không có quyền hủy đăng ký của học viên khác!", true);
                return;
            }

            if (Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM PHIEUTHU WHERE madangky=@m", new[] { new SqlParameter("@m", maDK) })) > 0)
            {
                HienTB("Đăng ký này đã phát sinh Phiếu thu (hoặc đang chờ duyệt biên lai), không thể xóa!", true);
                return;
            }
            string lopCu = kncsdl.LayGiaTriDon("SELECT malop FROM DANGKY WHERE madangky=@m", new[] { new SqlParameter("@m", maDK) })?.ToString();
            kncsdl.ThucThiLenh("DELETE FROM DANGKY WHERE madangky=@m", new[] { new SqlParameter("@m", maDK) });
            CapNhatTrangThaiSiSoLop(lopCu);
            kncsdl.GhiNhatKy(Session["TenDangNhap"]?.ToString(), "Hủy đăng ký học phần", $"Xóa hồ sơ đăng ký {maDK}");
            HienTB("Đã hủy đăng ký học phần thành công!");
            if (gvDangKy.Rows.Count == 1 && gvDangKy.PageIndex > 0) gvDangKy.PageIndex--;
            TaiDanhMuc();
            TaiDuLieu();
        }

        protected void btnXacNhanCK_Click(object sender, EventArgs e)
        {
            string maDK = hfMaDKThanhToan.Value;
            if (string.IsNullOrEmpty(maDK)) return;

            if (!KiemTraQuyenTrenDangKy(maDK))
            {
                HienTB("Bạn không có quyền gửi biên lai cho hồ sơ này!", true);
                return;
            }

            string ttLop = kncsdl.LayGiaTriDon("SELECT ISNULL(trangthailop, N'') FROM DANGKY WHERE madangky=@dk", new[] { new SqlParameter("@dk", maDK) })?.ToString() ?? "";
            if (ttLop.Contains("Hủy lớp"))
            {
                HienTB("Hồ sơ thuộc lớp đã hủy, không thể đóng học phí!", true);
                return;
            }

            if (!fuMinhChungCK.HasFile)
            {
                HienTB("Vui lòng chọn ảnh chụp màn hình chuyển khoản thành công trước khi gửi!", true);
                ScriptManager.RegisterStartupScript(this, GetType(), "qr_reopen", "moModalQR();", true);
                return;
            }

            string ext = Path.GetExtension(fuMinhChungCK.FileName)?.ToLowerInvariant() ?? "";
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".webp")
            {
                HienTB("Chỉ chấp nhận tệp hình ảnh (.jpg, .png, .webp)!", true);
                ScriptManager.RegisterStartupScript(this, GetType(), "qr_reopen2", "moModalQR();", true);
                return;
            }

            object conNoObj = kncsdl.LayGiaTriDon(@"SELECT (kh.hocphi - ISNULL(SUM(CASE WHEN ISNULL(pt.noidungthu, N'') NOT LIKE N'Chờ xác nhận%' THEN pt.sotienthu ELSE 0 END), 0))
                                                   FROM DANGKY dk JOIN KHOAHOC kh ON dk.makhoahoc = kh.makhoahoc
                                                   LEFT JOIN PHIEUTHU pt ON dk.madangky = pt.madangky
                                                   WHERE dk.madangky = @dk GROUP BY kh.hocphi", new[] { new SqlParameter("@dk", maDK) });
            decimal conNo = conNoObj != null ? Convert.ToDecimal(conNoObj) : 0;
            if (conNo <= 0)
            {
                HienTB("Hồ sơ đăng ký này đã đóng đủ học phí!", true);
                return;
            }

            int dangCho = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM PHIEUTHU WHERE madangky=@dk AND noidungthu LIKE N'Chờ xác nhận%'",
                new[] { new SqlParameter("@dk", maDK) }));
            if (dangCho > 0)
            {
                HienTB("Hồ sơ này đã gửi ảnh biên lai và đang chờ Nhân viên/Kế toán xác nhận!", true);
                return;
            }

            string folder = Server.MapPath("~/Uploads/BienLai");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            string fileName = $"BienLai_{maDK}_{DateTime.Now:yyyyMMdd_HHmmss}{ext}";
            fuMinhChungCK.SaveAs(Path.Combine(folder, fileName));

            int countPT = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM PHIEUTHU")) + 1;
            string maPT = "PT" + countPT.ToString("D2");
            while (Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM PHIEUTHU WHERE maphieuthu=@m", new[] { new SqlParameter("@m", maPT) })) > 0)
                maPT = "PT" + (++countPT).ToString("D2");

            using (SqlConnection conn = new SqlConnection(kncsdl.ChuoiKetNoi))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                try
                {
                    SqlCommand cmdPT = new SqlCommand(@"INSERT INTO PHIEUTHU(maphieuthu, ngaythu, sotienthu, nguoithu, phuongthucthanhtoan, noidungthu, madangky) 
                                                        VALUES(@ma, GETDATE(), @tien, @nguoithu, N'Chuyển khoản QR', @nd, @dk)", conn, tran);
                    cmdPT.Parameters.AddWithValue("@ma", maPT);
                    cmdPT.Parameters.AddWithValue("@tien", conNo);
                    cmdPT.Parameters.AddWithValue("@nguoithu", Session["HoTen"]?.ToString() ?? "Học viên");
                    cmdPT.Parameters.AddWithValue("@nd", "Chờ xác nhận CK QR (Ảnh: " + fileName + ")");
                    cmdPT.Parameters.AddWithValue("@dk", maDK);
                    cmdPT.ExecuteNonQuery();

                    SqlCommand cmdDK = new SqlCommand("UPDATE DANGKY SET trangthaithanhtoan = N'Chờ xác nhận' WHERE madangky = @m", conn, tran);
                    cmdDK.Parameters.AddWithValue("@m", maDK);
                    cmdDK.ExecuteNonQuery();

                    tran.Commit();
                    kncsdl.GhiNhatKy(Session["TenDangNhap"]?.ToString(), "Gửi biên lai QR", $"Hồ sơ {maDK}: {conNo:N0}đ");
                    HienTB($"Đã gửi ảnh biên lai chuyển khoản ({conNo:N0} đ) cho hồ sơ {maDK}! Vui lòng chờ Nhân viên/Kế toán xác nhận ở mục Phiếu thu.");
                    TaiDuLieu();
                }
                catch
                {
                    tran.Rollback();
                    HienTB("Có lỗi xảy ra khi gửi biên lai. Vui lòng thử lại!", true);
                }
            }
        }

        protected void btnGhepLop_Click(object sender, EventArgs e)
        {
            if (!LaQuanTriHoacNV())
            {
                HienTB("Bạn không có quyền thực hiện ghép lớp!", true);
                return;
            }

            string lopThieu = ddlLopThieu.SelectedValue;
            string lopDich = ddlLopDich.SelectedValue;

            if (string.IsNullOrEmpty(lopThieu) || string.IsNullOrEmpty(lopDich))
            {
                HienTB("Vui lòng chọn đầy đủ Lớp đang thiếu và Lớp đích!", true);
                return;
            }
            if (lopThieu == lopDich)
            {
                HienTB("Lớp đích phải khác lớp đang thiếu!", true);
                return;
            }

            object huyDich = kncsdl.LayGiaTriDon("SELECT ISNULL(dahuy, 0) FROM LOPHOC WHERE malop=@l", new[] { new SqlParameter("@l", lopDich) });
            if (huyDich != null && huyDich != DBNull.Value && Convert.ToBoolean(huyDich))
            {
                HienTB("Lớp đích đã bị hủy, vui lòng chọn lớp khác!", true);
                return;
            }

            string maKHThieu = kncsdl.LayGiaTriDon("SELECT makhoahoc FROM LOPHOC WHERE malop=@l", new[] { new SqlParameter("@l", lopThieu) })?.ToString();
            string maKHDich = kncsdl.LayGiaTriDon("SELECT makhoahoc FROM LOPHOC WHERE malop=@l", new[] { new SqlParameter("@l", lopDich) })?.ToString();
            if (maKHThieu != maKHDich)
            {
                HienTB("Không thể ghép! Lớp thiếu và Lớp đích phải thuộc CÙNG MỘT KHÓA HỌC.", true);
                return;
            }

            int ssThieu = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM DANGKY WHERE malop=@l", new[] { new SqlParameter("@l", lopThieu) }));
            int ssDich = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM DANGKY WHERE malop=@l", new[] { new SqlParameter("@l", lopDich) }));

            if (ssThieu + ssDich > SI_SO_CHUAN)
            {
                HienTB($"Không thể ghép! Tổng sĩ số sau ghép là {ssThieu + ssDich}/10 HV (vượt quá 10 học viên).", true);
                return;
            }

            DataTable dtHVThieu = kncsdl.DocDuLieu("SELECT dk.mahocvien, hv.hoten FROM DANGKY dk JOIN HOCVIEN hv ON dk.mahocvien = hv.mahocvien WHERE dk.malop=@l",
                new[] { new SqlParameter("@l", lopThieu) });
            foreach (DataRow rHV in dtHVThieu.Rows)
            {
                if (KiemTraTrungLichVaGoiY(rHV["mahocvien"].ToString(), lopDich, maKHDich))
                {
                    HienTB($"Không thể ghép! Học viên {rHV["hoten"]} ({rHV["mahocvien"]}) bị trùng lịch học với lớp đích {lopDich}.", true);
                    return;
                }
            }

            kncsdl.ThucThiLenh("UPDATE DANGKY SET malop=@dich WHERE malop=@thieu",
                new[] { new SqlParameter("@dich", lopDich), new SqlParameter("@thieu", lopThieu) });

            CapNhatTrangThaiSiSoLop(lopThieu);
            CapNhatTrangThaiSiSoLop(lopDich);
            kncsdl.GhiNhatKy(Session["TenDangNhap"]?.ToString(), "Ghép lớp thiếu sĩ số", $"Ghép {ssThieu} HV từ {lopThieu} sang {lopDich}");
            HienTB($"Đã ghép toàn bộ {ssThieu} học viên từ lớp {lopThieu} sang lớp {lopDich} thành công!");
            TaiDanhMuc();
            TaiDuLieu();
        }

        protected void btnMoDacCach_Click(object sender, EventArgs e)
        {
            if (!LaQuanTriHoacNV())
            {
                HienTB("Bạn không có quyền duyệt mở lớp đặc cách!", true);
                return;
            }

            string lopThieu = ddlLopThieu.SelectedValue;
            if (string.IsNullOrEmpty(lopThieu))
            {
                HienTB("Không có lớp thiếu nào được chọn!", true);
                return;
            }

            kncsdl.ThucThiLenh("UPDATE DANGKY SET trangthailop=N'Đã mở lớp (Đặc cách)' WHERE malop=@l",
                new[] { new SqlParameter("@l", lopThieu) });
            kncsdl.GhiNhatKy(Session["TenDangNhap"]?.ToString(), "Mở lớp đặc cách", $"Duyệt mở lớp đặc cách {lopThieu}");
            HienTB($"Đã duyệt mở lớp đặc cách cho lớp {lopThieu}!");
            TaiDuLieu();
        }

        protected void btnHuyLopThieu_Click(object sender, EventArgs e)
        {
            if (!LaQuanTriHoacNV())
            {
                HienTB("Bạn không có quyền hủy lớp học!", true);
                return;
            }

            string lopThieu = ddlLopThieu.SelectedValue;
            if (string.IsNullOrEmpty(lopThieu))
            {
                HienTB("Không có lớp thiếu nào được chọn!", true);
                return;
            }

            string tenLopHuy = kncsdl.LayGiaTriDon("SELECT tenlop FROM LOPHOC WHERE malop=@l", new[] { new SqlParameter("@l", lopThieu) })?.ToString() ?? lopThieu;
            string trangThaiHuy = $"Hủy lớp ({tenLopHuy})";
            if (trangThaiHuy.Length > 200) trangThaiHuy = trangThaiHuy.Substring(0, 199) + ")";

            using (SqlConnection conn = new SqlConnection(kncsdl.ChuoiKetNoi))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                try
                {
                    SqlCommand cmdDK = new SqlCommand("UPDATE DANGKY SET malop=NULL, trangthailop=@tt WHERE malop=@l", conn, tran);
                    cmdDK.Parameters.AddWithValue("@tt", trangThaiHuy);
                    cmdDK.Parameters.AddWithValue("@l", lopThieu);
                    cmdDK.ExecuteNonQuery();

                    SqlCommand cmdLop = new SqlCommand("UPDATE LOPHOC SET dahuy = 1 WHERE malop=@l", conn, tran);
                    cmdLop.Parameters.AddWithValue("@l", lopThieu);
                    cmdLop.ExecuteNonQuery();

                    tran.Commit();
                }
                catch
                {
                    tran.Rollback();
                    HienTB("Có lỗi xảy ra khi hủy lớp. Vui lòng thử lại!", true);
                    return;
                }
            }
            kncsdl.GhiNhatKy(Session["TenDangNhap"]?.ToString(), "Hủy lớp thiếu sĩ số", $"Hủy lớp {lopThieu} ({tenLopHuy})");
            HienTB($"Đã hủy lớp '{tenLopHuy}' và lưu thông tin bảo lưu học phí!");
            TaiDanhMuc();
            TaiDuLieu();
        }

        private void MoPopupNhapThongTinHoanTien(string maDK)
        {
            if (!KiemTraQuyenTrenDangKy(maDK)) return;

            DataTable dt = kncsdl.DocDuLieu(@"SELECT dk.madangky, dk.trangthailop, hv.hoten,
                                                     ISNULL(SUM(CASE WHEN ISNULL(pt.noidungthu, N'') NOT LIKE N'Chờ xác nhận%' THEN pt.sotienthu ELSE 0 END), 0) AS dadong
                                              FROM DANGKY dk
                                              JOIN HOCVIEN hv ON dk.mahocvien = hv.mahocvien
                                              LEFT JOIN PHIEUTHU pt ON dk.madangky = pt.madangky
                                              WHERE dk.madangky = @dk
                                              GROUP BY dk.madangky, dk.trangthailop, hv.hoten", new[] { new SqlParameter("@dk", maDK) });
            if (dt.Rows.Count == 0) return;

            decimal daDong = Convert.ToDecimal(dt.Rows[0]["dadong"]);
            if (daDong <= 0)
            {
                HienTB("Hồ sơ này chưa đóng học phí nên không phát sinh hoàn tiền!", true);
                return;
            }

            // M1: Server chặn yêu cầu hoàn tiền nếu lớp KHÔNG BỊ HỦY
            string ttHienTai = dt.Rows[0]["trangthailop"]?.ToString() ?? "";
            if (!ttHienTai.Contains("Hủy lớp"))
            {
                HienTB("Hồ sơ này không thuộc lớp bị hủy, không thể yêu cầu hoàn tiền!", true);
                return;
            }

            hfMaDKHoanTien.Value = maDK;
            litHTMaDK.Text = HttpUtility.HtmlEncode($"{maDK} - {dt.Rows[0]["hoten"]}");
            litHTSoTien.Text = daDong.ToString("N0") + " VNĐ";
            txtChuTKHT.Text = dt.Rows[0]["hoten"].ToString().ToUpper();
            txtNganHangHT.Text = "";
            txtSoTKHT.Text = "";

            string ttCu = dt.Rows[0]["trangthailop"]?.ToString() ?? "";
            int s1 = ttCu.IndexOf('[');
            int s2 = ttCu.IndexOf(']');
            if (s1 >= 0 && s2 > s1)
            {
                string stkCu = ttCu.Substring(s1 + 1, s2 - s1 - 1);
                if (stkCu.Contains("Tiền mặt"))
                {
                    ddlHinhThucHoan.SelectedValue = "Tiền mặt";
                }
                else
                {
                    ddlHinhThucHoan.SelectedValue = "Chuyển khoản";
                    string[] parts = stkCu.Split('-');
                    if (parts.Length >= 1) txtNganHangHT.Text = parts[0].Trim();
                    if (parts.Length >= 2) txtSoTKHT.Text = parts[1].Trim();
                    if (parts.Length >= 3) txtChuTKHT.Text = parts[2].Trim();
                }
            }

            pnlTB.Visible = false;
            ScriptManager.RegisterStartupScript(this, GetType(), "modal_nhapstk", "moModalNhapSTKHoanTien();", true);
        }

        protected void btnGuiYeuCauHoanTien_Click(object sender, EventArgs e)
        {
            string maDK = hfMaDKHoanTien.Value;
            if (string.IsNullOrEmpty(maDK)) return;

            if (!KiemTraQuyenTrenDangKy(maDK))
            {
                HienTB("Bạn không có quyền gửi thông tin hoàn tiền cho hồ sơ này!", true);
                return;
            }

            string ttCu = kncsdl.LayGiaTriDon("SELECT ISNULL(trangthailop, N'Hủy lớp') FROM DANGKY WHERE madangky=@dk",
                new[] { new SqlParameter("@dk", maDK) })?.ToString() ?? "Hủy lớp";

            // M1: Kiểm tra thêm ở server
            if (!ttCu.Contains("Hủy lớp"))
            {
                HienTB("Hồ sơ này không thuộc lớp bị hủy, không thể lưu STK hoàn tiền!", true);
                return;
            }

            string hinhThuc = ddlHinhThucHoan.SelectedValue;
            string thongTinNhan = "";

            if (hinhThuc == "Tiền mặt")
            {
                thongTinNhan = "Tiền mặt tại TT";
            }
            else
            {
                string nh = txtNganHangHT.Text.Trim().Replace("-", " ");
                string stk = txtSoTKHT.Text.Trim().Replace("-", "");
                string chuTK = txtChuTKHT.Text.Trim().ToUpper().Replace("-", " ");

                if (string.IsNullOrEmpty(nh) || string.IsNullOrEmpty(stk) || string.IsNullOrEmpty(chuTK))
                {
                    HienTB("Vui lòng nhập đầy đủ Tên ngân hàng, Số tài khoản và Tên chủ tài khoản!", true);
                    ScriptManager.RegisterStartupScript(this, GetType(), "reopen_stk", "moModalNhapSTKHoanTien();", true);
                    return;
                }
                thongTinNhan = $"{nh}-{stk}-{chuTK}";
            }

            int idxDash = ttCu.IndexOf(" - ");
            string prefixLop = idxDash > 0 ? ttCu.Substring(0, idxDash) : ttCu;
            string ttMoi = $"{prefixLop} - Chờ hoàn [{thongTinNhan}]";
            if (ttMoi.Length > 200) ttMoi = ttMoi.Substring(0, 199) + "]";

            kncsdl.ThucThiLenh("UPDATE DANGKY SET trangthailop=@tt, trangthaithanhtoan=N'Chờ hoàn tiền' WHERE madangky=@dk",
                new[] { new SqlParameter("@tt", ttMoi), new SqlParameter("@dk", maDK) });

            kncsdl.GhiNhatKy(Session["TenDangNhap"]?.ToString(), "Gửi STK hoàn tiền", $"Hồ sơ {maDK}: {thongTinNhan}");

            litNoiDungTBHoanTien.Text = $"Đã lưu thông tin nhận hoàn tiền <strong>({HttpUtility.HtmlEncode(thongTinNhan)})</strong> cho hồ sơ <strong>{HttpUtility.HtmlEncode(maDK)}</strong>.<br/>Kế toán sẽ kiểm tra và chuyển khoản hoàn trả học phí trong thời gian sớm nhất.";
            ScriptManager.RegisterStartupScript(this, GetType(), "modal_tb_ht", "moModalThongBaoHoanTien();", true);
            TaiDuLieu();
        }

        private void TaiDanhSachLopBiHuy()
        {
            string sql = @"SELECT dk.madangky, hv.mahocvien, hv.hoten, hv.dienthoai, kh.tenkhoahoc,
                                  CASE 
                                      WHEN CHARINDEX('(', dk.trangthailop) > 0 AND CHARINDEX(')', dk.trangthailop) > CHARINDEX('(', dk.trangthailop)
                                      THEN SUBSTRING(dk.trangthailop, CHARINDEX('(', dk.trangthailop) + 1, CHARINDEX(')', dk.trangthailop) - CHARINDEX('(', dk.trangthailop) - 1)
                                      ELSE N'Lớp đã hủy (chưa rõ tên)'
                                  END AS tenlopbihuy,
                                  CASE 
                                      WHEN CHARINDEX('[', dk.trangthailop) > 0 AND CHARINDEX(']', dk.trangthailop) > CHARINDEX('[', dk.trangthailop)
                                      THEN SUBSTRING(dk.trangthailop, CHARINDEX('[', dk.trangthailop) + 1, CHARINDEX(']', dk.trangthailop) - CHARINDEX('[', dk.trangthailop) - 1)
                                      ELSE N''
                                  END AS thongtinstk,
                                  ISNULL(SUM(CASE WHEN ISNULL(pt.noidungthu, N'') NOT LIKE N'Chờ xác nhận%' THEN pt.sotienthu ELSE 0 END), 0) AS dadong,
                                  dk.trangthailop
                           FROM DANGKY dk
                           JOIN HOCVIEN hv ON dk.mahocvien = hv.mahocvien
                           JOIN KHOAHOC kh ON dk.makhoahoc = kh.makhoahoc
                           LEFT JOIN PHIEUTHU pt ON dk.madangky = pt.madangky
                           WHERE ISNULL(dk.trangthailop, N'') LIKE N'%Hủy lớp%'
                           GROUP BY dk.madangky, hv.mahocvien, hv.hoten, hv.dienthoai, kh.tenkhoahoc, dk.trangthailop
                           ORDER BY 
                               CASE 
                                   WHEN ISNULL(dk.trangthailop, N'') LIKE N'%Chờ hoàn%' THEN 0
                                   WHEN ISNULL(dk.trangthailop, N'') NOT LIKE N'%Đã hoàn tiền%' AND ISNULL(SUM(CASE WHEN ISNULL(pt.noidungthu, N'') NOT LIKE N'Chờ xác nhận%' THEN pt.sotienthu ELSE 0 END), 0) > 0 THEN 1
                                   WHEN ISNULL(dk.trangthailop, N'') LIKE N'%Đã hoàn tiền%' THEN 2
                                   ELSE 3
                               END,
                               dk.madangky DESC";
            gvLopBiHuy.DataSource = kncsdl.DocDuLieu(sql);
            gvLopBiHuy.DataBind();
        }

        protected void btnXemLopHuyHoanTien_Click(object sender, EventArgs e)
        {
            if (!LaQuanTriHoacNV())
            {
                HienTB("Bạn không có quyền truy cập danh sách hoàn tiền!", true);
                return;
            }
            TaiDanhSachLopBiHuy();
            ScriptManager.RegisterStartupScript(this, GetType(), "modal_lophuy", "moModalLopBiHuy();", true);
        }

        protected void gvLopBiHuy_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!LaQuanTriHoacNV()) return;

            if (e.CommandName == "MoYeuCauHoanTien")
            {
                MoPopupNhapThongTinHoanTien(e.CommandArgument.ToString());
                return;
            }

            if (e.CommandName == "MoQRHoanTien")
            {
                string maDK = e.CommandArgument.ToString();
                DataTable dt = kncsdl.DocDuLieu(@"SELECT dk.trangthailop, hv.hoten,
                                                         ISNULL(SUM(CASE WHEN ISNULL(pt.noidungthu, N'') NOT LIKE N'Chờ xác nhận%' THEN pt.sotienthu ELSE 0 END), 0) AS dadong
                                                  FROM DANGKY dk
                                                  JOIN HOCVIEN hv ON dk.mahocvien = hv.mahocvien
                                                  LEFT JOIN PHIEUTHU pt ON dk.madangky = pt.madangky
                                                  WHERE dk.madangky = @dk
                                                  GROUP BY dk.trangthailop, hv.hoten", new[] { new SqlParameter("@dk", maDK) });
                if (dt.Rows.Count == 0) return;

                decimal daDong = Convert.ToDecimal(dt.Rows[0]["dadong"]);
                string hoTen = dt.Rows[0]["hoten"].ToString();
                string ttCu = dt.Rows[0]["trangthailop"].ToString();

                string thongTinSTK = "";
                int s1 = ttCu.IndexOf('[');
                int s2 = ttCu.IndexOf(']');
                if (s1 >= 0 && s2 > s1)
                    thongTinSTK = ttCu.Substring(s1 + 1, s2 - s1 - 1);

                hfMaDKAdminHoanTien.Value = maDK;
                litAdminHTHocVien.Text = HttpUtility.HtmlEncode($"{hoTen} ({maDK})");
                litAdminHTSTK.Text = HttpUtility.HtmlEncode(thongTinSTK);
                litAdminHTSoTien.Text = daDong.ToString("N0") + " VNĐ";

                if (thongTinSTK.Contains("Tiền mặt"))
                {
                    pnlQRHoanTienBox.Visible = false;
                }
                else
                {
                    pnlQRHoanTienBox.Visible = true;
                    string[] parts = thongTinSTK.Split('-');
                    string bankCode = parts.Length >= 1 ? parts[0].Trim() : "VCB";
                    string accNo = parts.Length >= 2 ? parts[1].Trim() : "";
                    string accName = parts.Length >= 3 ? HttpUtility.UrlEncode(parts[2].Trim()) : "";
                    string addInfo = HttpUtility.UrlEncode($"HOANTIEN {maDK}");
                    imgQRHoanTien.ImageUrl = $"https://img.vietqr.io/image/{bankCode}-{accNo}-compact2.png?amount={(long)daDong}&addInfo={addInfo}&accountName={accName}";
                }

                ScriptManager.RegisterStartupScript(this, GetType(), "open_qr_ht_admin", "moModalQRHoanTienAdmin();", true);
            }
        }

        protected void btnXacNhanHoanTienAdmin_Click(object sender, EventArgs e)
        {
            if (!LaQuanTriHoacNV())
            {
                HienTB("Chỉ Quản trị viên hoặc Nhân viên kế toán mới có quyền xác nhận hoàn tiền!", true);
                return;
            }

            string maDK = hfMaDKAdminHoanTien.Value;
            if (string.IsNullOrEmpty(maDK)) return;

            if (!fuBienLaiHoanTien.HasFile)
            {
                HienTB("Vui lòng tải lên ảnh chụp màn hình biên lai đã chuyển hoàn tiền trước khi xác nhận!", true);
                ScriptManager.RegisterStartupScript(this, GetType(), "reopen_qr_ht", "moModalQRHoanTienAdmin();", true);
                return;
            }

            string ext = Path.GetExtension(fuBienLaiHoanTien.FileName)?.ToLowerInvariant() ?? "";
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".webp")
            {
                HienTB("Chỉ chấp nhận tệp hình ảnh (.jpg, .png, .webp)!", true);
                ScriptManager.RegisterStartupScript(this, GetType(), "reopen_qr_ht2", "moModalQRHoanTienAdmin();", true);
                return;
            }

            DataTable dt = kncsdl.DocDuLieu(@"SELECT dk.trangthailop, hv.hoten,
                                                     ISNULL(SUM(CASE WHEN ISNULL(pt.noidungthu, N'') NOT LIKE N'Chờ xác nhận%' THEN pt.sotienthu ELSE 0 END), 0) AS dadong
                                              FROM DANGKY dk
                                              JOIN HOCVIEN hv ON dk.mahocvien = hv.mahocvien
                                              LEFT JOIN PHIEUTHU pt ON dk.madangky = pt.madangky
                                              WHERE dk.madangky = @dk
                                              GROUP BY dk.trangthailop, hv.hoten", new[] { new SqlParameter("@dk", maDK) });
            if (dt.Rows.Count == 0) return;

            decimal daDong = Convert.ToDecimal(dt.Rows[0]["dadong"]);
            string hoTen = dt.Rows[0]["hoten"].ToString();
            string ttCu = dt.Rows[0]["trangthailop"].ToString();

            if (daDong <= 0)
            {
                HienTB("Hồ sơ này đã được hoàn tiền trước đó!", true);
                return;
            }

            // C3: Chặn nếu hồ sơ còn biên lai chưa duyệt
            int dangCho = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM PHIEUTHU WHERE madangky=@dk AND noidungthu LIKE N'Chờ xác nhận%'",
                new[] { new SqlParameter("@dk", maDK) }));
            if (dangCho > 0)
            {
                HienTB("Hồ sơ này đang có biên lai quét QR đóng học phí chờ duyệt! Vui lòng từ chối biên lai đó trước khi hoàn tiền.", true);
                return;
            }

            string folder = Server.MapPath("~/Uploads/BienLai");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            string fileName = $"HoanTien_{maDK}_{DateTime.Now:yyyyMMdd_HHmmss}{ext}";
            fuBienLaiHoanTien.SaveAs(Path.Combine(folder, fileName));

            string thongTinSTK = "";
            int s1 = ttCu.IndexOf('[');
            int s2 = ttCu.IndexOf(']');
            if (s1 >= 0 && s2 > s1)
                thongTinSTK = ttCu.Substring(s1 + 1, s2 - s1 - 1);

            int countPT = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM PHIEUTHU")) + 1;
            string maPT = "PT" + countPT.ToString("D2");
            while (Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM PHIEUTHU WHERE maphieuthu=@m", new[] { new SqlParameter("@m", maPT) })) > 0)
                maPT = "PT" + (++countPT).ToString("D2");

            string ttMoi = ttCu.Replace("Chờ hoàn", "Đã hoàn tiền");
            if (ttMoi == ttCu) ttMoi = ttCu + " - Đã hoàn tiền";

            using (SqlConnection conn = new SqlConnection(kncsdl.ChuoiKetNoi))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                try
                {
                    SqlCommand cmdPT = new SqlCommand(@"INSERT INTO PHIEUTHU(maphieuthu, ngaythu, sotienthu, nguoithu, phuongthucthanhtoan, noidungthu, madangky)
                                                        VALUES(@ma, GETDATE(), @tien, @nguoi, N'Hoàn tiền học phí', @nd, @dk)", conn, tran);
                    cmdPT.Parameters.AddWithValue("@ma", maPT);
                    cmdPT.Parameters.AddWithValue("@tien", -daDong);
                    cmdPT.Parameters.AddWithValue("@nguoi", Session["HoTen"]?.ToString() ?? "Admin");
                    cmdPT.Parameters.AddWithValue("@nd", $"Đã hoàn tiền cho {hoTen} ({maDK}) -> {thongTinSTK} (Ảnh: {fileName})");
                    cmdPT.Parameters.AddWithValue("@dk", maDK);
                    cmdPT.ExecuteNonQuery();

                    SqlCommand cmdDK = new SqlCommand("UPDATE DANGKY SET sotiendong = 0, trangthaithanhtoan = N'Đã hoàn tiền', trangthailop = @tt WHERE madangky = @dk", conn, tran);
                    cmdDK.Parameters.AddWithValue("@tt", ttMoi);
                    cmdDK.Parameters.AddWithValue("@dk", maDK);
                    cmdDK.ExecuteNonQuery();

                    tran.Commit();
                    kncsdl.GhiNhatKy(Session["TenDangNhap"]?.ToString(), "Hoàn tiền hủy lớp", $"Đã hoàn {daDong:N0}đ cho {hoTen} ({maDK}) qua {thongTinSTK}");

                    litNoiDungTBHoanTien.Text = $"Đã lưu ảnh biên lai và xác nhận hoàn trả <strong>{daDong:N0} đ</strong> cho học viên <strong>{HttpUtility.HtmlEncode(hoTen)}</strong> ({HttpUtility.HtmlEncode(thongTinSTK)}) thành công!";
                    ScriptManager.RegisterStartupScript(this, GetType(), "modal_tb_ht2", "moModalThongBaoHoanTien();", true);
                    TaiDuLieu();
                }
                catch
                {
                    tran.Rollback();
                    HienTB("Có lỗi xảy ra khi xác nhận hoàn tiền. Vui lòng thử lại!", true);
                }
            }
        }

        protected void ddlKhoaHoc_SelectedIndexChanged(object sender, EventArgs e)
        {
            TaiLopTheoKhoa(ddlKhoaHoc.SelectedValue);
            CapNhatHocPhiTheoKhoa(ddlKhoaHoc.SelectedValue);
        }

        protected void ddlLocKhoaMo_SelectedIndexChanged(object sender, EventArgs e)
        {
            TaiDuLieu();
        }

        protected void btnTimDK_Click(object sender, EventArgs e)
        {
            TuKhoaDK = txtTimDK.Text.Trim();
            gvDangKy.PageIndex = 0;
            TaiDuLieu();
            pnlTB.Visible = false;
        }

        protected void gvDangKy_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvDangKy.PageIndex = e.NewPageIndex;
            TaiDuLieu();
            pnlTB.Visible = false;
        }

        private void HienTB(string msg, bool laLoi = false)
        {
            pnlTB.Visible = true;
            pnlTB.CssClass = laLoi
                ? "alert alert-danger text-danger fw-semibold alert-dismissible fade show shadow"
                : "alert alert-success text-success fw-semibold alert-dismissible fade show shadow";
            litTB.Text = HttpUtility.HtmlEncode(msg);
        }

        // M4: Đã bổ sung lọc thêm khoảng thời gian lớp học để không bắt trùng khi lớp cũ đã kết thúc.
        private bool KiemTraTrungLichVaGoiY(string maHV, string maLopMoi, string maKH, string maDKBoQua = "")
        {
            DataTable dtLichMoi = kncsdl.DocDuLieu("SELECT l.ngaykhaigiang, kh.thoigianhoc, lh.cahoc, lh.thu FROM LICHHOC lh JOIN LOPHOC l ON lh.malop=l.malop JOIN KHOAHOC kh ON l.makhoahoc=kh.makhoahoc WHERE l.malop=@l", new[] { new SqlParameter("@l", maLopMoi) });
            if (dtLichMoi.Rows.Count == 0 || dtLichMoi.Rows[0]["ngaykhaigiang"] == DBNull.Value) return false;

            DateTime kgMoi = Convert.ToDateTime(dtLichMoi.Rows[0]["ngaykhaigiang"]).Date;
            DateTime ktMoi = TinhNgayKetThucLop(kgMoi, dtLichMoi.Rows[0]["thoigianhoc"].ToString());
            string caMoi = dtLichMoi.Rows[0]["cahoc"].ToString().Trim();
            string thuMoi = dtLichMoi.Rows[0]["thu"].ToString().Trim();

            // Lấy tất cả lịch của học viên đang học (trong đó lớp chưa kết thúc)
            DataTable dtTrung = kncsdl.DocDuLieu(@"SELECT l.malop, l.tenlop, lh.cahoc, lh.thu, l.ngaykhaigiang, kh.thoigianhoc 
                                                   FROM DANGKY dk 
                                                   JOIN LOPHOC l ON dk.malop = l.malop 
                                                   JOIN KHOAHOC kh ON l.makhoahoc = kh.makhoahoc
                                                   JOIN LICHHOC lh ON l.malop = lh.malop 
                                                   WHERE dk.mahocvien = @hv AND (@madk = '' OR dk.madangky <> @madk) 
                                                     AND ISNULL(l.dahuy, 0) = 0 AND l.ngaykhaigiang IS NOT NULL",
                new[] { new SqlParameter("@hv", maHV), new SqlParameter("@madk", maDKBoQua ?? "") });

            string[] dsThuMoi = { "2", "3", "4", "5", "6", "7", "CN" };
            foreach (DataRow rTrung in dtTrung.Rows)
            {
                DateTime kgCu = Convert.ToDateTime(rTrung["ngaykhaigiang"]).Date;
                DateTime ktCu = TinhNgayKetThucLop(kgCu, rTrung["thoigianhoc"].ToString());
                // Không xét trùng nếu lớp đã học xong trước khi lớp mới bắt đầu, hoặc ngược lại
                if (ktCu < kgMoi || ktMoi < kgCu) continue;

                if (!string.Equals(rTrung["cahoc"].ToString().Trim(), caMoi, StringComparison.OrdinalIgnoreCase)) continue;

                string thuCu = rTrung["thu"].ToString().ToUpper();
                foreach (string t in dsThuMoi)
                {
                    if (thuMoi.ToUpper().Contains(t) && thuCu.Contains(t))
                    {
                        string lopBiTrung = rTrung["tenlop"].ToString();

                        // Đã phát hiện trùng -> tìm lớp trống để gợi ý
                        DataTable dtGoiY1 = kncsdl.DocDuLieu(@"SELECT l.malop, l.tenlop, lh.cahoc, lh.thu, lh.giohoc FROM LOPHOC l JOIN LICHHOC lh ON l.malop = lh.malop 
                                                               WHERE ISNULL(l.dahuy, 0) = 0 AND l.makhoahoc = @kh AND l.malop <> @lopmoi AND NOT (lh.cahoc = @ca AND lh.thu = @thu) AND (SELECT COUNT(*) FROM DANGKY d WHERE d.malop = l.malop) < 10",
                            new[] { new SqlParameter("@kh", maKH), new SqlParameter("@lopmoi", maLopMoi), new SqlParameter("@ca", caMoi), new SqlParameter("@thu", thuMoi) });

                        if (dtGoiY1.Rows.Count > 0)
                        {
                            DataRow rGY = dtGoiY1.Rows[0];
                            HienTB($"TRÙNG LỊCH ({caMoi} - {thuMoi}) với '{lopBiTrung}'! GỢI Ý: Chọn lớp '{rGY["tenlop"]}' ({rGY["malop"]}) vào {rGY["cahoc"]} - {rGY["thu"]} không bị trùng!", true);
                            return true;
                        }
                        HienTB($"TRÙNG LỊCH HỌC ({caMoi} - {thuMoi}) với lớp '{lopBiTrung}'! Khóa học này không còn lớp nào khác giờ trống chỗ.", true);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}