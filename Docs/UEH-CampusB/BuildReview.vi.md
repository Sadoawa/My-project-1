# Bản dựng UEH Cơ sở B để duyệt

Ngày: **05/11/2026**. Đã tạo bản tham quan độc lập và kiểm tra bằng **Unity 6000.6.8f1**. Kích thước và một phần kiến trúc được ước lượng theo hồ sơ khảo sát; đây chưa phải mô hình đo đạc.

**Cập nhật bố trí theo ảnh người dùng:** nhìn từ cổng vào, **1976 ở vách bên trái**, **WELCOME TO UEH ở vách bên phải**, gần cửa vào và hướng mặt về phía lối đi. Hai mảng đã được chuyển từ vách cuối sảnh sang hai vách bên. Prefab sảnh, scene, phản chiếu và ảnh xem trước đã được cập nhật.

**Bố trí hiện tại — mở rộng sảnh ngày 09/11/2026:** sàn tăng từ **36 × 24 m lên 36 × 48 m**, diện tích **1.728 m²** (thêm **864 m²**). Chừa **4 khu 12 × 6 m**, hai khu mỗi bên tại Z = -23 m và -37 m, với khung vách trống 10,2 × 3,2 m tương đương mảng Since 1976 và viền sàn; nội dung trang trí có thể bố trí tiếp. Theo xác nhận của người dùng, giữ **4 cột sát cổng và 2 cột giữa phía trong**, tổng cộng **6 cột**. Cặp cột giữa tại X = ±4,5 m, Z = -12 m; hàng cột cổng ở Z = -0,56 m với X = ±4,5 m và ±17,25 m, giữ vị trí sát kính/sát góc đã duyệt. Trần, tường, đèn và phản chiếu mở rộng đồng bộ; mép nối hành lang/sân trong chuyển từ Z = -24 m đến Z = -48 m. Khu 1976, WELCOME và vùng trống dành cho thang máy giữ vị trí. Kích thước là ước lượng phục vụ bản dựng. Nhóm `09_Four_expansion_areas` trong prefab cho phép chỉnh từng khu.

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
- 279 mesh asset được lưu trong `Assets/UEHCampusB/Meshes`: 139 mesh bản đầu, 23 mesh mới cho mảng 1976, 52 mesh cho khu WELCOME/chữ nổi, 26 mesh cho phần sảnh mở rộng, 37 mesh cho office/khu cây xanh-vách gỗ và 2 mesh cho sàn phụ/ốp tường đã mở ô của office lùi. Các mesh của những nhóm kiến trúc cũ được giữ trong thư mục nhưng không còn dùng trong prefab sau khi thay nhóm đó. Texture thủ tục, font có giấy phép, vật liệu dùng chung và bốn cubemap phản chiếu lưu cùng thư mục riêng của bản dựng.
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
| Cột sau mở rộng | Giữ 4 cột cổng và 2 cột giữa phía trong, tổng 6 cột; thân/chân/đầu/collider khớp |
| Bốn khu mở rộng | Mỗi vùng quy hoạch 12 × 6 m có sàn liên tục; trái là office/cây-vách gỗ, phải là thang máy/hành lang dịch vụ |
| Office và khu cây xanh/vách gỗ | Mặt tiền office cùng tuyến vách, phòng lùi 2,8 m, cửa đi được và sàn phụ liên tục; cây xanh ở bên trái vách gỗ, có hai ghế |
| Mảng 1976 nhìn từ sảnh | Tường trống bên trái, sau đó cây xanh SINCE 1976, đúng 9 ô màu, rồi 5 hình tròn với kích thước khác nhau |
| Khu WELCOME | Cột cạnh quầy đã bỏ; cây xanh bên trái, khoảng thoáng gần cửa vào 3,5 × 2,9 m không chứa collider; 12 chữ chính có mesh dày 6 cm, 6 hàng chữ nền có độ nổi |
| Đường đi trong Play Mode hiện tại | Đi hết khoảng **238,1 m** bằng CharacterController |
| Tuyến đi | Sân → cửa → vào/ra office → hành lang dịch vụ và bốn cửa → ba thang máy → cuối sảnh → hành lang tại Z = -52 m → ra ngoài |
| Liên tục cao độ | Không rơi khỏi sàn hoặc bị đẩy lên quá ngưỡng kiểm tra |
| Tường và kính cố định | Chặn nhân vật; cửa chính vẫn đi xuyên qua khoảng mở được |
| Xem hình | Đã xuất và xem ảnh ngoại thất, cửa, sảnh và quầy; sửa chữ xuyên tường, ánh sáng tối và dữ liệu GPU cũ khi dựng lại |

