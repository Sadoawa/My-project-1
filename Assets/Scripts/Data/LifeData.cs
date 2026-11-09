using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dữ liệu tĩnh của hệ thống cuộc sống sinh viên — port từ life.js.
/// Bao gồm: lịch học/làm, bữa ăn, tình huống bất ngờ.
/// </summary>
public static class LifeData
{
    // ── NGÀY TRONG TUẦN ───────────────────────────────────────────────────────
    public static readonly string[] DAYS = {
        "Thứ Hai","Thứ Ba","Thứ Tư","Thứ Năm","Thứ Sáu","Thứ Bảy","Chủ Nhật"
    };

    // ── BỮA ĂN ───────────────────────────────────────────────────────────────
    public static readonly MealDef[] MEALS = {
        new MealDef { id = "breakfast", name = "sáng", start = 360, end = 510 },
        new MealDef { id = "lunch",     name = "trưa", start = 690, end = 780 },
        new MealDef { id = "dinner",    name = "tối",  start = 1080, end = 1230 }
    };

    // ── LỊCH HỌC/LÀM THEO NGÀY (1–7) ─────────────────────────────────────────
    // Mỗi slot: (loại, giờ bắt đầu, giờ kết thúc)
    public static readonly Dictionary<int, List<ScheduleSlot>> SCHEDULE = new Dictionary<int, List<ScheduleSlot>>
    {
        [1] = new List<ScheduleSlot>
        {
            new ScheduleSlot { type = "class", start = 450, end = 690 },
            new ScheduleSlot { type = "work",  start = 780, end = 1020 }
        },
        [2] = new List<ScheduleSlot>
        {
            new ScheduleSlot { type = "work",  start = 510, end = 660 }
        },
        [3] = new List<ScheduleSlot>
        {
            new ScheduleSlot { type = "class", start = 450, end = 690 },
            new ScheduleSlot { type = "class", start = 780, end = 1020 }
        },
        [4] = new List<ScheduleSlot>
        {
            new ScheduleSlot { type = "work",  start = 510, end = 660 },
            new ScheduleSlot { type = "work",  start = 780, end = 1020 }
        },
        [5] = new List<ScheduleSlot>
        {
            new ScheduleSlot { type = "class", start = 450, end = 690 },
            new ScheduleSlot { type = "work",  start = 780, end = 1020 }
        },
        [6] = new List<ScheduleSlot>
        {
            new ScheduleSlot { type = "work",  start = 510, end = 660 },
            new ScheduleSlot { type = "work",  start = 780, end = 1020 }
        },
        [7] = new List<ScheduleSlot>()
    };

    // ── TÌNH HUỐNG BẤT NGỜ (SURPRISES) ────────────────────────────────────────
    public static readonly List<SurpriseEvent> SURPRISES = new List<SurpriseEvent>
    {
        new SurpriseEvent
        {
            id          = "cover",
            day         = 1,
            time        = 540,
            chooseUntil = 555,
            deadline    = 690,
            title       = "Lời nhờ làm thay buổi sáng",
            text        = "Anh Minh nhờ bạn hỗ trợ quán 75 phút trước 11:30. Bạn ưu tiên đi làm thay hay học?",
            options     = new List<SurpriseOption>
            {
                new SurpriseOption
                {
                    id       = "go",
                    label    = "Rời lớp, tới quán làm thay",
                    location = "cafe",
                    mode     = "work",
                    required = 75,
                    reward   = new StatDelta { money = 90_000, energy = -7, trust = 4 },
                    note     = "Tới quán Chạm, gặp quản lý. Làm đủ 75 phút trước 11:30 để nhận 90.000đ."
                },
                new SurpriseOption
                {
                    id       = "stay",
                    label    = "Ưu tiên học, từ chối ca thay",
                    location = "school",
                    mode     = "class",
                    required = 75,
                    reward   = new StatDelta { study = 4, trust = -3 },
                    note     = "Tiếp tục ngồi học 75 phút trước 11:30. −3 quan hệ khi từ chối."
                }
            }
        },
        new SurpriseEvent
        {
            id          = "hospital",
            day         = 3,
            time        = 840,
            chooseUntil = 855,
            deadline    = 1020,
            title       = "Linh nhập viện cần người thăm",
            text        = "Linh nhắn đang ở Bệnh viện An Tâm, phòng 02, sau khi ngã xe.",
            options     = new List<SurpriseOption>
            {
                new SurpriseOption
                {
                    id       = "visit",
                    label    = "Rời lớp, đến bệnh viện thăm Linh",
                    location = "hospital",
                    mode     = "visit",
                    required = 20,
                    reward   = new StatDelta { money = -20_000, trust = 12, energy = -4 },
                    note     = "Thăm 20 phút trước 17:00. Chi phí đi lại 20.000đ."
                },
                new SurpriseOption
                {
                    id       = "stay",
                    label    = "Ở lại học, gọi hỏi thăm sau",
                    location = "school",
                    mode     = "class",
                    required = 60,
                    reward   = new StatDelta { study = 4, trust = -8 },
                    note     = "Tiếp tục học 60 phút. −8 quan hệ."
                }
            }
        },
        new SurpriseEvent
        {
            id          = "deadline",
            day         = 6,
            time        = 900,
            chooseUntil = 915,
            deadline    = 1020,
            title       = "Nhóm gọi họp gấp trước hạn",
            text        = "Bài nhóm cần chỉnh trước 17:00. Linh rủ tới B.204 ngay.",
            options     = new List<SurpriseOption>
            {
                new SurpriseOption
                {
                    id       = "group",
                    label    = "Xin về sớm để tới họp nhóm",
                    location = "school",
                    mode     = "group",
                    required = 40,
                    reward   = new StatDelta { study = 10, trust = 8, energy = -6 },
                    note     = "Tới lớp B.204, ngồi họp 40 phút."
                },
                new SurpriseOption
                {
                    id       = "stay",
                    label    = "Ưu tiên tiếp tục làm tại quán",
                    location = "cafe",
                    mode     = "work",
                    required = 75,
                    reward   = new StatDelta { trust = -6, energy = -3 },
                    note     = "Làm thêm 75 phút. −6 quan hệ."
                }
            }
        }
    };

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Trả về ngày trong tuần (1=Thứ Hai … 7=Chủ Nhật)</summary>
    public static int Weekday(int calendarDay) => ((calendarDay - 1) % 7) + 1;

