# Bản dựng UEH Cơ sở B để duyệt

Ngày dựng đầu: **05/11/2026**, cập nhật mới nhất **11/11/2026**. Bản tham quan độc lập dùng **Unity 6000.6.8f1**. Kích thước và một phần kiến trúc được ước lượng theo hồ sơ khảo sát; đây chưa phải mô hình đo đạc.

**Cập nhật bố trí theo ảnh người dùng:** nhìn từ cổng vào, **1976 ở vách bên trái**, **WELCOME TO UEH ở vách bên phải**, gần cửa vào và hướng mặt về phía lối đi. Hai mảng đã được chuyển từ vách cuối sảnh sang hai vách bên. Prefab sảnh, scene, phản chiếu và ảnh xem trước đã được cập nhật.

**Bố trí hiện tại — 11/11/2026:** sàn chính **36 × 48 m**, dãy bên phải ở X = -13,4 m; dãy bên trái đã thu gọn quanh Z = -8,6 m / -20,1 m / -30,95 m. Giữ 6 cột, thân **2 × 2 m**; cặp trong X = ±4,5 m, Z = -24 m. Bốn cột cổng cùng Z = -1,12 m, tại X = **-19,6 / -4,5 / 4,5 / 18,7 m**. Kính nối giữa thân cột, có hai lối mở tại cổng giữa và phía Since 1976. Cột cuối phía phải vẫn sát vách cuối khu thang giảng viên. Trần giữ dải gỗ giữa, đèn chùm và 30 đèn tròn; lan can kính vẫn chữ L. Các số đo là **ước lượng dựng hình từ ảnh**, chưa phải kích thước đo tại UEH.

## Kính ở giữa thân cột và hai cổng đi lại được — 11/11/2026

- **Cột gần Since 1976** chuyển từ **(16,7; 0; -1,525)** tới **(18,7; 0; -1,12)**: đưa ra ngoài mép kính như vị trí khoanh đỏ, dịch +2 m theo X và +0,405 m theo Z. Cả thân, chân, đầu cột và collider cùng dịch chuyển. Cột phải ở X = -19,6 m và cặp cột giữa sảnh giữ vị trí.
- **Kính nằm ở giữa chiều dày cột:** toàn tuyến kính tại **Z = -1,12 m**, trùng tâm bốn cột phía cổng. Ba nhịp nối đúng mặt đá ở hai đầu: nhịp phải **X = -18,6…-5,5 m**, nhịp giữa **-3,5…3,5 m**, nhịp Since **5,5…17,7 m**. Không còn lớp kính cố định chạy ở phía trước mặt cột hoặc phủ ngang lối cửa bên.
- **Hai lối đi:** cổng giữa rộng **5,6 m**, tâm X = 0; cổng gần Since rộng **4,4 m**, tâm **X = 11,6 m**. Mỗi cổng có hai cánh kính mở 90° vào trong, ô kính phía trên và dầm ngang nối từ cột sang cột. Phần kính cố định giữ collider; phần trống giữa hai cánh đi qua được. Đây là trạng thái cửa mở cố định, chưa có thao tác đóng/mở cửa tương tác.
- Dời chậu cây phía Since tới khoảng **(16,1; 0; -2,8)** để không đè lên cánh cửa/lối đi. Phần vách đóng cạnh khu thang giảng viên thu vào sau cột cuối để kính nối vào đúng thân cột. Mặt bằng, vị trí hai thang và dãy office bên trong giữ bố trí.
- Các kích thước trên là **ước lượng dựng hình theo ảnh**, chưa phải số đo thực địa. Menu **20. Align gate glazing through columns and open Since entrance** áp dụng riêng đợt này; cờ `entranceGlassThroughColumns = true` giữ cấu hình khi Rebuild hoặc dựng lại cột/kính.

| Ảnh Unity mới | Nội dung |
|---|---|
| [51 — Cổng nhìn từ ngoài](Previews/51_Column_gates_exterior.png) | Cột Since ra ngoài mép kính theo minh hoạ |
| [52 — Ba nhịp kính từ trong sảnh](Previews/52_Column_gates_from_lobby.png) | Kính nối cột và hai lối cửa mở |
| [53 — Lối qua cổng Since](Previews/53_Since_gate_open_walkway.png) | Cánh kính mở và phần đi lại giữa cửa |
| [54 — Mối nối kính giữa thân cột](Previews/54_Glazing_mid_column_joint.png) | Kính chạm mặt bên cột tại giữa chiều dày |

Các ghi chú trước đó về kính Z = 0, cột Since X = 16,7 m và mép cột chạm tường xám xanh mô tả bước dựng cũ; tọa độ mới ở mục này được ưu tiên.

**Kiểm tra bản cổng mới:** Unity **6000.6.8f1** trong project kiểm tra riêng, cùng **53 script C# gốc**, packages và Player Settings của workspace. Đã lưu/mở lại scene, render **54 ảnh** và bốn cubemap. [validation.json](validation.json) có `passed = true`, `failures = []`; CharacterController hoàn tất **404,9 m**, đi qua cổng giữa và qua lại cổng Since. Các phép đo xác nhận kính nằm ở giữa chiều dày cột, đầu nhịp chạm mặt đá, lối cửa và sàn tiếp cận thông thoáng; kính cố định vẫn chặn va chạm. Chưa thử bàn phím/chuột thủ công trong Editor người dùng.

Scene có **455 Mesh Renderer**, **43 vật liệu đang dùng**, **223 collider gồm CharacterController**, **249.510 tam giác mesh**; **571 mesh asset** trên đĩa gồm phiên bản lịch sử. Không có lỗi biên dịch C# trong phiên kiểm tra riêng; lỗi chỉ mục `UnityEditor.Search.SearchDatabase` và cảnh báo API cũ ghi riêng trong [UnityVerification.log](UnityVerification.log). Asset giao về khớp byte với bản Unity đã kiểm tra; [báo cáo giao asset](column-gates-delivery.json) xác nhận không đổi GameWorld, gameplay, packages, Player Settings hoặc build settings so với snapshot đầu lượt.

## Dãy bên trái thu gọn và tường xám xanh cạnh 1976 — 11/11/2026

- Thêm phân khúc **xám xanh** rộng khoảng **1,45 m**, cao **4,2 m**, ngay bên trái cây xanh Since 1976 khi nhìn chính diện, có nẹp mảnh và khung tranh nhỏ như ảnh tham khảo. Nhóm riêng `00_Contrasting_slate_panel` trong khu 1976.
- Mảng 1976 chuyển từ Z = -7,5 m tới **-8,6 m**; office từ -23 m tới **-20,1 m**; khu cây xanh/vách gỗ từ -37 m tới **-30,95 m**. Chuyển cả nhóm, giữ kích thước nội dung và thứ tự Since 1976 → 9 màu → hình tròn → office → cây xanh/vách gỗ. Các khoảng trắng giữa các khu thu hẹp còn khoảng **0,5–0,6 m**.
- Cột đá cạnh 1976 giữ X = 16,7 m, chuyển Z từ -1,12 m tới **-1,525 m**. Mặt thân cột tại Z = -2,525 m chạm đúng mép phân khúc xám xanh. Thân, chân, đầu cột và collider cùng vị trí; cột phải ở cuối khu thang và cặp cột giữa giữ vị trí.
- Ô mở trên tường và lớp ốp tầng trệt chuyển theo office, phạm vi **Z = -14,8 tới -25,4 m**. Sàn riêng, cửa, nội thất và ghế chuyển cùng phòng; mặt tiền vẫn X = 17,4 m, phòng lùi sau vách. Hai volume bố trí bên trái thu còn **10,6 × 6 m** để không chồng nhau; tên nhóm cũ `[12m x 6m]` giữ tương thích. Hai khu bên phải không dịch chuyển.
- Các số đo là **ước lượng dựng hình theo ảnh**, không phải kích thước khảo sát thực địa. Menu **19. Compact left heritage office lounge and slate wall** áp dụng lại; `compactLeftSequence = true` lưu bố trí cho Rebuild và các menu liên quan.

