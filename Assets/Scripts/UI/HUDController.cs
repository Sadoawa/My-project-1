using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// HUD chính: thanh chỉ số, đồng hồ, ngày, thông báo.
/// Gắn vào Canvas GameObject trong scene GameWorld.
/// </summary>
public class HUDController : MonoBehaviour
{
    [Header("Thanh chỉ số")]
    public Slider moneyBar;
    public Slider energyBar;
    public Slider studyBar;
    public Slider trustBar;
    public Slider healthBar;

    [Header("Text số liệu")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI studyText;
    public TextMeshProUGUI trustText;
    public TextMeshProUGUI healthText;

    [Header("Đồng hồ & Ngày")]
    public TextMeshProUGUI clockText;
    public TextMeshProUGUI dayText;

    [Header("Mục tiêu hiện tại")]
    public TextMeshProUGUI goalTitleText;
    public TextMeshProUGUI goalDescText;
    public Button          goalButton;
    public TextMeshProUGUI goalButtonText;

    [Header("Tương tác (gần điểm F)")]
    public GameObject      interactPrompt;
    public TextMeshProUGUI interactLabel;
    public TextMeshProUGUI interactSub;

    [Header("Thông báo nhỏ")]
    public TextMeshProUGUI noticeText;
    public float           noticeDuration = 3f;

    [Header("Xe máy HUD")]
    public GameObject bikePanel;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI bikeCondText;

    [Header("Chỉ số tiền tối đa hiển thị")]
    public int maxMoneyDisplay = 5_000_000;

    // Nội bộ
    private float   _noticeTimer;
    private GameState _s => GameManager.Instance?.State;

    // ─────────────────────────────────────────────────────────────────────────
    void Start()
    {
        // Đăng ký lắng nghe khi điểm tương tác được kích hoạt
        InteractionZone.OnActivated += OnInteractionZoneEntered;
    }

    void OnDestroy()
    {
        InteractionZone.OnActivated -= OnInteractionZoneEntered;
    }

    // ─────────────────────────────────────────────────────────────────────────
    void Update()
    {
        if (_s == null) return;
        RefreshStats();
        RefreshClock();
        RefreshBikeHUD();
        HandleNoticeTimer();
    }

    // ─────────────────────────────────────────────────────────────────────────
    void RefreshStats()
    {
        // Tiền (không scale 0–100, hiện số trực tiếp)
        if (moneyBar)  moneyBar.value  = Mathf.Clamp01((float)_s.money / maxMoneyDisplay);
        if (moneyText) moneyText.text  = FormatMoney(_s.money);

        if (energyBar)  energyBar.value  = _s.energy / 100f;
        if (energyText) energyText.text  = $"{_s.energy}";

        if (studyBar)  studyBar.value  = _s.study / 100f;
        if (studyText) studyText.text  = $"{_s.study}";

        if (trustBar)  trustBar.value  = _s.trust / 100f;
        if (trustText) trustText.text  = $"{_s.trust}";

        if (healthBar)  healthBar.value  = _s.health / 100f;
        if (healthText) healthText.text  = $"{_s.health}";
    }

    void RefreshClock()
    {
        if (clockText) clockText.text = TimeManager.FormatTime(_s.minutes);
        if (dayText)   dayText.text   = TimeManager.DateLabel(_s);
    }

    void RefreshBikeHUD()
    {
        if (bikePanel == null) return;
        bool onBike = PlayerController.Instance?.IsOnBike ?? false;
        bikePanel.SetActive(onBike);
        if (onBike && speedText && bikeCondText)
        {
            speedText.text   = $"{PlayerController.Instance.BikeSpeed * 3.6f:F0} km/h";
            bikeCondText.text = $"Xe: {_s.bikeCondition}%";
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    public void ShowNotice(string text)
    {
        if (noticeText == null) return;
        noticeText.text    = text;
        noticeText.gameObject.SetActive(true);
        _noticeTimer = noticeDuration;
    }

    void HandleNoticeTimer()
    {
        if (_noticeTimer > 0)
        {
            _noticeTimer -= Time.deltaTime;
            if (_noticeTimer <= 0 && noticeText)
                noticeText.gameObject.SetActive(false);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    void OnInteractionZoneEntered(InteractionZone zone)
    {
        // Hiện prompt tương tác
        if (interactPrompt == null) return;
        interactPrompt.SetActive(true);
        if (interactLabel) interactLabel.text = zone.label;
        if (interactSub)   interactSub.text   = zone.subLabel;
    }

    public void HideInteractPrompt()
    {
        if (interactPrompt) interactPrompt.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────────────
    public void SetGoal(string title, string desc, string btnLabel)
    {
        if (goalTitleText) goalTitleText.text = title;
        if (goalDescText)  goalDescText.text  = desc;
        if (goalButtonText) goalButtonText.text = btnLabel;
    }

    // ─────────────────────────────────────────────────────────────────────────
    static string FormatMoney(int amount)
    {
        if (amount >= 1_000_000)
            return $"{amount / 1_000_000f:F1}M đ";
        if (amount >= 1_000)
            return $"{amount / 1_000}K đ";
        return $"{amount} đ";
    }
}

