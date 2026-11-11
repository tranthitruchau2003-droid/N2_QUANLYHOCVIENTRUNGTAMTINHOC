<%@ Page Title="Quản lý Phiếu thu" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeBehind="QuanLyPhieuThu.aspx.cs" Inherits="N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.PhieuThu.QuanLyPhieuThu" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Quản lý Phiếu thu</asp:Content>
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
    <!-- THÔNG BÁO NỔI TỰ ẨN SAU 5S -->
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
        <!-- FORM LẬP / SỬA PHIẾU THU -->
        <div class="col-lg-4">
            <div class="card card-custom">
                <div class="card-header"><span><i class="bi bi-cash-coin me-2"></i>Lập Phiếu thu Học phí</span></div>
                <div class="card-body">
                    <div class="mb-2">
                        <label class="small fw-bold">Mã phiếu thu (*)</label>
                        <asp:TextBox ID="txtMaPT" runat="server" CssClass="form-control" ReadOnly="true" placeholder="Hệ thống tự động tạo..." />
                    </div>
                    <div class="mb-2">
                        <label class="small fw-bold">Hồ sơ Đăng ký (*)</label>
                        <asp:DropDownList ID="ddlDangKy" runat="server" CssClass="form-select" />
                    </div>
                    <div class="mb-2">
                        <label class="small fw-bold">Số tiền thu (VNĐ) (*)</label>
                        <asp:TextBox ID="txtSoTien" runat="server" ClientIDMode="Static" CssClass="form-control"
                            autocomplete="off" placeholder="VD: 1,500,000" oninput="dinhDangSoTienPT(this);" />
                    </div>
                    <div class="mb-2">
                        <label class="small fw-bold">Phương thức thanh toán</label>
                        <asp:DropDownList ID="ddlPhuongThuc" runat="server" CssClass="form-select">
                            <asp:ListItem Text="Tiền mặt" Value="Tiền mặt" />
                            <asp:ListItem Text="Chuyển khoản Ngân hàng" Value="Chuyển khoản" />
                        </asp:DropDownList>
                    </div>
                    <div class="mb-3">
                        <label class="small fw-bold">Nội dung thu</label>
                        <asp:TextBox ID="txtNoiDung" runat="server" CssClass="form-control" autocomplete="new-password" placeholder="VD: Thu học phí đợt 1..." />
                    </div>
                    <div class="d-flex gap-2">
                        <asp:Button ID="btnLapPhieu" runat="server" Text="Lập phiếu" CssClass="btn btn-orange flex-grow-1" OnClick="btnLapPhieu_Click" />
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
                    <span class="text-nowrap"><i class="bi bi-receipt me-2"></i>Danh sách Phiếu thu đã lập</span>
                    <div class="d-flex gap-2 w-100 justify-content-end">
                        <div class="position-relative flex-grow-1" style="max-width: 260px;">
                            <asp:TextBox ID="txtTim" runat="server" ClientIDMode="Static" TextMode="Search" CssClass="form-control form-control-sm"
                                placeholder="Tìm số PT, tên học viên..." autocomplete="off" onclick="this.select();" />
                            <div id="goiYPT" class="list-group shadow position-absolute w-100"
                                style="top: 100%; left: 0; z-index: 1050; max-height: 150px; overflow-y: auto; display: none;">
                                <asp:Literal ID="litGoiYPT" runat="server" />
                            </div>
                        </div>
                        <asp:Button ID="btnTim" runat="server" ClientIDMode="Static" Text="Tìm" CssClass="btn btn-sm btn-primary-gradient" OnClick="btnTim_Click" />
                    </div>
                </div>
                <div class="card-body p-0">
                    <asp:GridView ID="gvPhieuThu" runat="server" CssClass="table table-hover table-custom mb-0" AutoGenerateColumns="false"
                        AllowPaging="true" PageSize="10" OnPageIndexChanging="gvPhieuThu_PageIndexChanging"
                        OnRowCommand="gvPhieuThu_RowCommand" DataKeyNames="maphieuthu"
                        EmptyDataText="Không tìm thấy phiếu thu nào.">
                        <PagerSettings Mode="NumericFirstLast" FirstPageText="&laquo;" LastPageText="&raquo;" PreviousPageText="&lsaquo;" NextPageText="&rsaquo;" PageButtonCount="5" />
                        <PagerStyle CssClass="pagination-ys" HorizontalAlign="Center" />
                        <Columns>
                            <asp:BoundField DataField="maphieuthu" HeaderText="Số PT" />
                            <asp:BoundField DataField="ngaythu" HeaderText="Ngày thu" DataFormatString="{0:dd/MM/yyyy}" />
                            <asp:BoundField DataField="hoten" HeaderText="Học viên" />
                            <asp:BoundField DataField="sotienthu" HeaderText="Số tiền" DataFormatString="{0:N0} đ" />
                            <asp:BoundField DataField="phuongthucthanhtoan" HeaderText="Hình thức" />
                            <asp:BoundField DataField="nguoithu" HeaderText="Người thu" />
                            <asp:TemplateField HeaderText="Thao tác">
                                <ItemTemplate>
                                    <div class="d-flex gap-1">
                                        <asp:LinkButton ID="btnChon" runat="server" CommandName="Chon" CommandArgument='<%# Eval("maphieuthu") %>'
                                            CssClass="btn btn-sm btn-outline-primary" title="Sửa"><i class="bi bi-pencil"></i></asp:LinkButton>
                                        <button type="button" class="btn btn-sm btn-outline-danger" title="Xóa"
                                            onclick="hienThiModalXoaPT('<%# Eval("maphieuthu") %>'); return false;"><i class="bi bi-trash"></i></button>
                                        <button type="button" class="btn btn-sm btn-outline-secondary" title="In phiếu"
                                            onclick="inPhieu('<%# Eval("maphieuthu") %>','<%# Eval("hoten") %>','<%# Eval("sotienthu","{0:N0}") %>','<%# Eval("noidungthu") %>')"><i class="bi bi-printer"></i></button>
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </div>

    <!-- POPUP XÁC NHẬN XÓA PHIẾU THU -->
    <div class="modal fade" id="modalXoaPT" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content border-0 shadow">
                <div class="modal-header bg-danger text-white">
                    <h5 class="modal-title fw-bold"><i class="bi bi-exclamation-triangle me-2"></i>Xác nhận Xóa</h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body py-4 text-center">
                    <i class="bi bi-trash display-4 text-danger mb-3 d-block"></i>
                    <p class="fs-5 mb-0">Bạn có chắc chắn muốn xóa phiếu thu <strong id="maPTCanXoa"></strong> này?</p>
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

    <asp:HiddenField ID="hfMaPTXoa" runat="server" ClientIDMode="Static" />

    <script>
        // @ts-nocheck
        function dinhDangSoTienPT(input) {
            var raw = input.value.replace(/[^0-9]/g, '');
            if (raw === '') {
                input.value = '';
                return;
            }
            raw = raw.replace(/^0+(?=\d)/, '');
            input.value = raw.replace(/\B(?=(\d{3})+(?!\d))/g, ',');
        }

        function hienThiModalXoaPT(maPT) {
            document.getElementById('hfMaPTXoa').value = maPT;
            document.getElementById('maPTCanXoa').innerText = maPT;
            var el = document.getElementById('modalXoaPT');
            if (el && window.bootstrap) {
                new window.bootstrap.Modal(el).show();
            }
        }

        function thietLapGoiYPT() {
            var input = document.getElementById('txtTim');
            var list = document.getElementById('goiYPT');
            var btn = document.getElementById('btnTim');
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

        window.addEventListener('DOMContentLoaded', function () {
            thietLapGoiYPT();

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

        function inPhieu(ma, hv, tien, nd) {
            const w = window.open('', '_blank', 'width=700,height=500');
            w.document.write(`<html><head><title>Phiếu thu ${ma}</title><link href="/Content/bootstrap.min.css" rel="stylesheet"/></head><body class="p-4">
                <div class="border p-4 rounded">
                    <h4 class="text-center fw-bold text-primary">PHIẾU THU HỌC PHÍ (${ma})</h4>
                    <hr/>
                    <p><strong>Họ tên học viên:</strong> ${hv}</p>
                    <p><strong>Số tiền thu:</strong> <span class="text-danger fw-bold">${tien} VNĐ</span></p>
                    <p><strong>Nội dung:</strong> ${nd}</p>
                    <p><strong>Ngày in:</strong> ${new Date().toLocaleDateString('vi-VN')}</p>
                    <div class="row mt-5 text-center"><div class="col-6"><strong>Người nộp tiền</strong></div><div class="col-6"><strong>Người lập phiếu</strong></div></div>
                </div>
                <script>window.print();<\/script></body></html>`);
            w.document.close();
        }
    </script>
</asp:Content>