| Ảnh Unity mới | Nội dung |
|---|---|
| [47 — Tường xám xanh và 1976](Previews/47_Slate_wall_and_1976.png) | Phân khúc khác màu, khung tranh và thứ tự các mảng |
| [48 — Dãy trái thu gọn](Previews/48_Compact_left_sequence.png) | Khoảng cách giữa 1976, office và khu cây xanh/vách gỗ |
| [49 — Mối nối office và cây xanh](Previews/49_Office_lounge_close_joint.png) | Hai khu sát nhau hơn |
| [50 — Cột sát tường xám xanh](Previews/50_Stone_column_beside_slate.png) | Mặt cột chạm mép phân khúc mới |

Các mục 10/11 và tọa độ Z = -23/-37 m bên dưới là lịch sử trước khi thu gọn dãy trái.

**Kiểm tra bản thu gọn:** Unity **6000.6.8f1**, project kiểm tra riêng có cùng **53 script C# gốc**, packages và Player Settings của workspace. Chạy lại menu 19 để xác nhận vị trí không dịch cộng dồn, lưu/mở lại scene, render **50 ảnh** và bốn cubemap. [validation.json](validation.json) có **passed = true**, `failures = []`; đo bounds mặt tiền được khoảng hở **0,62 m** và **0,44 m**, mặt cột chạm mép tường xám xanh. CharacterController hoàn tất **370,2 m**, vào office tại cửa mới, đi qua các khu bên trái và tuyến thang/toilet bên phải. Chưa thử bàn phím/chuột thủ công trong Editor người dùng.

Scene hiện có **443 Mesh Renderer**, **43 vật liệu đang dùng**, **231 collider gồm CharacterController**, **250.242 tam giác mesh**; **530 mesh asset** trên đĩa gồm phiên bản lịch sử. Không có lỗi biên dịch C# trong phiên kiểm tra riêng. Lỗi chỉ mục `UnityEditor.Search.SearchDatabase` và cảnh báo API cũ ghi riêng trong [UnityVerification.log](UnityVerification.log). Asset giao về khớp byte với bản Unity đã kiểm tra; so với snapshot đầu lượt, GameWorld, gameplay, packages, Player Settings và build settings không thay đổi.

## Hai thang máy giảng viên phía sau WELCOME — 10/11/2026

### Cột đá lùi sát vách cuối — bản hiện tại theo mũi tên

Cột cạnh dải kính đã lùi thêm **2,9 m**, từ **(-16,7; 0; -1,12)** tới **(-19,6; 0; -1,12)**. Mặt sau thân cột 2 × 2 m nằm đúng mặt vách cuối khu thang tại **X = -20,6 m**; chân và đầu cột ăn vào vách nhẹ để không hở mép. Thân, chân, đầu cột và collider cập nhật đồng bộ. Năm cột còn lại, hai thang GV1/GV2, kính, quầy và lan can giữ vị trí. Thông số là **ước lượng dựng hình**, không phải số đo thực địa.

Xem [46 — Cột đá tại góc vách cuối](Previews/46_Faculty_column_at_rear_wall.png). Menu **18. Move faculty stone column to rear wall** chỉ dựng lại nhóm cột và lưu cờ `facultyColumnAtRearWall = true`. Các menu dựng cột/Rebuild đọc cùng vị trí. Các mô tả bên dưới về cột phải ở X = -16,7 m là bước trước của lần tinh chỉnh này.

**Kiểm tra bản lùi cột cuối:** lưu/mở lại scene trong Unity 6000.6.8f1 ở project kiểm tra riêng; **46 ảnh**, bốn cubemap; [validation.json](validation.json) có `passed = true`, `failures = []`. Kiểm tra mặt sau thân cột chạm đúng vách cuối X = -20,6 m, đồng bộ mesh thân/chân/đầu và collider; CharacterController vẫn hoàn tất tuyến **367,3 m** tới hai thang và các khu trước đó. Chỉ ba mesh cột và prefab sảnh thay đổi về hình học; báo cáo phạm vi xác nhận không đổi GameWorld, gameplay hoặc thiết lập dự án. Cảnh báo API cũ và lỗi chỉ mục Unity Search vẫn ghi riêng trong log. Số renderer/collider/tam giác không đổi so với đợt hai thang.

- Thay cửa thang nhìn thẳng từ lối vào bằng **hai thang GV1/GV2** trên vách bên trái khi đi vào. Hai cửa quay **90°** so với hướng đi vào khu thang, nằm phía sau vách WELCOME; từ sảnh chính không còn thấy cửa thang đối diện lối vào.
- Qua ba trụ kiểm soát, đi tiếp và nhìn trái để tiếp cận từng thang. Tâm hai cửa lần lượt tại **X = -15,45 m / -18,75 m**, **Z = -5,82 m**; hướng mặt về **+Z**. Mỗi cửa rộng khoảng **1,82 m**, cao **2,62 m**, có hai cánh, bảng gọi và chỉ báo riêng. Cửa hiện là mô hình tĩnh.
- Khu thang sâu khoảng **7,2 m**, từ sau WELCOME tới mặt kính rộng khoảng **6 m**, trần khoảng **4,2 m**. Nâng trần khu thang gần trần sảnh, bỏ vách đá che dải kính ở cạnh lối vào; sàn riêng chỉ nối phần vượt X = -18 m. Mở đúng đoạn ốp tầng trệt đang giao với đường vào thang thứ hai. Loại bỏ dải viền sàn cũ chạy xuyên dưới khu thang.
- **Cột đá cạnh dải kính** lùi **4 m theo -X**, từ **(-12,7; 0; -1,12)** tới **(-16,7; 0; -1,12)**. Cột nằm sâu hơn mặt WELCOME 3,3 m theo trục X; ba cột cổng còn lại và cặp cột giữa không dịch chuyển. Kính cổng kéo dài tới cột, hiện **24 nhịp đứng**, giữ ô kính trên cao, rèm và lối chính rộng 5,6 m.
- Các kích thước trên là **ước lượng bố trí từ ảnh** và hướng nhìn người dùng yêu cầu; ảnh không cho số đo hoặc chi tiết bên trong hai buồng thang.

| Ảnh Unity mới | Nội dung |
|---|---|
| [43 — Lối vào và dải kính thoáng](Previews/43_Faculty_entry_clear_glass.png) | WELCOME ở trước; cột đá lùi sâu và không còn cửa thang đối diện |
| [44 — Nhìn trái thấy hai thang](Previews/44_Two_faculty_lifts_on_LEFT.png) | Hai cửa GV1/GV2 ở vách sau WELCOME |
| [45 — Tiếp cận thang phía trong](Previews/45_Faculty_behind_WELCOME_plan.png) | Lối đi, cửa thứ hai và sàn nối |

