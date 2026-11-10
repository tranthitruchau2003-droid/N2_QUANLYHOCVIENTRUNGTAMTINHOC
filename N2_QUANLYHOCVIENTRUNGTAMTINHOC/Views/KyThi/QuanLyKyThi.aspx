<%@ Page Title="Kỳ thi & Kết quả học tập" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeBehind="QuanLyKyThi.aspx.cs" Inherits="N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.KyThi.QuanLyKyThi" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Kỳ thi & Kết quả</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnlTB" runat="server" Visible="false" ClientIDMode="Static"
        CssClass="alert alert-info fw-semibold alert-dismissible fade show shadow"
        style="position: fixed; top: 76px; right: 24px; z-index: 1060; min-width: 320px; max-width: 480px;">
        <asp:Literal ID="litTB" runat="server" />
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    </asp:Panel>

    <div class="row g-4">
        <!-- BẢNG KỲ THI -->
        <div class="col-lg-5">
            <div class="card card-custom">
                <div class="card-header"><span>1. Tổ chức Kỳ thi</span></div>
                <div class="card-body">
                    <div class="mb-2">
                        <label class="small fw-bold">Mã kỳ thi (*)</label>
                        <asp:TextBox ID="txtMaKT" runat="server" CssClass="form-control" placeholder="KT02" />
                    </div>
                    <div class="mb-2">
                        <label class="small fw-bold">Khóa học</label>
                        <asp:DropDownList ID="ddlKhoaHoc" runat="server" CssClass="form-select" />
                    </div>
                    <div class="mb-2">
                        <label class="small fw-bold">Niên khóa / Đợt thi</label>
                        <asp:TextBox ID="txtNienKhoa" runat="server" CssClass="form-control" placeholder="Đợt 2 - 2026" />
                    </div>
                    <div class="mb-3">
                        <label class="small fw-bold">Ngày thi</label>
                        <asp:TextBox ID="txtNgayThi" runat="server" TextMode="Date" CssClass="form-control" />
                    </div>

                    <div class="d-flex gap-2 mb-3">
                        <asp:Button ID="btnTaoKyThi" runat="server" Text="Tạo Kỳ thi" CssClass="btn btn-orange flex-grow-1" OnClick="btnTaoKyThi_Click" />
                        <asp:Button ID="btnCapNhatKT" runat="server" Text="Cập nhật" CssClass="btn btn-primary-gradient flex-grow-1" OnClick="btnCapNhatKT_Click" />
                        <asp:Button ID="btnLamMoiKT" runat="server" Text="Làm mới" CssClass="btn btn-secondary" OnClick="btnLamMoiKT_Click" />
                    </div>

                    <!-- TÌM KIẾM KỲ THI -->
<div class="d-flex gap-2 mb-2">
    <div class="position-relative flex-grow-1">
        <asp:TextBox ID="txtTimKT" runat="server" ClientIDMode="Static" CssClass="form-control form-control-sm"
            placeholder="Tìm mã KT, khóa học, đợt thi..." autocomplete="off" onfocus="this.select();" />
        <div id="goiYKT" class="list-group shadow position-absolute w-100"
            style="top: 100%; left: 0; z-index: 1050; max-height: 150px; overflow-y: auto; display: none;">
            <asp:Literal ID="litGoiYKT" runat="server" />
        </div>
    </div>
    <asp:Button ID="btnTimKT" runat="server" ClientIDMode="Static" Text="Tìm" CssClass="btn btn-sm btn-primary-gradient" OnClick="btnTimKT_Click" />
