using System;
using System.Data;
using System.Data.SqlClient;
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
            if (quyen.Equals("GiangVien", StringComparison.OrdinalIgnoreCase))
            {
                // Giảng viên không được thêm/sửa lớp, chỉ mở rộng khung Danh sách để Điểm danh
                pnlFormAdmin.Visible = false;
                colDanhSachLop.Attributes["class"] = "col-12";
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
            string magv = Session["TenDangNhap"]?.ToString() ?? "";
            string kw = txtTimLop.Text.Trim();

            // SQL Lọc lớp theo Quyền (GV chỉ thấy lớp mình phụ trách)
            string sql = @"SELECT l.malop, l.tenlop, l.makhoahoc, kh.tenkhoahoc, 
                                  ISNULL(gv.hoten, N'Chưa phân công') AS hotengv, l.ngaykhaigiang,
                                  (SELECT COUNT(*) FROM DANGKY dk WHERE dk.malop = l.malop) AS siso
                           FROM LOPHOC l 
                           JOIN KHOAHOC kh ON l.makhoahoc = kh.makhoahoc 
                           LEFT JOIN GIANGVIEN gv ON l.magiangvien = gv.magiangvien
                           WHERE (@kw = '' OR l.malop LIKE @kw OR l.tenlop LIKE @kw OR kh.tenkhoahoc LIKE @kw)
                             AND (@isGV = 0 OR l.magiangvien = @magv)
                           ORDER BY l.malop DESC";

            SqlParameter[] p = {
                new SqlParameter("@kw", string.IsNullOrEmpty(kw) ? "" : "%" + kw + "%"),
                new SqlParameter("@isGV", quyen.Equals("GiangVien", StringComparison.OrdinalIgnoreCase) ? 1 : 0),
                new SqlParameter("@magv", magv)
            };

            gvLopHoc.DataSource = kncsdl.DocDuLieu(sql, p);
            gvLopHoc.DataBind();
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
            HienTB("Mở lớp học phần mới thành công!");
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
            HienTB("Cập nhật thông tin lớp thành công!");
            TaiDanhSachLop();
        }

        protected void btnTimLop_Click(object sender, EventArgs e) { TaiDanhSachLop(); }

        protected void gvLopHoc_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ChonLop")
            {
                DataTable dt = kncsdl.DocDuLieu("SELECT * FROM LOPHOC WHERE malop=@m", new[] { new SqlParameter("@m", e.CommandArgument.ToString()) });
                if (dt.Rows.Count > 0)
                {
                    DataRow r = dt.Rows[0]; txtMaLop.Text = r["malop"].ToString(); txtMaLop.ReadOnly = true;
                    txtTenLop.Text = r["tenlop"].ToString(); ddlKhoaHoc.SelectedValue = r["makhoahoc"].ToString();
                    ddlGiangVien.SelectedValue = r["magiangvien"] != DBNull.Value ? r["magiangvien"].ToString() : "";
                    if (r["ngaykhaigiang"] != DBNull.Value) txtKhaiGiang.Text = Convert.ToDateTime(r["ngaykhaigiang"]).ToString("yyyy-MM-dd");
                }
            }
            else if (e.CommandName == "XoaLop")
            {
                int hv = Convert.ToInt32(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM DANGKY WHERE malop=@m", new[] { new SqlParameter("@m", e.CommandArgument.ToString()) }));
                if (hv > 0) { HienTB("Không thể xóa vì lớp này đã có học viên đăng ký!", true); return; }
                kncsdl.ThucThiLenh("DELETE FROM LOPHOC WHERE malop=@m", new[] { new SqlParameter("@m", e.CommandArgument.ToString()) });
                HienTB("Xóa lớp thành công!"); TaiDanhSachLop();
            }
            else if (e.CommandName == "DiemDanh") // CHỨC NĂNG CỦA GIẢNG VIÊN
            {
                string[] parts = e.CommandArgument.ToString().Split('|');
                hfMaLopDiemDanh.Value = parts[0];
                litTenLopDiemDanh.Text = parts[1];

                // Load 10 học viên của lớp này để điểm danh
                string sql = @"SELECT hv.mahocvien, hv.hoten 
                               FROM DANGKY dk JOIN HOCVIEN hv ON dk.mahocvien = hv.mahocvien 
                               WHERE dk.malop=@malop ORDER BY hv.hoten";
                gvHocVienLop.DataSource = kncsdl.DocDuLieu(sql, new[] { new SqlParameter("@malop", hfMaLopDiemDanh.Value) });
                gvHocVienLop.DataBind();

                if (gvHocVienLop.Rows.Count == 0)
                {
                    HienTB("Lớp này hiện chưa có học viên nào đăng ký / xếp lớp!", true); return;
                }

                ScriptManager.RegisterStartupScript(this, GetType(), "showDD", "showModalDiemDanh();", true);
            }
        }

        // LƯU KẾT QUẢ ĐIỂM DANH XUỐNG CSDL
        protected void btnLuuDiemDanh_Click(object sender, EventArgs e)
        {
            string ngayDD = txtNgayDiemDanh.Text;
            string maLop = hfMaLopDiemDanh.Value;
            if (string.IsNullOrEmpty(ngayDD)) return;

            // Xóa điểm danh cũ của ngày hôm đó (nếu GV bấm lưu lại/chỉnh sửa)
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