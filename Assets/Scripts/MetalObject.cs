using UnityEngine;

public class MetalObject : MonoBehaviour
{
    public float metalMass = 0.5f;

    [HideInInspector] public float anchoredMass = 0f; // masa del objeto en que está clavado

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void ApplyForce(Vector2 force)
    {
        if (rb != null)
            rb.AddForce(force, ForceMode2D.Impulse);
    }

    // Masa efectiva = masa propia + masa del objeto anclado
    public float EffectiveMass => metalMass + anchoredMass;
}