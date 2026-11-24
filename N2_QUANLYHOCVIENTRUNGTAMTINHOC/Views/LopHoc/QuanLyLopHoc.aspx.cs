using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using N2_QUANLYHOCVIENTRUNGTAMTINHOC.DAL;

namespace N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.LopHoc
{
    public partial class QuanLyLopHoc : System.Web.UI.Page
    {
        private string QuyenHienTai => MasterPage.ChuanHoaQuyen(Session["Quyen"]);
        private bool LaAdmin => QuyenHienTai == "Admin";
        private bool LaQuanTriHoacNV => QuyenHienTai == "Admin" || QuyenHienTai == "NhanVien";
        private bool LaGiangVien => QuyenHienTai == "GiangVien";
        private bool LaHocVien => QuyenHienTai == "HocVien";
        private string MaNguoiDung => Session["TenDangNhap"]?.ToString() ?? "";

        private string TuKhoaHienTai
        {
            get { return ViewState["TuKhoaTimLop"]?.ToString() ?? ""; }
            set { ViewState["TuKhoaTimLop"] = value; }
        }

        private DateTime NgayMocTuanTKB
        {
            get
            {
                if (ViewState["NgayMocTuanTKB"] != null)
                    return (DateTime)ViewState["NgayMocTuanTKB"];
                return DateTime.Today;
            }
            set { ViewState["NgayMocTuanTKB"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
           
            DamBaoBangLichNghiBuTonTai();

            if (IsPostBack)
            {
                pnlTB.Visible = false;
            }
            else
            {
                TuKhoaHienTai = "";
                txtTimLop.Text = "";
                NgayMocTuanTKB = DateTime.Today;
                txtChonNgayTKB.Text = DateTime.Today.ToString("yyyy-MM-dd");
                txtKhaiGiang.Text = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
                txtNgayDiemDanh.Text = DateTime.Now.ToString("yyyy-MM-dd");
                CauHinhPhanQuyen();
                TaiDanhMuc();
                SinhMaLopMoi();
                TaiDanhSachLop();
            }
        }

        private void DamBaoBangLichNghiBuTonTai()
        {
            try
            {
                string sql = @"IF OBJECT_ID('LICH_NGHI_BU', 'U') IS NULL
                               BEGIN
                                   CREATE TABLE LICH_NGHI_BU (
                                       id INT IDENTITY(1,1) PRIMARY KEY,
                                       malop VARCHAR(20) NOT NULL,
                                       ngaynghi DATE NOT NULL,
                                       ngaybu DATE NULL,
                                       cahoc_bu NVARCHAR(50) NULL,
                                       giohoc_bu NVARCHAR(50) NULL,
                                       phonghoc_bu NVARCHAR(50) NULL,
                                       lydo NVARCHAR(255) NULL,
                                       nguoitao VARCHAR(50) NULL
                                   );
                               END";
                kncsdl.ThucThiLenh(sql);
            }
            catch { }
        }

        private void SinhMaLopMoi()
        {
            object maxObj = kncsdl.LayGiaTriDon("SELECT ISNULL(MAX(CAST(SUBSTRING(malop, 3, 10) AS INT)), 0) FROM LOPHOC WHERE malop LIKE 'LH[0-9]%'");
            int soLuong = Convert.ToInt32(maxObj) + 1;
            string maLop = "LH" + soLuong.ToString("D2");
            while (Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM LOPHOC WHERE malop=@ma", new[] { new SqlParameter("@ma", maLop) })) > 0)
            {
                soLuong++;
                maLop = "LH" + soLuong.ToString("D2");
            }
            txtMaLop.Text = maLop;
            txtMaLop.ReadOnly = true;
        }

        private void CauHinhPhanQuyen()
        {
            if (LaGiangVien || LaHocVien)
            {
                pnlFormAdmin.Visible = false;
                colDanhSachLop.Attributes["class"] = "col-12";
                litGhiChuTKB.Text = "(Theo dõi lịch học chính, ngày nghỉ đột xuất và lịch học bù trong tuần)";
            }
            else
            {
                litGhiChuTKB.Text = "(Bấm vào ô lớp để báo nghỉ đột xuất & xếp bù vào tuần dự trữ)";
            }

            if (LaHocVien)
            {
                pnlTimKiemLop.Visible = false;
                litTieuDeBangLop.Text = "Danh sách Lớp học đã đăng ký của tôi";
                gvLopHoc.Columns[gvLopHoc.Columns.Count - 1].Visible = false;
            }
        }

        private void TaiDanhMuc(string giuMaKH = "")
        {
            string sqlKH = @"SELECT makhoahoc, tenkhoahoc FROM KHOAHOC 
                             WHERE trangthaikhoahoc = N'Đang mở' OR (@giu <> '' AND makhoahoc = @giu)
                             ORDER BY tenkhoahoc";
            ddlKhoaHoc.DataSource = kncsdl.DocDuLieu(sqlKH, new[] { new SqlParameter("@giu", giuMaKH ?? "") });
            ddlKhoaHoc.DataTextField = "tenkhoahoc";
            ddlKhoaHoc.DataValueField = "makhoahoc";
            ddlKhoaHoc.DataBind();

            ddlGiangVien.DataSource = kncsdl.DocDuLieu("SELECT magiangvien, hoten + ISNULL(' (' + hocvi + ')', '') AS HienThi FROM GIANGVIEN ORDER BY hoten");
            ddlGiangVien.DataTextField = "HienThi";
            ddlGiangVien.DataValueField = "magiangvien";
            ddlGiangVien.DataBind();
            ddlGiangVien.Items.Insert(0, new ListItem("-- Chọn Giảng viên phụ trách --", ""));
        }

        private void TaiDanhSachLop()
        {
            string kw = TuKhoaHienTai;

            string sql = @"SELECT l.malop, l.tenlop, l.makhoahoc, kh.tenkhoahoc, kh.thoigianhoc,
                                  ISNULL(gv.hoten + ISNULL(N' (' + gv.hocvi + N')', N''), N'Chưa phân công') AS hotengv, l.ngaykhaigiang,
                                  ISNULL((SELECT TOP 1 lh.thu FROM LICHHOC lh WHERE lh.malop = l.malop), N'Thứ 2, 4, 6') AS thuhoc,
                                  ISNULL(STUFF((SELECT '; ' + lh.cahoc + ' - ' + lh.thu + ' (' + lh.giohoc + ', P.' + lh.phonghoc + ')'
                                                FROM LICHHOC lh WHERE lh.malop = l.malop FOR XML PATH('')), 1, 2, ''), N'Đang cập nhật lịch') AS thoikhoabieu,
                                  CASE WHEN ISNULL(l.dahuy, 0) = 1 THEN N'Đã hủy (thiếu sĩ số)'
                                       ELSE CAST((SELECT COUNT(*) FROM DANGKY dk WHERE dk.malop = l.malop) AS NVARCHAR(10)) + '/10 HV' END AS siso
                           FROM LOPHOC l 
                           JOIN KHOAHOC kh ON l.makhoahoc = kh.makhoahoc 
                           LEFT JOIN GIANGVIEN gv ON l.magiangvien = gv.magiangvien
                           WHERE (@kw = '' OR l.malop COLLATE Latin1_General_CI_AI LIKE @kw 
                                           OR l.tenlop COLLATE Latin1_General_CI_AI LIKE @kw 
                                           OR kh.tenkhoahoc COLLATE Latin1_General_CI_AI LIKE @kw)
                             AND (@isGV = 0 OR LTRIM(RTRIM(l.magiangvien)) = LTRIM(RTRIM(@user)))
                             AND (@isHV = 0 OR l.malop IN (SELECT malop FROM DANGKY WHERE mahocvien = @user))
                           ORDER BY 
                               CASE 
                                   WHEN ISNULL(l.dahuy, 0) = 0 AND l.ngaykhaigiang >= CAST(GETDATE() AS DATE) THEN 0
                                   WHEN ISNULL(l.dahuy, 0) = 0 THEN 1
                                   ELSE 2
                               END ASC,
                               CASE WHEN l.ngaykhaigiang >= CAST(GETDATE() AS DATE) THEN l.ngaykhaigiang END ASC,
                               l.ngaykhaigiang DESC,
                               l.malop DESC";

            SqlParameter[] p = {
                new SqlParameter("@kw", string.IsNullOrEmpty(kw) ? "" : "%" + kw + "%"),
                new SqlParameter("@isGV", LaGiangVien ? 1 : 0),
                new SqlParameter("@isHV", LaHocVien ? 1 : 0),
                new SqlParameter("@user", MaNguoiDung)
            };

            DataTable dt = kncsdl.DocDuLieu(sql, p);
            dt.Columns.Add("thoigianlop", typeof(string));
            foreach (DataRow r in dt.Rows)
            {
                if (r["ngaykhaigiang"] != DBNull.Value)
                {
                    DateTime batDau = Convert.ToDateTime(r["ngaykhaigiang"]);
                    string thuHoc = r["thuhoc"]?.ToString() ?? "Thứ 2, 4, 6";
                    DateTime ketThucThucHoc = TinhNgayKetThuc(batDau, r["thoigianhoc"]?.ToString() ?? "", thuHoc, false);
                    DateTime ketThucCoDuTru = TinhNgayKetThuc(batDau, r["thoigianhoc"]?.ToString() ?? "", thuHoc, true);
                    r["thoigianlop"] = $"{batDau:dd/MM/yyyy} - {ketThucThucHoc:dd/MM/yyyy} (Dự trữ đến {ketThucCoDuTru:dd/MM})";
                }
                else
                {
                    r["thoigianlop"] = "Chưa xác định";
                }
            }

            gvLopHoc.DataSource = dt;
            gvLopHoc.DataBind();

            TaoBangLuoiThoiKhoaBieu();
            TaiGoiYTimKiemLop();
        }

        private DateTime TinhNgayKetThuc(DateTime ngayBatDau, string thoiGianHoc, string thuHoc = "Thứ 2, 4, 6", bool coTuanDuTru = true)
        {
            int soBuoiTuan = string.IsNullOrWhiteSpace(thuHoc) ? 3 : Math.Max(1, thuHoc.Split(',').Length);
            DateTime ngayKetThucThuc = ngayBatDau.AddMonths(2);

            if (!string.IsNullOrWhiteSpace(thoiGianHoc))
            {
                var mBuoi = Regex.Match(thoiGianHoc, @"(\d+)\s*buổi", RegexOptions.IgnoreCase);
                var mThang = Regex.Match(thoiGianHoc, @"(\d+(?:[\.,]\d+)?)\s*tháng", RegexOptions.IgnoreCase);
                var mTuan = Regex.Match(thoiGianHoc, @"(\d+(?:[\.,]\d+)?)\s*tuần", RegexOptions.IgnoreCase);

                if (mBuoi.Success && int.TryParse(mBuoi.Groups[1].Value, out int buoi) && buoi > 0)
                {
                    ngayKetThucThuc = ngayBatDau.AddDays((int)Math.Ceiling((double)buoi / soBuoiTuan) * 7);
                }
                else if (mThang.Success && decimal.TryParse(mThang.Groups[1].Value.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal thang) && thang > 0)
                {
                    int thangNguyen = (int)Math.Floor(thang);
                    int ngayLe = (int)Math.Round((thang - thangNguyen) * 30m);
                    ngayKetThucThuc = ngayBatDau.AddMonths(thangNguyen).AddDays(ngayLe);
                }
                else if (mTuan.Success && decimal.TryParse(mTuan.Groups[1].Value.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal tuan) && tuan > 0)
                {
                    ngayKetThucThuc = ngayBatDau.AddDays((int)Math.Round(tuan * 7m));
                }
            }

            return coTuanDuTru ? ngayKetThucThuc.AddDays(7) : ngayKetThucThuc;
        }
        private int TinhTongBuoiHoc(string thoiGianHoc)
        {
            if (string.IsNullOrWhiteSpace(thoiGianHoc)) return 24;
            var mBuoi = Regex.Match(thoiGianHoc, @"(\d+)\s*buổi", RegexOptions.IgnoreCase);
            if (mBuoi.Success && int.TryParse(mBuoi.Groups[1].Value, out int buoi) && buoi > 0)
                return buoi;

            var mThang = Regex.Match(thoiGianHoc, @"(\d+(?:[\.,]\d+)?)\s*tháng", RegexOptions.IgnoreCase);
            if (mThang.Success && decimal.TryParse(mThang.Groups[1].Value.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal thang) && thang > 0)
                return Math.Max(1, (int)Math.Round(thang * 12m));

            return 24;
        }

        private bool KiemTraTrungPhongHoacGV(string maLopBoQua, string caHoc, string thuHoc, string phongHoc, string maGV)
        {
            string sql = @"SELECT l.malop, l.tenlop, lh.phonghoc, lh.thu, l.magiangvien
                           FROM LICHHOC lh
                           JOIN LOPHOC l ON lh.malop = l.malop
                           WHERE ISNULL(l.dahuy, 0) = 0
                             AND (@boqua = '' OR l.malop <> @boqua)
                             AND lh.cahoc = @ca";
            DataTable dt = kncsdl.DocDuLieu(sql, new[] {
                new SqlParameter("@boqua", maLopBoQua ?? ""),
                new SqlParameter("@ca", caHoc)
            });

            string[] dsThuMoi = { "2", "3", "4", "5", "6", "7", "CN" };
            foreach (DataRow r in dt.Rows)
            {
                string thuCu = r["thu"].ToString().ToUpper();
                bool trungThu = false;
                foreach (string t in dsThuMoi)
                {
                    if (thuHoc.ToUpper().Contains(t) && thuCu.Contains(t))
                    {
                        trungThu = true;
                        break;
                    }
                }
                if (!trungThu) continue;

                if (string.Equals(r["phonghoc"].ToString().Trim(), phongHoc.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    HienTB($"Trùng phòng học! {phongHoc} đã được xếp cho lớp '{r["tenlop"]}' ({r["malop"]}) vào {caHoc} - {thuHoc}.", true);
                    return true;
                }

                if (!string.IsNullOrEmpty(maGV) && string.Equals(r["magiangvien"]?.ToString().Trim(), maGV.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    HienTB($"Trùng lịch Giảng viên! Giảng viên này đã dạy lớp '{r["tenlop"]}' ({r["malop"]}) vào {caHoc} - {thuHoc}.", true);
                    return true;
                }
            }
            return false;
        }

        private void TaiGoiYTimKiemLop()
        {
            string sql = "SELECT l.malop, l.tenlop, kh.tenkhoahoc FROM LOPHOC l JOIN KHOAHOC kh ON l.makhoahoc = kh.makhoahoc ORDER BY l.malop DESC";
            DataTable dt = kncsdl.DocDuLieu(sql);
            StringBuilder sb = new StringBuilder();
            foreach (DataRow r in dt.Rows)
            {
                sb.AppendFormat("<button type=\"button\" class=\"list-group-item list-group-item-action py-2 px-2 small goi-y-item\" data-value=\"{0}\"><strong>{0}</strong> - {1} ({2})</button>",
                    HttpUtility.HtmlAttributeEncode(Convert.ToString(r["malop"])), HttpUtility.HtmlEncode(Convert.ToString(r["tenlop"])), HttpUtility.HtmlEncode(Convert.ToString(r["tenkhoahoc"])));
            }
            litGoiYLop.Text = sb.ToString();
        }

        private DateTime LayThuHaiDauTuan(DateTime ngay)
        {
            int diff = (7 + (ngay.DayOfWeek - DayOfWeek.Monday)) % 7;
            return ngay.AddDays(-1 * diff).Date;
        }

        protected void btnTuanTruoc_Click(object sender, EventArgs e)
        {
            NgayMocTuanTKB = NgayMocTuanTKB.AddDays(-7);
            txtChonNgayTKB.Text = NgayMocTuanTKB.ToString("yyyy-MM-dd");
            TaoBangLuoiThoiKhoaBieu();
        }

        protected void btnTuanSau_Click(object sender, EventArgs e)
        {
            NgayMocTuanTKB = NgayMocTuanTKB.AddDays(7);
            txtChonNgayTKB.Text = NgayMocTuanTKB.ToString("yyyy-MM-dd");
            TaoBangLuoiThoiKhoaBieu();
        }

        protected void btnTuanHienTai_Click(object sender, EventArgs e)
        {
            NgayMocTuanTKB = DateTime.Today;
            txtChonNgayTKB.Text = DateTime.Today.ToString("yyyy-MM-dd");
            TaoBangLuoiThoiKhoaBieu();
        }

        protected void txtChonNgayTKB_TextChanged(object sender, EventArgs e)
        {
            if (DateTime.TryParse(txtChonNgayTKB.Text, out DateTime d))
                NgayMocTuanTKB = d;
            TaoBangLuoiThoiKhoaBieu();
        }

        private void TaoBangLuoiThoiKhoaBieu()
        {
            DateTime thuHai = LayThuHaiDauTuan(NgayMocTuanTKB);
            DateTime chuNhat = thuHai.AddDays(6);
            litKhoangTuanTKB.Text = $"Từ {thuHai:dd/MM/yyyy} đến {chuNhat:dd/MM/yyyy}";

            string sql = @"SELECT l.malop, l.tenlop, kh.tenkhoahoc, kh.thoigianhoc, l.ngaykhaigiang, ISNULL(gv.hoten, '') AS hotengv,
                                  lh.cahoc, lh.thu, lh.giohoc, lh.phonghoc
                           FROM LICHHOC lh
                           JOIN LOPHOC l ON lh.malop = l.malop
                           JOIN KHOAHOC kh ON l.makhoahoc = kh.makhoahoc
                           LEFT JOIN GIANGVIEN gv ON l.magiangvien = gv.magiangvien
                           WHERE ISNULL(l.dahuy, 0) = 0
                             AND (@isGV = 0 OR LTRIM(RTRIM(l.magiangvien)) = LTRIM(RTRIM(@user)))
                             AND (@isHV = 0 OR l.malop IN (SELECT malop FROM DANGKY WHERE mahocvien = @user))";
            SqlParameter[] p = {
                new SqlParameter("@isGV", LaGiangVien ? 1 : 0),
                new SqlParameter("@isHV", LaHocVien ? 1 : 0),
                new SqlParameter("@user", MaNguoiDung)
            };
            DataTable dt = kncsdl.DocDuLieu(sql, p);

            // Lấy dữ liệu nghỉ đột xuất & dạy bù trong tuần đang xem
            DataTable dtNghiBu = kncsdl.DocDuLieu(@"SELECT nb.*, l.tenlop 
                                                    FROM LICH_NGHI_BU nb 
                                                    JOIN LOPHOC l ON nb.malop = l.malop 
                                                    WHERE (nb.ngaynghi BETWEEN @tu AND @den) OR (nb.ngaybu BETWEEN @tu AND @den)",
                new[] { new SqlParameter("@tu", thuHai), new SqlParameter("@den", chuNhat) });

            string[] dsCa = { "Ca sáng", "Ca chiều", "Ca tối" };
            string[] dsThu = { "2", "3", "4", "5", "6", "7", "CN" };
            string[] tenThu = { "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "Chủ Nhật" };

            StringBuilder sb = new StringBuilder();
            sb.Append("<table class='table table-bordered text-center align-middle mb-0' style='font-size: 0.85rem;'>");
            sb.Append("<thead class='table-light'><tr><th style='width:105px;'>Ca / Thứ</th>");
            for (int i = 0; i < 7; i++)
            {
                DateTime ngayCot = thuHai.AddDays(i);
                string highlightToday = ngayCot == DateTime.Today ? "bg-primary text-white" : "";
                sb.AppendFormat("<th class='{0}'>{1}<br/><small class='fw-normal'>{2:dd/MM}</small></th>", highlightToday, tenThu[i], ngayCot);
            }
            sb.Append("</tr></thead><tbody>");

            foreach (string ca in dsCa)
            {
                sb.Append("<tr>");
                sb.AppendFormat("<td class='fw-bold bg-light text-primary'>{0}</td>", ca);
                for (int i = 0; i < 7; i++)
                {
                    string thu = dsThu[i];
                    DateTime ngayTrongO = thuHai.AddDays(i);
                    string ngayStr = ngayTrongO.ToString("yyyy-MM-dd");
                    List<string> oLich = new List<string>();

                    foreach (DataRow r in dt.Rows)
                    {
                        string caDB = r["cahoc"].ToString().ToLower();
                        string thuDB = r["thu"].ToString().ToUpper();
                        if (!caDB.Contains(ca.ToLower()) || !thuDB.Contains(thu)) continue;

                        string maLop = r["malop"].ToString();
                        DateTime kg = r["ngaykhaigiang"] != DBNull.Value ? Convert.ToDateTime(r["ngaykhaigiang"]).Date : DateTime.MinValue;
                        DateTime ktThucHoc = kg != DateTime.MinValue ? TinhNgayKetThuc(kg, r["thoigianhoc"].ToString(), r["thu"].ToString(), false) : DateTime.MaxValue;
                        DateTime ktDuTru = kg != DateTime.MinValue ? TinhNgayKetThuc(kg, r["thoigianhoc"].ToString(), r["thu"].ToString(), true) : DateTime.MaxValue;

                        // Chỉ hiển thị lịch chính trong giai đoạn thực học (từ khai giảng đến hết tuần học chính)
                        if (ngayTrongO < kg || ngayTrongO > ktThucHoc) continue;

                        // Kiểm tra xem ngày này có bị báo nghỉ đột xuất không
                        DataRow[] rowsNghi = dtNghiBu.Select($"malop = '{maLop}' AND ngaynghi = #{ngayTrongO:MM/dd/yyyy}#");
                        bool biNghi = rowsNghi.Length > 0;

                        string tuanDuTruText = $"{ktThucHoc.AddDays(1):dd/MM/yyyy} - {ktDuTru:dd/MM/yyyy}";
                        string ngayBuGoiY = ktThucHoc.AddDays(1).ToString("yyyy-MM-dd");

                        Func<object, string> js = x => HttpUtility.HtmlAttributeEncode(HttpUtility.JavaScriptStringEncode(Convert.ToString(x)));
                        Func<object, string> ht = x => HttpUtility.HtmlEncode(Convert.ToString(x));

                        string onclickAttr = LaQuanTriHoacNV
                            ? $"style='cursor:pointer;' title='Bấm để báo nghỉ đột xuất hoặc đổi lịch ({ngayTrongO:dd/MM/yyyy})' onclick=\"moModalDoiLich('{js(maLop)}','{js(r["tenlop"])}','{js(r["cahoc"])}','{js(r["thu"])}','{js(r["giohoc"])}','{js(r["phonghoc"])}','{ngayStr}','{js(tuanDuTruText)}','{ngayBuGoiY}');\""
                            : "";

                        if (biNghi)
                        {
                            string lyDo = ht(rowsNghi[0]["lydo"]);
                            string ngayBuStr = rowsNghi[0]["ngaybu"] != DBNull.Value ? Convert.ToDateTime(rowsNghi[0]["ngaybu"]).ToString("dd/MM") : "Chưa xếp";
                            oLich.Add($"<div {onclickAttr} class='tkb-cell-item p-1 mb-1 rounded border border-danger bg-danger-subtle text-danger'><span class='badge bg-danger mb-1'>NGHỈ ĐỘT XUẤT</span><br/><strong class='text-decoration-line-through'>{ht(r["tenlop"])}</strong><br/><span class='small'>Lý do: {lyDo}<br/>Bù ngày: <strong>{ngayBuStr}</strong></span></div>");
                        }
                        else
                        {
                            oLich.Add($"<div {onclickAttr} class='tkb-cell-item p-1 mb-1 rounded border bg-primary-subtle text-dark'><strong>{ht(r["tenlop"])}</strong><br/><span class='small text-muted'>{ht(r["giohoc"])} | P.{ht(r["phonghoc"])}</span></div>");
                        }
                    }

                    // Hiển thị các buổi DẠY BÙ rơi vào ngày/ca này (thường nằm trong Tuần dự trữ)
                    foreach (DataRow rBu in dtNghiBu.Rows)
                    {
                        if (rBu["ngaybu"] == DBNull.Value) continue;
                        DateTime dBu = Convert.ToDateTime(rBu["ngaybu"]).Date;
                        string caBu = rBu["cahoc_bu"]?.ToString() ?? "";
                        if (dBu == ngayTrongO && caBu.Equals(ca, StringComparison.OrdinalIgnoreCase))
                        {
                            string maLopBu = rBu["malop"].ToString();
                            string ngayGocStr = Convert.ToDateTime(rBu["ngaynghi"]).ToString("yyyy-MM-dd");
                            string ngayGocHienThi = Convert.ToDateTime(rBu["ngaynghi"]).ToString("dd/MM");
                            Func<object, string> js = x => HttpUtility.HtmlAttributeEncode(HttpUtility.JavaScriptStringEncode(Convert.ToString(x)));
                            Func<object, string> ht = x => HttpUtility.HtmlEncode(Convert.ToString(x));

                            string onclickBu = LaQuanTriHoacNV
                                ? $"style='cursor:pointer;' title='Bấm để chỉnh sửa lịch dạy bù' onclick=\"moModalDoiLich('{js(maLopBu)}','{js(rBu["tenlop"])}','{js(caBu)}','Thứ 2, 4, 6','{js(rBu["giohoc_bu"])}','{js(rBu["phonghoc_bu"])}','{ngayGocStr}','Tuần dự trữ','{ngayStr}');\""
                                : "";
                            oLich.Add($"<div {onclickBu} class='tkb-cell-item p-1 mb-1 rounded border border-success bg-success-subtle text-dark'><span class='badge bg-success mb-1'>DẠY BÙ (TUẦN DỰ TRỮ)</span><br/><strong>{ht(rBu["tenlop"])}</strong><br/><span class='small text-success fw-semibold'>{ht(rBu["giohoc_bu"])} | P.{ht(rBu["phonghoc_bu"])}<br/>(Bù cho ngày {ngayGocHienThi})</span></div>");
                        }
                    }

                    if (oLich.Count > 1)
                    {
                        sb.Append("<td class='bg-warning-subtle'>");
                        sb.Append("<span class='badge bg-danger mb-1'>LƯU Ý TRÙNG CA!</span>");
                        sb.Append(string.Join("", oLich));
                        sb.Append("</td>");
                    }
                    else if (oLich.Count == 1)
                    {
                        sb.AppendFormat("<td>{0}</td>", oLich[0]);
                    }
                    else
                    {
                        sb.Append("<td class='text-muted'>-</td>");
                    }
                }
                sb.Append("</tr>");
            }
            sb.Append("</tbody></table>");
            litBangTKB.Text = sb.ToString();
        }

        protected void btnLuuNghiBu_Click(object sender, EventArgs e)
        {
            if (!LaQuanTriHoacNV)
            {
                HienTB("Bạn không có quyền điều chỉnh lịch nghỉ/bù!", true);
                return;
            }

            string maLop = hfMaLopDoiLich.Value;
            if (string.IsNullOrEmpty(maLop) || string.IsNullOrEmpty(txtNgayNghiDotXuat.Text))
            {
                HienTB("Vui lòng chọn ngày giảng viên nghỉ đột xuất!", true);
                return;
            }

            DateTime ngayNghi = Convert.ToDateTime(txtNgayNghiDotXuat.Text).Date;
            object ngayBuParam = DBNull.Value;
            if (!string.IsNullOrEmpty(txtNgayDayBu.Text))
            {
                DateTime ngayBu = Convert.ToDateTime(txtNgayDayBu.Text).Date;
                if (ngayBu <= ngayNghi)
                {
                    HienTB("Ngày dạy bù phải sau ngày nghỉ đột xuất!", true);
                    return;
                }
                ngayBuParam = ngayBu;
            }

            string lyDo = string.IsNullOrWhiteSpace(txtLyDoNghi.Text) ? "Giảng viên bận đột xuất" : txtLyDoNghi.Text.Trim();

            kncsdl.ThucThiLenh("DELETE FROM LICH_NGHI_BU WHERE malop=@l AND ngaynghi=@nn",
                new[] { new SqlParameter("@l", maLop), new SqlParameter("@nn", ngayNghi) });

            string sqlInsert = @"INSERT INTO LICH_NGHI_BU(malop, ngaynghi, ngaybu, cahoc_bu, giohoc_bu, phonghoc_bu, lydo, nguoitao)
                                 VALUES(@l, @nn, @nb, @ca, @gio, @ph, @ld, @user)";
            kncsdl.ThucThiLenh(sqlInsert, new[] {
                new SqlParameter("@l", maLop),
                new SqlParameter("@nn", ngayNghi),
                new SqlParameter("@nb", ngayBuParam),
                new SqlParameter("@ca", ddlCaHocBu.SelectedValue),
                new SqlParameter("@gio", txtGioHocBu.Text.Trim()),
                new SqlParameter("@ph", ddlPhongHocBu.SelectedValue),
                new SqlParameter("@ld", lyDo),
                new SqlParameter("@user", MaNguoiDung)
            });

            kncsdl.GhiNhatKy(MaNguoiDung, "Báo nghỉ đột xuất & Dạy bù", $"Lớp {maLop} nghỉ ngày {ngayNghi:dd/MM/yyyy}, bù ngày {txtNgayDayBu.Text}");
            HienTB($"Đã cập nhật báo nghỉ ngày {ngayNghi:dd/MM/yyyy} và xếp lịch bù vào tuần dự trữ thành công!");
            TaoBangLuoiThoiKhoaBieu();
        }

        protected void btnHuyNghiBuNgayNay_Click(object sender, EventArgs e)
        {
            if (!LaQuanTriHoacNV) return;
            string maLop = hfMaLopDoiLich.Value;
            if (string.IsNullOrEmpty(maLop) || string.IsNullOrEmpty(txtNgayNghiDotXuat.Text)) return;

            DateTime ngayNghi = Convert.ToDateTime(txtNgayNghiDotXuat.Text).Date;
            kncsdl.ThucThiLenh("DELETE FROM LICH_NGHI_BU WHERE malop=@l AND ngaynghi=@nn",
                new[] { new SqlParameter("@l", maLop), new SqlParameter("@nn", ngayNghi) });

            kncsdl.GhiNhatKy(MaNguoiDung, "Khôi phục buổi học", $"Hủy báo nghỉ lớp {maLop} ngày {ngayNghi:dd/MM/yyyy}");
            HienTB($"Đã khôi phục lịch học bình thường cho ngày {ngayNghi:dd/MM/yyyy}!");
            TaoBangLuoiThoiKhoaBieu();
        }

        private void CapNhatLichHocChoLop(string maLop)
        {
            kncsdl.ThucThiLenh("DELETE FROM LICHHOC WHERE malop=@l", new[] { new SqlParameter("@l", maLop) });
            int count = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM LICHHOC")) + 1;
            string maLH = "LH" + count.ToString("D2");
            while (Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM LICHHOC WHERE malichhoc=@m", new[] { new SqlParameter("@m", maLH) })) > 0)
                maLH = "LH" + (++count).ToString("D2");

            string sqlLH = "INSERT INTO LICHHOC(malichhoc, cahoc, phonghoc, thu, giohoc, malop) VALUES(@ma, @ca, @ph, @thu, @gio, @lop)";
            kncsdl.ThucThiLenh(sqlLH, new[] {
                new SqlParameter("@ma", maLH),
                new SqlParameter("@ca", ddlCaHoc.SelectedValue),
                new SqlParameter("@ph", ddlPhongHoc.SelectedValue),
                new SqlParameter("@thu", ddlThuHoc.SelectedValue),
                new SqlParameter("@gio", txtGioHoc.Text.Trim()),
                new SqlParameter("@lop", maLop)
            });
        }

        private void LamMoiForm()
        {
            SinhMaLopMoi();
            txtTenLop.Text = "";
            if (ddlKhoaHoc.Items.Count > 0) ddlKhoaHoc.SelectedIndex = 0;
            if (ddlGiangVien.Items.Count > 0) ddlGiangVien.SelectedIndex = 0;
            txtKhaiGiang.Text = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
            txtGioHoc.Text = "18:00 - 20:30";
            if (ddlPhongHoc.Items.Count > 0) ddlPhongHoc.SelectedIndex = 0;
        }

        protected void btnThemLop_Click(object sender, EventArgs e)
        {
            if (!LaQuanTriHoacNV)
            {
                HienTB("Bạn không có quyền mở lớp học!", true);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTenLop.Text))
            {
                HienTB("Vui lòng nhập Tên lớp!", true);
                txtTenLop.Focus();
                return;
            }

            if (string.IsNullOrEmpty(ddlGiangVien.SelectedValue))
            {
                HienTB("Vui lòng chọn Giảng viên phụ trách cho lớp học!", true);
                ddlGiangVien.Focus();
                return;
            }

            if (string.IsNullOrEmpty(ddlKhoaHoc.SelectedValue))
            {
                HienTB("Vui lòng chọn Khóa học đang mở!", true);
                return;
            }

            if (KiemTraTrungPhongHoacGV("", ddlCaHoc.SelectedValue, ddlThuHoc.SelectedValue, ddlPhongHoc.SelectedValue, ddlGiangVien.SelectedValue))
                return;

            SinhMaLopMoi();
            string sql = "INSERT INTO LOPHOC(malop, tenlop, makhoahoc, magiangvien, ngaykhaigiang) VALUES(@ma, @ten, @kh, @gv, @ngay)";
            kncsdl.ThucThiLenh(sql, new[] {
                new SqlParameter("@ma", txtMaLop.Text.Trim()),
                new SqlParameter("@ten", txtTenLop.Text.Trim()),
                new SqlParameter("@kh", ddlKhoaHoc.SelectedValue),
                new SqlParameter("@gv", ddlGiangVien.SelectedValue),
                new SqlParameter("@ngay", string.IsNullOrEmpty(txtKhaiGiang.Text) ? (object)DBNull.Value : txtKhaiGiang.Text)
            });

            CapNhatLichHocChoLop(txtMaLop.Text.Trim());
            kncsdl.GhiNhatKy(MaNguoiDung, "Mở lớp học", $"Mở lớp {txtMaLop.Text.Trim()} - {txtTenLop.Text.Trim()}");
            HienTB("Mở lớp học phần và tạo Thời khóa biểu (kèm 1 tuần dự trữ) thành công!");
            LamMoiForm();
            gvLopHoc.PageIndex = 0;
            TaiDanhSachLop();
        }

        protected void btnSuaLop_Click(object sender, EventArgs e)
        {
            if (!LaQuanTriHoacNV)
            {
                HienTB("Bạn không có quyền sửa lớp học!", true);
                return;
            }

            string maLop = txtMaLop.Text.Trim();
            if (string.IsNullOrWhiteSpace(maLop) || string.IsNullOrWhiteSpace(txtTenLop.Text))
            {
                HienTB("Vui lòng chọn lớp cần sửa!", true);
                return;
            }

            if (string.IsNullOrEmpty(ddlGiangVien.SelectedValue))
            {
                HienTB("Vui lòng chọn Giảng viên phụ trách cho lớp học!", true);
                ddlGiangVien.Focus();
                return;
            }

            DataTable dtCu = kncsdl.DocDuLieu("SELECT makhoahoc FROM LOPHOC WHERE malop=@m", new[] { new SqlParameter("@m", maLop) });
            if (dtCu.Rows.Count == 0)
            {
                HienTB("Lớp này chưa tồn tại trong hệ thống!", true);
                return;
            }

            string khCu = dtCu.Rows[0]["makhoahoc"].ToString();
            if (khCu != ddlKhoaHoc.SelectedValue)
            {
                int daCoDK = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM DANGKY WHERE malop=@m", new[] { new SqlParameter("@m", maLop) }));
                if (daCoDK > 0)
                {
                    HienTB("Không thể đổi khóa học vì lớp này đã có học viên đăng ký!", true);
                    ddlKhoaHoc.SelectedValue = khCu;
                    return;
                }
            }

            if (KiemTraTrungPhongHoacGV(maLop, ddlCaHoc.SelectedValue, ddlThuHoc.SelectedValue, ddlPhongHoc.SelectedValue, ddlGiangVien.SelectedValue))
                return;

            string sql = "UPDATE LOPHOC SET tenlop=@ten, makhoahoc=@kh, magiangvien=@gv, ngaykhaigiang=@ngay WHERE malop=@ma";
            kncsdl.ThucThiLenh(sql, new[] {
                new SqlParameter("@ma", maLop),
                new SqlParameter("@ten", txtTenLop.Text.Trim()),
                new SqlParameter("@kh", ddlKhoaHoc.SelectedValue),
                new SqlParameter("@gv", string.IsNullOrEmpty(ddlGiangVien.SelectedValue) ? (object)DBNull.Value : ddlGiangVien.SelectedValue),
                new SqlParameter("@ngay", string.IsNullOrEmpty(txtKhaiGiang.Text) ? (object)DBNull.Value : txtKhaiGiang.Text)
            });
            CapNhatLichHocChoLop(maLop);
            kncsdl.GhiNhatKy(MaNguoiDung, "Cập nhật lớp học", $"Cập nhật lớp {maLop} - {txtTenLop.Text.Trim()}");
            HienTB("Đã cập nhật thông tin lớp & đổi Thời khóa biểu thành công!");
            LamMoiForm();
            TaiDanhSachLop();
        }

        protected void btnLamMoi_Click(object sender, EventArgs e)
        {
            LamMoiForm();
            pnlTB.Visible = false;
        }

        protected void btnTimLop_Click(object sender, EventArgs e)
        {
            TuKhoaHienTai = txtTimLop.Text.Trim();
            gvLopHoc.PageIndex = 0;
            TaiDanhSachLop();
            pnlTB.Visible = false;
        }

        protected void gvLopHoc_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvLopHoc.PageIndex = e.NewPageIndex;
            TaiDanhSachLop();
            pnlTB.Visible = false;
        }

        protected void gvLopHoc_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ChonLop")
            {
                if (!LaQuanTriHoacNV) return;
                string ma = e.CommandArgument.ToString();
                DataTable dt = kncsdl.DocDuLieu("SELECT * FROM LOPHOC WHERE malop=@m", new[] { new SqlParameter("@m", ma) });
                if (dt.Rows.Count > 0)
                {
                    DataRow r = dt.Rows[0];
                    TaiDanhMuc(r["makhoahoc"].ToString());
                    txtMaLop.Text = r["malop"].ToString();
                    txtMaLop.ReadOnly = true;
                    txtTenLop.Text = r["tenlop"].ToString();
                    if (ddlKhoaHoc.Items.FindByValue(r["makhoahoc"].ToString()) != null)
                        ddlKhoaHoc.SelectedValue = r["makhoahoc"].ToString();
                    ddlGiangVien.SelectedValue = r["magiangvien"] != DBNull.Value ? r["magiangvien"].ToString() : "";
                    if (r["ngaykhaigiang"] != DBNull.Value)
                        txtKhaiGiang.Text = Convert.ToDateTime(r["ngaykhaigiang"]).ToString("yyyy-MM-dd");

                    DataTable dtLH = kncsdl.DocDuLieu("SELECT TOP 1 * FROM LICHHOC WHERE malop=@m", new[] { new SqlParameter("@m", ma) });
                    if (dtLH.Rows.Count > 0)
                    {
                        ddlCaHoc.SelectedValue = dtLH.Rows[0]["cahoc"].ToString();

                        string thu = dtLH.Rows[0]["thu"].ToString();
                        if (ddlThuHoc.Items.FindByValue(thu) == null)
                            ddlThuHoc.Items.Add(new ListItem(thu, thu));
                        ddlThuHoc.SelectedValue = thu;

                        txtGioHoc.Text = dtLH.Rows[0]["giohoc"].ToString();

                        string ph = dtLH.Rows[0]["phonghoc"].ToString();
                        if (ddlPhongHoc.Items.FindByValue(ph) == null)
                            ddlPhongHoc.Items.Add(new ListItem(ph, ph));
                        ddlPhongHoc.SelectedValue = ph;
                    }
                    pnlTB.Visible = false;
                }
            }
            else if (e.CommandName == "DiemDanh")
            {
                if (LaHocVien) return;
                string[] parts = e.CommandArgument.ToString().Split('|');
                string maLop = parts[0];
                if (!KiemTraQuyenTrenLop(maLop))
                {
                    HienTB("Bạn không có quyền điểm danh lớp học này!", true);
                    return;
                }

                hfMaLopDiemDanh.Value = maLop;
                litTenLopDiemDanh.Text = HttpUtility.HtmlEncode(parts.Length > 1 ? parts[1] : maLop);
                txtNgayDiemDanh.Text = DateTime.Now.ToString("yyyy-MM-dd");
                LoadDanhSachDiemDanh(maLop, txtNgayDiemDanh.Text);
            }
        }

        private bool KiemTraQuyenTrenLop(string maLop)
        {
            if (LaQuanTriHoacNV) return true;
            if (!LaGiangVien) return false;

            int c = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM LOPHOC WHERE malop=@l AND LTRIM(RTRIM(magiangvien))=LTRIM(RTRIM(@gv))",
                new[] { new SqlParameter("@l", maLop), new SqlParameter("@gv", MaNguoiDung) }));
            return c > 0;
        }

        protected void btnXoaChinhThuc_Click(object sender, EventArgs e)
        {
            if (!LaQuanTriHoacNV)
            {
                HienTB("Bạn không có quyền xóa lớp học!", true);
                return;
            }

            string ma = hfMaLopXoa.Value;
            if (!string.IsNullOrEmpty(ma))
            {
                int hv = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM DANGKY WHERE malop=@m", new[] { new SqlParameter("@m", ma) }));
                if (hv > 0)
                {
                    HienTB("Không thể xóa vì lớp này đã có học viên đăng ký!", true);
                    return;
                }

                kncsdl.ThucThiLenh("DELETE FROM LICH_NGHI_BU WHERE malop=@m", new[] { new SqlParameter("@m", ma) });
                kncsdl.ThucThiLenh("DELETE FROM LICHHOC WHERE malop=@m", new[] { new SqlParameter("@m", ma) });
                kncsdl.ThucThiLenh("DELETE FROM LOPHOC WHERE malop=@m", new[] { new SqlParameter("@m", ma) });
                kncsdl.GhiNhatKy(MaNguoiDung, "Xóa lớp học", $"Xóa lớp {ma}");
                HienTB("Xóa lớp thành công!");

                if (gvLopHoc.Rows.Count == 1 && gvLopHoc.PageIndex > 0)
                    gvLopHoc.PageIndex--;
                LamMoiForm();
                TaiDanhSachLop();
            }
        }

        private void LoadDanhSachDiemDanh(string maLop, string ngayDD)
        {
            if (!KiemTraQuyenTrenLop(maLop))
            {
                HienTB("Bạn không có quyền truy cập điểm danh lớp này!", true);
                return;
            }

            object tgObj = kncsdl.LayGiaTriDon(@"SELECT kh.thoigianhoc FROM LOPHOC l JOIN KHOAHOC kh ON l.makhoahoc = kh.makhoahoc WHERE l.malop=@malop",
                new[] { new SqlParameter("@malop", maLop) });
            int tongBuoi = TinhTongBuoiHoc(tgObj?.ToString() ?? "");

            string sql = @"SELECT hv.mahocvien, hv.hoten, 
                                  CASE WHEN dd.trangthai IN (N'Có mặt', N'Vắng phép', N'Vắng không phép') THEN dd.trangthai ELSE N'Có mặt' END AS trangthai, 
                                  ISNULL(dd.ghichu, '') AS ghichu,
                                  kh.hocphi,
                                  (kh.hocphi - ISNULL((SELECT SUM(pt.sotienthu) FROM PHIEUTHU pt WHERE pt.madangky = dk.madangky AND ISNULL(pt.noidungthu, N'') NOT LIKE N'Chờ xác nhận%'), 0)) AS ConNo,
                                  (SELECT COUNT(*) FROM DIEMDANH d2 
                                   WHERE d2.malop = dk.malop AND d2.mahocvien = dk.mahocvien 
                                     AND d2.trangthai IN (N'Vắng phép', N'Vắng không phép')) AS SoBuoiNghi
                           FROM DANGKY dk 
                           JOIN HOCVIEN hv ON dk.mahocvien = hv.mahocvien 
                           JOIN KHOAHOC kh ON dk.makhoahoc = kh.makhoahoc
                           LEFT JOIN DIEMDANH dd ON dk.mahocvien = dd.mahocvien AND dd.malop = dk.malop AND dd.ngaydiemdanh = @ngay
                           WHERE dk.malop = @malop 
                           ORDER BY hv.hoten";

            DataTable dt = kncsdl.DocDuLieu(sql, new[] {
                new SqlParameter("@malop", maLop),
                new SqlParameter("@ngay", ngayDD)
            });

            dt.Columns.Add("CanhBao", typeof(string));
            dt.Columns.Add("CanhBaoHocPhi", typeof(string));

            foreach (DataRow r in dt.Rows)
            {
                int nghi = Convert.ToInt32(r["SoBuoiNghi"]);
                if (nghi >= 3)
                    r["CanhBao"] = $" <br/><span class='badge bg-danger ms-1 mt-1'>Nghỉ {nghi}/{tongBuoi} buổi - Cấm thi</span>";
                else if (nghi == 2)
                    r["CanhBao"] = $" <br/><span class='badge bg-warning text-dark ms-1 mt-1'>Cảnh báo: Nghỉ 2/{tongBuoi} buổi</span>";
                else
                    r["CanhBao"] = "";

                decimal no = Convert.ToDecimal(r["ConNo"]);
                if (no > 0)
                    r["CanhBaoHocPhi"] = $" <br/><span class='badge border border-danger text-danger ms-1 mt-1'>Nợ học phí: {no:N0} đ</span>";
                else
                    r["CanhBaoHocPhi"] = "";
            }

            gvHocVienLop.DataSource = dt;
            gvHocVienLop.DataBind();

            if (gvHocVienLop.Rows.Count == 0)
            {
                HienTB("Lớp này hiện chưa có học viên nào đăng ký!", true);
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showDD_" + Guid.NewGuid().ToString("N"), "showModalDiemDanh();", true);
            }
        }

        protected void txtNgayDiemDanh_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(hfMaLopDiemDanh.Value) && !string.IsNullOrEmpty(txtNgayDiemDanh.Text))
            {
                LoadDanhSachDiemDanh(hfMaLopDiemDanh.Value, txtNgayDiemDanh.Text);
            }
        }

