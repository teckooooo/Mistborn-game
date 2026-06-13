using UnityEngine;
using UnityEngine.Events;

public class MetalReserve : MonoBehaviour
{
    [Header("Reservas máximas")]
    public float maxSteel = 100f;
    public float maxIron  = 100f;

    [Header("Reservas actuales (solo lectura)")]
    [SerializeField] private float currentSteel;
    [SerializeField] private float currentIron;

    [Header("Consumo por segundo")]
    public float steelCostPerSec = 5f;
    public float ironCostPerSec  = 5f;
    public float nailCost        = 10f;
    public float coinCost        = 5f;

    // Eventos
    public UnityEvent<float, float> OnSteelChanged;
    public UnityEvent<float, float> OnIronChanged;

    // Propiedades públicas
    public bool  HasSteel => currentSteel > 1f;
    public bool  HasIron  => currentIron  > 1f;

    public float SteelFraction => currentSteel / maxSteel;
    public float IronFraction  => currentIron  / maxIron;

    public float CurrentSteel => currentSteel;
    public float CurrentIron  => currentIron;

    // ── Compatibilidad — propiedades vacías para que otros scripts no rompan ──
    public bool  PewterActive          => false;
    public float PewterSpeedMultiplier => 1f;
    public float PewterJumpMultiplier  => 1f;
    public float PewterDamageReduction => 0f;
    public bool  DuraluMinActive       => false;
    public float DuraluMinBoost        => 1f;
    public bool  duraluMinAffectsSteel  => false;
    public bool  duraluMinAffectsIron   => false;

    // ── Inicialización ────────────────────────────────────────────────────────

    void Awake()
    {
        currentSteel = maxSteel;
        currentIron  = maxIron;
    }

    // ── Consumo ───────────────────────────────────────────────────────────────

    public void ConsumeSteelPerSec(float dt)
    {
        currentSteel = Mathf.Max(currentSteel - steelCostPerSec * dt, 0);
        OnSteelChanged?.Invoke(currentSteel, maxSteel);
    }

    public void ConsumeIronPerSec(float dt)
    {
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

    // Stubs para compatibilidad con IronPull/SteelPush
    public void MarkSteelUsed() { }
    public void MarkIronUsed()  { }

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
        }
    }
}
