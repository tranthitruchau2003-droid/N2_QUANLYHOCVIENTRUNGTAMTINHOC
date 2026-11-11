<%@ Page Title="Cấp Chứng chỉ" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeBehind="QuanLyChungChi.aspx.cs" Inherits="N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.ChungChi.QuanLyChungChi" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Chứng chỉ Đào tạo</asp:Content>
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
        <!-- FORM CẤP CHỨNG CHỈ (ẨN HOÀN TOÀN KHI LÀ HỌC VIÊN) -->
        <div class="col-lg-4" id="divFormCapCC" runat="server">
            <div class="card card-custom">
                <div class="card-header"><span><i class="bi bi-award-fill me-2"></i>Cấp Chứng chỉ Đào tạo</span></div>
                <div class="card-body">
                    <div class="mb-2">
                        <label class="small fw-bold">Mã chứng chỉ (*)</label>
                        <asp:TextBox ID="txtMaCC" runat="server" CssClass="form-control" ReadOnly="true" placeholder="Hệ thống tự động tạo..." />
                    </div>

                    <!-- BỘ LỌC HỌC VIÊN THEO KHÓA HỌC VÀ NĂM -->
                    <div class="row g-2 mb-2">
                        <div class="col-7">
                            <label class="small fw-bold text-primary"><i class="bi bi-funnel me-1"></i>Lọc theo Khóa học</label>
                            <asp:DropDownList ID="ddlLocKhoaHoc" runat="server" CssClass="form-select form-select-sm"
                                AutoPostBack="true" OnSelectedIndexChanged="ddlLocForm_SelectedIndexChanged" />
                        </div>
                        <div class="col-5">
                            <label class="small fw-bold text-primary"><i class="bi bi-calendar3 me-1"></i>Năm</label>
                            <asp:DropDownList ID="ddlLocNam" runat="server" CssClass="form-select form-select-sm"
                                AutoPostBack="true" OnSelectedIndexChanged="ddlLocForm_SelectedIndexChanged" />
                        </div>
                    </div>

                    <div class="mb-2">
                        <label class="small fw-bold">Học viên đủ điều kiện (Đạt) (*)</label>
                        <asp:DropDownList ID="ddlThamGia" runat="server" CssClass="form-select"
                            AutoPostBack="true" OnSelectedIndexChanged="ddlThamGia_SelectedIndexChanged" />
                    </div>
                    <div class="mb-2">
                        <label class="small fw-bold">Số hiệu chứng chỉ (*)</label>
                        <asp:TextBox ID="txtSoHieu" runat="server" CssClass="form-control" ReadOnly="true" placeholder="Hệ thống tự động tạo..." />
                    </div>
                    <div class="row mb-3">
                        <div class="col-6">
                            <label class="small fw-bold">Xếp loại</label>
                            <asp:DropDownList ID="ddlXepLoai" runat="server" CssClass="form-select">
                                <asp:ListItem Text="Xuất sắc" Value="Xuất sắc" />
                                <asp:ListItem Text="Giỏi" Value="Giỏi" />
                                <asp:ListItem Text="Khá" Value="Khá" />
                                <asp:ListItem Text="Trung bình" Value="Trung bình" />
                            </asp:DropDownList>
                        </div>
                        <div class="col-6">
                            <label class="small fw-bold">Ngày cấp</label>
                            <asp:TextBox ID="txtNgayCap" runat="server" TextMode="Date" CssClass="form-control" />
                        </div>
                    </div>
                    <div class="d-flex gap-2">
                        <asp:Button ID="btnCapCC" runat="server" Text="Cấp Chứng chỉ" CssClass="btn btn-orange flex-grow-1" OnClick="btnCapCC_Click" />
                        <asp:Button ID="btnCapNhatCC" runat="server" Text="Cập nhật" CssClass="btn btn-primary-gradient flex-grow-1" OnClick="btnCapNhatCC_Click" />
                        <asp:Button ID="btnLamMoiCC" runat="server" Text="Làm mới" CssClass="btn btn-secondary" OnClick="btnLamMoiCC_Click" />
                    </div>
                </div>
            </div>
        </div>

        <!-- DANH SÁCH CHỨNG CHỈ -->
        <div class="col-lg-8" id="divDanhSachCC" runat="server">
            <div class="card card-custom">
                <div class="card-header d-flex justify-content-between align-items-center flex-wrap gap-2">
                    <span class="text-nowrap"><i class="bi bi-award me-2"></i><asp:Literal ID="litTieuDeCC" runat="server" Text="Danh sách Chứng chỉ đã cấp" /></span>
                    <asp:Panel ID="pnlTimKiemCC" runat="server" CssClass="d-flex gap-2">
                        <div class="position-relative" style="width: 220px;">
                            <asp:TextBox ID="txtTimCC" runat="server" ClientIDMode="Static" TextMode="Search" CssClass="form-control form-control-sm"
                                placeholder="Số hiệu, tên HV, khóa học..." autocomplete="off" onclick="this.select();" />
                            <div id="goiYCC" class="list-group shadow position-absolute w-100"
                                style="top: 100%; left: 0; z-index: 1050; max-height: 150px; overflow-y: auto; display: none;">
                                <asp:Literal ID="litGoiYCC" runat="server" />
                            </div>
                        </div>
                        <asp:DropDownList ID="ddlLocXepLoai" runat="server" CssClass="form-select form-select-sm" style="width: 140px;"
                            AutoPostBack="true" OnSelectedIndexChanged="btnTimCC_Click">
                            <asp:ListItem Text="-- Xếp loại --" Value="" />
                            <asp:ListItem Text="Xuất sắc" Value="Xuất sắc" />
                            <asp:ListItem Text="Giỏi" Value="Giỏi" />
                            <asp:ListItem Text="Khá" Value="Khá" />
                            <asp:ListItem Text="Trung bình" Value="Trung bình" />
                        </asp:DropDownList>
                        <asp:Button ID="btnTimCC" runat="server" ClientIDMode="Static" Text="Tìm" CssClass="btn btn-sm btn-primary-gradient" OnClick="btnTimCC_Click" />
                    </asp:Panel>
                </div>
                <div class="card-body p-0">
                    <asp:GridView ID="gvChungChi" runat="server" CssClass="table table-sm table-hover table-custom mb-0"
                        AutoGenerateColumns="false" DataKeyNames="machungchi"
                        AllowPaging="true" PageSize="10" OnPageIndexChanging="gvChungChi_PageIndexChanging"
                        OnRowCommand="gvChungChi_RowCommand"
                        EmptyDataText="Bạn chưa có chứng chỉ nào được cấp.">
                        <PagerSettings Mode="NumericFirstLast" FirstPageText="&laquo;" LastPageText="&raquo;" PreviousPageText="&lsaquo;" NextPageText="&rsaquo;" PageButtonCount="5" />
                        <PagerStyle CssClass="pagination-ys" HorizontalAlign="Center" />
                        <Columns>
                            <asp:BoundField DataField="sohieuchungchi" HeaderText="Số hiệu" />
                            <asp:BoundField DataField="hoten" HeaderText="Họ tên Học viên" />
                            <asp:BoundField DataField="tenkhoahoc" HeaderText="Khóa học" />
                            <asp:BoundField DataField="diem" HeaderText="Điểm" />
                            <asp:BoundField DataField="xeploai" HeaderText="Xếp loại" />
                            <asp:BoundField DataField="ngaycap" HeaderText="Ngày cấp" DataFormatString="{0:dd/MM/yyyy}" />
                            <asp:TemplateField HeaderText="Tải / In">
                                <ItemTemplate>
                                    <div class="d-flex gap-1 justify-content-center">
                                        <button type="button" class="btn btn-sm btn-outline-success" title="Tải / In chứng chỉ"
                                            onclick="inChungChi('<%# Eval("sohieuchungchi") %>', '<%# Eval("hoten") %>', '<%# Eval("tenkhoahoc") %>', '<%# Eval("nienkhoa") %>', '<%# Eval("xeploai") %>', '<%# Eval("ngaycap", "{0:dd/MM/yyyy}") %>'); return false;">
                                            <i class="bi bi-printer"></i>
                                        </button>
                                        <asp:LinkButton ID="btnChonCC" runat="server" CommandName="ChonCC" CommandArgument='<%# Eval("machungchi") %>'
                                            Visible='<%# Session["Quyen"] != null && Session["Quyen"].ToString() != "HocVien" %>'
                                            CssClass="btn btn-sm btn-outline-primary" title="Sửa"><i class="bi bi-pencil"></i></asp:LinkButton>
                                        <button type="button" class="btn btn-sm btn-outline-danger" title="Xóa"
                                            style='<%# Session["Quyen"] != null && Session["Quyen"].ToString() == "HocVien" ? "display:none;" : "" %>'
                                            onclick="hienThiModalXoaCC('<%# Eval("machungchi") %>'); return false;"><i class="bi bi-trash"></i></button>
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </div>

    <!-- POPUP XÁC NHẬN XÓA CHỨNG CHỈ -->
    <div class="modal fade" id="modalXoaCC" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content border-0 shadow">
                <div class="modal-header bg-danger text-white">
                    <h5 class="modal-title fw-bold"><i class="bi bi-exclamation-triangle me-2"></i>Xác nhận Xóa Chứng chỉ</h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body py-4 text-center">
                    <i class="bi bi-trash display-4 text-danger mb-3 d-block"></i>
                    <p class="fs-5 mb-0">Bạn có chắc chắn muốn xóa chứng chỉ <strong id="maCCCanXoa"></strong> này?</p>
                </div>
                <div class="modal-footer justify-content-center border-0 pb-4">
                    <button type="button" class="btn btn-secondary px-4" data-bs-dismiss="modal">Hủy</button>
                    <asp:LinkButton ID="btnXoaCCChinhThuc" runat="server" ClientIDMode="Static" CssClass="btn btn-danger px-4 d-none" OnClick="btnXoaCCChinhThuc_Click">Đồng ý</asp:LinkButton>
                    <button type="button" class="btn btn-danger px-4" onclick="document.getElementById('btnXoaCCChinhThuc').click();">
                        <i class="bi bi-check-lg me-1"></i> Đồng ý
                    </button>
                </div>
            </div>
        </div>
    </div>

    <asp:HiddenField ID="hfMaCCXoa" runat="server" ClientIDMode="Static" />

    <script>
        // @ts-nocheck
        function hienThiModalXoaCC(maCC) {
            document.getElementById('hfMaCCXoa').value = maCC;
            document.getElementById('maCCCanXoa').innerText = maCC;
            var modalXoa = new window.bootstrap.Modal(document.getElementById('modalXoaCC'));
            modalXoa.show();
        }

        function thietLapGoiYCC() {
            var input = document.getElementById('txtTimCC');
            var list = document.getElementById('goiYCC');
            var btn = document.getElementById('btnTimCC');
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
            thietLapGoiYCC();

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

        function inChungChi(soHieu, hoTen, khoaHoc, nienKhoa, xepLoai, ngayCap) {
            var w = window.open('', '_blank', 'width=900,height=650');
            var html = '<!DOCTYPE html><html><head><title>Chứng chỉ ' + soHieu + '</title>' +
                '<style>' +
                'body { font-family: "Times New Roman", serif; margin: 0; padding: 30px; background: #fff; }' +
                '.khung { border: 6px double #1677D2; padding: 40px 50px; text-align: center; position: relative; max-width: 780px; margin: auto; }' +
                '.ten-tt { font-size: 16px; font-weight: bold; color: #0755A6; text-transform: uppercase; margin-bottom: 5px; }' +
                '.quoc-hieu { font-size: 18px; font-weight: bold; margin: 0; }' +
                '.tieu-ngu { font-size: 15px; font-weight: bold; margin: 4px 0 25px 0; text-decoration: underline; }' +
                '.tieu-de { font-size: 28px; font-weight: bold; color: #1677D2; margin: 15px 0; letter-spacing: 1px; }' +
                '.noi-dung { font-size: 18px; line-height: 1.8; margin: 15px 0; }' +
                '.ho-ten { font-size: 24px; font-weight: bold; text-transform: uppercase; color: #000; }' +
                '.footer-cc { display: flex; justify-content: space-between; align-items: flex-end; margin-top: 40px; padding: 0 20px; }' +
                '.thong-tin-so { text-align: left; font-size: 14px; line-height: 1.6; }' +
                '.ky-ten { text-align: center; position: relative; min-width: 220px; }' +
                '.dau-moc { width: 115px; height: 115px; border: 3px solid #d92525; border-radius: 50%; color: #d92525; ' +
                'display: flex; flex-direction: column; align-items: center; justify-content: center; font-size: 11px; font-weight: bold; ' +
                'text-transform: uppercase; margin: 10px auto 0; transform: rotate(-12deg); opacity: 0.85; }' +
                '</style></head><body>' +
                '<div class="khung">' +
                '<div class="ten-tt">TRUNG TÂM TIN HỌC NGÔI SAO VIỆT</div>' +
                '<p class="quoc-hieu">CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM</p>' +
                '<p class="tieu-ngu">Độc lập - Tự do - Hạnh phúc</p>' +
                '<div class="tieu-de">CHỨNG CHỈ HOÀN THÀNH KHÓA HỌC</div>' +
                '<div class="noi-dung">' +
                'Chứng nhận học viên: <span class="ho-ten">' + hoTen + '</span><br/>' +
                'Đã hoàn thành khóa học: <strong>' + khoaHoc + '</strong><br/>' +
                'Niên khóa: <strong>' + nienKhoa + '</strong> &nbsp;|&nbsp; Xếp loại: <strong>' + xepLoai + '</strong>' +
                '</div>' +
                '<div class="footer-cc">' +
                '<div class="thong-tin-so">Số hiệu: <strong>' + soHieu + '</strong><br/>Ngày cấp: <strong>' + ngayCap + '</strong></div>' +
                '<div class="ky-ten"><strong>GIÁM ĐỐC TRUNG TÂM</strong>' +
                '<div class="dau-moc"><span>TRUNG TÂM TIN HỌC</span><span style="font-size:13px;margin:3px 0;">NGÔI SAO VIỆT</span><span>ĐÃ CẤP</span></div>' +
                '</div></div></div>' +
                '<script>window.onload = function() { window.print(); };<\/script>' +
                '</body></html>';
            w.document.write(html);
            w.document.close();
        }
    </script>
</asp:Content>