        protected void btnLuuDiemDanh_Click(object sender, EventArgs e)
        {
            if (LaHocVien)
            {
                HienTB("Học viên không có quyền điểm danh!", true);
                return;
            }

            string ngayDD = txtNgayDiemDanh.Text;
            string maLop = hfMaLopDiemDanh.Value;
            if (string.IsNullOrEmpty(ngayDD) || string.IsNullOrEmpty(maLop)) return;

            if (!KiemTraQuyenTrenLop(maLop))
            {
                HienTB("Bạn chỉ được phép điểm danh lớp do mình phụ trách!", true);
                return;
            }

            using (SqlConnection conn = new SqlConnection(kncsdl.ChuoiKetNoi))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                try
                {
                    SqlCommand cmdXoa = new SqlCommand("DELETE FROM DIEMDANH WHERE malop=@l AND ngaydiemdanh=@n", conn, tran);
                    cmdXoa.Parameters.AddWithValue("@l", maLop);
                    cmdXoa.Parameters.AddWithValue("@n", ngayDD);
                    cmdXoa.ExecuteNonQuery();

                    foreach (GridViewRow row in gvHocVienLop.Rows)
                    {
                        string maHV = gvHocVienLop.DataKeys[row.RowIndex].Value.ToString();
                        RadioButtonList rbl = (RadioButtonList)row.FindControl("rblTrangThai");
                        TextBox txtGC = (TextBox)row.FindControl("txtGhiChuDD");

                        SqlCommand cmdLuu = new SqlCommand("INSERT INTO DIEMDANH(malop, mahocvien, ngaydiemdanh, trangthai, ghichu) VALUES(@l, @hv, @ngay, @tt, @gc)", conn, tran);
                        cmdLuu.Parameters.AddWithValue("@l", maLop);
                        cmdLuu.Parameters.AddWithValue("@hv", maHV);
                        cmdLuu.Parameters.AddWithValue("@ngay", ngayDD);
                        cmdLuu.Parameters.AddWithValue("@tt", rbl.SelectedValue);
                        cmdLuu.Parameters.AddWithValue("@gc", txtGC.Text.Trim());
                        cmdLuu.ExecuteNonQuery();
                    }

                    tran.Commit();
                    kncsdl.GhiNhatKy(MaNguoiDung, "Điểm danh lớp học", $"Điểm danh lớp {maLop} ngày {ngayDD}");
                    HienTB($"Đã lưu kết quả điểm danh ngày {Convert.ToDateTime(ngayDD).ToString("dd/MM/yyyy")} thành công!");
                }
                catch
                {
                    tran.Rollback();
                    HienTB("Có lỗi trong quá trình điểm danh!", true);
                }
            }
        }

