<%@ Page Title="Báo cáo & Thống kê" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeBehind="BaoCaoThongKe.aspx.cs" Inherits="N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.BaoCao.BaoCaoThongKe" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Báo cáo & Thống kê</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="card card-custom">
        <div class="card-header"><span>1. Báo cáo Học phí Còn nợ (Công nợ Học viên)</span></div>
        <div class="card-body p-0">
            <asp:GridView ID="gvCongNo" runat="server" CssClass="table table-hover table-custom mb-0" AutoGenerateColumns="false">
                <Columns>
                    <asp:BoundField DataField="madangky" HeaderText="Mã ĐK" />
                    <asp:BoundField DataField="hoten" HeaderText="Học viên" />
                    <asp:BoundField DataField="dienthoai" HeaderText="Điện thoại" />
                    <asp:BoundField DataField="tenkhoahoc" HeaderText="Khóa học" />
                    <asp:BoundField DataField="hocphi" HeaderText="Tổng học phí" DataFormatString="{0:N0} đ" />
                    <asp:BoundField DataField="sotiendong" HeaderText="Đã đóng" DataFormatString="{0:N0} đ" />
                    <asp:BoundField DataField="ConNo" HeaderText="Còn nợ" DataFormatString="{0:N0} đ" ItemStyle-CssClass="text-danger fw-bold" />
                </Columns>
            </asp:GridView>
        </div>
    </div>

    <div class="card card-custom">
        <div class="card-header"><span>2. Thống kê Số lượng Học viên & Doanh thu theo Khóa học</span></div>
        <div class="card-body p-0">
            <asp:GridView ID="gvTheoKhoa" runat="server" CssClass="table table-hover table-custom mb-0" AutoGenerateColumns="false">
                <Columns>
                    <asp:BoundField DataField="makhoahoc" HeaderText="Mã KH" />
                    <asp:BoundField DataField="tenkhoahoc" HeaderText="Tên khóa học" />
                    <asp:BoundField DataField="SoHV" HeaderText="Số lượng ĐK" />
                    <asp:BoundField DataField="TongThu" HeaderText="Tổng thực thu" DataFormatString="{0:N0} đ" />
                </Columns>
            </asp:GridView>
        </div>
    </div>
</asp:Content>