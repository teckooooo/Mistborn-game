using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Menú de pausa. Presionar ESC pausa/reanuda el juego.
///
/// ─── Setup en Unity ───────────────────────────────────────────────────────
/// 1. Crear un Canvas "PauseCanvas" (Screen Space - Overlay, Sort Order alto, ej. 10).
/// 2. Dentro crear tres paneles:
///      pausePanel    → botones: Reanudar | Cómo Jugar | Opciones | Menú Principal
///      comoJugarPanel → texto explicativo de controles
///      opcionesPanel  → placeholder "Próximamente"
/// 3. Arrastrar los paneles y botones a los campos de este script.
/// 4. Adjuntar este script a un GameObject vacío "PauseMenu" en la escena.
/// ─────────────────────────────────────────────────────────────────────────
/// </summary>
public class PauseMenu : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    [Header("Canvas raíz del menú de pausa")]
    [Tooltip("El Canvas entero que agrupa todos los paneles de pausa")]
    public GameObject pauseCanvas;

    [Header("Paneles")]
    public GameObject pausePanel;
    public GameObject comoJugarPanel;
    public GameObject opcionesPanel;

    [Header("Botones del panel principal")]
    public Button resumeButton;
    public Button comoJugarButton;
    public Button opcionesButton;
    public Button mainMenuButton;

    [Header("Botones Volver (en sub-paneles)")]
    public Button backFromComoJugar;
    public Button backFromOpciones;

    // ── Unity ─────────────────────────────────────────────────────────────────

    void Awake()
    {
        // Asegurarse de que empieza oculto y no pausado
        IsPaused = false;
        Time.timeScale = 1f;
        SetCanvasVisible(false);
    }

    void Start()
    {
        // Botón Reanudar
        if (resumeButton   != null) resumeButton  .onClick.AddListener(Resume);

        // Botón Cómo Jugar
        if (comoJugarButton != null) comoJugarButton.onClick.AddListener(ShowComoJugar);

        // Botón Opciones
        if (opcionesButton  != null) opcionesButton .onClick.AddListener(ShowOpciones);

        // Botón Menú Principal
        if (mainMenuButton  != null) mainMenuButton .onClick.AddListener(GoToMainMenu);

        // Botones Volver de sub-paneles
        if (backFromComoJugar != null) backFromComoJugar.onClick.AddListener(ShowPauseMain);
        if (backFromOpciones  != null) backFromOpciones .onClick.AddListener(ShowPauseMain);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPaused) Resume();
            else          Pause();
        }
    }

    void OnDestroy()
    {
        // Siempre restaurar timeScale al destruir (cambio de escena, etc.)
        Time.timeScale = 1f;
        IsPaused = false;
    }

    // ── Pausa / Reanuda ───────────────────────────────────────────────────────

    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;
        SetCanvasVisible(true);
        ShowPauseMain();
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        SetCanvasVisible(false);
    }

    // ── Navegación interna ────────────────────────────────────────────────────

    void ShowPauseMain()
    {
        SetPanel(pausePanel,     true);
        SetPanel(comoJugarPanel, false);
        SetPanel(opcionesPanel,  false);
    }

    void ShowComoJugar()
    {
        SetPanel(pausePanel,     false);
        SetPanel(comoJugarPanel, true);
        SetPanel(opcionesPanel,  false);
    }

    void ShowOpciones()
    {
        SetPanel(pausePanel,     false);
        SetPanel(comoJugarPanel, false);
        SetPanel(opcionesPanel,  true);
    }

    void GoToMainMenu()
    {
        // Restaurar tiempo antes de cambiar escena
        Time.timeScale = 1f;
        IsPaused = false;
        LevelManager.LoadMainMenu();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    void SetCanvasVisible(bool visible)
    {
        if (pauseCanvas != null) pauseCanvas.SetActive(visible);
    }

    void SetPanel(GameObject panel, bool active)
    {
        if (panel != null) panel.SetActive(active);
    }
}
