using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controla el menú principal.
///
/// ─── Escenas necesarias (agregar en File > Build Settings) ───────────────
///   0 - MainMenu   (esta escena)
///   1 - LevelSelect (selector de niveles)
///   2 - SampleScene (o el nombre de tu escena de juego)
/// ─────────────────────────────────────────────────────────────────────────
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject mainPanel;
    public GameObject comoJugarPanel;
    public GameObject opcionesPanel;

    [Header("Nombres de escenas")]
    public string levelSelectScene = "LevelSelect";

    void Start()
    {
        ShowMain();
    }

    // ── Navegación entre paneles ──────────────────────────────────────────────

    public void ShowMain()
    {
        mainPanel      .SetActive(true);
        comoJugarPanel .SetActive(false);
        if (opcionesPanel != null) opcionesPanel.SetActive(false);
    }

    public void ShowComoJugar()
    {
        mainPanel      .SetActive(false);
        comoJugarPanel .SetActive(true);
    }

    public void ShowOpciones()
    {
        mainPanel      .SetActive(false);
        if (opcionesPanel != null) opcionesPanel.SetActive(true);
    }

    // ── Botones ───────────────────────────────────────────────────────────────

    public void OnInicio()
    {
        SceneManager.LoadScene(levelSelectScene);
    }

    public void OnComoJugar()
    {
        ShowComoJugar();
    }

    public void OnOpciones()
    {
        ShowOpciones();
    }

    public void OnVolver()
    {
        ShowMain();
    }

    public void OnSalir()
    {
        Debug.Log("[MainMenu] Saliendo del juego.");
        Application.Quit();
    }
}
