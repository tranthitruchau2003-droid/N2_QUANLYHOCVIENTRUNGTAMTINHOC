<%@ Page Title="Quản lý Khóa học" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeBehind="QuanLyKhoaHoc.aspx.cs" Inherits="N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.KhoaHoc.QuanLyKhoaHoc" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Quản lý Khóa học</asp:Content>
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
    <!-- THÔNG BÁO NỔI KHÔNG ĐẨY GIAO DIỆN, TỰ ẨN SAU 5S -->
    <asp:Panel ID="pnlTB" runat="server" Visible="false" ClientIDMode="Static"
        CssClass="alert alert-info fw-semibold alert-dismissible fade show shadow"
        style="position: fixed; top: 76px; right: 24px; z-index: 1060; min-width: 320px; max-width: 480px;">
        <asp:Literal ID="litTB" runat="server" />
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    </asp:Panel>

    <!-- Thẻ ẩn để hứng tính năng tự điền (autofill) của Chrome/Edge -->
    <div style="position: absolute; left: -9999px; top: -9999px; opacity: 0; height: 0; overflow: hidden;">
        <input type="text" name="fake_username_autofill" tabindex="-1" />
        <input type="password" name="fake_password_autofill" tabindex="-1" />
    </div>

    <div class="row g-4">
        <!-- FORM NHẬP LIỆU -->
        <div class="col-lg-4">
            <div class="card card-custom">
                <div class="card-header"><span><i class="bi bi-journal-plus me-2"></i>Thông tin Khóa học</span></div>
                <div class="card-body">
                    <div class="mb-2">
                        <label class="form-label small fw-bold">Mã khóa học (*)</label>
                        <asp:TextBox ID="txtMaKH" runat="server" CssClass="form-control" ReadOnly="true" placeholder="Hệ thống tự động tạo..." />
                    </div>
                    <div class="mb-2 position-relative">
                        <label class="form-label small fw-bold">Tên khóa học (*)</label>
                        <asp:TextBox ID="txtTenKH" runat="server" ClientIDMode="Static" CssClass="form-control" autocomplete="off" onclick="this.select();" placeholder="Nhập hoặc chọn gợi ý..." />
                        <div id="goiYTenKH" class="list-group shadow position-absolute w-100"
                            style="top: 100%; left: 0; z-index: 1050; max-height: 150px; overflow-y: auto; display: none;">
                            <asp:Literal ID="litGoiYTenKH" runat="server" />
                        </div>
                    </div>
                    <div class="mb-2">
                        <label class="form-label small fw-bold">Học phí (VNĐ)</label>
                        <asp:TextBox ID="txtHocPhi" runat="server" ClientIDMode="Static" CssClass="form-control"
                            autocomplete="off" placeholder="VD: 1,500,000" oninput="dinhDangHocPhi(this);" />
                    </div>
                    <div class="mb-2">
                        <label class="form-label small fw-bold">Thời gian học/ tháng (*)</label>
                        <div class="input-group input-group-sm">
                            <asp:TextBox ID="txtThoiGian" runat="server" ClientIDMode="Static" CssClass="form-control" 
                                placeholder="VD: 3 hoặc 2.5" autocomplete="off" onkeypress="return chiChoNhapSoVaCham(event);" oninput="chiChoNhapSoVaChamInput(this);" />
                            <span class="input-group-text bg-light fw-bold text-muted">tháng</span>
                        </div>
                    </div>
                    <div class="mb-2">
                        <label class="form-label small fw-bold">Trạng thái</label>
                        <asp:DropDownList ID="ddlTrangThai" runat="server" CssClass="form-select">
                            <asp:ListItem Text="Đang mở" Value="Đang mở" />
                            <asp:ListItem Text="Tạm ngưng" Value="Tạm ngưng" />
                        </asp:DropDownList>
                    </div>
                    <div class="mb-3">
                        <label class="form-label small fw-bold">Mô tả khóa học</label>
                        <asp:TextBox ID="txtMoTa" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control" />
                    </div>
                    <div class="d-flex gap-2">
                        <asp:Button ID="btnThem" runat="server" Text="Thêm" CssClass="btn btn-orange flex-grow-1" OnClick="btnThem_Click" />
                        <asp:Button ID="btnSua" runat="server" Text="Cập nhật" CssClass="btn btn-primary-gradient flex-grow-1" OnClick="btnSua_Click" />
                        <asp:Button ID="btnLamMoi" runat="server" Text="Làm mới" CssClass="btn btn-secondary" OnClick="btnLamMoi_Click" />
                    </div>
                </div>
            </div>
        </div>

        <!-- DANH SÁCH & TÌM KIẾM GỢI Ý -->
        <div class="col-lg-8">
            <div class="card card-custom">
                <div class="card-header d-flex justify-content-between align-items-center">
                    <span class="text-nowrap"><i class="bi bi-journal-text me-2"></i>Danh sách Khóa học</span>
                    <div class="d-flex gap-2 w-100 justify-content-end">
                        <div class="position-relative flex-grow-1" style="max-width: 260px;">
                            <asp:TextBox ID="txtTim" runat="server" ClientIDMode="Static" TextMode="Search" CssClass="form-control form-control-sm"
                                placeholder="Tìm mã KH, tên khóa học..." autocomplete="off" onclick="this.select();" />
                            <div id="goiYKH" class="list-group shadow position-absolute w-100"
                                style="top: 100%; left: 0; z-index: 1050; max-height: 150px; overflow-y: auto; display: none;">
                                <asp:Literal ID="litGoiYKH" runat="server" />
                            </div>
                        </div>
                        <asp:Button ID="btnTim" runat="server" ClientIDMode="Static" Text="Lọc" CssClass="btn btn-sm btn-primary-gradient" OnClick="btnTim_Click" />
                    </div>
                </div>
                <div class="card-body p-0">
                    <asp:GridView ID="gvKhoaHoc" runat="server" CssClass="table table-hover table-custom mb-0" AutoGenerateColumns="false"
                        AllowPaging="true" PageSize="10" OnPageIndexChanging="gvKhoaHoc_PageIndexChanging"
                        OnRowCommand="gvKhoaHoc_RowCommand" DataKeyNames="makhoahoc"
                        EmptyDataText="Không tìm thấy khóa học nào.">
                        <PagerSettings Mode="NumericFirstLast" FirstPageText="&laquo;" LastPageText="&raquo;" PreviousPageText="&lsaquo;" NextPageText="&rsaquo;" PageButtonCount="5" />
                        <PagerStyle CssClass="pagination-ys" HorizontalAlign="Center" />
                        <Columns>
                            <asp:BoundField DataField="makhoahoc" HeaderText="Mã KH" />
                            <asp:BoundField DataField="tenkhoahoc" HeaderText="Tên khóa học" />
                            <asp:BoundField DataField="hocphi" HeaderText="Học phí" DataFormatString="{0:N0} đ" />
                            <asp:BoundField DataField="thoigianhoc" HeaderText="Thời lượng" />
                            <asp:BoundField DataField="trangthaikhoahoc" HeaderText="Trạng thái" />
                            <asp:TemplateField HeaderText="Thao tác">
                                <ItemTemplate>
                                    <div class="d-flex gap-1">
                                        <asp:LinkButton ID="btnChon" runat="server" CommandName="Chon" CommandArgument='<%# Eval("makhoahoc") %>' CssClass="btn btn-sm btn-outline-primary"><i class="bi bi-pencil"></i></asp:LinkButton>
                                        <button type="button" class="btn btn-sm btn-outline-danger" onclick="hienThiModalXoaKH('<%# Eval("makhoahoc") %>'); return false;"><i class="bi bi-trash"></i></button>
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </div>

    <!-- POPUP XÁC NHẬN XÓA KHÓA HỌC -->
    <div class="modal fade" id="modalXoaKH" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content border-0 shadow">
                <div class="modal-header bg-danger text-white">
                    <h5 class="modal-title fw-bold"><i class="bi bi-exclamation-triangle me-2"></i>Xác nhận Xóa</h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body py-4 text-center">
                    <i class="bi bi-trash display-4 text-danger mb-3 d-block"></i>
                    <p class="fs-5 mb-0">Bạn có chắc chắn muốn xóa khóa học <strong id="maKHCanXoa"></strong> này?</p>
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

    <asp:HiddenField ID="hfMaKHXoa" runat="server" ClientIDMode="Static" />

    <script>
