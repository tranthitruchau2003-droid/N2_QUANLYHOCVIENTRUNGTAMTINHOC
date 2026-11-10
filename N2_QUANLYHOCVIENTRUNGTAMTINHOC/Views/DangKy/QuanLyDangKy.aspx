<%@ Page Title="Đăng ký Khóa học" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeBehind="QuanLyDangKy.aspx.cs" Inherits="N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.DangKy.QuanLyDangKy" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Đăng ký Khóa học</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnlTB" runat="server" Visible="false" CssClass="alert alert-info"><asp:Literal ID="litTB" runat="server" /></asp:Panel>
    <div class="row g-4">
        <div class="col-lg-4">
            <div class="card card-custom">
                <div class="card-header"><span>Phiếu Đăng ký Khóa học</span></div>
                <div class="card-body">
                    <div class="mb-2"><label class="small fw-bold">Mã đăng ký (*)</label><asp:TextBox ID="txtMaDK" runat="server" CssClass="form-control" placeholder="DK02" /></div>
                    <div class="mb-2"><label class="small fw-bold">Chọn Học viên</label><asp:DropDownList ID="ddlHocVien" runat="server" CssClass="form-select" /></div>
                    <div class="mb-2"><label class="small fw-bold">Chọn Khóa học</label><asp:DropDownList ID="ddlKhoaHoc" runat="server" CssClass="form-select" /></div>
                    <div class="mb-3"><label class="small fw-bold">Ngày đăng ký</label><asp:TextBox ID="txtNgayDK" runat="server" TextMode="Date" CssClass="form-control" /></div>
                    <asp:Button ID="btnDangKy" runat="server" Text="Ghi nhận Đăng ký" CssClass="btn btn-orange w-100" OnClick="btnDangKy_Click" />
                </div>
            </div>
        </div>
        <div class="col-lg-8">
            <div class="card card-custom">
                <div class="card-header"><span>Lịch sử Đăng ký & Công nợ Học phí</span></div>
                <div class="card-body p-0">
                    <asp:GridView ID="gvDangKy" runat="server" CssClass="table table-hover table-custom mb-0" AutoGenerateColumns="false">
                        <Columns>
                            <asp:BoundField DataField="madangky" HeaderText="Mã ĐK" />
                            <asp:BoundField DataField="hoten" HeaderText="Học viên" />
                            <asp:BoundField DataField="tenkhoahoc" HeaderText="Khóa học" />
                            <asp:BoundField DataField="hocphi" HeaderText="Học phí" DataFormatString="{0:N0} đ" />
                            <asp:BoundField DataField="sotiendong" HeaderText="Đã đóng" DataFormatString="{0:N0} đ" />
                            <asp:BoundField DataField="trangthaithanhtoan" HeaderText="Trạng thái" />
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </div>
</asp:Content>