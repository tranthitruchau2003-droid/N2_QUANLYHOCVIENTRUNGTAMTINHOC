<%@ Page Title="Bảng điều khiển" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="N2_QUANLYHOCVIENTRUNGTAMTINHOC.Dashboard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Dashboard</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <!-- 6 THẺ THỐNG KÊ -->
    <div class="row g-3 mb-4">
        <div class="col-md-4 col-lg-2">
            <div class="stat-card blue">
                <div class="small opacity-75">Tổng Học viên</div>
                <h3 class="fw-bold mb-0"><asp:Literal ID="litTongHV" runat="server" /></h3>
            </div>
        </div>
        <div class="col-md-4 col-lg-2">
            <div class="stat-card teal">
                <div class="small opacity-75">Khóa học</div>
                <h3 class="fw-bold mb-0"><asp:Literal ID="litTongKH" runat="server" /></h3>
            </div>
        </div>
        <div class="col-md-4 col-lg-2">
            <div class="stat-card purple">
                <div class="small opacity-75">Lớp học</div>
                <h3 class="fw-bold mb-0"><asp:Literal ID="litTongLop" runat="server" /></h3>
            </div>
        </div>
        <div class="col-md-4 col-lg-2">
            <div class="stat-card blue">
                <div class="small opacity-75">Giảng viên</div>
                <h3 class="fw-bold mb-0"><asp:Literal ID="litTongGV" runat="server" /></h3>
            </div>
        </div>
        <div class="col-md-4 col-lg-2">
            <div class="stat-card orange">
                <div class="small opacity-75">Tổng Doanh thu</div>
                <h5 class="fw-bold mb-0"><asp:Literal ID="litDoanhThu" runat="server" /></h5>
            </div>
        </div>
        <div class="col-md-4 col-lg-2">
            <div class="stat-card orange" style="background:linear-gradient(135deg,#dc3545,#b02a37)">
                <div class="small opacity-75">Học phí Còn nợ</div>
                <h5 class="fw-bold mb-0"><asp:Literal ID="litCongNo" runat="server" /></h5>
            </div>
        </div>
    </div>

    <!-- BIỂU ĐỒ CHART.JS -->
    <div class="row g-4 mb-4">
        <div class="col-lg-7">
            <div class="card card-custom h-100">
                <div class="card-header"><span><i class="bi bi-graph-up-arrow me-2"></i>Biểu đồ Doanh thu theo Tháng</span></div>
                <div class="card-body">
                    <canvas id="chartDoanhThu" height="125"></canvas>
                </div>
            </div>
        </div>
        <div class="col-lg-5">
            <div class="card card-custom h-100">
                <div class="card-header"><span><i class="bi bi-pie-chart-fill me-2"></i>Học viên theo Khóa học</span></div>
                <div class="card-body">
                    <canvas id="chartKhoaHoc" height="175"></canvas>
                </div>
            </div>
        </div>
    </div>

    <!-- BẢNG DỮ LIỆU GẦN ĐÂY -->
    <div class="row g-4">
        <div class="col-lg-6">
            <div class="card card-custom">
                <div class="card-header"><span><i class="bi bi-calendar-event me-2"></i>Danh sách Lớp sắp khai giảng</span></div>
                <div class="card-body p-0">
                    <asp:GridView ID="gvLopMoi" runat="server" CssClass="table table-hover table-custom mb-0" AutoGenerateColumns="false">
                        <Columns>
                            <asp:BoundField DataField="malop" HeaderText="Mã lớp" />
                            <asp:BoundField DataField="tenlop" HeaderText="Tên lớp" />
                            <asp:BoundField DataField="ngaykhaigiang" HeaderText="Khai giảng" DataFormatString="{0:dd/MM/yyyy}" />
                            <asp:BoundField DataField="hoten" HeaderText="Giảng viên" />
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
        <div class="col-lg-6">
            <div class="card card-custom">
                <div class="card-header"><span><i class="bi bi-person-plus-fill me-2"></i>Học viên Đăng ký gần đây</span></div>
                <div class="card-body p-0">
                    <asp:GridView ID="gvDangKyMoi" runat="server" CssClass="table table-hover table-custom mb-0" AutoGenerateColumns="false">
                        <Columns>
                            <asp:BoundField DataField="madangky" HeaderText="Mã ĐK" />
                            <asp:BoundField DataField="hoten" HeaderText="Học viên" />
                            <asp:BoundField DataField="tenkhoahoc" HeaderText="Khóa học" />
                            <asp:BoundField DataField="sotiendong" HeaderText="Đã đóng" DataFormatString="{0:N0} đ" />
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </div>

    <!-- 4 HiddenField truyền dữ liệu biểu đồ từ C# sang JS -->
    <asp:HiddenField ID="hfThangDoanhThu" runat="server" ClientIDMode="Static" />
    <asp:HiddenField ID="hfGiaTriDoanhThu" runat="server" ClientIDMode="Static" />
    <asp:HiddenField ID="hfTenKhoaHoc" runat="server" ClientIDMode="Static" />
    <asp:HiddenField ID="hfSoLuongHV" runat="server" ClientIDMode="Static" />

    <script>
        const rawThang = document.getElementById('hfThangDoanhThu').value;
        const rawTien = document.getElementById('hfGiaTriDoanhThu').value;
        const dtLabels = rawThang ? rawThang.split('|') : [];
        const dtValues = rawTien ? rawTien.split('|').map(Number) : [];

        new Chart(document.getElementById('chartDoanhThu'), {
            type: 'bar',
            data: {
                labels: dtLabels,
                datasets: [{ label: 'Doanh thu (VNĐ)', data: dtValues, backgroundColor: '#1677D2', borderRadius: 6 }]
            }
        });

        const rawTenKH = document.getElementById('hfTenKhoaHoc').value;
        const rawSL = document.getElementById('hfSoLuongHV').value;
        const khLabels = rawTenKH ? rawTenKH.split('|') : [];
        const khValues = rawSL ? rawSL.split('|').map(Number) : [];

        new Chart(document.getElementById('chartKhoaHoc'), {
            type: 'doughnut',
            data: {
                labels: khLabels,
                datasets: [{ data: khValues, backgroundColor: ['#1677D2', '#FF8A26', '#00B4DB', '#6A11CB', '#28a745'] }]
            }
        });
    </script>
</asp:Content>