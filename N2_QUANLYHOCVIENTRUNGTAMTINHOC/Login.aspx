<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="N2_QUANLYHOCVIENTRUNGTAMTINHOC.Login" %>

<!DOCTYPE html>
<html lang="vi">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Đăng nhập & Tra cứu Chứng chỉ - Trung tâm Tin học</title>
    <link href="/Content/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet" />
    <style>
        body { background: #f0f2f5; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; display: flex; align-items: center; justify-content: center; min-height: 100vh; margin: 0; }
        .login-card { width: 100%; max-width: 400px; background: #fff; border-radius: 12px; box-shadow: 0 4px 20px rgba(0,0,0,0.08); overflow: hidden; }
        .login-header { background: linear-gradient(135deg, #1677D2, #0B5394); color: #fff; padding: 25px 20px; text-align: center; }
        .btn-orange { background: linear-gradient(135deg, #FF8A26, #E67215); color: #fff; border: none; font-weight: 600; }
        .btn-orange:hover { background: linear-gradient(135deg, #E67215, #CC620E); color: #fff; }
        input::-ms-reveal, input::-ms-clear { display: none !important; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager runat="server" />

        <div class="login-card">
            <div class="login-header">
                <i class="bi bi-mortarboard-fill display-4 mb-2 d-block text-warning"></i>
                <h4 class="fw-bold mb-0">HỆ THỐNG QUẢN LÝ</h4>
                <small class="text-white-50">Trung tâm Tin học Ngôi Sao Việt</small>
            </div>
            <div class="p-4">
                <asp:Panel ID="pnlLoi" runat="server" Visible="false" CssClass="alert alert-danger py-2 small fw-semibold text-center">
                    <asp:Literal ID="litLoi" runat="server" />
                </asp:Panel>
                
                <div class="mb-3">
                    <label class="small fw-bold text-muted">Tên đăng nhập / Mã tài khoản</label>
                    <div class="input-group">
                        <span class="input-group-text bg-light text-muted"><i class="bi bi-person"></i></span>
                        <asp:TextBox ID="txtTaiKhoan" runat="server" CssClass="form-control" placeholder="Nhập mã, email hoặc SĐT..." />
                    </div>
                </div>
                <div class="mb-3">
                    <label class="small fw-bold text-muted">Mật khẩu</label>
                    <div class="input-group">
                        <span class="input-group-text bg-light text-muted"><i class="bi bi-lock"></i></span>
                        <asp:TextBox ID="txtMatKhau" runat="server" CssClass="form-control" TextMode="Password" placeholder="Nhập mật khẩu..." />
                    </div>
                </div>
                <asp:Button ID="btnDangNhap" runat="server" Text="ĐĂNG NHẬP" CssClass="btn btn-primary w-100 fw-bold mb-3 py-2" OnClick="btnDangNhap_Click" />
                
                <hr class="text-muted" />
                <div class="d-grid gap-2">
                    <button type="button" class="btn btn-orange btn-sm" data-bs-toggle="modal" data-bs-target="#modalDangKy">
                        <i class="bi bi-person-plus-fill me-1"></i> Học viên Tự Đăng ký Tài khoản
                    </button>
                </div>
            </div>
        </div>

        <!-- POPUP HỌC VIÊN TỰ ĐĂNG KÝ -->
        <div class="modal fade" id="modalDangKy" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content border-0 shadow">
                    <asp:UpdatePanel ID="upDangKy" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <div class="modal-header text-white" style="background: #FF8A26;">
                                <h5 class="modal-title fw-bold"><i class="bi bi-person-badge me-2"></i>Đăng ký Tài khoản Học viên</h5>
                                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                            </div>
                            <div class="modal-body p-4">
                                <!-- Khung báo lỗi trực tiếp màu đỏ bên trong modal -->
                                <div id="divLoiDK" class="alert alert-danger py-2 small fw-semibold mb-3 d-none">
                                    <i class="bi bi-exclamation-circle-fill me-1"></i>
                                    <span id="txtLoiDK"></span>
                                </div>
                                <asp:Panel ID="pnlLoiDK" runat="server" Visible="false" CssClass="alert alert-danger py-2 small fw-semibold mb-3">
                                    <i class="bi bi-exclamation-circle-fill me-1"></i>
                                    <asp:Literal ID="litLoiDK" runat="server" />
                                </asp:Panel>

                                <div class="row g-2 mb-2">
                                    <div class="col-12">
                                        <label class="small fw-bold">Họ và tên (*)</label>
                                        <asp:TextBox ID="txtDKHoTen" runat="server" ClientIDMode="Static" CssClass="form-control" oninput="anLoiDK();" />
                                    </div>
                                    <div class="col-6">
                                        <label class="small fw-bold">Ngày sinh</label>
                                        <asp:TextBox ID="txtDKNgaySinh" runat="server" ClientIDMode="Static" TextMode="Date" CssClass="form-control" />
                                    </div>
                                    <div class="col-6">
                                        <label class="small fw-bold">Giới tính</label>
                                        <asp:DropDownList ID="ddlDKGioiTinh" runat="server" ClientIDMode="Static" CssClass="form-select">
                                            <asp:ListItem Text="Nam" Value="Nam" />
                                            <asp:ListItem Text="Nữ" Value="Nữ" />
                                        </asp:DropDownList>
                                    </div>
                                    <div class="col-6">
                                        <label class="small fw-bold">Điện thoại (*)</label>
                                        <asp:TextBox ID="txtDKDienThoai" runat="server" ClientIDMode="Static" CssClass="form-control" oninput="anLoiDK();" placeholder="0xxxxxxxxx" />
                                    </div>
                                    <div class="col-6">
                                        <label class="small fw-bold">Email</label>
                                        <asp:TextBox ID="txtDKEmail" runat="server" ClientIDMode="Static" CssClass="form-control" oninput="anLoiDK();" />
                                    </div>
                                    <div class="col-12">
                                        <label class="small fw-bold">Địa chỉ</label>
                                        <asp:TextBox ID="txtDKDiaChi" runat="server" ClientIDMode="Static" CssClass="form-control" />
                                    </div>
                                    <div class="col-12">
                                        <label class="small fw-bold">Mật khẩu đăng nhập (*)</label>
                                        <div class="input-group">
                                            <asp:TextBox ID="txtDKMatKhau" runat="server" ClientIDMode="Static" TextMode="Password"
                                                CssClass="form-control" autocomplete="new-password" oninput="anLoiDK();" placeholder="Tối thiểu 6 ký tự..." />
                                            <button type="button" class="btn btn-outline-secondary" tabindex="-1" onclick="toggleHienMatKhauLogin('txtDKMatKhau', this)" title="Hiện/Ẩn mật khẩu">
                                                <i class="bi bi-eye"></i>
                                            </button>
                                        </div>
                                    </div>
                                    <div class="col-12">
                                        <label class="small fw-bold">Xác nhận mật khẩu (*)</label>
                                        <div class="input-group">
                                            <asp:TextBox ID="txtDKMatKhauXN" runat="server" ClientIDMode="Static" TextMode="Password"
                                                CssClass="form-control" autocomplete="new-password" oninput="anLoiDK();" placeholder="Nhập lại mật khẩu..." />
                                            <button type="button" class="btn btn-outline-secondary" tabindex="-1" onclick="toggleHienMatKhauLogin('txtDKMatKhauXN', this)" title="Hiện/Ẩn mật khẩu">
                                                <i class="bi bi-eye"></i>
                                            </button>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="modal-footer justify-content-between bg-light">
                                <small class="text-danger">(*) Bắt buộc nhập</small>
                                <div>
                                    <button type="button" class="btn btn-secondary btn-sm" data-bs-dismiss="modal">Hủy</button>
                                    <asp:Button ID="btnDangKyTK" runat="server" Text="Đăng ký & Đăng nhập ngay" CssClass="btn btn-orange btn-sm fw-bold"
                                        OnClientClick="return kiemTraFormDangKy();" OnClick="btnDangKyTK_Click" />
                                </div>
                            </div>
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="btnDangKyTK" EventName="Click" />
                        </Triggers>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
    </form>

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/js/bootstrap.bundle.min.js"></script>
    <script>
        // @ts-nocheck
        function toggleHienMatKhauLogin(inputId, btn) {
            var inp = document.getElementById(inputId);
            if (!inp) return;
            var icon = btn.querySelector('i');
            if (inp.type === 'password') {
                inp.type = 'text';
                if (icon) { icon.classList.remove('bi-eye'); icon.classList.add('bi-eye-slash'); }
            } else {
                inp.type = 'password';
                if (icon) { icon.classList.remove('bi-eye-slash'); icon.classList.add('bi-eye'); }
            }
        }

        function anLoiDK() {
            var div = document.getElementById('divLoiDK');
            if (div && !div.classList.contains('d-none')) {
                div.classList.add('d-none');
            }
        }

        function hienLoiDKClient(msg) {
            var div = document.getElementById('divLoiDK');
            var span = document.getElementById('txtLoiDK');
            if (div && span) {
                span.innerText = msg;
                div.classList.remove('d-none');
            }
        }

        function kiemTraFormDangKy() {
            anLoiDK();
            var hoTen = (document.getElementById('txtDKHoTen')?.value || '').trim();
            var sdt = (document.getElementById('txtDKDienThoai')?.value || '').trim();
            var mk = (document.getElementById('txtDKMatKhau')?.value || '').trim();
            var mkXN = (document.getElementById('txtDKMatKhauXN')?.value || '').trim();

            if (!hoTen) {
                hienLoiDKClient('Vui lòng nhập Họ và tên!');
                document.getElementById('txtDKHoTen')?.focus();
                return false;
            }
            if (!sdt || !/^0\d{9}$/.test(sdt)) {
                hienLoiDKClient('Số điện thoại không hợp lệ (phải gồm 10 chữ số và bắt đầu bằng số 0)!');
                document.getElementById('txtDKDienThoai')?.focus();
                return false;
            }
            if (!mk) {
                hienLoiDKClient('Vui lòng nhập Mật khẩu đăng nhập!');
                document.getElementById('txtDKMatKhau')?.focus();
                return false;
            }
            if (mk.indexOf(' ') !== -1) {
                hienLoiDKClient('Mật khẩu không được chứa khoảng trắng!');
                document.getElementById('txtDKMatKhau')?.focus();
                return false;
            }
            if (mk.length < 6 || mk.length > 50) {
                hienLoiDKClient('Mật khẩu phải từ 6 đến 50 ký tự!');
                document.getElementById('txtDKMatKhau')?.focus();
                return false;
            }
            if (mk === sdt) {
                hienLoiDKClient('Mật khẩu không được trùng với Số điện thoại!');
                document.getElementById('txtDKMatKhau')?.focus();
                return false;
            }
            if (!mkXN) {
                hienLoiDKClient('Vui lòng nhập Xác nhận mật khẩu!');
                document.getElementById('txtDKMatKhauXN')?.focus();
                return false;
            }
            if (mk !== mkXN) {
                hienLoiDKClient('Xác nhận mật khẩu mới không khớp!');
                document.getElementById('txtDKMatKhauXN')?.focus();
                return false;
            }
            return true;
        }
    </script>
</body>
</html>