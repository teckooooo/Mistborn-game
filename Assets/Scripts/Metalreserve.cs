using UnityEngine;
using UnityEngine.Events;

public class MetalReserve : MonoBehaviour
{
    [Header("Reservas")]
    public float maxSteel     = 100f;
    public float maxIron      = 100f;
    public float maxDuralumin = 100f;

    [Header("Reservas actuales (solo lectura)")]
    [SerializeField] private float currentSteel;
    [SerializeField] private float currentIron;
    [SerializeField] private float currentDuralumin;

    [Header("Consumo por segundo")]
    public float steelCostPerSec  = 5f;
    public float ironCostPerSec   = 5f;
    public float nailCost         = 10f;
    public float coinCost         = 5f;
    public float duraluMinBoost   = 10f;

    [Header("Duraluminio")]
    public float duraluMinDuration = 5f;

    [Header("Duraluminio — Metales afectados")]
    public bool duraluMinAffectsSteel    = true;
    public bool duraluMinAffectsIron     = true;

    // Tracking de metales usados durante el boost
    private bool steelUsedDuringBoost = false;
    private bool ironUsedDuringBoost  = false;

    [Header("Estado Duraluminio (solo lectura)")]
    [SerializeField] private bool  duraluMinActive;
    [SerializeField] private float duraluMinTimer;

    public bool  DuraluMinActive => duraluMinActive;
    public float DuraluMinTimer  => duraluMinTimer;
    public float DuraluMinBoost  => duraluMinBoost;

    public UnityEvent<float, float> OnSteelChanged;
    public UnityEvent<float, float> OnIronChanged;
    public UnityEvent<float, float> OnDuraluminChanged;

    public bool HasSteel     => currentSteel     > 1f;
    public bool HasIron      => currentIron      > 1f;
    public bool HasDuralumin => currentDuralumin > 1f;

    public float SteelFraction     => currentSteel     / maxSteel;
    public float IronFraction      => currentIron      / maxIron;
    public float DuraluminFraction => currentDuralumin / maxDuralumin;

    public float CurrentSteel     => currentSteel;
    public float CurrentIron      => currentIron;
    public float CurrentDuralumin => currentDuralumin;

    void Awake()
    {
        currentSteel     = maxSteel;
        currentIron      = maxIron;
        currentDuralumin = maxDuralumin;
    }

    void Update()
    {
        if (!duraluMinActive) return;

        duraluMinTimer -= Time.deltaTime;

        if (duraluMinTimer <= 0)
        {
            duraluMinActive = false;
            duraluMinTimer  = 0;

            // Solo quemar los metales que realmente se usaron durante el boost
            if (duraluMinAffectsSteel && steelUsedDuringBoost)
            {
                currentSteel = 0;
                OnSteelChanged?.Invoke(currentSteel, maxSteel);
            }
            if (duraluMinAffectsIron && ironUsedDuringBoost)
            {
                currentIron = 0;
                OnIronChanged?.Invoke(currentIron, maxIron);
            }
            currentDuralumin = 0;
            OnDuraluminChanged?.Invoke(currentDuralumin, maxDuralumin);
            steelUsedDuringBoost = false;
            ironUsedDuringBoost  = false;
            Debug.Log("[Duraluminio] Boost terminado.");
        }
    }

    // ── Consumo normal ────────────────────────────────────────────

    public void ConsumeSteelPerSec(float dt)
    {
        if (duraluMinActive && duraluMinAffectsSteel)
        {
            steelUsedDuringBoost = true;
            return; // no consumir durante boost
        }
        currentSteel = Mathf.Max(currentSteel - steelCostPerSec * dt, 0);
        OnSteelChanged?.Invoke(currentSteel, maxSteel);
    }

    public void ConsumeIronPerSec(float dt)
    {
        if (duraluMinActive && duraluMinAffectsIron)
        {
            ironUsedDuringBoost = true;
            return;
        }
        currentIron = Mathf.Max(currentIron - ironCostPerSec * dt, 0);
        OnIronChanged?.Invoke(currentIron, maxIron);
    }

    public void ConsumeNail()
    {
        currentSteel = Mathf.Max(currentSteel - nailCost, 0);
        OnSteelChanged?.Invoke(currentSteel, maxSteel);
    }

    public void MarkSteelUsed() { if (duraluMinActive) steelUsedDuringBoost = true; }
    public void MarkIronUsed()  { if (duraluMinActive) ironUsedDuringBoost  = true; }

    public void ConsumeCoin()
    {
        currentSteel = Mathf.Max(currentSteel - coinCost, 0);
        OnSteelChanged?.Invoke(currentSteel, maxSteel);
    }

    // ── Duraluminio ───────────────────────────────────────────────

    /// <summary>Activa el boost por duraluMinDuration segundos.</summary>
    public bool ActivateDuraluMin()
    {
        if (!HasDuralumin) { Debug.Log("[Duraluminio] Sin Duraluminio."); return false; }
        if (duraluMinActive) { Debug.Log("[Duraluminio] Ya está activo."); return false; }

        // Verificar que al menos uno de los metales afectados tenga reservas
        bool hasRequired = (duraluMinAffectsSteel && HasSteel) ||
                           (duraluMinAffectsIron  && HasIron);
        if (!hasRequired) { Debug.Log("[Duraluminio] Sin metal para amplificar."); return false; }

        duraluMinActive = true;
        duraluMinTimer  = duraluMinDuration;
        Debug.Log($"[Duraluminio] Boost activado por {duraluMinDuration}s | boost={duraluMinBoost}x");
        return true;
    }

    // ── Recarga ───────────────────────────────────────────────────

    public void Refill(MetalFlask.FlaskType type, float amount)
    {
        switch (type)
        {
            case MetalFlask.FlaskType.Steel:
                currentSteel = Mathf.Min(currentSteel + amount, maxSteel);
                OnSteelChanged?.Invoke(currentSteel, maxSteel);
                break;
            case MetalFlask.FlaskType.Iron:
                currentIron = Mathf.Min(currentIron + amount, maxIron);
                OnIronChanged?.Invoke(currentIron, maxIron);
                break;
            case MetalFlask.FlaskType.Duralumin:
                currentDuralumin = Mathf.Min(currentDuralumin + amount, maxDuralumin);
                OnDuraluminChanged?.Invoke(currentDuralumin, maxDuralumin);
                break;
        }
    }
}