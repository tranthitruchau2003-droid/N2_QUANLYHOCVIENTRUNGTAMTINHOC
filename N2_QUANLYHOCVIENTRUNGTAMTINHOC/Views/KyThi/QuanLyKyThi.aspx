<%@ Page Title="Kỳ thi & Kết quả học tập" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeBehind="QuanLyKyThi.aspx.cs" Inherits="N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.KyThi.QuanLyKyThi" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Kỳ thi & Kết quả</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnlTB" runat="server" Visible="false" CssClass="alert alert-info"><asp:Literal ID="litTB" runat="server" /></asp:Panel>
    <div class="row g-4">
        <!-- BẢNG KYTHI -->
        <div class="col-lg-5">
            <div class="card card-custom">
                <div class="card-header"><span>1. Tổ chức Kỳ thi (KYTHI)</span></div>
                <div class="card-body">
                    <div class="mb-2"><label class="small fw-bold">Mã kỳ thi</label><asp:TextBox ID="txtMaKT" runat="server" CssClass="form-control" placeholder="KT02" /></div>
                    <div class="mb-2"><label class="small fw-bold">Khóa học</label><asp:DropDownList ID="ddlKhoaHoc" runat="server" CssClass="form-select" /></div>
                    <div class="mb-2"><label class="small fw-bold">Niên khóa / Đợt thi</label><asp:TextBox ID="txtNienKhoa" runat="server" CssClass="form-control" placeholder="Đợt 2 - 2026" /></div>
                    <div class="mb-3"><label class="small fw-bold">Ngày thi</label><asp:TextBox ID="txtNgayThi" runat="server" TextMode="Date" CssClass="form-control" /></div>
                    <asp:Button ID="btnTaoKyThi" runat="server" Text="Tạo Kỳ thi" CssClass="btn btn-orange w-100 mb-3" OnClick="btnTaoKyThi_Click" />
                    <asp:GridView ID="gvKyThi" runat="server" CssClass="table table-sm table-bordered table-custom" AutoGenerateColumns="false">
                        <Columns>
                            <asp:BoundField DataField="makythi" HeaderText="Mã KT" />
                            <asp:BoundField DataField="tenkhoahoc" HeaderText="Khóa học" />
                            <asp:BoundField DataField="ngaythi" HeaderText="Ngày thi" DataFormatString="{0:dd/MM/yyyy}" />
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>

        <!-- BẢNG THAM_GIA -->
        <div class="col-lg-7">
            <div class="card card-custom">
                <div class="card-header"><span>2. Nhập Điểm thi Học viên (THAM_GIA)</span></div>
                <div class="card-body">
                    <div class="row g-2 mb-3">
                        <div class="col-3"><label class="small fw-bold">Mã TG</label><asp:TextBox ID="txtMaTG" runat="server" CssClass="form-control" placeholder="TG02" /></div>
                        <div class="col-5"><label class="small fw-bold">Học viên</label><asp:DropDownList ID="ddlHocVien" runat="server" CssClass="form-select" /></div>
                        <div class="col-4"><label class="small fw-bold">Kỳ thi</label><asp:DropDownList ID="ddlKyThi" runat="server" CssClass="form-select" /></div>
                        <div class="col-12"><label class="small fw-bold">Điểm số (Hệ 10 - Tự động xét Đạt >= 5.0)</label><asp:TextBox ID="txtDiem" runat="server" CssClass="form-control" placeholder="8.5" /></div>
                    </div>
                    <asp:Button ID="btnLuuDiem" runat="server" Text="Lưu Điểm & Xếp loại Kết quả" CssClass="btn btn-primary-gradient w-100 mb-3" OnClick="btnLuuDiem_Click" />
                    <asp:GridView ID="gvThamGia" runat="server" CssClass="table table-sm table-bordered table-custom" AutoGenerateColumns="false">
                        <Columns>
                            <asp:BoundField DataField="matg" HeaderText="Mã TG" />
                            <asp:BoundField DataField="hoten" HeaderText="Học viên" />
                            <asp:BoundField DataField="makythi" HeaderText="Kỳ thi" />
                            <asp:BoundField DataField="diem" HeaderText="Điểm số" />
                            <asp:BoundField DataField="ketqua" HeaderText="Kết quả" />
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </div>
</asp:Content>