using UnityEngine;
using UnityEngine.Events;

public class MetalReserve : MonoBehaviour
{
    [Header("Reservas máximas")]
    public float maxSteel     = 100f;
    public float maxIron      = 100f;
    public float maxPewter    = 100f;
    public float maxDuralumin = 100f;

    [Header("Reservas actuales (solo lectura)")]
    [SerializeField] private float currentSteel;
    [SerializeField] private float currentIron;
    [SerializeField] private float currentPewter;
    [SerializeField] private float currentDuralumin;

    [Header("Consumo por segundo")]
    public float steelCostPerSec  = 5f;
    public float ironCostPerSec   = 5f;
    public float pewterCostPerSec = 8f;
    public float nailCost         = 10f;
    public float coinCost         = 5f;

    [Header("Pewter — efectos")]
    [Tooltip("Multiplicador de fuerza de salto mientras Pewter está activo")]
    public float pewterJumpMultiplier    = 1.4f;
    [Tooltip("Multiplicador de velocidad de movimiento mientras Pewter está activo")]
    public float pewterSpeedMultiplier   = 1.3f;
    [Tooltip("Porcentaje de daño absorbido mientras Pewter está activo (0-1)")]
    public float pewterDamageReduction   = 0.4f;

    [Header("Estado Pewter (solo lectura)")]
    [SerializeField] private bool pewterActive;

    public bool  PewterActive          => pewterActive;
    public float PewterJumpMultiplier  => pewterActive ? pewterJumpMultiplier  : 1f;
    public float PewterSpeedMultiplier => pewterActive ? pewterSpeedMultiplier : 1f;
    public float PewterDamageReduction => pewterActive ? pewterDamageReduction : 0f;

    [Header("Duraluminio")]
    public float duraluMinBoost    = 10f;
    public float duraluMinDuration = 5f;

    [Header("Duraluminio — Metales afectados")]
    public bool duraluMinAffectsSteel  = true;
    public bool duraluMinAffectsIron   = true;
    public bool duraluMinAffectsPewter = false;

    private bool steelUsedDuringBoost  = false;
    private bool ironUsedDuringBoost   = false;
    private bool pewterUsedDuringBoost = false;

    [Header("Estado Duraluminio (solo lectura)")]
    [SerializeField] private bool  duraluMinActive;
    [SerializeField] private float duraluMinTimer;

    public bool  DuraluMinActive => duraluMinActive;
    public float DuraluMinTimer  => duraluMinTimer;
    public float DuraluMinBoost  => duraluMinBoost;

    // Eventos
    public UnityEvent<float, float> OnSteelChanged;
    public UnityEvent<float, float> OnIronChanged;
    public UnityEvent<float, float> OnPewterChanged;
    public UnityEvent<float, float> OnDuraluminChanged;
    public UnityEvent<bool>         OnPewterToggled;

    // Propiedades públicas de lectura
    public bool HasSteel     => currentSteel     > 1f;
    public bool HasIron      => currentIron      > 1f;
    public bool HasPewter    => currentPewter    > 1f;
    public bool HasDuralumin => currentDuralumin > 1f;

    public float SteelFraction     => currentSteel     / maxSteel;
    public float IronFraction      => currentIron      / maxIron;
    public float PewterFraction    => currentPewter    / maxPewter;
    public float DuraluminFraction => currentDuralumin / maxDuralumin;

    public float CurrentSteel     => currentSteel;
    public float CurrentIron      => currentIron;
    public float CurrentPewter    => currentPewter;
    public float CurrentDuralumin => currentDuralumin;

    // ── Inicialización ────────────────────────────────────────────────────────

    void Awake()
    {
        currentSteel     = maxSteel;
        currentIron      = maxIron;
        currentPewter    = maxPewter;
        currentDuralumin = maxDuralumin;
    }

    // ── Update — Duraluminio timer ────────────────────────────────────────────

