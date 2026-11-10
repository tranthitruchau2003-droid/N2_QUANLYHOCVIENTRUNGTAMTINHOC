<%@ Page Title="Quản lý Lớp học & Thời khóa biểu" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeBehind="QuanLyLopHoc.aspx.cs" Inherits="N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.LopHoc.QuanLyLopHoc" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Lớp học & Thời khóa biểu</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnlTB" runat="server" Visible="false" ClientIDMode="Static"
        CssClass="alert alert-info fw-semibold alert-dismissible fade show shadow"
        style="position: fixed; top: 76px; right: 24px; z-index: 1060; min-width: 320px; max-width: 520px;">
        <asp:Literal ID="litTB" runat="server" />
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    </asp:Panel>

    <!-- BẢNG THỜI KHÓA BIỂU DẠNG LƯỚI THEO THỨ & CA HỌC (HIỂN THỊ CHO HỌC VIÊN & GIẢNG VIÊN) -->
    <asp:Panel ID="pnlLuoiTKB" runat="server" CssClass="card card-custom mb-4">
        <div class="card-header d-flex justify-content-between align-items-center">
            <span><i class="bi bi-calendar3-week-fill text-primary me-2"></i>Bảng Thời Khóa Biểu Trong Tuần (Tự động kiểm tra trùng lịch)</span>
            <span class="badge bg-info-subtle text-primary">Lịch học cập nhật trực tiếp từ Phòng Đào tạo</span>
        </div>
        <div class="card-body p-0">
            <div class="table-responsive">
                <asp:Literal ID="litBangTKB" runat="server" />
            </div>
        </div>
    </asp:Panel>

    <div class="row g-4">
        <!-- FORM QUẢN LÝ LỚP & ĐỔI LỊCH HỌC (CHỈ ADMIN / NHÂN VIÊN THẤY) -->
        <asp:Panel ID="pnlFormAdmin" runat="server" CssClass="col-lg-4">
            <div class="card card-custom">
                <div class="card-header"><span><i class="bi bi-easel2-fill me-2"></i>Mở Lớp & Cập nhật Lịch học (TKB)</span></div>
                <div class="card-body">
                    <div class="mb-2"><label class="small fw-bold">Mã Lớp (*)</label><asp:TextBox ID="txtMaLop" runat="server" CssClass="form-control form-control-sm" placeholder="VD: LH12" /></div>
                    <div class="mb-2"><label class="small fw-bold">Tên Lớp (*)</label><asp:TextBox ID="txtTenLop" runat="server" CssClass="form-control form-control-sm" placeholder="VD: Lớp Web K12 - Tối 2-4-6" /></div>
                    <div class="mb-2"><label class="small fw-bold">Thuộc Khóa học (*)</label><asp:DropDownList ID="ddlKhoaHoc" runat="server" CssClass="form-select form-select-sm" /></div>
                    <div class="mb-2"><label class="small fw-bold">Giảng viên phụ trách</label><asp:DropDownList ID="ddlGiangVien" runat="server" CssClass="form-select form-select-sm" /></div>
                    <div class="mb-2"><label class="small fw-bold">Ngày khai giảng</label><asp:TextBox ID="txtKhaiGiang" runat="server" TextMode="Date" CssClass="form-control form-control-sm" /></div>
                    
                    <hr class="my-2" />
                    <h6 class="small fw-bold text-primary mb-2"><i class="bi bi-clock-history me-1"></i>Thiết lập / Đổi Lịch học cho Lớp:</h6>
                    <div class="row g-2 mb-2">
                        <div class="col-6">
                            <label class="small fw-bold">Ca học</label>
                            <asp:DropDownList ID="ddlCaHoc" runat="server" CssClass="form-select form-select-sm">
                                <asp:ListItem Text="Ca sáng" Value="Ca sáng" />
                                <asp:ListItem Text="Ca chiều" Value="Ca chiều" />
                                <asp:ListItem Text="Ca tối" Value="Ca tối" Selected="True" />
                            </asp:DropDownList>
                        </div>
                        <div class="col-6">
                            <label class="small fw-bold">Các thứ trong tuần</label>
                            <asp:DropDownList ID="ddlThuHoc" runat="server" CssClass="form-select form-select-sm">
                                <asp:ListItem Text="Thứ 2, 4, 6" Value="Thứ 2, 4, 6" />
                                <asp:ListItem Text="Thứ 3, 5, 7" Value="Thứ 3, 5, 7" />
                                <asp:ListItem Text="Thứ 7, CN" Value="Thứ 7, CN" />
                            </asp:DropDownList>
                        </div>
                        <div class="col-6">
                            <label class="small fw-bold">Khung giờ học</label>
                            <asp:TextBox ID="txtGioHoc" runat="server" CssClass="form-control form-control-sm" Text="18:00 - 20:30" />
                        </div>
                        <div class="col-6">
                            <label class="small fw-bold">Phòng học</label>
                            <asp:TextBox ID="txtPhongHoc" runat="server" CssClass="form-control form-control-sm" Text="Phòng Lab 01" />
                        </div>
                    </div>

                    <div class="d-flex gap-2 mt-3">
                        <asp:Button ID="btnThemLop" runat="server" Text="Thêm Lớp & Lịch" CssClass="btn btn-sm btn-orange flex-grow-1" OnClick="btnThemLop_Click" />
                        <asp:Button ID="btnSuaLop" runat="server" Text="Cập nhật Lớp & Đổi Lịch" CssClass="btn btn-sm btn-primary-gradient flex-grow-1" OnClick="btnSuaLop_Click" />
                    </div>
                </div>
            </div>
        </asp:Panel>

        <!-- DANH SÁCH LỚP & CHI TIẾT THỜI KHÓA BIỂU -->
        <div class="col-lg-8" id="colDanhSachLop" runat="server">
            <div class="card card-custom">
                <div class="card-header d-flex justify-content-between align-items-center">
                    <span class="text-nowrap"><i class="bi bi-list-task me-2"></i><asp:Literal ID="litTieuDeBangLop" runat="server" Text="Danh sách Lớp học phần & Thời khóa biểu" /></span>
                    <asp:Panel ID="pnlTimKiemLop" runat="server" CssClass="d-flex gap-2">
                        <asp:TextBox ID="txtTimLop" runat="server" CssClass="form-control form-control-sm" autocomplete="off" placeholder="Tìm tên lớp, khóa..." />
                        <asp:Button ID="btnTimLop" runat="server" Text="Tìm" CssClass="btn btn-sm btn-primary-gradient" OnClick="btnTimLop_Click" />
                    </asp:Panel>
                </div>
                <div class="card-body p-0">
                    <asp:GridView ID="gvLopHoc" runat="server" CssClass="table table-sm table-hover table-custom mb-0"
                        AutoGenerateColumns="false" DataKeyNames="malop" OnRowCommand="gvLopHoc_RowCommand"
                        EmptyDataText="Bạn chưa đăng ký lớp học nào.">
                        <Columns>
                            <asp:BoundField DataField="malop" HeaderText="Mã Lớp" />
                            <asp:BoundField DataField="tenlop" HeaderText="Tên Lớp" ItemStyle-CssClass="fw-bold text-primary" />
                            <asp:BoundField DataField="tenkhoahoc" HeaderText="Khóa học" />
                            <asp:BoundField DataField="hotengv" HeaderText="Giảng viên" />
                            <asp:BoundField DataField="thoikhoabieu" HeaderText="Thời khóa biểu (Ca - Thứ - Phòng)" />
                            <asp:BoundField DataField="ngaykhaigiang" HeaderText="Khai giảng" DataFormatString="{0:dd/MM/yyyy}" />
                            <asp:BoundField DataField="siso" HeaderText="Sĩ số" ItemStyle-CssClass="text-center fw-semibold text-success" />
                            <asp:TemplateField HeaderText="Thao tác">
                                <ItemTemplate>
                                    <div class="d-flex gap-1">
                                        <asp:LinkButton ID="btnChon" runat="server" CommandName="ChonLop" CommandArgument='<%# Eval("malop") %>'
                                            Visible='<%# Session["Quyen"] != null && (Session["Quyen"].ToString() == "Admin" || Session["Quyen"].ToString() == "NhanVien") %>'
                                            CssClass="btn btn-sm btn-outline-primary" title="Sửa Lớp & Đổi Lịch học"><i class="bi bi-pencil"></i></asp:LinkButton>
                                        <asp:LinkButton ID="btnXoa" runat="server" CommandName="XoaLop" CommandArgument='<%# Eval("malop") %>'
                                            Visible='<%# Session["Quyen"] != null && Session["Quyen"].ToString() == "Admin" %>'
                                            CssClass="btn btn-sm btn-outline-danger" title="Xóa Lớp" OnClientClick="return confirm('Bạn có chắc muốn xóa lớp này?');"><i class="bi bi-trash"></i></asp:LinkButton>
                                        <asp:LinkButton ID="btnDiemDanh" runat="server" CommandName="DiemDanh" CommandArgument='<%# Eval("malop") + "|" + Eval("tenlop") %>'
                                            Visible='<%# Session["Quyen"] != null && Session["Quyen"].ToString() != "HocVien" %>'
                                            CssClass="btn btn-sm btn-success fw-semibold"><i class="bi bi-person-check-fill me-1"></i> Điểm danh</asp:LinkButton>
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </div>

    <!-- POPUP ĐIỂM DANH -->
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
                </div>
                <div class="modal-footer bg-light">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Đóng</button>
                    <asp:Button ID="btnLuuDiemDanh" runat="server" Text="Lưu kết quả Điểm danh" CssClass="btn btn-success fw-bold" OnClick="btnLuuDiemDanh_Click" />
                </div>
            </div>
        </div>
    </div>

    <script>
        // @ts-nocheck
        function showModalDiemDanh() {
            window.addEventListener('load', function () {
                var myModal = new window.bootstrap.Modal(document.getElementById('modalDiemDanh'));
                myModal.show();
            });
        }
    </script>
</asp:Content>