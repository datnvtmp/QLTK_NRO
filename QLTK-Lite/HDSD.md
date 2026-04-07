
## 1. Cài đặt & Khởi động

### Cấu trúc thư mục quan trọng
- `QLTK-Lite.exe`: File thực thi chính để mở phần mềm.
- `lib/NRO_Skia.exe`: File game client (Bắt buộc phải có để chạy bot).
- `lib/maps.txt`: Danh sách bản đồ (Có thể chỉnh sửa để cập nhật Map mới).
- `Config/`: Thư mục lưu trữ cấu hình server và ứng dụng.

### Khởi động lần đầu
1. Giải nén toàn bộ thư mục phần mềm.
2. Chạy file `QLTK-Lite.exe`.
3. Nếu có phiên bản mới, phần mềm sẽ tự động thông báo và hỗ trợ cập nhật.
4. Đăng nhập/Kích hoạt bản quyền (nếu yêu cầu).

---

## 2. Quản lý tài khoản

Giao diện chính cho phép bạn quản lý danh sách tài khoản một cách trực quan.

### Thêm tài khoản mới
- Nhập **Tài khoản**, **Mật khẩu** tại bảng bên phải.
- Chọn **Máy chủ**.
- Nhập **Proxy**
- Nhấn nút **Thêm**.

### Chỉnh sửa & Xóa
- Chọn tài khoản trong danh sách (Grid).
- Sửa thông tin ở bảng bên phải rồi nhấn **Sửa**, hoặc nhấn **Xóa** để loại bỏ.

### Nhập/Xuất dữ liệu (Chuột phải vào danh sách)
- **Nhập từ file TXT**: Định dạng mỗi dòng: `user|pass|server_index|proxy` (server_index bắt đầu từ 1).
- **Nhập/Xuất JSON**: Dùng để sao lưu toàn bộ cấu hình tài khoản và bot.

### Tính năng chọn nhiều & Thao tác hàng loạt
Phần mềm hỗ trợ xử lý nhiều tài khoản cùng lúc để tiết kiệm thời gian:
- **Chọn nhiều hàng**: Giữ phím `Ctrl` hoặc `Shift` và click chuột để chọn nhiều tài khoản trong danh sách.
- **Sửa nhanh Proxy**: Khi chọn nhiều acc, nhập Proxy mới ở bảng bên phải và nhấn **Sửa**. Toàn bộ các acc đang chọn sẽ được cập nhật Proxy này.
- **Xóa hàng loạt**: Chọn nhiều acc và nhấn **Xóa** để loại bỏ nhanh.
- **Bật/Tắt Auto hàng loạt**: Chọn nhiều acc, chuột phải và chọn **Chọn auto** (để tích dấu `✓`) hoặc **Bỏ chọn auto**.
- **Lưu cấu hình hàng loạt**: Khi bạn chỉnh sửa thông tin trong các tab (Auto Train, Săn Boss...), nhấn **Lưu cấu hình** sẽ áp dụng cài đặt đó cho **tất cả** các tài khoản đang được chọn trong danh sách.

---

## 3. Điều khiển Bot

### Bắt đầu/Dừng Bot
- **Nút BẬT (Xanh)**: Mở các game client cho những tài khoản được tích chọn (cột `✓`).
- **Nút TẮT (Đỏ)**: Đóng toàn bộ game client đang chạy.
- **Mũi tên Lên/Xuống**: Sắp xếp thứ tự ưu tiên mở tài khoản.

### Menu ngữ cảnh (Chuột phải)
- **Mở remote (GUI)**: Hiện cửa sổ game để thao tác trực tiếp.
- **Đóng remote**: Ẩn cửa sổ game về chế độ chạy ngầm.
- **Sắp xếp remote**: Tự động dàn hàng các cửa sổ game trên màn hình.
- **Hành trang**: Xem nhanh vật phẩm trong túi đồ của nhân vật.
- **Đóng process**: Chỉ đóng tiến trình của tài khoản đang chọn.

---

## 4. Cấu hình Auto Train

Tab này dùng để thiết lập hành vi tự động luyện tập.

- **Cột Train**:
    - **Map/Khu**: Chọn bản đồ và khu vực muốn treo.
    - **Loại quái**: Chọn đánh tất cả, quái đất, quái bay hoặc theo ID cụ thể.
    - **List skill**: Nhập ID skill muốn sử dụng (ví dụ: `0,1,2`).
- **Cột Item & Linh tính**:
    - **ID Dùng/Vứt**: Tự động sử dụng hoặc bỏ vật phẩm theo ID.
    - **Bông tai/Cờ đen**: Tự động thao tác trạng thái nhân vật.
    - **Chống KS**: Nhảy khu hoặc né khi có người chơi khác tranh quái.
    - **Đứng im**: Chỉ đứng tại chỗ đánh quái xung quanh.
- **Cột Lịch (Lập lịch)**:
    - **Tự ON/OFF**: Hẹn giờ mở hoặc tắt máy theo thời gian thực (Giờ:Phút).
    - **OFF sau**: Tự động tắt sau một khoảng thời gian treo (ví dụ: treo 2 tiếng rồi nghỉ).

**Lưu ý**: Sau khi chỉnh sửa, nhấn **Lưu cấu hình** để áp dụng.