Chọn `12_Faculty_lift_entrance/Faculty_lift_GV1` hoặc `Faculty_lift_GV2` trong prefab sảnh để sửa riêng. Menu **17. Two faculty lifts behind WELCOME and recessed stone column** áp dụng riêng đợt này. Cờ `facultyLiftsBehindWelcome = true` lưu trong `EstimatedDimensions.asset`; Rebuild và các menu dựng liên quan đọc cùng bố trí. Số liệu kiểm tra ở các mục 09/11 bên dưới là lịch sử của bản một thang nhìn thẳng.

**Kiểm tra mới nhất:** Unity **6000.6.8f1** ở project kiểm tra riêng, cùng **53 script C# gốc**, packages và Player Settings của workspace. Chạy lại menu 17 trên bản đã chỉnh, lưu/mở lại scene, render **45 ảnh** và bốn cubemap. [validation.json](validation.json) có **passed = true**, `failures = []`; CharacterController hoàn tất khoảng **367,3 m**, gồm đi qua hàng trụ tới cả hai cửa thang và quay lại. Kiểm tra xác nhận hai thang đúng vách trái, bốn cánh cửa riêng, không nhô qua mặt WELCOME, sàn nối liên tục và cả hai cửa đóng đều chặn nhân vật. Kính cổng, office, lan can và tuyến cũ vẫn qua kiểm tra. Chưa thao tác bàn phím/chuột thủ công trong Editor của người dùng.

Scene hiện có **437 Mesh Renderer**, **42 vật liệu đang dùng**, **231 collider gồm CharacterController**, **250.134 tam giác mesh**; **521 mesh asset** trên đĩa gồm phiên bản lịch sử. Không có lỗi biên dịch C# trong phiên kiểm tra riêng. Lỗi chỉ mục `UnityEditor.Search.SearchDatabase` xuất hiện sau khi mở Play Mode và cảnh báo API cũ vẫn được ghi riêng trong [UnityVerification.log](UnityVerification.log); không coi đây là một phiên Editor hoàn toàn sạch lỗi. So sánh snapshot đầu lượt xác nhận không thay đổi GameWorld, gameplay, packages, Player Settings hoặc build settings. Các file asset giao về khớp byte với project Unity đã kiểm tra.

## Dãy bên phải, kính cổng và thang máy giảng viên — lịch sử 09/11/2026

- **Đưa đồng bộ dãy bên phải vào trong 4 m:** tuyến mặt tiền X = -17,4 m thành **-13,4 m**. WELCOME, quầy, ghế/cây phía phải, ba thang máy A/B/C, hành lang thang bộ/toilet và office kính cùng dịch chuyển. Vách mở ô đúng vị trí mới. Sàn phụ các phòng chỉ nối phần vượt mép sàn chính, tránh mặt sàn trùng nhau.
- **Kính cổng theo ảnh:** thay các ô ngang rộng bằng **21 nhịp đứng** rộng tối đa khoảng 1,35 m, khung kim loại đen, hàng ô kính trên cao và ba rèm kéo ở phía phải. Giữ lối cửa chính rộng **5,6 m**. Cột góc phải cũng đưa sát tuyến vách mới; đoạn đầu khối phòng được đóng kín bằng đá trong phạm vi công trình.
- **Hốc thang máy giảng viên cạnh WELCOME:** bố trí phía bên phải khi nhìn chính diện vách chữ, tâm Z = -4 m. Kích thước ước lượng **3,8 m rộng × 3 m sâu**, trần khoảng 3,6 m; lùi sau tuyến mặt tiền, không nhô vào sảnh. Một cửa thang inox hai cánh, biển tên, chỉ báo tầng, nút gọi và **ba trụ kiểm soát**; lối giữa trụ rộng khoảng 1,02 m, đi bộ vào được. Cửa thang máy là mô hình tĩnh; chưa mô phỏng nâng hạ hoặc kiểm soát thẻ.
- **Lan can chữ L nối lại:** đầu cột **(-5,5; 0,6; -23)** → góc vuông **(-5,5; 0,6; -21,25)** → mặt trụ đá **(-13,32; 0,6; -21,25)**. Hai đoạn **1,75 + 7,82 m**, tổng **9,57 m**, 9 tấm kính có collider. Cặp cột giữa giữ vị trí.

| Ảnh Unity mới | Nội dung |
|---|---|
| [39 — WELCOME, thang giảng viên và kính](Previews/39_Right_entrance_and_faculty_lift.png) | Tương quan quầy/vách với khu cửa vào |
| [40 — Hốc thang máy giảng viên](Previews/40_Faculty_lift_alcove.png) | Cửa inox, biển tên và ba trụ kiểm soát |
| [41 — Dãy bên phải sau khi dịch](Previews/41_Right_wing_alignment.png) | Cột, lan can, ba thang và hành lang toilet |
| [42 — Kính cổng từ trong sảnh](Previews/42_Entrance_glazing_inside.png) | Nhịp khung đứng, ô trên cao và rèm |

**Kiểm tra Unity:** dùng **6000.6.8f1** trong project kiểm tra riêng, cùng **53 script C# gốc**, packages và Player Settings của workspace. Đã lưu/mở lại scene, render **42 ảnh** và bốn cubemap. [validation.json](validation.json) có **passed = true**, `failures = []`. CharacterController đi khoảng **359,4 m**, qua hàng trụ vào hốc thang giảng viên, vào hai office, hành lang toilet và tới ba thang máy; các phép thử va chạm xác nhận cửa thang giảng viên, kính cổng phải, kính office và lan can chặn nhân vật. Chưa kiểm tra thao tác bàn phím/chuột thủ công trong cửa sổ Editor của người dùng.

Scene có **419 Mesh Renderer**, **42 vật liệu đang dùng**, **219 collider gồm CharacterController**, **249.378 tam giác mesh**; **483 mesh asset** trên đĩa gồm các phiên bản lịch sử. Không có lỗi biên dịch C#. Cảnh báo API cũ và lỗi chỉ mục `UnityEditor.Search.SearchDatabase` vẫn được giữ trong [UnityVerification.log](UnityVerification.log), không được coi là lỗi mô hình. So sánh với snapshot đầu lượt xác nhận GameWorld, gameplay, packages, Player Settings và build settings không đổi.

Nhóm sửa được trong prefab: `05_WELCOME_RIGHT_from_gate`, `09_Four_expansion_areas/RIGHT_01`, `RIGHT_02`, `12_Faculty_lift_entrance`, `03_Glazing_with_open_doorway`, `10_Lift_side_glass_railing`. Thông số `rightWingInset = 4` và `facultyLiftEntrance = true` lưu ở asset kích thước. Menu **16. Align right wing entrance glazing and faculty lift** áp dụng lại riêng đợt này; Rebuild đọc các thông số này. Các mục bên dưới ghi lại các đợt dựng trước; tọa độ dãy bên phải và số liệu kiểm tra mới nhất ở mục này được ưu tiên.

## Mở bản dựng

**Sửa office nhô ra hành lang:** lùi toàn bộ `Office_room` **2,8 m** để mặt tiền ở **X = 17,4 m**, cùng tuyến vách sảnh. Khối phòng nằm sau tuyến vách; ghế chờ được đưa sát vách theo phòng. Mở ô tường sảnh và đoạn ốp bên tầng trệt đang chắn phòng, bổ sung sàn phụ **12 × 2,45 m** phía trong, nối sàn chính tại X = 18 m và Y = 0. Sàn chính sảnh giữ kích thước 36 × 48 m. Cửa office và lối hành lang được kiểm tra bằng CharacterController; ảnh 19 chụp xiên theo tuyến vách, ảnh 20 chụp từ bên trong office. Các kích thước là ước lượng.

