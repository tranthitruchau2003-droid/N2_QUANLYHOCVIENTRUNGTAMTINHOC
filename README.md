<p align="center">
  <img src="https://img.icons8.com/color/96/graduation-cap.png" width="80" alt="Logo Trung Tâm Tin Học">
</p>

<h1 align="center">Hệ thống Quản lý Học viên Trung tâm Tin học</h1>

<p align="center">
  <b>N2_QUANLYHOCVIENTRUNGTAMTINHOC</b> là giải pháp phần mềm toàn diện giúp số hóa toàn bộ quy trình đào tạo, quản lý học phí tích hợp VietQR, điểm danh chuyên cần, khảo thí và cấp chứng chỉ trực tuyến.
</p>

<p align="center">
  <em>Được phát triển trên nền tảng .NET Framework, ASP.NET Web Forms, C#, ADO.NET và Microsoft SQL Server.</em>
</p>

<p align="center">
  <a href="#-hướng-dẫn-nhanh"></a> &nbsp;&bull;&nbsp;
  <a href="#-phân-quyền-người-dùng">Phân quyền</a> &nbsp;&bull;&nbsp;
  <a href="#-luồng-hoạt-động-workflow">Luồng hoạt động</a> &nbsp;&bull;&nbsp;
  <a href="#-tính-năng-nổi-bật">Tính năng</a>
</p>

---

## `>_` Hướng dẫn nhanh (Quickstart)

Để khởi chạy và trải nghiệm dự án trên Visual Studio:

```bash
# 1. Clone hoặc tải mã nguồn về máy
git clone https://github.com/tranthitruchau2003-droid/N2_QUANLYHOCVIENTRUNGTAMTINHOC.git

# 2. Mở tệp giải pháp (Solution) bằng Visual Studio
N2_QUANLYHOCVIENTRUNGTAMTINHOC.sln

# 3. Cấu hình chuỗi kết nối CSDL trong Web.config và chạy ứng dụng (F5)
```


---

## 👥 Phân quyền người dùng

Hệ thống phân chia rõ ràng 4 nhóm vai trò để đảm bảo tính bảo mật và phân tách nghiệp vụ:

| Vai trò | Mô tả & Quyền hạn chính |
| :--- | :--- |
| 👑 **Admin (Quản trị viên)** | Toàn quyền quản trị hệ thống, nhân sự, phân quyền, sao lưu/phục hồi CSDL (`.bak`), xem nhật ký hệ thống (`System Logs`) và xử lý lớp học. |
| 💼 **NhanVien (Giáo vụ / Kế toán)** | Quản lý danh mục học viên, giảng viên, khóa học; lập phiếu thu và duyệt ảnh biên lai chuyển khoản QR; xử lý hoàn tiền lớp học hủy. |
| 👨‍🏫 **GiangVien (Giảng viên)** | Xem lịch giảng dạy, điểm danh chuyên cần hằng buổi (tự động tính điểm CC, cảnh báo nghỉ 2 buổi, cấm thi khi nghỉ $\ge 3$ buổi) và nhập điểm thi. |
| 🎓 **HocVien (Học viên)** | Tự đăng ký tài khoản, tra cứu lịch học/TKB, đăng ký học phần, đóng học phí qua mã VietQR, yêu cầu hoàn tiền và tải/in chứng chỉ cá nhân. |

---

## 🔄 Luồng hoạt động (Workflow)

### 1. Luồng của Học viên
1. **Đăng ký tài khoản:** Truy cập cổng thông tin, nhập thông tin cá nhân và mật khẩu an toàn.
2. **Đăng ký lớp học:** Lựa chọn lớp học phần đang mở trước ngày khai giảng.
3. **Đóng học phí QR:** Bấm **Đóng HP**, quét mã VietQR tự động sinh nội dung (`HOCPHI [MãĐK]`) và tải ảnh biên lai lên hệ thống chờ duyệt.
4. **Đi học & Điểm danh:** Tham gia lớp học theo thời khóa biểu; giáo viên điểm danh chuyên cần hằng ngày.
5. **Thi & Nhận chứng chỉ:** Tham gia kỳ thi cuối khóa, tra cứu kết quả Đạt ($\text{ĐTB} \ge 5.0$) và in chứng chỉ trực tuyến.

### 2. Luồng của Giảng viên
1. **Xem lịch dạy:** Theo dõi danh sách lớp học và lưới thời khóa biểu chi tiết do mình phụ trách.
2. **Điểm danh chuyên cần:** Đánh giá trạng thái từng học viên (*Có mặt, Vắng phép, Vắng không phép*). Hệ thống tự động trừ điểm chuyên cần và cảnh báo cấm thi.
3. **Nhập điểm thi:** Nhập điểm thi (80%), kết hợp chuyên cần (20%) để tự động tính Điểm trung bình và phân loại Đạt / Không đạt.

### 3. Luồng Quản lý & Vận hành (Admin / Giáo vụ)
1. **Thiết lập danh mục:** Khởi tạo khóa học, học phí, thời lượng và thông tin giảng viên/học viên.
2. **Mở lớp & Xếp lịch:** Tạo lớp học phần mới, gán giảng viên, phòng học lab và lịch học cố định (tự động cộng 1 tuần dự trữ).
3. **Duyệt học phí:** Kiểm tra ảnh biên lai chuyển khoản QR do học viên gửi lên tại giao diện quản lý phiếu thu và xác nhận đóng tiền.
4. **Xử lý lớp thiếu sĩ số:** Khi lớp có dưới 10 học viên vào ngày khai giảng, Admin thực hiện *Ghép lớp, Mở đặc cách*, hoặc *Hủy lớp và hoàn tiền học phí qua QR*.

---

## ✨ Tính năng nổi bật

* **📅 Thời khóa biểu thông minh theo tuần:** Trực quan theo 3 ca (*Sáng, Chiều, Tối*) từ T2 đến CN, hỗ trợ giáo vụ báo nghỉ đột xuất và xếp lịch bù vào tuần dự trữ cuối khóa.
* **💳 Tích hợp thanh toán VietQR:** Tự động tạo mã QR chính xác số tiền và cú pháp chuyển khoản, hạn chế tối đa sai sót tài chính.
* **⚠️ Cảnh báo học tập tự động:** Tự động tính điểm chuyên cần từ điểm danh, cảnh báo nghỉ 2 buổi và cấm thi khi nghỉ từ 3 buổi trở lên.
* **📜 Cấp & In chứng chỉ trực tuyến:** Sinh số hiệu chuẩn hóa (`TTTH-YYYY-XXXX`), xếp loại dựa trên ĐTB và cung cấp mẫu in chứng chỉ trang trọng.


---
<p align="center">
  <b>Hệ thống Quản lý Học viên Trung tâm Tin học &copy; 2026</b>
</p>
