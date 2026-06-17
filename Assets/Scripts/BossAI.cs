using UnityEngine;

/// <summary>
/// Jefe final. Ataca cuerpo a cuerpo (fuerte) y a distancia (proyectiles).
/// INMUNE A ALOMANCIA: no lleva MetalObject, así que Steel Push / Iron Pull
/// no lo afectan ni lo detectan como objetivo.
///
/// Estados según distancia al jugador:
///   melee (cerca) → ranged (media, dispara avanzando) → chase → idle
///
/// ─── Setup en Unity ───────────────────────────────────────────────────────
/// 1. GameObject del jefe con: Rigidbody2D, Collider2D, SpriteRenderer,
///    EnemyHealth (sube su Max Health, ej. 400), HitFlash (opcional) y este
///    script. NO le pongas MetalObject (eso lo haría empujable).
/// 2. Asignar 'playerLayer' = layer del jugador.
/// 3. Crear un prefab de proyectil (ver BossProjectile) y asignarlo en
///    'projectilePrefab'. 'firePoint' = un hijo vacío frente al jefe (opcional).
/// ─────────────────────────────────────────────────────────────────────────
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyHealth))]
public class BossAI : MonoBehaviour
{
    [Header("Detección")]
    public float detectionRange = 14f;

    [Header("Movimiento")]
    public float moveSpeed = 2.5f;

    [Header("Melee (golpe fuerte)")]
    public float meleeRange    = 2f;
    public float meleeWidth    = 2.4f;
    public float meleeHeight   = 2f;
    public float meleeDamage   = 30f;
    public float meleeCooldown = 1.5f;
    public LayerMask playerLayer;

    [Tooltip("Retraso (seg) desde que empieza el ataque hasta que conecta el daño. " +
             "Súbelo/bájalo hasta que coincida con el frame del swing de la guadaña.")]
    public float meleeHitDelay = 0.3f;

    [Tooltip("Si está activo, el daño NO se aplica por tiempo: lo dispara un " +
             "Animation Event que llame a DoMeleeDamage() en el frame del impacto.")]
    public bool meleeDamageByAnimationEvent = false;

    [Header("Proyectiles (a distancia)")]
    public GameObject projectilePrefab;
    [Tooltip("Punto desde donde salen los proyectiles. Si está vacío, sale del centro del jefe.")]
    public Transform  firePoint;
    [Tooltip("Distancia a la que empieza a disparar (debe ser mayor que meleeRange).")]
    public float rangedRange     = 11f;
    public float rangedCooldown  = 2.2f;
    public float projectileSpeed = 9f;
    [Tooltip("Cuántos proyectiles por ráfaga.")]
    public int   projectilesPerVolley = 1;
    [Tooltip("Dispersión en grados si la ráfaga tiene varios proyectiles.")]
    public float volleySpread = 0f;

    [Tooltip("Duración del cast (seg). Debe coincidir con tu animación 'cast'. " +
             "Es la ÚNICA ventana en que el jefe es inmune a proyectiles. " +
             "Entre casts (cada 'rangedCooldown') es vulnerable.")]
    public float castDuration = 0.7f;

    [Header("Enrage (opcional)")]
    [Tooltip("Por debajo de esta fracción de vida el jefe se acelera. 0 = desactivado.")]
    [Range(0f, 1f)] public float enrageThreshold = 0f;
    public float enrageSpeedMult    = 1.4f;
    public float enrageCooldownMult = 0.6f;

    [Header("Estado (solo lectura)")]
    [SerializeField] private string currentState = "Idle";

    // Para un futuro BossAnimator
    public bool IsChasing => currentState == "Chase";
    public bool IsMelee   => currentState == "Melee";
    public bool IsRanged  => currentState == "Ranged";
    public bool IsCasting => isCasting;
    public bool IsEnraged { get; private set; }

    private Transform      player;
    private Rigidbody2D    rb;
    private EnemyHealth    health;
    private SpriteRenderer sr;
    private bool  facingRight = true;
    private float meleeTimer;
    private float rangedTimer;
    private bool  isCasting;
    private float castTimer;

    void Awake()
    {
        rb     = GetComponent<Rigidbody2D>();
        health = GetComponent<EnemyHealth>();
        sr     = GetComponent<SpriteRenderer>();
        rb.freezeRotation = true;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        else Debug.LogWarning("[BossAI] No se encontró objeto con tag 'Player'.");

        health.OnDeath.AddListener(OnDied);
    }

    void Update()
    {
        if (health.IsDead || player == null) return;

        if (meleeTimer  > 0) meleeTimer  -= Time.deltaTime;
        if (rangedTimer > 0) rangedTimer -= Time.deltaTime;

        // Cast en curso: el jefe se queda quieto e inmune hasta que termina.
        if (isCasting)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            castTimer -= Time.deltaTime;
            if (castTimer <= 0f)
            {
                isCasting = false;
                health.projectileImmune = false;
            }
            return;
        }

