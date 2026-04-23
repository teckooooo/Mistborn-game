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

        MetalReserve reserve = obj.GetComponent<MetalReserve>();
        if (reserve == null) return;

        float max = flaskType == FlaskType.Steel     ? reserve.maxSteel     :
                    flaskType == FlaskType.Iron      ? reserve.maxIron      :
                    flaskType == FlaskType.Pewter    ? reserve.maxPewter    :
                                                       reserve.maxDuralumin;

        reserve.Refill(flaskType, max * refillFraction);
        Debug.Log($"[MetalFlask] Recogido frasco {flaskType} +{max * refillFraction:F0}.");
        Destroy(gameObject);
    }
}