# UEH Cơ sở B — bản dựng tham quan độc lập

**Chỉnh sân ngoài và cảnh quan — 13/11/2026:** menu **40. Refine forecourt paving and reference planting** dựng riêng sân gạch vuông lớn và vỉa hè hoa văn vòng cong, bồn đá hạt, cây phân nhánh có lá, bụi thấp, lá lưỡi hổ viền vàng và cụm cau cảnh. `UEHB1Landscape.cs` sinh vật liệu/texture và hình học cây, dùng chung trong Rebuild. Cây và kích thước được mô phỏng thị giác theo ảnh Virtual Tour. [Tổng thể](../../Docs/UEH-CampusB/Previews/133_B1_reference_forecourt_overview.png), [hai loại gạch](../../Docs/UEH-CampusB/Previews/134_B1_two_distinct_paving_types.png), [dải cây bên đường](../../Docs/UEH-CampusB/Previews/135_B1_sidewalk_planted_street_strip.png).

**Đặt khối tầng 5 trở lên sát phía trái — 13/11/2026:** menu **39. Align upper tower toward viewer left** dịch hai nhóm vỏ/mặt đứng tháp khoảng 5,75 m theo X dương (bên trái khi nhìn từ mặt tiền), cho mép trái còn cách mép tầng 3–4 khoảng 1 m. Bệ chân và hồi kính được cập nhật cùng trục; chiều rộng 32,5 m và chiều sâu 46 m giữ theo mặt bằng đã chỉnh. Mức lệch là ước lượng từ ảnh Virtual Tour. Rebuild dùng cùng mốc căn trái. [Nhìn thẳng](../../Docs/UEH-CampusB/Previews/129_B1_left_aligned_tower_front.png), [góc sân](../../Docs/UEH-CampusB/Previews/130_B1_left_aligned_tower_virtual_tour_angle.png).

**Khép các phần vách và tiếp giáp còn thiếu — 13/11/2026:** menu **38. Close missing floor infills and slab junctions** bổ sung riêng nhóm `19_Floor_enclosures_and_junctions`: vách có cửa kính hai bên và phía sau tầng 3–4, dải nối đầu vách/trần, bệ chân khối tháp và hồi kính góc tháp. Hành lang ban công khoảng 2,6 m bao quanh khối phòng. Rebuild và các lệnh chỉnh mặt bằng tầng cũng cập nhật nhóm bổ sung theo cùng kích thước. [Góc bên](../../Docs/UEH-CampusB/Previews/125_B1_closed_floor_side.png), [hành lang ban công](../../Docs/UEH-CampusB/Previews/127_B1_enclosed_room_beside_balcony.png).

**Điều chỉnh khối tầng 5 trở lên — 13/11/2026:** khối tháp rộng 32,5 m, sâu khoảng 46 m thay cho 19 m trước đây; lấy chiều sâu tầng 3–4 làm cơ sở rồi lùi 4 m ở mỗi đầu. Các kích thước là ước lượng theo ảnh. Menu **37. Balance upper tower length with lower floors** cập nhật riêng vỏ và mặt đứng khối tháp, Rebuild dùng cùng đường biên. [Bên hông](../../Docs/UEH-CampusB/Previews/121_B1_balanced_tower_full_side.png), [góc tổng thể](../../Docs/UEH-CampusB/Previews/122_B1_balanced_tower_front_corner.png).

**Chữ khối xanh lam viền trắng — 13/11/2026:** menu **36. Build freestanding blue and white university letters** thay dòng chữ nhỏ ở cạnh mái bằng bộ chữ khối dựng đứng có thanh đỡ riêng, phía trước ban công tầng 3 theo vùng khoanh đỏ. Rebuild dùng cùng thiết kế. Dữ liệu `Fonts/B1RaisedSignGeometry.json` được tạo từ font Roboto Bold trong dự án bằng `Docs/UEH-CampusB/generate-raised-sign.py`. Đây là mô phỏng thị giác theo ảnh Virtual Tour; không phải bản vẽ kỹ thuật hoặc bộ nhận diện gốc. [Mặt tiền](../../Docs/UEH-CampusB/Previews/117_B1_raised_sign_front.png), [viền và độ dày](../../Docs/UEH-CampusB/Previews/118_B1_raised_sign_white_outline_closeup.png).

**Tầng 3 và 4 lùi nhẹ — 13/11/2026:** mỗi cạnh lùi 0,6 m so với khối đế, tạo hai tầng trung gian rộng 43,8 m giữa khối đế 45 m và khối tháp 32,5 m. Chiều sâu còn khoảng 54 m; đây là kích thước ước lượng theo ảnh. Menu **35. Recess third and fourth floors slightly** cập nhật riêng hai tầng mở, đồng thời mã Rebuild dùng cùng đường biên mới. [Mặt tiền](../../Docs/UEH-CampusB/Previews/113_B1_mild_setback_front.png), [bên hông](../../Docs/UEH-CampusB/Previews/115_B1_mild_setback_side.png).

**Thu gọn ốp gỗ về hai khoang giữa — 13/11/2026:** ở cả hai cao độ, ốp gỗ chia thành hai mảng riêng, dừng trong mép cột phân khu và trước mép mái ngoài. Tổng cộng bốn mảng rộng khoảng 7,9/9,1 m, sâu 4,18 m. Menu **34. Restrict timber soffits to the central bays** cập nhật riêng nhóm sàn/trần; Rebuild cũng sử dụng giới hạn này. [Mặt tiền](../../Docs/UEH-CampusB/Previews/110_B1_timber_only_central_bays.png), [trần gỗ giữa](../../Docs/UEH-CampusB/Previews/111_B1_central_timber_ceiling_closeup.png), [trần cánh bên](../../Docs/UEH-CampusB/Previews/112_B1_plain_soffit_at_side_window_wing.png). Đã duyệt render Unity; chưa chạy Play Mode.

**Đồng bộ toàn bộ cụm hai cửa giữa — 13/11/2026:** viền ngoài, đố chia kính và cả mảng vách bao quanh hai cửa (trước đây màu gỗ nâu) dùng chung vật liệu `Ivory` với tường cơ sở. Mã Rebuild cũng dùng vật liệu này. [Toàn bộ cụm cửa](../../Docs/UEH-CampusB/Previews/109_B1_complete_central_surround_wall_colour.png), [ảnh cận cảnh](../../Docs/UEH-CampusB/Previews/108_B1_central_windows_wall_colour.png). Đã duyệt render Unity; chưa chạy Play Mode.

