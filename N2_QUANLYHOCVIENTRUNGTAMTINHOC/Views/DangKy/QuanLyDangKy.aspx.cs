using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
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
            if (IsPostBack)
            {
                pnlTB.Visible = false;
            }
            else
            {
                TuKhoaDK = "";
                txtTimDK.Text = "";
                txtNgayDK.Text = DateTime.Now.ToString("yyyy-MM-dd");
                CauHinhTheoQuyen();
                TaiDanhMuc();
                CapNhatHocPhiTheoKhoa(ddlKhoaHoc.SelectedValue);
                TaiDuLieu();
            }
        }

        private void CauHinhTheoQuyen()
        {
            string quyen = Session["Quyen"]?.ToString() ?? "Admin";
            bool isHV = quyen.Equals("HocVien", StringComparison.OrdinalIgnoreCase);
            pnlXuLyLopThieu.Visible = !isHV;
        }

        private void TaiDanhMuc()
        {
            string quyen = Session["Quyen"]?.ToString() ?? "Admin";
            string user = Session["TenDangNhap"]?.ToString() ?? "";

            if (quyen.Equals("HocVien", StringComparison.OrdinalIgnoreCase))
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

            DataTable dtKH = kncsdl.DocDuLieu("SELECT makhoahoc, tenkhoahoc FROM KHOAHOC ORDER BY tenkhoahoc");
            ddlKhoaHoc.DataSource = dtKH;
            ddlKhoaHoc.DataTextField = "tenkhoahoc";
            ddlKhoaHoc.DataValueField = "makhoahoc";
            ddlKhoaHoc.DataBind();

            ddlLocKhoaMo.DataSource = dtKH;
            ddlLocKhoaMo.DataTextField = "tenkhoahoc";
            ddlLocKhoaMo.DataValueField = "makhoahoc";
            ddlLocKhoaMo.DataBind();
            ddlLocKhoaMo.Items.Insert(0, new ListItem("-- Tất cả Khóa học --", ""));

            TaiLopTheoKhoa(ddlKhoaHoc.SelectedValue);
        }

        private void TaiLopTheoKhoa(string maKH)
        {
            string sql = @"SELECT l.malop, l.tenlop + N' - GV: ' + ISNULL(gv.hoten, N'Chưa phân công') + N' (Sĩ số: ' + CAST((SELECT COUNT(*) FROM DANGKY dk WHERE dk.malop = l.malop) AS NVARCHAR(10)) + N'/10)' AS HienThi
                           FROM LOPHOC l LEFT JOIN GIANGVIEN gv ON l.magiangvien = gv.magiangvien WHERE (@kh = '' OR l.makhoahoc = @kh)";
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
            string sqlLopMo = @"SELECT l.malop, l.tenlop, l.makhoahoc, kh.tenkhoahoc, kh.hocphi,
                                       ISNULL(gv.hoten + N' (' + gv.hocvi + N')', N'Chưa phân công') AS tengiangvien,
                                       ISNULL(STUFF((SELECT '; ' + lh.cahoc + ' - ' + lh.thu + ' (' + lh.giohoc + ', P.' + lh.phonghoc + ')'
                                                     FROM LICHHOC lh WHERE lh.malop = l.malop FOR XML PATH('')), 1, 2, ''), N'Đang cập nhật lịch') AS thoikhoabieu,
                                       (SELECT COUNT(*) FROM DANGKY dk WHERE dk.malop = l.malop) AS siso,
                                       CASE 
                                           WHEN (SELECT COUNT(*) FROM DANGKY dk WHERE dk.malop = l.malop) >= 10 THEN N'Đã mở lớp (Đủ 10/10 HV)'
                                           WHEN EXISTS (SELECT 1 FROM DANGKY dk WHERE dk.malop = l.malop AND dk.trangthailop LIKE N'%đặc cách%') THEN N'Đã mở lớp (Đặc cách)'
                                           ELSE N'Chờ đủ 10 HV (' + CAST((SELECT COUNT(*) FROM DANGKY dk WHERE dk.malop = l.malop) AS NVARCHAR(10)) + N'/10)'
                                       END AS trangthailop
                                FROM LOPHOC l JOIN KHOAHOC kh ON l.makhoahoc = kh.makhoahoc
                                LEFT JOIN GIANGVIEN gv ON l.magiangvien = gv.magiangvien
                                WHERE (@kh = '' OR l.makhoahoc = @kh) ORDER BY l.malop DESC";
            gvLopMo.DataSource = kncsdl.DocDuLieu(sqlLopMo, new[] { new SqlParameter("@kh", ddlLocKhoaMo.SelectedValue ?? "") });
            gvLopMo.DataBind();

            string sqlThieu = @"SELECT l.malop, l.tenlop + N' (' + CAST(COUNT(dk.madangky) AS NVARCHAR(10)) + N'/10 HV)' AS HienThi
                                FROM LOPHOC l LEFT JOIN DANGKY dk ON l.malop = dk.malop GROUP BY l.malop, l.tenlop HAVING COUNT(dk.madangky) > 0 AND COUNT(dk.madangky) < 10";
            ddlLopThieu.DataSource = kncsdl.DocDuLieu(sqlThieu);
            ddlLopThieu.DataTextField = "HienThi"; ddlLopThieu.DataValueField = "malop"; ddlLopThieu.DataBind();

            string sqlDich = @"SELECT l.malop, l.tenlop + N' (' + CAST(COUNT(dk.madangky) AS NVARCHAR(10)) + N'/10 HV)' AS HienThi
                               FROM LOPHOC l LEFT JOIN DANGKY dk ON l.malop = dk.malop GROUP BY l.malop, l.tenlop HAVING COUNT(dk.madangky) < 10";
            ddlLopDich.DataSource = kncsdl.DocDuLieu(sqlDich);
            ddlLopDich.DataTextField = "HienThi"; ddlLopDich.DataValueField = "malop"; ddlLopDich.DataBind();

            string quyen = Session["Quyen"]?.ToString() ?? "Admin";
            string user = Session["TenDangNhap"]?.ToString() ?? "";
            string kw = TuKhoaDK;

            string sqlDK = @"SELECT dk.madangky, hv.hoten, kh.tenkhoahoc, kh.hocphi,
                                    ISNULL(l.tenlop + N' (GV: ' + gv.hoten + N')', N'Chưa xếp lớp') AS tenlop,
                                    ISNULL(SUM(pt.sotienthu), 0) AS dadong,
                                    (kh.hocphi - ISNULL(SUM(pt.sotienthu), 0)) AS conno,
                                    ISNULL(dk.trangthailop, N'Chờ đủ 10 HV') AS trangthailop
                             FROM DANGKY dk
                             JOIN HOCVIEN hv ON dk.mahocvien = hv.mahocvien
                             JOIN KHOAHOC kh ON dk.makhoahoc = kh.makhoahoc
                             LEFT JOIN LOPHOC l ON dk.malop = l.malop
                             LEFT JOIN GIANGVIEN gv ON l.magiangvien = gv.magiangvien
                             LEFT JOIN PHIEUTHU pt ON dk.madangky = pt.madangky
                             WHERE (@isHV = 0 OR dk.mahocvien = @user)
                               AND (@kw = '' OR dk.madangky LIKE @kw OR hv.hoten LIKE @kw OR kh.tenkhoahoc LIKE @kw OR ISNULL(l.tenlop, '') LIKE @kw)
                             GROUP BY dk.madangky, hv.hoten, kh.tenkhoahoc, kh.hocphi, l.tenlop, gv.hoten, dk.trangthailop
                             ORDER BY dk.madangky DESC";
            SqlParameter[] pDK = {
                new SqlParameter("@isHV", quyen.Equals("HocVien", StringComparison.OrdinalIgnoreCase) ? 1 : 0),
                new SqlParameter("@user", user),
                new SqlParameter("@kw", string.IsNullOrEmpty(kw) ? "" : "%" + kw + "%")
            };
            gvDangKy.DataSource = kncsdl.DocDuLieu(sqlDK, pDK);
            gvDangKy.DataBind();

            TaiGoiYTimKiemDK(quyen.Equals("HocVien", StringComparison.OrdinalIgnoreCase), user);
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
                    r["madangky"], r["hoten"], r["tenlop"]);
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
            string maHV = ddlHocVien.SelectedValue;
            string maKH = ddlKhoaHoc.SelectedValue;
            if (string.IsNullOrEmpty(maLop)) { HienTB("Vui lòng chọn Lớp học phần!", true); return; }

            int siSoHienTai = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM DANGKY WHERE malop=@l", new[] { new SqlParameter("@l", maLop) }));
            if (siSoHienTai >= SI_SO_CHUAN) { HienTB("Lớp học này đã đủ 10/10 học viên! Vui lòng chọn lớp khác.", true); return; }

            int daDK = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM DANGKY WHERE mahocvien=@hv AND malop=@l", new[] { new SqlParameter("@hv", maHV), new SqlParameter("@l", maLop) }));
            if (daDK > 0) { HienTB("Học viên đã đăng ký lớp học này rồi!", true); return; }

            if (KiemTraTrungLichVaGoiY(maHV, maLop, maKH)) return;

            int count = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM DANGKY")) + 1;
            string maDK = "DK" + count.ToString("D2");
            while (Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM DANGKY WHERE madangky=@m", new[] { new SqlParameter("@m", maDK) })) > 0)
                maDK = "DK" + (++count).ToString("D2");

            string sql = @"INSERT INTO DANGKY(madangky, ngaydangky, sotiendong, trangthaithanhtoan, mahocvien, makhoahoc, malop, trangthailop)
                           VALUES(@ma, @ngay, 0, N'Chưa thanh toán', @hv, @kh, @lop, N'Chờ mở lớp')";
            kncsdl.ThucThiLenh(sql, new[] {
                new SqlParameter("@ma", maDK),
                new SqlParameter("@ngay", string.IsNullOrEmpty(txtNgayDK.Text) ? DateTime.Now.ToString("yyyy-MM-dd") : txtNgayDK.Text),
                new SqlParameter("@hv", maHV), new SqlParameter("@kh", maKH), new SqlParameter("@lop", maLop)
            });
            CapNhatTrangThaiSiSoLop(maLop);
            HienTB($"Đăng ký lớp {maLop} thành công (Sĩ số: {siSoHienTai + 1}/10)! Hãy bấm nút 'Đóng HP' ở bảng bên phải để quét mã QR chuyển khoản.");
            txtMaDK.Text = "";
            gvDangKy.PageIndex = 0;
            TaiDanhMuc();
            CapNhatHocPhiTheoKhoa(ddlKhoaHoc.SelectedValue);
            TaiDuLieu();
        }

        protected void btnDoiLop_Click(object sender, EventArgs e)
        {
            string maDK = txtMaDK.Text.Trim();
            string maLopMoi = ddlLopHoc.SelectedValue;
            if (string.IsNullOrEmpty(maDK) || string.IsNullOrEmpty(maLopMoi)) { HienTB("Vui lòng chọn nút Đổi lớp ở danh sách bên phải!", true); return; }

            int siSo = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM DANGKY WHERE malop=@l", new[] { new SqlParameter("@l", maLopMoi) }));
            if (siSo >= SI_SO_CHUAN) { HienTB("Lớp mới đã đủ 10/10 học viên, vui lòng chọn lớp khác!", true); return; }

            if (KiemTraTrungLichVaGoiY(ddlHocVien.SelectedValue, maLopMoi, ddlKhoaHoc.SelectedValue, maDK)) return;

            string lopCu = kncsdl.LayGiaTriDon("SELECT malop FROM DANGKY WHERE madangky=@ma", new[] { new SqlParameter("@ma", maDK) })?.ToString();
            kncsdl.ThucThiLenh("UPDATE DANGKY SET malop=@lop, makhoahoc=@kh WHERE madangky=@ma",
                new[] { new SqlParameter("@lop", maLopMoi), new SqlParameter("@kh", ddlKhoaHoc.SelectedValue), new SqlParameter("@ma", maDK) });

            CapNhatTrangThaiSiSoLop(lopCu);
            CapNhatTrangThaiSiSoLop(maLopMoi);
            HienTB($"Đã chuyển đăng ký {maDK} sang lớp {maLopMoi} thành công!");
            TaiDanhMuc();
            TaiDuLieu();
        }

        protected void gvLopMo_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "DangKyLop")
            {
                string[] parts = e.CommandArgument.ToString().Split('|');
                ddlKhoaHoc.SelectedValue = parts[1];
                TaiLopTheoKhoa(parts[1]);
                if (ddlLopHoc.Items.FindByValue(parts[0]) != null) ddlLopHoc.SelectedValue = parts[0];
                CapNhatHocPhiTheoKhoa(parts[1]);
                HienTB($"Đã chọn lớp {parts[0]}. Hãy bấm 'Xác nhận Đăng ký' ở khung bên trái!");
            }
        }

        protected void gvDangKy_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ChonDK")
            {
                DataTable dt = kncsdl.DocDuLieu("SELECT * FROM DANGKY WHERE madangky=@m", new[] { new SqlParameter("@m", e.CommandArgument.ToString()) });
                if (dt.Rows.Count > 0)
                {
                    DataRow r = dt.Rows[0];
                    txtMaDK.Text = r["madangky"].ToString();
                    ddlHocVien.SelectedValue = r["mahocvien"].ToString();
                    ddlKhoaHoc.SelectedValue = r["makhoahoc"].ToString();
                    TaiLopTheoKhoa(r["makhoahoc"].ToString());
                    CapNhatHocPhiTheoKhoa(r["makhoahoc"].ToString());
                    if (r["malop"] != DBNull.Value && ddlLopHoc.Items.FindByValue(r["malop"].ToString()) != null)
                        ddlLopHoc.SelectedValue = r["malop"].ToString();
                }
            }
            else if (e.CommandName == "DongHocPhi")
            {
                pnlTB.Visible = false;
                string[] parts = e.CommandArgument.ToString().Split('|');
                hfMaDKThanhToan.Value = parts[0];
                decimal conNo = Convert.ToDecimal(parts[1]);
                hfSoTienThanhToan.Value = conNo.ToString();
                litSoTienQR.Text = conNo.ToString("N0") + " VNĐ";
                litNoiDungQR.Text = "HOCPHI " + parts[0];
                string qrUrl = $"https://img.vietqr.io/image/VCB-1023407856-compact2.png?amount={(long)conNo}&addInfo=HOCPHI%20{parts[0]}&accountName=TRUNG%20TAM%20TIN%20HOC";
                imgQRCode.ImageUrl = qrUrl;
                lnkMoAnhQR.NavigateUrl = qrUrl;
                ScriptManager.RegisterStartupScript(this, GetType(), "qr_modal", "moModalQR();", true);
            }
        }

        protected void btnHuyDKChinhThuc_Click(object sender, EventArgs e)
        {
            string maDK = hfMaDKHuy.Value;
            if (string.IsNullOrEmpty(maDK)) return;
            if (Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM PHIEUTHU WHERE madangky=@m", new[] { new SqlParameter("@m", maDK) })) > 0)
            {
                HienTB("Đăng ký này đã phát sinh Phiếu thu học phí, chỉ được phép Đổi lớp chứ không thể xóa!", true);
                return;
            }
            string lopCu = kncsdl.LayGiaTriDon("SELECT malop FROM DANGKY WHERE madangky=@m", new[] { new SqlParameter("@m", maDK) })?.ToString();
            kncsdl.ThucThiLenh("DELETE FROM DANGKY WHERE madangky=@m", new[] { new SqlParameter("@m", maDK) });
            CapNhatTrangThaiSiSoLop(lopCu);
            HienTB("Đã hủy đăng ký học phần thành công!");
            if (gvDangKy.Rows.Count == 1 && gvDangKy.PageIndex > 0) gvDangKy.PageIndex--;
            TaiDanhMuc();
            TaiDuLieu();
        }

        protected void btnXacNhanCK_Click(object sender, EventArgs e)
        {
            string maDK = hfMaDKThanhToan.Value;
            decimal conNo = Convert.ToDecimal(hfSoTienThanhToan.Value);
            if (!fuMinhChungCK.HasFile)
            {
                HienTB("Vui lòng chọn ảnh chụp màn hình chuyển khoản thành công trước khi xác nhận!", true);
                ScriptManager.RegisterStartupScript(this, GetType(), "qr_reopen", "moModalQR();", true);
                return;
            }
            string folder = Server.MapPath("~/Uploads/BienLai");
            if (!System.IO.Directory.Exists(folder)) System.IO.Directory.CreateDirectory(folder);
            string fileName = $"BienLai_{maDK}_{DateTime.Now:yyyyMMdd_HHmmss}{System.IO.Path.GetExtension(fuMinhChungCK.FileName)}";
            fuMinhChungCK.SaveAs(System.IO.Path.Combine(folder, fileName));

            int countPT = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM PHIEUTHU")) + 1;
            string maPT = "PT" + countPT.ToString("D2");
            while (Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM PHIEUTHU WHERE maphieuthu=@m", new[] { new SqlParameter("@m", maPT) })) > 0)
                maPT = "PT" + (++countPT).ToString("D2");

            kncsdl.ThucThiLenh(@"INSERT INTO PHIEUTHU(maphieuthu, ngaythu, sotienthu, nguoithu, phuongthucthanhtoan, noidungthu, madangky) 
                                 VALUES(@ma, GETDATE(), @tien, @nguoithu, N'Chuyển khoản QR', @nd, @dk)",
                new[] {
                    new SqlParameter("@ma", maPT),
                    new SqlParameter("@tien", conNo),
                    new SqlParameter("@nguoithu", Session["HoTen"]?.ToString() ?? "Hệ thống"),
                    new SqlParameter("@nd", "CK QR (Ảnh: " + fileName + ")"),
                    new SqlParameter("@dk", maDK)
                });
            kncsdl.ThucThiLenh("UPDATE DANGKY SET sotiendong = sotiendong + @tien, trangthaithanhtoan=N'Đã thanh toán' WHERE madangky=@m",
                new[] { new SqlParameter("@tien", conNo), new SqlParameter("@m", maDK) });

            HienTB($"Đã nhận ảnh biên lai và thanh toán {conNo:N0} đ thành công cho mã đăng ký {maDK}!");
            TaiDuLieu();
        }

        protected void btnGhepLop_Click(object sender, EventArgs e)
        {
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

            int ssThieu = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM DANGKY WHERE malop=@l", new[] { new SqlParameter("@l", lopThieu) }));
            int ssDich = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM DANGKY WHERE malop=@l", new[] { new SqlParameter("@l", lopDich) }));

            if (ssThieu + ssDich > SI_SO_CHUAN)
            {
                HienTB($"Không thể ghép! Tổng sĩ số sau ghép là {ssThieu + ssDich}/10 HV (vượt quá 10 học viên).", true);
                return;
            }

            string maKHDich = kncsdl.LayGiaTriDon("SELECT makhoahoc FROM LOPHOC WHERE malop=@l", new[] { new SqlParameter("@l", lopDich) })?.ToString();
            kncsdl.ThucThiLenh("UPDATE DANGKY SET malop=@dich, makhoahoc=@kh WHERE malop=@thieu",
                new[] { new SqlParameter("@dich", lopDich), new SqlParameter("@kh", maKHDich), new SqlParameter("@thieu", lopThieu) });

            CapNhatTrangThaiSiSoLop(lopThieu);
            CapNhatTrangThaiSiSoLop(lopDich);
            HienTB($"Đã ghép toàn bộ {ssThieu} học viên từ lớp {lopThieu} sang lớp {lopDich} thành công!");
            TaiDanhMuc();
            TaiDuLieu();
        }

        protected void btnMoDacCach_Click(object sender, EventArgs e)
        {
            string lopThieu = ddlLopThieu.SelectedValue;
            if (string.IsNullOrEmpty(lopThieu))
            {
                HienTB("Không có lớp thiếu nào được chọn!", true);
                return;
            }

            kncsdl.ThucThiLenh("UPDATE DANGKY SET trangthailop=N'Đã mở lớp (Đặc cách)' WHERE malop=@l",
                new[] { new SqlParameter("@l", lopThieu) });
            HienTB($"Đã duyệt mở lớp đặc cách cho lớp {lopThieu}!");
            TaiDuLieu();
        }

        protected void btnHuyLopThieu_Click(object sender, EventArgs e)
        {
            string lopThieu = ddlLopThieu.SelectedValue;
            if (string.IsNullOrEmpty(lopThieu))
            {
                HienTB("Không có lớp thiếu nào được chọn!", true);
                return;
            }

            kncsdl.ThucThiLenh("UPDATE DANGKY SET malop=NULL, trangthailop=N'Hủy lớp - Bảo lưu học phí' WHERE malop=@l",
                new[] { new SqlParameter("@l", lopThieu) });
            HienTB($"Đã hủy lớp {lopThieu} và chuyển trạng thái bảo lưu học phí cho học viên!");
            TaiDanhMuc();
            TaiDuLieu();
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
            litTB.Text = msg;
        }

        private bool KiemTraTrungLichVaGoiY(string maHV, string maLopMoi, string maKH, string maDKBoQua = "")
        {
            DataTable dtLichMoi = kncsdl.DocDuLieu("SELECT cahoc, thu, giohoc FROM LICHHOC WHERE malop=@l", new[] { new SqlParameter("@l", maLopMoi) });
            if (dtLichMoi.Rows.Count == 0) return false;

            string caMoi = dtLichMoi.Rows[0]["cahoc"].ToString().Trim();
            string thuMoi = dtLichMoi.Rows[0]["thu"].ToString().Trim();
            DataTable dtTrung = kncsdl.DocDuLieu(@"SELECT l.tenlop FROM DANGKY dk JOIN LOPHOC l ON dk.malop = l.malop JOIN LICHHOC lh ON l.malop = lh.malop 
                                                   WHERE dk.mahocvien = @hv AND (@madk = '' OR dk.madangky <> @madk) AND lh.cahoc = @ca AND lh.thu = @thu",
                new[] { new SqlParameter("@hv", maHV), new SqlParameter("@madk", maDKBoQua ?? ""), new SqlParameter("@ca", caMoi), new SqlParameter("@thu", thuMoi) });

            if (dtTrung.Rows.Count == 0) return false;

            string lopBiTrung = dtTrung.Rows[0]["tenlop"].ToString();
            DataTable dtGoiY1 = kncsdl.DocDuLieu(@"SELECT l.malop, l.tenlop, lh.cahoc, lh.thu, lh.giohoc FROM LOPHOC l JOIN LICHHOC lh ON l.malop = lh.malop 
                                                   WHERE l.makhoahoc = @kh AND l.malop <> @lopmoi AND NOT (lh.cahoc = @ca AND lh.thu = @thu) AND (SELECT COUNT(*) FROM DANGKY d WHERE d.malop = l.malop) < 10",
                new[] { new SqlParameter("@kh", maKH), new SqlParameter("@lopmoi", maLopMoi), new SqlParameter("@ca", caMoi), new SqlParameter("@thu", thuMoi) });

            if (dtGoiY1.Rows.Count > 0)
            {
                DataRow r = dtGoiY1.Rows[0];
                HienTB($"TRÙNG LỊCH ({caMoi} - {thuMoi}) với '{lopBiTrung}'! GỢI Ý: Chọn lớp '{r["tenlop"]}' ({r["malop"]}) vào {r["cahoc"]} - {r["thu"]} không bị trùng!", true);
                return true;
            }
            HienTB($"TRÙNG LỊCH HỌC ({caMoi} - {thuMoi}) với lớp '{lopBiTrung}'! Khóa học này không còn lớp nào khác giờ trống chỗ.", true);
            return true;
        }
    }
}