<%@ Page Title="Kỳ thi & Kết quả học tập" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeBehind="QuanLyKyThi.aspx.cs" Inherits="N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.KyThi.QuanLyKyThi" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Kỳ thi & Kết quả</asp:Content>
<asp:Content ID="ContentHead" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/css/select2.min.css" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/select2-bootstrap-5-theme@1.3.0/dist/select2-bootstrap-5-theme.min.css" rel="stylesheet" />
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
        .select2-container--bootstrap-5 .select2-selection { min-height: 38px; font-size: 0.95rem; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnlTB" runat="server" Visible="false" ClientIDMode="Static"
        CssClass="alert alert-info fw-semibold alert-dismissible fade show shadow"
        style="position: fixed; top: 76px; right: 24px; z-index: 1060; min-width: 320px; max-width: 480px;">
        <asp:Literal ID="litTB" runat="server" />
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    </asp:Panel>

    <!-- Thẻ ẩn để hứng tính năng tự điền (autofill) của trình duyệt -->
    <div style="position: absolute; left: -9999px; top: -9999px; opacity: 0; height: 0; overflow: hidden;">
        <input type="text" name="fake_username_autofill" tabindex="-1" />
        <input type="password" name="fake_password_autofill" tabindex="-1" />
    </div>

    <div class="row g-4">
        <!-- BẢNG 1: TỔ CHỨC KỲ THI (ẨN KHI LÀ HỌC VIÊN) -->
        <div class="col-lg-5" id="divFormKyThi" runat="server">
            <div class="card card-custom h-100 d-flex flex-column">
                <div class="card-header"><span>1. Tổ chức Kỳ thi</span></div>
                <div class="card-body d-flex flex-column flex-grow-1">
                    <div class="mb-2">
                        <label class="small fw-bold">Mã kỳ thi (*)</label>
                        <asp:TextBox ID="txtMaKT" runat="server" CssClass="form-control" ReadOnly="true" placeholder="Hệ thống tự động tạo..." />
                    </div>
                    <div class="mb-2">
                        <label class="small fw-bold">Khóa học</label>
                        <asp:DropDownList ID="ddlKhoaHoc" runat="server" CssClass="form-select" />
                    </div>
                    <div class="row g-2 mb-2">
                        <div class="col-6">
                            <label class="small fw-bold">Đợt thi</label>
                            <asp:DropDownList ID="ddlDotThi" runat="server" CssClass="form-select">
                                <asp:ListItem Text="Đợt 1" Value="Đợt 1" />
                                <asp:ListItem Text="Đợt 2" Value="Đợt 2" />
                            </asp:DropDownList>
                        </div>
                        <div class="col-6">
                            <label class="small fw-bold">Niên khóa (Năm)</label>
                            <asp:DropDownList ID="ddlNamThi" runat="server" CssClass="form-select" />
                        </div>
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

                    <div class="d-flex gap-2 mb-2">
                        <div class="position-relative flex-grow-1">
                            <asp:TextBox ID="txtTimKT" runat="server" ClientIDMode="Static" TextMode="Search" CssClass="form-control form-control-sm"
                                placeholder="Tìm mã KT, khóa học, đợt thi..." autocomplete="off" onclick="this.select();" />
                            <div id="goiYKT" class="list-group shadow position-absolute w-100"
                                style="top: 100%; left: 0; z-index: 1050; max-height: 150px; overflow-y: auto; display: none;">
                                <asp:Literal ID="litGoiYKT" runat="server" />
                            </div>
                        </div>
                        <asp:Button ID="btnTimKT" runat="server" ClientIDMode="Static" Text="Tìm" CssClass="btn btn-sm btn-primary-gradient" OnClick="btnTimKT_Click" />
                    </div>

                    <div class="table-responsive flex-grow-1">
                        <asp:GridView ID="gvKyThi" runat="server" CssClass="table table-sm table-bordered table-hover table-custom mb-0"
                            AutoGenerateColumns="false" DataKeyNames="makythi"
                            AllowPaging="true" PageSize="10" OnPageIndexChanging="gvKyThi_PageIndexChanging"
                            OnRowCommand="gvKyThi_RowCommand" EmptyDataText="Không tìm thấy kỳ thi nào.">
                            <PagerSettings Mode="NumericFirstLast" FirstPageText="&laquo;" LastPageText="&raquo;" PreviousPageText="&lsaquo;" NextPageText="&rsaquo;" PageButtonCount="5" />
                            <PagerStyle CssClass="pagination-ys" HorizontalAlign="Center" />
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
        </div>

        <!-- BẢNG 2: KẾT QUẢ ĐIỂM THI -->
        <div class="col-lg-7" id="divFormNhapDiem" runat="server">
            <div class="card card-custom h-100 d-flex flex-column">
                <div class="card-header d-flex justify-content-between align-items-center flex-wrap gap-2">
                    <span><i class="bi bi-journal-check me-2"></i><asp:Literal ID="litTieuDeDiem" runat="server" Text="2. Nhập Điểm thi Học viên & Tính Điểm Trung Bình" /></span>
                    <asp:Button ID="btnMoNhapHangLoat" runat="server" Text="Nhập điểm hàng loạt" CssClass="btn btn-sm btn-success fw-semibold" OnClick="btnMoNhapHangLoat_Click" />
                </div>
                <div class="card-body d-flex flex-column flex-grow-1">
                    <!-- KHUNG NHẬP ĐIỂM & TÌM KIẾM (ẨN KHI LÀ HỌC VIÊN) -->
                    <asp:Panel ID="pnlKhuVucNhapDiem" runat="server">
                        <div class="row g-2 mb-3">
                            <div class="col-3">
                                <label class="small fw-bold">Mã TG (*)</label>
                                <asp:TextBox ID="txtMaTG" runat="server" CssClass="form-control" ReadOnly="true" placeholder="Tự động..." />
                            </div>
                            <div class="col-9">
                                <label class="small fw-bold">1. Chọn Kỳ thi (*)</label>
                                <asp:DropDownList ID="ddlKyThi" runat="server" ClientIDMode="Static" CssClass="form-select select2-search"
                                    AutoPostBack="true" OnSelectedIndexChanged="ddlKyThi_SelectedIndexChanged" />
                            </div>
                            <div class="col-6">
                                <label class="small fw-bold">2. Chọn Học viên (Đã lọc theo Kỳ thi)</label>
                                <asp:DropDownList ID="ddlHocVien" runat="server" ClientIDMode="Static" CssClass="form-select select2-search"
                                    AutoPostBack="true" OnSelectedIndexChanged="ddlDiemDanh_SelectedIndexChanged" />
                            </div>
                            <div class="col-3">
                                <label class="small fw-bold">Chuyên cần (20%)</label>
                                <asp:TextBox ID="txtDiemCC" runat="server" CssClass="form-control" ReadOnly="true" Text="10" />
                            </div>
                            <div class="col-3">
                                <label class="small fw-bold">Điểm thi (80%) (*)</label>
                                <asp:TextBox ID="txtDiem" runat="server" CssClass="form-control" autocomplete="off" placeholder="Từ 0 - 10" />
                            </div>
                        </div>

                        <div class="d-flex gap-2 mb-3">
                            <asp:Button ID="btnThemTG" runat="server" Text="Thêm mới & Tính ĐTB" CssClass="btn btn-orange flex-grow-1" OnClick="btnThemTG_Click" />
                            <asp:Button ID="btnCapNhatTG" runat="server" Text="Cập nhật Điểm" CssClass="btn btn-primary-gradient flex-grow-1" OnClick="btnCapNhatTG_Click" />
                            <asp:Button ID="btnLamMoiTG" runat="server" Text="Làm mới" CssClass="btn btn-secondary" OnClick="btnLamMoiTG_Click" />
                        </div>

                        <div class="d-flex gap-2 mb-2">
                            <div class="position-relative flex-grow-1">
                                <asp:TextBox ID="txtTimTG" runat="server" ClientIDMode="Static" TextMode="Search" CssClass="form-control form-control-sm"
                                    placeholder="Tìm mã TG, tên học viên, mã KT..." autocomplete="off" onclick="this.select();" />
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
                    </asp:Panel>

                    <div class="table-responsive flex-grow-1">
                        <asp:GridView ID="gvThamGia" runat="server" CssClass="table table-sm table-bordered table-hover table-custom mb-0"
                            AutoGenerateColumns="false" DataKeyNames="matg"
                            AllowPaging="true" PageSize="10" OnPageIndexChanging="gvThamGia_PageIndexChanging"
                            OnRowCommand="gvThamGia_RowCommand"
                            EmptyDataText="Không tìm thấy bản ghi điểm thi nào.">
                            <PagerSettings Mode="NumericFirstLast" FirstPageText="&laquo;" LastPageText="&raquo;" PreviousPageText="&lsaquo;" NextPageText="&rsaquo;" PageButtonCount="5" />
                            <PagerStyle CssClass="pagination-ys" HorizontalAlign="Center" />
                            <Columns>
                                <asp:BoundField DataField="matg" HeaderText="Mã TG" />
                                <asp:BoundField DataField="hoten" HeaderText="Học viên" />
                                <asp:BoundField DataField="tenkhoahoc" HeaderText="Khóa học / Môn học" ItemStyle-CssClass="fw-semibold" />
                                <asp:BoundField DataField="makythi" HeaderText="Kỳ thi" />
                                <asp:BoundField DataField="ngaythi" HeaderText="Ngày thi" DataFormatString="{0:dd/MM/yyyy}" />
                                <asp:BoundField DataField="diem" HeaderText="Điểm số" ItemStyle-CssClass="fw-bold text-primary text-center" />
                                <asp:BoundField DataField="ketqua" HeaderText="Kết quả" ItemStyle-CssClass="fw-semibold text-center" />
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
    </div>

    <!-- POPUP NHẬP ĐIỂM HÀNG LOẠT THEO KỲ THI -->
    <div class="modal fade" id="modalNhapHangLoat" tabindex="-1" aria-hidden="true" data-bs-backdrop="static">
        <div class="modal-dialog modal-lg modal-dialog-centered">
            <div class="modal-content border-0 shadow">
                <div class="modal-header bg-success text-white">
                    <h5 class="modal-title fw-bold"><i class="bi bi-table me-2"></i>Nhập điểm hàng loạt: <asp:Literal ID="litTenKyThiHL" runat="server" /></h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body p-0">
                    <div class="table-responsive" style="max-height: 420px;">
                        <asp:GridView ID="gvNhapHangLoat" runat="server" CssClass="table table-sm table-bordered table-hover mb-0"
                            AutoGenerateColumns="false" DataKeyNames="mahocvien" EmptyDataText="Không có học viên nào đăng ký khóa học này.">
                            <Columns>
                                <asp:BoundField DataField="mahocvien" HeaderText="Mã HV" ItemStyle-Width="90" />
                                <asp:BoundField DataField="hoten" HeaderText="Họ tên Học viên" ItemStyle-CssClass="fw-semibold" />
                                <asp:TemplateField HeaderText="Chuyên cần (20%)" ItemStyle-Width="130" ItemStyle-CssClass="text-center">
                                    <ItemTemplate>
                                        <asp:Label ID="lblDiemCCHL" runat="server" Text='<%# Eval("DiemCC") %>' CssClass="fw-bold text-primary" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Điểm thi (80%)" ItemStyle-Width="150">
                                    <ItemTemplate>
                                        <asp:TextBox ID="txtDiemThiHL" runat="server" CssClass="form-control form-control-sm text-center"
                                            Text='<%# Eval("DiemHienTai") %>' placeholder="0 - 10" autocomplete="off" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>
                <div class="modal-footer bg-light">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Đóng</button>
                    <asp:Button ID="btnLuuHangLoat" runat="server" Text="Lưu tất cả Điểm" CssClass="btn btn-success fw-bold" OnClick="btnLuuHangLoat_Click" />
                </div>
            </div>
        </div>
    </div>

    <!-- POPUP XÁC NHẬN XÓA KỲ THI -->
    <div class="modal fade" id="modalXoaKT" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content border-0 shadow">
                <div class="modal-header bg-danger text-white">
                    <h5 class="modal-title fw-bold"><i class="bi bi-exclamation-triangle me-2"></i>Xác nhận Xóa Kỳ thi</h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
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
                    <h5 class="modal-title fw-bold"><i class="bi bi-exclamation-triangle me-2"></i>Xác nhận Xóa Điểm thi</h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
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

    <asp:HiddenField ID="hfMaKTXoa" runat="server" ClientIDMode="Static" />
    <asp:HiddenField ID="hfMaTGXoa" runat="server" ClientIDMode="Static" />

    <script src="https://code.jquery.com/jquery-3.7.1.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/js/select2.min.js"></script>
    <script>
        // @ts-nocheck
        function showModalNhapHangLoat() {
            window.addEventListener('load', function () {
                var el = document.getElementById('modalNhapHangLoat');
                if (el && window.bootstrap) new window.bootstrap.Modal(el).show();
            });
        }

        function hienThiModalXoaKT(maKT) {
            document.getElementById('hfMaKTXoa').value = maKT;
            document.getElementById('maKTCanXoa').innerText = maKT;
            new window.bootstrap.Modal(document.getElementById('modalXoaKT')).show();
        }

        function hienThiModalXoaTG(maTG) {
            document.getElementById('hfMaTGXoa').value = maTG;
            document.getElementById('maTGCanXoa').innerText = maTG;
            new window.bootstrap.Modal(document.getElementById('modalXoaTG')).show();
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
            if (window.jQuery && jQuery.fn.select2) {
                jQuery('.select2-search').select2({
                    theme: 'bootstrap-5',
                    width: '100%'
                });
            }

            var focusId = sessionStorage.getItem('focusSearchId');
            if (focusId) {
                var el = document.getElementById(focusId);
                if (el) el.focus();
                sessionStorage.removeItem('focusSearchId');
            }

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