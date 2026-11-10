# UEH Cơ sở B — bản dựng tham quan độc lập

Mở `Scenes/UEH_CampusB_Study.unity` bằng cách nhấp đúp trong Unity, sau đó nhấn **Play**. Scene tham quan có camera và điều khiển riêng, không cần GameManager hoặc save game.

Điều khiển: **WASD / phím mũi tên** để đi, **Shift** để chạy, **chuột** để nhìn, **Esc** để thả/khóa con trỏ. Phím **1** trở về ngoài cổng, **2** đến giữa sảnh, **H** ẩn/hiện hướng dẫn. Nhấp chuột để khóa lại con trỏ.

## Các prefab

**Cổng cập nhật — 11/11/2026:** cột ngoài cạnh Since 1976 chuyển tới **(18,7; 0; -1,12)** theo minh hoạ. Kính tại **Z = -1,12 m**, đi qua giữa chiều dày bốn cột cổng và nối đúng mặt đá giữa từng cặp cột. Ba nhịp gồm **18 ô kính cố định**, ô trên cao, khung đen và ba rèm phía phải. Cổng giữa mở rộng **5,6 m**; cổng Since mở rộng **4,4 m** tại X = 11,6 m. Mỗi cổng có hai cánh kính mở 90° vào trong, đi bộ qua được; cửa đang ở trạng thái mở cố định. Menu **20. Align gate glazing through columns and open Since entrance**, cờ `entranceGlassThroughColumns`, giữ cấu hình khi dựng lại. Các kích thước là ước lượng theo ảnh.

Ảnh: [toàn cảnh cổng](../../Docs/UEH-CampusB/Previews/52_Column_gates_from_lobby.png) · [lối cổng Since](../../Docs/UEH-CampusB/Previews/53_Since_gate_open_walkway.png) · [kính nối giữa thân cột](../../Docs/UEH-CampusB/Previews/54_Glazing_mid_column_joint.png). Unity kiểm tra tự động trong project riêng cùng script/packages/settings: `passed = true`; nhân vật đi qua cả hai cổng. Chi tiết và giới hạn kiểm tra ở [BuildReview](../../Docs/UEH-CampusB/BuildReview.vi.md).

**Dãy bên trái — 11/11/2026:** thêm mảng tường **xám xanh 1,45 × 4,2 m** với khung tranh nhỏ ngay bên trái Since 1976. Vị trí cột cạnh đó dùng bản cổng mới ở trên; giữa mặt sau cột và mảng xám xanh có đoạn tường trắng ngắn khoảng **0,405 m**. Mảng 1976 tại **Z = -8,6 m**, office **-20,1 m**, cây xanh/vách gỗ **-30,95 m**; khoảng cách mép nhìn thấy còn **0,62 m** và **0,44 m**. Chuyển ô mở tường/lớp ốp theo office, giữ phòng lùi sau vách X = 17,4 m. Hai volume bên trái rộng 10,6 m, sâu 6 m; tên nhóm `[12m x 6m]` giữ tương thích. Menu **19. Compact left heritage office lounge and slate wall**, cờ `compactLeftSequence`, lưu bố trí cho các lần dựng lại. Các kích thước là ước lượng theo ảnh.

Ảnh mới: [tường xám xanh và 1976](../../Docs/UEH-CampusB/Previews/47_Slate_wall_and_1976.png) · [dãy trái thu gọn](../../Docs/UEH-CampusB/Previews/48_Compact_left_sequence.png) · [office sát khu cây xanh](../../Docs/UEH-CampusB/Previews/49_Office_lounge_close_joint.png) · [cột sát mảng tường mới](../../Docs/UEH-CampusB/Previews/50_Stone_column_beside_slate.png).

**Cập nhật vị trí cột theo mũi tên:** cột đá cạnh kính đã lùi tiếp **2,9 m theo -X**, từ X = -16,7 m tới **X = -19,6 m**, giữ Z = -1,12 m. Mặt sau thân cột chạm vách cuối khu thang tại X = -20,6 m; cả chân, đầu cột và collider cùng dịch chuyển. Hai thang GV1/GV2 và năm cột còn lại giữ vị trí. Đây là kích thước bố trí ước lượng. Menu **18. Move faculty stone column to rear wall** áp dụng lại; cờ `facultyColumnAtRearWall` lưu vị trí cho những lần dựng sau. Xem [ảnh cột ở góc cuối](../../Docs/UEH-CampusB/Previews/46_Faculty_column_at_rear_wall.png). Các tọa độ -16,7 m của cột phải trong phần mô tả đợt trước bên dưới đã được thay bằng vị trí này.

