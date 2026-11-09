# UEH Cơ sở B — bản dựng tham quan độc lập

Mở `Scenes/UEH_CampusB_Study.unity` bằng cách nhấp đúp trong Unity, sau đó nhấn **Play**. Scene tham quan có camera và điều khiển riêng, không cần GameManager hoặc save game.

Điều khiển: **WASD / phím mũi tên** để đi, **Shift** để chạy, **chuột** để nhìn, **Esc** để thả/khóa con trỏ. Phím **1** trở về ngoài cổng, **2** đến giữa sảnh, **H** ẩn/hiện hướng dẫn. Nhấp chuột để khóa lại con trỏ.

## Các prefab

**Cập nhật 09/11/2026 — sảnh mở rộng:** sàn hiện rộng **36 × 48 m = 1.728 m²**, tăng 864 m² so với bản 36 × 24 m. Theo xác nhận của người dùng, giữ **4 cột sát cổng** và **2 cột giữa phía trong tại X = ±4,5 m, Z = -12 m**, tổng cộng **6 cột**. Hàng cột cổng ở Z = -0,56 m, gồm hai cột X = ±4,5 m và hai cột góc X = ±17,25 m; thân/chân/đầu/collider đi cùng. Các cột khác phía trong đã bỏ.

Đã bố trí **4 khu, mỗi khu 12 × 6 m (72 m²)**, hai bên trái và hai bên phải tính từ cổng nhìn vào. Tâm các khu nằm tại Z = -23 m và -37 m. Hai khu bên trái đã dựng office và khu cây xanh/vách gỗ; hai khu bên phải vẫn có khung vách trống rộng 10,2 m, cao 3,2 m để bố trí tiếp. Chọn nhóm `09_Four_expansion_areas` để chỉnh từng khu; các volume `Reserved_area_LEFT_01/02`, `Reserved_area_RIGHT_01/02` hiện gizmo khi chọn, cờ `furnished` phân biệt khoang đã bố trí. Trần, tường, đèn và phản chiếu đã mở rộng đồng bộ; hành lang/sân trong chuyển về sau mép sảnh Z = -48 m. Phím **2** đưa tới giữa sảnh mới tại Z = -24 m. Các kích thước là ước lượng phục vụ bản dựng.

**Hai khu ngay sau Since 1976:** `LEFT_01 [12m x 6m]/Office_room` có phòng office rộng khoảng **10,2 × 2,8 m**, cao **4 m**, mặt tiền xám, cửa sổ 8 ô kính khung đen, viền sáng xanh dạng chữ U, cửa bên trái mở rộng **1,2 m**, cửa bên phải đóng, 2 bàn làm việc bên trong và 2 ghế chờ đen phía trước. Phần viền xanh mô phỏng cũ sau mảng 1976 đã được thay bằng office này. `LEFT_02 [12m x 6m]/Green_timber_lounge` có mảng cây xanh rộng **1,8 m** bên trái khi nhìn chính diện, vách gỗ rộng khoảng **8,3 m** ở phần còn lại và 2 ghế gỗ chân kim loại. Tường, kính, cửa, nội thất, cây và ghế nằm trong các nhóm riêng của prefab; các kích thước được ước lượng theo ảnh.

- `Prefabs/UEH_Exterior.prefab`: khối ngoài, vỏ tầng cao, lam, ban công, cổng, sân trước và cây.
- `Prefabs/UEH_LobbyB1.prefab`: sảnh có thể đi bộ, sàn, cột, cửa kính, trần gỗ, đèn, quầy tiếp đón và mảng 1976.
- `Prefabs/UEH_CampusB.prefab`: ghép hai prefab trên theo cùng hệ tọa độ.

Mỗi prefab chia thành các nhóm kiến trúc có thể chọn/chỉnh riêng. Các thanh lam, nan trần, mảng lá và chi tiết nhỏ cùng vật liệu được gom mesh trong mỗi nhóm. Vật liệu dùng chung nằm trong `Materials`, mesh được lưu thật trong `Meshes`. Ánh sáng và phản chiếu cục bộ được bố trí trong scene.

