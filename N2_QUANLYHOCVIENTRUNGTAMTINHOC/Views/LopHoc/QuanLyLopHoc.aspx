<%@ Page Title="Quản lý Lớp học & Thời khóa biểu" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeBehind="QuanLyLopHoc.aspx.cs" Inherits="N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.LopHoc.QuanLyLopHoc" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Lớp học & Thời khóa biểu</asp:Content>
<asp:Content ID="ContentHead" ContentPlaceHolderID="HeadContent" runat="server">
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
        .table-lophoc { font-size: 0.8rem; }
        .table-lophoc th, .table-lophoc td { vertical-align: middle !important; padding: 0.35rem 0.3rem !important; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnlTB" runat="server" Visible="false" ClientIDMode="Static"
        CssClass="alert alert-info fw-semibold alert-dismissible fade show shadow"
        style="position: fixed; top: 76px; right: 24px; z-index: 1060; min-width: 320px; max-width: 520px;">
        <asp:Literal ID="litTB" runat="server" />
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    </asp:Panel>

    <!-- Thẻ ẩn để hứng tính năng tự điền (autofill) của trình duyệt -->
    <div style="position: absolute; left: -9999px; top: -9999px; opacity: 0; height: 0; overflow: hidden;">
        <input type="text" name="fake_username_autofill" tabindex="-1" />
        <input type="password" name="fake_password_autofill" tabindex="-1" />
    </div>

    <!-- PHẦN 1 (TRÊN): FORM MỞ LỚP & DANH SÁCH LỚP -->
    <div class="row g-4 mb-4">
        <!-- FORM QUẢN LÝ LỚP & ĐỔI LỊCH HỌC -->
        <asp:Panel ID="pnlFormAdmin" runat="server" CssClass="col-lg-4">
            <div class="card card-custom">
                <div class="card-header"><span><i class="bi bi-easel2-fill me-2"></i>Mở Lớp & Cập nhật Lịch học (TKB)</span></div>
                <div class="card-body">
                    <div class="mb-2"><label class="small fw-bold">Mã Lớp (*)</label><asp:TextBox ID="txtMaLop" runat="server" CssClass="form-control form-control-sm" placeholder="VD: LH12" /></div>
                    <div class="mb-2"><label class="small fw-bold">Tên Lớp (*)</label><asp:TextBox ID="txtTenLop" runat="server" CssClass="form-control form-control-sm" autocomplete="new-password" placeholder="VD: Lớp Web K12 - Tối 2-4-6" /></div>
                    <div class="mb-2"><label class="small fw-bold">Thuộc Khóa học (*)</label><asp:DropDownList ID="ddlKhoaHoc" runat="server" CssClass="form-select form-select-sm" /></div>
                    <div class="mb-2"><label class="small fw-bold">Giảng viên phụ trách</label><asp:DropDownList ID="ddlGiangVien" runat="server" CssClass="form-select form-select-sm" /></div>
                    <div class="mb-2"><label class="small fw-bold">Ngày khai giảng</label><asp:TextBox ID="txtKhaiGiang" runat="server" TextMode="Date" CssClass="form-control form-control-sm" /></div>
                    
                    <hr class="my-2" />
                    <h6 class="small fw-bold text-dark mb-2"><i class="bi bi-clock-history me-1"></i>Thiết lập / Đổi Lịch học cho Lớp:</h6>
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
                                <asp:ListItem Text="Thứ 2, 3, 4" Value="Thứ 2, 3, 4" />
                                <asp:ListItem Text="Thứ 5, 6, 7" Value="Thứ 5, 6, 7" />
                                <asp:ListItem Text="Thứ 7, CN" Value="Thứ 7, CN" />
                            </asp:DropDownList>
                        </div>
                        <div class="col-6">
                            <label class="small fw-bold">Khung giờ học</label>
                            <asp:TextBox ID="txtGioHoc" runat="server" CssClass="form-control form-control-sm" autocomplete="new-password" Text="18:00 - 20:30" />
                        </div>
                        <div class="col-6">
                            <label class="small fw-bold">Phòng học</label>
                            <asp:DropDownList ID="ddlPhongHoc" runat="server" CssClass="form-select form-select-sm">
                                <asp:ListItem Text="Phòng Lab 01" Value="Phòng Lab 01" />
                                <asp:ListItem Text="Phòng Lab 02" Value="Phòng Lab 02" />
                                <asp:ListItem Text="Phòng Lab 03" Value="Phòng Lab 03" />
                                <asp:ListItem Text="Phòng Lab 04" Value="Phòng Lab 04" />
                                <asp:ListItem Text="Phòng Lab 05" Value="Phòng Lab 05" />
                                <asp:ListItem Text="Phòng Đồ họa" Value="Phòng Đồ họa" />
                            </asp:DropDownList>
                        </div>
                    </div>

                    <div class="d-flex flex-wrap gap-2 mt-3">
                        <asp:Button ID="btnThemLop" runat="server" Text="Thêm Lớp & Lịch" CssClass="btn btn-sm btn-orange flex-grow-1" OnClick="btnThemLop_Click" />
                        <asp:Button ID="btnSuaLop" runat="server" Text="Cập nhật Lớp & Đổi Lịch" CssClass="btn btn-sm btn-primary-gradient flex-grow-1" OnClick="btnSuaLop_Click" />
                        <asp:Button ID="btnLamMoi" runat="server" Text="Làm mới" CssClass="btn btn-sm btn-secondary flex-grow-1" OnClick="btnLamMoi_Click" />
                    </div>
                </div>
            </div>
        </asp:Panel>

        <!-- DANH SÁCH LỚP & CHI TIẾT THỜI KHÓA BIỂU -->
        <div class="col-lg-8" id="colDanhSachLop" runat="server">
            <div class="card card-custom">
                <div class="card-header d-flex justify-content-between align-items-center">
                    <span class="text-nowrap"><i class="bi bi-list-task me-2"></i><asp:Literal ID="litTieuDeBangLop" runat="server" Text="Danh sách Lớp học phần & Thời khóa biểu" /></span>
                    <asp:Panel ID="pnlTimKiemLop" runat="server" CssClass="d-flex gap-2 w-100 justify-content-end">
                        <div class="position-relative flex-grow-1" style="max-width: 260px;">
                            <asp:TextBox ID="txtTimLop" runat="server" ClientIDMode="Static" TextMode="Search" CssClass="form-control form-control-sm"
                                placeholder="Tìm mã, tên lớp, khóa..." autocomplete="off" onclick="this.select();" />
                            <div id="goiYLop" class="list-group shadow position-absolute w-100"
                                style="top: 100%; left: 0; z-index: 1050; max-height: 150px; overflow-y: auto; display: none;">
                                <asp:Literal ID="litGoiYLop" runat="server" />
                            </div>
                        </div>
                        <asp:Button ID="btnTimLop" runat="server" ClientIDMode="Static" Text="Tìm" CssClass="btn btn-sm btn-primary-gradient" OnClick="btnTimLop_Click" />
                    </asp:Panel>
                </div>
                <div class="card-body p-0">
                    <asp:GridView ID="gvLopHoc" runat="server" CssClass="table table-sm table-hover table-custom table-lophoc mb-0"
                        AutoGenerateColumns="false" DataKeyNames="malop" 
                        AllowPaging="true" PageSize="8" OnPageIndexChanging="gvLopHoc_PageIndexChanging" OnRowCommand="gvLopHoc_RowCommand"
                        EmptyDataText="Không tìm thấy lớp học nào.">
                        <PagerSettings Mode="NumericFirstLast" FirstPageText="&laquo;" LastPageText="&raquo;" PreviousPageText="&lsaquo;" NextPageText="&rsaquo;" PageButtonCount="5" />
                        <PagerStyle CssClass="pagination-ys" HorizontalAlign="Center" />
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
                                        
                                        <button type="button" class="btn btn-sm btn-outline-danger" title="Xóa Lớp"
                                            style='<%# Session["Quyen"] != null && Session["Quyen"].ToString() == "Admin" ? "" : "display:none;" %>'
                                            onclick="hienThiModalXoaLop('<%# Eval("malop") %>'); return false;"><i class="bi bi-trash"></i></button>
                                        
                                        <asp:LinkButton ID="btnDiemDanh" runat="server" CommandName="DiemDanh" CommandArgument='<%# Eval("malop") + "|" + Eval("tenlop") %>'
                                            Visible='<%# Session["Quyen"] != null && Session["Quyen"].ToString() != "HocVien" %>'
                                            CssClass="btn btn-sm btn-success fw-semibold text-nowrap"><i class="bi bi-person-check-fill me-1"></i> Điểm danh</asp:LinkButton>
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </div>

    <!-- PHẦN 2 (DƯỚI): BẢNG THỜI KHÓA BIỂU DẠNG LƯỚI THEO THỨ & CA HỌC -->
    <asp:Panel ID="pnlLuoiTKB" runat="server" CssClass="card card-custom">
        <div class="card-header d-flex justify-content-between align-items-center">
            <span><i class="bi bi-calendar3-week-fill text-primary me-2"></i>Bảng Thời Khóa Biểu Trong Tuần (Bấm trực tiếp vào lớp để đổi lịch)</span>
            <span class="badge bg-info-subtle text-primary">Lịch học cập nhật trực tiếp từ Phòng Đào tạo</span>
        </div>
        <div class="card-body p-0">
            <div class="table-responsive">
                <asp:Literal ID="litBangTKB" runat="server" />
            </div>
        </div>
    </asp:Panel>

    <!-- POPUP ĐỔI LỊCH HỌC TRỰC TIẾP TRÊN TKB -->
    <div class="modal fade" id="modalDoiLichTKB" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content border-0 shadow">
                <div class="modal-header bg-primary text-white">
                    <h5 class="modal-title fw-bold"><i class="bi bi-calendar2-check me-2"></i>Đổi Lịch học: <span id="lblTenLopDoiLich"></span></h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <asp:HiddenField ID="hfMaLopDoiLich" runat="server" ClientIDMode="Static" />
                    <div class="row g-3">
                        <div class="col-6">
                            <label class="small fw-bold">Ca học</label>
                            <asp:DropDownList ID="ddlCaHocTKB" runat="server" ClientIDMode="Static" CssClass="form-select form-select-sm">
                                <asp:ListItem Text="Ca sáng" Value="Ca sáng" />
                                <asp:ListItem Text="Ca chiều" Value="Ca chiều" />
                                <asp:ListItem Text="Ca tối" Value="Ca tối" />
                            </asp:DropDownList>
                        </div>
                        <div class="col-6">
                            <label class="small fw-bold">Các thứ trong tuần</label>
                            <asp:DropDownList ID="ddlThuHocTKB" runat="server" ClientIDMode="Static" CssClass="form-select form-select-sm">
                                <asp:ListItem Text="Thứ 2, 4, 6" Value="Thứ 2, 4, 6" />
                                <asp:ListItem Text="Thứ 3, 5, 7" Value="Thứ 3, 5, 7" />
                                <asp:ListItem Text="Thứ 2, 3, 4" Value="Thứ 2, 3, 4" />
                                <asp:ListItem Text="Thứ 5, 6, 7" Value="Thứ 5, 6, 7" />
                                <asp:ListItem Text="Thứ 7, CN" Value="Thứ 7, CN" />
                            </asp:DropDownList>
                        </div>
                        <div class="col-6">
                            <label class="small fw-bold">Khung giờ học</label>
                            <asp:TextBox ID="txtGioHocTKB" runat="server" ClientIDMode="Static" CssClass="form-control form-control-sm" autocomplete="off" />
                        </div>
                        <div class="col-6">
                            <label class="small fw-bold">Phòng học</label>
                            <asp:DropDownList ID="ddlPhongHocTKB" runat="server" ClientIDMode="Static" CssClass="form-select form-select-sm">
                                <asp:ListItem Text="Phòng Lab 01" Value="Phòng Lab 01" />
                                <asp:ListItem Text="Phòng Lab 02" Value="Phòng Lab 02" />
                                <asp:ListItem Text="Phòng Lab 03" Value="Phòng Lab 03" />
                                <asp:ListItem Text="Phòng Lab 04" Value="Phòng Lab 04" />
                                <asp:ListItem Text="Phòng Lab 05" Value="Phòng Lab 05" />
                                <asp:ListItem Text="Phòng Đồ họa" Value="Phòng Đồ họa" />
                            </asp:DropDownList>
                        </div>
                    </div>
                </div>
                <div class="modal-footer bg-light">
                    <button type="button" class="btn btn-sm btn-secondary" data-bs-dismiss="modal">Hủy</button>
                    <asp:Button ID="btnLuuDoiLichTKB" runat="server" Text="Lưu Lịch mới" CssClass="btn btn-sm btn-primary-gradient fw-bold" OnClick="btnLuuDoiLichTKB_Click" />
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
                    <div class="bg-light p-3 border-bottom d-flex align-items-center justify-content-between flex-wrap gap-2">
                        <div class="d-flex align-items-center gap-3">
                            <label class="fw-bold text-nowrap mb-0">Ngày điểm danh:</label>
                            <asp:TextBox ID="txtNgayDiemDanh" runat="server" TextMode="Date" CssClass="form-control form-control-sm" style="width: 150px;" 
                                AutoPostBack="true" OnTextChanged="txtNgayDiemDanh_TextChanged" />
                            <asp:HiddenField ID="hfMaLopDiemDanh" runat="server" />
                        </div>
                        <span class="small text-muted fw-semibold">
                            <i class="bi bi-info-circle me-1 text-warning"></i>Nghỉ 2 buổi: Cảnh báo | Nghỉ &ge; 3 buổi: Cấm thi
                        </span>
                    </div>
                    <div class="table-responsive" style="max-height: 400px;">
                        <asp:GridView ID="gvHocVienLop" runat="server" CssClass="table table-bordered table-hover mb-0" AutoGenerateColumns="false" DataKeyNames="mahocvien" EmptyDataText="Chưa có học viên.">
                            <Columns>
                                <asp:BoundField DataField="mahocvien" HeaderText="Mã HV" ItemStyle-Width="90" />
                                <asp:TemplateField HeaderText="Họ tên Học viên">
                                    <ItemTemplate>
                                        <span class="fw-semibold"><%# Eval("hoten") %></span>
                                        <asp:Literal ID="litCanhBao" runat="server" Text='<%# Eval("CanhBao") %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Trạng thái Điểm danh (Chuyên cần)" ItemStyle-Width="340">
                                    <ItemTemplate>
                                        <asp:RadioButtonList ID="rblTrangThai" runat="server" RepeatDirection="Horizontal" CssClass="d-flex gap-3 small mt-1 mb-0"
                                            SelectedValue='<%# Eval("trangthai") %>'>
                                            <asp:ListItem Text="Có mặt" Value="Có mặt" />
                                            <asp:ListItem Text="Vắng phép" Value="Vắng phép" />
                                            <asp:ListItem Text="Vắng không phép" Value="Vắng không phép" />
                                        </asp:RadioButtonList>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Ghi chú">
                                    <ItemTemplate>
                                        <asp:TextBox ID="txtGhiChuDD" runat="server" CssClass="form-control form-control-sm" autocomplete="off" placeholder="..." Text='<%# Eval("ghichu") %>' />
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

    <!-- POPUP XÁC NHẬN XÓA LỚP HỌC -->
    <div class="modal fade" id="modalXoaLop" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content border-0 shadow">
                <div class="modal-header bg-danger text-white">
                    <h5 class="modal-title fw-bold"><i class="bi bi-exclamation-triangle me-2"></i>Xác nhận Xóa</h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body py-4 text-center">
                    <i class="bi bi-trash display-4 text-danger mb-3 d-block"></i>
                    <p class="fs-5 mb-0">Bạn có chắc chắn muốn xóa lớp <strong id="maLopCanXoa"></strong> này?</p>
                </div>
                <div class="modal-footer justify-content-center border-0 pb-4">
                    <button type="button" class="btn btn-secondary px-4" data-bs-dismiss="modal">Hủy</button>
                    <asp:LinkButton ID="btnXoaChinhThuc" runat="server" ClientIDMode="Static" CssClass="btn btn-danger px-4 d-none" OnClick="btnXoaChinhThuc_Click">Đồng ý</asp:LinkButton>
                    <button type="button" class="btn btn-danger px-4" onclick="document.getElementById('btnXoaChinhThuc').click();">
                        <i class="bi bi-check-lg me-1"></i> Đồng ý
                    </button>
                </div>
            </div>
        </div>
    </div>

    <asp:HiddenField ID="hfMaLopXoa" runat="server" ClientIDMode="Static" />

    <script>
        // @ts-nocheck
        function showModalDiemDanh() {
            window.addEventListener('load', function () {
                var myModal = new window.bootstrap.Modal(document.getElementById('modalDiemDanh'));
                myModal.show();
            });
        }

        function moModalDoiLich(maLop, tenLop, caHoc, thuHoc, gioHoc, phongHoc) {
            document.getElementById('hfMaLopDoiLich').value = maLop;
            document.getElementById('lblTenLopDoiLich').innerText = tenLop;
            document.getElementById('ddlCaHocTKB').value = caHoc;
            document.getElementById('ddlThuHocTKB').value = thuHoc;
            document.getElementById('txtGioHocTKB').value = gioHoc;
            document.getElementById('ddlPhongHocTKB').value = phongHoc;
            var el = document.getElementById('modalDoiLichTKB');
            if (el && window.bootstrap) {
                new window.bootstrap.Modal(el).show();
            }
        }

        function hienThiModalXoaLop(maLop) {
            document.getElementById('hfMaLopXoa').value = maLop;
            document.getElementById('maLopCanXoa').innerText = maLop;
            var el = document.getElementById('modalXoaLop');
            if (el && window.bootstrap) {
                new window.bootstrap.Modal(el).show();
            }
        }

        function thietLapGoiYLop() {
            var input = document.getElementById('txtTimLop');
            var list = document.getElementById('goiYLop');
            var btn = document.getElementById('btnTimLop');
            if (!input || !list || !btn) return;

            var items = list.querySelectorAll('.goi-y-item');

            function locGoiY() {
                var kw = input.value.trim().toLowerCase();
                var co = false;
                items.forEach(function (item) {
                    if (kw !== '' && item.innerText.toLowerCase().indexOf(kw) !== -1) {
                        item.style.display = 'block';
                        co = true;
                    } else {
                        item.style.display = 'none';
                    }
                });
                list.style.display = co ? 'block' : 'none';
            }

            input.addEventListener('input', function () {
                if (this.value.trim() === '') {
                    list.style.display = 'none';
                    btn.click();
                } else {
                    locGoiY();
                }
            });

            input.addEventListener('focus', function () {
                if (this.value.trim() !== '') locGoiY();
            });

            items.forEach(function (item) {
                item.addEventListener('mousedown', function (e) {
                    e.preventDefault();
                    input.value = this.getAttribute('data-value');
                    list.style.display = 'none';
                    btn.click();
                });
            });

            document.addEventListener('click', function (e) {
                if (!input.contains(e.target) && !list.contains(e.target)) {
                    list.style.display = 'none';
                }
            });
        }

        window.addEventListener('beforeunload', function () {
            sessionStorage.setItem('scrollPos_LopHoc', window.scrollY);
        });

        window.addEventListener('load', function () {
            var savedScroll = sessionStorage.getItem('scrollPos_LopHoc');
            if (savedScroll !== null) {
                window.scrollTo({ top: parseInt(savedScroll, 10), behavior: 'instant' });
            }
        });

        window.addEventListener('DOMContentLoaded', function () {
            thietLapGoiYLop();

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
    </script>
</asp:Content>