**Bố trí hiện tại — sàn chính 36 × 48 m:** dãy bên phải ở **X = -13,4 m** sau khi dịch 4 m về giữa sảnh. WELCOME/quầy, hành lang toilet, ba thang A/B/C và office kính cùng tuyến vách. Giữ 6 cột, thân **2 × 2 m**: cặp trong **X = ±4,5 m, Z = -24 m**; bốn cột cổng tại **Z = -1,12 m**, X = **-19,6 / -4,5 / 4,5 / 18,7 m**. Cột cuối phía phải đã lùi sát vách cuối khu thang giảng viên. Trần chỉ giữ dải gỗ giữa. Thông số là **ước lượng từ ảnh**, chưa phải số đo thực tế.

**Thang giảng viên — 10/11/2026:** đi vào cạnh WELCOME qua ba trụ kiểm soát, **nhìn sang trái thấy hai thang GV1/GV2** trên vách vuông góc, phía sau WELCOME. Không còn cửa thang nhìn thẳng từ lối vào. Sảnh thang sâu khoảng **7,2 m**, rộng từ mép WELCOME tới mặt kính khoảng **6 m**, trần gần **4,2 m**; hai cửa rộng khoảng 1,82 m, mỗi cửa hai cánh riêng. Cửa, nút gọi và chỉ báo là mô hình tĩnh. Nhóm chỉnh sửa: `12_Faculty_lift_entrance/Faculty_lift_GV1` và `Faculty_lift_GV2`. Menu **17. Two faculty lifts behind WELCOME and recessed stone column** áp dụng riêng đợt này; cờ `facultyLiftsBehindWelcome` lưu cấu hình để các menu dựng lại cũng giữ hướng nhìn mới.

Xem [lối vào và cột đá đã lùi](../../Docs/UEH-CampusB/Previews/43_Faculty_entry_clear_glass.png), [hai thang ở bên trái](../../Docs/UEH-CampusB/Previews/44_Two_faculty_lifts_on_LEFT.png) và [góc nhìn sau WELCOME](../../Docs/UEH-CampusB/Previews/45_Faculty_behind_WELCOME_plan.png).

Đã bố trí **4 khu**, hai bên trái và hai bên phải tính từ cổng nhìn vào. Hai volume bên trái hiện **10,6 × 6 m**, tâm Z = **-20,1 m / -30,95 m**; hai volume bên phải **12 × 6 m**, tâm Z = -23 m / -37 m. Bên trái có office và cây xanh/vách gỗ; bên phải có hành lang dịch vụ, ba thang máy và office kính. Chọn nhóm `09_Four_expansion_areas` để chỉnh từng khu; các volume `Reserved_area_LEFT_01/02`, `Reserved_area_RIGHT_01/02` hiện gizmo khi chọn, cờ `furnished` phân biệt khoang đã bố trí. Hành lang/sân trong ở sau mép sảnh Z = -48 m. Phím **2** đưa tới giữa sảnh tại Z = -24 m. Các kích thước là ước lượng phục vụ bản dựng.

**Hai khu ngay sau Since 1976:** `LEFT_01 [12m x 6m]/Office_room` có phòng office rộng khoảng **10,2 × 2,8 m**, cao **4 m**, mặt tiền xám, cửa sổ 8 ô kính khung đen, viền sáng xanh dạng chữ U, cửa bên trái mở rộng **1,2 m**, cửa bên phải đóng, 2 bàn làm việc bên trong và 2 ghế chờ đen phía trước. Phần viền xanh mô phỏng cũ sau mảng 1976 đã được thay bằng office này. `LEFT_02 [12m x 6m]/Green_timber_lounge` có mảng cây xanh rộng **1,8 m** bên trái khi nhìn chính diện, vách gỗ rộng khoảng **8,3 m** ở phần còn lại và 2 ghế gỗ chân kim loại. Tường, kính, cửa, nội thất, cây và ghế nằm trong các nhóm riêng của prefab; các kích thước được ước lượng theo ảnh.

