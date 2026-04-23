using UnityEngine;

/// <summary>
/// Empuje al recibir daño.
/// Agregar al mismo GameObject que Rigidbody2D.
/// Llamar ApplyKnockback(direction) desde PlayerHealth.OnDamaged o EnemyHealth.OnDamaged.
/// </summary>
public class Knockback : MonoBehaviour
{
    [Header("Knockback")]
    public float force    = 6f;
    public float duration = 0.15f;

    private Rigidbody2D rb;
    private float       knockbackTimer = 0f;

    public bool IsKnockedBack => knockbackTimer > 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (knockbackTimer > 0)
            knockbackTimer -= Time.deltaTime;
    }

    /// <summary>
    /// Aplicar knockback en dirección dada.
    /// La dirección se calcula desde quien golpea hacia quien recibe.
    /// </summary>
    public void ApplyKnockback(Vector2 direction)
    {
        if (rb == null) return;
        knockbackTimer    = duration;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
    }
}