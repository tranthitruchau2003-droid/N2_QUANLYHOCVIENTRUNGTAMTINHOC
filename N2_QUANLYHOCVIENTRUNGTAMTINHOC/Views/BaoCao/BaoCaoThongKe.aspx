<%@ Page Title="Báo cáo & Thống kê" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeBehind="BaoCaoThongKe.aspx.cs" Inherits="N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.BaoCao.BaoCaoThongKe" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Báo cáo & Thống kê</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <!-- 1. BÁO CÁO HỌC PHÍ CÒN NỢ -->
    <div class="card card-custom mb-4">
        <div class="card-header d-flex justify-content-between align-items-center flex-wrap gap-2">
            <span class="text-nowrap">1. Báo cáo Học phí Còn nợ (Công nợ Học viên)</span>
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

    <!-- 2. THỐNG KÊ SỐ LƯỢNG HỌC VIÊN & DOANH THU THEO KHÓA HỌC -->
    <div class="card card-custom mb-4">
        <div class="card-header d-flex justify-content-between align-items-center flex-wrap gap-2">
            <span>2. Thống kê Số lượng Học viên & Doanh thu theo Khóa học</span>
            <asp:LinkButton ID="btnXuatExcel" runat="server" CssClass="btn btn-sm btn-success fw-semibold" OnClick="btnXuatExcel_Click">
                <i class="bi bi-file-earmark-excel-fill me-1"></i> Xuất Excel
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
        // 1. TÌM KIẾM GỢI Ý CÔNG NỢ
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
        });

        // 2. VẼ BIỂU ĐỒ KHÓA HỌC (SỐ LƯỢNG & SỐ TIỀN)
        const khLabels = <%= JsonKHLabels %>;
        const khSoLuong = <%= JsonKHSoLuong %>;
        const khDoanhThu = <%= JsonKHDoanhThu %>;

        new Chart(document.getElementById('chartKhoaHoc'), {
            type: 'bar',
            data: {
                labels: khLabels,
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

        // 3. VẼ BIỂU ĐỒ DOANH THU THEO THÁNG / NĂM
        const tnLabels = <%= JsonTNLabels %>;
        const tnDoanhThu = <%= JsonTNDoanhThu %>;

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