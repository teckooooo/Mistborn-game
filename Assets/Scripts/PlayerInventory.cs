using UnityEngine;

/// <summary>
/// Inventario físico del jugador: monedas, clavos y frascos de Acero/Hierro.
/// Adjuntar al mismo GameObject que PlayerController.
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    [Header("Proyectiles — máximo")]
    public int maxCoins = 15;
    public int maxNails = 99;

    [Header("Proyectiles iniciales")]
    public int startCoins = 15;
    public int startNails = 10;

    [Header("Frascos — máximo por tipo")]
    public int maxSteelFlasks = 5;
    public int maxIronFlasks  = 5;

    [Header("Frascos iniciales")]
    public int startSteelFlasks = 0;
    public int startIronFlasks  = 0;

    [Header("Estado (solo lectura)")]
    [SerializeField] private int currentCoins;
    [SerializeField] private int currentNails;
    [SerializeField] private int currentSteelFlasks;
    [SerializeField] private int currentIronFlasks;

    // ── Propiedades ───────────────────────────────────────────────────────────
    public int  Coins    => currentCoins;
    public int  Nails    => currentNails;
    public bool HasCoins => currentCoins > 0;
    public bool HasNails => currentNails > 0;

    public int  SteelFlasks    => currentSteelFlasks;
    public int  IronFlasks     => currentIronFlasks;
    public bool HasSteelFlask  => currentSteelFlasks > 0;
    public bool HasIronFlask   => currentIronFlasks  > 0;

    // ── Eventos ───────────────────────────────────────────────────────────────
    public System.Action<int> OnCoinsChanged;
    public System.Action<int> OnNailsChanged;
    public System.Action<int> OnSteelFlasksChanged;
    public System.Action<int> OnIronFlasksChanged;

    void Awake()
    {
        currentCoins       = Mathf.Clamp(startCoins, 0, maxCoins);
        currentNails       = Mathf.Clamp(startNails, 0, maxNails);
        currentSteelFlasks = Mathf.Clamp(startSteelFlasks, 0, maxSteelFlasks);
        currentIronFlasks  = Mathf.Clamp(startIronFlasks,  0, maxIronFlasks);
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

    /// <summary>Devuelve true si se agregó al menos una moneda, false si el inventario está lleno.</summary>
    public bool AddCoins(int amount)
    {
        if (currentCoins >= maxCoins) return false;
        currentCoins = Mathf.Min(currentCoins + amount, maxCoins);
        OnCoinsChanged?.Invoke(currentCoins);
        return true;
    }

    public void AddNails(int amount)
    {
        currentNails = Mathf.Min(currentNails + amount, maxNails);
        OnNailsChanged?.Invoke(currentNails);
    }

    // ── Frascos ───────────────────────────────────────────────────────────────

    public void AddFlask(MetalFlask.FlaskType type)
    {
        switch (type)
        {
            case MetalFlask.FlaskType.Steel:
                if (currentSteelFlasks >= maxSteelFlasks) { Debug.Log("[Inventario] Frascos de Acero al máximo."); return; }
                currentSteelFlasks++; OnSteelFlasksChanged?.Invoke(currentSteelFlasks); break;
            case MetalFlask.FlaskType.Iron:
                if (currentIronFlasks >= maxIronFlasks) { Debug.Log("[Inventario] Frascos de Hierro al máximo."); return; }
                currentIronFlasks++;  OnIronFlasksChanged?.Invoke(currentIronFlasks);   break;
        }
    }

    public bool ConsumeFlask(MetalFlask.FlaskType type)
    {
        switch (type)
        {
            case MetalFlask.FlaskType.Steel:
                if (currentSteelFlasks <= 0) return false;
                currentSteelFlasks--; OnSteelFlasksChanged?.Invoke(currentSteelFlasks); return true;
            case MetalFlask.FlaskType.Iron:
                if (currentIronFlasks <= 0) return false;
                currentIronFlasks--;  OnIronFlasksChanged?.Invoke(currentIronFlasks);   return true;
        }
        return false;
    }
}
