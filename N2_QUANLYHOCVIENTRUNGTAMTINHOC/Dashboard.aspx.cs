using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using N2_QUANLYHOCVIENTRUNGTAMTINHOC.DAL;

namespace N2_QUANLYHOCVIENTRUNGTAMTINHOC
{
    public partial class Dashboard : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
           
            string quyen = MasterPage.ChuanHoaQuyen(Session["Quyen"]);
            if (quyen != "Admin" && quyen != "NhanVien")
            {
                string target = quyen == "GiangVien" ? "~/Views/LopHoc/QuanLyLopHoc.aspx" : "~/Views/DangKy/QuanLyDangKy.aspx";
                Response.Redirect(target, false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack) TaiDuLieuDashboard();
        }

        private void TaiDuLieuDashboard()
        {
            litTongHV.Text = Convert.ToString(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM HOCVIEN") ?? 0);
            litTongKH.Text = Convert.ToString(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM KHOAHOC") ?? 0);
            litTongLop.Text = Convert.ToString(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM LOPHOC") ?? 0);
            litTongGV.Text = Convert.ToString(kncsdl.LayGiaTriDon("SELECT COUNT(*) FROM GIANGVIEN") ?? 0);

            decimal doanhThu = Convert.ToDecimal(kncsdl.LayGiaTriDon("SELECT ISNULL(SUM(sotienthu),0) FROM PHIEUTHU") ?? 0);
            litDoanhThu.Text = doanhThu.ToString("N0") + " đ";

         
            string sqlNo = @"SELECT ISNULL(SUM(T.ConNo), 0)
                             FROM (
                                 SELECT (kh.hocphi - ISNULL(SUM(pt.sotienthu), 0)) AS ConNo
                                 FROM DANGKY dk
                                 JOIN KHOAHOC kh ON dk.makhoahoc = kh.makhoahoc
                                 LEFT JOIN PHIEUTHU pt ON dk.madangky = pt.madangky
                                 WHERE ISNULL(dk.trangthailop, N'') NOT LIKE N'%Hủy lớp%'
                                 GROUP BY dk.madangky, kh.hocphi
                                 HAVING (kh.hocphi - ISNULL(SUM(pt.sotienthu), 0)) > 0
                             ) T";
            decimal congNo = Convert.ToDecimal(kncsdl.LayGiaTriDon(sqlNo) ?? 0);
            litCongNo.Text = congNo.ToString("N0") + " đ";

         
            gvLopMoi.DataSource = kncsdl.DocDuLieu(@"SELECT TOP 5 l.malop, l.tenlop, l.ngaykhaigiang, ISNULL(g.hoten, N'Chưa phân công') AS hoten
                                                     FROM LOPHOC l 
                                                     LEFT JOIN GIANGVIEN g ON l.magiangvien = g.magiangvien 
                                                     ORDER BY CASE WHEN l.ngaykhaigiang >= CAST(GETDATE() AS DATE) THEN 0 ELSE 1 END, l.ngaykhaigiang ASC");
            gvLopMoi.DataBind();

          
            gvDangKyMoi.DataSource = kncsdl.DocDuLieu(@"SELECT TOP 5 dk.madangky, hv.hoten, kh.tenkhoahoc, ISNULL(SUM(pt.sotienthu), 0) AS sotiendong
                                                        FROM DANGKY dk 
                                                        JOIN HOCVIEN hv ON dk.mahocvien = hv.mahocvien
                                                        JOIN KHOAHOC kh ON dk.makhoahoc = kh.makhoahoc
                                                        LEFT JOIN PHIEUTHU pt ON dk.madangky = pt.madangky
                                                        GROUP BY dk.madangky, hv.hoten, kh.tenkhoahoc, dk.ngaydangky
                                                        ORDER BY dk.ngaydangky DESC, dk.madangky DESC");
            gvDangKyMoi.DataBind();

           
            DataTable dtDT = kncsdl.DocDuLieu(@"SELECT YEAR(ngaythu) AS Nam, MONTH(ngaythu) AS Thang, SUM(sotienthu) AS Tong 
                                                FROM PHIEUTHU 
                                                GROUP BY YEAR(ngaythu), MONTH(ngaythu) 
                                                ORDER BY Nam, Thang");
            List<string> lThang = new List<string>();
            List<string> lTien = new List<string>();
            foreach (DataRow r in dtDT.Rows)
            {
                lThang.Add(string.Format("{0:D2}/{1}", Convert.ToInt32(r["Thang"]), r["Nam"]));
                lTien.Add(Convert.ToDecimal(r["Tong"]).ToString("0.##", CultureInfo.InvariantCulture));
            }
            hfThangDoanhThu.Value = string.Join("|", lThang);
            hfGiaTriDoanhThu.Value = string.Join("|", lTien);

        
            DataTable dtKH = kncsdl.DocDuLieu(@"SELECT kh.tenkhoahoc, COUNT(dk.madangky) AS SL 
                                                FROM KHOAHOC kh LEFT JOIN DANGKY dk ON kh.makhoahoc = dk.makhoahoc 
                                                GROUP BY kh.tenkhoahoc");
            List<string> lTenKH = new List<string>();
            List<string> lSL = new List<string>();
            foreach (DataRow r in dtKH.Rows)
            {
                lTenKH.Add(r["tenkhoahoc"].ToString().Replace("|", "-"));
                lSL.Add(Convert.ToInt32(r["SL"]).ToString(CultureInfo.InvariantCulture));
            }
            hfTenKhoaHoc.Value = string.Join("|", lTenKH);
            hfSoLuongHV.Value = string.Join("|", lSL);
        }
    }
}