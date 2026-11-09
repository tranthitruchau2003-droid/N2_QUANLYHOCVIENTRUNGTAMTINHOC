<%@ Page Title="Quản lý Học viên" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeBehind="QuanLyHocVien.aspx.cs" Inherits="N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.HocVien.QuanLyHocVien" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Quản lý Học viên</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnlThongBao" runat="server" Visible="false" CssClass="alert alert-info alert-dismissible fade show">
        <asp:Literal ID="litThongBao" runat="server" />
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    </asp:Panel>

    <div class="row g-4">
        <!-- FORM NHẬP LIỆU -->
        <div class="col-lg-4">
            <div class="card card-custom">
                <div class="card-header"><span><i class="bi bi-person-lines-fill me-2"></i>Thông tin Học viên</span></div>
                <div class="card-body">
                    <div class="mb-2">
                        <label class="form-label small fw-bold">Mã học viên (*)</label>
                        <asp:TextBox ID="txtMaHV" runat="server" CssClass="form-control" placeholder="VD: HV02" />
                    </div>
                    <div class="mb-2">
                        <label class="form-label small fw-bold">Họ và tên (*)</label>
                        <asp:TextBox ID="txtHoTen" runat="server" CssClass="form-control" />
                    </div>
                    <div class="row mb-2">
                        <div class="col-6">
                            <label class="form-label small fw-bold">Ngày sinh</label>
                            <asp:TextBox ID="txtNgaySinh" runat="server" TextMode="Date" CssClass="form-control" />
                        </div>
                        <div class="col-6">
                            <label class="form-label small fw-bold">Giới tính</label>
                            <asp:DropDownList ID="ddlGioiTinh" runat="server" CssClass="form-select">
                                <asp:ListItem Text="Nam" Value="Nam" />
                                <asp:ListItem Text="Nữ" Value="Nữ" />
                            </asp:DropDownList>
                        </div>
                    </div>
                    <div class="mb-2">
                        <label class="form-label small fw-bold">Số điện thoại</label>
                        <asp:TextBox ID="txtDienThoai" runat="server" CssClass="form-control" />
                    </div>
                    <div class="mb-2">
                        <label class="form-label small fw-bold">Email</label>
                        <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" />
                    </div>
                    <div class="mb-3">
                        <label class="form-label small fw-bold">Địa chỉ</label>
                        <asp:TextBox ID="txtDiaChi" runat="server" TextMode="MultiLine" Rows="2" CssClass="form-control" />
                    </div>
                    <div class="d-flex gap-2">
                        <asp:Button ID="btnThem" runat="server" Text="Thêm mới" CssClass="btn btn-orange flex-grow-1" OnClick="btnThem_Click" />
                        <asp:Button ID="btnCapNhat" runat="server" Text="Cập nhật" CssClass="btn btn-primary-gradient flex-grow-1" OnClick="btnCapNhat_Click" />
                        <asp:Button ID="btnLamMoi" runat="server" Text="Làm mới" CssClass="btn btn-secondary" OnClick="btnLamMoi_Click" />
                    </div>
                </div>
            </div>
        </div>

        <!-- DANH SÁCH & TÌM KIẾM -->
        <div class="col-lg-8">
            <div class="card card-custom">
                <div class="card-header">
                    <span><i class="bi bi-table me-2"></i>Danh sách Học viên</span>
                    <div class="d-flex gap-2">
                        <asp:TextBox ID="txtTimKiem" runat="server" CssClass="form-control form-control-sm" placeholder="Mã, tên hoặc SĐT..." />
                        <asp:Button ID="btnTim" runat="server" Text="Tìm" CssClass="btn btn-sm btn-primary-gradient" OnClick="btnTim_Click" />
                    </div>
                </div>
                <div class="card-body p-0">
                    <asp:GridView ID="gvHocVien" runat="server" CssClass="table table-hover table-custom mb-0" AutoGenerateColumns="false"
                        AllowPaging="true" PageSize="8" OnPageIndexChanging="gvHocVien_PageIndexChanging" OnRowCommand="gvHocVien_RowCommand">
                        <Columns>
                            <asp:BoundField DataField="mahocvien" HeaderText="Mã HV" />
                            <asp:BoundField DataField="hoten" HeaderText="Họ tên" />
                            <asp:BoundField DataField="ngaysinh" HeaderText="Ngày sinh" DataFormatString="{0:dd/MM/yyyy}" />
                            <asp:BoundField DataField="gioitinh" HeaderText="Phái" />
                            <asp:BoundField DataField="dienthoai" HeaderText="Điện thoại" />
                            <asp:BoundField DataField="diachi" HeaderText="Địa chỉ" />
                            <asp:TemplateField HeaderText="Thao tác">
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnChon" runat="server" CommandName="Chon" CommandArgument='<%# Eval("mahocvien") %>' CssClass="btn btn-sm btn-outline-primary"><i class="bi bi-pencil"></i></asp:LinkButton>
                                    <asp:LinkButton ID="btnXoa" runat="server" CommandName="Xoa" CommandArgument='<%# Eval("mahocvien") %>' CssClass="btn btn-sm btn-outline-danger" OnClientClick="return confirm('Xác nhận xóa học viên này?');"><i class="bi bi-trash"></i></asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </div>
</asp:Content>