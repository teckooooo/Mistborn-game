using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CultistAnimator : MonoBehaviour
{
    private static readonly int ParamSpeed = Animator.StringToHash("Speed");

    private Animator    anim;
    private CultistAI   ai;
    private EnemyHealth health;

    private bool deathPlayed    = false;
    private bool attackPlaying  = false;

    void Start()
    {
        anim   = GetComponent<Animator>();
        ai     = GetComponent<CultistAI>();
        health = GetComponent<EnemyHealth>();

        health.OnDeath.RemoveListener(OnDead);
        health.OnDeath.AddListener(OnDead);
    }

    void OnDestroy()
    {
        if (health != null)
            health.OnDeath.RemoveListener(OnDead);
    }

    void Update()
    {
        if (health.IsDead) return;

        // Atacando — Play() directo, una sola vez por ciclo
        if (ai.IsAttacking && !attackPlaying)
        {
            attackPlaying = true;
            anim.Play("attack_enemy1", 0, 0f);
            // Volver a idle después de la duración del clip
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

    void OnDead()
    {
        if (deathPlayed) return;
        deathPlayed = true;
        CancelInvoke(nameof(EndAttack));
        anim.Play("death_enemy1", 0, 0f);
    }
}