**Cập nhật hai khoang cạnh Since 1976:** khoang `LEFT_01` tại Z = -23 m đã dựng office với mặt tiền xám, cửa sổ 8 ô kính khung đen, viền sáng xanh, cửa trái mở rộng 1,2 m, hai bàn làm việc bên trong và hai ghế chờ đen phía trước. Kích thước phòng khoảng **10,2 × 2,8 m, cao 4 m**, nằm trong khoang 12 × 6 m. Khoang `LEFT_02` tại Z = -37 m có cây xanh **1,8 m** ở bên trái khi nhìn chính diện, vách gỗ khoảng **8,3 m** ở phần còn lại và hai ghế gỗ chân kim loại theo ảnh. Các thông số là ước lượng dựng hình. Phần viền xanh mô phỏng cũ sau 1976 đã được thay bằng office; mỗi khu có nhóm chỉnh sửa riêng trong `09_Four_expansion_areas`. Hai khoang phải hiện đã được bố trí thang máy/hành lang theo mục cập nhật cuối tài liệu.

**Cập nhật 08/11/2026 — hàng 4 cột sát cổng:** dịch hàng cột phía trước 1,64 m về phía cửa, từ tâm Z = -2,20 m đến Z = -0,56 m. Thân cột còn cách mặt kính khoảng 0,11 m; đầu cột chạm mặt kính Z = 0. Chân, đầu cột và collider được dịch cùng thân. Khoảng cách theo chiều ngang, kích thước cột và hai hàng cột phía trong giữ nguyên. Đây là vị trí ước lượng theo ảnh người dùng cung cấp. Đã render lại 7 góc nhìn, cập nhật phản chiếu và kiểm tra đường đi trong Unity.

**Điều chỉnh tiếp theo — hai cột ngoài cùng:** dịch riêng hai cột ngoài cùng ngang thêm 3,75 m mỗi bên, từ X = ±13,50 m đến X = ±17,25 m. Mặt ngoài thân cột sát mặt trong tường bên tại X = ±17,70 m; phần đầu và chân cột giao vào tường. Giữ Z = -0,56 m, hai cột giữa tại X = ±4,50 m và các hàng cột phía trong. Mesh thân/chân/đầu và collider của hai cột được cập nhật đồng bộ. Thông số này là lựa chọn dựng hình theo ảnh, chưa đo thực địa.

Trong Unity, nhấp đúp [UEH_CampusB_Study.unity](../../Assets/UEHCampusB/Scenes/UEH_CampusB_Study.unity), chờ nhập asset/biên dịch xong rồi nhấn **Play**. Mở scene này riêng để tránh chạy cùng các camera, collider và GameCore của scene khác.

**WASD** để đi, **Shift** để chạy, **chuột** để nhìn, **Esc** để thả con trỏ. **1** về ngoài cổng; **2** tới giữa sảnh; **H** ẩn/hiện hướng dẫn. Đi từ giữa cổng theo lối thẳng qua khoảng mở cửa kính để vào sảnh.

[Hướng dẫn chỉnh sửa và kích thước](../../Assets/UEHCampusB/README.vi.md) · [Hồ sơ khảo sát](Survey.vi.md)

## Ảnh chụp từ camera Unity

**Điều chỉnh khu WELCOME theo ảnh tham chiếu:** bỏ cột phía trong cạnh quầy tại X = -13,50 m, Z = -12 m (thân/chân/đầu/collider); thêm mảng cây xanh bên trái và giữ vùng trống bên phải khi nhìn chính diện vách WELCOME. Vùng trống dự kiến cho thang máy rộng **3,5 m**, sâu **2,9 m**, có nhóm `03_Future_lift_space_RIGHT` với gizmo để chọn/chỉnh; chưa dựng thang máy. Vách chữ rộng khoảng **6,8 m**, mảng cây xanh **1,4 m**, quầy **4,5 m**. Chữ WELCOME / TO / UEH gồm **12 mesh chữ đặc**, nổi **6 cm**; thêm **6 hàng chữ cái trang trí** nổi **1,2 cm**, chừa chỗ quanh chữ chính. Đường nét lấy từ Liberation Sans có giấy phép trong project, có mặt bên và khoảng rỗng của chữ. Mọi kích thước là ước lượng để bố trí bản dựng. Góc 09 xem toàn bộ khu này và vùng trống bên phải; góc 10 xem độ nổi của chữ.

**Điều chỉnh mảng 1976:** khi nhìn chính diện từ trong sảnh, thứ tự trái → phải là **tường trống → SINCE 1976 trên nền cây xanh → phân khúc 9 ô màu → phân khúc 5 hình tròn lớn nhỏ**. Khoảng tường trống rộng khoảng 1,9 m; ba phân khúc chính rộng lần lượt 3,5 m, 3,5 m và 3 m (ước lượng). Dải ô màu đặt thấp trên nền sáng như ảnh tham chiếu; các hình tròn đặt trên nền gỗ, có đường kính khoảng 0,55–1,05 m. Mỗi phân khúc, ô màu và hình tròn có nhóm chỉnh sửa riêng trong prefab. Góc 08 bên dưới chụp chính diện để duyệt thứ tự này.

Ảnh được render trực tiếp từ các mesh, vật liệu và nguồn sáng của scene; không phải ảnh minh họa sinh riêng.

| Góc | Ảnh |
|---|---|
| Tổng thể khối ngoài | [01 — Ngoại thất](Previews/01_Exterior.png) |
| Ngoài cổng nhìn vào sảnh | [02 — Lối vào](Previews/02_Entrance.png) |
| Từ cổng nhìn vào hai vách trái/phải | [03 — Sảnh B1](Previews/03_Lobby.png) |
| Quầy tiếp đón | [04 — Reception](Previews/04_Reception.png) |
| Trong sảnh nhìn ngược ra cửa | [05 — Hướng ra ngoài](Previews/05_Lobby_toward_entrance.png) |
| Khu 1976 trên vách trái | [06 — Khu 1976](Previews/06_Heritage.png) |
| Hàng 4 cột sát vách kính cổng được giữ lại | [07 — Hướng ra cổng](Previews/07_Entrance_columns_from_inside.png) |
| Chính diện tường trống và ba phân khúc mảng 1976 | [08 — Mảng 1976 theo thứ tự mới](Previews/08_Heritage_front.png) |
| Chính diện WELCOME, cây bên trái và vùng trống bên phải | [09 — Khu tiếp tân mới](Previews/09_Welcome_front.png) |
| Cận cảnh chữ WELCOME TO UEH có độ dày | [10 — Chữ nổi và chữ nền](Previews/10_Welcome_raised_letters.png) |
| Nhìn vào phần sảnh bên trong, chỉ còn cặp cột giữa | [11 — Toàn sảnh mở rộng](Previews/11_Expanded_lobby_forward.png) |
| Từ cuối sảnh mới nhìn ra cổng | [12 — Sảnh mở rộng nhìn ngược](Previews/12_Expanded_lobby_reverse.png) |
| Office và khu cây xanh/vách gỗ bên trái | [13 — Hai khu bên trái](Previews/13_Expanded_left_areas.png) |
| Dãy thang máy/hành lang bên phải | [14 — Hai khu bên phải](Previews/14_Expanded_right_areas.png) |
| Office chính diện, viền xanh và ghế chờ | [15 — Office](Previews/15_Office_front.png) |
| Cửa sổ nhìn vào nội thất office | [16 — Cận office](Previews/16_Office_window_detail.png) |
| Khu cây xanh bên trái, vách gỗ và hai ghế | [17 — Cây xanh và vách gỗ](Previews/17_Green_timber_lounge.png) |
| Liên hệ Since 1976, office và khu cây xanh/vách gỗ | [18 — Dãy bên trái](Previews/18_Left_side_sequence.png) |
| Office cùng tuyến vách, nhìn xiên dọc hành lang | [19 — Office lùi vào trong](Previews/19_Office_flush_wall.png) |
| Không gian phòng phía sau vách sảnh | [20 — Bên trong office](Previews/20_Office_inside.png) |

