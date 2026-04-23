using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyHealth))]
public class CultistAI : MonoBehaviour
{
    [Header("Detección")]
    public float detectionRange = 6f;

    [Header("Movimiento")]
    public float moveSpeed   = 2.5f;
    public float patrolSpeed = 1.5f;

    [Header("Patrulla — asignar en Inspector")]
    public Transform patrolPointA;
    public Transform patrolPointB;
    public float     patrolReachDistance = 1.0f;

    [Header("Ataque")]
    public float attackRange    = 0.8f;
    public float attackWidth    = 1.0f;   // ancho del OverlapBox
    public float attackHeight   = 1.2f;   // alto del OverlapBox
    public float attackDamage   = 10f;
    public float attackCooldown = 1.2f;
    public LayerMask playerLayer;

    [Header("Estado (solo lectura)")]
    [SerializeField] private string currentState = "Idle";

    public bool IsChasing    => currentState == "Chase";
    public bool IsAttacking  => currentState == "Attack";
    public bool IsPatrolling => currentState == "Patrol";
    public float AttackCooldown => attackCooldown;

    private Transform      player;
    private Rigidbody2D    rb;
    private EnemyHealth    health;
    private SpriteRenderer sr;
    private Transform      currentPatrolTarget;
    private float          attackTimer = 0f;
    private bool           facingRight = true;

    void Awake()
    {
        rb     = GetComponent<Rigidbody2D>();
        health = GetComponent<EnemyHealth>();
        sr     = GetComponent<SpriteRenderer>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning("[CultistAI] No se encontró objeto con tag 'Player'.");

        rb.freezeRotation = true;

        if (patrolPointA != null)
            currentPatrolTarget = patrolPointA;
    }

    void Update()
    {
        if (health.IsDead || player == null) return;

        if (attackTimer > 0) attackTimer -= Time.deltaTime;

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if (distToPlayer <= attackRange)
            StateAttack();
        else if (distToPlayer <= detectionRange)
            StateChase();
        else
            StatePatrol();
    }

    void StatePatrol()
    {
        if (patrolPointA == null || patrolPointB == null)
        {
            currentState      = "Idle";
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        currentState = "Patrol";

        float distToTarget = Vector2.Distance(transform.position, currentPatrolTarget.position);
        if (distToTarget <= patrolReachDistance)
            currentPatrolTarget = currentPatrolTarget == patrolPointA ? patrolPointB : patrolPointA;

        float dir     = currentPatrolTarget.position.x - transform.position.x;
        float moveDir = Mathf.Sign(dir);

        rb.linearVelocity = new Vector2(moveDir * patrolSpeed, rb.linearVelocity.y);

        Flip(moveDir > 0);
    }

    void StateChase()
    {
        currentState = "Chase";

        float dir     = player.position.x - transform.position.x;
        float moveDir = Mathf.Sign(dir);

        rb.linearVelocity = new Vector2(moveDir * moveSpeed, rb.linearVelocity.y);

        Flip(moveDir > 0);
    }

    void StateAttack()
    {
        currentState      = "Attack";
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        if (attackTimer > 0) return;

        // OverlapBox direccional — igual que PlayerAttack
        Vector2 direction = facingRight ? Vector2.right : Vector2.left;
        Vector2 boxCenter = (Vector2)transform.position + direction * (attackRange * 0.5f);
        Vector2 boxSize   = new Vector2(attackWidth, attackHeight);

        Collider2D hit = Physics2D.OverlapBox(boxCenter, boxSize, 0f, playerLayer);
        if (hit != null)
        {
            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(attackDamage, transform);
        }

        attackTimer = attackCooldown;
        Debug.Log($"[CultistAI] Atacó por {attackDamage} daño.");
    }

    void Flip(bool toRight)
    {
        facingRight = toRight;
        if (sr != null) sr.flipX = !toRight;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Visualizar OverlapBox de ataque
        Vector2 direction = facingRight ? Vector2.right : Vector2.left;
        Vector2 boxCenter = (Vector2)transform.position + direction * (attackRange * 0.5f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(boxCenter, new Vector3(attackWidth, attackHeight, 0));

        if (patrolPointA != null && patrolPointB != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(patrolPointA.position, 0.2f);
            Gizmos.DrawSphere(patrolPointB.position, 0.2f);
            Gizmos.DrawLine(patrolPointA.position, patrolPointB.position);
        }
    }
}