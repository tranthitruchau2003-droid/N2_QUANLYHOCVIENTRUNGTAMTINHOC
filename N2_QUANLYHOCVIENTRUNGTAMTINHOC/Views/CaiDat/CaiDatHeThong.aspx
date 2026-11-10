<%@ Page Title="Cài đặt Hệ thống & Sao lưu" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeBehind="CaiDatHeThong.aspx.cs" Inherits="N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.CaiDat.CaiDatHeThong" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Cài đặt & Sao lưu Dữ liệu</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnlTB" runat="server" Visible="false" CssClass="alert alert-warning fw-semibold">
        <asp:Literal ID="litTB" runat="server" />
    </asp:Panel>

    <div class="row g-4">
        <!-- SAO LƯU & PHỤC HỒI DATABASE -->
        <div class="col-lg-6">
            <div class="card card-custom">
                <div class="card-header"><span><i class="bi bi-database-fill-gear me-2"></i>Sao lưu & Phục hồi Cơ sở dữ liệu (QuanLyDaoTao)</span></div>
                <div class="card-body">
                    <div class="mb-3">
                        <label class="small fw-bold">Thư mục lưu trữ Backup trên Server</label>
                        <asp:TextBox ID="txtThuMucBackup" runat="server" CssClass="form-control" />
                    </div>
                    <asp:Button ID="btnBackup" runat="server" Text="THỰC HIỆN SAO LƯU NGAY (.BAK)" CssClass="btn btn-primary-gradient w-100 mb-4" OnClick="btnBackup_Click" />

                    <hr />
                    <h6 class="fw-bold text-danger"><i class="bi bi-exclamation-triangle-fill me-1"></i>Phục hồi Dữ liệu (Restore Database)</h6>
                    <div class="mb-3">
                        <label class="small fw-bold">Chọn bản sao lưu (.bak) để phục hồi</label>
                        <asp:DropDownList ID="ddlFileBackup" runat="server" CssClass="form-select" />
                    </div>
                    <asp:Button ID="btnRestore" runat="server" Text="PHỤC HỒI CƠ SỞ DỮ LIỆU" CssClass="btn btn-orange w-100"
                        OnClientClick="return confirm('CẢNH BÁO: Phục hồi dữ liệu sẽ ghi đè dữ liệu hiện tại bằng bản sao lưu đã chọn. Bạn chắc chắn muốn tiếp tục?');"
                        OnClick="btnRestore_Click" />
                </div>
            </div>
        </div>

        <!-- ĐỔI MẬT KHẨU & CẤU HÌNH -->
        <div class="col-lg-6" id="doimatkhau">
            <div class="card card-custom">
                <div class="card-header"><span><i class="bi bi-shield-lock-fill me-2"></i>Đổi mật khẩu Tài khoản</span></div>
                <div class="card-body">
                    <div class="mb-2"><label class="small fw-bold">Mật khẩu hiện tại</label><asp:TextBox ID="txtMKCu" runat="server" TextMode="Password" CssClass="form-control" /></div>
                    <div class="mb-3"><label class="small fw-bold">Mật khẩu mới</label><asp:TextBox ID="txtMKMoi" runat="server" TextMode="Password" CssClass="form-control" /></div>
                    <asp:Button ID="btnDoiMK" runat="server" Text="Cập nhật Mật khẩu" CssClass="btn btn-primary-gradient w-100" OnClick="btnDoiMK_Click" />
                </div>
            </div>

            <div class="card card-custom">
                <div class="card-header"><span><i class="bi bi-clock-history me-2"></i>Nhật ký Hệ thống gần đây</span></div>
                <div class="card-body p-0">
                    <asp:GridView ID="gvNhatKy" runat="server" CssClass="table table-sm table-striped mb-0" AutoGenerateColumns="false">
                        <Columns>
                            <asp:BoundField DataField="thoigian" HeaderText="Thời gian" DataFormatString="{0:dd/MM HH:mm}" />
                            <asp:BoundField DataField="tendangnhap" HeaderText="Tài khoản" />
                            <asp:BoundField DataField="hanhdong" HeaderText="Thao tác" />
                            <asp:BoundField DataField="noidung" HeaderText="Chi tiết" />
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </div>
</asp:Content>