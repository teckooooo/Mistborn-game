using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Salud")]
    public float maxHealth      = 100f;
    public float invincibleTime = 0.5f;

    [Header("Estado (solo lectura)")]
    [SerializeField] private float currentHealth;
    [SerializeField] private bool  isInvincible;
    [SerializeField] private float invincibleTimer;

    private MetalReserve reserve;
    private HitFlash     hitFlash;
    private Knockback    knockback;

    public UnityEvent<float, float> OnHealthChanged;
    public UnityEvent               OnDeath;
    public UnityEvent               OnDamaged;

    public float CurrentHealth  => currentHealth;
    public float MaxHealth      => maxHealth;
    public float HealthFraction => currentHealth / maxHealth;
    public bool  IsDead         => currentHealth <= 0f;

    void Awake()
    {
        reserve       = GetComponent<MetalReserve>();
        hitFlash      = GetComponent<HitFlash>();
        knockback     = GetComponent<Knockback>();
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (!isInvincible) return;
        invincibleTimer -= Time.deltaTime;
        if (invincibleTimer <= 0) { isInvincible = false; invincibleTimer = 0; }
    }

    /// <summary>
    /// Recibir daño. attacker = quien golpea (para calcular dirección del knockback).
    /// </summary>
    public void TakeDamage(float amount, Transform attacker = null)
    {
        if (isInvincible || IsDead) return;

        float reduction  = reserve != null ? reserve.PewterDamageReduction : 0f;
        float finalDamage = amount * (1f - reduction);

        currentHealth = Mathf.Max(currentHealth - finalDamage, 0);

        Debug.Log($"[PlayerHealth] Daño: {finalDamage:F1} (reducción Pewter {reduction * 100:F0}%) | HP: {currentHealth:F1}/{maxHealth}");

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnDamaged?.Invoke();

        // Hit flash
        hitFlash?.Flash();

        // Knockback — empuje opuesto al atacante
        if (knockback != null && attacker != null)
        {
            Vector2 dir = ((Vector2)transform.position - (Vector2)attacker.position).normalized;
            knockback.ApplyKnockback(dir);
        }
        else if (knockback != null)
        {
            // Sin atacante conocido — empujar hacia arriba
            knockback.ApplyKnockback(Vector2.up);
        }

        isInvincible    = true;
        invincibleTimer = invincibleTime;

        if (currentHealth <= 0) Die();
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Mata al jugador instantáneamente ignorando invencibilidad y reducción
    /// de Pewter. Para zonas de muerte instantánea (lava, vacío, pinchos).
    /// </summary>
    public void Kill()
    {
        if (IsDead) return;
        currentHealth = 0f;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Die();
    }

    void Die()
    {
        Debug.Log("[PlayerHealth] Jugador muerto — cargando pantalla de muerte.");
        OnDeath?.Invoke();
        DeathScreen.GoToDeathScreen();
    }
}