<%@ Page Title="Quản lý Lớp học & Điểm danh" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeBehind="QuanLyLopHoc.aspx.cs" Inherits="N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.LopHoc.QuanLyLopHoc" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Lớp học & Lịch học</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnlTB" runat="server" Visible="false" ClientIDMode="Static"
        CssClass="alert alert-info fw-semibold alert-dismissible fade show shadow"
        style="position: fixed; top: 76px; right: 24px; z-index: 1060; min-width: 320px; max-width: 480px;">
        <asp:Literal ID="litTB" runat="server" />
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    </asp:Panel>

    <div class="row g-4">
        <!-- FORM QUẢN LÝ LỚP (BỊ ẨN NẾU LÀ GIẢNG VIÊN) -->
        <asp:Panel ID="pnlFormAdmin" runat="server" CssClass="col-lg-4">
            <div class="card card-custom">
                <div class="card-header"><span><i class="bi bi-easel2-fill me-2"></i>Mở Lớp Học phần & Phân công</span></div>
                <div class="card-body">
                    <div class="mb-2"><label class="small fw-bold">Mã Lớp (*)</label><asp:TextBox ID="txtMaLop" runat="server" CssClass="form-control" placeholder="VD: L01" /></div>
                    <div class="mb-2"><label class="small fw-bold">Tên Lớp (*)</label><asp:TextBox ID="txtTenLop" runat="server" CssClass="form-control" placeholder="VD: Lớp Tin học Căn bản tối 246" /></div>
                    <div class="mb-2"><label class="small fw-bold">Thuộc Khóa học (*)</label><asp:DropDownList ID="ddlKhoaHoc" runat="server" CssClass="form-select" /></div>
                    <div class="mb-2"><label class="small fw-bold">Giảng viên phụ trách</label><asp:DropDownList ID="ddlGiangVien" runat="server" CssClass="form-select" /></div>
                    <div class="mb-3"><label class="small fw-bold">Ngày khai giảng dự kiến</label><asp:TextBox ID="txtKhaiGiang" runat="server" TextMode="Date" CssClass="form-control" /></div>
                    <div class="d-flex gap-2">
                        <asp:Button ID="btnThemLop" runat="server" Text="Thêm mới" CssClass="btn btn-orange flex-grow-1" OnClick="btnThemLop_Click" />
                        <asp:Button ID="btnSuaLop" runat="server" Text="Cập nhật" CssClass="btn btn-primary-gradient flex-grow-1" OnClick="btnSuaLop_Click" />
                    </div>
                </div>
            </div>
        </asp:Panel>

        <!-- DANH SÁCH LỚP HỌC (HIỂN THỊ CHUNG CHO CẢ ADMIN VÀ GIẢNG VIÊN) -->
        <div class="col-lg-8" id="colDanhSachLop" runat="server">
            <div class="card card-custom">
                <div class="card-header d-flex justify-content-between align-items-center">
                    <span class="text-nowrap"><i class="bi bi-list-task me-2"></i>Danh sách Lớp học phần</span>
                    <div class="d-flex gap-2">
                        <asp:TextBox ID="txtTimLop" runat="server" CssClass="form-control form-control-sm" placeholder="Tìm tên lớp, khóa..." />
                        <asp:Button ID="btnTimLop" runat="server" Text="Tìm" CssClass="btn btn-sm btn-primary-gradient" OnClick="btnTimLop_Click" />
                    </div>
                </div>
                <div class="card-body p-0">
                    <asp:GridView ID="gvLopHoc" runat="server" CssClass="table table-sm table-hover table-custom mb-0"
                        AutoGenerateColumns="false" DataKeyNames="malop" OnRowCommand="gvLopHoc_RowCommand">
                        <Columns>
                            <asp:BoundField DataField="malop" HeaderText="Mã Lớp" />
                            <asp:BoundField DataField="tenlop" HeaderText="Tên Lớp" ItemStyle-CssClass="fw-bold" />
                            <asp:BoundField DataField="tenkhoahoc" HeaderText="Khóa học" />
                            <asp:BoundField DataField="hotengv" HeaderText="Giảng viên" />
                            <asp:BoundField DataField="siso" HeaderText="Sĩ số (10)" ItemStyle-CssClass="text-center fw-semibold text-success" />
                            <asp:TemplateField HeaderText="Thao tác (Admin / GV)">
                                <ItemTemplate>
                                    <div class="d-flex gap-1">
                                        <!-- Nút dành cho Admin -->
                                        <asp:LinkButton ID="btnChon" runat="server" CommandName="ChonLop" CommandArgument='<%# Eval("malop") %>'
    Visible='<%# Session["Quyen"] != null && Session["Quyen"].ToString() == "Admin" %>' 
    CssClass="btn btn-sm btn-outline-primary" title="Sửa Lớp"><i class="bi bi-pencil"></i></asp:LinkButton>
