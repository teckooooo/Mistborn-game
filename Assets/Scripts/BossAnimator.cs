using UnityEngine;

/// <summary>
/// Conecta el BossAI con el Animator del jefe. Reproduce los clips según el
/// estado del jefe (idle / correr / melee / proyectil / golpeado / muerte).
///
/// Pon en los campos los NOMBRES EXACTOS de los estados de tu Animator
/// Controller (los clips que creaste a partir de los frames Fullmain_*).
/// </summary>
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BossAI))]
[RequireComponent(typeof(EnemyHealth))]
public class BossAnimator : MonoBehaviour
{
    [Header("Nombres de los estados del Animator")]
    public string idleClip   = "Idle";
    public string runClip    = "Run";
    public string meleeClip  = "Melee";
    public string rangedClip = "Ranged";
    public string hitClip    = "Hit";
    public string deathClip  = "Death";

    [Tooltip("Si no creaste animación de 'golpeado', desactívalo (usará idle/run).")]
    public bool useHitAnim = true;

    private Animator    anim;
    private BossAI      ai;
    private EnemyHealth health;

    private string current      = "";
    private bool   attackPlaying = false;
    private bool   deathPlayed   = false;

    void Start()
    {
        anim   = GetComponent<Animator>();
        ai     = GetComponent<BossAI>();
        health = GetComponent<EnemyHealth>();

        health.OnDeath.RemoveListener(OnDead);
        health.OnDeath.AddListener(OnDead);
        health.OnDamaged.AddListener(OnHit);
    }

    void OnDestroy()
    {
        if (health != null)
        {
            health.OnDeath.RemoveListener(OnDead);
            health.OnDamaged.RemoveListener(OnHit);
        }
    }

    void Update()
    {
        if (health.IsDead) return;

        // Cast — mientras el jefe castea (BossAI controla duración e inmunidad).
        if (ai.IsCasting)
        {
            Play(rangedClip);
            return;
        }

        // Melee — se reproduce una vez por golpe.
        if (ai.IsMelee && !attackPlaying)
        {
            attackPlaying = true;
            Play(meleeClip, restart: true);
            Invoke(nameof(EndAttack), Mathf.Min(ai.meleeCooldown, 1f));
            return;
        }

        if (attackPlaying) return;

        // Locomoción (en Ranged entre casts también avanza → run).
        Play(ai.IsChasing || ai.IsRanged ? runClip : idleClip);
    }

    void EndAttack() => attackPlaying = false;

    void OnHit()
    {
        if (!useHitAnim || health.IsDead || attackPlaying) return;
        Play(hitClip, restart: true);
    }

    void OnDead()
    {
        if (deathPlayed) return;
        deathPlayed = true;
        CancelInvoke(nameof(EndAttack));
        Play(deathClip, restart: true);
    }

    /// <summary>Reproduce un estado solo si cambió (o si restart = true).</summary>
    void Play(string clip, bool restart = false)
    {
        if (string.IsNullOrEmpty(clip)) return;
        if (!restart && clip == current) return;
        current = clip;
        anim.Play(clip, 0, 0f);
    }
}