## Kéo mái và bỏ dải tường dày ở mép mặt tiền — 13/11/2026

- Theo ảnh 161025, kéo bản mái tại cao độ **10,8 m** ra thêm **0,6 m** phía trước và hai mép bên. Mái vươn 1,25 m tính từ trục cột, nối liền qua góc mặt tiền và bên hông; nẹp mái và trần gỗ theo mép mới.
- Bỏ cả tấm tường/biển rời cao 1,18 m cùng nẹp trên được khoanh trong ảnh 161222. Cạnh bản mái dày 0,36 m tạo thành mép hoàn thiện; tên trường thu nhỏ và gắn trực tiếp vào cạnh này.
- Menu **32. Extend canopy and remove deep fascia wall** cập nhật riêng bản mái và tên trường. Rebuild toàn bộ cũng dùng thiết kế mới. Kích thước kéo mái là ước lượng theo ảnh.

[104 — Mép mái kéo dài](../../Docs/UEH-CampusB/Previews/104_B1_extended_canopy_grazing_edge.png) · [105 — Dải tường đã bỏ](../../Docs/UEH-CampusB/Previews/105_B1_canopy_without_deep_fascia.png) · [106 — Mặt tiền mái mỏng](../../Docs/UEH-CampusB/Previews/106_B1_thin_continuous_canopy_front.png) · [107 — Góc nối mái](../../Docs/UEH-CampusB/Previews/107_B1_canopy_flush_return_corner.png).

[Tọa độ mái](../../Docs/UEH-CampusB/b1-canopy-layout.json), [đối chiếu bàn giao](../../Docs/UEH-CampusB/b1-canopy-delivery.json). Đã duyệt render Unity; chưa chạy Play Mode.

## Mở rộng tầng 4 bằng mặt bằng tầng dưới — 13/11/2026

- Tầng ban công trên cùng của khối đế, cao độ sàn **14,4 m**, mở ra **1,1 m mỗi bên**, **1,2 m phía trước** và **0,7 m phía sau** để trùng mép mặt bằng tầng ngay dưới. Mái phủ cao độ 18 m được mở rộng tương ứng; cột biên, kính, vách sau, lan can và phào nối theo mép mới.
- Theo các kích thước ước lượng hiện có, mặt bằng theo trục tăng từ **42,8 × 53,3 m** lên **45 × 55,2 m**. Các tầng và chi tiết ban công bên dưới giữ thiết kế đã duyệt.
- Menu **31. Expand fourth floor to lower podium footprint** cập nhật riêng nhóm hai tầng ban công và mái khối đế. Rebuild toàn bộ dùng cùng mặt bằng mới.

[100 — Mặt tiền tầng 4](../../Docs/UEH-CampusB/Previews/100_B1_fourth_floor_full_width_front.png) · [101 — Cột và góc mái thẳng trục](../../Docs/UEH-CampusB/Previews/101_B1_fourth_floor_aligned_corners.png) · [102 — Mặt bên](../../Docs/UEH-CampusB/Previews/102_B1_fourth_floor_full_depth_side.png) · [103 — Góc phía sau](../../Docs/UEH-CampusB/Previews/103_B1_fourth_floor_rear_alignment.png).

[Tọa độ và diện tích ước lượng](../../Docs/UEH-CampusB/b1-fourth-floor-layout.json), [đối chiếu bàn giao](../../Docs/UEH-CampusB/b1-fourth-floor-delivery.json). Đã duyệt render Unity; chưa chạy Play Mode.

## Chi tiết ban công giữa và bốn trục cửa bên phải — 13/11/2026

- Khép hai đầu ban công giữa bằng tường đặc màu kem, nối từ vách sau đến sát cột ngoài, có nẹp chân và nẹp trần. Vách sau nối kín qua trục cột giữa.
- Hai mảng kính giữa cùng rộng **5,2 m**, cao **4,35 m**; bản trước rộng khoảng 6,9/8,1 m và cao 3,25 m. Kính giữ vị trí lùi sau lan can, có khung và đố chia.
- Thêm **hai dải lam treo cao 1,2 m** trên hai khoang ban công, cùng vật liệu bạc và nẹp cam với mảng lam trên OCB.
- Cánh bên phải chia thành **bốn trục cửa đều nhau**, mỗi tầng bốn cửa, hai tầng thẳng trục; mỗi mảng kính khoảng **2,55 × 2,8 m**, có khung trắng, đố giữa, ô thoáng trên và tường ngăn giữa các cửa.
- Menu **30. Refine central balcony walls and four right windows** cập nhật riêng mặt đứng tầng ban công; Rebuild toàn bộ cũng dùng các chi tiết mới. Kích thước được ước lượng theo ảnh. Đã duyệt ảnh Unity, chưa chạy Play Mode.

[94 — Ban công giữa](../../Docs/UEH-CampusB/Previews/94_B1_central_balcony_finished.png) · [95 — Tường đầu trái](../../Docs/UEH-CampusB/Previews/95_B1_left_balcony_end_wall.png) · [96 — Tường đầu phải](../../Docs/UEH-CampusB/Previews/96_B1_right_balcony_end_wall.png) · [97 — Kính hẹp và cao hơn](../../Docs/UEH-CampusB/Previews/97_B1_narrow_taller_central_glass.png) · [98 — Hai dải lam treo](../../Docs/UEH-CampusB/Previews/98_B1_two_suspended_louvre_screens.png) · [99 — Bốn trục cửa bên phải](../../Docs/UEH-CampusB/Previews/99_B1_four_even_right_window_stacks.png).

[Kích thước chi tiết](../../Docs/UEH-CampusB/b1-balcony-details-layout.json), [đối chiếu bàn giao](../../Docs/UEH-CampusB/b1-balcony-details-delivery.json). Các mục bên dưới lưu lại các lần dựng trước.

## Hiên và tầng trên B1 chỉnh theo ảnh 150336/150754 — 13/11/2026