**Bố trí từ cổng nhìn vào:** khu **1976 ở vách trái**, khu **WELCOME TO UEH và quầy tiếp đón ở vách phải**, gần cửa vào. Hai nhóm quay mặt vào sảnh; trong hệ tọa độ này bên trái là +X và bên phải là -X. Chọn các nhóm `06_1976_LEFT_from_gate` và `05_WELCOME_RIGHT_from_gate` trong prefab sảnh để chỉnh vị trí từng khu.

**Đứng trong sảnh nhìn thẳng vào vách 1976**, thứ tự trái → phải là: khoảng tường trống rộng khoảng **1,9 m** → phân khúc cây xanh **SINCE 1976** rộng **3,5 m** → phân khúc nền sáng có **9 ô màu** rộng **3,5 m** → phân khúc nền gỗ có **5 hình tròn lớn nhỏ** rộng **3 m**. Các kích thước này được ước lượng để bố trí theo ảnh. Trong nhóm `06_Heritage_1976_and_gallery`, các phân khúc được đặt tên theo thứ tự `01_Plain_wall_left`, `02_SINCE_1976_green`, `03_Nine_colour_panel`, `04_Circles_panel`; từng ô màu và hình tròn có nhóm riêng để chỉnh sửa.

**Đứng trong sảnh nhìn vào WELCOME**, mảng cây xanh nằm bên trái vách chữ. Khoảng trống **3,5 × 2,9 m** phía gần cửa vào tiếp tục giữ thoáng, đổi tên thành `03_Clear_entrance_pocket`; cụm thang máy được bố trí ở hai khoang mở rộng phía trong theo yêu cầu mới. Cột phía trong cạnh quầy tại `(X = -13,5 m, Z = -12 m)` đã được bỏ cả thân, chân, đầu và collider.

Vách WELCOME rộng khoảng **6,8 m**, mảng lá bên trái rộng **1,4 m**, quầy rộng **4,5 m**. Chữ **WELCOME / TO / UEH** gồm 12 chữ mesh đặc riêng, nổi khoảng **6 cm**; 6 hàng chữ trang trí nền nổi khoảng **1,2 cm**. Mesh chữ lấy đường nét từ Liberation Sans có giấy phép sẵn trong `Fonts`, có mặt trước, mặt sau, cạnh bên và khoảng rỗng trong chữ. Dữ liệu `Fonts/UppercaseSolidGlyphs.json` dùng cho công cụ Unity; mã sinh dữ liệu nằm ở `Docs/UEH-CampusB/Tools/BuildWelcomeGlyphs.py`. Các kích thước đều là lựa chọn dựng hình ước lượng.

## Kích thước ước lượng

**Office lùi vào trong vách:** toàn phòng đã được lùi **2,8 m** so với bản dựng nhô ra trước đó; mặt tiền ở **X = 17,4 m**, cùng tuyến với các mảng vách bên trái. Phòng nằm phía sau mặt tiền, ghế chờ sát vách hơn. Đã mở ô trên tường sảnh và lớp ốp bên tầng trệt đang giao với phòng; bổ sung sàn phụ **12 × 2,45 m** nối đúng mép sàn chính tại X = 18 m, cao độ Y = 0. Sàn chính sảnh vẫn 36 × 48 m. Chọn menu **10. Recess office behind lobby side wall** để áp dụng lại vị trí này; menu 9 và Rebuild cũng dựng office theo vị trí lùi. Kích thước là ước lượng dựng hình.

1 Unity unit = 1 m; mặt trước hướng +Z; giữa ngưỡng cửa chính là `(0,0,0)`. Sàn sảnh và sân trước nối ở cao độ Y = 0.

