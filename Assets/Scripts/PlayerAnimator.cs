using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerAnimator : MonoBehaviour
{
    private static readonly int ParamSpeed      = Animator.StringToHash("Speed");
    private static readonly int ParamIsGrounded = Animator.StringToHash("IsGrounded");
    private static readonly int ParamIsJumping  = Animator.StringToHash("IsJumping");
    private static readonly int ParamIsFalling  = Animator.StringToHash("IsFalling");

    private Animator         anim;
    private Rigidbody2D      rb;
    private PlayerController pc;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb   = GetComponent<Rigidbody2D>();
        pc   = GetComponent<PlayerController>();
    }

    void Update()
    {
        float speedX    = Mathf.Abs(rb.linearVelocity.x);
        float velocityY = rb.linearVelocity.y;
        bool  grounded  = pc.IsGrounded;

        anim.SetFloat(ParamSpeed,      speedX);
        anim.SetBool(ParamIsGrounded,  grounded);
        anim.SetBool(ParamIsJumping,   !grounded && velocityY > 0.1f);
        anim.SetBool(ParamIsFalling, !grounded && velocityY < -0.5f);
    }
}