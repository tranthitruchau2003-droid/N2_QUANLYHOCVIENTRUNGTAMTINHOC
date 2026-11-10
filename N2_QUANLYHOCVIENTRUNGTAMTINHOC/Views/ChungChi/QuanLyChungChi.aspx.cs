using System;
using System.Data.SqlClient;
using N2_QUANLYHOCVIENTRUNGTAMTINHOC.DAL;

namespace N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.ChungChi
{
    public partial class QuanLyChungChi : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) TaiDuLieu();
        }

        private void TaiDuLieu()
        {
            string sqlTG = @"SELECT tg.matg, tg.matg + ' - ' + hv.hoten + ' (Điểm: ' + CAST(tg.diem AS VARCHAR) + ')' AS HienThi
                             FROM THAM_GIA tg JOIN HOCVIEN hv ON tg.mahocvien=hv.mahocvien
                             WHERE tg.ketqua = N'Đạt' AND tg.matg NOT IN (SELECT matg FROM CHUNGCHI)";
            ddlThamGia.DataSource = kncsdl.DocDuLieu(sqlTG);
            ddlThamGia.DataTextField = "HienThi"; ddlThamGia.DataValueField = "matg"; ddlThamGia.DataBind();

            string sqlCC = @"SELECT cc.*, hv.hoten, tg.diem FROM CHUNGCHI cc
                             JOIN THAM_GIA tg ON cc.matg=tg.matg JOIN HOCVIEN hv ON tg.mahocvien=hv.mahocvien";
            gvChungChi.DataSource = kncsdl.DocDuLieu(sqlCC);
            gvChungChi.DataBind();
        }

        protected void btnCapCC_Click(object sender, EventArgs e)
        {
            string sql = "INSERT INTO CHUNGCHI(machungchi, ngaycap, sohieuchungchi, xeploai, matg) VALUES(@ma, GETDATE(), @sh, @xl, @tg)";
            SqlParameter[] p = {
                new SqlParameter("@ma", txtMaCC.Text.Trim()),
                new SqlParameter("@sh", txtSoHieu.Text.Trim()),
                new SqlParameter("@xl", ddlXepLoai.SelectedValue),
                new SqlParameter("@tg", ddlThamGia.SelectedValue)
            };
            kncsdl.ThucThiLenh(sql, p);
            kncsdl.GhiNhatKy(Session["TenDangNhap"]?.ToString(), "Cấp chứng chỉ", "Số hiệu: " + txtSoHieu.Text.Trim());
            pnlTB.Visible = true; litTB.Text = "Cấp chứng chỉ thành công!";
            TaiDuLieu();
        }
    }
}