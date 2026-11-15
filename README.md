TÀI LIỆU ĐẶC TẢ HỆ THỐNG & LUỒNG HOẠT ĐỘNG
QUẢN LÝ HỌC VIÊN TRUNG TÂM TIN HỌC
Dự án: N2_QUANLYHOCVIENTRUNGTAMTINHOC

1. Giới thiệu tổng quan
Hệ thống Quản lý Học viên Trung tâm Tin học được xây dựng nhằm số hóa toàn bộ các hoạt động vận hành tại trung tâm, giúp thay thế các quy trình thủ công bằng giải pháp phần mềm tự động, minh bạch và chính xác. Hệ thống hỗ trợ quản lý xuyên suốt từ khâu học viên đăng ký, xếp lớp, điểm danh, đóng học phí qua mã QR đến khi thi kết thúc khóa học và cấp chứng chỉ.
2. Phân quyền người dùng (Ai làm gì trong hệ thống?)
•	Quản trị viên (Admin): Có quyền cao nhất trong hệ thống. Quản lý toàn bộ nhân sự, phân quyền, thực hiện sao lưu/phục hồi cơ sở dữ liệu (.bak), xem nhật ký hoạt động (System Logs), quản lý lớp học và xử lý các lớp chưa đủ sĩ số (ghép lớp, mở đặc cách hoặc hủy lớp hoàn tiền).
•	Nhân viên Giáo vụ / Kế toán (NhanVien): Hỗ trợ quản lý danh mục học viên, giảng viên, khóa học; lập phiếu thu học phí trực tiếp hoặc duyệt ảnh biên lai chuyển khoản QR của học viên; xử lý hoàn tiền khi lớp học bị hủy và tổ chức các kỳ thi.
•	Giảng viên (GiangVien): Xem lịch giảng dạy cá nhân, thực hiện điểm danh học viên theo từng buổi học (hệ thống tự động trừ điểm chuyên cần và cảnh báo cấm thi nếu nghỉ >= 3 buổi), nhập điểm thi (80%) và xem điểm tổng kết.
•	Học viên (HocVien): Tự đăng ký tài khoản học viên, tra cứu lịch học và thời khóa biểu trong tuần, đăng ký lớp học phần, đóng học phí qua quét mã VietQR kèm tải lên ảnh biên lai, yêu cầu hoàn tiền nếu lớp bị hủy, tra cứu điểm thi và tải/in chứng chỉ cá nhân.
3. Luồng hoạt động chi tiết của hệ thống (Workflow)
3.1. Luồng của Học viên (Từ đăng ký đến nhận chứng chỉ)
1. Đăng ký tài khoản: Học viên truy cập trang đăng ký, điền thông tin cá nhân và mật khẩu để tạo tài khoản riêng.
2. Đăng ký lớp học: Học viên xem danh sách các lớp đang mở, chọn lớp phù hợp trước ngày khai giảng và tiến hành xác nhận đăng ký.
3. Đóng học phí: Chọn tính năng 'Đóng HP', hệ thống hiển thị mã VietQR chuyển khoản đúng số tiền và nội dung. Học viên chuyển khoản và tải ảnh chụp màn hình biên lai lên hệ thống.
4. Đi học & Điểm danh: Tham gia lớp học theo thời khóa biểu. Giảng viên điểm danh chuyên cần hằng ngày.
5. Thi & Nhận chứng chỉ: Tham gia kỳ thi cuối khóa. Sau khi có kết quả Đạt (ĐTB >= 5.0), học viên có thể tra cứu và in chứng chỉ trực tuyến.
3.2. Luồng của Giảng viên (Quản lý lớp & Nhập điểm)
1. Xem lịch dạy: Đăng nhập để xem danh sách các lớp và thời khóa biểu chi tiết do mình phụ trách.
2. Điểm danh buổi học: Chọn lớp, chọn ngày điểm danh và đánh giá trạng thái từng học viên (Có mặt, Vắng phép, Vắng không phép). Hệ thống tự động cảnh báo nếu học viên nghỉ từ 2-3 buổi trở lên.
3. Nhập điểm thi: Chọn kỳ thi của lớp, hệ thống tự động tính điểm chuyên cần (20%), giảng viên nhập điểm thi (80%), hệ thống tự động tính điểm trung bình và phân loại Đạt / Không đạt.
3.3. Luồng Quản lý & Vận hành của Trung tâm (Admin / Nhân viên)
1. Thiết lập danh mục: Khởi tạo danh sách khóa học, học phí, thời lượng, thêm thông tin giảng viên và học viên.
2. Mở lớp & Xếp lịch: Tạo lớp học phần mới, gán giảng viên, phòng học lab và lịch học cố định (hệ thống tự động tính ngày kết thúc cộng thêm 1 tuần dự trữ).
3. Duyệt học phí: Kiểm tra ảnh biên lai chuyển khoản QR do học viên gửi lên tại giao diện quản lý phiếu thu và bấm 'Xác nhận Đã đóng' để cập nhật công nợ.
4. Xử lý lớp thiếu sĩ số: Khi đến ngày khai giảng mà lớp có dưới 10 học viên, Admin chọn xử lý: Ghép sang lớp khác, Mở lớp đặc cách, hoặc Hủy lớp và hỗ trợ học viên làm thủ tục nhận hoàn tiền học phí.
4. Các tính năng cốt lõi và tiện ích thông minh
•	Thời khóa biểu thông minh theo tuần: Hiển thị trực quan lịch học theo 3 ca (Sáng, Chiều, Tối) từ Thứ 2 đến Chủ Nhật, hỗ trợ giáo vụ báo nghỉ đột xuất và xếp lịch bù vào tuần dự trữ cuối khóa.
•	Tích hợp thanh toán VietQR: Tự động tạo mã QR chứa chính xác số tiền học phí và cú pháp nội dung chuyển khoản, giúp giảm thiểu sai sót khi thu ngân sách.
•	Cảnh báo học tập tự động: Tự động tính điểm chuyên cần từ dữ liệu điểm danh, cảnh báo học viên nghỉ 2 buổi và tự động cấm thi nếu nghỉ từ 3 buổi trở lên hoặc còn nợ học phí.
•	Cấp và in chứng chỉ trực tuyến: Tự động sinh số hiệu chứng chỉ theo chuẩn (TTTH-YYYY-XXXX), xếp loại học lực dựa trên điểm trung bình và cung cấp giao diện in chứng chỉ mẫu trang trọng.
