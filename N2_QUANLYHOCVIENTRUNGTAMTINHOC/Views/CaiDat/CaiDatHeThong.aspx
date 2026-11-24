<%@ Page Title="Cài đặt & Sao lưu Hệ thống" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeBehind="CaiDatHeThong.aspx.cs" Inherits="N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.CaiDat.CaiDatHeThong" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Cài đặt Hệ thống</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnlTB" runat="server" Visible="false" ClientIDMode="Static"
        CssClass="alert alert-info fw-semibold alert-dismissible fade show shadow"
        style="position: fixed; top: 76px; right: 24px; z-index: 1060; min-width: 320px; max-width: 480px;">
        <asp:Literal ID="litTB" runat="server" />
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    </asp:Panel>

    <div class="row g-4">
        <!-- KHUNG 1: SAO LƯU & PHỤC HỒI CSDL -->
        <div class="col-lg-5">
            <div class="card card-custom h-100">
                <div class="card-header"><span><i class="bi bi-database-fill-gear me-2"></i>Sao lưu & Phục hồi Cơ sở dữ liệu</span></div>
                <div class="card-body">
                    <div class="mb-3">
                        <label class="small fw-bold">Thư mục lưu trữ Backup trên Server</label>
                        <asp:TextBox ID="txtThuMucBackup" runat="server" CssClass="form-control form-control-sm" ReadOnly="true" />
                    </div>
                    <asp:Button ID="btnBackup" runat="server" Text="THỰC HIỆN SAO LƯU NGAY (.BAK)" CssClass="btn btn-primary-gradient w-100 fw-bold mb-4" OnClick="btnBackup_Click" />

                    <hr />
                    <h6 class="fw-bold text-danger mb-2"><i class="bi bi-exclamation-triangle-fill me-1"></i> Phục hồi Dữ liệu (Restore Database)</h6>
                    <div class="mb-3">
                        <label class="small fw-bold">Chọn bản sao lưu (.bak) để phục hồi</label>
                        <asp:DropDownList ID="ddlFileBak" runat="server" ClientIDMode="Static" CssClass="form-select" />
                    </div>
                    <asp:Button ID="btnRestore" runat="server" ClientIDMode="Static" Text="PHỤC HỒI CƠ SỞ DỮ LIỆU" CssClass="d-none" OnClick="btnRestore_Click" />
                    <button type="button" class="btn btn-orange w-100 fw-bold" onclick="kiemTraTruocKhiRestore();">
                        PHỤC HỒI CƠ SỞ DỮ LIỆU
                    </button>
                </div>
            </div>
        </div>

        <!-- KHUNG 2: NHẬT KÝ HỆ THỐNG (ĐÃ BỎ KHUNG ĐỔI MẬT KHẨU) -->
        <div class="col-lg-7">
            <div class="card card-custom h-100 d-flex flex-column">
                <div class="card-header d-flex justify-content-between align-items-center flex-wrap gap-2">
                    <span><i class="bi bi-clock-history me-2"></i>Nhật ký Hệ thống gần đây</span>
                    <asp:Button ID="btnXemThemNhatKy" runat="server" Text="Xem thêm (Cả tuần)" CssClass="btn btn-sm btn-outline-primary fw-semibold" OnClick="btnXemThemNhatKy_Click" />
                </div>
                <div class="card-body p-0 flex-grow-1">
                    <div class="table-responsive" style="max-height: 460px; overflow-y: auto;">
                        <asp:GridView ID="gvNhatKy" runat="server" CssClass="table table-sm table-hover table-custom mb-0" AutoGenerateColumns="false"
                            EmptyDataText="Chưa có nhật ký hoạt động nào.">
                            <Columns>
                                <asp:BoundField DataField="thoigian" HeaderText="Thời gian" DataFormatString="{0:dd/MM HH:mm}" ItemStyle-CssClass="text-nowrap" />
                                <asp:BoundField DataField="tendangnhap" HeaderText="Tài khoản" ItemStyle-CssClass="fw-semibold" />
                                <asp:BoundField DataField="hanhdong" HeaderText="Thao tác" />
                                <asp:BoundField DataField="noidung" HeaderText="Chi tiết" />
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="modal fade" id="modalXacNhanRestore" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content border-0 shadow">
                <div class="modal-header bg-danger text-white">
                    <h5 class="modal-title fw-bold"><i class="bi bi-exclamation-triangle-fill me-2"></i>Cảnh báo Phục hồi CSDL</h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body py-4 text-center">
                    <i class="bi bi-database-exclamation display-4 text-danger mb-3 d-block"></i>
                    <p class="fs-6 mb-0">Phục hồi từ file sao lưu sẽ <strong>ghi đè toàn bộ dữ liệu hiện tại</strong> và không thể hoàn tác.<br/>Bạn có chắc chắn muốn tiếp tục không?</p>
                </div>
                <div class="modal-footer justify-content-center border-0 pb-4">
                    <button type="button" class="btn btn-secondary px-4" data-bs-dismiss="modal">Hủy</button>
                    <button type="button" class="btn btn-danger px-4 fw-bold" onclick="document.getElementById('btnRestore').click();">
                        <i class="bi bi-check-lg me-1"></i> Đồng ý Phục hồi
                    </button>
                </div>
            </div>
        </div>
    </div>

    <script>
// @ts-nocheck
window.addEventListener('DOMContentLoaded', function () {
    setTimeout(function () {
        var tb = document.getElementById('pnlTB');
        if (tb && tb.style.display !== 'none') {
            tb.style.transition = 'opacity 0.5s ease';
            tb.style.opacity = '0';
            setTimeout(function () {
                if (tb) tb.style.display = 'none';
            }, 500);
        }
    }, 5000);
});
function hienThongBaoClient(msg) {
    var pnl = document.getElementById('pnlTB');
    if (!pnl) {
        pnl = document.createElement('div');
        pnl.id = 'pnlTB';
        pnl.style.cssText = 'position: fixed; top: 76px; right: 24px; z-index: 1060; min-width: 320px; max-width: 480px;';
        document.body.appendChild(pnl);
    }
    pnl.className = 'alert alert-danger text-danger fw-semibold alert-dismissible fade show shadow';
    pnl.style.display = 'block';
    pnl.style.opacity = '1';
    pnl.innerHTML = msg + '<button type="button" class="btn-close" data-bs-dismiss="alert"></button>';
}

function kiemTraTruocKhiRestore() {
    var ddl = document.getElementById('ddlFileBak');
    if (!ddl || ddl.options.length === 0 || !ddl.value) {
        hienThongBaoClient('Vui lòng chọn file sao lưu (.bak) trước khi phục hồi!');
        return;
    }
    var modalEl = document.getElementById('modalXacNhanRestore');
    if (modalEl && window.bootstrap) {
        window.bootstrap.Modal.getOrCreateInstance(modalEl).show();
    }
}
    </script>
</asp:Content>