<%@ Page Title="Quản lý Giảng viên" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeBehind="QuanLyGiangVien.aspx.cs" Inherits="N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.GiangVien.QuanLyGiangVien" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Quản lý Giảng viên</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnlTB" runat="server" Visible="false" CssClass="alert alert-info"><asp:Literal ID="litTB" runat="server" /></asp:Panel>
    <div class="row g-4">
        <div class="col-lg-4">
            <div class="card card-custom">
                <div class="card-header"><span>Thông tin Giảng viên</span></div>
                <div class="card-body">
                    <div class="mb-2"><label class="small fw-bold">Mã GV</label><asp:TextBox ID="txtMaGV" runat="server" CssClass="form-control" /></div>
                    <div class="mb-2"><label class="small fw-bold">Họ tên</label><asp:TextBox ID="txtHoTen" runat="server" CssClass="form-control" /></div>
                    <div class="mb-2"><label class="small fw-bold">Điện thoại</label><asp:TextBox ID="txtSDT" runat="server" CssClass="form-control" /></div>
                    <div class="mb-2"><label class="small fw-bold">Email</label><asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" /></div>
                    <div class="mb-2"><label class="small fw-bold">Chuyên môn</label><asp:TextBox ID="txtChuyenMon" runat="server" CssClass="form-control" /></div>
                    <div class="mb-3"><label class="small fw-bold">Học vị</label><asp:TextBox ID="txtHocVi" runat="server" CssClass="form-control" placeholder="Thạc sĩ, Kỹ sư..." /></div>
                    <div class="d-flex gap-2">
                        <asp:Button ID="btnThem" runat="server" Text="Thêm" CssClass="btn btn-orange flex-grow-1" OnClick="btnThem_Click" />
                        <asp:Button ID="btnSua" runat="server" Text="Cập nhật" CssClass="btn btn-primary-gradient flex-grow-1" OnClick="btnSua_Click" />
                    </div>
                </div>
            </div>
        </div>
        <div class="col-lg-8">
            <div class="card card-custom">
                <div class="card-header"><span>Danh sách Giảng viên & Lớp phụ trách</span></div>
                <div class="card-body p-0">
                    <asp:GridView ID="gvGV" runat="server" CssClass="table table-hover table-custom mb-0" AutoGenerateColumns="false" OnRowCommand="gvGV_RowCommand">
                        <Columns>
                            <asp:BoundField DataField="magiangvien" HeaderText="Mã GV" />
                            <asp:BoundField DataField="hoten" HeaderText="Họ tên" />
                            <asp:BoundField DataField="dienthoai" HeaderText="Điện thoại" />
                            <asp:BoundField DataField="chuyenmon" HeaderText="Chuyên môn" />
                            <asp:BoundField DataField="hocvi" HeaderText="Học vị" />
                            <asp:BoundField DataField="SoLop" HeaderText="Số lớp dạy" />
                            <asp:TemplateField>
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnChon" runat="server" CommandName="Chon" CommandArgument='<%# Eval("magiangvien") %>' CssClass="btn btn-sm btn-outline-primary"><i class="bi bi-pencil"></i></asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </div>
</asp:Content>