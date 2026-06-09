using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Pantalla de muerte. Se muestra como una escena separada con overlay negro
/// y dos botones: Reiniciar (vuelve al nivel donde murió) y Menú Principal.
///
/// Cómo funciona:
///   1. PlayerHealth.Die() llama a DeathScreen.GoToDeathScreen() y guarda el
///      nombre de la escena actual en una variable estática.
///   2. Se carga la escena "Muerte" (debe estar en Build Settings).
///   3. Esta clase, en la escena Muerte, hookea los botones y al pulsar
///      Reiniciar carga la escena guardada; al pulsar Menú Principal va al
///      menú.
///
/// ─── Setup en Unity ───────────────────────────────────────────────────────
/// 1. Crear escena nueva "Muerte" y agregarla a Build Settings.
/// 2. En la escena:
///      Canvas (Screen Space Overlay)
///        ├── BackgroundOverlay  → UI Image negro, alpha 0.85, anchors a 4 esquinas
///        ├── TextoMuerte        → TextMeshPro "Has muerto" (centrado, opcional)
///        ├── BtnReiniciar       → Button con texto "Reiniciar"
///        └── BtnMenuPrincipal   → Button con texto "Menú Principal"
/// 3. Crear GameObject "DeathScreen" en la escena con este componente.
/// 4. Arrastrar BtnReiniciar y BtnMenuPrincipal a sus campos en el Inspector.
/// ─────────────────────────────────────────────────────────────────────────
/// </summary>
public class DeathScreen : MonoBehaviour
{
    [Header("Botones")]
    public Button restartButton;
    public Button mainMenuButton;

    [Header("Nombres de escena")]
    [Tooltip("Nombre exacto (sin extensión) de la escena del menú principal.")]
    public string mainMenuSceneName = "MainMenu";

    // ── Estado estático compartido entre escenas ──────────────────────────────

    /// <summary>Nombre de la escena de la pantalla de muerte (en Build Settings).</summary>
    public const string DeathSceneName = "Muerte";

    /// <summary>Escena donde el jugador murió. La usa Reiniciar.</summary>
    private static string sceneToRestart;

    /// <summary>
    /// Llamar desde PlayerHealth.Die() en lugar de recargar la escena.
    /// Guarda la escena actual y carga la pantalla de muerte.
    /// </summary>
    public static void GoToDeathScreen()
    {
        sceneToRestart = SceneManager.GetActiveScene().name;
        Time.timeScale = 1f; // garantizar que no quede pausado
        SceneManager.LoadScene(DeathSceneName);
    }

    // ── Comportamiento en la escena Muerte ────────────────────────────────────

    void Start()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(Restart);
        else
            Debug.LogWarning("[DeathScreen] restartButton sin asignar.");

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        else
            Debug.LogWarning("[DeathScreen] mainMenuButton sin asignar.");
    }

    void Restart()
    {
        if (string.IsNullOrEmpty(sceneToRestart))
        {
            Debug.LogWarning("[DeathScreen] No hay escena previa guardada — yendo al menú.");
            GoToMainMenu();
            return;
        }
        SceneManager.LoadScene(sceneToRestart);
    }

    void GoToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
