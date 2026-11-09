<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="N2_QUANLYHOCVIENTRUNGTAMTINHOC.Login" %>
<!DOCTYPE html>
<html lang="vi">
<head runat="server">
    <meta charset="utf-8" />
    <title>Đăng nhập Hệ thống - Quản lý Trung tâm Tin học</title>
    <link href="/Content/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet" />
    <link href="/Content/css/theme.css" rel="stylesheet" />
    <style>
        .login-bg {
            min-height: 100vh;
            background: linear-gradient(135deg, #1677D2, #0755A6);
            display: flex;
            align-items: center;
            justify-content: center;
        }
        .login-card {
            width: 100%;
            max-width: 420px;
            background: #fff;
            border-radius: 14px;
            box-shadow: 0 12px 32px rgba(0,0,0,0.22);
            padding: 2.2rem;
        }
    </style>
</head>
<body class="login-bg">
    <form id="form1" runat="server">
        <div class="login-card">
            <div class="text-center mb-4">
                <div class="d-inline-flex align-items-center justify-content-center bg-primary-subtle text-primary rounded-circle mb-2" style="width:60px;height:60px;">
                    <i class="bi bi-mortarboard-fill fs-2"></i>
                </div>
                <h4 class="fw-bold text-primary mb-1">QUẢN LÝ ĐÀO TẠO</h4>
                <p class="text-muted small mb-0">Hệ thống Quản lý Học viên Trung tâm Tin học</p>
            </div>

            <asp:Panel ID="pnlLoi" runat="server" Visible="false" CssClass="alert alert-danger py-2 small">
                <asp:Literal ID="litLoi" runat="server" />
            </asp:Panel>

            <div class="mb-3">
                <label class="form-label fw-semibold">Tên đăng nhập</label>
                <div class="input-group">
                    <span class="input-group-text"><i class="bi bi-person"></i></span>
                    <asp:TextBox ID="txtTenDangNhap" runat="server" CssClass="form-control" placeholder="Nhập tài khoản (admin)" required="required" />
                </div>
            </div>

            <div class="mb-4">
                <label class="form-label fw-semibold">Mật khẩu</label>
                <div class="input-group">
                    <span class="input-group-text"><i class="bi bi-lock"></i></span>
                    <asp:TextBox ID="txtMatKhau" runat="server" CssClass="form-control" TextMode="Password" placeholder="Nhập mật khẩu" required="required" />
                </div>
            </div>

            <asp:Button ID="btnDangNhap" runat="server" Text="ĐĂNG NHẬP HỆ THỐNG" CssClass="btn btn-orange w-100 py-2" OnClick="btnDangNhap_Click" />
        </div>
    </form>
</body>
</html>