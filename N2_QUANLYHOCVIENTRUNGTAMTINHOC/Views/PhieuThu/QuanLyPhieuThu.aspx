<%@ Page Title="Quản lý Phiếu thu" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeBehind="QuanLyPhieuThu.aspx.cs" Inherits="N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.PhieuThu.QuanLyPhieuThu" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Quản lý Phiếu thu</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnlTB" runat="server" Visible="false" CssClass="alert alert-success"><asp:Literal ID="litTB" runat="server" /></asp:Panel>
    <div class="row g-4">
        <div class="col-lg-4">
            <div class="card card-custom">
                <div class="card-header"><span><i class="bi bi-cash-coin me-2"></i>Lập Phiếu thu Học phí</span></div>
                <div class="card-body">
                    <div class="mb-2"><label class="small fw-bold">Mã phiếu thu (*)</label><asp:TextBox ID="txtMaPT" runat="server" CssClass="form-control" placeholder="PT02" /></div>
                    <div class="mb-2"><label class="small fw-bold">Hồ sơ Đăng ký (*)</label><asp:DropDownList ID="ddlDangKy" runat="server" CssClass="form-select" /></div>
                    <div class="mb-2"><label class="small fw-bold">Số tiền thu (VNĐ)</label><asp:TextBox ID="txtSoTien" runat="server" TextMode="Number" CssClass="form-control" /></div>
                    <div class="mb-2">
                        <label class="small fw-bold">Phương thức thanh toán</label>
                        <asp:DropDownList ID="ddlPhuongThuc" runat="server" CssClass="form-select">
                            <asp:ListItem Text="Tiền mặt" Value="Tiền mặt" />
                            <asp:ListItem Text="Chuyển khoản Ngân hàng" Value="Chuyển khoản" />
                        </asp:DropDownList>
                    </div>
                    <div class="mb-3"><label class="small fw-bold">Nội dung thu</label><asp:TextBox ID="txtNoiDung" runat="server" CssClass="form-control" /></div>
                    <asp:Button ID="btnLapPhieu" runat="server" Text="Lập Phiếu & Cập nhật Học phí" CssClass="btn btn-orange w-100" OnClick="btnLapPhieu_Click" />
                </div>
            </div>
        </div>
        <div class="col-lg-8">
            <div class="card card-custom">
                <div class="card-header"><span>Danh sách Phiếu thu đã lập</span></div>
                <div class="card-body p-0">
                    <asp:GridView ID="gvPhieuThu" runat="server" CssClass="table table-hover table-custom mb-0" AutoGenerateColumns="false">
                        <Columns>
                            <asp:BoundField DataField="maphieuthu" HeaderText="Số PT" />
                            <asp:BoundField DataField="ngaythu" HeaderText="Ngày thu" DataFormatString="{0:dd/MM/yyyy}" />
                            <asp:BoundField DataField="hoten" HeaderText="Học viên" />
                            <asp:BoundField DataField="sotienthu" HeaderText="Số tiền" DataFormatString="{0:N0} đ" />
                            <asp:BoundField DataField="phuongthucthanhtoan" HeaderText="Hình thức" />
                            <asp:BoundField DataField="nguoithu" HeaderText="Người thu" />
                            <asp:TemplateField HeaderText="In">
                                <ItemTemplate>
                                    <button type="button" class="btn btn-sm btn-outline-primary" onclick="inPhieu('<%# Eval("maphieuthu") %>','<%# Eval("hoten") %>','<%# Eval("sotienthu","{0:N0}") %>','<%# Eval("noidungthu") %>')">
                                        <i class="bi bi-printer"></i>
                                    </button>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </div>

    <script>
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