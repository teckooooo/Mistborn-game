using UnityEngine;

/// <summary>
/// Proyectil que dispara el jefe (BossAI). Vuela en línea recta, daña al
/// jugador al tocarlo y se destruye al impactar superficies o tras 'lifeTime'.
///
/// ─── Setup del prefab ─────────────────────────────────────────────────────
/// 1. GameObject con SpriteRenderer (sprite placeholder por ahora).
/// 2. Rigidbody2D (este script le pone gravityScale = 0).
/// 3. Collider2D con 'Is Trigger' MARCADO.
/// 4. Este script.
/// 5. Guardar como prefab y asignarlo en BossAI → 'projectilePrefab'.
/// ─────────────────────────────────────────────────────────────────────────
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class BossProjectile : MonoBehaviour
{
    [Header("Daño")]
    public float damage = 15f;

    [Header("Vida")]
    [Tooltip("Segundos antes de autodestruirse si no impacta nada.")]
    public float lifeTime = 5f;

    [Header("Tags")]
    public string playerTag = "Player";
    [Tooltip("Superficies que destruyen el proyectil al impactar.")]
    public string[] destroyOnTags = { "Ground", "Piso", "Muro", "Cristal" };

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    /// <summary>Lanza el proyectil en una dirección a cierta velocidad.</summary>
    public void Launch(Vector2 dir, float speed)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        dir = dir.normalized;
        rb.linearVelocity = dir * speed;

        // Orientar el sprite hacia la dirección de vuelo.
        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, ang);

        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            other.GetComponent<PlayerHealth>()?.TakeDamage(damage, transform);
            Destroy(gameObject);
            return;
        }

        foreach (string t in destroyOnTags)
            if (other.CompareTag(t)) { Destroy(gameObject); return; }
    }
}
