<%@ Page Title="Quản lý Lớp học & Lịch học" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeBehind="QuanLyLopHoc.aspx.cs" Inherits="N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.LopHoc.QuanLyLopHoc" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Lớp học & Lịch học</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnlTB" runat="server" Visible="false" CssClass="alert alert-info"><asp:Literal ID="litTB" runat="server" /></asp:Panel>
    <div class="row g-4">
        <!-- BẢNG LOPHOC -->
        <div class="col-lg-6">
            <div class="card card-custom">
                <div class="card-header"><span>1. Quản lý Lớp học (LOPHOC)</span></div>
                <div class="card-body">
                    <div class="row g-2 mb-3">
                        <div class="col-4"><label class="small fw-bold">Mã lớp</label><asp:TextBox ID="txtMaLop" runat="server" CssClass="form-control" /></div>
                        <div class="col-8"><label class="small fw-bold">Tên lớp</label><asp:TextBox ID="txtTenLop" runat="server" CssClass="form-control" /></div>
                        <div class="col-6"><label class="small fw-bold">Khóa học</label><asp:DropDownList ID="ddlKhoaHoc" runat="server" CssClass="form-select" /></div>
                        <div class="col-6"><label class="small fw-bold">Giảng viên</label><asp:DropDownList ID="ddlGiangVien" runat="server" CssClass="form-select" /></div>
                        <div class="col-12"><label class="small fw-bold">Ngày khai giảng</label><asp:TextBox ID="txtNgayKG" runat="server" TextMode="Date" CssClass="form-control" /></div>
                    </div>
                    <asp:Button ID="btnThemLop" runat="server" Text="Tạo Lớp học" CssClass="btn btn-orange w-100 mb-3" OnClick="btnThemLop_Click" />

                    <asp:GridView ID="gvLop" runat="server" CssClass="table table-sm table-bordered table-custom" AutoGenerateColumns="false">
                        <Columns>
                            <asp:BoundField DataField="malop" HeaderText="Mã lớp" />
                            <asp:BoundField DataField="tenlop" HeaderText="Tên lớp" />
                            <asp:BoundField DataField="tenkhoahoc" HeaderText="Khóa học" />
                            <asp:BoundField DataField="hoten" HeaderText="Giảng viên" />
                            <asp:BoundField DataField="ngaykhaigiang" HeaderText="Khai giảng" DataFormatString="{0:dd/MM/yyyy}" />
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>

        <!-- BẢNG LICHHOC -->
        <div class="col-lg-6">
            <div class="card card-custom">
                <div class="card-header"><span>2. Phân bổ Lịch học (LICHHOC)</span></div>
                <div class="card-body">
                    <div class="row g-2 mb-3">
                        <div class="col-4"><label class="small fw-bold">Mã lịch</label><asp:TextBox ID="txtMaLich" runat="server" CssClass="form-control" /></div>
                        <div class="col-8"><label class="small fw-bold">Thuộc Lớp học</label><asp:DropDownList ID="ddlLopLich" runat="server" CssClass="form-select" /></div>
                        <div class="col-6"><label class="small fw-bold">Ca học</label><asp:TextBox ID="txtCaHoc" runat="server" CssClass="form-control" placeholder="Ca tối 1" /></div>
                        <div class="col-6"><label class="small fw-bold">Phòng học</label><asp:TextBox ID="txtPhongHoc" runat="server" CssClass="form-control" placeholder="Lab 01" /></div>
                        <div class="col-6"><label class="small fw-bold">Thứ học</label><asp:TextBox ID="txtThu" runat="server" CssClass="form-control" placeholder="Thứ 2-4-6" /></div>
                        <div class="col-6"><label class="small fw-bold">Giờ học</label><asp:TextBox ID="txtGioHoc" runat="server" CssClass="form-control" placeholder="18:00 - 20:30" /></div>
                    </div>
                    <asp:Button ID="btnThemLich" runat="server" Text="Thêm Lịch học" CssClass="btn btn-primary-gradient w-100 mb-3" OnClick="btnThemLich_Click" />

                    <asp:GridView ID="gvLich" runat="server" CssClass="table table-sm table-bordered table-custom" AutoGenerateColumns="false">
                        <Columns>
                            <asp:BoundField DataField="malichhoc" HeaderText="Mã lịch" />
                            <asp:BoundField DataField="tenlop" HeaderText="Lớp" />
                            <asp:BoundField DataField="phonghoc" HeaderText="Phòng" />
                            <asp:BoundField DataField="thu" HeaderText="Thứ" />
                            <asp:BoundField DataField="giohoc" HeaderText="Giờ học" />
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </div>
</asp:Content>