</div>

                    <asp:GridView ID="gvKyThi" runat="server" CssClass="table table-sm table-bordered table-hover table-custom mb-0"
                        AutoGenerateColumns="false" DataKeyNames="makythi" OnRowCommand="gvKyThi_RowCommand">
                        <Columns>
                            <asp:BoundField DataField="makythi" HeaderText="Mã KT" />
                            <asp:BoundField DataField="tenkhoahoc" HeaderText="Khóa học" />
                            <asp:BoundField DataField="ngaythi" HeaderText="Ngày thi" DataFormatString="{0:dd/MM/yyyy}" />
                            <asp:TemplateField HeaderText="Thao tác">
                                <ItemTemplate>
                                    <div class="d-flex gap-1 justify-content-center">
                                        <asp:LinkButton ID="btnChonKT" runat="server" CommandName="ChonKT" CommandArgument='<%# Eval("makythi") %>'
                                            CssClass="btn btn-sm btn-outline-primary" title="Sửa"><i class="bi bi-pencil"></i></asp:LinkButton>
                                        <button type="button" class="btn btn-sm btn-outline-danger" title="Xóa"
                                            onclick="hienThiModalXoaKT('<%# Eval("makythi") %>'); return false;"><i class="bi bi-trash"></i></button>
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>

        <!-- BẢNG THAM GIA -->
        <div class="col-lg-7">
            <div class="card card-custom">
                <div class="card-header"><span>2. Nhập Điểm thi Học viên</span></div>
                <div class="card-body">
                    <div class="row g-2 mb-3">
                        <div class="col-3">
                            <label class="small fw-bold">Mã TG</label>
                            <asp:TextBox ID="txtMaTG" runat="server" CssClass="form-control" placeholder="TG02" />
                        </div>
                        <div class="col-5">
                            <label class="small fw-bold">Học viên</label>
                            <asp:DropDownList ID="ddlHocVien" runat="server" CssClass="form-select" />
                        </div>
                        <div class="col-4">
                            <label class="small fw-bold">Kỳ thi</label>
                            <asp:DropDownList ID="ddlKyThi" runat="server" CssClass="form-select" />
                        </div>
                        <div class="col-12">
                            <label class="small fw-bold">Điểm số (Hệ 10 - Tự động xét Đạt >= 5.0)</label>
                            <asp:TextBox ID="txtDiem" runat="server" CssClass="form-control" placeholder="8.5" />
                        </div>
                    </div>
                    <div class="d-flex gap-2 mb-3">
    <asp:Button ID="btnThemTG" runat="server" Text="Thêm mới" CssClass="btn btn-orange flex-grow-1" OnClick="btnThemTG_Click" />
    <asp:Button ID="btnCapNhatTG" runat="server" Text="Cập nhật" CssClass="btn btn-primary-gradient flex-grow-1" OnClick="btnCapNhatTG_Click" />
    <asp:Button ID="btnLamMoiTG" runat="server" Text="Làm mới" CssClass="btn btn-secondary" OnClick="btnLamMoiTG_Click" />
</div>

                   <!-- TÌM KIẾM & BỘ LỌC THAM GIA -->
<div class="d-flex gap-2 mb-2">
    <div class="position-relative flex-grow-1">
        <asp:TextBox ID="txtTimTG" runat="server" ClientIDMode="Static" CssClass="form-control form-control-sm"
            placeholder="Tìm mã TG, tên học viên, mã KT..." autocomplete="off" onfocus="this.select();" />
        <div id="goiYTG" class="list-group shadow position-absolute w-100"
            style="top: 100%; left: 0; z-index: 1050; max-height: 150px; overflow-y: auto; display: none;">
            <asp:Literal ID="litGoiYTG" runat="server" />
        </div>
    </div>
    <asp:DropDownList ID="ddlLocKetQua" runat="server" CssClass="form-select form-select-sm" style="max-width: 150px;"
        AutoPostBack="true" OnSelectedIndexChanged="btnTimTG_Click">
        <asp:ListItem Text="-- Tất cả KQ --" Value="" />
        <asp:ListItem Text="Đạt" Value="Đạt" />
        <asp:ListItem Text="Không đạt" Value="Không đạt" />
    </asp:DropDownList>
    <asp:Button ID="btnTimTG" runat="server" ClientIDMode="Static" Text="Tìm" CssClass="btn btn-sm btn-primary-gradient" OnClick="btnTimTG_Click" />