- Dời năm trục cột mặt tiền từ **Z = 0,8 m sang Z = 3,2 m**, tức ra phía sân thêm **2,4 m**; mặt kính và cột đá trong sảnh giữ vị trí. Hiên sâu khoảng **4,32 m** tính từ mặt kính, với trần gỗ và nền lát nối liền đến hàng cột mới.
- Bỏ hẳn hai chữ lơ lửng `UEH · 279 NGUYỄN TRI PHƯƠNG` và `UEH / CƠ SỞ B` trong prefab lẫn các đường dựng lại. Chữ tên trường trên dải biển mặt tiền vẫn gắn vào nền biển.
- Thay các tầng lặp bằng ô lam lớn trên OCB, ban công giữa sâu có hoa rủ, hai tầng kính bên phải, hai tầng ban công giật lùi phía trên; các mép mái/phào nối quanh khối. Khối tháp có dải ban công ngang phía trái và ba trục kính đứng phía phải khi nhìn từ sân.
- Menu **29. Refine B1 porch and stepped upper facade** dựng lại thiết kế này. Chiều sâu khung vẫn tới **Z = -52 m**. Các tỷ lệ, khoảng lùi và độ dời cột là ước lượng từ ảnh Virtual Tour.

[Toàn cảnh mới](../../Docs/UEH-CampusB/Previews/88_B1_refined_front.png) · [các tầng trên](../../Docs/UEH-CampusB/Previews/89_B1_reference_upper_storeys.png) · [hiên sau khi dời cột](../../Docs/UEH-CampusB/Previews/90_B1_deeper_porch_columns.png) · [cổng đã bỏ chữ](../../Docs/UEH-CampusB/Previews/91_B1_entrance_without_floating_labels.png) · [ban công và lam](../../Docs/UEH-CampusB/Previews/92_B1_stepped_terraces_and_screen.png) · [khung bên hông](../../Docs/UEH-CampusB/Previews/93_B1_full_depth_refined_frame.png).

[Đối chiếu lần chỉnh này](../../Docs/UEH-CampusB/b1-reference-facade-delivery.json). Đã dựng ảnh trong Unity; chưa chạy Play Mode. Mục khung ban đầu bên dưới là lịch sử trước lần chỉnh này.

## Khung bao sảnh B1 theo Virtual Tour — 13/11/2026

- Dựng lại **năm trục cột mặt tiền**, thân màu kem, nẹp nổi đôi, rãnh màu đá nhạt và chân đế tối theo ảnh khoanh đỏ. Nối với dầm và phào bao liên tục ở ba cao độ; bổ sung trần hiên gỗ, lam ngang, lan can và bồn hoa ở ban công.
- Khung chạy dọc hai bên tới **Z = -52 m**, vượt cuối sảnh 48 m thêm **4 m**; có cột và dầm nối ở phía sau, sàn hiên chạy liền quanh sảnh. Không dựng lại bức tường bịt cuối sảnh đã bỏ trước đó.
- Cập nhật `UEH_Exterior.prefab`, scene tham quan và tùy chọn `wrappedB1Frame`. Menu **28. Rebuild B1 structural frame around full lobby** cập nhật riêng phần khung; Rebuild toàn bộ cũng sử dụng thiết kế này.
- [Mặt tiền](../../Docs/UEH-CampusB/Previews/82_B1_frame_front.png), [góc Virtual Tour](../../Docs/UEH-CampusB/Previews/83_B1_frame_virtual_tour_angle.png), [cột và hiên cạnh OCB](../../Docs/UEH-CampusB/Previews/84_B1_frame_OCB_porch.png), [toàn bộ chiều sâu](../../Docs/UEH-CampusB/Previews/85_B1_frame_full_side_depth.png), [góc phía sau](../../Docs/UEH-CampusB/Previews/86_B1_frame_rear_overrun.png), [khung bao sảnh nhìn từ trên](../../Docs/UEH-CampusB/Previews/87_B1_frame_lobby_cutaway.png).
- Ảnh 87 chỉ tạm ẩn mái, kính và trần khi render để thấy khung bao quanh sảnh; prefab và scene vẫn có đủ các phần này. Kích thước suy từ ảnh, chưa phải số đo khảo sát. Đã dựng và xem sáu ảnh bằng Unity; kiểm tra collider của khung không chắn hai cổng, chưa chạy Play Mode.

[Tọa độ khung](../../Docs/UEH-CampusB/b1-frame-layout.json), [đối chiếu bàn giao](../../Docs/UEH-CampusB/b1-frame-delivery.json).

## Chi nhánh OCB tại mặt tiền sảnh B1 — 13/11/2026

- OCB đã chuyển sang khoang bên trái cột đá khi nhìn từ ngoài, đúng mũi tên/khoanh đỏ mới: tâm **X = 19,4 m**, mặt kính **Z = -1,12 m**, mép sau **Z = -7,12 m**. Diện tích **7,2 × 6 m (43,2 m²)**. Cột, mesh và collider giữ nguyên; cổng thông thoáng, bồn cây trước cổng đã hoàn lại.
- Bỏ các đoạn vách cũ che cửa và cắt qua bên trong office. Menu 27 cập nhật riêng office và các đoạn vách liên quan; Rebuild dùng cùng vị trí mới.
- Kích thước và vị trí là ước lượng theo ảnh, chưa phải số đo khảo sát.

| Ảnh duyệt | Góc nhìn |
|---|---|
| [78 — Mặt tiền chi nhánh](../../Docs/UEH-CampusB/Previews/78_OCB_branch_frontage.png) | Cửa kính và bảng OCB |
| [79 — Nhìn từ cổng](../../Docs/UEH-CampusB/Previews/79_OCB_branch_from_gate.png) | Vị trí cạnh cổng và lối tiếp cận |
| [80 — Bên trong chi nhánh](../../Docs/UEH-CampusB/Previews/80_OCB_branch_interior.png) | Quầy giao dịch và bàn tư vấn |
| [81 — Vị trí đã sửa](../../Docs/UEH-CampusB/Previews/81_OCB_corrected_side_bay.png) | Khoang bên trái cột góc theo mũi tên |

