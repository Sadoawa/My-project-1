using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Main Menu: Bắt đầu mới, Tiếp tục, Giới thiệu.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("Nút chính")]
    public Button newGameButton;
    public Button continueButton;
    public Button quitButton;

    [Header("Panel giới thiệu")]
    public GameObject introPanel;
    public Button     introCloseButton;
    public Button     showIntroButton;

    [Header("Version text")]
    public TextMeshProUGUI versionText;

    // ─────────────────────────────────────────────────────────────────────────
    void Start()
    {
        bool hasSave = GameManager.Instance?.HasSave() ?? false;

        newGameButton?.onClick.AddListener(() => GameManager.Instance.StartNewGame());

        if (continueButton)
        {
            continueButton.interactable = hasSave;
            continueButton.onClick.AddListener(() => GameManager.Instance.ContinueGame());
        }

        quitButton?.onClick.AddListener(() => Application.Quit());

        showIntroButton?.onClick.AddListener(() => introPanel?.SetActive(true));
        introCloseButton?.onClick.AddListener(() => introPanel?.SetActive(false));

        if (versionText) versionText.text = "v0.14 · Unity Port";
        if (introPanel)  introPanel.SetActive(false);
    }
}