</div>

                    <asp:GridView ID="gvThamGia" runat="server" CssClass="table table-sm table-bordered table-hover table-custom mb-0"
                    AutoGenerateColumns="false" DataKeyNames="matg" OnRowCommand="gvThamGia_RowCommand">
                    <Columns>
                        <asp:BoundField DataField="matg" HeaderText="Mã TG" />
                        <asp:BoundField DataField="hoten" HeaderText="Học viên" />
                        <asp:BoundField DataField="makythi" HeaderText="Kỳ thi" />
                        <asp:BoundField DataField="diem" HeaderText="Điểm số" />
                        <asp:BoundField DataField="ketqua" HeaderText="Kết quả" />
                        <asp:TemplateField HeaderText="Thao tác">
                            <ItemTemplate>
                                <div class="d-flex gap-1 justify-content-center">
                                    <asp:LinkButton ID="btnChonTG" runat="server" CommandName="ChonTG" CommandArgument='<%# Eval("matg") %>'
                                        CssClass="btn btn-sm btn-outline-primary" title="Sửa"><i class="bi bi-pencil"></i></asp:LinkButton>
                                    <button type="button" class="btn btn-sm btn-outline-danger" title="Xóa"
                                        onclick="hienThiModalXoaTG('<%# Eval("matg") %>'); return false;"><i class="bi bi-trash"></i></button>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
                </div>
            </div>
        </div>
    </div>

    <!-- POPUP XÁC NHẬN XÓA KỲ THI -->
    <div class="modal fade" id="modalXoaKT" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content border-0 shadow">
                <div class="modal-header bg-danger text-white">
                    <h5 class="modal-title fw-bold">
                        <i class="bi bi-exclamation-triangle me-2"></i>Xác nhận Xóa Kỳ thi
                    </h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body py-4 text-center">
                    <i class="bi bi-trash display-4 text-danger mb-3 d-block"></i>
                    <p class="fs-5 mb-0">Bạn có chắc chắn muốn xóa kỳ thi <strong id="maKTCanXoa"></strong> này?</p>
                </div>
                <div class="modal-footer justify-content-center border-0 pb-4">
                    <button type="button" class="btn btn-secondary px-4" data-bs-dismiss="modal">Hủy</button>
                    <asp:LinkButton ID="btnXoaKTChinhThuc" runat="server" ClientIDMode="Static" CssClass="btn btn-danger px-4 d-none" OnClick="btnXoaKTChinhThuc_Click">Đồng ý</asp:LinkButton>
                    <button type="button" class="btn btn-danger px-4" onclick="document.getElementById('btnXoaKTChinhThuc').click();">
                        <i class="bi bi-check-lg me-1"></i> Đồng ý
                    </button>
                </div>
            </div>
        </div>
    </div>

    <!-- POPUP XÁC NHẬN XÓA ĐIỂM THI -->
    <div class="modal fade" id="modalXoaTG" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content border-0 shadow">
                <div class="modal-header bg-danger text-white">
                    <h5 class="modal-title fw-bold">
                        <i class="bi bi-exclamation-triangle me-2"></i>Xác nhận Xóa Điểm thi
                    </h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body py-4 text-center">
                    <i class="bi bi-trash display-4 text-danger mb-3 d-block"></i>
                    <p class="fs-5 mb-0">Bạn có chắc chắn muốn xóa bản ghi <strong id="maTGCanXoa"></strong> này?</p>
                </div>
                <div class="modal-footer justify-content-center border-0 pb-4">
                    <button type="button" class="btn btn-secondary px-4" data-bs-dismiss="modal">Hủy</button>
                    <asp:LinkButton ID="btnXoaTGChinhThuc" runat="server" ClientIDMode="Static" CssClass="btn btn-danger px-4 d-none" OnClick="btnXoaTGChinhThuc_Click">Đồng ý</asp:LinkButton>
                    <button type="button" class="btn btn-danger px-4" onclick="document.getElementById('btnXoaTGChinhThuc').click();">
                        <i class="bi bi-check-lg me-1"></i> Đồng ý
                    </button>
                </div>
            </div>
        </div>
    </div>

<asp:HiddenField ID="hfMaTGXoa" runat="server" ClientIDMode="Static" />

    <asp:HiddenField ID="hfMaKTXoa" runat="server" ClientIDMode="Static" />

    <script>
        function hienThiModalXoaKT(maKT) {
            document.getElementById('hfMaKTXoa').value = maKT;
            document.getElementById('maKTCanXoa').innerText = maKT;
            var modalXoa = new window.bootstrap.Modal(document.getElementById('modalXoaKT'));
            modalXoa.show();
        }

        function hienThiModalXoaTG(maTG) {
            document.getElementById('hfMaTGXoa').value = maTG;
            document.getElementById('maTGCanXoa').innerText = maTG;
            var modalXoa = new window.bootstrap.Modal(document.getElementById('modalXoaTG'));
            modalXoa.show();
        }

        function thietLapTimKiem(inputId, listId, btnId) {
            var input = document.getElementById(inputId);
            var list = document.getElementById(listId);
            var btn = document.getElementById(btnId);
            if (!input || !list || !btn) return;

            var items = list.querySelectorAll('.goi-y-item');

            function locGoiY() {
                var kw = input.value.trim().toLowerCase();
                var coKetQua = false;
                items.forEach(function (item) {
                    var text = item.innerText.toLowerCase();
                    if (kw !== '' && text.indexOf(kw) !== -1) {
                        item.style.display = 'block';
                        coKetQua = true;
                    } else {
                        item.style.display = 'none';
                    }
                });
                list.style.display = coKetQua ? 'block' : 'none';
            }

            input.addEventListener('input', function () {
                if (this.value.trim() === '') {
                    list.style.display = 'none';
                    sessionStorage.setItem('focusSearchId', inputId);
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

        window.addEventListener('DOMContentLoaded', function () {
            // Giữ lại con trỏ trong ô tìm kiếm sau khi xóa rỗng và tải lại trang
            var focusId = sessionStorage.getItem('focusSearchId');
            if (focusId) {
                var el = document.getElementById(focusId);
                if (el) el.focus();
                sessionStorage.removeItem('focusSearchId');
            }

            // Tự động tắt thông báo sau 5 giây
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

            thietLapTimKiem('txtTimKT', 'goiYKT', 'btnTimKT');
            thietLapTimKiem('txtTimTG', 'goiYTG', 'btnTimTG');
        });
    </script>
</asp:Content>