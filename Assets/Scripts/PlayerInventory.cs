using UnityEngine;

/// <summary>
/// Inventario físico del jugador: monedas, clavos y frascos de metal.
/// Adjuntar al mismo GameObject que PlayerController.
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    [Header("Proyectiles iniciales")]
    public int startCoins = 15;
    public int startNails = 10;

    [Header("Frascos — máximo por tipo")]
    public int maxSteelFlasks     = 5;
    public int maxIronFlasks      = 3;
    public int maxPewterFlasks    = 5;
    public int maxDuraluminFlasks = 2;

    [Header("Frascos iniciales")]
    public int startSteelFlasks     = 0;
    public int startIronFlasks      = 0;
    public int startPewterFlasks    = 0;
    public int startDuraluminFlasks = 0;

    [Header("Estado (solo lectura)")]
    [SerializeField] private int currentCoins;
    [SerializeField] private int currentNails;
    [SerializeField] private int currentSteelFlasks;
    [SerializeField] private int currentIronFlasks;
    [SerializeField] private int currentPewterFlasks;
    [SerializeField] private int currentDuraluminFlasks;

    // ── Propiedades ───────────────────────────────────────────────────────────
    public int  Coins    => currentCoins;
    public int  Nails    => currentNails;
    public bool HasCoins => currentCoins > 0;
    public bool HasNails => currentNails > 0;

    public int  SteelFlasks     => currentSteelFlasks;
    public int  IronFlasks      => currentIronFlasks;
    public int  PewterFlasks    => currentPewterFlasks;
    public int  DuraluminFlasks => currentDuraluminFlasks;
    public bool HasSteelFlask     => currentSteelFlasks     > 0;
    public bool HasIronFlask      => currentIronFlasks      > 0;
    public bool HasPewterFlask    => currentPewterFlasks    > 0;
    public bool HasDuraluminFlask => currentDuraluminFlasks > 0;

    // ── Eventos ───────────────────────────────────────────────────────────────
    public System.Action<int> OnCoinsChanged;
    public System.Action<int> OnNailsChanged;
    public System.Action<int> OnSteelFlasksChanged;
    public System.Action<int> OnIronFlasksChanged;
    public System.Action<int> OnPewterFlasksChanged;
    public System.Action<int> OnDuraluminFlasksChanged;

    void Awake()
    {
        currentCoins          = startCoins;
        currentNails          = startNails;
        currentSteelFlasks    = Mathf.Clamp(startSteelFlasks,     0, maxSteelFlasks);
        currentIronFlasks     = Mathf.Clamp(startIronFlasks,      0, maxIronFlasks);
        currentPewterFlasks   = Mathf.Clamp(startPewterFlasks,    0, maxPewterFlasks);
        currentDuraluminFlasks= Mathf.Clamp(startDuraluminFlasks, 0, maxDuraluminFlasks);
    }

    // ── Proyectiles ───────────────────────────────────────────────────────────

    public bool ConsumeCoin()
    {
        if (currentCoins <= 0) { Debug.Log("[Inventario] Sin monedas."); return false; }
        currentCoins--;
        OnCoinsChanged?.Invoke(currentCoins);
        return true;
    }

    public bool ConsumeNail()
    {
        if (currentNails <= 0) { Debug.Log("[Inventario] Sin clavos."); return false; }
        currentNails--;
        OnNailsChanged?.Invoke(currentNails);
        return true;
    }

    public void AddCoins(int amount) { currentCoins += amount; OnCoinsChanged?.Invoke(currentCoins); }
    public void AddNails(int amount) { currentNails += amount; OnNailsChanged?.Invoke(currentNails); }

    // ── Frascos ───────────────────────────────────────────────────────────────

    public void AddFlask(MetalFlask.FlaskType type)
    {
        switch (type)
        {
            case MetalFlask.FlaskType.Steel:
                if (currentSteelFlasks >= maxSteelFlasks) { Debug.Log("[Inventario] Frascos de Acero al máximo."); return; }
                currentSteelFlasks++;     OnSteelFlasksChanged?.Invoke(currentSteelFlasks);     break;
            case MetalFlask.FlaskType.Iron:
                if (currentIronFlasks >= maxIronFlasks) { Debug.Log("[Inventario] Frascos de Hierro al máximo."); return; }
                currentIronFlasks++;      OnIronFlasksChanged?.Invoke(currentIronFlasks);       break;
            case MetalFlask.FlaskType.Pewter:
                if (currentPewterFlasks >= maxPewterFlasks) { Debug.Log("[Inventario] Frascos de Estaño al máximo."); return; }
                currentPewterFlasks++;    OnPewterFlasksChanged?.Invoke(currentPewterFlasks);   break;
            case MetalFlask.FlaskType.Duralumin:
                if (currentDuraluminFlasks >= maxDuraluminFlasks) { Debug.Log("[Inventario] Frascos de Duraluminio al máximo."); return; }
                currentDuraluminFlasks++; OnDuraluminFlasksChanged?.Invoke(currentDuraluminFlasks); break;
        }
    }

    public bool ConsumeFlask(MetalFlask.FlaskType type)
    {
        switch (type)
        {
            case MetalFlask.FlaskType.Steel:
                if (currentSteelFlasks <= 0) return false;
                currentSteelFlasks--;     OnSteelFlasksChanged?.Invoke(currentSteelFlasks);     return true;
            case MetalFlask.FlaskType.Iron:
                if (currentIronFlasks <= 0) return false;
                currentIronFlasks--;      OnIronFlasksChanged?.Invoke(currentIronFlasks);       return true;
            case MetalFlask.FlaskType.Pewter:
                if (currentPewterFlasks <= 0) return false;
                currentPewterFlasks--;    OnPewterFlasksChanged?.Invoke(currentPewterFlasks);   return true;
            case MetalFlask.FlaskType.Duralumin:
                if (currentDuraluminFlasks <= 0) return false;
                currentDuraluminFlasks--; OnDuraluminFlasksChanged?.Invoke(currentDuraluminFlasks); return true;
        }
        return false;
    }
}
