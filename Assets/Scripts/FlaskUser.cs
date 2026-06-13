using UnityEngine;

/// <summary>
/// Permite al jugador usar frascos de metal del inventario.
///   1 → frasco de Acero  (Steel)
///   2 → frasco de Hierro (Iron)
/// </summary>
public class FlaskUser : MonoBehaviour
{
    [Header("Teclas de uso")]
    public KeyCode steelKey = KeyCode.Alpha1;
    public KeyCode ironKey  = KeyCode.Alpha2;

    [Header("Recarga por frasco (fracción 0-1)")]
    public float refillFraction = 0.5f;

    private PlayerInventory inventory;
    private MetalReserve    reserve;

    void Start()
    {
        inventory = GetComponent<PlayerInventory>();
        reserve   = GetComponent<MetalReserve>();
    }

    void Update()
    {
        if (PauseMenu.IsPaused) return;

        if (Input.GetKeyDown(steelKey)) TryUse(MetalFlask.FlaskType.Steel);
        if (Input.GetKeyDown(ironKey))  TryUse(MetalFlask.FlaskType.Iron);
    }

    void TryUse(MetalFlask.FlaskType type)
    {
        if (inventory == null || reserve == null) return;

        if (!inventory.ConsumeFlask(type))
        {
            Debug.Log($"[FlaskUser] Sin frascos de {type}.");
            return;
        }

        float max = type == MetalFlask.FlaskType.Steel ? reserve.maxSteel : reserve.maxIron;
        reserve.Refill(type, max * refillFraction);
        Debug.Log($"[FlaskUser] Usó frasco de {type}. +{max * refillFraction:F0}");
    }
}
