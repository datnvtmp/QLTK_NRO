# Tài liệu Kỹ thuật Chi tiết Class Panel.cs

Tài liệu này cung cấp cái nhìn sâu sắc về class `Panel.cs` trong project `NRO_Skia`. Với hơn 10.000 dòng code, đây là bộ não của toàn bộ giao diện người dùng (UI) trong game.

## 1. Kiến trúc Cốt lõi & Vòng đời (Life Cycle)

Class `Panel` quản lý các trạng thái UI thông qua biến `type`. Vòng đời của một bảng (panel) thường tuân theo quy trình sau:
1.  **Khởi tạo**: Một hàm `setType...()` được gọi (ví dụ: `setTypeShop()`).
2.  **Thiết lập Layout**: Hàm `setType(int position)` được gọi nội bộ để thiết lập chiều rộng (W), chiều cao (H), tọa độ (X, Y) và vùng cuộn.
3.  **Vòng lặp Cập nhật**: Các hàm `update()` và `updateKey()` xử lý các hiệu ứng hoạt họa và nhập liệu từ bàn phím/chuột.
4.  **Hiển thị (Rendering)**: Hàm `paint(mGraphics g)` sẽ điều hướng đến các hàm `paint...()` cụ thể dựa trên biến `type`.
5.  **Tương tác**: Các cú click của người dùng kích hoạt hàm `perform()`, sau đó điều hướng đến các hàm `doFire...()`.

---

## 2. Các loại Panel (`TYPE_...`)

| ID Loại | Hằng số (Constant) | Mô tả |
| :--- | :--- | :--- |
| 0 | `TYPE_MAIN` | Hành trang, Kỹ năng, Bang hội. |
| 1 | `TYPE_SHOP` | Cửa hàng NPC. |
| 2 | `TYPE_BOX` | Rương đồ công cộng/cá nhân. |
| 3 | `TYPE_ZONE` | Chọn khu vực (Zone). |
| 4 | `TYPE_MAP` | Bản đồ thế giới. |
| 6 | `TYPE_INFOMATION` | Hiển thị thông báo/thông tin chung. |
| 10 | `PLAYER_MENU` | Menu ngữ cảnh khi tương tác với người chơi khác. |
| 12 | `TYPE_COMBINE` | Giao diện ép đồ/nâng cấp vật phẩm. |
| 13 | `TYPE_GIAODICH` | **Giao dịch giữa người chơi (Trade)**. |
| 15 | `TYPE_TOP` | Bảng xếp hạng. |
| 20 | `TYPE_ACCOUNT` | Thiết lập tài khoản (Mật khẩu rương, Bạn bè, Kẻ thù...). |
| 22 | `TYPE_AUTO` | Thiết lập tự động đánh/treo máy. |

---

## 3. Danh mục các Nhóm Hàm Chính

### 3.1 Nhóm Khởi tạo (`setType...`)
Các hàm này chuẩn bị dữ liệu cho một chế độ cụ thể:
- `setTypeShop(int typeShop)`: Thiết lập cửa hàng, tên các tab và danh sách vật phẩm bán.
- `setTypeGiaoDich()`: Khởi tạo các vector giao dịch (`vMyGD`, `vFriendGD`) và chuyển sang tab giao dịch.
- `setTypeAccount()`: (Dòng 10056) Thiết lập danh sách quản lý tài khoản.

### 3.2 Nhóm Hiển thị (`paint...`)
- `paintInventory(mGraphics g)`: Vẽ các ô vật phẩm hành trang theo dạng lưới.
- `paintGiaoDich(mGraphics g, bool isMe)`: (Dòng 2212) Vẽ các ô giao dịch, nút Khóa/Xong và các trường nhập tiền.
- `paintAccount(mGraphics g)`: (Dòng 10152) Vẽ danh sách các tùy chọn tài khoản.
- `paintUpgradeEffect(...)`: (Line 10319) Hàm tĩnh xử lý hiệu ứng lấp lánh xung quanh vật phẩm khi nâng cấp thành công.

### 3.3 Nhóm Xử lý Tương tác (`doFire...`)
Chứa logic nghiệp vụ khi người dùng click nút:
- `doFireGiaoDich()`: (Dòng 7283) Xử lý việc chọn đồ từ hành trang đưa lên khung giao dịch và kích hoạt lệnh Khóa/Xong.
- `doFireAccount()`: (Dòng 10172) Xử lý các menu con như mở danh sách Bạn bè hoặc đổi mật khẩu rương.
- `doFireOption()`: (Dòng 10013) Cài đặt âm thanh và điều chỉnh kích thước màn hình.

---

## 4. Phân tích Luồng Logic Kỹ thuật

### 4.1 Cơ chế Giao dịch (Giao Dịch)
1.  **Trạng thái**: Quản lý bởi `isLock`, `isFriendLock`, `isAccept`, `isFriendAccep`.
2.  **Hình ảnh**: Vật phẩm đã khóa sẽ hiện nền tối (`13748667`).
3.  **Hoàn tất**: Khi cả hai bên đều `Locked`, nút "Xong" (done) sẽ được kích hoạt trong hàm `paintGiaoDich`.

### 4.2 Các Hàm Tiện ích UI
- `GetFont(int color)`: (Dòng 10430) Trả về font chữ phù hợp dựa trên phẩm chất vật phẩm.
- `GetColor_ItemBg(int id)`: (Dòng 10383) Ánh xạ loại vật phẩm sang màu nền tương ứng (ví dụ: Tím cho phẩm chất cấp 2, Vàng cho cấp 5).
- `GetColor_Item_Upgrade(int lv)`: (Dòng 10397) Xác định hiệu ứng hào quang dựa trên cấp độ cường hóa (ví dụ: Cấp 9 trở lên bắt đầu có màu đặc biệt).

---

## 5. Mẹo cho Lập trình viên
- **File Cực lớn**: Vì `Panel.cs` >10k dòng, hãy luôn tìm kiếm theo từ khóa `case [TYPE_ID]:` bên trong các hàm `paint`, `perform`, và `update` để tìm logic cụ thể của tính năng đó.
- **Cuộn trang (Scrolling)**: Được quản lý bởi `cmy` (tọa độ Y hiện tại) và `cmyLim` (giới hạn cuộn). Các hàm như `updateKeyScrollView()` xử lý vật lý của việc kéo/cuộn.
- **Giao tiếp Server**: Hầu hết các hàm `doFire` kết thúc bằng việc gọi `Service.gI().[Tên_Hàm]` để gửi hành động về server game.

---
*Khởi tạo ngày 07/04/2026. Tài liệu này giúp tăng tốc việc tìm kiếm và sửa lỗi hệ thống UI NRO_Skia.*
