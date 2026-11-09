using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// UI hội thoại và lựa chọn.
/// Hiện hộp thoại với tối đa 3 lựa chọn, trả về index khi chọn.
/// </summary>
public class DialogueUI : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────────
    public static DialogueUI Instance { get; private set; }

    [Header("Panel")]
    public GameObject panel;

    [Header("Nội dung")]
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI quoteText;
    public TextMeshProUGUI titleText;

    [Header("Nút lựa chọn (tối đa 3)")]
    public Button[]          choiceButtons;
    public TextMeshProUGUI[] choiceLabels;
    public TextMeshProUGUI[] choiceDescs;

    [Header("Receipt / kết quả")]
    public GameObject         receiptPanel;
    public TextMeshProUGUI    receiptText;
    public TextMeshProUGUI    lessonText;
    public Button             receiptCloseButton;

    // Nội bộ
    private System.Action<int> _onChoice;
    private bool _isOpen = false;
    public bool IsOpen => _isOpen;

    // ─────────────────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (panel) panel.SetActive(false);
        if (receiptPanel) receiptPanel.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────────────
    /// <summary>Mở hộp thoại với danh sách lựa chọn.</summary>
    public void Show(DialogueData data, System.Action<int> onChoice)
    {
        _onChoice = onChoice;
        _isOpen   = true;

        if (speakerText) speakerText.text = data.speaker;
        if (quoteText)   quoteText.text   = data.quote;
        if (titleText)   titleText.text   = data.title;

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < data.options.Count)
            {
                choiceButtons[i].gameObject.SetActive(true);
                if (choiceLabels.Length > i) choiceLabels[i].text = data.options[i].label;
                if (choiceDescs.Length  > i) choiceDescs[i].text  = data.options[i].description;

                int idx = i;  // closure capture
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(idx));
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }

        if (panel) panel.SetActive(true);
        TimeManager.Instance?.Pause();
    }

    // ─────────────────────────────────────────────────────────────────────────
    void OnChoiceSelected(int idx)
    {
        _onChoice?.Invoke(idx);
        _isOpen = false;
        if (panel) panel.SetActive(false);
        TimeManager.Instance?.Resume();
    }

    // ─────────────────────────────────────────────────────────────────────────
    /// <summary>Hiện kết quả sau khi chọn (receipt + lesson).</summary>
    public void ShowReceipt(string receipt, string lesson, System.Action onClose = null)
    {
        if (receiptPanel == null) return;
        if (receiptText) receiptText.text = receipt;
        if (lessonText)  lessonText.text  = lesson;
        receiptPanel.SetActive(true);

        if (receiptCloseButton)
        {
            receiptCloseButton.onClick.RemoveAllListeners();
            receiptCloseButton.onClick.AddListener(() =>
            {
                receiptPanel.SetActive(false);
                onClose?.Invoke();
                TimeManager.Instance?.Resume();
            });
        }
    }

    public void Close()
    {
        _isOpen = false;
        if (panel)        panel.SetActive(false);
        if (receiptPanel) receiptPanel.SetActive(false);
        TimeManager.Instance?.Resume();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Data classes cho dialogue
// ─────────────────────────────────────────────────────────────────────────────

public class DialogueData
{
    public string              title;
    public string              speaker;
    public string              quote;
    public List<DialogueOption> options = new List<DialogueOption>();
}

public class DialogueOption
{
    public string label;
    public string description;
}

