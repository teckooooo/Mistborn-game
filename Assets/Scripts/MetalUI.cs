using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Muestra en pantalla las reservas de metal (barras) y el conteo de frascos.
///
/// ─── Setup en Unity ───────────────────────────────────────────────────────
/// 1. Crear un GameObject "MetalUI" en el Canvas.
/// 2. Por cada metal, crear una Slider y (opcional) un TextMeshPro para frascos.
/// 3. Arrastrar el Player al campo Inventory.
///    MetalReserve se encuentra automáticamente desde el Player.
///
/// ─── Configurar cada Slider ───────────────────────────────────────────────
///   Min Value = 0 | Max Value = 100 | Interactable = OFF
///   Fill Area > Fill: cambiar color según el metal.
/// ─────────────────────────────────────────────────────────────────────────
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

    [Header("Estaño — LeftCtrl")]
    public MetalBarEntry pewter;

    [Header("Duraluminio — F")]
    public MetalBarEntry duralumin;

    [Header("Jugador")]
    public PlayerInventory inventory;   // ← arrastra el Player aquí

    private MetalReserve reserve;

    // ── Inicialización ────────────────────────────────────────────────────────

    // Guardamos referencias para poder desuscribir en OnDestroy
    private UnityEngine.Events.UnityAction<float,float> onSteelRes, onIronRes, onPewterRes, onDuralRes;

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

        // UnityEvent<float,float> → AddListener
        onSteelRes  = (cur, max) => SetBar(steel,     cur, max);
        onIronRes   = (cur, max) => SetBar(iron,      cur, max);
        onPewterRes = (cur, max) => SetBar(pewter,    cur, max);
        onDuralRes  = (cur, max) => SetBar(duralumin, cur, max);

        reserve.OnSteelChanged    .AddListener(onSteelRes);
        reserve.OnIronChanged     .AddListener(onIronRes);
        reserve.OnPewterChanged   .AddListener(onPewterRes);
        reserve.OnDuraluminChanged.AddListener(onDuralRes);

        // System.Action<int> → +=
        inventory.OnSteelFlasksChanged     += n => SetFlaskText(steel,     n);
        inventory.OnIronFlasksChanged      += n => SetFlaskText(iron,      n);
        inventory.OnPewterFlasksChanged    += n => SetFlaskText(pewter,    n);
        inventory.OnDuraluminFlasksChanged += n => SetFlaskText(duralumin, n);

        // Valores iniciales
        SetBar(steel,     reserve.CurrentSteel,     reserve.maxSteel);
        SetBar(iron,      reserve.CurrentIron,      reserve.maxIron);
        SetBar(pewter,    reserve.CurrentPewter,    reserve.maxPewter);
        SetBar(duralumin, reserve.CurrentDuralumin, reserve.maxDuralumin);

        SetFlaskText(steel,     inventory.SteelFlasks);
        SetFlaskText(iron,      inventory.IronFlasks);
        SetFlaskText(pewter,    inventory.PewterFlasks);
        SetFlaskText(duralumin, inventory.DuraluminFlasks);
    }

    void OnDestroy()
    {
        if (reserve != null)
        {
            reserve.OnSteelChanged    .RemoveListener(onSteelRes);
            reserve.OnIronChanged     .RemoveListener(onIronRes);
            reserve.OnPewterChanged   .RemoveListener(onPewterRes);
            reserve.OnDuraluminChanged.RemoveListener(onDuralRes);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

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