        protected void btnXuatDSHocVienDuThi_Click(object sender, EventArgs e)
        {
            string maLop = hfMaLopDiemDanh.Value;
            if (string.IsNullOrEmpty(maLop)) return;

            if (!KiemTraQuyenTrenLop(maLop))
            {
                HienTB("Bạn không có quyền xuất danh sách lớp này!", true);
                return;
            }

            object tgObj = kncsdl.LayGiaTriDon(@"SELECT kh.thoigianhoc FROM LOPHOC l JOIN KHOAHOC kh ON l.makhoahoc = kh.makhoahoc WHERE l.malop=@malop",
                new[] { new SqlParameter("@malop", maLop) });
            int tongBuoi = TinhTongBuoiHoc(tgObj?.ToString() ?? "");

            string sql = @"SELECT hv.mahocvien, hv.hoten, hv.ngaysinh, hv.dienthoai, 
                                  (SELECT COUNT(*) FROM DIEMDANH d2 WHERE d2.malop = dk.malop AND d2.mahocvien = dk.mahocvien AND d2.trangthai IN (N'Vắng phép', N'Vắng không phép')) AS SoBuoiNghi,
                                  (kh.hocphi - ISNULL((SELECT SUM(pt.sotienthu) FROM PHIEUTHU pt WHERE pt.madangky = dk.madangky AND ISNULL(pt.noidungthu, N'') NOT LIKE N'Chờ xác nhận%'), 0)) AS ConNo
                           FROM DANGKY dk 
                           JOIN HOCVIEN hv ON dk.mahocvien = hv.mahocvien 
                           JOIN KHOAHOC kh ON dk.makhoahoc = kh.makhoahoc
                           WHERE dk.malop = @malop
                           ORDER BY hv.hoten";

            DataTable dt = kncsdl.DocDuLieu(sql, new[] { new SqlParameter("@malop", maLop) });

            DataTable dtDuThi = new DataTable();
            dtDuThi.Columns.Add("STT", typeof(int));
            dtDuThi.Columns.Add("Mã HV", typeof(string));
            dtDuThi.Columns.Add("Họ và tên", typeof(string));
            dtDuThi.Columns.Add("Ngày sinh", typeof(string));
            dtDuThi.Columns.Add("Số điện thoại", typeof(string));
            dtDuThi.Columns.Add("Số buổi nghỉ", typeof(string));
            dtDuThi.Columns.Add("Ghi chú / Ký tên", typeof(string));

            DataTable dtCamThi = new DataTable();
            dtCamThi.Columns.Add("STT", typeof(int));
            dtCamThi.Columns.Add("Mã HV", typeof(string));
            dtCamThi.Columns.Add("Họ và tên", typeof(string));
            dtCamThi.Columns.Add("Ngày sinh", typeof(string));
            dtCamThi.Columns.Add("Số điện thoại", typeof(string));
            dtCamThi.Columns.Add("Số buổi nghỉ", typeof(string));
            dtCamThi.Columns.Add("Lý do cấm thi", typeof(string));

            int sttDuThi = 1;
            int sttCamThi = 1;

            foreach (DataRow r in dt.Rows)
            {
                string maHV = r["mahocvien"].ToString();
                string hoTen = r["hoten"].ToString();
                string ngaySinh = r["ngaysinh"] != DBNull.Value ? Convert.ToDateTime(r["ngaysinh"]).ToString("dd/MM/yyyy") : "";
                string sdt = r["dienthoai"].ToString();
                int nghi = Convert.ToInt32(r["SoBuoiNghi"]);
                string chuoiBuoiNghi = $"{nghi}/{tongBuoi}";
                decimal no = Convert.ToDecimal(r["ConNo"]);

                List<string> lyDo = new List<string>();
                if (nghi >= 3)
                    lyDo.Add($"Nghỉ quá quy định ({nghi}/{tongBuoi} buổi)");
                if (no > 0)
                    lyDo.Add($"Còn nợ học phí ({no:N0} đ)");

                if (lyDo.Count == 0)
                {
                    dtDuThi.Rows.Add(sttDuThi++, maHV, hoTen, ngaySinh, sdt, chuoiBuoiNghi, "");
                }
                else
                {
                    dtCamThi.Rows.Add(sttCamThi++, maHV, hoTen, ngaySinh, sdt, chuoiBuoiNghi, string.Join("; ", lyDo));
                }
            }

            string tenLopHienThi = litTenLopDiemDanh.Text;
            string tenLopFile = Regex.Replace(tenLopHienThi, @"[^\w\-]+", "_");
            string fileName = $"DS_DuThi_CamThi_{tenLopFile}_{DateTime.Now:yyyyMMdd}";

            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment;filename=" + fileName + ".xls");
            Response.Charset = "";
            Response.ContentType = "application/vnd.ms-excel";
            Response.ContentEncoding = Encoding.UTF8;
            Response.BinaryWrite(Encoding.UTF8.GetPreamble());

            StringBuilder sb = new StringBuilder();
            sb.Append("<html><head><meta charset='utf-8'></head><body>");
            sb.AppendFormat("<h2 style='color:#0755A6;font-family:Arial;margin-bottom:4px;'>DANH SÁCH XÉT ĐIỀU KIỆN DỰ THI - {0}</h2>", HttpUtility.HtmlEncode(tenLopHienThi.ToUpper()));
            sb.AppendFormat("<p style='font-family:Arial;font-size:12px;color:#555;margin-top:0;'>Ngày xuất: {0:dd/MM/yyyy HH:mm} | Tổng sĩ số: {1} HV (Đủ điều kiện: {2} HV - Cấm thi: {3} HV)</p>",
                DateTime.Now, dt.Rows.Count, dtDuThi.Rows.Count, dtCamThi.Rows.Count);

            sb.AppendFormat("<h3 style='color:#198754;font-family:Arial;margin-top:16px;margin-bottom:6px;'>I. DANH SÁCH HỌC VIÊN ĐỦ ĐIỀU KIỆN DỰ THI ({0} HV)</h3>", dtDuThi.Rows.Count);
            sb.Append("<table border='1' style='border-collapse:collapse;font-family:Arial;font-size:13px;width:100%;'>");
            sb.Append("<tr style='background-color:#198754;color:white;font-weight:bold;text-align:center;'>");
            foreach (DataColumn c in dtDuThi.Columns)
                sb.AppendFormat("<th style='padding:7px 10px;'>{0}</th>", HttpUtility.HtmlEncode(c.ColumnName));
            sb.Append("</tr>");

            if (dtDuThi.Rows.Count == 0)
            {
                sb.Append("<tr><td colspan='7' style='padding:8px;text-align:center;font-style:italic;color:#666;'>Không có học viên nào đủ điều kiện dự thi.</td></tr>");
            }
            else
            {
                foreach (DataRow r in dtDuThi.Rows)
                {
                    sb.Append("<tr>");
                    foreach (DataColumn c in dtDuThi.Columns)
                    {
                        string val = r[c].ToString();
                        string align = (c.ColumnName == "STT" || c.ColumnName == "Mã HV" || c.ColumnName == "Ngày sinh" || c.ColumnName == "Số buổi nghỉ") ? "text-align:center;" : "";
                        sb.AppendFormat("<td style='padding:6px 10px;{0}mso-number-format:\"\\@\";'>{1}</td>", align, HttpUtility.HtmlEncode(val));
                    }
                    sb.Append("</tr>");
                }
            }
            sb.Append("</table>");

            sb.AppendFormat("<h3 style='color:#DC3545;font-family:Arial;margin-top:24px;margin-bottom:6px;'>II. DANH SÁCH HỌC VIÊN BỊ CẤM THI ({0} HV)</h3>", dtCamThi.Rows.Count);
            sb.Append("<table border='1' style='border-collapse:collapse;font-family:Arial;font-size:13px;width:100%;'>");
            sb.Append("<tr style='background-color:#DC3545;color:white;font-weight:bold;text-align:center;'>");
            foreach (DataColumn c in dtCamThi.Columns)
                sb.AppendFormat("<th style='padding:7px 10px;'>{0}</th>", HttpUtility.HtmlEncode(c.ColumnName));
            sb.Append("</tr>");

            if (dtCamThi.Rows.Count == 0)
            {
                sb.Append("<tr><td colspan='7' style='padding:8px;text-align:center;font-style:italic;color:#666;'>Không có học viên nào bị cấm thi.</td></tr>");
            }
            else
            {
                foreach (DataRow r in dtCamThi.Rows)
                {
                    sb.Append("<tr style='background-color:#fff5f5;'>");
                    foreach (DataColumn c in dtCamThi.Columns)
                    {
                        string val = r[c].ToString();
                        if (c.ColumnName == "Lý do cấm thi")
                        {
                            sb.AppendFormat("<td style='padding:6px 10px;color:#DC3545;font-weight:bold;'>{0}</td>", HttpUtility.HtmlEncode(val));
                        }
                        else
                        {
                            string align = (c.ColumnName == "STT" || c.ColumnName == "Mã HV" || c.ColumnName == "Ngày sinh" || c.ColumnName == "Số buổi nghỉ") ? "text-align:center;" : "";
                            sb.AppendFormat("<td style='padding:6px 10px;{0}mso-number-format:\"\\@\";'>{1}</td>", align, HttpUtility.HtmlEncode(val));
                        }
                    }
                    sb.Append("</tr>");
                }
            }
            sb.Append("</table></body></html>");

            Response.Output.Write(sb.ToString());
            Response.Flush();
            Response.SuppressContent = true;
            Context.ApplicationInstance.CompleteRequest();
        }