- `Prefabs/UEH_Exterior.prefab`: khối ngoài, vỏ tầng cao, lam, ban công, cổng, sân trước và cây.
- `Prefabs/UEH_LobbyB1.prefab`: sảnh có thể đi bộ, sàn, cột, cửa kính, trần gỗ, đèn, quầy tiếp đón và mảng 1976.
- `Prefabs/UEH_CampusB.prefab`: ghép hai prefab trên theo cùng hệ tọa độ.

Mỗi prefab chia thành các nhóm kiến trúc có thể chọn/chỉnh riêng. Các thanh lam, nan trần, mảng lá và chi tiết nhỏ cùng vật liệu được gom mesh trong mỗi nhóm. Vật liệu dùng chung nằm trong `Materials`, mesh được lưu thật trong `Meshes`. Ánh sáng và phản chiếu cục bộ được bố trí trong scene.

**Bố trí từ cổng nhìn vào:** khu **1976 ở vách trái**, khu **WELCOME TO UEH và quầy tiếp đón ở vách phải**, gần cửa vào. Hai nhóm quay mặt vào sảnh; trong hệ tọa độ này bên trái là +X và bên phải là -X. Chọn các nhóm `06_1976_LEFT_from_gate` và `05_WELCOME_RIGHT_from_gate` trong prefab sảnh để chỉnh vị trí từng khu.

**Đứng trong sảnh nhìn thẳng vào vách 1976**, thứ tự trái → phải là: cột đá → tường xám xanh **1,45 m** → cây xanh **SINCE 1976** rộng **3,5 m** → nền sáng có **9 ô màu** rộng **3,5 m** → nền gỗ có **5 hình tròn lớn nhỏ** rộng **3 m**. Trong nhóm `06_Heritage_1976_and_gallery`, phần mới là `00_Contrasting_slate_panel`; mảng trắng nền cũ `01_Plain_wall_left` vẫn ở phía sau. Từng phân khúc, ô màu, hình tròn chỉnh được riêng. Các kích thước là ước lượng theo ảnh.

**Đứng trong sảnh nhìn vào WELCOME**, mảng cây xanh nằm bên trái vách chữ. Khoảng phía bên phải gần cửa vào đã dùng cho **hốc thang máy giảng viên**, khác với ba thang máy A/B/C phía trong. Marker `03_Clear_entrance_pocket` giữ tên để tương thích nhưng đã đặt `furnished = true` và cập nhật kích thước/mục đích. Cột cũ cạnh quầy tại `(X = -13,5 m, Z = -12 m)` đã được bỏ cả thân, chân, đầu và collider.

Vách WELCOME rộng khoảng **6,8 m**, mảng lá bên trái rộng **1,4 m**, quầy rộng **4,5 m**. Chữ **WELCOME / TO / UEH** gồm 12 chữ mesh đặc riêng, nổi khoảng **6 cm**; 6 hàng chữ trang trí nền nổi khoảng **1,2 cm**. Mesh chữ lấy đường nét từ Liberation Sans có giấy phép sẵn trong `Fonts`, có mặt trước, mặt sau, cạnh bên và khoảng rỗng trong chữ. Dữ liệu `Fonts/UppercaseSolidGlyphs.json` dùng cho công cụ Unity; mã sinh dữ liệu nằm ở `Docs/UEH-CampusB/Tools/BuildWelcomeGlyphs.py`. Các kích thước đều là lựa chọn dựng hình ước lượng.

## Kích thước ước lượng

**Office lùi vào trong vách:** toàn phòng đã được lùi **2,8 m** so với bản dựng nhô ra trước đó; mặt tiền ở **X = 17,4 m**, cùng tuyến với các mảng vách bên trái. Phòng nằm phía sau mặt tiền, ghế chờ sát vách hơn. Đã mở ô trên tường sảnh và lớp ốp bên tầng trệt đang giao với phòng; bổ sung sàn phụ **12 × 2,45 m** nối đúng mép sàn chính tại X = 18 m, cao độ Y = 0. Sàn chính sảnh vẫn 36 × 48 m. Chọn menu **10. Recess office behind lobby side wall** để áp dụng lại vị trí này; menu 9 và Rebuild cũng dựng office theo vị trí lùi. Kích thước là ước lượng dựng hình.

1 Unity unit = 1 m; mặt trước hướng +Z; giữa ngưỡng cửa chính là `(0,0,0)`. Sàn sảnh và sân trước nối ở cao độ Y = 0.

