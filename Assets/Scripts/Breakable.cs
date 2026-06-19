using UnityEngine;

/// <summary>
/// Objeto destructible. Aguanta cierta cantidad de impactos de monedas/clavos
/// y se rompe al agotar su resistencia. Pensado para muros (tag "Muro") que
/// el jugador puede derribar a fuerza de proyectiles.
///
/// Detecta los impactos por colisión física: cuando una Coin o un Nail choca
/// con él a suficiente velocidad, recibe daño. No necesita Rigidbody2D propio
/// (la colisión la dispara el rigidbody del proyectil).
///
/// ─── Setup en Unity ───────────────────────────────────────────────────────
/// 1. El muro debe tener Collider2D (NO trigger) y su tag "Muro".
/// 2. Add Component → Breakable.
/// 3. Ajustar 'maxHealth' = cuántos impactos aguanta.
/// 4. (Opcional) 'damageStages' con sprites de roto progresivo, y 'breakEffect'
///    con un prefab de partículas/escombros.
/// ─────────────────────────────────────────────────────────────────────────
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Breakable : MonoBehaviour
{
    [Header("Resistencia")]
    [Tooltip("Daño total que aguanta antes de romperse. Con daño 1 por impacto, = nº de impactos.")]
    public float maxHealth = 3f;

    [Tooltip("Velocidad mínima del proyectil para que el impacto cuente.")]
    public float minImpactSpeed = 3f;

    [Header("Qué lo daña y cuánto")]
    public bool  damagedByCoins = true;
    public float coinDamage     = 1f;
    public bool  damagedByNails = true;
    public float nailDamage     = 2f;

    [Header("Estados visuales (opcional)")]
    [Tooltip("Sprites de daño progresivo, de sano (índice 0) a casi roto (último). Vacío = sin cambio.")]
    public Sprite[] damageStages;

    [Header("Al romperse")]
    [Tooltip("Prefab opcional de partículas/escombros al romperse.")]
    public GameObject breakEffect;
    [Tooltip("Si está activo, desactiva el objeto en vez de destruirlo (reutilizable).")]
    public bool disableInsteadOfDestroy = false;

    [Header("Estado (solo lectura)")]
    [SerializeField] private float currentHealth;

    private SpriteRenderer sr;
    private bool broken;

    public bool IsBroken => broken;

    void Awake()
    {
        currentHealth = maxHealth;
        sr = GetComponent<SpriteRenderer>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (broken) return;

        float speed = collision.relativeVelocity.magnitude;
        if (speed < minImpactSpeed) return;

        GameObject other = collision.gameObject;

        if (damagedByCoins && other.GetComponent<Coin>() != null)
        {
            Hit(coinDamage);
        }
        else if (damagedByNails && other.GetComponent<Nail>() != null)
        {
            Hit(nailDamage);
        }
    }

    /// <summary>Aplica daño al objeto. Público para poder romperlo también desde
    /// otras fuentes (melee, explosiones) en el futuro.</summary>
    public void Hit(float amount)
    {
        if (broken) return;

        currentHealth -= amount;
        Debug.Log($"[Breakable] {name} recibió {amount} | resistencia: {currentHealth}/{maxHealth}");

        UpdateVisual();

        if (currentHealth <= 0f) Break();
    }

    void UpdateVisual()
    {
        if (sr == null || damageStages == null || damageStages.Length == 0) return;

        // frac 1 = sano → stage 0 ; frac → 0 = casi roto → último stage.
        float frac = Mathf.Clamp01(currentHealth / maxHealth);
        int   idx  = Mathf.Clamp(
            Mathf.FloorToInt((1f - frac) * damageStages.Length),
            0, damageStages.Length - 1);
        sr.sprite = damageStages[idx];
    }

    void Break()
    {
        broken = true;

        if (breakEffect != null)
            Instantiate(breakEffect, transform.position, Quaternion.identity);

        if (disableInsteadOfDestroy) gameObject.SetActive(false);
        else                         Destroy(gameObject);
    }
}
