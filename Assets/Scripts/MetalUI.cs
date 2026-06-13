using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Muestra en pantalla las reservas de Acero y Hierro (barras) y el conteo de frascos.
/// </summary>
public class MetalUI : MonoBehaviour
{
    [System.Serializable]
    public struct MetalBarEntry
    {
        [Tooltip("Slider que representa el nivel de reserva")]
        public Slider bar;
        [Tooltip("(Opcional) Texto con el conteo de frascos")]
        public TextMeshProUGUI flaskText;
    }

    [Header("Acero  — Q")]
    public MetalBarEntry steel;

    [Header("Hierro — Click derecho")]
    public MetalBarEntry iron;

    [Header("Jugador")]
    public PlayerInventory inventory;   // ← arrastra el Player aquí

    private MetalReserve reserve;

    private UnityEngine.Events.UnityAction<float,float> onSteelRes, onIronRes;

    void Start()
    {
        if (inventory == null)
        {
            Debug.LogWarning("[MetalUI] Asigna el Player (PlayerInventory) en el Inspector.");
            return;
        }

        reserve = inventory.GetComponent<MetalReserve>();
        if (reserve == null)
        {
            Debug.LogWarning("[MetalUI] MetalReserve no encontrado en el Player.");
            return;
        }

        onSteelRes = (cur, max) => SetBar(steel, cur, max);
        onIronRes  = (cur, max) => SetBar(iron,  cur, max);

        reserve.OnSteelChanged.AddListener(onSteelRes);
        reserve.OnIronChanged .AddListener(onIronRes);

        inventory.OnSteelFlasksChanged += n => SetFlaskText(steel, n);
        inventory.OnIronFlasksChanged  += n => SetFlaskText(iron,  n);

        SetBar(steel, reserve.CurrentSteel, reserve.maxSteel);
        SetBar(iron,  reserve.CurrentIron,  reserve.maxIron);

        SetFlaskText(steel, inventory.SteelFlasks);
        SetFlaskText(iron,  inventory.IronFlasks);
    }

    void OnDestroy()
    {
        if (reserve != null)
        {
            reserve.OnSteelChanged.RemoveListener(onSteelRes);
            reserve.OnIronChanged .RemoveListener(onIronRes);
        }
    }

    void SetBar(MetalBarEntry entry, float current, float max)
    {
        if (entry.bar == null) return;
        entry.bar.minValue = 0f;
        entry.bar.maxValue = max;
        entry.bar.value    = current;
    }

    void SetFlaskText(MetalBarEntry entry, int count)
    {
        if (entry.flaskText == null) return;
        entry.flaskText.text = count > 0 ? $"{count}" : "";
    }
}