        if (enrageThreshold > 0f && !IsEnraged && health.HealthFraction <= enrageThreshold)
            IsEnraged = true;

        FacePlayer();

        float dist = Vector2.Distance(transform.position, player.position);

        if      (dist <= meleeRange)     StateMelee();
        else if (dist <= rangedRange)    StateRanged();
        else if (dist <= detectionRange) StateChase();
        else                             StateIdle();
    }

    private float SpeedMult => IsEnraged ? enrageSpeedMult    : 1f;
    private float CdMult    => IsEnraged ? enrageCooldownMult : 1f;

    void StateIdle()
    {
        currentState = "Idle";
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    void StateChase()
    {
        currentState = "Chase";
        float dir = Mathf.Sign(player.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(dir * moveSpeed * SpeedMult, rb.linearVelocity.y);
    }

    void StateRanged()
    {
        currentState = "Ranged";

        // Entre casts: avanza lento, manteniendo distancia media (VULNERABLE).
        float dir = Mathf.Sign(player.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(dir * moveSpeed * 0.4f * SpeedMult, rb.linearVelocity.y);

        // Cuando toca, inicia un cast puntual (inmune solo durante 'castDuration').
        if (rangedTimer <= 0f)
            StartCast();
    }

    /// <summary>Inicia un cast: inmune a proyectiles, lanza la ráfaga, y queda
    /// en cooldown. La inmunidad dura solo 'castDuration'.</summary>
    void StartCast()
    {
        isCasting   = true;
        castTimer   = castDuration;
        rangedTimer = rangedCooldown * CdMult;
        health.projectileImmune = true;
        FireVolley();
    }

    void StateMelee()
    {
        currentState = "Melee";
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        if (meleeTimer > 0f) return;

        // Inicia el ataque ahora; el daño conecta más tarde (sincronizado con el swing).
        meleeTimer = meleeCooldown * CdMult;

        if (!meleeDamageByAnimationEvent)
            Invoke(nameof(DoMeleeDamage), meleeHitDelay);
    }

    /// <summary>
    /// Aplica el daño del golpe melee. Se llama tras 'meleeHitDelay', o desde un
    /// Animation Event en el frame del impacto (si 'meleeDamageByAnimationEvent').
    /// </summary>
    public void DoMeleeDamage()
    {
        if (health.IsDead) return;

        Vector2    dir = facingRight ? Vector2.right : Vector2.left;
        Vector2    c   = (Vector2)transform.position + dir * (meleeRange * 0.5f);
        Collider2D hit = Physics2D.OverlapBox(c, new Vector2(meleeWidth, meleeHeight), 0f, playerLayer);

        if (hit != null)
            hit.GetComponent<PlayerHealth>()?.TakeDamage(meleeDamage, transform);
    }

    void FireVolley()
    {
        if (projectilePrefab == null) return;

        Vector2 origin   = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
        Vector2 toPlayer = ((Vector2)player.position - origin).normalized;
        float   baseAng  = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;

        int   n     = Mathf.Max(1, projectilesPerVolley);
        float start = baseAng - volleySpread * 0.5f;
        float step  = n > 1 ? volleySpread / (n - 1) : 0f;

        for (int i = 0; i < n; i++)
        {
            float   ang = n == 1 ? baseAng : start + step * i;
            Vector2 d   = new Vector2(Mathf.Cos(ang * Mathf.Deg2Rad), Mathf.Sin(ang * Mathf.Deg2Rad));

            GameObject proj = Instantiate(projectilePrefab, origin, Quaternion.identity);
            BossProjectile bp = proj.GetComponent<BossProjectile>();
            if (bp != null)
                bp.Launch(d, projectileSpeed);
            else
            {
                Rigidbody2D prb = proj.GetComponent<Rigidbody2D>();
                if (prb != null) prb.linearVelocity = d * projectileSpeed;
            }
        }
    }

    void FacePlayer()
    {
        facingRight = player.position.x > transform.position.x;
        if (sr != null) sr.flipX = !facingRight;
    }

    void OnDied()
    {
        currentState = "Dead";
        isCasting    = false;
        rb.linearVelocity = Vector2.zero;
        if (health != null) health.projectileImmune = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, rangedRange);

        Vector2 dir = (Application.isPlaying ? facingRight : true) ? Vector2.right : Vector2.left;
        Vector2 c   = (Vector2)transform.position + dir * (meleeRange * 0.5f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(c, new Vector3(meleeWidth, meleeHeight, 0f));
    }
}
