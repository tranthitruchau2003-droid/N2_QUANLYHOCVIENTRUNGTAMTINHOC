<%@ Page Title="Quản lý Chứng chỉ" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeBehind="QuanLyChungChi.aspx.cs" Inherits="N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.ChungChi.QuanLyChungChi" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Cấp Chứng chỉ</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnlTB" runat="server" Visible="false" CssClass="alert alert-success"><asp:Literal ID="litTB" runat="server" /></asp:Panel>
    <div class="row g-4">
        <div class="col-lg-4">
            <div class="card card-custom">
                <div class="card-header"><span><i class="bi bi-award me-2"></i>Cấp Chứng chỉ Đào tạo</span></div>
                <div class="card-body">
                    <div class="mb-2"><label class="small fw-bold">Mã chứng chỉ</label><asp:TextBox ID="txtMaCC" runat="server" CssClass="form-control" placeholder="CC02" /></div>
                    <div class="mb-2"><label class="small fw-bold">Học viên đủ điều kiện (Đạt)</label><asp:DropDownList ID="ddlThamGia" runat="server" CssClass="form-select" /></div>
                    <div class="mb-2"><label class="small fw-bold">Số hiệu chứng chỉ</label><asp:TextBox ID="txtSoHieu" runat="server" CssClass="form-control" placeholder="TTTH-2026-0002" /></div>
                    <div class="mb-3">
                        <label class="small fw-bold">Xếp loại</label>
                        <asp:DropDownList ID="ddlXepLoai" runat="server" CssClass="form-select">
                            <asp:ListItem Text="Xuất sắc" Value="Xuất sắc" />
                            <asp:ListItem Text="Giỏi" Value="Giỏi" />
                            <asp:ListItem Text="Khá" Value="Khá" />
                            <asp:ListItem Text="Trung bình" Value="Trung bình" />
                        </asp:DropDownList>
                    </div>
                    <asp:Button ID="btnCapCC" runat="server" Text="Cấp Chứng chỉ" CssClass="btn btn-orange w-100" OnClick="btnCapCC_Click" />
                </div>
            </div>
        </div>
        <div class="col-lg-8">
            <div class="card card-custom">
                <div class="card-header"><span>Danh sách Chứng chỉ đã cấp</span></div>
                <div class="card-body p-0">
                    <asp:GridView ID="gvChungChi" runat="server" CssClass="table table-hover table-custom mb-0" AutoGenerateColumns="false">
                        <Columns>
                            <asp:BoundField DataField="sohieuchungchi" HeaderText="Số hiệu" />
                            <asp:BoundField DataField="hoten" HeaderText="Họ tên Học viên" />
                            <asp:BoundField DataField="diem" HeaderText="Điểm thi" />
                            <asp:BoundField DataField="xeploai" HeaderText="Xếp loại" />
                            <asp:BoundField DataField="ngaycap" HeaderText="Ngày cấp" DataFormatString="{0:dd/MM/yyyy}" />
                            <asp:TemplateField HeaderText="In">
                                <ItemTemplate>
                                    <button type="button" class="btn btn-sm btn-outline-primary" onclick="inCC('<%# Eval("sohieuchungchi") %>','<%# Eval("hoten") %>','<%# Eval("xeploai") %>')">
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
        function inCC(sh, ten, xl) {
            const w = window.open('', '_blank', 'width=800,height=550');
            w.document.write(`<html><head><title>Chứng chỉ ${sh}</title><link href="/Content/bootstrap.min.css" rel="stylesheet"/></head><body class="p-4 text-center">
                <div style="border:6px double #0755A6; padding:40px;">
                    <h5 class="text-uppercase">CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM</h5>
                    <p>Độc lập - Tự do - Hạnh phúc</p>
                    <h2 class="text-primary fw-bold mt-4">CHỨNG CHỈ HOÀN THÀNH KHÓA HỌC</h2>
                    <p class="mt-3">Chứng nhận học viên: <strong class="fs-4 text-uppercase">${ten}</strong></p>
                    <p>Xếp loại: <strong>${xl}</strong> | Số hiệu: <strong>${sh}</strong></p>
                </div><script>window.print();<\/script></body></html>`);
            w.document.close();
        }
    </script>
</asp:Content>