// @ts-nocheck

function chiChoNhapSoVaCham(e) {
    var charCode = (e.which) ? e.which : e.keyCode;
    // Cho phép các phím điều hướng, xóa, backspace, tab
    if (charCode === 8 || charCode === 9 || charCode === 37 || charCode === 39 || charCode === 46) {
        return true;
    }
    var charStr = String.fromCharCode(charCode);
    // Chỉ cho phép số 0-9 và dấu chấm (.)
    if (/^[0-9.]$/.test(charStr)) {
        // Không cho phép nhập nhiều hơn 1 dấu chấm
        if (charStr === '.' && e.target.value.indexOf('.') !== -1) {
            return false;
        }
        return true;
    }
    return false;
}

function chiChoNhapSoVaChamInput(input) {
    // Lọc bỏ mọi ký tự không phải số và dấu chấm
    var val = input.value.replace(/[^0-9.]/g, '');
    // Chỉ giữ lại dấu chấm đầu tiên nếu có nhiều dấu chấm
    var parts = val.split('.');
    if (parts.length > 2) {
        val = parts[0] + '.' + parts.slice(1).join('');
    }
    input.value = val;
}
        function hienThiModalXoaKH(maKH) {
            document.getElementById('hfMaKHXoa').value = maKH;
            document.getElementById('maKHCanXoa').innerText = maKH;
            var el = document.getElementById('modalXoaKH');
            if (el && window.bootstrap) {
                new window.bootstrap.Modal(el).show();
            }
        }

        function thietLapGoiY(inputId, listId, btnId) {
            var input = document.getElementById(inputId);
            var list = document.getElementById(listId);
            var btn = btnId ? document.getElementById(btnId) : null;
            if (!input || !list) return;

            var items = list.querySelectorAll('.goi-y-item');

            function locGoiY() {
                var kw = input.value.trim().toLowerCase();
                var co = false;
                items.forEach(function (item) {
                    if (kw === '' || item.innerText.toLowerCase().indexOf(kw) !== -1) {
                        item.style.display = 'block';
                        co = true;
                    } else {
                        item.style.display = 'none';
                    }
                });
                list.style.display = co ? 'block' : 'none';
            }

            input.addEventListener('input', function () {
                if (this.value.trim() === '' && btn) {
                    list.style.display = 'none';
                    btn.click();
                } else {
                    locGoiY();
                }
            });

            input.addEventListener('focus', function () {
                locGoiY();
            });

            items.forEach(function (item) {
                item.addEventListener('mousedown', function (e) {
                    e.preventDefault();
                    input.value = this.getAttribute('data-value');
                    list.style.display = 'none';
                    if (btn) {
                        btn.click();
                    } else {
                        setTimeout(function () {
                            input.blur();
                        }, 10);
                    }
                });
            });

            document.addEventListener('click', function (e) {
                if (!input.contains(e.target) && !list.contains(e.target)) {
                    list.style.display = 'none';
                }
            });
        }

        window.addEventListener('DOMContentLoaded', function () {
            thietLapGoiY('txtTim', 'goiYKH', 'btnTim');
            thietLapGoiY('txtTenKH', 'goiYTenKH', null);

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

        function dinhDangHocPhi(input) {

            var raw = input.value.replace(/[^0-9]/g, '');
            if (raw === '') {
                input.value = '';
                return;
            }

            raw = raw.replace(/^0+(?=\d)/, '');
            input.value = raw.replace(/\B(?=(\d{3})+(?!\d))/g, ',');
        }
    </script>
</asp:Content>