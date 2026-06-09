using UnityEngine;

/// <summary>
/// Guardián — reacciona a Steel Push e Iron Pull como cualquier objeto de metal.
/// Cuando recibe una fuerza alomántica, el AI cede el control brevemente
/// para que el empuje físico sea visible, luego retoma su comportamiento.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(MetalObject))]
public class GuardianAI : MonoBehaviour
{
    [Header("Detección")]
    public float detectionRange = 7f;

    [Header("Movimiento")]
    public float moveSpeed   = 1.8f;
    public float patrolSpeed = 1f;

    [Header("Patrulla — asignar en Inspector")]
    public Transform patrolPointA;
    public Transform patrolPointB;
    public float     patrolReachDistance = 0.8f;

    [Header("Ataque")]
    public float attackRange    = 1.4f;
    public float attackWidth    = 1.6f;
    public float attackHeight   = 1.4f;
    public float attackDamage   = 20f;
    public float attackCooldown = 1.8f;
    public LayerMask playerLayer;

    [Header("Reacción alomántica")]
    [Tooltip("Segundos que el AI cede el control al recibir un empuje/jale alomántico.")]
    public float pushedDuration = 0.3f;

    [Header("Estado (solo lectura)")]
    [SerializeField] private string currentState = "Idle";
    [SerializeField] private float  pushedTimer  = 0f;

    // Compatibilidad con GuardiaAnimator
    public bool  IsStunned    => currentState == "Pushed";
    public bool  IsChasing    => currentState == "Chase";
    public bool  IsAttacking  => currentState == "Attack";
    public bool  IsPatrolling => currentState == "Patrol";
    public float AttackCooldown => attackCooldown;

    private Transform      player;
    private Rigidbody2D    rb;
    private EnemyHealth    health;
    private SpriteRenderer sr;
    private MetalObject    metalObj;
    private HitFlash       hitFlash;

    private Transform currentPatrolTarget;
    private float     attackTimer = 0f;
    private bool      facingRight = true;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Awake()
    {
        rb       = GetComponent<Rigidbody2D>();
        health   = GetComponent<EnemyHealth>();
        sr       = GetComponent<SpriteRenderer>();
        metalObj = GetComponent<MetalObject>();
        hitFlash = GetComponent<HitFlash>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
        else Debug.LogWarning("[GuardianAI] No se encontró objeto con tag 'Player'.");

        rb.freezeRotation = true;

        if (patrolPointA != null) currentPatrolTarget = patrolPointA;

        if (metalObj != null) metalObj.OnAllomancyForce += OnAllomancyForceReceived;
        else Debug.LogError("[GuardianAI] MetalObject no encontrado.");

        health.OnDeath.AddListener(OnDied);
    }

    void OnDestroy()
    {
        if (metalObj != null) metalObj.OnAllomancyForce -= OnAllomancyForceReceived;
    }

    void Update()
    {
        if (health.IsDead || player == null) return;

        if (attackTimer > 0) attackTimer -= Time.deltaTime;

        // Cediendo control a la física
        if (currentState == "Pushed")
        {
            pushedTimer -= Time.deltaTime;
            if (pushedTimer <= 0f)
                currentState = "Idle";
            return; // no sobrescribir la velocidad — dejar que la física actúe
        }

        float dist = Vector2.Distance(transform.position, player.position);

        if      (dist <= attackRange)    StateAttack();
        else if (dist <= detectionRange) StateChase();
        else                             StatePatrol();
    }

    // ── Reacción alomántica ───────────────────────────────────────────────────

    void OnAllomancyForceReceived(float magnitude)
    {
        if (health.IsDead) return;

        // Cada empuje reinicia el timer — el AI cede mientras le sigan aplicando fuerza
        currentState = "Pushed";
        pushedTimer  = pushedDuration;
        hitFlash?.Flash();
    }

    // ── Estados ───────────────────────────────────────────────────────────────

    void StatePatrol()
    {
        if (patrolPointA == null || patrolPointB == null)
        {
            currentState      = "Idle";
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        currentState = "Patrol";

        float distToTarget = Vector2.Distance(transform.position, currentPatrolTarget.position);
        if (distToTarget <= patrolReachDistance)
            currentPatrolTarget = currentPatrolTarget == patrolPointA ? patrolPointB : patrolPointA;

        float moveDir = Mathf.Sign(currentPatrolTarget.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(moveDir * patrolSpeed, rb.linearVelocity.y);
        Flip(moveDir > 0);
    }

    void StateChase()
    {
        currentState = "Chase";
        float moveDir = Mathf.Sign(player.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(moveDir * moveSpeed, rb.linearVelocity.y);
        Flip(moveDir > 0);
    }

    void StateAttack()
    {
        currentState      = "Attack";
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        if (attackTimer > 0) return;

        Vector2    dir       = facingRight ? Vector2.right : Vector2.left;
        Vector2    boxCenter = (Vector2)transform.position + dir * (attackRange * 0.5f);
        Collider2D hit       = Physics2D.OverlapBox(boxCenter, new Vector2(attackWidth, attackHeight), 0f, playerLayer);

        if (hit != null)
            hit.GetComponent<PlayerHealth>()?.TakeDamage(attackDamage, transform);

        attackTimer = attackCooldown;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    void Flip(bool toRight)
    {
        facingRight = toRight;
        if (sr != null) sr.flipX = !toRight;
    }

    void OnDied()
    {
        currentState      = "Dead";
        rb.linearVelocity = Vector2.zero;
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Vector2 dir    = (Application.isPlaying ? facingRight : true) ? Vector2.right : Vector2.left;
        Vector2 center = (Vector2)transform.position + dir * (attackRange * 0.5f);
        Gizmos.color   = Color.red;
        Gizmos.DrawWireCube(center, new Vector3(attackWidth, attackHeight, 0));

        if (patrolPointA != null && patrolPointB != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(patrolPointA.position, 0.2f);
            Gizmos.DrawSphere(patrolPointB.position, 0.2f);
            Gizmos.DrawLine(patrolPointA.position, patrolPointB.position);
        }
    }
}
