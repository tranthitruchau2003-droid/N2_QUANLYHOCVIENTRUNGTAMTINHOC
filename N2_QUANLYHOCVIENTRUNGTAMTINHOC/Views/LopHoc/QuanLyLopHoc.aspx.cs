using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using N2_QUANLYHOCVIENTRUNGTAMTINHOC.DAL;

namespace N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.LopHoc
{
    public partial class QuanLyLopHoc : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                txtKhaiGiang.Text = DateTime.Now.ToString("yyyy-MM-dd");
                txtNgayDiemDanh.Text = DateTime.Now.ToString("yyyy-MM-dd");
                CauHinhPhanQuyen();
                TaiDanhMuc();
                TaiDanhSachLop();
            }
        }

        private void CauHinhPhanQuyen()
        {
            string quyen = Session["Quyen"]?.ToString() ?? "Admin";
            bool isGV = quyen.Equals("GiangVien", StringComparison.OrdinalIgnoreCase);
            bool isHV = quyen.Equals("HocVien", StringComparison.OrdinalIgnoreCase);

            if (isGV || isHV)
            {
                pnlFormAdmin.Visible = false;
                colDanhSachLop.Attributes["class"] = "col-12";
            }

            if (isHV)
            {
                pnlTimKiemLop.Visible = false;
                litTieuDeBangLop.Text = "Danh sách Lớp học đã đăng ký của tôi";
                gvLopHoc.Columns[gvLopHoc.Columns.Count - 1].Visible = false;
            }
        }

        private void TaiDanhMuc()
        {
            ddlKhoaHoc.DataSource = kncsdl.DocDuLieu("SELECT makhoahoc, tenkhoahoc FROM KHOAHOC");
            ddlKhoaHoc.DataTextField = "tenkhoahoc"; ddlKhoaHoc.DataValueField = "makhoahoc"; ddlKhoaHoc.DataBind();

            ddlGiangVien.DataSource = kncsdl.DocDuLieu("SELECT magiangvien, hoten + ' (' + hocvi + ')' AS HienThi FROM GIANGVIEN");
            ddlGiangVien.DataTextField = "HienThi"; ddlGiangVien.DataValueField = "magiangvien"; ddlGiangVien.DataBind();
            ddlGiangVien.Items.Insert(0, new ListItem("-- Chưa phân công --", ""));
        }

        private void TaiDanhSachLop()
        {
            string quyen = Session["Quyen"]?.ToString() ?? "Admin";
            string user = Session["TenDangNhap"]?.ToString() ?? "";
            string kw = txtTimLop.Text.Trim();

            string sql = @"SELECT l.malop, l.tenlop, l.makhoahoc, kh.tenkhoahoc, 
                                  ISNULL(gv.hoten + N' (' + gv.hocvi + N')', N'Chưa phân công') AS hotengv, l.ngaykhaigiang,
                                  ISNULL(STUFF((SELECT '; ' + lh.cahoc + ' - ' + lh.thu + ' (' + lh.giohoc + ', P.' + lh.phonghoc + ')'
                                                FROM LICHHOC lh WHERE lh.malop = l.malop FOR XML PATH('')), 1, 2, ''), N'Đang cập nhật lịch') AS thoikhoabieu,
                                  CAST((SELECT COUNT(*) FROM DANGKY dk WHERE dk.malop = l.malop) AS NVARCHAR(10)) + '/10 HV' AS siso
                           FROM LOPHOC l 
                           JOIN KHOAHOC kh ON l.makhoahoc = kh.makhoahoc 
                           LEFT JOIN GIANGVIEN gv ON l.magiangvien = gv.magiangvien
                           WHERE (@kw = '' OR l.malop LIKE @kw OR l.tenlop LIKE @kw OR kh.tenkhoahoc LIKE @kw)
                             AND (@isGV = 0 OR l.magiangvien = @user)
                             AND (@isHV = 0 OR l.malop IN (SELECT malop FROM DANGKY WHERE mahocvien = @user))
                           ORDER BY l.malop DESC";

            SqlParameter[] p = {
                new SqlParameter("@kw", string.IsNullOrEmpty(kw) ? "" : "%" + kw + "%"),
                new SqlParameter("@isGV", quyen.Equals("GiangVien", StringComparison.OrdinalIgnoreCase) ? 1 : 0),
                new SqlParameter("@isHV", quyen.Equals("HocVien", StringComparison.OrdinalIgnoreCase) ? 1 : 0),
                new SqlParameter("@user", user)
            };

            gvLopHoc.DataSource = kncsdl.DocDuLieu(sql, p);
            gvLopHoc.DataBind();

            TaoBangLuoiThoiKhoaBieu(quyen, user);
        }

        // VẼ BẢNG THỜI KHÓA BIỂU DẠNG LƯỚI (THỨ 2 -> CHỦ NHẬT) & BÁO ĐỎ NẾU TRÙNG LỊCH
        private void TaoBangLuoiThoiKhoaBieu(string quyen, string user)
        {
            string sql = @"SELECT l.malop, l.tenlop, kh.tenkhoahoc, ISNULL(gv.hoten, '') AS hotengv,
                                  lh.cahoc, lh.thu, lh.giohoc, lh.phonghoc
                           FROM LICHHOC lh
                           JOIN LOPHOC l ON lh.malop = l.malop
                           JOIN KHOAHOC kh ON l.makhoahoc = kh.makhoahoc
                           LEFT JOIN GIANGVIEN gv ON l.magiangvien = gv.magiangvien
                           WHERE (@isGV = 0 OR l.magiangvien = @user)
                             AND (@isHV = 0 OR l.malop IN (SELECT malop FROM DANGKY WHERE mahocvien = @user))";
            SqlParameter[] p = {
                new SqlParameter("@isGV", quyen.Equals("GiangVien", StringComparison.OrdinalIgnoreCase) ? 1 : 0),
                new SqlParameter("@isHV", quyen.Equals("HocVien", StringComparison.OrdinalIgnoreCase) ? 1 : 0),
                new SqlParameter("@user", user)
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
                            oLich.Add($"<div class='p-1 mb-1 rounded border bg-primary-subtle text-dark'><strong>{r["tenlop"]}</strong><br/><span class='small text-muted'>{r["giohoc"]} | P.{r["phonghoc"]}</span></div>");
                        }
                    }

                    if (oLich.Count > 1)
                    {
                        // Cảnh báo đỏ nếu có >= 2 lớp trùng cùng Ca + Thứ
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
                new SqlParameter("@ph", txtPhongHoc.Text.Trim()),
                new SqlParameter("@thu", ddlThuHoc.SelectedValue),
                new SqlParameter("@gio", txtGioHoc.Text.Trim()),
                new SqlParameter("@lop", maLop)
            });
        }

        protected void btnThemLop_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaLop.Text) || string.IsNullOrWhiteSpace(txtTenLop.Text)) { HienTB("Vui lòng nhập Mã lớp và Tên lớp!", true); return; }
            int c = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM LOPHOC WHERE malop=@m", new[] { new SqlParameter("@m", txtMaLop.Text.Trim()) }));
            if (c > 0) { HienTB("Mã lớp đã tồn tại!", true); return; }

            string sql = "INSERT INTO LOPHOC(malop, tenlop, makhoahoc, magiangvien, ngaykhaigiang) VALUES(@ma, @ten, @kh, @gv, @ngay)";
            kncsdl.ThucThiLenh(sql, new[] {
                new SqlParameter("@ma", txtMaLop.Text.Trim()), new SqlParameter("@ten", txtTenLop.Text.Trim()),
                new SqlParameter("@kh", ddlKhoaHoc.SelectedValue), new SqlParameter("@gv", string.IsNullOrEmpty(ddlGiangVien.SelectedValue) ? (object)DBNull.Value : ddlGiangVien.SelectedValue),
                new SqlParameter("@ngay", string.IsNullOrEmpty(txtKhaiGiang.Text) ? (object)DBNull.Value : txtKhaiGiang.Text)
            });
            CapNhatLichHocChoLop(txtMaLop.Text.Trim());
            HienTB("Mở lớp học phần và tạo Thời khóa biểu thành công!");
            TaiDanhSachLop();
        }

        protected void btnSuaLop_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaLop.Text)) { HienTB("Vui lòng chọn lớp cần sửa!", true); return; }
            string sql = "UPDATE LOPHOC SET tenlop=@ten, makhoahoc=@kh, magiangvien=@gv, ngaykhaigiang=@ngay WHERE malop=@ma";
            kncsdl.ThucThiLenh(sql, new[] {
                new SqlParameter("@ma", txtMaLop.Text.Trim()), new SqlParameter("@ten", txtTenLop.Text.Trim()),
                new SqlParameter("@kh", ddlKhoaHoc.SelectedValue), new SqlParameter("@gv", string.IsNullOrEmpty(ddlGiangVien.SelectedValue) ? (object)DBNull.Value : ddlGiangVien.SelectedValue),
                new SqlParameter("@ngay", string.IsNullOrEmpty(txtKhaiGiang.Text) ? (object)DBNull.Value : txtKhaiGiang.Text)
            });
            CapNhatLichHocChoLop(txtMaLop.Text.Trim());
            HienTB("Đã cập nhật thông tin lớp & đổi Thời khóa biểu thành công! Lịch mới đã được cập nhật cho Học viên.");
            TaiDanhSachLop();
        }

        protected void btnTimLop_Click(object sender, EventArgs e) { TaiDanhSachLop(); }

        protected void gvLopHoc_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ChonLop")
            {
                string ma = e.CommandArgument.ToString();
                DataTable dt = kncsdl.DocDuLieu("SELECT * FROM LOPHOC WHERE malop=@m", new[] { new SqlParameter("@m", ma) });
                if (dt.Rows.Count > 0)
                {
                    DataRow r = dt.Rows[0]; txtMaLop.Text = r["malop"].ToString(); txtMaLop.ReadOnly = true;
                    txtTenLop.Text = r["tenlop"].ToString(); ddlKhoaHoc.SelectedValue = r["makhoahoc"].ToString();
                    ddlGiangVien.SelectedValue = r["magiangvien"] != DBNull.Value ? r["magiangvien"].ToString() : "";
                    if (r["ngaykhaigiang"] != DBNull.Value) txtKhaiGiang.Text = Convert.ToDateTime(r["ngaykhaigiang"]).ToString("yyyy-MM-dd");

                    DataTable dtLH = kncsdl.DocDuLieu("SELECT TOP 1 * FROM LICHHOC WHERE malop=@m", new[] { new SqlParameter("@m", ma) });
                    if (dtLH.Rows.Count > 0)
                    {
                        txtGioHoc.Text = dtLH.Rows[0]["giohoc"].ToString();
                        txtPhongHoc.Text = dtLH.Rows[0]["phonghoc"].ToString();
                    }
                }
            }
            else if (e.CommandName == "XoaLop")
            {
                string ma = e.CommandArgument.ToString();
                int hv = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM DANGKY WHERE malop=@m", new[] { new SqlParameter("@m", ma) }));
                if (hv > 0) { HienTB("Không thể xóa vì lớp này đã có học viên đăng ký!", true); return; }
                kncsdl.ThucThiLenh("DELETE FROM LICHHOC WHERE malop=@m", new[] { new SqlParameter("@m", ma) });
                kncsdl.ThucThiLenh("DELETE FROM LOPHOC WHERE malop=@m", new[] { new SqlParameter("@m", ma) });
                HienTB("Xóa lớp thành công!"); TaiDanhSachLop();
            }
            else if (e.CommandName == "DiemDanh")
            {
                string[] parts = e.CommandArgument.ToString().Split('|');
                hfMaLopDiemDanh.Value = parts[0];
                litTenLopDiemDanh.Text = parts[1];

                string sql = @"SELECT hv.mahocvien, hv.hoten 
                               FROM DANGKY dk JOIN HOCVIEN hv ON dk.mahocvien = hv.mahocvien 
                               WHERE dk.malop=@malop ORDER BY hv.hoten";
                gvHocVienLop.DataSource = kncsdl.DocDuLieu(sql, new[] { new SqlParameter("@malop", hfMaLopDiemDanh.Value) });
                gvHocVienLop.DataBind();

                if (gvHocVienLop.Rows.Count == 0)
                {
                    HienTB("Lớp này hiện chưa có học viên nào đăng ký!", true); return;
                }
                ScriptManager.RegisterStartupScript(this, GetType(), "showDD", "showModalDiemDanh();", true);
            }
        }

        protected void btnLuuDiemDanh_Click(object sender, EventArgs e)
        {
            string ngayDD = txtNgayDiemDanh.Text;
            string maLop = hfMaLopDiemDanh.Value;
            if (string.IsNullOrEmpty(ngayDD)) return;

            kncsdl.ThucThiLenh("DELETE FROM DIEMDANH WHERE malop=@l AND ngaydiemdanh=@n",
                new[] { new SqlParameter("@l", maLop), new SqlParameter("@n", ngayDD) });

            foreach (GridViewRow row in gvHocVienLop.Rows)
            {
                string maHV = gvHocVienLop.DataKeys[row.RowIndex].Value.ToString();
                RadioButtonList rbl = (RadioButtonList)row.FindControl("rblTrangThai");
                TextBox txtGC = (TextBox)row.FindControl("txtGhiChuDD");

                string sql = "INSERT INTO DIEMDANH(malop, mahocvien, ngaydiemdanh, trangthai, ghichu) VALUES(@l, @hv, @ngay, @tt, @gc)";
                kncsdl.ThucThiLenh(sql, new[] {
                    new SqlParameter("@l", maLop), new SqlParameter("@hv", maHV), new SqlParameter("@ngay", ngayDD),
                    new SqlParameter("@tt", rbl.SelectedValue), new SqlParameter("@gc", txtGC.Text.Trim())
                });
            }
            HienTB($"Đã lưu kết quả điểm danh ngày {Convert.ToDateTime(ngayDD).ToString("dd/MM/yyyy")} thành công!");
        }

        private void HienTB(string msg, bool laLoi = false)
        {
            pnlTB.Visible = true;
            pnlTB.CssClass = laLoi ? "alert alert-danger text-danger fw-semibold alert-dismissible fade show shadow" : "alert alert-success text-success fw-semibold alert-dismissible fade show shadow";
            litTB.Text = msg;
        }
    }
}