![Sảnh B1 trong bản dựng](Previews/03_Lobby.png)

## Asset đã tạo

- [UEH_Exterior.prefab](../../Assets/UEHCampusB/Prefabs/UEH_Exterior.prefab): vỏ công trình, nhịp tầng, cửa kính, lam, ban công, bảng chữ, cổng thu, bồn cây, sân và phần đường tham chiếu.
- [UEH_LobbyB1.prefab](../../Assets/UEHCampusB/Prefabs/UEH_LobbyB1.prefab): sảnh rỗng có thể đi bộ, cột đá, cửa thông, trần nan gỗ, đèn, quầy, vách WELCOME và mảng 1976.
- [UEH_CampusB.prefab](../../Assets/UEHCampusB/Prefabs/UEH_CampusB.prefab): lắp hai prefab trên trong cùng hệ tọa độ.
- Hiện có **435 mesh asset** lưu trong `Assets/UEHCampusB/Meshes`: 431 mesh trước lần nối lan can vào tường và 4 mesh mới cho hai đoạn lan can. Gồm cả mesh các phiên bản cũ được giữ trên đĩa; không phải tất cả cùng dùng trong scene. Texture thủ tục, font có giấy phép, vật liệu và bốn cubemap lưu cùng thư mục riêng.
- Công cụ Editor có menu `UEH → Campus B`, cùng asset tham số `Settings/EstimatedDimensions.asset` để dựng lại.
- Điều khiển tham quan độc lập với save game, lịch học và GameManager hiện có.

Các nhóm kiến trúc được chỉnh riêng trong prefab; những chi tiết nhỏ lặp lại cùng vật liệu đã được gom mesh theo nhóm. Muốn thay từng thanh đã gom thì sửa công cụ sinh hoặc mesh; không phải mọi chi tiết đều là GameObject riêng.

## Kích thước bản đầu

| Mục | Giá trị | Căn cứ |
|---|---:|---|
| Mặt tiền | 36 m | Ước lượng |
| Sảnh | 36 × 24 m | Ước lượng |
| Trần sảnh | 4,5 m | Ước lượng |
| Cửa chính | Rộng 5,6 m, thông cao khoảng 3,2 m | Bố trí phục vụ tham quan |
| Cột | 0,9 × 0,9 m | Ước lượng |
| Khối cao tầng | Đỉnh khoảng 55 m | Mốc dự án UEH 2018; hình dáng/khoảng lùi và chia tầng được ước lượng |
| Phân tầng | Khối đế 3 tầng và 12 tầng vỏ phía trên | Diễn giải để đối chiếu tổng thể 15 tầng |
| Cao độ sàn và sân | Y = 0 | Quy ước dựng giúp đi liên tục |

Mặt chính hướng +Z, gốc ở giữa ngưỡng vào. Mái, mặt sau, lõi thang và tầng hầm chưa có dữ liệu để dựng chính xác. Hành lang và sân sau là bối cảnh suy luận; chưa dựng toàn bộ English Zone hoặc phòng Khánh tiết.

## Các kiểm tra đã thực hiện

Kết quả máy đọc được: [validation.json](validation.json). Trích log Unity: [UnityVerification.log](UnityVerification.log).

Kiểm tra chạy trong project tạm độc lập để không chuyển scene hoặc thay trạng thái Play Mode của Editor người dùng. Project kiểm tra dùng đúng Unity 6000.6.8f1, bản Player Settings, package manifest/lock và toàn bộ **53 script C# sẵn có** từ workspace, cộng các script mới. Asset đã sinh được đưa về workspace với cả `.meta` để giữ nguyên GUID.

| Kiểm tra | Kết quả |
|---|---|
| Biên dịch với các script/package hiện có | Không có lỗi biên dịch C# trong lượt cuối |
| Lưu, đóng và mở lại scene | Các tham chiếu mesh, vật liệu, font và prefab giữ được |
| Ba prefab và các script trên scene | Có đủ prefab; không phát hiện missing script |
| Shader và vật liệu đang dùng | Không phát hiện vật liệu thiếu hoặc shader không hỗ trợ |
| Camera và AudioListener | Mỗi loại đúng 1 đối tượng trong scene |
| Font biển tên | Có ký tự tiếng Việt dùng trong tên trường |
| Trái/phải từ hướng cổng | 1976 ở vách trái (+X), WELCOME ở vách phải (-X); cả hai hướng mặt vào sảnh |
| Phản chiếu | Bốn cubemap cho phần trước/sau sảnh được chụp và lưu; tham chiếu giữ được sau mở lại |
| Cột sau chỉnh sửa | 6 thân cột rộng 2 m; cặp trong tại Z = -24 m, hàng cổng căn sát kính/góc theo bề rộng mới; mesh/collider khớp |
| Bốn khu mở rộng | Mỗi vùng quy hoạch 12 × 6 m có sàn liên tục; trái là office/cây-vách gỗ, phải là thang máy/hành lang dịch vụ/office kính |
| Office và khu cây xanh/vách gỗ | Mặt tiền office cùng tuyến vách, phòng lùi 2,8 m, cửa đi được và sàn phụ liên tục; cây xanh ở bên trái vách gỗ, có hai ghế |
| Mảng 1976 nhìn từ sảnh | Tường trống bên trái, sau đó cây xanh SINCE 1976, đúng 9 ô màu, rồi 5 hình tròn với kích thước khác nhau |
| Khu WELCOME | Cột cạnh quầy đã bỏ; cây xanh bên trái, khoảng thoáng gần cửa vào 3,5 × 2,9 m không chứa collider; 12 chữ chính có mesh dày 6 cm, 6 hàng chữ nền có độ nổi |
| Đường đi trong Play Mode hiện tại | Đi hết khoảng **342,6 m** bằng CharacterController |
| Tuyến đi | Sân → cửa → vào/ra office → hành lang dịch vụ và bốn cửa → vòng qua cột giữa tới ba thang máy → vào/ra office kính → cuối sảnh → qua lối mở rộng tại X = ±12 m, Z = -52 m → ra ngoài |
| Liên tục cao độ | Không rơi khỏi sàn hoặc bị đẩy lên quá ngưỡng kiểm tra |
| Tường và kính cố định | Chặn nhân vật; cửa chính vẫn đi xuyên qua khoảng mở được |
| Xem hình | Đã xuất và xem ảnh ngoại thất, cửa, sảnh và quầy; sửa chữ xuyên tường, ánh sáng tối và dữ liệu GPU cũ khi dựng lại |

