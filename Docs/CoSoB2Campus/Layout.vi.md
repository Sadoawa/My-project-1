# B2 — bố cục bốn dãy theo mô tả ngày 11/11/2026

## Mở bản mới

Trong Unity chọn **UEH → CoSoB2 → Open four-wing campus**, hoặc mở `Assets/CoSoB2Campus/Scenes/CoSoB2_Campus.unity`, nhấn Play.

Prefab của bản bốn dãy: `Assets/CoSoB2Campus/Prefabs/CoSoB2.prefab`. Bản nghiên cứu một dãy trong `Assets/CoSoB2` được giữ lại để đối chiếu. Không ghép vào hay ghi đè scene B1/GameWorld.

- **Toàn cảnh**: hiển thị cả bốn khối và cầu kính.
- **Tầng trệt / Tầng 1–6**: chỉ giữ các đối tượng ở cao độ tương ứng, ẩn phần tường cao để xem mặt bằng/nội thất.
- Chọn dãy, rồi bấm tên phòng để vào góc nhìn bên trong; trạng thái phòng hiển thị tên và số chỗ.
- **Sân trong**, **Cầu kính**: các góc xem riêng.
- Giữ chuột phải + WASD để bay, Q/E xuống/lên, Shift để tăng tốc. Camera quan sát không có va chạm; thang máy hiện là mô hình tĩnh.

## Bố cục không gian

Quy ước đứng phía B1 nhìn vào B2: khối cũ vàng ở phía trước, hai dãy trái/phải chạy dọc hai cạnh, dãy giữa đóng phía sau, sân trong nằm giữa bốn dãy. Đây là cách bố trí suy ra từ mô tả, chưa phải mặt bằng khảo sát.

```text
                   DÃY GIỮA
           Bãi xe / tự học / lớp học
        ┌────────────────────────────┐
        │                            │
 DÃY    │         SÂN TRONG          │  DÃY
 TRÁI   │       cây xanh, ghế        │  PHẢI
        │                            │
        └────── KHỐI VÀNG CŨ ────────┘
                   cầu kính
                 hướng tới B1
```

## Công năng đã dựng

| Cao độ | Dãy trái | Dãy phải | Dãy giữa |
|---|---|---|---|
| Trệt | 2 cửa hàng sách và văn phòng phẩm | Y tế nhỏ → căn tin lớn → UEH Maker Space → cụm 2 thang máy | Bãi giữ xe mở, xe máy mô phỏng |
| Tầng 1 | 3 lớp × 80 chỗ | 3 lớp × 80 chỗ | 1 phòng tự học chiếm toàn diện tích phòng của dãy |
| Tầng 2 | 5 lớp × 40 chỗ | 5 lớp × 40 chỗ | 2 lớp lớn × 80 chỗ |
| Tầng 3 | 5 lớp × 40 chỗ | 5 lớp × 40 chỗ | 2 lớp lớn × 80 chỗ |
| Tầng 4 | 5 lớp × 40 chỗ | 5 lớp × 40 chỗ | 2 lớp lớn × 80 chỗ |
| Tầng 5 | 5 phòng máy × 40 chỗ | 5 phòng máy × 40 chỗ | 2 phòng máy lớn × 80 chỗ |
| Tầng 6 | 5 lớp × 40 chỗ | 5 lớp × 40 chỗ | 2 lớp lớn × 80 chỗ |

Chọn 80 chỗ cho lớp lớn hai bên tầng 1 (nằm trong khoảng 60–100 được yêu cầu), 5 phòng cho các tầng trên (nằm trong khoảng 4–5 được yêu cầu). Số chỗ dãy giữa, phòng tự học, căn tin và Maker Space là giả định phục vụ mô hình, không phải sức chứa đã xác nhận.

Tổng 79 phòng/khu chức năng có metadata riêng: trái 30, phải 31, giữa 12, khối cũ 6. Số này không tính hành lang, cầu thang và thang máy. Tầng 5 có **12 phòng máy**, gồm 10 phòng nhỏ ở hai bên và 2 phòng lớn ở giữa.

### Khối nối vàng cũ

| Tên tầng theo mô tả | Cao độ trong model | Công năng |
|---|---|---|
| Trệt | 0 m | 2 phòng tự học lớn hai bên cầu thang rộng |
| Tầng 2 | 4,2 m | Bên trái phòng tự học theo ảnh, bên phải kho (vị trí kho là giả định) |
| Tầng 3 | 8,4 m | Hai không gian chưa xác định công năng, để trống; cầu kính sang hướng B1 |

Tầng cũ được giữ đúng tên người dùng gọi. Vì chưa rõ cách đánh số tầng, bộ lọc xem theo cao độ: nút **Tầng 1** của dãy mới đồng thời hiện **Tầng 2** của khối cũ; nút **Tầng 2** của dãy mới hiện **Tầng 3** của khối cũ. Nhãn phòng và bảng thông tin nêu rõ sự khác biệt.

## Căn cứ hình ảnh

