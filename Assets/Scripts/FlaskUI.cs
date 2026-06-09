using UnityEngine;
using TMPro;

/// <summary>
/// Muestra en pantalla la cantidad de frascos de metal del jugador.
///
/// ─── Setup en Unity ───────────────────────────────────────────────────────
/// 1. En el Canvas crear cuatro TextMeshPro-UI y asignarlos aquí.
/// 2. Arrastrar el Player al campo Inventory.
/// ─────────────────────────────────────────────────────────────────────────
/// </summary>
public class FlaskUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public TextMeshProUGUI steelText;
    public TextMeshProUGUI ironText;
    public TextMeshProUGUI pewterText;
    public TextMeshProUGUI duraluminText;

    [Header("Inventario del jugador")]
    public PlayerInventory inventory;

    void Start()
    {
        if (inventory == null)
        {
            Debug.LogWarning("[FlaskUI] Asigna PlayerInventory en el Inspector.");
            return;
        }

        inventory.OnSteelFlasksChanged     += UpdateSteel;
        inventory.OnIronFlasksChanged      += UpdateIron;
        inventory.OnPewterFlasksChanged    += UpdatePewter;
        inventory.OnDuraluminFlasksChanged += UpdateDuralumin;

        // Valores iniciales
        UpdateSteel(inventory.SteelFlasks);
        UpdateIron(inventory.IronFlasks);
        UpdatePewter(inventory.PewterFlasks);
        UpdateDuralumin(inventory.DuraluminFlasks);
    }

    void OnDestroy()
    {
        if (inventory == null) return;
        inventory.OnSteelFlasksChanged     -= UpdateSteel;
        inventory.OnIronFlasksChanged      -= UpdateIron;
        inventory.OnPewterFlasksChanged    -= UpdatePewter;
        inventory.OnDuraluminFlasksChanged -= UpdateDuralumin;
    }

    void UpdateSteel(int n)     { if (steelText     != null) steelText.text     = $"[1] Acero ×{n}";   }
    void UpdateIron(int n)      { if (ironText      != null) ironText.text      = $"[2] Hierro ×{n}";  }
    void UpdatePewter(int n)    { if (pewterText    != null) pewterText.text    = $"[3] Estaño ×{n}";  }
    void UpdateDuralumin(int n) { if (duraluminText != null) duraluminText.text = $"[4] Dural. ×{n}";  }
}
