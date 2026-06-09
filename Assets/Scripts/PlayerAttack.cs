using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Ataque cuerpo a cuerpo de Fantasma.
/// Click izquierdo → golpe en dirección que mira → daña enemigos en rango.
/// Si Pewter está activo, el daño se multiplica.
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    [Header("Ataque")]
    public float attackDamage   = 15f;
    public float attackRange    = 1.2f;
    public float attackCooldown = 0.4f;

    [Header("Pewter — bonus de daño")]
    [Tooltip("Multiplicador de daño cuando Pewter está activo")]
    public float pewterDamageMultiplier = 2f;

    [Header("Detección")]
    [Tooltip("Layer de los enemigos — crear un Layer 'Enemy' en Unity")]
    public LayerMask enemyLayer;

    [Header("Estado (solo lectura)")]
    [SerializeField] private float attackTimer;

    // Componentes
    private MetalReserve   reserve;
    private SpriteRenderer sr;
    private PlayerAnimator playerAnim;

    // ── Inicialización ────────────────────────────────────────────────────────

    void Start()
    {
        reserve    = GetComponent<MetalReserve>();
        sr         = GetComponent<SpriteRenderer>();
        playerAnim = GetComponent<PlayerAnimator>();
    }

    // ── Update ────────────────────────────────────────────────────────────────

    void Update()
    {
        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;

        if (PauseMenu.IsPaused) return;

        // Ignorar click si el cursor está sobre un elemento de UI
        bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        if (Input.GetMouseButtonDown(0) && attackTimer <= 0 && !overUI)
            Attack();
    }

    // ── Ataque ────────────────────────────────────────────────────────────────

    void Attack()
    {
        attackTimer = attackCooldown;

        bool facingRight  = sr != null ? !sr.flipX : true;
        Vector2 direction = facingRight ? Vector2.right : Vector2.left;

        // Box frente al jugador — solo cubre el lado que mira
        Vector2 boxCenter = (Vector2)transform.position + direction * (attackRange * 0.5f);
        Vector2 boxSize   = new Vector2(attackRange, 1.2f);

        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f, enemyLayer);

        bool  pewterOn    = reserve != null && reserve.PewterActive;
        float multiplier  = pewterOn ? pewterDamageMultiplier : 1f;
        float finalDamage = attackDamage * multiplier;

        foreach (Collider2D hit in hits)
        {
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(finalDamage);
                Debug.Log($"[PlayerAttack] Golpeó a {hit.name} por {finalDamage:F1}" +
                          $"{(pewterOn ? " (Pewter activo)" : "")}");
            }
        }

        playerAnim?.TriggerAttack();
    }

    // ── Gizmos — visualizar rango de ataque en el editor ─────────────────────

    void OnDrawGizmosSelected()
    {
        SpriteRenderer s  = GetComponent<SpriteRenderer>();
        bool facingRight   = s != null ? !s.flipX : true;
        Vector2 direction  = facingRight ? Vector2.right : Vector2.left;
        Vector2 boxCenter  = (Vector2)transform.position + direction * (attackRange * 0.5f);
        Vector2 boxSize    = new Vector2(attackRange, 1.2f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(boxCenter, boxSize);
    }
}