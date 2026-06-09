using UnityEngine;

public class MetalFlask : MonoBehaviour
{
    public enum FlaskType { Steel, Iron, Pewter, Duralumin }

    [Header("Tipo")]
    public FlaskType flaskType = FlaskType.Steel;

    [Header("Recarga")]
    [Tooltip("Fracción de la barra que recarga (0.5 = 50%)")]
    public float refillFraction = 0.5f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Collect(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Collect(other.gameObject);
    }

    void Collect(GameObject obj)
    {
        if (!obj.CompareTag("Player")) return;

        // Va al inventario — el jugador lo usa cuando quiera con 1/2/3/4
        PlayerInventory inventory = obj.GetComponent<PlayerInventory>();
        if (inventory == null) return;

        inventory.AddFlask(flaskType);
        Debug.Log($"[MetalFlask] Recogido frasco {flaskType} → inventario.");
        Destroy(gameObject);
    }
}