using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CultistAnimator : MonoBehaviour
{
    private static readonly int ParamSpeed = Animator.StringToHash("Speed");

    private Animator    anim;
    private CultistAI   ai;
    private EnemyHealth health;

    private bool deathPlayed   = false;
    private bool attackPlaying = false;

    void Start()
    {
        anim   = GetComponent<Animator>();
        ai     = GetComponent<CultistAI>();
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

        // Atacando — Play() directo, una sola vez por ciclo
        if (ai.IsAttacking && !attackPlaying)
        {
            attackPlaying = true;
            anim.Play("attack_enemy1", 0, 0f);
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
        if (!health.IsDead)
            anim.Play("idle", 0, 0f);
    }

    void OnHit()
    {
        if (health.IsDead || attackPlaying) return;
        anim.Play("hit", 0, 0f);
    }

    void OnDead()
    {
        if (deathPlayed) return;
        deathPlayed = true;
        CancelInvoke(nameof(EndAttack));
        anim.Play("death_enemy1", 0, 0f);
    }
}