Bài kiểm tra đường đi dùng bộ điều khiển va chạm thật trong Play Mode, gọi bước di chuyển 1/60 giây ở tốc độ 4,8 m/s. Chưa có phiên kiểm thử thủ công bằng bàn phím/chuột trong Editor đang mở của người dùng, chưa benchmark FPS trên thiết bị đích, và chưa thử tương tác gameplay trong GameWorld.

Trong batch Editor, Unity phát sinh một `ArgumentOutOfRangeException` từ `UnityEditor.Search.SearchDatabase` ở bước khởi tạo chỉ mục tìm kiếm. Stack trace nằm trong hệ thống Search của Editor; không có frame script UEH ở lỗi này. Quá trình dựng, render và kiểm tra Play Mode vẫn hoàn tất. Lỗi này được giữ trong trích log; không được tính là lỗi của tuyến đi hoặc coi là log hoàn toàn sạch.

## Chi phí hình học và ánh sáng

Sau khi chỉnh cột, lan can, đèn tròn và mở thông sảnh, scene được Unity kiểm đếm có **372 Mesh Renderer**, **42 vật liệu khác nhau đang dùng**, **159 collider tính cả CharacterController**, khoảng **247.242 tam giác mesh kiến trúc/cây/đồ vật/chữ nổi**. Chữ nổi có độ dày được tính trong số tam giác mesh; con số này không gồm các quad chữ TextMesh còn dùng ở biển tên và khu 1976. Mảng lá xanh dùng các lá dạng mesh đơn giản thay cho các khối cầu dẹt.

Đây là số lượng đối tượng và hình học, không phải draw call hoặc FPS đo được. Nắng và các đèn bổ trợ dùng chiếu sáng thời gian thực, kết hợp ambient và cubemap cục bộ. Bản này không có lightmap GI hoàn chỉnh; phản chiếu sàn là xấp xỉ bằng probe, không phải phản chiếu phẳng chính xác.

## Phạm vi bàn giao để duyệt

Hai prefab và scene tham quan đã sẵn sàng xem riêng. `GameWorld.unity`, các script gameplay, package manifest và build settings không được sửa để tích hợp bản dựng này. Khi đã duyệt tỷ lệ/diện mạo, bước tiếp theo mới là bố trí footprint vào bản đồ và căn lại các điểm tương tác.

## Cập nhật 09/11/2026 — mở thông sảnh, cột giữa, lan can và đèn tròn

Đã bỏ **hai mảng tường cuối sảnh tại Z = -48 m**, hai vách của hành lang hẹp 6 m và đà đá thấp cũ. Đoạn nối sâu 6 m có sàn/trần rộng theo sảnh **36 m**, cùng Y = 0; sàn bối cảnh phía sau và viền biên mở rộng tương ứng. Biển Không gian Học thuật chuyển lên gần trần. Kiến trúc khu Học thuật phía sau vẫn là bối cảnh suy luận, chưa dựng toàn bộ theo hiện trạng.

Sáu thân cột tăng từ **0,9 × 0,9 m lên 1,6 × 1,6 m**, chân **1,72 m**, đầu **1,84 m**. Hai cột trong chuyển từ Z = -12 tới **Z = -24 m**, đúng nửa chiều sâu sảnh 48 m và đối xứng X = ±4,5 m. Bốn cột cổng được căn lại theo kích thước mới tại Z = -0,92 m, hai cột ngoài X = ±16,9 m, giữ đầu cột chạm kính và thân cột ngoài sát mặt trong tường. Mesh và collider cập nhật cùng nhau.

Ở đợt chỉnh sảnh đầu tiên, thêm lan can kính cao khoảng **1,1 m**, đoạn chính **5 m** nối sát mặt ngoài cột phía thang máy tại X = -5,3 m, Z = -24 m; đầu còn lại gập **2 m**. Có tay vịn, trụ kim loại và sáu tấm kính có collider, giữ lối tiếp cận cụm thang máy bên ngoài lan can. Nhánh gập ngắn đã được thay bằng đoạn nối tường ở lần sửa tiếp theo. Bản dựng không tạo hố hoặc đổi cao độ sàn trong vùng lan can. Dải trần gỗ giữa có thêm **30 đèn tròn nhỏ** thân tối, mặt sáng đường kính khoảng **11,5 cm**, hai hàng X = ±1,1 m, nhịp 3 m, kết hợp đèn chùm hiện có. Các số đo là **ước lượng dựng hình theo ảnh**, chưa đo thực địa.

| Ảnh Unity | Nội dung |
|---|---|
| [30 — Mở thông Không gian Học thuật](Previews/30_Open_academic_connection.png) | Bỏ vách ngăn, sàn nối rộng và biển treo cao |
| [31 — Cặp cột giữa sảnh](Previews/31_Centered_wider_columns.png) | Thân rộng hơn, cặp cột ở giữa chiều sâu sảnh |
| [32 — Cột và lan can kính](Previews/32_Column_glass_railing.png) | Liên kết sát cột phía thang máy |
| [33 — Đèn tròn trên trần giữa](Previews/33_Central_round_lights.png) | Hai hàng đèn tròn mới xen với đèn chùm |
| [34 — Đường đi bên hông qua tường cũ](Previews/34_Academic_side_walkway.png) | Kiểm tra khoảng mở bên ngoài lối hẹp cũ |

Menu **13. Open academic hall and refine columns lights railing** áp dụng riêng chỉnh sửa; Rebuild/menu 8 giữ bố trí theo `refinedLobbyLayout` và `lobbyColumnWidth`. Các nhóm `02_Stone_columns`, `07_Inner_passage_inferred`, `10_Lift_side_glass_railing`, `11_Central_round_spotlights` chỉnh được trong prefab. Hai office, WELCOME, 1976, cụm thang máy và hành lang dịch vụ không được dựng lại trong đợt áp dụng riêng này.

**Kiểm tra đợt chỉnh sảnh trước khi nối lan can vào tường:** Unity **6000.6.8f1**, cùng 53 script C# hiện có, packages và Player Settings. Đã lưu/mở lại scene, render **34 ảnh**, cập nhật bốn cubemap. Báo cáo [validation.json](validation.json) có **passed = true**, không có mục thất bại: xác nhận cột rộng và đúng tâm, vị trí cổng sát kính/góc, lan can nối cột, 30 đèn và lối mở/sàn tại 10 điểm. CharacterController hoàn tất khoảng **314,2 m**, gồm đi qua hai bên vị trí tường cũ tại X = ±12 m, Z = -52 m; lan can chặn nhân vật. Chưa có phiên kiểm thử bàn phím/chuột thủ công trong Editor người dùng.

Trước khi nối lan can tới tường, scene có **370 Mesh Renderer**, **42 vật liệu đang dùng**, **153 collider gồm CharacterController**, **247.650 tam giác mesh**; trên đĩa có **431 mesh asset** gồm các phiên bản cũ. Cảnh báo API cũ và lỗi chỉ mục `UnityEditor.Search` vẫn được ghi trong log; không có lỗi biên dịch C#. GameWorld, các script gameplay, packages, Player Settings và build settings giữ nguyên qua đợt chỉnh sửa này.

## Chỉnh trần, tăng cột và lan can chữ L — bản hiện tại

