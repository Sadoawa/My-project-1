using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lưu toàn bộ trạng thái trò chơi — port từ story.js và life.js.
/// </summary>
[Serializable]
public class GameState
{
    // ── Chỉ số nhân vật ──────────────────────────────────────────────────────
    public int   money       = 2_000_000;  // VND
    public int   energy      = 80;         // 0–100
    public int   study       = 20;         // 0–100
    public int   trust       = 50;         // 0–100
    public int   health      = 85;         // 0–100

    // ── Thời gian ─────────────────────────────────────────────────────────────
    public int   calendarDay = 1;          // 1–7 (Thứ Hai–Chủ Nhật)
    public float minutes     = 420f;       // phút trong ngày (0–1440)
    public int   version     = 4;

    // ── Xe máy ────────────────────────────────────────────────────────────────
    public int   bikeCondition = 100;      // 0–100

    // ── Trạng thái story (Demo 0.14 cũ) ──────────────────────────────────────
    public string choice  = null;          // "MEET" | "WORK" | "REST" | null
    public string task    = null;          // "serve" | "meet" | null
    public int    stage   = 0;
    public bool   phoneRead   = false;
    public bool   finished    = false;

    // ── Vị trí spawn ─────────────────────────────────────────────────────────
    public Vector3 spawnPosition = new Vector3(-18f, 0f, -13f);
    public float   spawnYaw     = Mathf.PI;

    // ── Lịch sử sự kiện ──────────────────────────────────────────────────────
    public List<HistoryEntry> history = new List<HistoryEntry>();

    // ── Flags tình huống đã xảy ra ───────────────────────────────────────────
    [SerializeField] public Dictionary<string, string> flags = new Dictionary<string, string>();

    // ── Hệ thống Life (đầy đủ từ life.js) ───────────────────────────────────
    public LifeState life = new LifeState();

    // ─────────────────────────────────────────────────────────────────────────
    // Áp delta lên state (giống applyDelta trong JS)
    // ─────────────────────────────────────────────────────────────────────────
    public void ApplyDelta(StatDelta delta, string eventName = null)
    {
        money  += delta.money;
        energy  = Mathf.Clamp(energy  + delta.energy,  0, 100);
        study   = Mathf.Clamp(study   + delta.study,   0, 100);
        trust   = Mathf.Clamp(trust   + delta.trust,   0, 100);
        health  = Mathf.Clamp(health  + delta.health,  0, 100);

        if (!string.IsNullOrEmpty(eventName))
        {
            history.Add(new HistoryEntry
            {
                day   = calendarDay,
                time  = TimeManager.FormatTime(minutes),
                evt   = eventName,
                delta = delta
            });
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Game-over check
    // ─────────────────────────────────────────────────────────────────────────
    public string GameOverReason()
    {
        if (money  <= 0) return "Bạn đã hết tiền.";
        if (energy <= 0) return "Bạn đã kiệt sức.";
        if (trust  <= 0) return "Bạn đã mất kết nối với mọi người.";
        if (health <= 0) return "Sức khỏe bạn đã kiệt quệ.";
        return null;
    }

    public bool IsGameOver() => GameOverReason() != null;
}

// ─────────────────────────────────────────────────────────────────────────────
// Các struct/class hỗ trợ
// ─────────────────────────────────────────────────────────────────────────────

[Serializable]
public class StatDelta
{
    public int money  = 0;
    public int energy = 0;
    public int study  = 0;
    public int trust  = 0;
    public int health = 0;

    public static StatDelta operator +(StatDelta a, StatDelta b) =>
        new StatDelta
        {
            money  = a.money  + b.money,
            energy = a.energy + b.energy,
            study  = a.study  + b.study,
            trust  = a.trust  + b.trust,
            health = a.health + b.health,
        };
}

[Serializable]
public class HistoryEntry
{
    public int       day;
    public string    time;
    public string    evt;
    public StatDelta delta;
}

[Serializable]
public class LifeState
{
    public bool   sleeping       = true;
    public bool   initialWake    = true;
    public float  slept          = 0f;      // phút đã ngủ
    public string mode           = null;    // "class" | "work" | "fixed" | null
    public bool   weekOver       = false;
    public bool   scheduleLocked = false;

    // Điểm danh theo slot id
    [SerializeField] public Dictionary<string, AttendRecord> attendance  = new Dictionary<string, AttendRecord>();
    [SerializeField] public Dictionary<string, bool>         meals       = new Dictionary<string, bool>();
    [SerializeField] public Dictionary<string, bool>         penalties   = new Dictionary<string, bool>();
    [SerializeField] public Dictionary<string, DecisionRecord> decisions = new Dictionary<string, DecisionRecord>();
    [SerializeField] public Dictionary<string, bool>         deadlineDone = new Dictionary<string, bool>();

    public List<GameMessage> messages = new List<GameMessage>();
    public List<string>      notices  = new List<string>();

    public MissionState mission = null;
    public string       pending = null;    // id của SURPRISE đang chờ quyết định
}

[Serializable]
public class AttendRecord
{
    public float minutes = 0f;
    public bool  closed  = false;
}

[Serializable]
public class DecisionRecord
{
    public string choice;
    public long   chosenAt;    // stamp (phút tuyệt đối)
    public string status;      // "accepted" | "done" | "failed"
}

[Serializable]
public class MissionState
{
    public string eventId;
    public string choice;
    public float  progress;    // phút đã hoàn thành
    public bool   reminded;
    public long   start;       // stamp
}

[Serializable]
public class GameMessage
{
    public int    id;
    public string from;
    public string text;
    public long   at;
    public bool   read;
    // Invite
    public bool   invite   = false;
    public string location = null;
    public string address  = null;
    public long   expires  = 0;
    public string reply    = null;
    public string replyText = null;
    public bool   done     = false;
    public bool   expired  = false;
}