[Tọa độ và kích thước](../../Docs/UEH-CampusB/ocb-branch-layout.json), [đối chiếu cột](../../Docs/UEH-CampusB/ocb-relocation-delivery.json). Đã dựng và xem ảnh render bằng Unity; chưa chạy Play Mode cho lần cập nhật này.

Mở `Scenes/UEH_CampusB_Study.unity` bằng cách nhấp đúp trong Unity, sau đó nhấn **Play**. Scene tham quan có camera và điều khiển riêng, không cần GameManager hoặc save game.

Điều khiển: **WASD / phím mũi tên** để đi, **Shift** để chạy, **chuột** để nhìn, **Esc** để thả/khóa con trỏ. Phím **1** trở về ngoài cổng, **2** đến giữa sảnh, **H** ẩn/hiện hướng dẫn. Nhấp chuột để khóa lại con trỏ.

## Các prefab

**Mở cuối sảnh và thêm bình chữa cháy — 12/11/2026:** bỏ tường sáng cuối sảnh cùng chân viền/collider, để phía sau lộ ra theo ảnh mũi tên. Sân cây và lối học thuật đã xoá ở lần trước vẫn được bỏ. Thêm **một bình chữa cháy đỏ treo tường** ngay giữa office bên trái và phân khúc cây xanh/vách gỗ, hướng ra trục giữa sảnh; có giá treo, vòi đen, tay bóp, đồng hồ và nhãn ABC, biển nhận diện phía trên. Bình là mô hình trang trí tĩnh.

Menu **26. Open rear lobby and add left wall fire extinguisher**, cờ `openRearAndLeftExtinguisher`, giữ trạng thái khi Rebuild. Bình nằm trong nhóm `16_Left_wall_fire_extinguisher`; vị trí dọc sảnh lấy theo khoảng giữa hai mặt trưng bày. Ảnh mới: [cuối sảnh mở](../../Docs/UEH-CampusB/Previews/75_Rear_lobby_wall_removed.png) · [bình giữa hai khu](../../Docs/UEH-CampusB/Previews/76_Extinguisher_between_office_and_greenery.png) · [chi tiết bình](../../Docs/UEH-CampusB/Previews/77_Wall_mounted_extinguisher_detail.png). Lần này sinh asset và render Unity; chưa chạy Play Mode hoặc bài kiểm tra tự động. Mô tả tường khép cuối sảnh ở lần trước bên dưới là lịch sử.

**Office kính mở rộng và bỏ lối học thuật — 12/11/2026:** office bên phải dài **16 m** (trước 9,8 m), sâu **4,6 m**, từ **Z = -31,3 tới -47,3 m**, đi qua cột souvenir Z = -42 m. Mặt tiền ở **X = -13,4 m**, phòng lùi sau vách. Cửa giữ gần thang máy; có sáu bàn làm việc, tủ và sáu đèn. Bỏ hành lang/sân cây học thuật suy luận và biển chỉ dẫn; tường sáng khép cuối sảnh tại Z = -48 m. Kích thước là ước lượng theo ảnh.

Menu **25. Extend glass office past rear column and remove academic connection**, cờ `extendedGlassOfficeAndClosedRear`, giữ cấu hình khi dựng lại. Ảnh: [office mở rộng](../../Docs/UEH-CampusB/Previews/71_Extended_glass_office_front.png) · [office qua cột souvenir](../../Docs/UEH-CampusB/Previews/72_Office_past_souvenir_column.png) · [nội thất](../../Docs/UEH-CampusB/Previews/73_Extended_office_interior.png) · [cuối sảnh](../../Docs/UEH-CampusB/Previews/74_Rear_lobby_without_academic_connection.png). Lần này sinh asset và render Unity; chưa chạy Play Mode hoặc bài kiểm tra tự động. Các số đo office và lối học thuật ở những mục cũ là lịch sử.

**Nới khoảng tường trắng bên trái — 12/11/2026:** mảng xám xanh đầu dãy giữ vị trí; bộ ba Since 1976/9 màu/hình tròn lùi thêm **0,8 m**, office **1,9 m**, khu cây xanh/vách gỗ **3,05 m** dọc sảnh. Khoảng hở nhìn thấy giữa mảng xám xanh và khung Since khoảng **0,8 m**; giữa khung Since và office khoảng **1,72 m**; giữa office và khu cây xanh khoảng **1,59 m**. Tâm các khu lần lượt **Z = -9,4 / -22 / -34 m**, cùng tuyến mặt tiền X = 13,4 m. Kích thước là ước lượng theo ảnh.

Menu **24. Widen white wall gaps along left gallery**, cờ `widerLeftGaps`, lưu bố trí cho Rebuild và các công cụ đặt khu. Ảnh mới: [khoảng đầu dãy](../../Docs/UEH-CampusB/Previews/67_Wider_gap_slate_to_Since1976.png) · [Since và office](../../Docs/UEH-CampusB/Previews/68_Wider_gap_heritage_to_office.png) · [office và cây xanh](../../Docs/UEH-CampusB/Previews/69_Wider_gap_office_to_greenery.png) · [toàn dãy](../../Docs/UEH-CampusB/Previews/70_Left_gallery_wider_spacing_overview.png). Lượt này sinh asset/render Unity, chưa chạy lại Play Mode. Các khoảng hở 0,62/0,44 m và tọa độ Z cũ bên dưới là lịch sử.

**Hai cột phía sâu, souvenir và cây — 12/11/2026:** thêm cặp cột **X = ±4,5 m, Z = -42 m**, thân 2 × 2 m, nâng tổng số cột lên **8**. Tủ kính **UEH SOUVENIR** áp mặt trong cột mới bên phải, hướng mặt trưng bày **+X về giữa sảnh**; tâm khoảng **(-3,21; 0; -42)**, rộng 1,96 m, sâu 0,58 m, cao 4,1 m. Tủ có khung sáng, kính, sáu kệ, nền đỏ, biển tên chân đen và các mẫu mũ/bình/ly/quà mô phỏng. Thêm đúng ba cây chậu trắng tại **(4,5; 0; -22,2)**, **(4,5; 0; -40,2)**, **(-4,5; 0; -40,2)** theo các khoanh đỏ. Các số đo là ước lượng theo ảnh; cửa tủ đóng tĩnh.

