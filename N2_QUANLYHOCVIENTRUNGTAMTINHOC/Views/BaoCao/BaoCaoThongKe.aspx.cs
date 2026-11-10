using System;
using N2_QUANLYHOCVIENTRUNGTAMTINHOC.DAL;

namespace N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.BaoCao
{
    public partial class BaoCaoThongKe : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                gvCongNo.DataSource = kncsdl.DocDuLieu(@"
                    SELECT dk.madangky, hv.hoten, hv.dienthoai, kh.tenkhoahoc, kh.hocphi, dk.sotiendong, (kh.hocphi - dk.sotiendong) AS ConNo
                    FROM DANGKY dk JOIN HOCVIEN hv ON dk.mahocvien=hv.mahocvien JOIN KHOAHOC kh ON dk.makhoahoc=kh.makhoahoc
                    WHERE kh.hocphi > dk.sotiendong");
                gvCongNo.DataBind();

                gvTheoKhoa.DataSource = kncsdl.DocDuLieu(@"
                    SELECT kh.makhoahoc, kh.tenkhoahoc, COUNT(dk.madangky) AS SoHV, ISNULL(SUM(dk.sotiendong),0) AS TongThu
                    FROM KHOAHOC kh LEFT JOIN DANGKY dk ON kh.makhoahoc=dk.makhoahoc
                    GROUP BY kh.makhoahoc, kh.tenkhoahoc");
                gvTheoKhoa.DataBind();
            }
        }
    }
}