- Ảnh người dùng cung cấp về phòng tự học: tường vàng kem, gạch nền sáng, khung kính sáng, bàn ghế gỗ, vách chia xanh nhạt trên bàn, đèn trần vuông. Nội thất được dựng từ primitive kết hợp mesh; không dựng người.
- Ảnh cầu kính người dùng cung cấp: khung thép xám đậm, kính xanh trong, hệ giằng chéo chữ X/kim cương màu cam, các thanh ngang và dải lam phía trên. Cầu được dựng với sàn, mái kính, hai bên kính và các thanh giằng riêng.
- Video ban đầu, không dùng âm thanh: [bảng ảnh và mốc tham chiếu](../CoSoB2/Reference.vi.md), ưu tiên vòm, phào và màu khối cũ. Mô tả mới của người dùng thay thế giả định trước đây rằng B2 chỉ là một dãy thấp.

## Giả định cần người dùng kiểm tra

1. Câu “6 tầng mỗi bên” tạm được triển khai thành **trệt + tầng 1 đến tầng 6**, tổng 7 cao độ, do người dùng có nêu rõ tầng 6. Dãy giữa cũng dùng đủ các tầng được nêu. Nếu thực tế tính trệt là một trong sáu tầng thì cần hiệu chỉnh số cao độ.
2. Dãy trái/phải dài khoảng 46 m, rộng 14 m tính cả hành lang; dãy giữa rộng 60 m và sâu 12 m; cao tầng 4,2 m. Khối cũ rộng 60 m, sâu 15 m tính hiên. Tất cả là kích thước ước lượng.
3. Cụm phòng hai bên chiếm khoảng 40 m chiều dài dãy; phần cuối dành cho giao thông. Hành lang hướng sân, cầu thang dãy trái và cụm hai thang máy dãy phải là phương án bố trí mô hình. Vị trí giao thông thực tế chưa được đo.
4. Kho khối cũ đặt bên phải để giữ phòng tự học lớn bên trái; chưa có ảnh xác nhận vị trí kho. Hai phòng trống tầng 3 không được gán công năng tùy ý.
5. Cầu kính dài 13 m, rộng 3,6 m, nối phía trước trung tâm khối cũ ở cao độ 8,4 m; hướng/vị trí cụ thể và chiều dài chưa xác nhận. Chỉ đặt một chiếu nghỉ đánh dấu hướng B1, không dựng giả toàn bộ tòa B1.
6. Tầng 1 dãy giữa là một phòng lớn liền mạch; vẫn giữ hành lang tiếp cận. Không chia thành nhiều phòng.
7. Nội thất là mô hình sơ bộ có thể chỉnh sửa; cây, xe, quầy và thiết bị được giản lược. Thang máy chưa có chuyển động/call buttons hoạt động; cầu thang có bậc và ô thông sàn nhưng chưa kiểm tra luồng di chuyển của nhân vật.
8. Không bổ sung các phòng chưa được yêu cầu (ví dụ toilet/phòng kỹ thuật). Đây chưa phải thiết kế kiến trúc đủ tiêu chuẩn thi công hoặc thoát hiểm.

Danh sách từng phòng, công năng, số chỗ và kích thước ước lượng được xuất ở [RoomSchedule.csv](RoomSchedule.csv). Có thể chỉnh trực tiếp các nhóm dãy/tầng/phòng trong prefab. Đồ nội thất được gộp mesh theo vật liệu trong từng phòng để giảm số GameObject.

## Kiểm tra và ảnh xem thử

Unity 6000.6.8f1 đã biên dịch, tạo/lưu prefab và scene trong project tạm rồi render các góc dưới đây. Kiểm tra tự động đạt: 79 phòng/khu chức năng, ID không trùng, số phòng và sức chứa các dãy bên đúng cấu hình, 12 phòng máy tầng 5, các công năng tầng trệt, hai cửa thang máy tại cả bảy cao độ, tham chiếu mesh/material/shader đầy đủ, lọc tầng 5 chỉ hiện 12 phòng, vào phòng vẫn giữ trần, chế độ toàn cảnh khôi phục mọi tầng. Xem [validation.json](validation.json).

Đây là kiểm tra bản dựng và chế độ quan sát, chưa phải kiểm thử đi bộ có va chạm hoặc thang máy hoạt động. Kết quả đối chiếu scene cũ: [scene-preservation.json](scene-preservation.json).

- [Toàn cảnh bốn dãy](Previews/00_Overview.png)
- [Mặt bằng tầng trệt](Previews/01_Ground_plan.png)
- [Tầng 1 dãy mới / tầng 2 khối cũ](Previews/02_First_level.png)
- [Tầng 5 — toàn bộ phòng máy](Previews/03_Computer_floor.png)
- [Cầu kính giằng cam](Previews/04_Glass_bridge.png)
- [Phòng tự học khối cũ](Previews/05_Old_study_room.png)
- [Nội thất phòng máy](Previews/06_Computer_room.png)
- [Sân trong](Previews/07_Courtyard.png)

## Phòng cuối dãy sát bãi xe — 2026-11-12
Theo vị trí người dùng xác nhận, phòng cuối ở cả hai dãy bên được kéo dài thêm 4 m đến mép hành lang ngang (Z=26) trên tầng 1–6, lấp khoảng trống Z=22–26. Tầng 5 là phòng máy; các tầng còn lại là phòng học. Giữ số phòng đã chốt (3 phòng tầng 1, 5 phòng mỗi tầng trên mỗi bên), nội thất theo mẫu, hành lang ngang Z=26–28 và lối đến cầu thang/thang máy. Tầng trệt giữ các công năng cũ. Các kích thước vẫn là ước lượng. Xem [mặt bằng phòng cuối dãy](Previews/17_Rear_end_classrooms.png).