Menu **23. Add rear columns souvenir cabinet and three trees**, cờ `rearColumnsAndSouvenir`, giữ cấu hình khi dựng lại. Chọn `14_Rear_souvenir_and_three_trees` để chỉnh tủ và từng cây. Ảnh: [cặp cột phía trong](../../Docs/UEH-CampusB/Previews/63_Rear_pair_souvenir_and_trees.png) · [tủ cạnh cột](../../Docs/UEH-CampusB/Previews/64_Souvenir_cabinet_at_column.png) · [tủ chính diện](../../Docs/UEH-CampusB/Previews/65_UEH_souvenir_display_front.png) · [ba chậu cây](../../Docs/UEH-CampusB/Previews/66_Three_new_white_pot_trees.png). Lượt này sinh asset/render Unity; chưa chạy lại Play Mode. Các ghi chú sáu cột của bản trước bên dưới là lịch sử.

**Dãy trái đưa về giữa sảnh — 12/11/2026:** toàn tuyến tường, mảng xám xanh, Since 1976, office và cây xanh/vách gỗ chuyển **4 m theo -X**, từ mặt tiền X = 17,4 m tới **13,4 m**, đối xứng tuyến bên phải X = -13,4 m. Cột ngoài phía Since chuyển từ X = 18,7 m tới **14,7 m**, cùng chân/đầu/collider. Nhịp kính phía Since thu từ **12,2 m còn 8,2 m** (X = 5,5…13,7 m), vẫn nằm giữa chiều dày cột tại Z = -1,12 m. Cửa giữ rộng **4,4 m**, tâm mới **X = 9,6 m**. Các số đo là ước lượng dựng hình.

Menu **22. Inset left wall heritage office and entrance glass**, tham số `leftWingInset = 4`, giữ bố trí khi dựng lại. Office nay nằm hoàn toàn trên sàn chính nên bỏ sàn phụ trùng mặt và đóng ô lớp ốp ngoài cũ. Ảnh mới: [Since 1976 và tường](../../Docs/UEH-CampusB/Previews/59_Inset_left_heritage_wall.png) · [hai bên sảnh](../../Docs/UEH-CampusB/Previews/60_Balanced_lobby_sides_and_gates.png) · [kính cổng thu gọn](../../Docs/UEH-CampusB/Previews/61_Shorter_Since_gate_glazing.png) · [office và khu cây xanh](../../Docs/UEH-CampusB/Previews/62_Inset_left_office_lounge.png). Các tọa độ X = 17,4 / 18,7 m và tâm cửa 11,6 m ở các đợt cũ bên dưới là lịch sử. Lượt này sinh asset và render Unity; chưa chạy lại Play Mode.

**Dãy bên phải — 12/11/2026:** dành **1,3 × 4,2 m tường đá trống** cạnh cây xanh WELCOME cho thiết bị chữa cháy; chưa đặt bình/tủ. Hành lang dịch vụ đưa tới **Z = -17,2 m**, ba thang A/B/C **-25 m**, office kính **-36,2 m** để các khu sát nhau hơn; tất cả vẫn lùi sau tuyến vách X = -13,4 m. Lan can chữ L nối từ **giữa mặt trước thân cột** tại **(-4,5; 0,6; -23)**, qua góc **(-4,5; 0,6; -19,25)**, tới trụ đá giữa thang C và hành lang toilet tại **(-13,32; 0,6; -19,25)**. Vị trí cột giữ nguyên. Menu **21. Compact right areas reserve fire wall and center railing joint**, cờ `compactRightSequence`, lưu bố trí khi dựng lại. Các kích thước là ước lượng theo ảnh.

Ảnh mới: [dãy bên phải](../../Docs/UEH-CampusB/Previews/55_Compact_right_WELCOME_services.png) · [tường chữa cháy để trống](../../Docs/UEH-CampusB/Previews/56_Empty_fire_equipment_wall.png) · [mối nối giữa thân cột](../../Docs/UEH-CampusB/Previews/57_Railing_middle_column_joint.png) · [thang máy và office](../../Docs/UEH-CampusB/Previews/58_Compact_right_lifts_office.png). Lượt này sinh asset/render bằng Unity, chưa chạy lại Play Mode. Các kết quả kiểm tra ngày 11/11 bên dưới là lịch sử.

**Cổng cập nhật — 11/11/2026:** cột ngoài cạnh Since 1976 chuyển tới **(18,7; 0; -1,12)** theo minh hoạ. Kính tại **Z = -1,12 m**, đi qua giữa chiều dày bốn cột cổng và nối đúng mặt đá giữa từng cặp cột. Ba nhịp gồm **18 ô kính cố định**, ô trên cao, khung đen và ba rèm phía phải. Cổng giữa mở rộng **5,6 m**; cổng Since mở rộng **4,4 m** tại X = 11,6 m. Mỗi cổng có hai cánh kính mở 90° vào trong, đi bộ qua được; cửa đang ở trạng thái mở cố định. Menu **20. Align gate glazing through columns and open Since entrance**, cờ `entranceGlassThroughColumns`, giữ cấu hình khi dựng lại. Các kích thước là ước lượng theo ảnh.

Ảnh: [toàn cảnh cổng](../../Docs/UEH-CampusB/Previews/52_Column_gates_from_lobby.png) · [lối cổng Since](../../Docs/UEH-CampusB/Previews/53_Since_gate_open_walkway.png) · [kính nối giữa thân cột](../../Docs/UEH-CampusB/Previews/54_Glazing_mid_column_joint.png). Unity kiểm tra tự động trong project riêng cùng script/packages/settings: `passed = true`; nhân vật đi qua cả hai cổng. Chi tiết và giới hạn kiểm tra ở [BuildReview](../../Docs/UEH-CampusB/BuildReview.vi.md).

