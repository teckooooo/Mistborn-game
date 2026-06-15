using UnityEngine;
using UnityEngine.Events;

public class EnemyHealth : MonoBehaviour
{
    [Header("Salud")]
    public float maxHealth  = 30f;

    [Header("Invencibilidad tras recibir daño")]
    [Tooltip("Segundos de invulnerabilidad tras un golpe. Limita el daño por 'spam' " +
             "(ej. monedas). 0 = sin límite. Para jefes prueba 0.2–0.3.")]
    public float damageCooldown = 0f;

    [Header("Muerte")]
    public float deathDelay = 1f;

    [Header("Estado (solo lectura)")]
    [SerializeField] private float currentHealth;
    [SerializeField] private float damageTimer;

    private HitFlash  hitFlash;
    private Knockback knockback;

    public UnityEvent OnDeath;
    public UnityEvent OnDamaged;

    public bool  IsDead         => currentHealth <= 0f;
    public float HealthFraction => currentHealth / maxHealth;

    /// <summary>Si está activo, los proyectiles (monedas/clavos) no le hacen daño.
    /// Lo controla el jefe durante el cast para deflectar el metal.</summary>
    [System.NonSerialized] public bool projectileImmune = false;

    void Awake()
    {
        currentHealth = maxHealth;
        hitFlash      = GetComponent<HitFlash>();
        knockback     = GetComponent<Knockback>();
    }

    /// <summary>
    /// Recibir daño. attacker = quien golpea (para dirección del knockback).
    /// </summary>
    void Update()
    {
        if (damageTimer > 0f) damageTimer -= Time.deltaTime;
    }

    public void TakeDamage(float amount, Transform attacker = null)
    {
        if (IsDead) return;

        // Invencibilidad breve: ignora golpes mientras el timer corra.
        if (damageCooldown > 0f && damageTimer > 0f) return;
        damageTimer = damageCooldown;

        currentHealth = Mathf.Max(currentHealth - amount, 0);
        Debug.Log($"[EnemyHealth] {gameObject.name} recibió {amount:F1} | HP: {currentHealth:F1}/{maxHealth}");

        OnDamaged?.Invoke();

        // Hit flash
        hitFlash?.Flash();

        // Knockback
        if (knockback != null)
        {
            Vector2 dir = attacker != null
                ? ((Vector2)transform.position - (Vector2)attacker.position).normalized
                : Vector2.up;
            knockback.ApplyKnockback(dir);
        }

        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        Debug.Log($"[EnemyHealth] {gameObject.name} eliminado.");

        // Deshabilitar collider para que los drops no colisionen con el cadáver
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        OnDeath?.Invoke();
        Destroy(gameObject, deathDelay);
    }
}