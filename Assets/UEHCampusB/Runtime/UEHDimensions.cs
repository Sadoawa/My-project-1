using UnityEngine;

namespace UEH.CampusB
{
    [CreateAssetMenu(menuName = "UEH/Campus B estimated dimensions")]
    public sealed class UEHDimensions : ScriptableObject
    {
        [TextArea(3, 6)] public string notice = "KÍCH THƯỚC ƯỚC LƯỢNG từ panorama; không phải bản vẽ đo đạc. Mốc cao 55 m tham khảo dự án UEH 2018; cách phân tầng và khoảng lùi được ước lượng.";
        [Min(24)] public float frontage = 36;
        [Min(18)] public float lobbyDepth = 24;
        [Min(18)] public float exteriorDepth = 24;
        public bool expandedLobby;
        public bool keepEntranceColumnsWhenExpanded = true;
        public bool leftOfficeAndLounge;
        public bool rightLiftsAndServices;
        [Range(3.8f, 6)] public float lobbyHeight = 4.5f;
        [Range(3, 8)] public float entryWidth = 5.6f;
        [Range(30, 65)] public float towerHeight = 55;
        [Range(8, 16)] public int upperFloors = 12;
        [Range(8, 18)] public float forecourtDepth = 15;
        [TextArea(2, 5)] public string omittedGeometry = "Mái, mặt sau, lõi thang, tầng hầm và các phòng ngoài tuyến tham quan chưa có dữ liệu đo. Khối tầng cao dựng vỏ ngoài; lối ra sân trong là phần bối cảnh suy luận.";
    }
}
