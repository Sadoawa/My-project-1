# CoSoB2 — bản dựng nghiên cứu từ video

Nguồn: `../../video.mp4.mp4`, 1080 × 1920, 30 fps, 91,6 giây. Chỉ dùng hình ảnh; không nghe, nhận dạng hay trích âm thanh.

## Ảnh tham chiếu

46 ảnh lấy đúng frame 0, 60, 120…2700, tương ứng 0–90 giây, bước 2 giây. Xem [bảng tổng hợp](contact.jpg) và [bảng mốc thời gian](frames.csv). Ảnh giảm còn 720 × 1280 để quan sát, không dùng làm texture dán người/chữ TikTok lên model.

| Mốc | Ảnh | Chi tiết dùng |
|---|---|---|
| 0, 1, 2 s | [0 s](Frames/frame_0001.jpg), [1 s](Frames/detail_1s.jpg), [2 s](Frames/frame_0002.jpg) | Dãy B2 thấp màu kem bên trái: nhịp vòm, cột vuông, hành lang mở, lan can đặc, cây trồng. Mặt tiền chỉ nhìn thấy một phần, không xác định toàn bộ số nhịp. |
| 3, 4 s | [3 s](Frames/detail_3s.jpg), [4 s](Frames/frame_0003.jpg) | Sảnh có cột kem, sàn đỏ nâu, tủ cứu hỏa đỏ và máy bán hàng đen. Vị trí trong model chỉ mang tính minh họa. |
| 7, 8 s | [7 s](Frames/detail_7s.jpg), [8 s](Frames/frame_0005.jpg) | Vòm lối vào thật, phào kép sáng màu, khóa đỉnh, khung kính, bàn ghế và lối thông. |
| 10–24 s | [12 s](Frames/frame_0007.jpg), [16 s](Frames/frame_0009.jpg) | Hành lang có mái, dầm, ô kính và cây xanh. Không dựng người hoặc bàn sự kiện tạm thời. |
| 30–36 s | [32 s](Frames/frame_0017.jpg), [35 s](Frames/detail_35s.jpg), [36 s](Frames/frame_0019.jpg) | Bồn cây đá xám, cây tán rộng, ghế cam và sân trong. Các khối cao tầng, cầu thang lớn thuộc bối cảnh liên thông, chưa đủ căn cứ gán riêng cho B2 nên không ghép thành một khối B2 giả định. |
| 56, 57 s | [57 s](Frames/detail_57s.jpg) | Video ghi rõ tòa B1: loại khỏi hình khối B2. |
| 58–64 s | [60 s](Frames/frame_0031.jpg) | Hành lang trên cao, khung kính, lan can; tham khảo màu, không suy ra thêm tầng B2 từ đoạn này. |
| 87, 88 s | [87 s](Frames/detail_87s.jpg), [88 s](Frames/frame_0045.jpg) | Mặt nhìn từ sân có cửa kính vòm tầng dưới, cửa chữ nhật tầng trên, cột kem và phào trắng. Quan hệ chính xác với mặt tiền đầu clip chưa được khảo sát. |
| 89, 90 s | [89 s](Frames/detail_89s.jpg), [90 s](Frames/frame_0046.jpg) | Lối thông vòm nhìn sang khu B1. Dùng tham khảo hình vòm và nền, không sao chép biển B1 thành biển B2. |

## Phần quan sát được và phần ước lượng

- Quan sát được: ngôn ngữ kiến trúc vòm, cột vuông, tường kem, chỉ nổi trắng ngà, lan can đặc có ô trang trí, kính xám xanh, nền đá và cây xanh.
- Mặt tiền đầu video cho thấy tầng trệt và hai cao độ phía trên; mái bị cắt khuôn hình. Model chọn **3 tầng nhìn thấy**, không khẳng định đây là tổng số tầng thực tế.
- Chọn **7 nhịp × 4,2 m**, dài 29,4 m; sâu 9 m; cao tầng 3,9 m. **Tất cả là ước lượng**, không phải số đo thực địa. Chiều cao người/cửa chỉ dùng để giữ tỷ lệ hợp lý.
- Đường cong vòm dùng nửa ellipse bán kính ngang 1,81 m, độ vồng 1,48 m; chi tiết phào được đơn giản hóa.
- Giả định mặt có cửa kính hướng về sân ở mặt đối diện dãy vòm. Đây là cách bố trí nghiên cứu, **chưa được video xác nhận liên tục bằng một cú máy**.
- Mặt khuất, vách đầu hồi, chiều sâu phòng, mái phẳng và số cửa lặp lại là hình khối giản lược. Cửa kính là vật liệu tĩnh có màu, chưa mô phỏng quang học kính trong suốt.
- Bồn cây, cây, ghế, máy bán hàng và tủ cứu hỏa là chi tiết nhận diện; số lượng và vị trí cụ thể được ước lượng. Không dựng các khu vực không xác định chắc là B2 như khối cao tầng sân trong/cầu thang lớn.
- Chữ “Tòa B2” ở 2 s là đồ họa chèn trên clip, không phải biển kiến trúc. Không có biển B2 gắn tường đủ rõ để chép chính xác nên không bịa biển hiệu.

## Xem trong Unity

1. Trong Project, mở `Assets/CoSoB2/Scenes/CoSoB2_Preview.unity`. Nếu Unity hỏi lưu scene đang sửa, lưu theo ý bạn trước khi chuyển.
2. Nhấn Play; dùng năm nút ở góc trái để đổi góc: toàn cảnh, mặt tiền, sân trong, cửa sân, lối thông.
3. Trong Scene view, chọn `CoSoB2` rồi nhấn F; Alt + kéo chuột để xoay quan sát.
4. Prefab nằm ở `Assets/CoSoB2/Prefabs/CoSoB2.prefab`. Nhấp đúp để sửa riêng từng nhóm kiến trúc.

Model dùng mesh cửa vòm tạo trong Unity và các primitive/material Standard sẵn có, không cần plugin mô hình mới. Scene xem thử có camera và ánh sáng riêng. Đây là bản dựng nghiên cứu từ video, không phải bản đo đạc/as-built.