| Phần | Giá trị mặc định | Tính chất |
|---|---:|---|
| Mặt tiền | 36 m | Ước lượng |
| Sảnh hiện tại | 36 × 48 m, cao 4,5 m | Mở rộng ước lượng theo yêu cầu bố trí |
| Bốn khu mở rộng | 2 trái, 2 phải; 12 × 6 m mỗi khu quy hoạch | Trái: office và cây/vách gỗ; phải: cụm thang máy và hành lang dịch vụ |
| Cửa chính thông thủy | 5,6 m | Lựa chọn dựng hình/gameplay |
| Cột sảnh | 0,9 × 0,9 m | Ước lượng |
| Hai cột giữa phía trong | X = ±4,5 m, Z = -12 m | Chỉ giữ cặp cột giữa trong phần sảnh bên trong |
| Bốn cột sát cổng | Z = -0,56 m; X = ±4,5 m và ±17,25 m | Giữ bố trí sát kính và sát hai góc đã duyệt; tổng sảnh có 6 cột |
| Khối đế | 3 tầng, tầng trệt cao 4,5 m, hai nhịp tiếp theo 3,9 m | Diễn giải kiến trúc từ ảnh |
| Đỉnh khối cao tầng | 55 m | Tham khảo thông số dự án UEH 2018; hình dáng và khoảng lùi vẫn ước lượng |
| Tầng cao phía trên khối đế | 12 tầng | Cách phân chia đề xuất để tổng thể có 15 tầng |
| Camera người chơi | Cao 1,65 m | Quy ước tham quan |

Đây là bản tái dựng dựa trên ảnh panorama, không phải mô hình đo đạc. Mặt sau, lõi thang, tầng hầm và các phòng ngoài tuyến tham quan chưa được tái dựng chi tiết. Đoạn hành lang/sân trong là bối cảnh suy luận để kết thúc tuyến đi.

## Chỉnh hoặc dựng lại

Chỉnh trực tiếp prefab để thay bố trí, vật liệu và các nhóm kiến trúc. Muốn thay kích thước tổng thể bằng công cụ sinh, chọn `Settings/EstimatedDimensions.asset`, sửa tham số rồi dùng menu **UEH → Campus B → 2. Rebuild study scene from estimated dimensions**.

Lệnh Rebuild tạo lại các asset sinh trong thư mục này và scene khảo sát; chỉnh sửa thủ công trên các phần được sinh lại có thể bị ghi đè. Nên lưu một bản sao prefab/scene trước khi dùng Rebuild sau khi tự chỉnh. Các texture thủ tục hiện có được giữ lại.

Menu **1. Open walkthrough scene** mở scene tham quan riêng. Menu **3. Capture review images and local reflections** xuất 25 ảnh camera vào `Docs/UEH-CampusB/Previews`, đồng thời cập nhật cubemap của sảnh. Menu **4. Place 1976 left and WELCOME right from gate** đặt lại hai nhóm trên vách trái/phải của prefab hiện có mà không sinh lại toàn bộ kiến trúc. Menu **5. Place front columns at glazing and outer corners** đặt hàng 4 cột trước sát kính, hai cột ngoài cùng sát hai góc tường bên, cập nhật cả mesh thân/chân/đầu cột và collider; chạy lại lệnh vẫn giữ cùng vị trí. Menu **6. Refine 1976 wall and three ordered sections** dựng lại riêng mảng 1976 theo thứ tự tường trống, cây xanh, 9 ô màu, hình tròn. Menu **7. Refine WELCOME wall and reserve lift space** dựng lại riêng khu tiếp tân; khi đã bật `rightLiftsAndServices`, vùng trống gần cửa vào được ghi là khoảng thoáng, cụm thang máy ở phía trong. Menu 6 và 7 thay các chỉnh sửa thủ công trong nhóm tương ứng.

Ánh sáng bản đầu dùng nắng, ambient và đèn bổ trợ cùng phản chiếu cubemap chụp bằng Unity. Cubemap có box projection, không phải phản chiếu phẳng chính xác; không có lightmap GI hoàn chỉnh. Khi đổi bố trí lớn, chụp lại phản chiếu. Không cần chuyển render pipeline.

