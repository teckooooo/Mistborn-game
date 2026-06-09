using UnityEngine;
using TMPro;

/// <summary>
/// Muestra en pantalla el conteo de monedas y clavos del jugador.
///
/// ─── Setup en Unity ───────────────────────────────────────────────────────
/// 1. En el Canvas crear dos TextMeshPro-UI:
///    - "CoinCount"  → asignar a coinText
///    - "NailCount"  → asignar a nailText
/// 2. Adjuntar este script a cualquier GameObject del Canvas (o al HUD).
/// ─────────────────────────────────────────────────────────────────────────
/// </summary>
public class ProjectileUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI nailText;

    [Header("Inventario del jugador")]
    public PlayerInventory inventory;   // ← arrastrar el Player aquí en Inspector

    void Start()
    {
        if (inventory == null)
        {
            Debug.LogWarning("[ProjectileUI] Asigna el PlayerInventory en el Inspector.");
            return;
        }

        inventory.OnCoinsChanged += UpdateCoins;
        inventory.OnNailsChanged += UpdateNails;

        UpdateCoins(inventory.Coins);
        UpdateNails(inventory.Nails);
    }

    void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnCoinsChanged -= UpdateCoins;
            inventory.OnNailsChanged -= UpdateNails;
        }
    }

    void UpdateCoins(int count) { if (coinText != null) coinText.text = $"{count}"; }
    void UpdateNails(int count) { if (nailText != null) nailText.text = $"{count}"; }
}