Bài kiểm tra đường đi dùng bộ điều khiển va chạm thật trong Play Mode, gọi bước di chuyển 1/60 giây ở tốc độ 4,8 m/s. Chưa có phiên kiểm thử thủ công bằng bàn phím/chuột trong Editor đang mở của người dùng, chưa benchmark FPS trên thiết bị đích, và chưa thử tương tác gameplay trong GameWorld.

Trong batch Editor, Unity phát sinh một `ArgumentOutOfRangeException` từ `UnityEditor.Search.SearchDatabase` ở bước khởi tạo chỉ mục tìm kiếm. Stack trace nằm trong hệ thống Search của Editor; không có frame script UEH ở lỗi này. Quá trình dựng, render và kiểm tra Play Mode vẫn hoàn tất. Lỗi này được giữ trong trích log; không được tính là lỗi của tuyến đi hoặc coi là log hoàn toàn sạch.

## Chi phí hình học và ánh sáng

Sau khi thêm thang máy và hành lang dịch vụ, scene được Unity kiểm đếm có **311 Mesh Renderer**, **40 vật liệu khác nhau đang dùng**, **123 collider tính cả CharacterController**, khoảng **235.646 tam giác mesh kiến trúc/cây/đồ vật/chữ nổi**. Chữ nổi có độ dày được tính trong số tam giác mesh; con số này không gồm các quad chữ TextMesh còn dùng ở biển tên và khu 1976. Mảng lá xanh dùng các lá dạng mesh đơn giản thay cho các khối cầu dẹt.

Đây là số lượng đối tượng và hình học, không phải draw call hoặc FPS đo được. Nắng và các đèn bổ trợ dùng chiếu sáng thời gian thực, kết hợp ambient và cubemap cục bộ. Bản này không có lightmap GI hoàn chỉnh; phản chiếu sàn là xấp xỉ bằng probe, không phải phản chiếu phẳng chính xác.

## Phạm vi bàn giao để duyệt

Hai prefab và scene tham quan đã sẵn sàng xem riêng. `GameWorld.unity`, các script gameplay, package manifest và build settings không được sửa để tích hợp bản dựng này. Khi đã duyệt tỷ lệ/diện mạo, bước tiếp theo mới là bố trí footprint vào bản đồ và căn lại các điểm tương tác.

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

Kết quả hiện tại trong [validation.json](validation.json): **311 Mesh Renderer**, **40 vật liệu đang dùng**, **123 collider tính cả CharacterController**, **235.646 tam giác mesh**, **25 ảnh** và **4 cubemap**. Có **369 mesh asset** lưu trên đĩa (gồm mesh các phiên bản trước được giữ lại); 90 asset được thêm trong đợt dựng dãy phải. Các số lượng ở những mục lịch sử phía trên phản ánh bản trước khi thêm thang máy.

Đã biên dịch trong Unity **6000.6.8f1** với **53 script C# hiện có** và cùng packages/Player Settings, lưu/mở lại scene, kiểm tra ba bộ thang, bốn cửa đúng phía, collider cửa đóng và sàn/lối vào hành lang tại năm độ sâu. CharacterController hoàn tất tuyến khoảng **238,1 m**, gồm vào/ra office, đi tới cả bốn cửa hành lang và ba thang máy; báo cáo không có mục thất bại. Cảnh báo API cũ `FindFirstObjectByType` và lỗi chỉ mục `UnityEditor.Search` được giữ trong trích log; không có lỗi biên dịch C#. Chưa kiểm thử thủ công trong Editor đang mở của người dùng.

Chỉ các asset UEH và tài liệu khảo sát được cập nhật; chưa ghép vào `GameWorld`.
