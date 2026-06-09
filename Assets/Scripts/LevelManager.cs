using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestiona el progreso de niveles. Usa PlayerPrefs para persistir entre sesiones.
/// Niveles numerados del 1 al 9. El nivel 1 siempre está desbloqueado.
///
/// Índices de escena en Build Profiles:
///   0 = MainMenu
///   1 = LevelSelect
///   2 = Level1  ← levelNumber + 1
///   3 = Level2
///   ...
///   10 = Level9
/// </summary>
public static class LevelManager
{
    public const int TotalLevels    = 9;
    private const string PrefKey    = "MaxLevelUnlocked";
    private const int   LevelOffset = 2; // escena 2 = nivel 1

    // ── Consultas ─────────────────────────────────────────────────────────────

    /// <summary>Número máximo de nivel desbloqueado (mínimo 1).</summary>
    public static int MaxUnlocked => Mathf.Max(1, PlayerPrefs.GetInt(PrefKey, 1));

    /// <summary>True si el nivel puede jugarse.</summary>
    public static bool IsUnlocked(int levelNumber) => levelNumber <= MaxUnlocked;

    // ── Progreso ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Llamar al completar un nivel. Desbloquea el siguiente y guarda.
    /// </summary>
    public static void CompleteLevel(int levelNumber)
    {
        int next = levelNumber + 1;
        if (next > MaxUnlocked && next <= TotalLevels)
        {
            PlayerPrefs.SetInt(PrefKey, next);
            PlayerPrefs.Save();
            Debug.Log($"[LevelManager] Nivel {next} desbloqueado.");
        }
    }

    // ── Navegación ────────────────────────────────────────────────────────────

    public static void LoadLevel(int levelNumber)
    {
        if (!IsUnlocked(levelNumber)) return;
        SceneManager.LoadScene(levelNumber + LevelOffset - 1);
    }

    public static void LoadMainMenu()    => SceneManager.LoadScene(0);
    public static void LoadLevelSelect() => SceneManager.LoadScene(1);

    /// <summary>Borrar todo el progreso (útil para debug).</summary>
    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(PrefKey);
        PlayerPrefs.Save();
        Debug.Log("[LevelManager] Progreso borrado.");
    }
}
