using UnityEngine;
using UnityEngine.Events;

public class EnemyHealth : MonoBehaviour
{
    [Header("Salud")]
    public float maxHealth  = 30f;

    [Header("Muerte")]
    public float deathDelay = 1f;

    [Header("Estado (solo lectura)")]
    [SerializeField] private float currentHealth;

    private HitFlash  hitFlash;
    private Knockback knockback;

    public UnityEvent OnDeath;
    public UnityEvent OnDamaged;

    public bool  IsDead         => currentHealth <= 0f;
    public float HealthFraction => currentHealth / maxHealth;

    void Awake()
    {
        currentHealth = maxHealth;
        hitFlash      = GetComponent<HitFlash>();
        knockback     = GetComponent<Knockback>();
    }

    /// <summary>
    /// Recibir daño. attacker = quien golpea (para dirección del knockback).
    /// </summary>
    public void TakeDamage(float amount, Transform attacker = null)
    {
        if (IsDead) return;

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
        OnDeath?.Invoke();
        Destroy(gameObject, deathDelay);
    }
}