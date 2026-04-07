# Tóm tắt Logic Class Panel (NRO_Skia)

Tài liệu này tóm tắt cấu trúc và logic quan trọng của file `Panel.cs` (hơn 10,000 dòng code) để hỗ trợ việc tra cứu và hiểu code nhanh chóng.

## 1. Thông tin chung
- **File:** `NRO_Skia/Core/Panel.cs`
- **Chức năng:** Quản lý toàn bộ hệ thống menu, hành trang, cửa hàng, giao dịch, bang hội, bản đồ... trong game.
- **Interface:** Thực thi `IActionListener` (xử lý sự kiện) và `IChatable` (nhập liệu văn bản).

## 2. Các loại Menu (Constants)
Class sử dụng biến `type` để xác định loại bảng đang hiển thị:
- `TYPE_MAIN = 0`: Hành trang, Tiềm năng, Bang hội.
- `TYPE_SHOP = 1`: Cửa hàng NPC.
- `TYPE_BOX = 2`: Rương đồ.
- `TYPE_ZONE = 3`: Đổi khu vực.
- `TYPE_MAP = 4`: Bản đồ vệ tinh.
- `TYPE_COMBINE = 12`: Ghép đồ (nâng cấp).
- `TYPE_GIAODICH = 13`: **Giao dịch giữa người chơi**.
- `TYPE_KIGUI = 17`: Ký gửi vật phẩm.

## 3. Hệ thống Giao dịch (GiaoDich)
Đây là phần logic phức tạp nhất trong Panel.

### Các biến trạng thái:
- `vMyGD`, `vFriendGD`: Vector chứa danh sách vật phẩm của mình và đối phương.
- `moneyGD`, `friendMoneyGD`: Số tiền (xu) đưa vào giao dịch.
- `isLock`, `isFriendLock`: Trạng thái đã nhấn "Khóa". Khi khóa, không thể thay đổi vật phẩm. (Màu nền đổi sang xám: `13748667`).
- `isAccept`, `isFriendAccep`: Trạng thái đã nhấn "Xong" (Xác nhận cuối cùng).

### Các hàm quan trọng:
- `paintGiaoDich(mGraphics g, bool isMe)` (Dòng ~2212): Vẽ khung giao dịch, vật phẩm, số tiền và các nút Khóa/Xong.
- `doFireGiaoDich()` (Dòng ~7283): Xử lý khi người dùng click vào vật phẩm hoặc các nút trong bảng giao dịch.
- `doFireInventory()`: Khi đang ở tab hành trang của bảng giao dịch, click vào đồ sẽ gửi yêu cầu đưa đồ lên khung GD thông qua `Service.gI()`.

## 4. Cơ chế Xử lý Sự kiện (Event Handling)
Sử dụng phương thức `perform(int idAction, object obj)` để điều hướng:
- Khi một nút hoặc hành động được kích hoạt, nó sẽ kiểm tra `type` hiện tại và gọi hàm `doFire...` tương ứng.
- Ví dụ: `case 13: doFireGiaoDich(); break;`

## 5. Lưu ý kỹ thuật cho AI & Dev
- File `Panel.cs` rất lớn, tránh đọc toàn bộ cùng lúc (tốn token/bộ nhớ). 
- Khi cần sửa logic giao diện, hãy tìm từ khóa `paintGiaoDich`.
- Khi cần sửa logic xử lý (click nút), hãy tìm `doFireGiaoDich`.
- Mọi tương tác với Server đều thông qua class `Service.gI()`.

---
*Tài liệu được khởi tạo ngày 07/04/2026 bởi Antigravity AI.*