    void Update()
    {
        if (!duraluMinActive) return;

        duraluMinTimer -= Time.deltaTime;

        // Barra refleja el tiempo restante en tiempo real
        currentDuralumin = Mathf.Max((duraluMinTimer / duraluMinDuration) * maxDuralumin, 0f);
        OnDuraluminChanged?.Invoke(currentDuralumin, maxDuralumin);

        if (duraluMinTimer <= 0)
        {
            duraluMinActive = false;
            duraluMinTimer  = 0;

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
            if (duraluMinAffectsPewter && pewterUsedDuringBoost)
            {
                currentPewter = 0;
                OnPewterChanged?.Invoke(currentPewter, maxPewter);
            }

            currentDuralumin = 0;
            OnDuraluminChanged?.Invoke(currentDuralumin, maxDuralumin);

            steelUsedDuringBoost  = false;
            ironUsedDuringBoost   = false;
            pewterUsedDuringBoost = false;

            Debug.Log("[Duraluminio] Boost terminado.");
        }
    }

    // ── Consumo — Steel / Iron ────────────────────────────────────────────────

    public void ConsumeSteelPerSec(float dt)
    {
        if (duraluMinActive && duraluMinAffectsSteel)
        {
            steelUsedDuringBoost = true;
            return;
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

    public void ConsumeCoin()
    {
        currentSteel = Mathf.Max(currentSteel - coinCost, 0);
        OnSteelChanged?.Invoke(currentSteel, maxSteel);
    }

    public void MarkSteelUsed()  { if (duraluMinActive) steelUsedDuringBoost  = true; }
    public void MarkIronUsed()   { if (duraluMinActive) ironUsedDuringBoost   = true; }

    // ── Pewter ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Llama cada frame mientras el jugador mantiene la tecla de Pewter.
    /// Devuelve false si se quedó sin reserva y desactivó solo.
    /// </summary>
    public bool ConsumePewterPerSec(float dt)
    {
        if (!pewterActive) return false;

        if (duraluMinActive && duraluMinAffectsPewter)
        {
            pewterUsedDuringBoost = true;
            return true;
        }

        currentPewter = Mathf.Max(currentPewter - pewterCostPerSec * dt, 0);
        OnPewterChanged?.Invoke(currentPewter, maxPewter);

        if (currentPewter <= 0)
        {
            DeactivatePewter();
            return false;
        }

        return true;
    }

    /// <summary>Activa Pewter si hay reserva. Devuelve true si se activó.</summary>
    public bool ActivatePewter()
    {
        if (!HasPewter)
        {
            Debug.Log("[Pewter] Sin reserva.");
            return false;
        }

        if (pewterActive) return true; // ya estaba activo

        pewterActive = true;
        OnPewterToggled?.Invoke(true);
        Debug.Log("[Pewter] Activado.");
        return true;
    }

    /// <summary>Desactiva Pewter manualmente o cuando se agota la reserva.</summary>
    public void DeactivatePewter()
    {
        if (!pewterActive) return;
        pewterActive = false;
        OnPewterToggled?.Invoke(false);
        Debug.Log("[Pewter] Desactivado.");
    }

    // ── Duraluminio ───────────────────────────────────────────────────────────

    public bool ActivateDuraluMin()
    {
        if (!HasDuralumin) { Debug.Log("[Duraluminio] Sin Duraluminio."); return false; }
        if (duraluMinActive) { Debug.Log("[Duraluminio] Ya está activo."); return false; }

        bool hasRequired = (duraluMinAffectsSteel  && HasSteel)  ||
                           (duraluMinAffectsIron   && HasIron)   ||
                           (duraluMinAffectsPewter && HasPewter);
        if (!hasRequired) { Debug.Log("[Duraluminio] Sin metal para amplificar."); return false; }

        duraluMinActive = true;
        duraluMinTimer  = duraluMinDuration;
        Debug.Log($"[Duraluminio] Boost activado por {duraluMinDuration}s");
        return true;
    }

    // ── Recarga (frascos) ─────────────────────────────────────────────────────

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
            case MetalFlask.FlaskType.Pewter:
                currentPewter = Mathf.Min(currentPewter + amount, maxPewter);
                OnPewterChanged?.Invoke(currentPewter, maxPewter);
                break;
            case MetalFlask.FlaskType.Duralumin:
                currentDuralumin = Mathf.Min(currentDuralumin + amount, maxDuralumin);
                OnDuraluminChanged?.Invoke(currentDuralumin, maxDuralumin);
                break;
        }
    }
}