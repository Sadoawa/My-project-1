using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// UI điện thoại: tin nhắn và lịch tuần.
/// Mở/đóng bằng phím Tab.
/// </summary>
public class PhoneUI : MonoBehaviour
{
    public static PhoneUI Instance { get; private set; }

    [Header("Panel")]
    public GameObject phonePanel;
    public Button     tabMessages;
    public Button     tabSchedule;

    [Header("Tab Tin nhắn")]
    public Transform          messageContainer;
    public GameObject         messageItemPrefab;    // Prefab: avatar + from + text
    public TextMeshProUGUI    unreadBadge;

    [Header("Tab Lịch tuần")]
    public Transform          scheduleContainer;
    public GameObject         scheduleRowPrefab;

    [Header("Phím tắt")]
    public KeyCode toggleKey = KeyCode.Tab;

    // Nội bộ
    private bool _open = false;
    private enum PhoneTab { Messages, Schedule }
    private PhoneTab _currentTab = PhoneTab.Messages;

    private GameState _s => GameManager.Instance?.State;

    // ─────────────────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (phonePanel) phonePanel.SetActive(false);
        tabMessages?.onClick.AddListener(() => SwitchTab(PhoneTab.Messages));
        tabSchedule?.onClick.AddListener(() => SwitchTab(PhoneTab.Schedule));
    }

    void Update()
    {
        bool togglePressed = false;
#if ENABLE_INPUT_SYSTEM
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb != null)
        {
            togglePressed = kb.tabKey.wasPressedThisFrame || kb.pKey.wasPressedThisFrame;
        }
#endif
        try
        {
            if (!togglePressed)
                togglePressed = Input.GetKeyDown(toggleKey) || Input.GetKeyDown(KeyCode.P);
        }
        catch {}

        if (togglePressed)
            Toggle();

        // Huy hiệu tin chưa đọc
        if (unreadBadge && _s != null)
        {
            int unread = _s.life.messages.FindAll(m => !m.read).Count;
            unreadBadge.text = unread > 0 ? unread.ToString() : "";
            unreadBadge.gameObject.SetActive(unread > 0);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    public void Toggle()
    {
        _open = !_open;
        phonePanel?.SetActive(_open);
        if (_open)
        {
            SwitchTab(_currentTab);
            TimeManager.Instance?.Pause();
        }
        else
        {
            TimeManager.Instance?.Resume();
        }
    }

    void SwitchTab(PhoneTab tab)
    {
        _currentTab = tab;
        if (tab == PhoneTab.Messages) RenderMessages();
        else RenderSchedule();
    }

    // ─────────────────────────────────────────────────────────────────────────
    void RenderMessages()
    {
        if (_s == null || messageContainer == null) return;
        ClearChildren(messageContainer);

        // Đánh dấu đã đọc
        _s.phoneRead = true;
        foreach (var m in _s.life.messages) m.read = true;

        var msgs = new List<GameMessage>(_s.life.messages);
        msgs.Reverse(); // Mới nhất lên đầu

        foreach (var msg in msgs)
        {
            if (messageItemPrefab == null) break;
            var item = Instantiate(messageItemPrefab, messageContainer);
            var texts = item.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 2)
            {
                texts[0].text = msg.from;
                texts[1].text = msg.text;
            }

            // Nút reply nếu là invite
            if (msg.invite && msg.reply == null)
            {
                var btns = item.GetComponentsInChildren<Button>();
                if (btns.Length >= 2)
                {
                    GameMessage m2 = msg;  // closure
                    btns[0].onClick.AddListener(() => ReplyInvite(m2, "yes"));
                    btns[1].onClick.AddListener(() => ReplyInvite(m2, "no"));
                }
            }
        }
    }

    void RenderSchedule()
    {
        if (_s == null || scheduleContainer == null) return;
        ClearChildren(scheduleContainer);

        int wd = LifeData.Weekday(_s.calendarDay);
        if (!LifeData.SCHEDULE.TryGetValue(wd, out var slots)) return;

        foreach (var slot in slots)
        {
            if (scheduleRowPrefab == null) break;
            var row = Instantiate(scheduleRowPrefab, scheduleContainer);
            var txts = row.GetComponentsInChildren<TextMeshProUGUI>();
            if (txts.Length >= 2)
            {
                txts[0].text = $"{TimeManager.FormatTime(slot.start)} – {TimeManager.FormatTime(slot.end)}";
                txts[1].text = slot.type == "class" ? "Lớp B.204" : "Ca làm Chạm Coffee";
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    void ReplyInvite(GameMessage msg, string reply)
    {
        msg.reply = reply;
        if (reply == "yes")
            LifeData.AddMessage(_s, "Linh", $"Ok, mình đợi bạn tại {msg.address}. Tới nơi bấm Gặp bạn nhé.", msg.location);
        else
            _s.ApplyDelta(new StatDelta { trust = -2 }, "Từ chối lời mời của Linh");

        RenderMessages();
    }

    // ─────────────────────────────────────────────────────────────────────────
    static void ClearChildren(Transform t)
    {
        for (int i = t.childCount - 1; i >= 0; i--)
            Destroy(t.GetChild(i).gameObject);
    }
}