Font dùng Liberation Sans cùng giấy phép tại `Fonts/LICENSE.txt`. Các texture đá, gạch và gỗ được tạo bằng mã, không cần kết nối web khi mở scene.

Menu **8. Expand lobby for four side areas** áp dụng sảnh 48 m với bốn khu và bố trí cột hiện tại. Trạng thái `expandedLobby` và kích thước nằm trong `Settings/EstimatedDimensions.asset`; công cụ Rebuild cũng dùng bố trí mở rộng này. Menu 5 dành cho bố trí cột trước khi mở rộng và không thay cột khi trạng thái mở rộng đang bật. Ảnh xem trước hiện có **25 góc**, cùng **4 cubemap** cho phần trước/sau sảnh.

Kết quả kiểm tra và ảnh xem trước nằm trong `Docs/UEH-CampusB`. Bản dựng này dành để duyệt độc lập trước bước bố trí vào GameWorld.

Menu **9. Furnish left office and green timber lounge** dựng lại riêng hai khoang trái theo bố cục trên và thay các chỉnh sửa thủ công trong hai nhóm đó. Trạng thái `leftOfficeAndLounge` được lưu trong asset kích thước để Rebuild/menu 8 cũng giữ bố cục đã dựng.

## Thang máy và hành lang dịch vụ

Hai khoang phải được bố trí thành một dãy liên tục: **WELCOME → hành lang dịch vụ → cụm 3 thang máy** theo chiều đi sâu vào sảnh. Tâm hành lang tại **Z = -19,5 m**; tâm cụm thang máy tại **Z = -27 m**. Cụm thang máy nằm tiếp giáp miệng hành lang, thay vì đặt theo tâm hai khung dự phòng cũ cách nhau 14 m. Viền sàn dự phòng được thay bằng viền khu chờ trước thang máy.

| Hạng mục | Kích thước ước lượng |
|---|---:|
| Vách cụm thang máy | Rộng 12 m, cao 4,2 m |
| Ba bộ cửa A/B/C | Mỗi bộ hai cánh inox; tổng rộng khoảng 1,73 m, cao 2,51 m |
| Hành lang sau vách | Thông rộng 2,8 m, sâu 8,6 m, cao 3,4 m |
| Bốn cửa dịch vụ | Cánh rộng 1,02 m, cao 2,34 m; cách miệng hành lang 2,4 m và 5,8 m |

**Đứng từ sảnh nhìn vào hành lang:** trái là cửa **THANG BỘ LÊN / TẦNG TRÊN**, sau đó **THANG BỘ XUỐNG / TẦNG GỬI XE**; phải là **TOILET NAM**, sau đó **TOILET NỮ**. Mặt cửa hành lang cùng tuyến vách **X = -17,4 m**, không dựng khối phòng lấn vào sảnh. Ô đi xuyên qua cả tường sảnh và lớp ốp bên tầng trệt; sàn phụ nối mép sàn chính tại **X = -18 m**, cùng cao độ **Y = 0**.

Chỉnh trong `09_Four_expansion_areas/RIGHT_01 [12m x 6m]/Service_corridor` và `RIGHT_02 [12m x 6m]/Three_lift_bank`. Đây là các nhóm bố trí trong hai vùng quy hoạch cũ; hình học mới có thể chạy qua ranh giữa hai vùng. Từng thang, hai cánh trượt, bốn cửa, khung, biển tên và đèn có nhóm riêng. Menu **11. Build right lifts and service corridor** dựng lại riêng dãy này; `rightLiftsAndServices` lưu trong asset kích thước để Rebuild/menu 8 giữ bố cục.

Cửa hiện là mô hình tĩnh có collider; chưa có cơ chế gọi thang, mở cửa hoặc di chuyển tầng, chưa dựng phòng toilet, buồng thang bộ và tầng gửi xe phía sau các cửa. Ảnh Unity số **21–25** xem toàn dãy, ba thang máy, hành lang, hai cửa trái và hai cửa phải. Các tỷ lệ và độ sâu hành lang được chọn để dựng hình theo ảnh, chưa đo thực địa.
