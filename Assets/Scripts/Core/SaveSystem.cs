using UnityEngine;
using System.IO;

/// <summary>
/// Lưu và load GameState dạng JSON xuống file.
/// Trên PC: Application.persistentDataPath/savegame.json
/// </summary>
public static class SaveSystem
{
    private static readonly string FileName = "savegame.json";
    private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

    public static void Save(GameState state)
    {
        try
        {
            string json = JsonUtility.ToJson(state, prettyPrint: true);
            File.WriteAllText(FilePath, json);
            Debug.Log($"[SaveSystem] Saved to {FilePath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SaveSystem] Save failed: {ex.Message}");
        }
    }

    public static GameState Load()
    {
        if (!File.Exists(FilePath)) return null;
        try
        {
            string json = File.ReadAllText(FilePath);
            return JsonUtility.FromJson<GameState>(json);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SaveSystem] Load failed: {ex.Message}");
            return null;
        }
    }

    public static bool HasSave() => File.Exists(FilePath);

    public static void Delete()
    {
        if (File.Exists(FilePath))
            File.Delete(FilePath);
    }
}