| Phần | Giá trị mặc định | Tính chất |
|---|---:|---|
| Mặt tiền | 36 m | Ước lượng |
| Sảnh hiện tại | 36 × 48 m, cao 4,5 m | Mở rộng ước lượng theo yêu cầu bố trí |
| Bốn khu mở rộng | 2 trái, 2 phải; 12 × 6 m mỗi khu quy hoạch | Trái: office và cây/vách gỗ; phải: thang máy, hành lang dịch vụ và office kính |
| Cửa chính thông thủy | 5,6 m | Lựa chọn dựng hình/gameplay |
| Cột sảnh | 2 × 2 m; chân 2,12 m, đầu 2,24 m | Ước lượng, tăng từ thân 1,6 m |
| Hai cột giữa phía trong | X = ±4,5 m, Z = -24 m | Đối xứng qua trục X = 0, tại nửa chiều sâu sảnh 48 m |
| Cột gần cổng và hai bên sảnh | Z = -1,12 m tại X = -19,6 / -4,5 / 4,5 / 18,7 m | Kính giữa chiều dày cột; cột phải sát vách cuối khu thang |
| Khối đế | 3 tầng, tầng trệt cao 4,5 m, hai nhịp tiếp theo 3,9 m | Diễn giải kiến trúc từ ảnh |
| Đỉnh khối cao tầng | 55 m | Tham khảo thông số dự án UEH 2018; hình dáng và khoảng lùi vẫn ước lượng |
| Tầng cao phía trên khối đế | 12 tầng | Cách phân chia đề xuất để tổng thể có 15 tầng |
| Camera người chơi | Cao 1,65 m | Quy ước tham quan |

Đây là bản tái dựng dựa trên ảnh panorama, không phải mô hình đo đạc. Mặt sau, lõi thang, tầng hầm và các phòng ngoài tuyến tham quan chưa được tái dựng chi tiết. Đoạn hành lang/sân trong là bối cảnh suy luận để kết thúc tuyến đi.

## Chỉnh hoặc dựng lại

Chỉnh trực tiếp prefab để thay bố trí, vật liệu và các nhóm kiến trúc. Muốn thay kích thước tổng thể bằng công cụ sinh, chọn `Settings/EstimatedDimensions.asset`, sửa tham số rồi dùng menu **UEH → Campus B → 2. Rebuild study scene from estimated dimensions**.

Lệnh Rebuild tạo lại các asset sinh trong thư mục này và scene khảo sát; chỉnh sửa thủ công trên các phần được sinh lại có thể bị ghi đè. Nên lưu một bản sao prefab/scene trước khi dùng Rebuild sau khi tự chỉnh. Các texture thủ tục hiện có được giữ lại.

Menu **1. Open walkthrough scene** mở scene tham quan riêng. Menu **3. Capture review images and local reflections** xuất 54 ảnh camera vào `Docs/UEH-CampusB/Previews`, đồng thời cập nhật cubemap của sảnh. Menu **4. Place 1976 left and WELCOME right from gate** đặt lại hai nhóm trên vách trái/phải của prefab hiện có mà không sinh lại toàn bộ kiến trúc. Menu **5. Place front columns at glazing and outer corners** đặt hàng 4 cột trước sát kính/góc tường theo cấu hình hiện tại, cập nhật cả mesh thân/chân/đầu cột và collider. Menu **6. Refine 1976 wall and three ordered sections** dựng lại riêng mảng 1976 theo thứ tự tường trống, cây xanh, 9 ô màu, hình tròn. Menu **7. Refine WELCOME wall and reserve lift space** dựng lại riêng khu tiếp tân; khi đã bật `facultyLiftEntrance`, hốc thang máy giảng viên cũng được dựng lại cạnh WELCOME. Menu 6 và 7 thay các chỉnh sửa thủ công trong nhóm tương ứng.

Ánh sáng bản đầu dùng nắng, ambient và đèn bổ trợ cùng phản chiếu cubemap chụp bằng Unity. Cubemap có box projection, không phải phản chiếu phẳng chính xác; không có lightmap GI hoàn chỉnh. Khi đổi bố trí lớn, chụp lại phản chiếu. Không cần chuyển render pipeline.

Font dùng Liberation Sans cùng giấy phép tại `Fonts/LICENSE.txt`. Các texture đá, gạch và gỗ được tạo bằng mã, không cần kết nối web khi mở scene.

