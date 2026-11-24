<%@ Page Title="Cấp lại Mật khẩu" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeBehind="CapLaiMatKhau.aspx.cs" Inherits="N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.TaiKhoan.CapLaiMatKhau" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Cấp lại Mật khẩu</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .pagination-ys { padding: 10px 15px; }
        .pagination-ys table > tbody > tr > td { display: inline-block; margin: 0 3px; }
        .pagination-ys table > tbody > tr > td > a,
        .pagination-ys table > tbody > tr > td > span {
            display: inline-flex; align-items: center; justify-content: center; min-width: 34px; height: 34px;
            padding: 0 10px; font-size: 0.9rem; font-weight: 600; text-decoration: none; border-radius: 6px;
            border: 1px solid #dee2e6; color: #1677D2; background-color: #ffffff; transition: all 0.2s;
        }
        .pagination-ys table > tbody > tr > td > span { background-color: #1677D2; color: #ffffff; border-color: #1677D2; }
        .pagination-ys table > tbody > tr > td > a:hover { background-color: #EAF4FF; border-color: #1677D2; }
    </style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnlThongBao" runat="server" Visible="false" ClientIDMode="Static"
        CssClass="alert alert-info fw-semibold alert-dismissible fade show shadow"
        style="position: fixed; top: 76px; right: 24px; z-index: 1060; min-width: 320px; max-width: 480px;">
        <asp:Literal ID="litThongBao" runat="server" />
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    </asp:Panel>

    <div class="d-flex justify-content-between align-items-center mb-3 flex-wrap gap-2">
        <h4 class="fw-bold mb-0 text-dark">
            <i class="bi bi-key-fill text-warning me-2"></i>Cấp lại Mật khẩu Giảng viên &amp; Học viên
        </h4>
        <div class="d-flex gap-2">
            <asp:TextBox ID="txtTimKiem" runat="server" CssClass="form-control form-control-sm" style="width: 260px;"
                placeholder="Tìm mã, họ tên, SĐT hoặc email..." autocomplete="off" />
            <asp:Button ID="btnTim" runat="server" Text="Tìm" CssClass="btn btn-sm btn-primary-gradient px-3" OnClick="btnTim_Click" />
            <asp:Button ID="btnLamMoi" runat="server" Text="Làm mới" CssClass="btn btn-sm btn-secondary" OnClick="btnLamMoi_Click" />
        </div>
    </div>

    <!-- CHUYỂN TAB GIẢNG VIÊN / HỌC VIÊN -->
    <ul class="nav nav-tabs mb-0 border-bottom-0">
        <li class="nav-item">
            <asp:LinkButton ID="btnTabGV" runat="server" CssClass="nav-link active fw-bold" OnClick="btnTabGV_Click">
                <i class="bi bi-person-badge-fill me-1"></i> Giảng viên
            </asp:LinkButton>
        </li>
        <li class="nav-item">
            <asp:LinkButton ID="btnTabHV" runat="server" CssClass="nav-link fw-bold" OnClick="btnTabHV_Click">
                <i class="bi bi-people-fill me-1"></i> Học viên
            </asp:LinkButton>
        </li>
    </ul>

    <div class="card card-custom rounded-top-0">
        <div class="card-body p-0">
            <!-- DANH SÁCH GIẢNG VIÊN -->
            <asp:Panel ID="pnlGiangVien" runat="server" Visible="true">
                <div class="table-responsive">
                    <asp:GridView ID="gvGiangVien" runat="server" CssClass="table table-hover table-custom mb-0" AutoGenerateColumns="false"
                        AllowPaging="true" PageSize="10" OnPageIndexChanging="gvGiangVien_PageIndexChanging"
                        DataKeyNames="magiangvien" EmptyDataText="Không tìm thấy giảng viên nào.">
                        <PagerSettings Mode="NumericFirstLast" FirstPageText="&laquo;" LastPageText="&raquo;" PreviousPageText="&lsaquo;" NextPageText="&rsaquo;" PageButtonCount="5" />
                        <PagerStyle CssClass="pagination-ys" HorizontalAlign="Center" />
                        <Columns>
                            <asp:BoundField DataField="magiangvien" HeaderText="Mã GV" ItemStyle-CssClass="fw-semibold text-nowrap" />
                            <asp:BoundField DataField="hoten" HeaderText="Họ và tên" />
                            <asp:BoundField DataField="dienthoai" HeaderText="Điện thoại (Tài khoản ĐN)" />
                            <asp:BoundField DataField="email" HeaderText="Email" />
                            <asp:BoundField DataField="chuyenmon" HeaderText="Chuyên môn" />
                            <asp:TemplateField HeaderText="Thao tác" ItemStyle-CssClass="text-center text-nowrap" HeaderStyle-CssClass="text-center">
                                <ItemTemplate>
                                    <button type="button" class="btn btn-sm btn-warning fw-semibold text-dark"
                                        onclick='<%# "moModalCapMK(\"GV\", \"" + HttpUtility.JavaScriptStringEncode(Convert.ToString(Eval("magiangvien"))) + "\", \"" + HttpUtility.JavaScriptStringEncode(Convert.ToString(Eval("hoten"))) + "\", \"" + HttpUtility.JavaScriptStringEncode(Convert.ToString(Eval("dienthoai"))) + "\"); return false;" %>'>
                                        <i class="bi bi-key me-1"></i> Cấp lại MK
                                    </button>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </asp:Panel>

            <!-- DANH SÁCH HỌC VIÊN -->
            <asp:Panel ID="pnlHocVien" runat="server" Visible="false">
                <div class="table-responsive">
                    <asp:GridView ID="gvHocVien" runat="server" CssClass="table table-hover table-custom mb-0" AutoGenerateColumns="false"
                        AllowPaging="true" PageSize="10" OnPageIndexChanging="gvHocVien_PageIndexChanging"
                        DataKeyNames="mahocvien" EmptyDataText="Không tìm thấy học viên nào.">
                        <PagerSettings Mode="NumericFirstLast" FirstPageText="&laquo;" LastPageText="&raquo;" PreviousPageText="&lsaquo;" NextPageText="&rsaquo;" PageButtonCount="5" />
                        <PagerStyle CssClass="pagination-ys" HorizontalAlign="Center" />
                        <Columns>
                            <asp:BoundField DataField="mahocvien" HeaderText="Mã HV" ItemStyle-CssClass="fw-semibold text-nowrap" />
                            <asp:BoundField DataField="hoten" HeaderText="Họ và tên" />
                            <asp:BoundField DataField="dienthoai" HeaderText="Điện thoại" />
                            <asp:BoundField DataField="email" HeaderText="Email" />
                            <asp:BoundField DataField="diachi" HeaderText="Địa chỉ" />
                            <asp:TemplateField HeaderText="Thao tác" ItemStyle-CssClass="text-center text-nowrap" HeaderStyle-CssClass="text-center">
                                <ItemTemplate>
                                    <button type="button" class="btn btn-sm btn-warning fw-semibold text-dark"
                                        onclick='<%# "moModalCapMK(\"HV\", \"" + HttpUtility.JavaScriptStringEncode(Convert.ToString(Eval("mahocvien"))) + "\", \"" + HttpUtility.JavaScriptStringEncode(Convert.ToString(Eval("hoten"))) + "\", \"" + HttpUtility.JavaScriptStringEncode(Convert.ToString(Eval("dienthoai"))) + "\"); return false;" %>'>
                                        <i class="bi bi-key me-1"></i> Cấp lại MK
                                    </button>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </asp:Panel>
        </div>
    </div>

    <!-- MODAL CẤP LẠI MẬT KHẨU -->
    <div class="modal fade" id="modalCapLaiMK" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content border-0 shadow rounded-4">
                <div class="modal-header bg-warning text-dark">
                    <h5 class="modal-title fw-bold">
                        <i class="bi bi-key-fill me-2"></i>Cấp lại Mật khẩu <span id="lblLoaiTK"></span>
                    </h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body p-4">
                    <div class="alert alert-light border small mb-3">
                        <div><strong>Họ và tên:</strong> <span id="lblHoTenTK"></span></div>
                        <div><strong>Mã tài khoản:</strong> <span id="lblMaTK" class="text-primary fw-semibold"></span></div>
                        <div><strong>Số điện thoại:</strong> <span id="lblSdtTK"></span></div>
                    </div>

                    <div class="mb-3">
                        <label class="form-label small fw-bold">Mật khẩu mới <span class="text-danger">*</span></label>
                        <div class="input-group">
                            <asp:TextBox ID="txtMKMoi" runat="server" ClientIDMode="Static" TextMode="Password"
                                CssClass="form-control" autocomplete="new-password" placeholder="Từ 6 đến 50 ký tự, không khoảng trắng..." />
                            <button type="button" class="btn btn-outline-secondary" tabindex="-1" onclick="toggleMKCapLai('txtMKMoi', this)">
                                <i class="bi bi-eye"></i>
                            </button>
                        </div>
                    </div>

                    <div class="mb-2">
                        <label class="form-label small fw-bold">Xác nhận mật khẩu mới <span class="text-danger">*</span></label>
                        <div class="input-group">
                            <asp:TextBox ID="txtMKXacNhan" runat="server" ClientIDMode="Static" TextMode="Password"
                                CssClass="form-control" autocomplete="new-password" placeholder="Nhập lại mật khẩu mới..." />
                            <button type="button" class="btn btn-outline-secondary" tabindex="-1" onclick="toggleMKCapLai('txtMKXacNhan', this)">
                                <i class="bi bi-eye"></i>
                            </button>
                        </div>
                    </div>
                </div>
                <div class="modal-footer bg-light justify-content-between">
                    <asp:Button ID="btnDatVeSDT" runat="server" Text="Đặt về SĐT mặc định"
                        CssClass="btn btn-outline-secondary btn-sm fw-semibold" OnClick="btnDatVeSDT_Click"
                        ToolTip="Đặt mật khẩu về SĐT và yêu cầu người dùng đổi lại khi đăng nhập" />
                    <div class="d-flex gap-2">
                        <button type="button" class="btn btn-secondary btn-sm px-3" data-bs-dismiss="modal">Hủy</button>
                        <asp:Button ID="btnLuuCapMK" runat="server" Text="Lưu mật khẩu mới"
                            CssClass="btn btn-warning btn-sm fw-bold text-dark px-3" OnClick="btnLuuCapMK_Click" />
                    </div>
                </div>
            </div>
        </div>
    </div>

    <asp:HiddenField ID="hfLoaiTK" runat="server" ClientIDMode="Static" />
    <asp:HiddenField ID="hfMaTK" runat="server" ClientIDMode="Static" />

    <script>
// @ts-nocheck
function moModalCapMK(loai, ma, hoTen, sdt) {
    document.getElementById('hfLoaiTK').value = loai;
    document.getElementById('hfMaTK').value = ma;
    document.getElementById('lblLoaiTK').innerText = (loai === 'GV') ? 'Giảng viên' : 'Học viên';
    document.getElementById('lblMaTK').innerText = ma;
    document.getElementById('lblHoTenTK').innerText = hoTen;
    document.getElementById('lblSdtTK').innerText = sdt || '(Chưa có)';

    ['txtMKMoi', 'txtMKXacNhan'].forEach(function (id) {
        var inp = document.getElementById(id);
        if (inp) { inp.value = ''; inp.type = 'password'; }
    });

    var el = document.getElementById('modalCapLaiMK');
    if (el && window.bootstrap) {
        window.bootstrap.Modal.getOrCreateInstance(el).show();
    }
}

function toggleMKCapLai(inputId, btn) {
    var inp = document.getElementById(inputId);
    if (!inp) return;
    var icon = btn.querySelector('i');
    if (inp.type === 'password') {
        inp.type = 'text';
        if (icon) { icon.classList.remove('bi-eye'); icon.classList.add('bi-eye-slash'); }
    } else {
        inp.type = 'password';
        if (icon) { icon.classList.remove('bi-eye-slash'); icon.classList.add('bi-eye'); }
    }
}

window.addEventListener('DOMContentLoaded', function () {
    setTimeout(function () {
        var tb = document.getElementById('pnlThongBao');
        if (tb && tb.style.display !== 'none') {
            tb.style.transition = 'opacity 0.5s ease';
            tb.style.opacity = '0';
            setTimeout(function () { if (tb) tb.style.display = 'none'; }, 500);
        }
    }, 5000);
});
</script>
</asp:Content>