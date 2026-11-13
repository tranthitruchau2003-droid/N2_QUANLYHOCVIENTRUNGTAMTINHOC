<%@ Page Title="Đăng ký Học phần & Xếp lớp" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeBehind="QuanLyDangKy.aspx.cs" Inherits="N2_QUANLYHOCVIENTRUNGTAMTINHOC.Views.DangKy.QuanLyDangKy" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">Đăng ký Khóa học & Xếp lớp</asp:Content>
<asp:Content ID="ContentHead" ContentPlaceHolderID="HeadContent" runat="server">
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
        .table-dangky { font-size: 0.82rem; }
        .table-dangky th, .table-dangky td { vertical-align: middle !important; padding: 0.4rem 0.35rem !important; }
        .btn-xuly-lop { font-size: 0.84rem !important; font-weight: 600 !important; padding: 0.42rem 0.75rem !important; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" />
    <asp:Panel ID="pnlTB" runat="server" Visible="false" ClientIDMode="Static"
        CssClass="alert alert-info fw-semibold alert-dismissible fade show shadow"
        style="position: fixed; top: 76px; right: 24px; z-index: 1060; min-width: 320px; max-width: 500px;">
        <asp:Literal ID="litTB" runat="server" />
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    </asp:Panel>

    <!-- Thẻ ẩn để hứng tính năng tự điền (autofill) của trình duyệt -->
    <div style="position: absolute; left: -9999px; top: -9999px; opacity: 0; height: 0; overflow: hidden;">
        <input type="text" name="fake_username_autofill" tabindex="-1" />
        <input type="password" name="fake_password_autofill" tabindex="-1" />
    </div>

    <!-- 1. DANH SÁCH LỚP HỌC PHẦN ĐANG MỞ -->
    <div class="card card-custom mb-4">
        <div class="card-header d-flex justify-content-between align-items-center flex-wrap gap-2">
            <span><i class="bi bi-calendar2-week-fill text-primary me-2"></i>1. Các Lớp Học phần đang mở (Tối đa 10 Học viên / Lớp)</span>
            <div class="d-flex gap-2 align-items-center">
                <span class="small fw-bold text-nowrap">Lọc theo Khóa học:</span>
                <asp:DropDownList ID="ddlLocKhoaMo" runat="server" CssClass="form-select form-select-sm" style="width: 240px;"
                    AutoPostBack="true" OnSelectedIndexChanged="ddlLocKhoaMo_SelectedIndexChanged" />
            </div>
        </div>
        <div class="card-body p-0">
            <div class="table-responsive" style="max-height: 360px; overflow-y: auto;">
                <asp:GridView ID="gvLopMo" runat="server" CssClass="table table-sm table-hover table-custom mb-0"
                    AutoGenerateColumns="false" OnRowCommand="gvLopMo_RowCommand">
                    <Columns>
                        <asp:BoundField DataField="malop" HeaderText="Mã Lớp" />
                        <asp:TemplateField HeaderText="Tên Lớp">
                            <ItemTemplate>
                                <a href='<%# ResolveUrl("~/Views/LopHoc/QuanLyLopHoc.aspx") %>' class="fw-bold text-primary text-decoration-none" title="Xem chi tiết Lớp học & Thời khóa biểu">
                                    <%# Eval("tenlop") %> <i class="bi bi-box-arrow-up-right small"></i>
                                </a>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="tenkhoahoc" HeaderText="Khóa học" />
                        <asp:BoundField DataField="tengiangvien" HeaderText="Giảng viên giảng dạy" />
                        <asp:BoundField DataField="thoikhoabieu" HeaderText="Thời khóa biểu (Ca - Thứ - Phòng)" />
                        <asp:BoundField DataField="thoigianlop" HeaderText="Thời gian học (Từ - Đến)" ItemStyle-CssClass="text-nowrap" />
                        <asp:BoundField DataField="hocphi" HeaderText="Học phí" DataFormatString="{0:N0} đ" ItemStyle-CssClass="text-nowrap" />
                        <asp:TemplateField HeaderText="Sĩ số (Chuẩn 10 HV)">
                            <ItemTemplate>
                                <span class='badge <%# Convert.ToInt32(Eval("siso")) >= 10 ? "bg-success" : "bg-warning text-dark" %>'>
                                    <%# Eval("siso") %> / 10 HV
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="trangthailop" HeaderText="Trạng thái mở lớp" />
                        <asp:TemplateField HeaderText="Đăng ký học">
                            <ItemTemplate>
                                <asp:LinkButton ID="btnChonLopHocPhan" runat="server" CommandName="DangKyLop"
                                    CommandArgument='<%# Eval("malop") + "|" + Eval("makhoahoc") + "|" + Eval("hocphi") %>'
                                    Enabled='<%# !Convert.ToBoolean(Eval("dadongdk")) && Convert.ToInt32(Eval("siso")) < 10 %>'
                                    CssClass='<%# (Convert.ToBoolean(Eval("dadongdk")) || Convert.ToInt32(Eval("siso")) >= 10) ? "btn btn-sm btn-secondary disabled text-nowrap" : "btn btn-sm btn-orange text-nowrap" %>'>
                                    <%# Convert.ToBoolean(Eval("dadongdk")) ? "Đã đóng ĐK" : (Convert.ToInt32(Eval("siso")) >= 10 ? "Đã đầy (10/10)" : "Chọn lớp này") %>
                                </asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </div>

    <div class="row g-4">
        <!-- 2. FORM ĐĂNG KÝ -->
        <div class="col-lg-4 d-flex flex-column">
            <div class="card card-custom mb-3">
                <div class="card-header"><span><i class="bi bi-card-checklist me-2"></i>2. Phiếu Đăng ký Học phần</span></div>
                <div class="card-body">
                    <asp:UpdatePanel ID="upPhieuDangKy" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <div class="mb-2">
                                <label class="small fw-bold">Mã đăng ký</label>
                                <asp:TextBox ID="txtMaDK" runat="server" CssClass="form-control" ReadOnly="true" placeholder="Tự động sinh mã..." />
                            </div>
                            <div class="mb-2">
                                <label class="small fw-bold">Học viên (*)</label>
                                <asp:DropDownList ID="ddlHocVien" runat="server" CssClass="form-select" />
                            </div>
                            <div class="mb-2">
                                <label class="small fw-bold">Khóa học (*)</label>
                                <asp:DropDownList ID="ddlKhoaHoc" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlKhoaHoc_SelectedIndexChanged" />
                            </div>
                            <div class="mb-2">
                                <label class="small fw-bold">Chọn Lớp & Giảng viên muốn học (*)</label>
                                <asp:DropDownList ID="ddlLopHoc" runat="server" CssClass="form-select" />
                            </div>
                            <asp:Panel ID="pnlSoTienDongNgay" runat="server" CssClass="mb-2">
                                <label class="small fw-bold">Học phí khóa học (VNĐ)</label>
                                <asp:TextBox ID="txtSoTienDong" runat="server" CssClass="form-control" ReadOnly="true" Text="0" />
                                <small class="text-muted">Sau khi đăng ký lớp, bấm nút <strong>"Đóng HP"</strong> ở bảng bên phải để quét QR và gửi ảnh chuyển khoản.</small>
                            </asp:Panel>
                            <div class="mb-3">
                                <label class="small fw-bold">Ngày đăng ký</label>
                                <asp:TextBox ID="txtNgayDK" runat="server" TextMode="Date" CssClass="form-control" />
                            </div>
                            <div class="d-flex gap-2">
                                <asp:Button ID="btnDangKy" runat="server" Text="Xác nhận Đăng ký" CssClass="btn btn-orange flex-grow-1" OnClick="btnDangKy_Click" />
                                <asp:Button ID="btnDoiLop" runat="server" Text="Đổi Lớp" CssClass="btn btn-primary-gradient" OnClick="btnDoiLop_Click" />
                            </div>
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="ddlKhoaHoc" EventName="SelectedIndexChanged" />
                            <asp:PostBackTrigger ControlID="btnDangKy" />
                            <asp:PostBackTrigger ControlID="btnDoiLop" />
                        </Triggers>
                    </asp:UpdatePanel>
                </div>
            </div>

            <!-- PANEL XỬ LÝ LỚP CHƯA ĐỦ 10 HỌC VIÊN (CHỈ ADMIN / NHÂN VIÊN THẤY) -->
            <asp:Panel ID="pnlXuLyLopThieu" runat="server" CssClass="card card-custom border-warning flex-grow-1">
                <div class="card-header bg-warning-subtle text-dark fw-bold">
                    <i class="bi bi-tools me-1"></i> Xử lý Lớp chưa đủ 10 Học viên (< 10 HV)
                </div>
                <div class="card-body">
                    <div class="mb-2">
                        <label class="small fw-bold">Lớp đang thiếu học viên (< 10 HV)</label>
                        <asp:DropDownList ID="ddlLopThieu" runat="server" CssClass="form-select form-select-sm" />
                    </div>
                    <div class="mb-2">
                        <label class="small fw-bold">Ghép sang Lớp đích (cùng khóa, còn chỗ)</label>
                        <asp:DropDownList ID="ddlLopDich" runat="server" CssClass="form-select form-select-sm" />
                    </div>
                    <div class="d-grid gap-2 mt-3">
                        <asp:Button ID="btnGhepLop" runat="server" Text="Ghép toàn bộ HV sang Lớp đích" CssClass="btn btn-primary btn-xuly-lop" OnClick="btnGhepLop_Click" />
                        <asp:Button ID="btnMoDacCach" runat="server" Text="Duyệt mở lớp đặc cách (Dù < 10 HV)" CssClass="btn btn-success btn-xuly-lop" OnClick="btnMoDacCach_Click" />
                        <asp:Button ID="btnHuyLopThieu" runat="server" Text="Hủy lớp & Bảo lưu học phí" CssClass="btn btn-outline-danger btn-xuly-lop" OnClick="btnHuyLopThieu_Click" />
                        <asp:Button ID="btnXemLopHuyHoanTien" runat="server" Text="Danh sách Lớp bị hủy & Hoàn tiền" CssClass="btn btn-warning text-dark btn-xuly-lop" OnClick="btnXemLopHuyHoanTien_Click" />
                    </div>
                </div>
            </asp:Panel>
        </div>

        <!-- 3. DANH SÁCH ĐĂNG KÝ & ĐÓNG HỌC PHÍ QR -->
        <div class="col-lg-8 d-flex flex-column">
            <div class="card card-custom h-100 d-flex flex-column">
                <div class="card-header d-flex justify-content-between align-items-center flex-wrap gap-2">
                    <span class="text-nowrap"><i class="bi bi-list-ul me-2"></i>3. Kết quả Đăng ký Lớp học & Đóng Học phí Chuyển khoản QR</span>
                    <asp:Panel ID="pnlTimDK" runat="server" DefaultButton="btnTimDK" CssClass="d-flex gap-2">
                        <div class="position-relative" style="width: 220px;">
                            <asp:TextBox ID="txtTimDK" runat="server" ClientIDMode="Static" TextMode="Search" CssClass="form-control form-control-sm"
                                placeholder="Tìm mã ĐK, học viên, lớp..." autocomplete="off" onclick="this.select();" />
                            <div id="goiYDK" class="list-group shadow position-absolute w-100"
                                style="top: 100%; left: 0; z-index: 1050; max-height: 150px; overflow-y: auto; display: none;">
                                <asp:Literal ID="litGoiYDK" runat="server" />
                            </div>
                        </div>
                        <asp:Button ID="btnTimDK" runat="server" ClientIDMode="Static" Text="Tìm" CssClass="btn btn-sm btn-primary-gradient" OnClick="btnTimDK_Click" />
                    </asp:Panel>
                </div>
                <div class="card-body p-0 flex-grow-1 d-flex flex-column">
                    <div class="table-responsive flex-grow-1">
                        <asp:GridView ID="gvDangKy" runat="server" CssClass="table table-sm table-hover table-custom table-dangky mb-0"
                            AutoGenerateColumns="false" AllowPaging="true" PageSize="12"
                            OnPageIndexChanging="gvDangKy_PageIndexChanging" OnRowCommand="gvDangKy_RowCommand"
                            EmptyDataText="Không tìm thấy bản ghi đăng ký nào.">
                            <PagerSettings Mode="NumericFirstLast" FirstPageText="&laquo;" LastPageText="&raquo;" PreviousPageText="&lsaquo;" NextPageText="&rsaquo;" PageButtonCount="5" />
                            <PagerStyle CssClass="pagination-ys" HorizontalAlign="Center" />
                            <Columns>
                                <asp:BoundField DataField="madangky" HeaderText="Mã ĐK" ItemStyle-CssClass="text-nowrap fw-semibold" />
                                <asp:BoundField DataField="hoten" HeaderText="Học viên" />
                                <asp:BoundField DataField="tenkhoahoc" HeaderText="Khóa học" />
                                <asp:BoundField DataField="tenlop" HeaderText="Lớp & Giảng viên" />
                                <asp:BoundField DataField="hocphi" HeaderText="Học phí" DataFormatString="{0:N0} đ" ItemStyle-CssClass="text-nowrap text-end" HeaderStyle-CssClass="text-end" />
                                <asp:BoundField DataField="dadong" HeaderText="Đã đóng" DataFormatString="{0:N0} đ" ItemStyle-CssClass="text-nowrap text-end text-success fw-bold" HeaderStyle-CssClass="text-end" />
                                <asp:BoundField DataField="trangthailop" HeaderText="Tình trạng Xếp lớp" />
                                <asp:TemplateField HeaderText="Thao tác" ItemStyle-CssClass="text-nowrap text-center" HeaderStyle-CssClass="text-center">
                                    <ItemTemplate>
                                        <div class="d-flex gap-1 justify-content-center">
                                            <asp:LinkButton ID="btnChonDK" runat="server" CommandName="ChonDK" CommandArgument='<%# Eval("madangky") %>'
                                                CssClass="btn btn-sm btn-outline-primary" title="Chọn để đổi lớp"><i class="bi bi-arrow-left-right"></i></asp:LinkButton>
                                            
                                            <asp:LinkButton ID="btnDongHP" runat="server" CommandName="DongHocPhi" CommandArgument='<%# Eval("madangky") + "|" + Eval("conno") %>'
                                                Visible='<%# Convert.ToDecimal(Eval("conno")) > 0 && Eval("trangthaithanhtoan").ToString() != "Chờ xác nhận" && !Eval("trangthailop").ToString().Contains("Hủy lớp") %>'
                                                CssClass="btn btn-sm btn-success text-nowrap" title="Mở mã QR chuyển khoản & gửi ảnh biên lai">
                                                <i class="bi bi-qr-code-scan"></i> Đóng HP
                                            </asp:LinkButton>

                                            <asp:LinkButton ID="btnYeuCauHoanTien" runat="server" CommandName="MoYeuCauHoanTien" CommandArgument='<%# Eval("madangky") %>'
                                                Visible='<%# Convert.ToDecimal(Eval("dadong")) > 0 && Eval("trangthailop").ToString().Contains("Hủy lớp") && !Eval("trangthailop").ToString().Contains("Đã hoàn tiền") %>'
                                                CssClass='<%# Eval("trangthailop").ToString().Contains("Chờ hoàn") ? "btn btn-sm btn-outline-warning fw-bold text-dark text-nowrap" : "btn btn-sm btn-warning fw-bold text-dark text-nowrap" %>'
                                                title="Nhập hoặc sửa lại thông tin ngân hàng nhận hoàn tiền">
                                                <i class="bi bi-bank"></i> <%# Eval("trangthailop").ToString().Contains("Chờ hoàn") ? "Sửa STK hoàn" : "Nhận hoàn tiền" %>
                                            </asp:LinkButton>
                                            <asp:LinkButton ID="btnXemBienLaiHoanHV" runat="server" CommandName="XemBienLaiHoanHV" CommandArgument='<%# Eval("madangky") %>'
                                                Visible='<%# Eval("trangthailop").ToString().Contains("Đã hoàn tiền") || Eval("trangthaithanhtoan").ToString() == "Đã hoàn tiền" %>'
                                                CssClass="btn btn-sm btn-outline-success fw-semibold text-nowrap" title="Xem ảnh chứng từ trung tâm đã chuyển khoản hoàn tiền">
                                                <i class="bi bi-image"></i> Ảnh hoàn tiền
                                            </asp:LinkButton>
                                            <button type="button" class="btn btn-sm btn-outline-danger" title="Hủy học phần"
                                                onclick="hienThiModalHuyDK('<%# Eval("madangky") %>'); return false;"><i class="bi bi-trash"></i></button>
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

    <!-- POPUP THÔNG BÁO KẾT QUẢ HOÀN TIỀN -->
    <div class="modal fade" id="modalThongBaoHoanTien" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content border-0 shadow">
                <div class="modal-header bg-success text-white">
                    <h5 class="modal-title fw-bold"><i class="bi bi-check-circle-fill me-2"></i>Thông báo Xử lý Hoàn tiền</h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body py-4 text-center">
                    <i class="bi bi-cash-coin display-4 text-success mb-3 d-block"></i>
                    <p class="fs-6 mb-0"><asp:Literal ID="litNoiDungTBHoanTien" runat="server" /></p>
                </div>
                <div class="modal-footer justify-content-center border-0 pb-3">
                    <button type="button" class="btn btn-success px-4 fw-semibold" data-bs-dismiss="modal">Đã hiểu</button>
                </div>
            </div>
        </div>
    </div>

    <!-- POPUP XÁC NHẬN HỦY ĐĂNG KÝ -->
    <div class="modal fade" id="modalHuyDK" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content border-0 shadow">
                <div class="modal-header bg-danger text-white">
                    <h5 class="modal-title fw-bold"><i class="bi bi-exclamation-triangle me-2"></i>Xác nhận Hủy Đăng ký</h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body py-4 text-center">
                    <i class="bi bi-trash display-4 text-danger mb-3 d-block"></i>
                    <p class="fs-5 mb-0">Bạn có chắc chắn muốn hủy đăng ký học phần <strong id="maDKCanHuy"></strong> này?</p>
                </div>
                <div class="modal-footer justify-content-center border-0 pb-4">
                    <button type="button" class="btn btn-secondary px-4" data-bs-dismiss="modal">Không</button>
                    <asp:LinkButton ID="btnHuyDKChinhThuc" runat="server" ClientIDMode="Static" CssClass="btn btn-danger px-4 d-none" OnClick="btnHuyDKChinhThuc_Click">Đồng ý</asp:LinkButton>
                    <button type="button" class="btn btn-danger px-4" onclick="document.getElementById('btnHuyDKChinhThuc').click();">
                        <i class="bi bi-check-lg me-1"></i> Đồng ý
                    </button>
                </div>
            </div>
        </div>
    </div>
    <asp:HiddenField ID="hfMaDKHuy" runat="server" ClientIDMode="Static" />

    <!-- POPUP THANH TOÁN CHUYỂN KHOẢN QR & UPLOAD ẢNH CHỤP MÀN HÌNH -->
    <div class="modal fade" id="modalThanhToanQR" tabindex="-1" aria-hidden="true" data-bs-backdrop="static">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">
                <div class="modal-header bg-success text-white">
                    <h5 class="modal-title fw-bold"><i class="bi bi-qr-code-scan me-2"></i>Chuyển khoản Học phí qua QR</h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body p-4 text-center">
                    <p class="small text-muted mb-2">1. Quét mã QR bên dưới hoặc bấm vào link để mở ảnh QR rõ nét:</p>
                    <asp:Image ID="imgQRCode" runat="server" CssClass="img-fluid border p-2 mb-2 rounded" style="max-height: 240px;" />
                    <div class="mb-2">
                        <asp:HyperLink ID="lnkMoAnhQR" runat="server" Target="_blank" CssClass="btn btn-sm btn-outline-primary">
                            <i class="bi bi-box-arrow-up-right me-1"></i> Mở Link Ảnh QR Chuyển khoản
                        </asp:HyperLink>
                    </div>
                    <h4 class="text-danger fw-bold mb-1"><asp:Literal ID="litSoTienQR" runat="server" /></h4>
                    <p class="small fw-bold mb-3">Nội dung CK: <span class="badge bg-warning text-dark fs-6"><asp:Literal ID="litNoiDungQR" runat="server" /></span></p>

                    <div class="text-start border-top pt-3">
                        <label class="small fw-bold text-danger mb-1">2. Tải lên ảnh chụp màn hình chuyển khoản thành công (*):</label>
                        <asp:FileUpload ID="fuMinhChungCK" runat="server" CssClass="form-control form-control-sm mb-2" accept="image/*" onchange="xemTruocAnhCK(this);" />
                        <div class="text-center">
                            <img id="imgXemTruocCK" src="#" alt="Ảnh biên lai" class="img-thumbnail d-none" style="max-height: 160px;" />
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <asp:HiddenField ID="hfMaDKThanhToan" runat="server" />
                    <asp:HiddenField ID="hfSoTienThanhToan" runat="server" />
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Đóng</button>
                    <asp:Button ID="btnXacNhanCK" runat="server" Text="Gửi Ảnh & Xác nhận Đã chuyển khoản" CssClass="btn btn-success fw-bold" OnClick="btnXacNhanCK_Click" />
                </div>
            </div>
        </div>
    </div>

    <!-- POPUP HỌC VIÊN NHẬP / SỬA THÔNG TIN TÀI KHOẢN NGÂN HÀNG NHẬN HOÀN TIỀN -->
    <div class="modal fade" id="modalNhapSTKHoanTien" tabindex="-1" aria-hidden="true" data-bs-backdrop="static">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content border-0 shadow">
                <div class="modal-header bg-warning text-dark">
                    <h5 class="modal-title fw-bold"><i class="bi bi-bank2 me-2"></i>Thông tin Tài khoản Nhận Hoàn tiền</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body p-4">
                    <asp:HiddenField ID="hfMaDKHoanTien" runat="server" />
                    <div class="alert alert-info py-2 small mb-3">
                        Hồ sơ: <strong><asp:Literal ID="litHTMaDK" runat="server" /></strong> | 
                        Số tiền được hoàn: <strong class="text-danger"><asp:Literal ID="litHTSoTien" runat="server" /></strong>
                    </div>
                    <div class="mb-2">
                        <label class="small fw-bold">Hình thức nhận hoàn tiền (*)</label>
                        <asp:DropDownList ID="ddlHinhThucHoan" runat="server" CssClass="form-select form-select-sm">
                            <asp:ListItem Text="Chuyển khoản Ngân hàng" Value="Chuyển khoản" />
                            <asp:ListItem Text="Nhận tiền mặt tại Trung tâm" Value="Tiền mặt" />
                        </asp:DropDownList>
                    </div>
                    <div class="mb-2">
                        <label class="small fw-bold">Mã / Tên Ngân hàng (VD: VCB, MB, BIDV, Techcombank...)</label>
                        <asp:TextBox ID="txtNganHangHT" runat="server" CssClass="form-control form-control-sm" placeholder="VD: MB hoặc Vietcombank" />
                    </div>
                    <div class="mb-2">
                        <label class="small fw-bold">Số tài khoản nhận tiền</label>
                        <asp:TextBox ID="txtSoTKHT" runat="server" CssClass="form-control form-control-sm" placeholder="Nhập chính xác số tài khoản..." />
                    </div>
                    <div class="mb-2">
                        <label class="small fw-bold">Tên chủ tài khoản (Viết hoa không dấu)</label>
                        <asp:TextBox ID="txtChuTKHT" runat="server" CssClass="form-control form-control-sm" placeholder="VD: NGUYEN VAN MINH" />
                    </div>
                </div>
                <div class="modal-footer bg-light">
                    <button type="button" class="btn btn-secondary btn-sm" data-bs-dismiss="modal">Hủy</button>
                    <asp:Button ID="btnGuiYeuCauHoanTien" runat="server" Text="Lưu & Gửi yêu cầu Hoàn tiền" CssClass="btn btn-warning btn-sm fw-bold text-dark" OnClick="btnGuiYeuCauHoanTien_Click" />
                </div>
            </div>
        </div>
    </div>

    <!-- POPUP ADMIN QUÉT MÃ QR CHUYỂN TRẢ & GỬI ẢNH BIÊN LAI XÁC NHẬN HOÀN TIỀN -->
    <div class="modal fade" id="modalQRHoanTienAdmin" tabindex="-1" aria-hidden="true" data-bs-backdrop="static">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content border-0 shadow">
                <div class="modal-header bg-danger text-white">
                    <h5 class="modal-title fw-bold"><i class="bi bi-qr-code-scan me-2"></i>Chuyển khoản Hoàn tiền & Gửi Biên lai</h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body p-4 text-center">
                    <asp:HiddenField ID="hfMaDKAdminHoanTien" runat="server" />
                    <div class="alert alert-warning py-2 small text-start mb-2">
                        <div><strong>Học viên:</strong> <asp:Literal ID="litAdminHTHocVien" runat="server" /></div>
                        <div><strong>Tài khoản nhận:</strong> <span class="text-primary fw-bold"><asp:Literal ID="litAdminHTSTK" runat="server" /></span></div>
                        <div><strong>Số tiền hoàn trả:</strong> <span class="text-danger fw-bold fs-6"><asp:Literal ID="litAdminHTSoTien" runat="server" /></span></div>
                    </div>

                    <asp:Panel ID="pnlQRHoanTienBox" runat="server">
                        <p class="small text-muted mb-1">1. Quét mã QR bên dưới để chuyển khoản hoàn tiền cho học viên:</p>
                        <asp:Image ID="imgQRHoanTien" runat="server" CssClass="img-fluid border p-2 mb-2 rounded" style="max-height: 220px;" />
                    </asp:Panel>

                    <div class="text-start border-top pt-3 mt-2">
                        <label class="small fw-bold text-danger mb-1">2. Tải lên ảnh chụp màn hình biên lai đã chuyển hoàn (*):</label>
                        <asp:FileUpload ID="fuBienLaiHoanTien" runat="server" CssClass="form-control form-control-sm mb-2" accept="image/*" onchange="xemTruocAnhHT(this);" />
                        <div class="text-center">
                            <img id="imgXemTruocHT" src="#" alt="Ảnh biên lai hoàn tiền" class="img-thumbnail d-none" style="max-height: 150px;" />
                        </div>
                    </div>
                </div>
                <div class="modal-footer bg-light">
                    <button type="button" class="btn btn-secondary btn-sm" data-bs-dismiss="modal">Đóng</button>
                    <asp:Button ID="btnXacNhanHoanTienAdmin" runat="server" Text="Gửi Ảnh & Xác nhận Đã hoàn tiền" CssClass="btn btn-danger btn-sm fw-bold" OnClick="btnXacNhanHoanTienAdmin_Click" />
                </div>
            </div>
        </div>
    </div>

    <!-- POPUP DANH SÁCH LỚP BỊ HỦY & HOÀN TIỀN HỌC PHÍ (DÀNH CHO ADMIN / KẾ TOÁN) -->
    <div class="modal fade" id="modalLopBiHuy" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable">
            <div class="modal-content border-0 shadow">
                <div class="modal-header bg-warning text-dark">
                    <h5 class="modal-title fw-bold"><i class="bi bi-arrow-counterclockwise me-2"></i>Danh sách Học viên thuộc Lớp bị hủy & Hoàn tiền học phí</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body p-0">
                    <div class="table-responsive">
                        <asp:GridView ID="gvLopBiHuy" runat="server" CssClass="table table-sm table-bordered table-hover mb-0"
                            AutoGenerateColumns="false" OnRowCommand="gvLopBiHuy_RowCommand"
                            EmptyDataText="Hiện không có học viên nào thuộc lớp bị hủy.">
                            <Columns>
                                <asp:BoundField DataField="madangky" HeaderText="Mã ĐK" ItemStyle-CssClass="fw-semibold text-nowrap" />
                                <asp:BoundField DataField="hoten" HeaderText="Học viên" ItemStyle-CssClass="fw-bold" />
                                <asp:BoundField DataField="dienthoai" HeaderText="SĐT" />
                                <asp:BoundField DataField="tenkhoahoc" HeaderText="Khóa học" />
                                <asp:BoundField DataField="tenlopbihuy" HeaderText="Tên Lớp bị hủy" ItemStyle-CssClass="text-danger fw-semibold" />
                                <asp:BoundField DataField="dadong" HeaderText="Đã đóng" DataFormatString="{0:N0} đ" ItemStyle-CssClass="text-end fw-bold text-success text-nowrap" HeaderStyle-CssClass="text-end" />
                                <asp:BoundField DataField="thongtinstk" HeaderText="Tài khoản nhận hoàn tiền" ItemStyle-CssClass="text-primary fw-semibold" />
                                <asp:TemplateField HeaderText="Xử lý Hoàn tiền" ItemStyle-CssClass="text-center text-nowrap" HeaderStyle-CssClass="text-center">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="btnNhapSTKHo" runat="server" CommandName="MoYeuCauHoanTien" CommandArgument='<%# Eval("madangky") %>'
                                            Visible='<%# Convert.ToDecimal(Eval("dadong")) > 0 && string.IsNullOrEmpty(Eval("thongtinstk").ToString()) && !Eval("trangthailop").ToString().Contains("Đã hoàn tiền") %>'
                                            CssClass="btn btn-sm btn-outline-primary" title="Nhập STK ngân hàng để hoàn tiền">
                                            <i class="bi bi-pencil-square me-1"></i> Nhập STK
                                        </asp:LinkButton>

                                        <asp:LinkButton ID="btnHoanTien" runat="server" CommandName="MoQRHoanTien" CommandArgument='<%# Eval("madangky") %>'
                                            Visible='<%# Convert.ToDecimal(Eval("dadong")) > 0 && !string.IsNullOrEmpty(Eval("thongtinstk").ToString()) && !Eval("trangthailop").ToString().Contains("Đã hoàn tiền") %>'
                                            CssClass="btn btn-sm btn-danger fw-semibold">
                                            <i class="bi bi-qr-code-scan me-1"></i> Tạo QR & Hoàn tiền
                                        </asp:LinkButton>

                                        <span class='badge bg-secondary' style='<%# Convert.ToDecimal(Eval("dadong")) == 0 && !Eval("trangthailop").ToString().Contains("Đã hoàn tiền") ? "" : "display:none;" %>'>Chưa đóng tiền</span>
                                        <span class='badge bg-success' style='<%# Eval("trangthailop").ToString().Contains("Đã hoàn tiền") ? "" : "display:none;" %>'>Đã hoàn tiền</span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>
                <div class="modal-footer bg-light">
                    <button type="button" class="btn btn-secondary px-4" data-bs-dismiss="modal">Đóng</button>
                </div>
            </div>
        </div>
    </div>

    <!-- POPUP HỌC VIÊN XEM ẢNH CHỨNG TỪ TRUNG TÂM ĐÃ HOÀN TIỀN -->
    <div class="modal fade" id="modalXemBienLaiHoanHV" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content border-0 shadow">
                <div class="modal-header bg-success text-white">
                    <h5 class="modal-title fw-bold"><i class="bi bi-check-circle-fill me-2"></i>Chứng từ Đã Hoàn tiền Học phí</h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body p-4 text-center">
                    <div class="alert alert-light border text-start small mb-3">
                        <div><strong>Hồ sơ đăng ký:</strong> <asp:Literal ID="litHVHoanMaDK" runat="server" /></div>
                        <div><strong>Số tiền đã hoàn:</strong> <span class="text-danger fw-bold"><asp:Literal ID="litHVHoanSoTien" runat="server" /></span></div>
                        <div><strong>Ngày chuyển hoàn:</strong> <asp:Literal ID="litHVHoanNgay" runat="server" /></div>
                        <div><strong>Chi tiết giao dịch:</strong> <asp:Literal ID="litHVHoanChiTiet" runat="server" /></div>
                    </div>
                    <asp:Image ID="imgBienLaiHoanHV" runat="server" CssClass="img-fluid border rounded p-1 mb-2" style="max-height: 350px;" />
                    <div>
                        <asp:HyperLink ID="lnkMoAnhHoanHV" runat="server" Target="_blank" CssClass="btn btn-sm btn-outline-primary">
                            <i class="bi bi-arrows-fullscreen me-1"></i> Mở ảnh kích thước đầy đủ
                        </asp:HyperLink>
                    </div>
                </div>
                <div class="modal-footer bg-light">
                    <button type="button" class="btn btn-secondary px-4" data-bs-dismiss="modal">Đóng</button>
                </div>
            </div>
        </div>
    </div>

    <script>
        // @ts-nocheck
        function showBootstrapModal(id) {
            var checkModal = function () {
                var el = document.getElementById(id);
                if (el && window.bootstrap) {
                    new window.bootstrap.Modal(el).show();
                } else {
                    setTimeout(checkModal, 100);
                }
            };
            checkModal();
        }

        function hienThiModalHuyDK(maDK) {
            document.getElementById('hfMaDKHuy').value = maDK;
            document.getElementById('maDKCanHuy').innerText = maDK;
            showBootstrapModal('modalHuyDK');
        }

        function moModalQR() { showBootstrapModal('modalThanhToanQR'); }
        function moModalNhapSTKHoanTien() { showBootstrapModal('modalNhapSTKHoanTien'); }
        function moModalLopBiHuy() { showBootstrapModal('modalLopBiHuy'); }
        function moModalQRHoanTienAdmin() { showBootstrapModal('modalQRHoanTienAdmin'); }
        function moModalThongBaoHoanTien() { showBootstrapModal('modalThongBaoHoanTien'); }

        function xemTruocAnhCK(input) {
            var preview = document.getElementById('imgXemTruocCK');
            if (input.files && input.files[0]) {
                var reader = new FileReader();
                reader.onload = function (e) {
                    preview.src = e.target.result;
                    preview.classList.remove('d-none');
                };
                reader.readAsDataURL(input.files[0]);
            }
        }

        function xemTruocAnhHT(input) {
            var preview = document.getElementById('imgXemTruocHT');
            if (input.files && input.files[0]) {
                var reader = new FileReader();
                reader.onload = function (e) {
                    preview.src = e.target.result;
                    preview.classList.remove('d-none');
                };
                reader.readAsDataURL(input.files[0]);
            }
        }

        function thietLapGoiYDK() {
            var input = document.getElementById('txtTimDK');
            var list = document.getElementById('goiYDK');
            var btn = document.getElementById('btnTimDK');
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

        window.addEventListener('beforeunload', function () {
            sessionStorage.setItem('scrollPos_DangKy', window.scrollY);
        });

        window.addEventListener('load', function () {
            var savedScroll = sessionStorage.getItem('scrollPos_DangKy');
            if (savedScroll !== null) {
                window.scrollTo({ top: parseInt(savedScroll, 10), behavior: 'instant' });
            }
        });

        window.addEventListener('DOMContentLoaded', function () {
            thietLapGoiYDK();

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

function moModalXemBienLaiHoanHV() { showBootstrapModal('modalXemBienLaiHoanHV'); }
    </script>
</asp:Content>