<asp:LinkButton ID="btnXoa" runat="server" CommandName="XoaLop" CommandArgument='<%# Eval("malop") %>'
    Visible='<%# Session["Quyen"] != null && Session["Quyen"].ToString() == "Admin" %>' 
    CssClass="btn btn-sm btn-outline-danger" title="Xóa Lớp" OnClientClick="return confirm('Bạn có chắc muốn xóa lớp này?');"><i class="bi bi-trash"></i></asp:LinkButton>
                                        
                                        <!-- Nút Điểm danh dành cho Giảng viên & Admin -->
                                        <asp:LinkButton ID="btnDiemDanh" runat="server" CommandName="DiemDanh" CommandArgument='<%# Eval("malop") + "|" + Eval("tenlop") %>'
                                            CssClass="btn btn-sm btn-success fw-semibold"><i class="bi bi-person-check-fill me-1"></i> Điểm danh & Nhập điểm</asp:LinkButton>
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </div>

    <!-- POPUP ĐIỂM DANH CHUYÊN CẦN THEO LỚP (DÀNH CHO GIẢNG VIÊN) -->
    <div class="modal fade" id="modalDiemDanh" tabindex="-1" aria-hidden="true" data-bs-backdrop="static">
        <div class="modal-dialog modal-xl modal-dialog-centered">
            <div class="modal-content border-0 shadow">
                <div class="modal-header bg-success text-white">
                    <h5 class="modal-title fw-bold"><i class="bi bi-person-check me-2"></i>Điểm danh Lớp: <asp:Literal ID="litTenLopDiemDanh" runat="server" /></h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body p-0">
                    <div class="bg-light p-3 border-bottom d-flex align-items-center gap-3">
                        <label class="fw-bold text-nowrap mb-0">Ngày điểm danh:</label>
                        <asp:TextBox ID="txtNgayDiemDanh" runat="server" TextMode="Date" CssClass="form-control form-control-sm" style="width: 150px;" />
                        <asp:HiddenField ID="hfMaLopDiemDanh" runat="server" />
                    </div>
                    <div class="table-responsive" style="max-height: 400px;">
                        <asp:GridView ID="gvHocVienLop" runat="server" CssClass="table table-bordered table-hover mb-0" AutoGenerateColumns="false" DataKeyNames="mahocvien">
                            <Columns>
                                <asp:BoundField DataField="mahocvien" HeaderText="Mã HV" ItemStyle-Width="100" />
                                <asp:BoundField DataField="hoten" HeaderText="Họ tên Học viên" ItemStyle-CssClass="fw-semibold" />
                                <asp:TemplateField HeaderText="Trạng thái Điểm danh (Chuyên cần)" ItemStyle-Width="350">
                                    <ItemTemplate>
                                        <asp:RadioButtonList ID="rblTrangThai" runat="server" RepeatDirection="Horizontal" CssClass="d-flex gap-3 small mt-1 mb-0">
                                            <asp:ListItem Text="Có mặt" Value="Có mặt" Selected="True" />
                                            <asp:ListItem Text="Vắng phép" Value="Vắng phép" />
                                            <asp:ListItem Text="Vắng không phép" Value="Vắng không phép" />
                                        </asp:RadioButtonList>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Ghi chú">
                                    <ItemTemplate>
                                        <asp:TextBox ID="txtGhiChuDD" runat="server" CssClass="form-control form-control-sm" placeholder="..." />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>
                    <div class="alert alert-warning m-3 py-2 small">
                        <i class="bi bi-info-circle-fill me-1"></i> Điểm danh chuyên cần sẽ được cộng dồn và tính vào 10% điểm Trung bình chung (ĐTB) của học viên (nhập điểm Quá trình và Cuối kỳ ở tab Kỳ thi).
                    </div>
                </div>
                <div class="modal-footer bg-light">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Đóng</button>
                    <asp:Button ID="btnLuuDiemDanh" runat="server" Text="Lưu kết quả Điểm danh" CssClass="btn btn-success fw-bold" OnClick="btnLuuDiemDanh_Click" />
                </div>
            </div>
        </div>
    </div>

    <script>
function showModalDiemDanh() {
    var myModal = new bootstrap.Modal(document.getElementById('modalDiemDanh'));
    myModal.show();
}
window.addEventListener('DOMContentLoaded', function () {
    setTimeout(function () {
        var tb = document.getElementById('pnlTB');
        if (tb && tb.style.display !== 'none') {
            tb.style.transition = 'opacity 0.5s ease'; tb.style.opacity = '0';
            setTimeout(function () { if (tb) tb.style.display = 'none'; }, 500);
        }
    }, 5000);
});
</script>
</asp:Content>