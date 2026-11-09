using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Màn hình tổng kết cuối tuần — port từ màn Summary trong web game.
/// </summary>
public class SummaryUI : MonoBehaviour
{
    [Header("Chỉ số cuối")]
    public TextMeshProUGUI moneyFinal;
    public TextMeshProUGUI energyFinal;
    public TextMeshProUGUI studyFinal;
    public TextMeshProUGUI trustFinal;
    public TextMeshProUGUI healthFinal;

    [Header("Lịch sử quyết định")]
    public Transform historyContainer;
    public GameObject historyRowPrefab;

    [Header("Hậu quả")]
    public TextMeshProUGUI consequenceText;

    [Header("Kết luận")]
    public TextMeshProUGUI lessonText;
    public TextMeshProUGUI gameOverText;

    [Header("Nút")]
    public Button playAgainButton;
    public Button mainMenuButton;

    // ─────────────────────────────────────────────────────────────────────────
    void Start()
    {
        var s = GameManager.Instance?.State;
        if (s == null) return;

        // Áp dụng hậu quả nếu chưa
        StoryData.ResolveConsequences(s);

        // Chỉ số
        if (moneyFinal)  moneyFinal.text  = $"{s.money:N0} đ";
        if (energyFinal) energyFinal.text = $"{s.energy}/100";
        if (studyFinal)  studyFinal.text  = $"{s.study}/100";
        if (trustFinal)  trustFinal.text  = $"{s.trust}/100";
        if (healthFinal) healthFinal.text = $"{s.health}/100";

        // Hậu quả lựa chọn chiều ngày 8
        if (!string.IsNullOrEmpty(s.choice) && StoryData.CHOICES.TryGetValue(s.choice, out var c))
        {
            if (consequenceText) consequenceText.text = c.consequence;
            if (lessonText)      lessonText.text      = c.lesson;
        }

        // Game over
        string goReason = s.GameOverReason();
        if (gameOverText)
        {
            gameOverText.text = !string.IsNullOrEmpty(goReason)
                ? $"Game Over: {goReason}"
                : "Bạn đã hoàn thành chương 1!";
        }

        // Lịch sử
        if (historyContainer != null && historyRowPrefab != null)
        {
            foreach (var entry in s.history)
            {
                var row = Instantiate(historyRowPrefab, historyContainer);
                var txts = row.GetComponentsInChildren<TextMeshProUGUI>();
                if (txts.Length >= 2)
                {
                    txts[0].text = $"Ngày {entry.day} – {entry.evt}";
                    txts[1].text = FormatDelta(entry.delta);
                }
            }
        }

        // Nút
        playAgainButton?.onClick.AddListener(() =>
        {
            GameManager.Instance.DeleteSave();
            GameManager.Instance.StartNewGame();
        });
        mainMenuButton?.onClick.AddListener(() => GameManager.Instance.GoToMainMenu());
    }

    static string FormatDelta(StatDelta d)
    {
        var parts = new System.Collections.Generic.List<string>();
        if (d.money  != 0) parts.Add((d.money  > 0 ? "+" : "") + $"{d.money:N0} đ");
        if (d.energy != 0) parts.Add((d.energy > 0 ? "+" : "") + $"{d.energy} NL");
        if (d.study  != 0) parts.Add((d.study  > 0 ? "+" : "") + $"{d.study} HT");
        if (d.trust  != 0) parts.Add((d.trust  > 0 ? "+" : "") + $"{d.trust} QH");
        if (d.health != 0) parts.Add((d.health > 0 ? "+" : "") + $"{d.health} SK");
        return parts.Count > 0 ? string.Join(" · ", parts) : "–";
    }
}

