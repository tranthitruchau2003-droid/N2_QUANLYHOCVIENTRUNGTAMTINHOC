<%@ Page Title="Quản lý Học viên" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeBehind="QuanLyHocVien.aspx.cs" Inherits="N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.HocVien.QuanLyHocVien" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Quản lý Học viên</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        /* CSS làm đẹp thanh phân trang GridView thành nút < 1 2 3 > */
        .pagination-ys {
            padding: 10px 15px;
        }
        .pagination-ys table > tbody > tr > td {
            display: inline-block;
            margin: 0 3px;
        }
        .pagination-ys table > tbody > tr > td > a,
        .pagination-ys table > tbody > tr > td > span {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            min-width: 34px;
            height: 34px;
            padding: 0 10px;
            font-size: 0.9rem;
            font-weight: 600;
            text-decoration: none;
            border-radius: 6px;
            border: 1px solid #dee2e6;
            color: #1677D2;
            background-color: #ffffff;
            transition: all 0.2s;
        }
        .pagination-ys table > tbody > tr > td > span {
            background-color: #1677D2;
            color: #ffffff;
            border-color: #1677D2;
        }
        .pagination-ys table > tbody > tr > td > a:hover {
            background-color: #EAF4FF;
            border-color: #1677D2;
        }
    </style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnlThongBao" runat="server" Visible="false" ClientIDMode="Static"
        CssClass="alert alert-danger text-danger fw-semibold alert-dismissible fade show shadow"
        style="position: fixed; top: 76px; right: 24px; z-index: 1060; min-width: 320px; max-width: 480px;">
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
                <div class="card-header d-flex justify-content-between align-items-center">
                    <span class="text-nowrap"><i class="bi bi-table me-2"></i>Danh sách Học viên</span>
                    <div class="d-flex gap-2 w-100 justify-content-end">
                        <asp:TextBox ID="txtTimKiem" runat="server" ClientIDMode="Static" CssClass="form-control form-control-sm" 
                            style="max-width: 250px;" list="dsGoiYHocVien" placeholder="Mã, tên hoặc SĐT..." 
                            autocomplete="off" onfocus="this.select();" />
                        <datalist id="dsGoiYHocVien">
                            <asp:Literal ID="litGoiY" runat="server" />
                        </datalist>
                        <asp:Button ID="btnTim" runat="server" ClientIDMode="Static" Text="Tìm" CssClass="btn btn-sm btn-primary-gradient" OnClick="btnTim_Click" />
                    </div>
                </div>
                <div class="card-body p-0">
                    <asp:GridView ID="gvHocVien" runat="server" CssClass="table table-hover table-custom mb-0" AutoGenerateColumns="false"
                        AllowPaging="true" PageSize="8" OnPageIndexChanging="gvHocVien_PageIndexChanging" OnRowCommand="gvHocVien_RowCommand" DataKeyNames="mahocvien">
                        <PagerSettings Mode="NumericFirstLast" FirstPageText="&laquo;" LastPageText="&raquo;" PreviousPageText="&lsaquo;" NextPageText="&rsaquo;" PageButtonCount="5" />
                        <PagerStyle CssClass="pagination-ys" HorizontalAlign="Center" />
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
                                    <button type="button" class="btn btn-sm btn-outline-danger" onclick="hienThiModalXoa('<%# Eval("mahocvien") %>'); return false;"><i class="bi bi-trash"></i></button>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </div>

    <!-- POPUP XÁC NHẬN XÓA HỌC VIÊN -->
    <div class="modal fade" id="modalXoaHocVien" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content border-0 shadow">
                <div class="modal-header bg-danger text-white">
                    <h5 class="modal-title fw-bold">
                        <i class="bi bi-exclamation-triangle me-2"></i>Xác nhận Xóa
                    </h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body py-4 text-center">
                    <i class="bi bi-trash display-4 text-danger mb-3 d-block"></i>
                    <p class="fs-5 mb-0">Bạn có chắc chắn muốn xóa học viên <strong id="maHocVienCanXoa"></strong> này?</p>
                </div>
                <div class="modal-footer justify-content-center border-0 pb-4">
                    <button type="button" class="btn btn-secondary px-4" data-bs-dismiss="modal">Hủy</button>
                    <asp:LinkButton ID="btnXoaChinhThuc" runat="server" CssClass="btn btn-danger px-4 d-none" OnClick="btnXoaChinhThuc_Click">Đồng ý</asp:LinkButton>
                    <button type="button" class="btn btn-danger px-4" onclick="document.getElementById('<%= btnXoaChinhThuc.ClientID %>').click();">
                        <i class="bi bi-check-lg me-1"></i> Đồng ý
                    </button>
                </div>
            </div>
        </div>
    </div>
    
    <asp:HiddenField ID="hfMaHocVienXoa" runat="server" ClientIDMode="Static" />

    <script>
       function hienThiModalXoa(maHocVien) {
    document.getElementById('hfMaHocVienXoa').value = maHocVien;
    document.getElementById('maHocVienCanXoa').innerText = maHocVien;
    var modalXoa = new window.bootstrap.Modal(document.getElementById('modalXoaHocVien'));
    modalXoa.show();
       }

        window.addEventListener('DOMContentLoaded', function () {
            // 1. Tự động tắt thông báo sau 5 giây
            setTimeout(function () {
                var tb = document.getElementById('pnlThongBao');
                if (tb && tb.style.display !== 'none') {
                    tb.style.transition = 'opacity 0.5s ease';
                    tb.style.opacity = '0';
                    setTimeout(function () {
                        if (tb) tb.style.display = 'none';
                    }, 500);
                }
            }, 5000);

            // 2. TỰ ĐỘNG TẢI LẠI DANH SÁCH KHI XÓA RỖNG Ô TÌM KIẾM
            var txtTim = document.getElementById('txtTimKiem');
            var btnTim = document.getElementById('btnTim');
            if (txtTim && btnTim) {
                txtTim.addEventListener('input', function () {
                    if (this.value.trim() === '') {
                        btnTim.click();
                    }
                });
            }
        });
    </script>
</asp:Content>