**Dãy bên trái — 11/11/2026:** thêm mảng tường **xám xanh 1,45 × 4,2 m** với khung tranh nhỏ ngay bên trái Since 1976. Vị trí cột cạnh đó dùng bản cổng mới ở trên; giữa mặt sau cột và mảng xám xanh có đoạn tường trắng ngắn khoảng **0,405 m**. Mảng 1976 tại **Z = -8,6 m**, office **-20,1 m**, cây xanh/vách gỗ **-30,95 m**; khoảng cách mép nhìn thấy còn **0,62 m** và **0,44 m**. Chuyển ô mở tường/lớp ốp theo office, giữ phòng lùi sau vách X = 17,4 m. Hai volume bên trái rộng 10,6 m, sâu 6 m; tên nhóm `[12m x 6m]` giữ tương thích. Menu **19. Compact left heritage office lounge and slate wall**, cờ `compactLeftSequence`, lưu bố trí cho các lần dựng lại. Các kích thước là ước lượng theo ảnh.

Ảnh mới: [tường xám xanh và 1976](../../Docs/UEH-CampusB/Previews/47_Slate_wall_and_1976.png) · [dãy trái thu gọn](../../Docs/UEH-CampusB/Previews/48_Compact_left_sequence.png) · [office sát khu cây xanh](../../Docs/UEH-CampusB/Previews/49_Office_lounge_close_joint.png) · [cột sát mảng tường mới](../../Docs/UEH-CampusB/Previews/50_Stone_column_beside_slate.png).

**Cập nhật vị trí cột theo mũi tên:** cột đá cạnh kính đã lùi tiếp **2,9 m theo -X**, từ X = -16,7 m tới **X = -19,6 m**, giữ Z = -1,12 m. Mặt sau thân cột chạm vách cuối khu thang tại X = -20,6 m; cả chân, đầu cột và collider cùng dịch chuyển. Hai thang GV1/GV2 và năm cột còn lại giữ vị trí. Đây là kích thước bố trí ước lượng. Menu **18. Move faculty stone column to rear wall** áp dụng lại; cờ `facultyColumnAtRearWall` lưu vị trí cho những lần dựng sau. Xem [ảnh cột ở góc cuối](../../Docs/UEH-CampusB/Previews/46_Faculty_column_at_rear_wall.png). Các tọa độ -16,7 m của cột phải trong phần mô tả đợt trước bên dưới đã được thay bằng vị trí này.

**Bố trí hiện tại — sàn chính 36 × 48 m:** hai tuyến mặt tiền ở **X = ±13,4 m** sau khi dịch 4 m về giữa sảnh. WELCOME/quầy, hành lang toilet, ba thang A/B/C và office kính cùng tuyến vách. Hiện có 8 cột, thân **2 × 2 m**: cặp trong **X = ±4,5 m, Z = -24 m**; cặp mới **X = ±4,5 m, Z = -42 m**; bốn cột cổng tại **Z = -1,12 m**, X = **-19,6 / -4,5 / 4,5 / 14,7 m**. Cột cuối phía phải đã lùi sát vách cuối khu thang giảng viên. Trần chỉ giữ dải gỗ giữa. Thông số là **ước lượng từ ảnh**, chưa phải số đo thực tế.

**Thang giảng viên — 10/11/2026:** đi vào cạnh WELCOME qua ba trụ kiểm soát, **nhìn sang trái thấy hai thang GV1/GV2** trên vách vuông góc, phía sau WELCOME. Không còn cửa thang nhìn thẳng từ lối vào. Sảnh thang sâu khoảng **7,2 m**, rộng từ mép WELCOME tới mặt kính khoảng **6 m**, trần gần **4,2 m**; hai cửa rộng khoảng 1,82 m, mỗi cửa hai cánh riêng. Cửa, nút gọi và chỉ báo là mô hình tĩnh. Nhóm chỉnh sửa: `12_Faculty_lift_entrance/Faculty_lift_GV1` và `Faculty_lift_GV2`. Menu **17. Two faculty lifts behind WELCOME and recessed stone column** áp dụng riêng đợt này; cờ `facultyLiftsBehindWelcome` lưu cấu hình để các menu dựng lại cũng giữ hướng nhìn mới.

Xem [lối vào và cột đá đã lùi](../../Docs/UEH-CampusB/Previews/43_Faculty_entry_clear_glass.png), [hai thang ở bên trái](../../Docs/UEH-CampusB/Previews/44_Two_faculty_lifts_on_LEFT.png) và [góc nhìn sau WELCOME](../../Docs/UEH-CampusB/Previews/45_Faculty_behind_WELCOME_plan.png).

Đã bố trí **4 khu**, hai bên trái và hai bên phải tính từ cổng nhìn vào. Hai volume bên trái hiện **10,6 × 6 m**, tâm Z = **-22 m / -34 m**; hai volume bên phải **12 × 6 m**, tâm Z = -23 m / -37 m. Bên trái có office và cây xanh/vách gỗ; bên phải có hành lang dịch vụ, ba thang máy và office kính. Chọn nhóm `09_Four_expansion_areas` để chỉnh từng khu; các volume `Reserved_area_LEFT_01/02`, `Reserved_area_RIGHT_01/02` hiện gizmo khi chọn, cờ `furnished` phân biệt khoang đã bố trí. Hành lang/sân trong ở sau mép sảnh Z = -48 m. Phím **2** đưa tới giữa sảnh tại Z = -24 m. Các kích thước là ước lượng phục vụ bản dựng.

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

**Office lùi vào trong vách:** phòng sâu **2,8 m**, mặt tiền hiện ở **X = 13,4 m**, cùng tuyến với các mảng vách bên trái. Phòng nằm sau mặt tiền, ghế chờ sát vách. Ô mở tường sảnh chuyển theo phòng; sàn chính 36 × 48 m đã đủ phủ toàn bộ phòng nên không còn sàn phụ trùng mặt. Ô lớp ốp ngoài X = 18 m trước đó được đóng lại. Menu **10. Recess office behind lobby side wall**, menu 9 và Rebuild cùng đọc vị trí mới. Kích thước là ước lượng dựng hình.

1 Unity unit = 1 m; mặt trước hướng +Z; giữa ngưỡng cửa chính là `(0,0,0)`. Sàn sảnh và sân trước nối ở cao độ Y = 0.

