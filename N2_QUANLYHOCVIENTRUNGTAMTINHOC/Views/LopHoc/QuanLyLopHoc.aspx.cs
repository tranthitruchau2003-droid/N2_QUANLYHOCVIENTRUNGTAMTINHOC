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

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                pnlTB.Visible = false;
            }
            else
            {
                TuKhoaHienTai = "";
                txtTimLop.Text = "";
                txtKhaiGiang.Text = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
                txtNgayDiemDanh.Text = DateTime.Now.ToString("yyyy-MM-dd");
                CauHinhPhanQuyen();
                TaiDanhMuc();
                SinhMaLopMoi();
                TaiDanhSachLop();
            }
        }

        private void SinhMaLopMoi()
        {
            int soLuong = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM LOPHOC")) + 1;
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
            }

            if (LaHocVien)
            {
                pnlTimKiemLop.Visible = false;
                litTieuDeBangLop.Text = "Danh sách Lớp học đã đăng ký của tôi";
                gvLopHoc.Columns[gvLopHoc.Columns.Count - 1].Visible = false;
            }
        }

        private void TaiDanhMuc()
        {
            ddlKhoaHoc.DataSource = kncsdl.DocDuLieu("SELECT makhoahoc, tenkhoahoc FROM KHOAHOC ORDER BY tenkhoahoc");
            ddlKhoaHoc.DataTextField = "tenkhoahoc";
            ddlKhoaHoc.DataValueField = "makhoahoc";
            ddlKhoaHoc.DataBind();

            ddlGiangVien.DataSource = kncsdl.DocDuLieu("SELECT magiangvien, hoten + ISNULL(' (' + hocvi + ')', '') AS HienThi FROM GIANGVIEN ORDER BY hoten");
            ddlGiangVien.DataTextField = "HienThi";
            ddlGiangVien.DataValueField = "magiangvien";
            ddlGiangVien.DataBind();
            ddlGiangVien.Items.Insert(0, new ListItem("-- Chưa phân công --", ""));
        }

        private void TaiDanhSachLop()
        {
            string kw = TuKhoaHienTai;

            string sql = @"SELECT l.malop, l.tenlop, l.makhoahoc, kh.tenkhoahoc, kh.thoigianhoc,
                                  ISNULL(gv.hoten + ISNULL(N' (' + gv.hocvi + N')', N''), N'Chưa phân công') AS hotengv, l.ngaykhaigiang,
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
                           ORDER BY l.malop DESC";

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
                    DateTime ketThuc = TinhNgayKetThuc(batDau, r["thoigianhoc"]?.ToString() ?? "");
                    r["thoigianlop"] = $"{batDau:dd/MM/yyyy} - {ketThuc:dd/MM/yyyy}";
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

        private DateTime TinhNgayKetThuc(DateTime ngayBatDau, string thoiGianHoc)
        {
            if (string.IsNullOrWhiteSpace(thoiGianHoc)) return ngayBatDau.AddMonths(2);

            var mThang = Regex.Match(thoiGianHoc, @"(\d+(?:[\.,]\d+)?)\s*tháng", RegexOptions.IgnoreCase);
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

            var mTuan = Regex.Match(thoiGianHoc, @"(\d+(?:[\.,]\d+)?)\s*tuần", RegexOptions.IgnoreCase);
            if (mTuan.Success)
            {
                decimal tuan;
                if (decimal.TryParse(mTuan.Groups[1].Value.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out tuan) && tuan > 0)
                    return ngayBatDau.AddDays((int)Math.Round(tuan * 7m));
            }

            var mBuoi = Regex.Match(thoiGianHoc, @"(\d+)\s*buổi", RegexOptions.IgnoreCase);
            if (mBuoi.Success)
            {
                int buoi;
                if (int.TryParse(mBuoi.Groups[1].Value, out buoi) && buoi > 0)
                    return ngayBatDau.AddDays((int)Math.Ceiling(buoi / 3.0) * 7);
            }

            return ngayBatDau.AddMonths(2);
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

        private void TaoBangLuoiThoiKhoaBieu()
        {
            string sql = @"SELECT l.malop, l.tenlop, kh.tenkhoahoc, ISNULL(gv.hoten, '') AS hotengv,
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

            string[] dsCa = { "Ca sáng", "Ca chiều", "Ca tối" };
            string[] dsThu = { "2", "3", "4", "5", "6", "7", "CN" };
            string[] tenThu = { "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "Chủ Nhật" };

            StringBuilder sb = new StringBuilder();
            sb.Append("<table class='table table-bordered text-center align-middle mb-0' style='font-size: 0.85rem;'>");
            sb.Append("<thead class='table-light'><tr><th style='width:110px;'>Ca / Thứ</th>");
            foreach (string t in tenThu) sb.AppendFormat("<th>{0}</th>", t);
            sb.Append("</tr></thead><tbody>");

            foreach (string ca in dsCa)
            {
                sb.Append("<tr>");
                sb.AppendFormat("<td class='fw-bold bg-light text-primary'>{0}</td>", ca);
                foreach (string thu in dsThu)
                {
                    List<string> oLich = new List<string>();
                    foreach (DataRow r in dt.Rows)
                    {
                        string caDB = r["cahoc"].ToString().ToLower();
                        string thuDB = r["thu"].ToString().ToUpper();
                        if (caDB.Contains(ca.ToLower()) && thuDB.Contains(thu))
                        {
                            Func<object, string> js = x => HttpUtility.HtmlAttributeEncode(HttpUtility.JavaScriptStringEncode(Convert.ToString(x)));
                            Func<object, string> ht = x => HttpUtility.HtmlEncode(Convert.ToString(x));
                            string onclickAttr = LaQuanTriHoacNV
                                ? "style='cursor:pointer;' title='Bấm để đổi lịch lớp này' onclick=\"moModalDoiLich('" + js(r["malop"]) + "','" + js(r["tenlop"]) + "','" + js(r["cahoc"]) + "','" + js(r["thu"]) + "','" + js(r["giohoc"]) + "','" + js(r["phonghoc"]) + "');\""
                                : "";
                            oLich.Add("<div " + onclickAttr + " class='p-1 mb-1 rounded border bg-primary-subtle text-dark'><strong>" + ht(r["tenlop"]) + "</strong><br/><span class='small text-muted'>" + ht(r["giohoc"]) + " | P." + ht(r["phonghoc"]) + "</span></div>");
                        }
                    }

                    if (oLich.Count > 1)
                    {
                        sb.Append("<td class='bg-danger-subtle'>");
                        sb.Append("<span class='badge bg-danger mb-1'>TRÙNG LỊCH HỌC!</span>");
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
                new SqlParameter("@gv", string.IsNullOrEmpty(ddlGiangVien.SelectedValue) ? (object)DBNull.Value : ddlGiangVien.SelectedValue),
                new SqlParameter("@ngay", string.IsNullOrEmpty(txtKhaiGiang.Text) ? (object)DBNull.Value : txtKhaiGiang.Text)
            });
            CapNhatLichHocChoLop(txtMaLop.Text.Trim());
            kncsdl.GhiNhatKy(MaNguoiDung, "Mở lớp học", $"Mở lớp {txtMaLop.Text.Trim()} - {txtTenLop.Text.Trim()}");
            HienTB("Mở lớp học phần và tạo Thời khóa biểu thành công!");
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
                    txtMaLop.Text = r["malop"].ToString();
                    txtMaLop.ReadOnly = true;
                    txtTenLop.Text = r["tenlop"].ToString();
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
            if (!LaAdmin)
            {
                HienTB("Chỉ Quản trị viên (Admin) mới có quyền xóa lớp học!", true);
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
                double phanTram = Math.Round((double)nghi / tongBuoi * 100, 1);
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

            // BẢNG 1: DANH SÁCH ĐƯỢC DỰ THI
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

            // BẢNG 2: DANH SÁCH BỊ CẤM THI
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
            HienTB("Đã đổi lịch học trực tiếp trên Thời khóa biểu thành công!");
            TaiDanhSachLop();
        }
    }
}