    /// <summary>Stamp tuyệt đối (phút) = (ngày-1)*1440 + phút</summary>
    public static long Stamp(GameState s) => (long)(s.calendarDay - 1) * 1440 + (long)s.minutes;

    /// <summary>Trả về bữa ăn đang trong khung giờ, hoặc null</summary>
    public static MealDef CurrentMeal(GameState s)
    {
        foreach (var m in MEALS)
            if (s.minutes >= m.start && s.minutes < m.end)
                return m;
        return null;
    }

    /// <summary>Trả về slot đang hoạt động, hoặc null</summary>
    public static ScheduleSlot ActiveSlot(GameState s)
    {
        int wd = Weekday(s.calendarDay);
        if (!SCHEDULE.TryGetValue(wd, out var daySlots)) return null;
        foreach (var slot in daySlots)
            if (s.minutes >= slot.start && s.minutes < slot.end)
                return slot;
        return null;
    }

    /// <summary>Khởi tạo Life state khi bắt đầu game đầy đủ</summary>
    public static void InitLife(GameState s)
    {
        s.version     = 4;
        s.calendarDay = 1;
        s.minutes     = 420;
        s.health      = 85;
        s.energy      = 85;
        s.study       = 45;
        s.choice      = null;
        s.task        = null;
        s.stage       = 0;
        s.finished    = false;
        s.flags       = new Dictionary<string, string>();
        s.history     = new List<HistoryEntry>();
        s.life        = new LifeState
        {
            sleeping     = true,
            initialWake  = true,
            slept        = 0f
        };
    }

    /// <summary>Ăn bữa — trả về (thành công, lý do nếu thất bại)</summary>
    public static (bool ok, string reason) EatMeal(GameState s, int cost, int energyGain = 14)
    {
        var m = CurrentMeal(s);
        if (m == null) return (false, "Ngoài khung bữa chính.");
        string key = $"{s.calendarDay}:{m.id}";
        if (s.life.meals.ContainsKey(key)) return (false, $"Bạn đã ăn bữa {m.name} hôm nay.");
        if (s.money < cost) return (false, "Không đủ tiền.");
        s.life.meals[key] = true;
        s.ApplyDelta(new StatDelta { money = -cost, energy = energyGain, health = 3 }, $"Ăn {m.name} đúng khung giờ");
        return (true, null);
    }

    /// <summary>Bắt đầu ngủ (sau 21:00 hoặc trước 06:00)</summary>
    public static (bool ok, string reason) StartSleep(GameState s)
    {
        if (s.minutes < 1260 && s.minutes > 360)
            return (false, "Ngủ đêm từ 21:00. Giờ này có thể ngủ trưa/nghỉ ngắn.");
        s.life.sleeping = true;
        s.life.mode     = null;
        s.life.slept    = 0f;
        return (true, null);
    }

    public static void AddNotice(GameState s, string text)
    {
        s.life.notices.Add(text);
        if (s.life.notices.Count > 12) s.life.notices.RemoveAt(0);
    }

    public static void AddMessage(GameState s, string from, string text, string location = null)
    {
        var msg = new GameMessage
        {
            id       = s.life.messages.Count + 1,
            from     = from,
            text     = text,
            at       = Stamp(s),
            read     = false,
            location = location
        };
        s.life.messages.Add(msg);
        if (s.life.messages.Count > 100) s.life.messages.RemoveAt(0);
        s.phoneRead = false;
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Struct hỗ trợ
// ─────────────────────────────────────────────────────────────────────────────

[Serializable]
public class MealDef
{
    public string id;
    public string name;
    public int    start;  // phút trong ngày
    public int    end;
}

[Serializable]
public class ScheduleSlot
{
    public string id;      // sinh tự động
    public string type;    // "class" | "work" | "fixed"
    public string name;
    public string location;
    public int    start;
    public int    end;
}

[Serializable]
public class SurpriseEvent
{
    public string              id;
    public int                 day;
    public int                 time;
    public int                 chooseUntil;
    public int                 deadline;
    public string              title;
    public string              text;
    public List<SurpriseOption> options;
}

[Serializable]
public class SurpriseOption
{
    public string    id;
    public string    label;
    public string    location;
    public string    mode;
    public int       required;   // phút cần hoàn thành
    public StatDelta reward;
    public string    note;
}