| Phần | Giá trị mặc định | Tính chất |
|---|---:|---|
| Mặt tiền | 36 m | Ước lượng |
| Sảnh hiện tại | 36 × 48 m, cao 4,5 m | Mở rộng ước lượng theo yêu cầu bố trí |
| Bốn khu mở rộng | 2 trái, 2 phải; 12 × 6 m mỗi khu quy hoạch | Trái: office và cây/vách gỗ; phải: thang máy, hành lang dịch vụ và office kính |
| Cửa chính thông thủy | 5,6 m | Lựa chọn dựng hình/gameplay |
| Cột sảnh | 2 × 2 m; chân 2,12 m, đầu 2,24 m | Ước lượng, tăng từ thân 1,6 m |
| Hai cột giữa phía trong | X = ±4,5 m, Z = -24 m | Đối xứng qua trục X = 0, tại nửa chiều sâu sảnh 48 m |
| Hai cột mới phía sâu | X = ±4,5 m, Z = -42 m | Gần Không gian Học thuật; cột phải có tủ souvenir hướng vào lối giữa |
| Cột gần cổng và hai bên sảnh | Z = -1,12 m tại X = -19,6 / -4,5 / 4,5 / 14,7 m | Kính giữa chiều dày cột; cột phải sát vách cuối khu thang |
| Khối đế | 3 tầng, tầng trệt cao 4,5 m, hai nhịp tiếp theo 3,9 m | Diễn giải kiến trúc từ ảnh |
| Đỉnh khối cao tầng | 55 m | Tham khảo thông số dự án UEH 2018; hình dáng và khoảng lùi vẫn ước lượng |
| Tầng cao phía trên khối đế | 12 tầng | Cách phân chia đề xuất để tổng thể có 15 tầng |
| Camera người chơi | Cao 1,65 m | Quy ước tham quan |

Đây là bản tái dựng dựa trên ảnh panorama, không phải mô hình đo đạc. Mặt sau, lõi thang, tầng hầm và các phòng ngoài tuyến tham quan chưa được tái dựng chi tiết. Đoạn hành lang/sân trong là bối cảnh suy luận để kết thúc tuyến đi.

## Chỉnh hoặc dựng lại

Chỉnh trực tiếp prefab để thay bố trí, vật liệu và các nhóm kiến trúc. Muốn thay kích thước tổng thể bằng công cụ sinh, chọn `Settings/EstimatedDimensions.asset`, sửa tham số rồi dùng menu **UEH → Campus B → 2. Rebuild study scene from estimated dimensions**.

Lệnh Rebuild tạo lại các asset sinh trong thư mục này và scene khảo sát; chỉnh sửa thủ công trên các phần được sinh lại có thể bị ghi đè. Nên lưu một bản sao prefab/scene trước khi dùng Rebuild sau khi tự chỉnh. Các texture thủ tục hiện có được giữ lại.

Menu **1. Open walkthrough scene** mở scene tham quan riêng. Menu **3. Capture review images and local reflections** xuất 70 ảnh camera vào `Docs/UEH-CampusB/Previews`, đồng thời cập nhật cubemap của sảnh. Menu **4. Place 1976 left and WELCOME right from gate** đặt lại hai nhóm trên vách trái/phải của prefab hiện có mà không sinh lại toàn bộ kiến trúc. Menu **5. Place front columns at glazing and outer corners** đặt hàng 4 cột trước sát kính/góc tường theo cấu hình hiện tại, cập nhật cả mesh thân/chân/đầu cột và collider. Menu **6. Refine 1976 wall and three ordered sections** dựng lại riêng mảng 1976 theo thứ tự tường trống, cây xanh, 9 ô màu, hình tròn. Menu **7. Refine WELCOME wall and reserve lift space** dựng lại riêng khu tiếp tân; khi đã bật `facultyLiftEntrance`, hốc thang máy giảng viên cũng được dựng lại cạnh WELCOME. Menu 6 và 7 thay các chỉnh sửa thủ công trong nhóm tương ứng.

Ánh sáng bản đầu dùng nắng, ambient và đèn bổ trợ cùng phản chiếu cubemap chụp bằng Unity. Cubemap có box projection, không phải phản chiếu phẳng chính xác; không có lightmap GI hoàn chỉnh. Khi đổi bố trí lớn, chụp lại phản chiếu. Không cần chuyển render pipeline.

Font dùng Liberation Sans cùng giấy phép tại `Fonts/LICENSE.txt`. Các texture đá, gạch và gỗ được tạo bằng mã, không cần kết nối web khi mở scene.

Menu **8. Expand lobby for four side areas** áp dụng sảnh 48 m với bốn khu và bố trí cột hiện tại. Trạng thái `expandedLobby` và kích thước nằm trong `Settings/EstimatedDimensions.asset`; công cụ Rebuild cũng dùng bố trí mở rộng này. Menu 5 dành cho bố trí cột trước khi mở rộng và không thay cột khi trạng thái mở rộng đang bật. Ảnh xem trước hiện có **38 góc**, cùng **4 cubemap** cho phần trước/sau sảnh.

Kết quả kiểm tra và ảnh xem trước nằm trong `Docs/UEH-CampusB`. Bản dựng này dành để duyệt độc lập trước bước bố trí vào GameWorld.

Menu **9. Furnish left office and green timber lounge** dựng lại riêng hai khoang trái theo bố cục trên và thay các chỉnh sửa thủ công trong hai nhóm đó. Trạng thái `leftOfficeAndLounge` được lưu trong asset kích thước để Rebuild/menu 8 cũng giữ bố cục đã dựng.

## Thang máy và hành lang dịch vụ

Hai khoang phải được bố trí thành một dãy liên tục: **WELCOME → tường chữa cháy → hành lang dịch vụ → cụm 3 thang máy** theo chiều đi sâu vào sảnh. Tâm hành lang hiện tại **Z = -17,2 m**; tâm cụm thang máy **Z = -25 m**. Cụm thang máy nằm tiếp giáp miệng hành lang, thay vì đặt theo tâm hai khung dự phòng cũ cách nhau 14 m. Viền sàn dự phòng được thay bằng viền khu chờ trước thang máy.

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

Phần trống phía trong cụm thang máy được dựng thành office **9,8 × 4,6 m**, cao **4 m**, tâm hiện tại **Z = -36,2 m**. Toàn bộ phòng nằm sau mặt tiền **X = -13,4 m**, dùng phần sàn chính bên dưới và phần nối rất nhỏ vượt X = -18 m, Y = 0. Tường sảnh đã được mở ô theo vị trí mới. Office nằm ngoài chiều sâu 24 m của lớp ốp khối đế, nên không có lớp ốp ngoài nào chắn mặt kính tại đây.

