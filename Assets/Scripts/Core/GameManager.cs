using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Điều phối luồng chính của game: khởi tạo, chuyển cảnh, game over.
/// </summary>
public class GameManager : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────────
    public static GameManager Instance { get; private set; }

    // ── State ─────────────────────────────────────────────────────────────────
    public GameState State { get; private set; }

    // ── Scene names ───────────────────────────────────────────────────────────
    [Header("Tên Scene")]
    public string mainMenuScene  = "MainMenu";
    public string gameWorldScene = "GameWorld";
    public string summaryScene   = "Summary";

    // ─────────────────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Load save hoặc tạo mới
        State = SaveSystem.Load() ?? StoryData.CreateInitialState();
    }

    // ─────────────────────────────────────────────────────────────────────────
    public void StartNewGame(bool fullLife = false)
    {
        if (fullLife)
            LifeData.InitLife(State);
        else
            State = StoryData.CreateInitialState();

        SaveSystem.Save(State);
        SceneManager.LoadScene(gameWorldScene);
    }

    public void ContinueGame()
    {
        SceneManager.LoadScene(gameWorldScene);
    }

    public void GoToMainMenu()
    {
        TimeManager.Instance?.Pause();
        SceneManager.LoadScene(mainMenuScene);
    }

    public void GoToSummary()
    {
        TimeManager.Instance?.Pause();
        StoryData.ResolveConsequences(State);
        SaveSystem.Save(State);
        SceneManager.LoadScene(summaryScene);
    }

    // ─────────────────────────────────────────────────────────────────────────
    void Update()
    {
        if (State == null) return;
        string reason = State.GameOverReason();
        if (!string.IsNullOrEmpty(reason))
        {
            HandleGameOver(reason);
        }
    }

    void HandleGameOver(string reason)
    {
        TimeManager.Instance?.Pause();
        // Hiển thị qua HUD hoặc chuyển cảnh summary
        Debug.Log($"[GameOver] {reason}");
        GoToSummary();
    }

    // ─────────────────────────────────────────────────────────────────────────
    public void SaveGame() => SaveSystem.Save(State);

    public bool HasSave() => SaveSystem.HasSave();

    public void DeleteSave() => SaveSystem.Delete();
}

