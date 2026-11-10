<%@ Page Title="Báo cáo & Thống kê" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeBehind="BaoCaoThongKe.aspx.cs" Inherits="N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.BaoCao.BaoCaoThongKe" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Báo cáo & Thống kê</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <!-- MỤC MỚI: TRA CỨU & XUẤT DANH SÁCH HỌC VIÊN THEO KHÓA, LỚP, HỌC PHÍ -->
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
                        <asp:ListItem Text="Chưa đóng đủ (Còn nợ)" Value="ConNo" />
                        <asp:ListItem Text="Chưa đóng đồng nào (0 đ)" Value="ChuaDong" />
                    </asp:DropDownList>
                </div>
                <div class="col-md-3">
                    <label class="small fw-bold">Tìm nhanh học viên</label>
                    <div class="d-flex gap-1">
                        <asp:TextBox ID="txtTimHVLoc" runat="server" ClientIDMode="Static" CssClass="form-control form-control-sm"
                            placeholder="Mã HV, Họ tên, SĐT..." autocomplete="off" onfocus="this.select();" />
                        <asp:Button ID="btnLocHV" runat="server" ClientIDMode="Static" Text="Lọc" CssClass="btn btn-sm btn-primary-gradient" OnClick="BoLocHocVien_Changed" />
                    </div>
                </div>
            </div>

            <div class="table-responsive">
                <asp:GridView ID="gvDSHocVienLoc" runat="server" CssClass="table table-sm table-bordered table-hover table-custom mb-0" AutoGenerateColumns="false">
                    <Columns>
                        <asp:BoundField DataField="madangky" HeaderText="Mã ĐK" />
                        <asp:BoundField DataField="mahocvien" HeaderText="Mã HV" />
                        <asp:TemplateField HeaderText="Họ tên Học viên">
                            <ItemTemplate>
                                <a href='/Views/PhieuThu/QuanLyPhieuThu.aspx?madk=<%# Eval("madangky") %>'
                                   class="fw-bold text-primary text-decoration-none" title="Chuyển sang lập Phiếu thu">
                                    <%# Eval("hoten") %>
                                </a>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="dienthoai" HeaderText="Điện thoại" />
                        <asp:BoundField DataField="tenkhoahoc" HeaderText="Khóa học" />
                        <asp:BoundField DataField="tenlop" HeaderText="Lớp học" />
                        <asp:BoundField DataField="hocphi" HeaderText="Học phí" DataFormatString="{0:N0} đ" />
                        <asp:BoundField DataField="dadong" HeaderText="Đã đóng" DataFormatString="{0:N0} đ" ItemStyle-CssClass="text-success fw-semibold" />
                        <asp:BoundField DataField="conno" HeaderText="Còn nợ" DataFormatString="{0:N0} đ" ItemStyle-CssClass="text-danger fw-bold" />
                        <asp:BoundField DataField="trangthaihp" HeaderText="Trạng thái" />
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </div>

    <!-- 2. BÁO CÁO HỌC PHÍ CÒN NỢ (CÔNG NỢ HỌC VIÊN) -->
    <div class="card card-custom mb-4">
        <div class="card-header d-flex justify-content-between align-items-center flex-wrap gap-2">
            <span class="text-nowrap"><i class="bi bi-exclamation-circle-fill text-danger me-2"></i>2. Báo cáo Học phí Còn nợ (Công nợ Học viên)</span>
            <div class="d-flex gap-2">
                <div class="position-relative" style="width: 260px;">
                    <asp:TextBox ID="txtTimNo" runat="server" ClientIDMode="Static" CssClass="form-control form-control-sm"
                        placeholder="Tìm mã ĐK, tên học viên, SĐT..." autocomplete="off" onfocus="this.select();" />
                    <div id="goiYNo" class="list-group shadow position-absolute w-100"
                        style="top: 100%; left: 0; z-index: 1050; max-height: 150px; overflow-y: auto; display: none;">
                        <asp:Literal ID="litGoiYNo" runat="server" />
                    </div>
                </div>
                <asp:Button ID="btnTimNo" runat="server" ClientIDMode="Static" Text="Tìm" CssClass="btn btn-sm btn-primary-gradient" OnClick="btnTimNo_Click" />
                <asp:LinkButton ID="btnXuatCongNo" runat="server" CssClass="btn btn-sm btn-outline-success fw-semibold" OnClick="btnXuatCongNo_Click">
                    <i class="bi bi-file-earmark-excel"></i> Xuất Công nợ
                </asp:LinkButton>
            </div>
        </div>
        <div class="card-body p-0">
            <asp:GridView ID="gvCongNo" runat="server" CssClass="table table-hover table-custom mb-0" AutoGenerateColumns="false">
                <Columns>
                    <asp:BoundField DataField="madangky" HeaderText="Mã ĐK" />
                    <asp:TemplateField HeaderText="Học viên">
                        <ItemTemplate>
                            <a href='/Views/PhieuThu/QuanLyPhieuThu.aspx?madk=<%# Eval("madangky") %>'
                               class="fw-bold text-primary text-decoration-none" title="Nhấn để lập phiếu thu cho học viên này">
                                <%# Eval("hoten") %> <i class="bi bi-box-arrow-up-right small ms-1"></i>
                            </a>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="dienthoai" HeaderText="Điện thoại" />
                    <asp:BoundField DataField="tenkhoahoc" HeaderText="Khóa học" />
                    <asp:BoundField DataField="hocphi" HeaderText="Tổng học phí" DataFormatString="{0:N0} đ" />
                    <asp:BoundField DataField="dadong" HeaderText="Đã đóng" DataFormatString="{0:N0} đ" />
                    <asp:BoundField DataField="conno" HeaderText="Còn nợ" DataFormatString="{0:N0} đ" ItemStyle-CssClass="text-danger fw-bold" />
                </Columns>
            </asp:GridView>
        </div>
    </div>

    <!-- 3. THỐNG KÊ SỐ LƯỢNG HỌC VIÊN & DOANH THU THEO KHÓA HỌC -->
    <div class="card card-custom mb-4">
        <div class="card-header d-flex justify-content-between align-items-center flex-wrap gap-2">
            <span><i class="bi bi-bar-chart-fill me-2"></i>3. Thống kê Số lượng Học viên & Doanh thu theo Khóa học</span>
            <asp:LinkButton ID="btnXuatExcel" runat="server" CssClass="btn btn-sm btn-success fw-semibold" OnClick="btnXuatExcel_Click">
                <i class="bi bi-file-earmark-excel-fill me-1"></i> Xuất Excel Thống kê Khóa học
            </asp:LinkButton>
        </div>
        <div class="card-body p-0">
            <asp:GridView ID="gvThongKeKH" runat="server" CssClass="table table-hover table-custom mb-0" AutoGenerateColumns="false">
                <Columns>
                    <asp:BoundField DataField="makhoahoc" HeaderText="Mã KH" />
                    <asp:BoundField DataField="tenkhoahoc" HeaderText="Tên khóa học" />
                    <asp:BoundField DataField="soluongdk" HeaderText="Số lượng ĐK" ItemStyle-CssClass="fw-bold text-primary" />
                    <asp:BoundField DataField="tongthucthu" HeaderText="Tổng thực thu" DataFormatString="{0:N0} đ" ItemStyle-CssClass="fw-bold text-success" />
                </Columns>
            </asp:GridView>
        </div>
    </div>

    <!-- 4. BIỂU ĐỒ TRỰC QUAN -->
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
        window.addEventListener('DOMContentLoaded', function () {
            var focusId = sessionStorage.getItem('focusSearchNo');
            if (focusId) {
                var el = document.getElementById(focusId);
                if (el) el.focus();
                sessionStorage.removeItem('focusSearchNo');
            }

            var input = document.getElementById('txtTimNo');
            var list = document.getElementById('goiYNo');
            var btn = document.getElementById('btnTimNo');
            if (input && list && btn) {
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
                        sessionStorage.setItem('focusSearchNo', 'txtTimNo');
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

            // Tự động lọc khi xóa trắng ô tìm kiếm học viên
            var txtTimHVLoc = document.getElementById('txtTimHVLoc');
            var btnLocHV = document.getElementById('btnLocHV');
            if (txtTimHVLoc && btnLocHV) {
                txtTimHVLoc.addEventListener('input', function () {
                    if (this.value.trim() === '') {
                        sessionStorage.setItem('focusSearchNo', 'txtTimHVLoc');
                        btnLocHV.click();
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