**Nhìn chính diện từ sảnh vào office:** mặt trước và vách **bên phải** là kính trong khung kim loại, có dải nhận diện ngang; vách trái và vách sau là tường kín. Cửa kính rộng **1,25 m** ở phía phải mặt tiền, mở vào trong phòng. Nội thất có **3 bàn máy tính, ghế, tủ hồ sơ, cây và đèn trần**; lối đi ngoài sảnh không chứa bàn ghế hoặc khối phòng nhô ra. Kích thước đều là ước lượng để dựng hình theo bố cục người dùng yêu cầu.

Chỉnh nhóm `09_Four_expansion_areas/RIGHT_02 [12m x 6m]/Glass_office_recessed`: `01_FRONT_glass_wall`, `02_RIGHT_glass_wall`, `Glass_office_opaque_LEFT_and_BACK`, `03_Open_entrance_1_25m` cùng từng bàn/đèn là các nhóm riêng. Menu **12. Build recessed glass office near lifts** dựng lại riêng phòng này; tham số `rightGlassOffice` trong asset kích thước giữ phòng khi Rebuild/menu 8. Cửa kính đang ở tư thế mở cố định, chưa có tương tác đóng/mở. Ảnh **26–29** xem mặt tiền, góc kính phải, bên trong và quan hệ với cụm thang máy.

## Mở thông sảnh, cột giữa, lan can và đèn tròn

Menu **13. Open academic hall and refine columns lights railing** áp dụng riêng đợt chỉnh này. Trạng thái `refinedLobbyLayout` và `lobbyColumnWidth` được lưu trong `Settings/EstimatedDimensions.asset`; Rebuild/menu 8 giữ bố trí. Menu 13 giữ bề rộng đang lưu, tối thiểu 1,6 m; muốn chọn bề rộng khác thì sửa tham số và dùng Rebuild/menu 8.

- Bỏ hai mảng tường cuối sảnh tại **Z = -48 m**, hai vách của lối thông hẹp 6 m và đà đá thấp cũ. Sàn/trần đoạn nối sâu **6 m** mở rộng theo sảnh **36 m**, cùng cao độ Y = 0; sàn bối cảnh phía sau rộng tương ứng. Biển Không gian Học thuật được treo lên cao, không chắn đường đi.
- Sáu thân cột hiện **2 × 2 m**, tăng qua các đợt từ 0,9 m và 1,6 m. Cặp trong tại **Z = -24 m**, X = ±4,5 m. Bốn cột cổng cùng Z = -1,12 m, X = **-19,6 / -4,5 / 4,5 / 18,7 m**; kính đi qua giữa chiều dày cột. Cột phải chạm vách cuối khu thang; cột Since đã chuyển ra ngoài theo minh hoạ mới.
- `10_Lift_side_glass_railing`: lan can kính cao khoảng **1,1 m**, hình chữ **L**, từ giữa mặt trước cột **(-4,5; 0,6; -23)** tới góc rẽ **(-4,5; 0,6; -19,25)** rồi vào mặt trụ đá giữa thang máy C và hành lang toilet tại **(-13,32; 0,6; -19,25)**. Hai đoạn dài khoảng **3,75 m + 8,82 m**, tổng **12,57 m**, góc **90°** và **11 tấm kính có collider**. Sàn giữ nguyên cao độ. Các ảnh 30–38 và mô tả menu bên dưới ghi lại bố trí cũ; mốc mới ở đầu tài liệu được ưu tiên.
- `11_Central_round_spotlights`: **30 đèn tròn nhỏ**, hai hàng X = ±1,1 m dưới dải trần gỗ giữa, cách nhau **3 m** dọc sảnh. Thân tối, mặt phát sáng tròn đường kính khoảng **11,5 cm**; kết hợp đèn chùm đã có. Đây là chi tiết mesh phát sáng, không thêm 30 point light.

Kích thước trên là ước lượng dựng hình theo ảnh. Phần Không gian Học thuật/sân sau vẫn là bối cảnh suy luận, chưa dựng toàn bộ kiến trúc của khu đó. Ảnh Unity **30–34** xem lối mở, cặp cột, liên kết lan can, đèn tròn và đường đi bên hông qua vị trí tường cũ.

**Sửa đầu lan can theo ảnh đánh dấu:** menu **14. Connect glass railing to lift and toilet dividing wall** dựng lại riêng lan can tới đúng trụ đá giữa thang C và hành lang toilet, giữ nguyên đầu nối tại cột. Hai đoạn `Column_to_elbow` và `Elbow_to_dividing_wall` chỉnh được riêng; các mốc `Column_connection`, `Elbow_connection`, `Wall_connection` chỉ rõ đầu nối. Menu 13, Rebuild/menu 8 cũng dùng hình dáng nối tường này. Ảnh **35–36** xem toàn tuyến và mối nối vào trụ đá. Đường đi từ phía trước lan can tới thang máy vòng qua phía trong cột giữa; cửa vào hành lang toilet vẫn thông thoáng.

**Chỉnh trần, cột và chữ L theo ảnh Virtual Tour:** menu **15. Center timber only wider columns and right angle railing** chỉ dựng lại ba nhóm trần/cột/lan can. Đặt `centerTimberOnly = true`, thân cột **2 m**, xóa cả nan gỗ và nền tối của hai dải bên, giữ nguyên dải giữa, đèn chùm và 30 đèn tròn nhỏ. Lan can gồm hai đoạn song song với trục sảnh, vuông góc tại mốc `Elbow_connection`; không còn đoạn chéo. Mốc nối ở góc trước ngoài của cột rộng hơn, đầu cuối vẫn ở trụ đá đã chọn. Menu 8/Rebuild giữ bố cục theo tham số; menu 14 cũng dựng lan can chữ L. Các khu office, WELCOME, 1976, thang máy và hành lang dịch vụ được giữ. Ảnh **37–38** xem góc chữ L và trần/cột mới. Kích thước đều là ước lượng theo ảnh.
