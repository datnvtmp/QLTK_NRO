# QLTK_NRO (Quản Lý Tài Khoản Ngũ Rồng Online)

Phần mềm hỗ trợ quản lý tài khoản và tự động luyện tập (auto bot) cho game **Ngũ Rồng Online** (NRO). Dự án bao gồm bộ quản lý tài khoản Lite (`QLTK-Lite`) và client game tích hợp render Skia (`NRO_Skia`).

## 🌟 Tính năng chính

### 1. Quản lý tài khoản thông minh
*   Hỗ trợ thêm, sửa, xóa tài khoản theo lô.
*   Quản lý Proxy riêng biệt cho từng tài khoản để chống khóa IP.
*   Tính năng nhập/xuất danh sách tài khoản qua định dạng TXT hoặc JSON.
*   Sắp xếp và lọc tài khoản linh hoạt.

### 2. Hệ thống Auto Bot mạnh mẽ
*   **Auto Train**: Tự chọn Map, khu vực và loại quái để treo.
*   **Lập lịch thông minh**: Hẹn giờ tự động bật/tắt bot theo thời gian thực.
*   **Chống KS**: Tự động chuyển khu hoặc né tránh khi có người chơi khác tranh bãi.
*   **Quản lý vật phẩm**: Tự động sử dụng hoặc vứt vật phẩm theo ID cấu hình.
*   **Skill Combo**: Tự động sử dụng kỹ năng theo danh sách ID thiết lập.

### 3. Điều khiển tập trung
*   Mở/Đóng hàng loạt client game chỉ với một nút bấm.
*   Chế độ chạy ẩn (hidden) để tiết kiệm tài nguyên máy tính.
*   Xem nhanh hành trang và trạng thái nhân vật mà không cần mở client.
*   Tự động sắp xếp cửa sổ game trên màn hình.

---

## 📂 Cấu trúc thư mục

*   `/QLTK-Lite`: Mã nguồn ứng dụng quản lý chính.
*   `/NRO_Skia`: Mã nguồn Game Client đã được tối ưu hóa.
*   `.gitignore`: Cấu hình loại bỏ các file rác và build artifacts.

---

## 🚀 Hướng dẫn bắt đầu

### Yêu cầu hệ thống
*   Windows OS (Hỗ trợ tốt nhất trên Windows 10/11).
*   .NET Framework 4.7.2 hoặc mới hơn.

### Cài đặt
1.  Giải nén thư mục phần mềm.
2.  Chạy file `QLTK-Lite.exe` (được build từ source trong thư mục `QLTK-Lite`).
3.  Cấu hình server và thêm tài khoản vào danh sách.
4.  Nhấn nút **BẬT** để bắt đầu treo bot.

---

## 🛠️ Công nghệ sử dụng

*   **Language**: C# (.NET Framework)
*   **UI Library**: Windows Forms, SkiaSharp (cho client game)
*   **Data Format**: JSON, TXT

---

## ⚠️ Lưu ý bảo mật
*   Đây là mã nguồn gốc của ứng dụng.
*   Vui lòng không chia sẻ Proxy hoặc thông tin tài khoản cá nhân trong mã nguồn.

---

## ✍️ Tác giả
Dự án được phát triển và duy trì bởi **Banbuaboy**.

---

### English Summary
**QLTK_NRO** is an account management and automation tool for the game "Dragon Boy Online" (Ngũ Rồng Online). It features multi-account management, proxy support, auto-training with customizable skills, scheduling, and efficient resource handling using hidden game clients. Built with C# and SkiaSharp for optimized performance.