Menu **8. Expand lobby for four side areas** áp dụng sảnh 48 m với bốn khu và bố trí cột hiện tại. Trạng thái `expandedLobby` và kích thước nằm trong `Settings/EstimatedDimensions.asset`; công cụ Rebuild cũng dùng bố trí mở rộng này. Menu 5 dành cho bố trí cột trước khi mở rộng và không thay cột khi trạng thái mở rộng đang bật. Ảnh xem trước hiện có **38 góc**, cùng **4 cubemap** cho phần trước/sau sảnh.

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

**Đứng từ sảnh nhìn vào hành lang:** trái là cửa **THANG BỘ LÊN / TẦNG TRÊN**, sau đó **THANG BỘ XUỐNG / TẦNG GỬI XE**; phải là **TOILET NAM**, sau đó **TOILET NỮ**. Mặt cửa hành lang cùng tuyến vách **X = -13,4 m**, toàn bộ phòng lùi sau tuyến vách. Ô đi xuyên qua tường sảnh và lớp ốp bên tầng trệt; sàn phụ chỉ dựng phần vượt mép sàn chính **X = -18 m**, cùng cao độ **Y = 0**, tránh hai lớp sàn trùng nhau.

Chỉnh trong `09_Four_expansion_areas/RIGHT_01 [12m x 6m]/Service_corridor` và `RIGHT_02 [12m x 6m]/Three_lift_bank`. Đây là các nhóm bố trí trong hai vùng quy hoạch cũ; hình học mới có thể chạy qua ranh giữa hai vùng. Từng thang, hai cánh trượt, bốn cửa, khung, biển tên và đèn có nhóm riêng. Menu **11. Build right lifts and service corridor** dựng lại riêng dãy này; `rightLiftsAndServices` lưu trong asset kích thước để Rebuild/menu 8 giữ bố cục.

Cửa hiện là mô hình tĩnh có collider; chưa có cơ chế gọi thang, mở cửa hoặc di chuyển tầng, chưa dựng phòng toilet, buồng thang bộ và tầng gửi xe phía sau các cửa. Ảnh Unity số **21–25** xem toàn dãy, ba thang máy, hành lang, hai cửa trái và hai cửa phải. Các tỷ lệ và độ sâu hành lang được chọn để dựng hình theo ảnh, chưa đo thực địa.

## Office kính sau cụm thang máy

Phần trống phía trong cụm thang máy được dựng thành office **9,8 × 4,6 m**, cao **4 m**, tâm tại **Z = -38,5 m**. Toàn bộ phòng nằm sau mặt tiền **X = -13,4 m**, dùng phần sàn chính bên dưới và phần nối rất nhỏ vượt X = -18 m, Y = 0. Tường sảnh đã được mở ô theo vị trí mới. Office nằm ngoài chiều sâu 24 m của lớp ốp khối đế, nên không có lớp ốp ngoài nào chắn mặt kính tại đây.

**Nhìn chính diện từ sảnh vào office:** mặt trước và vách **bên phải** là kính trong khung kim loại, có dải nhận diện ngang; vách trái và vách sau là tường kín. Cửa kính rộng **1,25 m** ở phía phải mặt tiền, mở vào trong phòng. Nội thất có **3 bàn máy tính, ghế, tủ hồ sơ, cây và đèn trần**; lối đi ngoài sảnh không chứa bàn ghế hoặc khối phòng nhô ra. Kích thước đều là ước lượng để dựng hình theo bố cục người dùng yêu cầu.

Chỉnh nhóm `09_Four_expansion_areas/RIGHT_02 [12m x 6m]/Glass_office_recessed`: `01_FRONT_glass_wall`, `02_RIGHT_glass_wall`, `Glass_office_opaque_LEFT_and_BACK`, `03_Open_entrance_1_25m` cùng từng bàn/đèn là các nhóm riêng. Menu **12. Build recessed glass office near lifts** dựng lại riêng phòng này; tham số `rightGlassOffice` trong asset kích thước giữ phòng khi Rebuild/menu 8. Cửa kính đang ở tư thế mở cố định, chưa có tương tác đóng/mở. Ảnh **26–29** xem mặt tiền, góc kính phải, bên trong và quan hệ với cụm thang máy.

## Mở thông sảnh, cột giữa, lan can và đèn tròn

