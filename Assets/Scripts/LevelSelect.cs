using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Selector de niveles. Gestiona 9 botones: desbloqueados = interactuables,
/// bloqueados = grises con candado.
///
/// ─── Setup en Unity ───────────────────────────────────────────────────────
/// 1. Crear 9 Buttons en el Canvas y asignarlos al array levelButtons.
/// 2. En cada botón, el texto hijo debe llamarse "Label".
/// 3. Asignar el botón Volver al campo backButton.
/// ─────────────────────────────────────────────────────────────────────────
/// </summary>
public class LevelSelect : MonoBehaviour
{
    [Header("Botones de nivel (asignar en orden 1-9)")]
    public Button[] levelButtons = new Button[9];

    [Header("Colores")]
    public Color unlockedColor = Color.white;
    public Color lockedColor   = new Color(0.4f, 0.4f, 0.4f, 1f);

    [Header("Texto de nivel bloqueado")]
    public string lockedLabel = "🔒";

    [Header("Botón volver")]
    public Button backButton;

    void Start()
    {
        RefreshButtons();

        if (backButton != null)
            backButton.onClick.AddListener(LevelManager.LoadMainMenu);
    }

    void RefreshButtons()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelNumber = i + 1; // botón 0 = nivel 1
            Button btn = levelButtons[i];
            if (btn == null) continue;

            bool unlocked = LevelManager.IsUnlocked(levelNumber);

            // Interactuabilidad
            btn.interactable = unlocked;

            // Color del botón
            ColorBlock cb = btn.colors;
            cb.normalColor   = unlocked ? unlockedColor : lockedColor;
            cb.disabledColor = lockedColor;
            btn.colors = cb;

            // Texto
            TextMeshProUGUI label = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = unlocked ? $"Nivel {levelNumber}" : lockedLabel;

            // Click — capturar levelNumber en closure
            int lvl = levelNumber;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => LevelManager.LoadLevel(lvl));
        }
    }

    // Llamar desde el editor para resetear progreso en debug
    [ContextMenu("Resetear progreso (Debug)")]
    void DebugReset()
    {
        LevelManager.ResetProgress();
        RefreshButtons();
    }
}
