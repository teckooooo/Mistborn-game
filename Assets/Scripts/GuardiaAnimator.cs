using UnityEngine;

[RequireComponent(typeof(Animator))]
public class GuardiaAnimator : MonoBehaviour
{
    private static readonly int ParamSpeed = Animator.StringToHash("Speed");

    // Nombres de estados del Animator Controller del Guardian
    private const string ClipIdle   = "Idle";
    private const string ClipRun    = "run_enemy2";
    private const string ClipAttack = "attack_enemy2";
    private const string ClipHit    = "Idle";
    private const string ClipDeath  = "death_enemy2";

    private Animator    anim;
    private GuardianAI  ai;
    private EnemyHealth health;

    private bool deathPlayed   = false;
    private bool attackPlaying = false;
    private bool wasStunned    = false;

    void Start()
    {
        anim   = GetComponent<Animator>();
        ai     = GetComponent<GuardianAI>();
        health = GetComponent<EnemyHealth>();

        if (ai == null)
            Debug.LogError("[GuardiaAnimator] GuardianAI no encontrado.");

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

        // Stun alomántico — entra en hit animation
        if (ai.IsStunned)
        {
            if (!wasStunned)
            {
                wasStunned    = true;
                attackPlaying = false;
                CancelInvoke(nameof(EndAttack));
                anim.Play(ClipHit, 0, 0f);
            }
            return;
        }
        wasStunned = false;

        // Ataque — una sola vez por ciclo
        if (ai.IsAttacking && !attackPlaying)
        {
            attackPlaying = true;
            anim.Play(ClipAttack, 0, 0f);
            Invoke(nameof(EndAttack), ai.AttackCooldown);
            return;
        }

        if (attackPlaying) return;

        // Movimiento
        bool moving = ai.IsChasing || ai.IsPatrolling;
        anim.SetFloat(ParamSpeed, moving ? 1f : 0f);
    }

    void EndAttack()
    {
        attackPlaying = false;
        if (!health.IsDead && !ai.IsStunned)
            anim.Play(ClipIdle, 0, 0f);
    }

    void OnHit()
    {
        if (health.IsDead || attackPlaying || ai.IsStunned) return;
        anim.Play(ClipHit, 0, 0f);
    }

    void OnDead()
    {
        if (deathPlayed) return;
        deathPlayed = true;
        CancelInvoke(nameof(EndAttack));
        anim.Play(ClipDeath, 0, 0f);
    }
}
