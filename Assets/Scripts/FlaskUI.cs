using UnityEngine;
using TMPro;

/// <summary>
/// Muestra en pantalla la cantidad de frascos de Acero y Hierro.
/// </summary>
public class FlaskUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public TextMeshProUGUI steelText;
    public TextMeshProUGUI ironText;

    [Header("Inventario del jugador")]
    public PlayerInventory inventory;

    void Start()
    {
        if (inventory == null)
        {
            Debug.LogWarning("[FlaskUI] Asigna PlayerInventory en el Inspector.");
            return;
        }

        inventory.OnSteelFlasksChanged += UpdateSteel;
        inventory.OnIronFlasksChanged  += UpdateIron;

        UpdateSteel(inventory.SteelFlasks);
        UpdateIron(inventory.IronFlasks);
    }

    void OnDestroy()
    {
        if (inventory == null) return;
        inventory.OnSteelFlasksChanged -= UpdateSteel;
        inventory.OnIronFlasksChanged  -= UpdateIron;
    }

    void UpdateSteel(int n) { if (steelText != null) steelText.text = $"[1] Acero ×{n}"; }
    void UpdateIron(int n)  { if (ironText  != null) ironText.text  = $"[2] Hierro ×{n}"; }
}
