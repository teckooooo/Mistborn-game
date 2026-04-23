using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI de salud del jugador. Solo muestra — no tiene lógica de juego.
/// Conectar en el Inspector: asignar el PlayerHealth del jugador
/// y los elementos de UI que quieras usar.
///
/// En fase de desarrollo: usa una Slider simple de Unity.
/// En fase de estética: reemplaza o extiende este script sin tocar PlayerHealth.
/// </summary>
public class PlayerHealthUI : MonoBehaviour
{
    [Header("Referencia al jugador")]
    public PlayerHealth playerHealth;

    [Header("UI — asignar en Inspector")]
    [Tooltip("Slider de Unity para mostrar la barra de vida. Suficiente para desarrollo.")]
    public Slider healthSlider;

    [Tooltip("(Opcional) Texto que muestra HP numérico, ej: '80 / 100'")]
    public Text healthText;

    // ── Inicialización ────────────────────────────────────────────────────────

    void Start()
    {
        if (playerHealth == null)
        {
            Debug.LogWarning("[PlayerHealthUI] No hay PlayerHealth asignado.");
            return;
        }

        // Suscribirse al evento de cambio de salud
        playerHealth.OnHealthChanged.AddListener(UpdateUI);

        // Inicializar con valores actuales
        UpdateUI(playerHealth.CurrentHealth, playerHealth.MaxHealth);
    }

    void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged.RemoveListener(UpdateUI);
    }

    // ── Actualizar UI ─────────────────────────────────────────────────────────

    void UpdateUI(float current, float max)
    {
        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = max;
            healthSlider.value    = current;
        }

        if (healthText != null)
            healthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
    }
}