        private void HienTB(string msg, bool laLoi = false)
        {
            pnlTB.Visible = true;
            pnlTB.CssClass = laLoi ? "alert alert-danger text-danger fw-semibold alert-dismissible fade show shadow" : "alert alert-success text-success fw-semibold alert-dismissible fade show shadow";
            litTB.Text = HttpUtility.HtmlEncode(msg);
        }

        protected void btnLuuDoiLichTKB_Click(object sender, EventArgs e)
        {
            if (!LaQuanTriHoacNV)
            {
                HienTB("Bạn không có quyền đổi Thời khóa biểu!", true);
                return;
            }

            string maLop = hfMaLopDoiLich.Value;
            if (string.IsNullOrEmpty(maLop)) return;

            string maGV = kncsdl.LayGiaTriDon("SELECT magiangvien FROM LOPHOC WHERE malop=@l", new[] { new SqlParameter("@l", maLop) })?.ToString() ?? "";
            if (KiemTraTrungPhongHoacGV(maLop, ddlCaHocTKB.SelectedValue, ddlThuHocTKB.SelectedValue, ddlPhongHocTKB.SelectedValue, maGV))
                return;

            kncsdl.ThucThiLenh("DELETE FROM LICHHOC WHERE malop=@l", new[] { new SqlParameter("@l", maLop) });
            int count = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM LICHHOC")) + 1;
            string maLH = "LH" + count.ToString("D2");
            while (Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM LICHHOC WHERE malichhoc=@m", new[] { new SqlParameter("@m", maLH) })) > 0)
                maLH = "LH" + (++count).ToString("D2");

            string sqlLH = "INSERT INTO LICHHOC(malichhoc, cahoc, phonghoc, thu, giohoc, malop) VALUES(@ma, @ca, @ph, @thu, @gio, @lop)";
            kncsdl.ThucThiLenh(sqlLH, new[] {
                new SqlParameter("@ma", maLH),
                new SqlParameter("@ca", ddlCaHocTKB.SelectedValue),
                new SqlParameter("@ph", ddlPhongHocTKB.SelectedValue),
                new SqlParameter("@thu", ddlThuHocTKB.SelectedValue),
                new SqlParameter("@gio", txtGioHocTKB.Text.Trim()),
                new SqlParameter("@lop", maLop)
            });

            kncsdl.GhiNhatKy(MaNguoiDung, "Đổi lịch TKB", $"Đổi lịch lớp {maLop}: {ddlCaHocTKB.SelectedValue} - {ddlThuHocTKB.SelectedValue} ({ddlPhongHocTKB.SelectedValue})");
            HienTB("Đã đổi lịch học cố định trên Thời khóa biểu thành công!");
            TaiDanhSachLop();
        }
    }
}