- **Xóa hai dải gỗ bên trần:** bỏ cả nan gỗ lẫn nền tối ở hai phía. Giữ dải giữa rộng khoảng 3,8 m, các đèn chùm và 30 đèn tròn nhỏ. Trần trắng, đèn âm trần và các mảng gỗ trong office/vách trang trí giữ nguyên.
- **Cột rộng hơn:** sáu thân cột tăng từ 1,6 m lên **2 × 2 m**; chân 2,12 m, đầu 2,24 m. Giữ vị trí giữa sảnh ở X = ±4,5 m, Z = -24 m. Hàng cổng căn lại Z = -1,12 m và hai cột ngoài X = ±16,7 m, để đầu cột chạm kính và thân cột ngoài sát tường.
- **Lan can đúng chữ L:** nối góc trước ngoài của cột **(-5,5; 0,6; -23)** tới góc rẽ **(-5,5; 0,6; -21,25)**, sau đó tới trụ đá **(-17,32; 0,6; -21,25)** giữa thang máy C và hành lang toilet. Hai đoạn thẳng dài khoảng **1,75 m + 11,82 m**, tổng **13,57 m**, vuông góc **90°**, gồm 12 tấm kính có collider. Không còn đoạn chéo. Các kích thước là **ước lượng dựng hình**.

| Ảnh Unity mới | Nội dung |
|---|---|
| [37 — Lan can chữ L](Previews/37_Right_angle_glass_railing.png) | Góc vuông tại cột và đoạn dài nối đúng trụ tường |
| [38 — Trần giữa và cột rộng hơn](Previews/38_Central_timber_and_wider_columns.png) | Chỉ còn dải gỗ giữa; thân cột 2 m |

Menu **15. Center timber only wider columns and right angle railing** chỉ dựng lại ba nhóm cột/trần/lan can. `centerTimberOnly` và `lobbyColumnWidth` lưu trong asset kích thước; Rebuild/menu 8 giữ bố trí, menu 14 cũng dựng lan can vuông góc. Các khu khác trong prefab sảnh được giữ.

**Kiểm tra hiện tại:** Unity **6000.6.8f1**, cùng 53 script C# hiện có và packages/Player Settings của workspace. Đã lưu/mở lại scene, render **38 ảnh** và cập nhật bốn cubemap. [validation.json](validation.json) có **passed = true**, không có mục thất bại: không còn nan gỗ/nền tối ngoài dải giữa, giữ đèn chùm/30 đèn tròn; cột rộng đúng 2 m, sát kính/góc; góc lan can đúng 90°, đầu nối vào mesh trụ đá và 12 tấm kính có va chạm. CharacterController hoàn tất **342,6 m**, gồm đường vòng tới ba thang máy và vào hành lang toilet; kính lan can chặn nhân vật. Chưa có phiên kiểm tra bàn phím/chuột thủ công trong Editor người dùng.

Scene hiện có **372 Mesh Renderer**, **42 vật liệu đang dùng**, **159 collider gồm CharacterController**, **247.242 tam giác mesh**, **435 mesh asset** trên đĩa. Không thêm mesh asset mới trong lần này; cập nhật các mesh đã sinh và giữ mesh lịch sử. Không có lỗi biên dịch C#. GameWorld, script gameplay, packages, Player Settings và build settings không thay đổi qua đợt này.

## Lịch sử lần nối lan can vào tường trước khi đổi chữ L

Đầu lan can đã được nối tới **mặt trụ đá giữa thang máy C và cửa vào hành lang toilet**, đúng vị trí ảnh đánh dấu. Giữ đầu nối tại cột giữa và đoạn đầu dài **5 m**; bỏ nhánh gập tự do 2 m, thay bằng đoạn chéo khoảng **7,54 m** tới trụ đá. Tổng chiều dài khoảng **12,54 m**, cao khoảng **1,1 m**, gồm **11 tấm kính có collider**. Các kích thước là ước lượng dựng hình.

- Mốc cột: **(-5,3; 0,6; -24)**.
- Mốc góc nối: **(-10,3; 0,6; -24)**.
- Mốc mặt tường: **(-17,32; 0,6; -21,25)**; đối chiếu trực tiếp với mesh của trụ đá.

| Ảnh Unity | Nội dung |
|---|---|
| [35 — Lan can nối vào tường](Previews/35_Railing_connected_to_dividing_wall.png) | Toàn tuyến từ cột đến vách ngăn thang máy/hành lang toilet |
| [36 — Mối nối sát trụ đá](Previews/36_Railing_wall_joint.png) | Đầu tay vịn và trụ cuối chạm mặt đá, cửa hành lang vẫn thông |

Menu **14. Connect glass railing to lift and toilet dividing wall** chỉ dựng lại nhóm lan can trong prefab sảnh; các khu khác được giữ. Menu 13 và Rebuild/menu 8 cũng dùng hình dáng nối tường mới. Cập nhật đường đi kiểm tra để tiếp cận thang máy bằng lối vòng quanh cột, không đi xuyên qua lan can mới.

**Kiểm tra bản nối tường trước lần đổi sang chữ L:** Unity **6000.6.8f1** đã lưu/mở lại scene và xuất **36 ảnh**, cập nhật bốn cubemap. [validation.json](validation.json) có **passed = true**, không có mục thất bại: mốc cuối lan can khớp mặt trước mesh trụ đá, đầu nối tại cột được giữ, có đủ 11 tấm kính va chạm, lối hành lang toilet thông thoáng. CharacterController hoàn tất khoảng **342,6 m**, gồm đường vòng quanh cột tới ba thang máy; đoạn kính mới chặn nhân vật. Kiểm tra trong project tạm cùng 53 script C# hiện có, packages/Player Settings của workspace; chưa có phiên bàn phím/chuột thủ công trong Editor người dùng.

Trước lần đổi sang chữ L và tăng cột lên 2 m, scene có **372 Mesh Renderer**, **42 vật liệu đang dùng**, **158 collider gồm CharacterController**, **247.770 tam giác mesh**; trên đĩa có **435 mesh asset** gồm các phiên bản cũ. Không có lỗi biên dịch C#; cảnh báo API cũ/lỗi chỉ mục `UnityEditor.Search` của Editor vẫn được ghi trong log. GameWorld, các script gameplay, packages, Player Settings và build settings không thay đổi qua lần sửa lan can này.

## Cập nhật 09/11/2026 — thang máy và hành lang cạnh WELCOME

Hai khoang phải đã được bố trí thành dãy **WELCOME → hành lang dịch vụ → ba thang máy** đi sâu vào sảnh. Tâm hành lang ở **Z = -19,5 m**, tâm cụm thang máy ở **Z = -27 m**; cụm thang máy rộng **12 m**, cao **4,2 m**, tiếp giáp miệng hành lang. Ba cửa **A/B/C** có hai cánh inox, khe giữa, khung kim loại, ngưỡng, nút gọi và chỉ báo tầng. Vật liệu inox riêng được làm sáng hơn sau lần xem ảnh đầu; không đổi vật liệu Silver dùng chung của các khu cũ.

Hành lang **rộng 2,8 m × sâu 8,6 m × cao 3,4 m**, nằm sau tuyến vách **X = -17,4 m**. Đứng từ sảnh nhìn vào: **trái là thang bộ lên, thang bộ xuống tầng gửi xe; phải là toilet nam, toilet nữ**. Hai cặp cửa cách miệng hành lang lần lượt **2,4 m** và **5,8 m**; mỗi cánh khoảng **1,02 × 2,34 m**, có biển tiếng Việt và hình nhận diện. Đã mở cả tường sảnh lẫn lớp ốp tầng trệt tại lối vào, thêm sàn phụ nối mép X = -18 m và ba đèn trần trong hành lang. Tất cả kích thước là **ước lượng dựng hình**, không phải số đo hiện trạng.

