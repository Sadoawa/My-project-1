using UnityEngine;
using System;

/// <summary>
/// Quản lý thời gian trong game.
/// 1 phút game = timeScale giây thực (mặc định 2 giây).
/// Phát sự kiện OnMinuteChanged, OnHourChanged, OnDayChanged.
/// </summary>
public class TimeManager : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────────
    public static TimeManager Instance { get; private set; }

    // ── Cài đặt ───────────────────────────────────────────────────────────────
    [Header("Tốc độ thời gian")]
    [Tooltip("Số giây thực = 1 phút game")]
    public float secondsPerGameMinute = 2f;

    [Tooltip("Có đang chạy không")]
    public bool isRunning = false;

    // ── Sự kiện ───────────────────────────────────────────────────────────────
    public event Action<float>  OnMinuteChanged;   // minutes (0–1440)
    public event Action<int>    OnHourChanged;     // giờ (0–23)
    public event Action<int>    OnDayChanged;      // calendarDay

    // ── Nội bộ ────────────────────────────────────────────────────────────────
    private float    _accumulator = 0f;
    private int      _lastHour    = -1;
    private GameState _state => GameManager.Instance?.State;

    // ─────────────────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (!isRunning || _state == null || _state.life.weekOver) return;
        if (_state.life.sleeping) return; // thời gian vẫn tính nhưng do TickSleep xử lý riêng

        _accumulator += Time.deltaTime;
        float minutesPerSecond = 1f / secondsPerGameMinute;

        while (_accumulator >= secondsPerGameMinute)
        {
            _accumulator -= secondsPerGameMinute;
            AdvanceOneMinute();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    public void AdvanceOneMinute()
    {
        if (_state == null) return;

        _state.minutes += 1f;
        CheckDayRollover();
        CheckMeals();
        CheckSchedule();
        CheckSurprises();
        CheckNightPenalties();
        PassiveEnergyDrain();

        OnMinuteChanged?.Invoke(_state.minutes);

        int currentHour = Mathf.FloorToInt(_state.minutes / 60f);
        if (currentHour != _lastHour)
        {
            _lastHour = currentHour;
            OnHourChanged?.Invoke(currentHour);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    void CheckDayRollover()
    {
        if (_state.minutes < 1440f) return;

        // Đóng tất cả slot của ngày cũ
        CloseAllSlotsForDay();

        _state.calendarDay++;
        _state.minutes -= 1440f;
        _state.ApplyDelta(new StatDelta { money = -20_000 }, "Sinh hoạt phí qua đêm");
        _lastHour = -1;

        int wd = LifeData.Weekday(_state.calendarDay);
        if (_state.calendarDay > 1 && wd == 1)
        {
            _state.life.weekOver = true;
            _state.finished      = true;
            LifeData.AddNotice(_state, "Đã hết Chủ Nhật. Xem tổng kết tuần.");
        }

        OnDayChanged?.Invoke(_state.calendarDay);
    }

    // ─────────────────────────────────────────────────────────────────────────
    void CheckMeals()
    {
        foreach (var meal in LifeData.MEALS)
        {
            string key = $"{_state.calendarDay}:{meal.id}";
            if (_state.minutes >= meal.end
                && !_state.life.meals.ContainsKey(key)
                && !_state.life.penalties.ContainsKey(key))
            {
                _state.life.penalties[key] = true;
                _state.ApplyDelta(new StatDelta { health = -8, energy = -6 }, $"Bỏ bữa {meal.name}");
                LifeData.AddNotice(_state, $"Bỏ bữa {meal.name}: −8 sức khỏe, −6 năng lượng.");
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    void CheckSchedule()
    {
        int wd = LifeData.Weekday(_state.calendarDay);
        if (!LifeData.SCHEDULE.TryGetValue(wd, out var slots)) return;

        foreach (var slot in slots)
        {
            if (slot.id == null)
                slot.id = $"{_state.calendarDay}:{slot.type}:{slot.start}";

            // Nhắc nhở 15 phút trước
            string remindKey = $"reminder:{slot.id}";
            if (_state.minutes >= slot.start - 15 && _state.minutes < slot.start
                && !_state.life.penalties.ContainsKey(remindKey))
            {
                _state.life.penalties[remindKey] = true;
                string slotName = slot.type == "class" ? "Lớp B.204" : "Ca làm Chạm Coffee";
                LifeData.AddMessage(_state, slot.type == "class" ? "Lớp B.204" : "Anh Minh",
                    $"{slotName} bắt đầu {FormatTime(slot.start)}. Hãy đến đúng giờ.", slot.location);
            }

            // Đóng slot khi hết giờ
            if (_state.minutes >= slot.end)
                CloseSlot(slot);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    void CloseSlot(ScheduleSlot slot)
    {
        if (!_state.life.attendance.TryGetValue(slot.id, out var rec)) return;
        if (rec.closed) return;
        rec.closed = true;

        int full     = slot.end - slot.start;
        int attended = Mathf.Min(full, Mathf.RoundToInt(rec.minutes));
        int missed   = full - attended;
        float ratio  = (float)attended / full;

        if (slot.type == "class")
        {
            int gain = Mathf.RoundToInt(10 * ratio);
            int loss = Mathf.CeilToInt(16 * (1 - ratio));
            _state.ApplyDelta(new StatDelta { study = gain - loss, energy = -Mathf.RoundToInt(8 * ratio) },
                $"Kết thúc lớp {FormatTime(slot.start)}–{FormatTime(slot.end)}: học {attended}/{full} phút");
        }
        else if (slot.type == "work")
        {
            int base_ = Mathf.RoundToInt(full / 60f * 40_000);
            int earned = Mathf.RoundToInt(base_ * ratio);
            int deduct = Mathf.RoundToInt(40_000 * (1 - ratio));
            _state.ApplyDelta(new StatDelta { money = earned - deduct, energy = -Mathf.RoundToInt(14 * ratio) },
                $"Chốt ca {FormatTime(slot.start)}–{FormatTime(slot.end)}: {attended}/{full} phút");
        }

        LifeData.AddNotice(_state, _state.history.Count > 0 ? _state.history[^1].evt : "Slot closed");
    }

    void CloseAllSlotsForDay()
    {
        int wd = LifeData.Weekday(_state.calendarDay);
        if (!LifeData.SCHEDULE.TryGetValue(wd, out var slots)) return;
        foreach (var slot in slots) CloseSlot(slot);
    }

    // ─────────────────────────────────────────────────────────────────────────
    void CheckSurprises()
    {
        if (!string.IsNullOrEmpty(_state.life.pending)) return;
        int wd = LifeData.Weekday(_state.calendarDay);

        foreach (var ev in LifeData.SURPRISES)
        {
            if (wd == ev.day
                && _state.minutes >= ev.time
                && !_state.life.decisions.ContainsKey(ev.id))
            {
                _state.life.pending = ev.id;
                LifeData.AddNotice(_state, $"Tình huống bất ngờ: {ev.title}");
                return;
            }
        }

        // Timeout nếu không phản hồi kịp
        if (!string.IsNullOrEmpty(_state.life.pending))
        {
            var ev = LifeData.SURPRISES.Find(e => e.id == _state.life.pending);
            if (ev != null && _state.minutes > ev.chooseUntil)
            {
                _state.life.decisions[ev.id] = new DecisionRecord
                {
                    choice = "timeout", status = "failed",
                    chosenAt = LifeData.Stamp(_state)
                };
                _state.life.pending = null;
                _state.ApplyDelta(new StatDelta { trust = -8 }, $"Không phản hồi tình huống: {ev.title}");
                LifeData.AddMessage(_state, "Nhắc việc", "Bạn đã không phản hồi kịp tình huống. −8 quan hệ.");
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    void CheckNightPenalties()
    {
        string lateKey = $"{_state.calendarDay}:late";
        if (_state.minutes >= 1380 && !_state.life.sleeping
            && !_state.life.penalties.ContainsKey(lateKey))
        {
            _state.life.penalties[lateKey] = true;
            _state.ApplyDelta(new StatDelta { health = -6, energy = -5 }, "Chưa ngủ lúc 23:00");
            LifeData.AddNotice(_state, "Đã 23:00: thức khuya −6 sức khỏe, −5 năng lượng.");
        }
    }

    void PassiveEnergyDrain()
    {
        // Mỗi giờ hao 1 energy khi thức
        if (!_state.life.sleeping && Mathf.FloorToInt(_state.minutes) % 60 == 0)
            _state.ApplyDelta(new StatDelta { energy = -1 }, null);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Public helpers
    // ─────────────────────────────────────────────────────────────────────────
    public void Pause() => isRunning = false;
    public void Resume() => isRunning = true;

    public static string FormatTime(float minutes)
    {
        int h = Mathf.FloorToInt(minutes / 60f) % 24;
        int m = Mathf.FloorToInt(minutes) % 60;
        return $"{h:D2}:{m:D2}";
    }

    public static string DateLabel(GameState s)
    {
        int idx = (s.calendarDay - 1) % 7;
        return $"{LifeData.DAYS[idx]} · Ngày {s.calendarDay}";
    }
}

