using UnityEngine;

/// <summary>
/// Permite al jugador usar frascos de metal del inventario con las teclas 1-4.
/// Adjuntar al mismo GameObject que PlayerController.
///
///   1 → frasco de Acero   (Steel)
///   2 → frasco de Hierro  (Iron)
///   3 → frasco de Estaño  (Pewter)
///   4 → frasco de Dural.  (Duralumin)
/// </summary>
public class FlaskUser : MonoBehaviour
{
    [Header("Teclas de uso")]
    public KeyCode steelKey     = KeyCode.Alpha1;
    public KeyCode ironKey      = KeyCode.Alpha2;
    public KeyCode pewterKey    = KeyCode.Alpha3;
    public KeyCode duraluminKey = KeyCode.Alpha4;

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
        if (Input.GetKeyDown(steelKey))     TryUse(MetalFlask.FlaskType.Steel);
        if (Input.GetKeyDown(ironKey))      TryUse(MetalFlask.FlaskType.Iron);
        if (Input.GetKeyDown(pewterKey))    TryUse(MetalFlask.FlaskType.Pewter);
        if (Input.GetKeyDown(duraluminKey)) TryUse(MetalFlask.FlaskType.Duralumin);
    }

    void TryUse(MetalFlask.FlaskType type)
    {
        if (inventory == null || reserve == null) return;

        if (!inventory.ConsumeFlask(type))
        {
            Debug.Log($"[FlaskUser] Sin frascos de {type}.");
            return;
        }

        float max = type == MetalFlask.FlaskType.Steel     ? reserve.maxSteel     :
                    type == MetalFlask.FlaskType.Iron      ? reserve.maxIron      :
                    type == MetalFlask.FlaskType.Pewter    ? reserve.maxPewter    :
                                                              reserve.maxDuralumin;

        reserve.Refill(type, max * refillFraction);
        Debug.Log($"[FlaskUser] Usó frasco de {type}. +{max * refillFraction:F0}");
    }
}
