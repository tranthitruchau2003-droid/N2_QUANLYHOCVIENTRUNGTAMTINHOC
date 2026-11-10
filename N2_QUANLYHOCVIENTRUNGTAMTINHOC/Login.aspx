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
    </style>
</head>
<body>
    <form id="form1" runat="server">
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
                    <button type="button" class="btn btn-outline-success btn-sm" data-bs-toggle="modal" data-bs-target="#modalTraCuu">
                        <i class="bi bi-search me-1"></i> Tra cứu & Xác thực Chứng chỉ
                    </button>
                </div>
            </div>
        </div>

        <!-- POPUP HỌC VIÊN TỰ ĐĂNG KÝ -->
        <div class="modal fade" id="modalDangKy" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content border-0 shadow">
                    <div class="modal-header text-white" style="background: #FF8A26;">
                        <h5 class="modal-title fw-bold"><i class="bi bi-person-badge me-2"></i>Đăng ký Tài khoản Học viên</h5>
                        <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body p-4">
                        <div class="row g-2 mb-2">
                            <div class="col-12"><label class="small fw-bold">Họ và tên (*)</label><asp:TextBox ID="txtDKHoTen" runat="server" CssClass="form-control" /></div>
                            <div class="col-6"><label class="small fw-bold">Ngày sinh</label><asp:TextBox ID="txtDKNgaySinh" runat="server" TextMode="Date" CssClass="form-control" /></div>
                            <div class="col-6">
                                <label class="small fw-bold">Giới tính</label>
                                <asp:DropDownList ID="ddlDKGioiTinh" runat="server" CssClass="form-select">
                                    <asp:ListItem Text="Nam" Value="Nam" /><asp:ListItem Text="Nữ" Value="Nữ" />
                                </asp:DropDownList>
                            </div>
                            <div class="col-6"><label class="small fw-bold">Điện thoại (*)</label><asp:TextBox ID="txtDKDienThoai" runat="server" CssClass="form-control" /></div>
                            <div class="col-6"><label class="small fw-bold">Email</label><asp:TextBox ID="txtDKEmail" runat="server" CssClass="form-control" /></div>
                            <div class="col-12"><label class="small fw-bold">Địa chỉ</label><asp:TextBox ID="txtDKDiaChi" runat="server" CssClass="form-control" /></div>
                            <div class="col-12"><label class="small fw-bold">Mật khẩu đăng nhập (*)</label><asp:TextBox ID="txtDKMatKhau" runat="server" TextMode="Password" CssClass="form-control" /></div>
                        </div>
                    </div>
                    <div class="modal-footer justify-content-between bg-light">
                        <small class="text-danger">(*) Bắt buộc nhập</small>
                        <div>
                            <button type="button" class="btn btn-secondary btn-sm" data-bs-dismiss="modal">Hủy</button>
                            <asp:Button ID="btnDangKyTK" runat="server" Text="Đăng ký & Đăng nhập ngay" CssClass="btn btn-orange btn-sm" OnClick="btnDangKyTK_Click" />
                        </div>
                    </div>
                </div>
            </div>
        </div>

       
    </form>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/js/bootstrap.bundle.min.js"></script>
    <script>
        // @ts-nocheck
        function openModal(id) {
            var el = document.getElementById(id);
            if (el && window.bootstrap) {
                var myModal = new window.bootstrap.Modal(el);
                myModal.show();
            }
        }
    </script>
</body>
</html>