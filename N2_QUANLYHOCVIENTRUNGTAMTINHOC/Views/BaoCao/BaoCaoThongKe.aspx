<%@ Page Title="Báo cáo & Thống kê" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeBehind="BaoCaoThongKe.aspx.cs" Inherits="N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.BaoCao.BaoCaoThongKe" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Báo cáo & Thống kê</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
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
    <!-- Thẻ ẩn để hứng tính năng tự điền (autofill) của trình duyệt -->
    <div style="position: absolute; left: -9999px; top: -9999px; opacity: 0; height: 0; overflow: hidden;">
        <input type="text" name="fake_username_autofill" tabindex="-1" />
        <input type="password" name="fake_password_autofill" tabindex="-1" />
    </div>

    <!-- 1. TRA CỨU & XUẤT DANH SÁCH HỌC VIÊN THEO KHÓA, LỚP, HỌC PHÍ -->
    <div class="card card-custom mb-4">
        <div class="card-header d-flex justify-content-between align-items-center flex-wrap gap-2">
            <span><i class="bi bi-people-fill text-primary me-2"></i>1. Danh sách Học viên theo Khóa học, Lớp học & Tình trạng Học phí</span>
            <asp:LinkButton ID="btnXuatDSHocVien" runat="server" CssClass="btn btn-sm btn-success fw-semibold" OnClick="btnXuatDSHocVien_Click">
                <i class="bi bi-file-earmark-excel-fill me-1"></i> Xuất Excel Danh sách Học viên
            </asp:LinkButton>
        </div>
        <div class="card-body">
            <div class="row g-2 mb-3">
                <div class="col-md-3">
                    <label class="small fw-bold">Lọc theo Khóa học</label>
                    <asp:DropDownList ID="ddlLocKhoaHoc" runat="server" CssClass="form-select form-select-sm"
                        AutoPostBack="true" OnSelectedIndexChanged="ddlLocKhoaHoc_SelectedIndexChanged" />
                </div>
                <div class="col-md-3">
                    <label class="small fw-bold">Lọc theo Lớp học</label>
                    <asp:DropDownList ID="ddlLocLopHoc" runat="server" CssClass="form-select form-select-sm"
                        AutoPostBack="true" OnSelectedIndexChanged="BoLocHocVien_Changed" />
                </div>
                <div class="col-md-3">
                    <label class="small fw-bold">Trạng thái Học phí</label>
                    <asp:DropDownList ID="ddlLocTrangThaiHP" runat="server" CssClass="form-select form-select-sm"
                        AutoPostBack="true" OnSelectedIndexChanged="BoLocHocVien_Changed">
                        <asp:ListItem Text="-- Tất cả trạng thái --" Value="" />
                        <asp:ListItem Text="Đã đóng đủ học phí" Value="DaDongDu" />
                        <asp:ListItem Text="Còn nợ một phần" Value="ConNoMotPhan" />
                        <asp:ListItem Text="Chưa đóng học phí" Value="ChuaDong" />
                        <asp:ListItem Text="Đã hủy lớp (0 đ)" Value="DaHuyLop" />
                        <asp:ListItem Text="Chưa hoàn tiền (Hủy lớp)" Value="ChuaHoanTien" />
                        <asp:ListItem Text="Chờ hoàn tiền (Đã gửi STK)" Value="ChoHoanTien" />
                        <asp:ListItem Text="Đã hoàn tiền học phí" Value="DaHoanTien" />
                    </asp:DropDownList>
                </div>
                <div class="col-md-3">
                    <label class="small fw-bold">Tìm nhanh học viên</label>
                    <div class="d-flex gap-1">
                        <div class="position-relative flex-grow-1">
                            <asp:TextBox ID="txtTimHVLoc" runat="server" ClientIDMode="Static" TextMode="Search" CssClass="form-control form-control-sm"
                                placeholder="Mã HV, Họ tên, SĐT..." autocomplete="off" onclick="this.select();" />
                            <!-- Khung gợi ý xổ xuống ngay dưới ô tìm kiếm -->
                            <div id="goiYTimKiem" class="list-group shadow position-absolute w-100"
                                style="top: 100%; left: 0; z-index: 1050; max-height: 150px; overflow-y: auto; display: none;">
                                <asp:Literal ID="litGoiY" runat="server" />
                            </div>
                        </div>
                        <asp:Button ID="btnLocHV" runat="server" ClientIDMode="Static" Text="Lọc" CssClass="btn btn-sm btn-primary-gradient" OnClick="BoLocHocVien_Changed" />
                    </div>
                </div>
            </div>

            <div class="table-responsive">
                <asp:GridView ID="gvDSHocVienLoc" runat="server" CssClass="table table-sm table-bordered table-hover table-custom mb-0"
                    AutoGenerateColumns="false" AllowPaging="true" PageSize="10" OnPageIndexChanging="gvDSHocVienLoc_PageIndexChanging"
                    EmptyDataText="Không tìm thấy học viên nào phù hợp.">
                    <PagerSettings Mode="NumericFirstLast" FirstPageText="&laquo;" LastPageText="&raquo;" PreviousPageText="&lsaquo;" NextPageText="&rsaquo;" PageButtonCount="5" />
                    <PagerStyle CssClass="pagination-ys" HorizontalAlign="Center" />
                    <Columns>
                        <asp:BoundField DataField="madangky" HeaderText="Mã ĐK" />
                        <asp:BoundField DataField="mahocvien" HeaderText="Mã HV" />
                        <asp:TemplateField HeaderText="Họ tên Học viên">
                            <ItemTemplate>
                                <a href='<%# (Eval("trangthaihp").ToString().Contains("Chưa hoàn tiền") || Eval("trangthaihp").ToString().Contains("Chờ hoàn tiền")) 
                                        ? ResolveUrl("~/Views/DangKy/QuanLyDangKy.aspx?madk=" + Eval("madangky")) 
                                        : ResolveUrl("~/Views/PhieuThu/QuanLyPhieuThu.aspx?madk=" + Eval("madangky")) %>'
                                   class='<%# (Eval("trangthaihp").ToString().Contains("Chưa hoàn tiền") || Eval("trangthaihp").ToString().Contains("Chờ hoàn tiền")) 
                                        ? "fw-bold text-danger text-decoration-none" 
                                        : "fw-bold text-primary text-decoration-none" %>'
                                   title='<%# (Eval("trangthaihp").ToString().Contains("Chưa hoàn tiền") || Eval("trangthaihp").ToString().Contains("Chờ hoàn tiền")) 
                                        ? "Chuyển sang trang Đăng ký để xử lý hoàn tiền" 
                                        : "Chuyển sang lập Phiếu thu" %>'>
                                    <%# Eval("hoten") %>
                                </a>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="dienthoai" HeaderText="Điện thoại" />
                        <asp:BoundField DataField="tenkhoahoc" HeaderText="Khóa học" />
                        <asp:BoundField DataField="tenlop" HeaderText="Lớp học" />
                        <asp:BoundField DataField="hocphi" HeaderText="Học phí" DataFormatString="{0:N0} đ" ItemStyle-CssClass="text-nowrap" />
                        <asp:BoundField DataField="dadong" HeaderText="Đã đóng" DataFormatString="{0:N0} đ" ItemStyle-CssClass="text-success fw-semibold text-nowrap" />
                        <asp:BoundField DataField="conno" HeaderText="Còn nợ" DataFormatString="{0:N0} đ" ItemStyle-CssClass="text-danger fw-bold text-nowrap" />
                        <asp:BoundField DataField="trangthaihp" HeaderText="Trạng thái" />
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </div>

    <!-- 2. THỐNG KÊ SỐ LƯỢNG HỌC VIÊN & DOANH THU THEO KHÓA HỌC & NĂM -->
    <div class="card card-custom mb-4">
        <div class="card-header d-flex justify-content-between align-items-center flex-wrap gap-2">
            <span><i class="bi bi-bar-chart-fill text-primary me-2"></i>2. Thống kê Số lượng Học viên & Doanh thu theo Khóa học</span>
            <div class="d-flex flex-wrap gap-2 align-items-center">
                <asp:DropDownList ID="ddlNamThongKe" runat="server" CssClass="form-select form-select-sm" style="width: 130px;"
                    AutoPostBack="true" OnSelectedIndexChanged="BoLocThongKe_Changed" />
                <asp:DropDownList ID="ddlKhoaThongKe" runat="server" CssClass="form-select form-select-sm" style="width: 220px;"
                    AutoPostBack="true" OnSelectedIndexChanged="BoLocThongKe_Changed" />
                <asp:LinkButton ID="btnXuatExcel" runat="server" CssClass="btn btn-sm btn-success fw-semibold text-nowrap" OnClick="btnXuatExcel_Click">
                    <i class="bi bi-file-earmark-excel-fill me-1"></i> Xuất Excel Thống kê
                </asp:LinkButton>
            </div>
        </div>
        <div class="card-body p-0">
            <div class="table-responsive">
                <asp:GridView ID="gvThongKeKH" runat="server" CssClass="table table-hover table-custom mb-0" AutoGenerateColumns="false"
                    EmptyDataText="Không có dữ liệu thống kê.">
                    <Columns>
                        <asp:BoundField DataField="makhoahoc" HeaderText="Mã KH" />
                        <asp:BoundField DataField="tenkhoahoc" HeaderText="Tên khóa học" />
                        <asp:BoundField DataField="soluongdk" HeaderText="Số lượng ĐK" ItemStyle-CssClass="fw-bold text-primary" />
                        <asp:BoundField DataField="tongthucthu" HeaderText="Tổng thực thu" DataFormatString="{0:N0} đ" ItemStyle-CssClass="fw-bold text-success" />
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </div>

    <!-- 3. BIỂU ĐỒ TRỰC QUAN -->
    <div class="row g-4">
        <div class="col-lg-6">
            <div class="card card-custom h-100">
                <div class="card-header">
                    <span><i class="bi bi-graph-up me-2"></i>Biểu đồ Khóa học (Số lượng ĐK & Doanh thu)</span>
                </div>
                <div class="card-body">
                    <canvas id="chartKhoaHoc"></canvas>
                </div>
            </div>
        </div>
        <div class="col-lg-6">
            <div class="card card-custom h-100">
                <div class="card-header d-flex justify-content-between align-items-center">
                    <span><i class="bi bi-calendar3 me-2"></i>Biểu đồ Doanh thu theo Tháng / Năm</span>
                    <asp:DropDownList ID="ddlNam" runat="server" CssClass="form-select form-select-sm" style="width: 110px;"
                        AutoPostBack="true" OnSelectedIndexChanged="ddlNam_SelectedIndexChanged" />
                </div>
                <div class="card-body">
                    <canvas id="chartThangNam"></canvas>
                </div>
            </div>
        </div>
    </div>

    <script>
        // @ts-nocheck
        // Lưu vị trí cuộn trang và ID ô đang tìm kiếm để khôi phục focus sau khi postback
        window.addEventListener('beforeunload', function () {
            sessionStorage.setItem('scrollPos_BaoCao', window.scrollY);
            var activeEl = document.activeElement;
            if (activeEl && activeEl.id) {
                sessionStorage.setItem('focusSearchBC', activeEl.id);
            }
        });

        window.addEventListener('load', function () {
            var savedScroll = sessionStorage.getItem('scrollPos_BaoCao');
            if (savedScroll !== null) {
                window.scrollTo({ top: parseInt(savedScroll, 10), behavior: 'instant' });
                sessionStorage.removeItem('scrollPos_BaoCao');
            }

            var focusId = sessionStorage.getItem('focusSearchBC');
            if (focusId) {
                var el = document.getElementById(focusId);
                if (el) {
                    el.focus();
                    if (typeof el.selectionStart === 'number') {
                        el.selectionStart = el.selectionEnd = el.value.length;
                    }
                }
                sessionStorage.removeItem('focusSearchBC');
            }
        });

        window.addEventListener('DOMContentLoaded', function () {
            var txtTimHVLoc = document.getElementById('txtTimHVLoc');
            var btnLocHV = document.getElementById('btnLocHV');
            var listGoiY = document.getElementById('goiYTimKiem');

            if (txtTimHVLoc && btnLocHV) {
                var items = listGoiY ? listGoiY.querySelectorAll('.goi-y-item') : [];

                function locGoiY() {
                    var kw = txtTimHVLoc.value.trim().toLowerCase();
                    var co = false;
                    items.forEach(function (item) {
                        if (kw !== '' && item.innerText.toLowerCase().indexOf(kw) !== -1) {
                            item.style.display = 'block';
                            co = true;
                        } else {
                            item.style.display = 'none';
                        }
                    });
                    if (listGoiY) listGoiY.style.display = co ? 'block' : 'none';
                }

                // Khi gõ phím: Xóa hết chữ thì tự động lọc lại bảng, gõ có chữ thì hiển thị gợi ý
                txtTimHVLoc.addEventListener('input', function () {
                    sessionStorage.setItem('focusSearchBC', 'txtTimHVLoc');
                    if (this.value.trim() === '') {
                        if (listGoiY) listGoiY.style.display = 'none';
                        btnLocHV.click();
                    } else {
                        locGoiY();
                    }
                });

                // Hỗ trợ bấm phím Enter để tìm kiếm ngay lập tức
                txtTimHVLoc.addEventListener('keydown', function (e) {
                    if (e.key === 'Enter') {
                        e.preventDefault();
                        if (listGoiY) listGoiY.style.display = 'none';
                        sessionStorage.setItem('focusSearchBC', 'txtTimHVLoc');
                        btnLocHV.click();
                    }
                });

                // Khi click chọn một dòng gợi ý
                items.forEach(function (item) {
                    item.addEventListener('mousedown', function (e) {
                        e.preventDefault();
                        txtTimHVLoc.value = this.getAttribute('data-value');
                        if (listGoiY) listGoiY.style.display = 'none';
                        sessionStorage.setItem('focusSearchBC', 'txtTimHVLoc');
                        btnLocHV.click();
                    });
                });

                // Ẩn bảng gợi ý khi bấm ra ngoài
                document.addEventListener('click', function (e) {
                    if (!txtTimHVLoc.contains(e.target) && listGoiY && !listGoiY.contains(e.target)) {
                        listGoiY.style.display = 'none';
                    }
                });
            }
        });

        var bcKhLabels = JSON.parse('<%= JsonKHLabels %>');
        var khSoLuong = JSON.parse('<%= JsonKHSoLuong %>');
        var khDoanhThu = JSON.parse('<%= JsonKHDoanhThu %>');

        new Chart(document.getElementById('chartKhoaHoc'), {
            type: 'bar',
            data: {
                labels: bcKhLabels,
                datasets: [
                    {
                        label: 'Doanh thu (VNĐ)',
                        data: khDoanhThu,
                        backgroundColor: 'rgba(22, 119, 210, 0.8)',
                        yAxisID: 'y',
                        order: 2
                    },
                    {
                        label: 'Số lượng ĐK',
                        data: khSoLuong,
                        type: 'line',
                        borderColor: '#FF8A26',
                        backgroundColor: '#FF8A26',
                        borderWidth: 2,
                        tension: 0.3,
                        yAxisID: 'y1',
                        order: 1
                    }
                ]
            },
            options: {
                responsive: true,
                scales: {
                    y: { type: 'linear', position: 'left', beginAtZero: true, title: { display: true, text: 'VNĐ' } },
                    y1: { type: 'linear', position: 'right', beginAtZero: true, grid: { drawOnChartArea: false }, title: { display: true, text: 'Học viên' } }
                }
            }
        });

        var tnLabels = JSON.parse('<%= JsonTNLabels %>');
        var tnDoanhThu = JSON.parse('<%= JsonTNDoanhThu %>');

        new Chart(document.getElementById('chartThangNam'), {
            type: 'bar',
            data: {
                labels: tnLabels,
                datasets: [{
                    label: 'Doanh thu (VNĐ)',
                    data: tnDoanhThu,
                    backgroundColor: '#28A745',
                    borderRadius: 5
                }]
            },
            options: {
                responsive: true,
                scales: {
                    y: { beginAtZero: true }
                }
            }
        });
    </script>
</asp:Content>