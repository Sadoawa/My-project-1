using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dữ liệu tĩnh của cốt truyện — port từ story.js.
/// Chứa CHOICES, ENCOUNTERS và các hàm áp dụng logic.
/// </summary>
public static class StoryData
{
    // ── TRẠNG THÁI BAN ĐẦU (Demo 0.14 cũ) ───────────────────────────────────
    public static GameState CreateInitialState()
    {
        var s = new GameState();
        s.money       = 2_000_000;
        s.energy      = 80;
        s.study       = 20;
        s.trust       = 50;
        s.health      = 85;
        s.calendarDay = 8;   // Demo 0.14 bắt đầu từ ngày 8 trong tháng
        s.minutes     = 920; // ~15:20
        s.choice      = null;
        s.task        = null;
        s.stage       = 0;
        s.bikeCondition = 100;
        s.spawnPosition = new Vector3(-18f, 0f, -13f);
        s.spawnYaw    = Mathf.PI;
        return s;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CHOICES — ba lựa chọn chiều ngày 8
    // ─────────────────────────────────────────────────────────────────────────
    public static readonly Dictionary<string, Choice> CHOICES = new Dictionary<string, Choice>
    {
        ["MEET"] = new Choice
        {
            name    = "Tham gia họp nhóm",
            icon    = "✎",
            delta   = new StatDelta { money = -60_000, energy = -10, study = 12, trust = 5 },
            receipt = "Bạn dành thời gian chuẩn bị cùng nhóm. Tốn một ít tiền và sức lực, đổi lại bạn hiểu rõ phần việc và gần gũi với mọi người hơn.",
            consequence = "\"May mà hôm trước bạn có tới họp! Phần của bạn đã thống nhất rồi. Mình có ghi chú đoạn chuyển ý cho bạn đây.\"",
            lesson      = "Bạn đã đầu tư vào sự chuẩn bị và quan hệ. Đến lúc cần phối hợp, nhóm sẵn sàng hỗ trợ bạn."
        },
        ["WORK"] = new Choice
        {
            name    = "Nhận ca ở quán",
            icon    = "☕",
            delta   = new StatDelta { money = 180_000, energy = -25, study = 0, trust = -5 },
            receipt = "Khay cà phê được giao đúng bàn. Bạn có thêm 180.000đ sau ca làm, nhưng mệt hơn và đã bỏ buổi họp nhóm.",
            consequence = "\"Hôm họp bạn vắng nên phần của bạn tụi mình phải phân lại. Bạn đọc nhanh tài liệu nhé, mọi người đang chuẩn bị phần của mình.\"",
            lesson      = "Ca làm giúp bạn bớt áp lực tiền bạc. Nhưng khoản thu ấy đi cùng sức lực và phần chuẩn bị chung đã bỏ lỡ."
        },
        ["REST"] = new Choice
        {
            name    = "Ở nhà nghỉ ngơi",
            icon    = "☾",
            delta   = new StatDelta { money = -40_000, energy = 20, study = 0, trust = 0 },
            receipt = "Bạn ăn một bữa đơn giản và ngủ một giấc. Sức lực đã hồi phục, nhưng buổi họp nhóm diễn ra mà không có bạn.",
            consequence = "\"Bạn khỏe hơn chưa? Hôm trước bạn không tới nên chắc chưa nắm phân công. Bạn xem lại tin nhắn nhóm rồi mình bắt đầu nhé.\"",
            lesson      = "Nghỉ ngơi giúp bạn hồi phục. Dù vậy, thông tin và công việc đã bỏ lỡ vẫn cần được bù lại."
        }
    };

    // ─────────────────────────────────────────────────────────────────────────
    // ENCOUNTERS — tình huống ngoài phố
    // ─────────────────────────────────────────────────────────────────────────
    public static readonly Dictionary<string, Encounter> ENCOUNTERS = new Dictionary<string, Encounter>
    {
        ["help"] = new Encounter
        {
            id      = "help",
            title   = "Tài liệu rơi giữa buổi chiều.",
            speaker = "BẠN SINH VIÊN · BÊN VỈA HÈ",
            quote   = "\"Bạn ơi, giúp mình nhặt mấy tờ này với! Mình sắp nộp bài mà gió thổi bay hết…\"",
            options = new List<EncounterOption>
            {
                new EncounterOption
                {
                    id          = "helped",
                    label       = "Dừng lại giúp bạn",
                    description = "−6 năng lượng · +8 quan hệ · 12 phút",
                    delta       = new StatDelta { energy = -6, trust = 8 },
                    minutes     = 12
                },
                new EncounterOption
                {
                    id          = "passed",
                    label       = "Xin lỗi, mình đang có việc",
                    description = "Tiếp tục đi · không đổi chỉ số",
                    delta       = new StatDelta(),
                    minutes     = 0
                }
            }
        },
        ["wallet"] = new Encounter
        {
            id      = "wallet",
            title   = "Một chiếc ví bị bỏ quên.",
            speaker = "GHẾ NGHỈ · TRƯỚC KHU HỌC XÁ",
            quote   = "Trong ví có thẻ sinh viên và 200.000đ. Có thể gửi lại quầy bảo vệ gần trường, hoặc giữ số tiền và bỏ chiếc ví lại.",
            options = new List<EncounterOption>
            {
                new EncounterOption
                {
                    id          = "returned",
                    label       = "Gửi ví lại bảo vệ",
                    description = "+10 quan hệ · 15 phút · chủ ví có thể liên hệ sau",
                    delta       = new StatDelta { trust = 10 },
                    minutes     = 15
                },
                new EncounterOption
                {
                    id          = "kept",
                    label       = "Giữ 200.000đ",
                    description = "+200.000đ · −12 quan hệ · có thể bị nhận ra",
                    delta       = new StatDelta { money = 200_000, trust = -12 },
                    minutes     = 2
                },
                new EncounterOption
                {
                    id          = "left",
                    label       = "Không đụng vào ví",
                    description = "Không đổi chỉ số",
                    delta       = new StatDelta(),
                    minutes     = 0
                }
            }
        }
    };

    // ─────────────────────────────────────────────────────────────────────────
    // Áp dụng lựa chọn chiều (MEET/WORK/REST)
    // ─────────────────────────────────────────────────────────────────────────
    public static bool ApplyChoice(GameState s, string key)
    {
        if (!string.IsNullOrEmpty(s.choice) || !CHOICES.ContainsKey(key)) return false;
        s.ApplyDelta(CHOICES[key].delta, $"choice:{key}");
        s.choice = key;
        s.task   = null;
        s.stage  = 0;
        s.history.Add(new HistoryEntry
        {
            day   = 8,
            time  = "afternoon",
            evt   = "choice:" + key,
            delta = CHOICES[key].delta
        });
        return true;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Áp dụng tình huống ngoài phố
    // ─────────────────────────────────────────────────────────────────────────
    public static bool ApplyEncounter(GameState s, string encounterId, string optionId)
    {
        if (!ENCOUNTERS.TryGetValue(encounterId, out var enc)) return false;
        var opt = enc.options.Find(o => o.id == optionId);
        if (opt == null) return false;
        if (s.flags.ContainsKey(encounterId)) return false;

        s.ApplyDelta(opt.delta, $"encounter:{encounterId}:{optionId}");
        s.flags[encounterId] = optionId;
        s.minutes += opt.minutes;
        return true;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Hậu quả ngày 18 (resolve cuối game)
    // ─────────────────────────────────────────────────────────────────────────
    public static void ResolveConsequences(GameState s)
    {
        if (s.flags.ContainsKey("resolved")) return;
        s.flags["resolved"] = "true";

        if (s.flags.TryGetValue("help", out var helpChoice) && helpChoice == "helped")
            s.ApplyDelta(new StatDelta { study = 3 }, "shared_notes");

        if (s.flags.TryGetValue("wallet", out var walletChoice))
        {
            if (walletChoice == "returned")
                s.ApplyDelta(new StatDelta { money = 40_000 }, "wallet_thanks");
            else if (walletChoice == "kept")
                s.ApplyDelta(new StatDelta { trust = -8 }, "wallet_recognized");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Thiệt hại va chạm xe
    // ─────────────────────────────────────────────────────────────────────────
    public static (int damage, int energy, int repairPerPoint) CrashCost(float speed)
    {
        int damage = Mathf.Min(45, Mathf.RoundToInt(Mathf.Abs(speed) * 2f));
        int eng    = Mathf.Min(25, Mathf.RoundToInt(Mathf.Abs(speed) * 1.3f));
        return (damage, eng, 2500);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Struct / Class hỗ trợ dữ liệu story
// ─────────────────────────────────────────────────────────────────────────────

[Serializable]
public class Choice
{
    public string    name;
    public string    icon;
    public StatDelta delta;
    public string    receipt;
    public string    consequence;
    public string    lesson;
}

[Serializable]
public class Encounter
{
    public string              id;
    public string              title;
    public string              speaker;
    public string              quote;
    public List<EncounterOption> options;
}

[Serializable]
public class EncounterOption
{
    public string    id;
    public string    label;
    public string    description;
    public StatDelta delta;
    public int       minutes;
}