Vùng dành thang máy cũ gần cửa vào được đổi thành `03_Clear_entrance_pocket`, tiếp tục để thoáng; thang máy thực tế của bản dựng nằm ở dãy mới. Hai khung vách trống và viền sàn dự phòng phải đã được thay bằng kiến trúc và khu chờ mới. Office cùng các khu 1976/WELCOME và bố trí 6 cột được giữ qua đợt chỉnh sửa này.

| Ảnh Unity mới | Nội dung |
|---|---|
| [21 — Toàn dãy cạnh WELCOME](Previews/21_Right_services_sequence.png) | Quan hệ WELCOME, hành lang và thang máy |
| [22 — Ba thang máy](Previews/22_Three_elevators.png) | Cửa inox, vách đá, bảng gọi và ký hiệu A/B/C |
| [23 — Nhìn vào hành lang](Previews/23_Service_corridor.png) | Cả hai cặp cửa và lối đi phía sau vách |
| [24 — Hai cửa bên trái](Previews/24_Stairs_doors_LEFT.png) | Thang bộ lên và xuống tầng gửi xe |
| [25 — Hai cửa bên phải](Previews/25_Toilets_doors_RIGHT.png) | Toilet nam và nữ |

Các cửa mới là **mô hình tĩnh có collider**. Chưa mô phỏng gọi thang, mở cửa hoặc di chuyển tầng; phòng toilet, buồng thang bộ và tầng gửi xe phía sau cửa chưa được dựng. Hành lang và khu chờ đi bộ được; từng bộ cửa, cánh, biển tên và đèn chỉnh sửa riêng trong prefab. Menu 11 dựng lại riêng dãy mới, Rebuild/menu 8 giữ bố cục theo `rightLiftsAndServices`.

Kết quả trước đợt office kính: **311 Mesh Renderer**, **40 vật liệu đang dùng**, **123 collider tính cả CharacterController**, **235.646 tam giác mesh**, **25 ảnh** và **4 cubemap**. Có **369 mesh asset** lưu trên đĩa (gồm mesh các phiên bản trước được giữ lại); 90 asset được thêm trong đợt dựng dãy phải. Các số lượng ở những mục lịch sử phía trên phản ánh bản trước khi thêm thang máy.

Đã biên dịch trong Unity **6000.6.8f1** với **53 script C# hiện có** và cùng packages/Player Settings, lưu/mở lại scene, kiểm tra ba bộ thang, bốn cửa đúng phía, collider cửa đóng và sàn/lối vào hành lang tại năm độ sâu. CharacterController hoàn tất tuyến khoảng **238,1 m**, gồm vào/ra office, đi tới cả bốn cửa hành lang và ba thang máy; báo cáo không có mục thất bại. Cảnh báo API cũ `FindFirstObjectByType` và lỗi chỉ mục `UnityEditor.Search` được giữ trong trích log; không có lỗi biên dịch C#. Chưa kiểm thử thủ công trong Editor đang mở của người dùng.

Chỉ các asset UEH và tài liệu khảo sát được cập nhật; chưa ghép vào `GameWorld`.

## Cập nhật 09/11/2026 — office kính ở phần trống sau thang máy

Đã bố trí office **9,8 × 4,6 m**, cao **4 m**, tâm **Z = -38,5 m**, sau cụm thang máy kết thúc tại Z = -33 m. Mặt tiền cùng tuyến vách sảnh **X = -17,4 m**, khối phòng lùi về phía -X; sàn phụ nối mép sàn chính tại X = -18 m và cùng cao độ Y = 0. Tường sảnh cũ được mở ô để không chắn kính hoặc cửa đi. Vị trí office ở sau chiều sâu 24 m của lớp ốp khối đế, không giao với lớp ốp đó.

**Khi đứng trong sảnh nhìn chính diện office:** mặt trước và vách **bên phải** là kính trong khung kim loại; vách trái và vách sau là tường kín. Có dải nhận diện trên kính, cửa rộng **1,25 m** mở vào trong, ba bàn làm việc có máy tính/ghế, tủ hồ sơ, cây và đèn trần. Tất cả là kích thước **ước lượng dựng hình**; hình ảnh Virtual Tour chỉ được dùng làm tham chiếu bối cảnh.

| Ảnh camera Unity | Nội dung |
|---|---|
| [26 — Mặt tiền office kính](Previews/26_Glass_office_front.png) | Kính trước và nội thất nhìn từ sảnh |
| [27 — Góc phải office](Previews/27_Glass_office_right_corner.png) | Góc kính trước/phải, cửa mở vào trong |
| [28 — Bên trong office](Previews/28_Glass_office_inside.png) | Vách kính phải, nội thất và hướng ra sảnh |
| [29 — Thang máy và office kính](Previews/29_Lifts_and_glass_office.png) | Office lùi sau tuyến vách cạnh cụm thang máy |

Nhóm `RIGHT_02 [12m x 6m]/Glass_office_recessed` nằm trong prefab sảnh, chỉnh được riêng mặt kính trước/phải, tường kín, cửa, bàn và đèn. Menu 12 dựng lại riêng office; Rebuild/menu 8 giữ phòng theo `rightGlassOffice`. Cửa hiện mở cố định, chưa có tương tác đóng/mở. Office cũ bên trái, cụm thang máy, hành lang dịch vụ và bố trí cột được giữ trong đợt này.

Phía ngoài vách kính phải có thêm phần bối cảnh kín sau mặt tiền thang máy, tách khỏi ranh office, để không nhìn qua kính vào khoảng trời trống của mô hình. Chi tiết này là bối cảnh suy luận; không phải tường kính được thay thành tường kín.

**Kiểm tra đợt office kính, trước khi chỉnh cột/mở thông sảnh:** Unity **6000.6.8f1** đã dựng, lưu/mở lại scene và render **29 ảnh**, cập nhật **4 cubemap**. Báo cáo [validation.json](validation.json) có **passed = true**, không có mục thất bại; gồm kiểm tra đúng hai phía kính, hai vách kín, cửa đi không bị tường cũ chắn, sàn nối liên tục và hình học phòng không lấn vào sảnh. CharacterController hoàn tất tuyến **256,4 m**, vào/ra cả hai office, hành lang dịch vụ và các khu đã dựng; kính trước và kính phải đều chặn nhân vật. Kiểm tra trong project tạm cùng **53 script C# hiện có**, packages và Player Settings của workspace; chưa có phiên dùng bàn phím/chuột thủ công trong Editor đang mở của người dùng.

Ở phiên bản office kính trước đợt chỉnh sảnh, scene có **366 Mesh Renderer**, **41 vật liệu đang dùng**, **149 collider tính cả CharacterController**, **242.694 tam giác mesh**. Trên đĩa có **422 mesh asset**, tăng 53 so với bản thang máy. Log vẫn có cảnh báo API cũ và lỗi chỉ mục `UnityEditor.Search` như đã ghi ở trên; không có lỗi biên dịch C#. Asset được đưa về workspace cùng `.meta`; GameWorld, các script gameplay, packages, Player Settings và build settings giữ nguyên qua đợt này.