Menu **13. Open academic hall and refine columns lights railing** áp dụng riêng đợt chỉnh này. Trạng thái `refinedLobbyLayout` và `lobbyColumnWidth` được lưu trong `Settings/EstimatedDimensions.asset`; Rebuild/menu 8 giữ bố trí. Menu 13 giữ bề rộng đang lưu, tối thiểu 1,6 m; muốn chọn bề rộng khác thì sửa tham số và dùng Rebuild/menu 8.

- Bỏ hai mảng tường cuối sảnh tại **Z = -48 m**, hai vách của lối thông hẹp 6 m và đà đá thấp cũ. Sàn/trần đoạn nối sâu **6 m** mở rộng theo sảnh **36 m**, cùng cao độ Y = 0; sàn bối cảnh phía sau rộng tương ứng. Biển Không gian Học thuật được treo lên cao, không chắn đường đi.
- Sáu thân cột hiện **2 × 2 m**, tăng qua các đợt từ 0,9 m và 1,6 m. Cặp trong tại **Z = -24 m**, X = ±4,5 m. Bốn cột cổng cùng Z = -1,12 m, X = **-19,6 / -4,5 / 4,5 / 18,7 m**; kính đi qua giữa chiều dày cột. Cột phải chạm vách cuối khu thang; cột Since đã chuyển ra ngoài theo minh hoạ mới.
- `10_Lift_side_glass_railing`: lan can kính cao khoảng **1,1 m**, hình chữ **L**, từ góc trước ngoài cột **(-5,5; 0,6; -23)** tới góc rẽ **(-5,5; 0,6; -21,25)** rồi vào mặt trụ đá giữa thang máy C và hành lang toilet tại **(-13,32; 0,6; -21,25)**. Hai đoạn dài khoảng **1,75 m + 7,82 m**, tổng **9,57 m**, góc đúng **90°** và **9 tấm kính có collider**. Sàn giữ nguyên cao độ.
- `11_Central_round_spotlights`: **30 đèn tròn nhỏ**, hai hàng X = ±1,1 m dưới dải trần gỗ giữa, cách nhau **3 m** dọc sảnh. Thân tối, mặt phát sáng tròn đường kính khoảng **11,5 cm**; kết hợp đèn chùm đã có. Đây là chi tiết mesh phát sáng, không thêm 30 point light.

Kích thước trên là ước lượng dựng hình theo ảnh. Phần Không gian Học thuật/sân sau vẫn là bối cảnh suy luận, chưa dựng toàn bộ kiến trúc của khu đó. Ảnh Unity **30–34** xem lối mở, cặp cột, liên kết lan can, đèn tròn và đường đi bên hông qua vị trí tường cũ.

**Sửa đầu lan can theo ảnh đánh dấu:** menu **14. Connect glass railing to lift and toilet dividing wall** dựng lại riêng lan can tới đúng trụ đá giữa thang C và hành lang toilet, giữ nguyên đầu nối tại cột. Hai đoạn `Column_to_elbow` và `Elbow_to_dividing_wall` chỉnh được riêng; các mốc `Column_connection`, `Elbow_connection`, `Wall_connection` chỉ rõ đầu nối. Menu 13, Rebuild/menu 8 cũng dùng hình dáng nối tường này. Ảnh **35–36** xem toàn tuyến và mối nối vào trụ đá. Đường đi từ phía trước lan can tới thang máy vòng qua phía trong cột giữa; cửa vào hành lang toilet vẫn thông thoáng.

**Chỉnh trần, cột và chữ L theo ảnh Virtual Tour:** menu **15. Center timber only wider columns and right angle railing** chỉ dựng lại ba nhóm trần/cột/lan can. Đặt `centerTimberOnly = true`, thân cột **2 m**, xóa cả nan gỗ và nền tối của hai dải bên, giữ nguyên dải giữa, đèn chùm và 30 đèn tròn nhỏ. Lan can gồm hai đoạn song song với trục sảnh, vuông góc tại mốc `Elbow_connection`; không còn đoạn chéo. Mốc nối ở góc trước ngoài của cột rộng hơn, đầu cuối vẫn ở trụ đá đã chọn. Menu 8/Rebuild giữ bố cục theo tham số; menu 14 cũng dựng lan can chữ L. Các khu office, WELCOME, 1976, thang máy và hành lang dịch vụ được giữ. Ảnh **37–38** xem góc chữ L và trần/cột mới. Kích thước đều là ước lượng theo ảnh.
