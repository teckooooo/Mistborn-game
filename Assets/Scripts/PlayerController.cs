using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float jumpForce = 14f;
    public float maxSpeed  = 20f;

    [Header("Dash")]
    public float dashForce    = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;


    [Header("Coin Throw")]
    public GameObject coinPrefab;
    public Transform  coinSpawn;
    public float      coinThrowForce = 20f;

    private Rigidbody2D     rb;
    private MetalReserve    reserve;
    private PlayerInventory inventory;
    private SpriteRenderer  sr;
    private Animator        anim;

    private bool       isGrounded;
    private bool       facingRight       = true;
    private GameObject currentGround;
    private bool       isDashing         = false;
    private float      dashTimer         = 0f;
    private float      dashCooldownTimer = 0f;

    public bool IsGrounded => isGrounded;
    public bool IsDashing  => isDashing;

    void Start()
    {
        rb        = GetComponent<Rigidbody2D>();
        sr        = GetComponent<SpriteRenderer>();
        reserve   = GetComponent<MetalReserve>();
        inventory = GetComponent<PlayerInventory>();
        anim      = GetComponent<Animator>();

    }

    void Update()
    {
        if (PauseMenu.IsPaused) return;

        Dash();
        if (!isDashing) Move();
        Jump();
        ThrowCoin();
        ClampSpeed();

        if (dashCooldownTimer > 0) dashCooldownTimer -= Time.deltaTime;

        if (dashTimer > 0)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
            {
                isDashing = false;
                if (anim != null)
                    anim.Play("idle", 0, 0f);
            }
        }
    }

    void Move()
    {
        float move  = Input.GetAxis("Horizontal");
        float speed = moveSpeed;
        rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        bool mouseIsRight  = mouseWorld.x > transform.position.x;

        if      (move > 0)                      Flip(true);
        else if (move < 0)                      Flip(false);
        else if (mouseIsRight  && !facingRight) Flip(true);
        else if (!mouseIsRight &&  facingRight) Flip(false);
    }

    void Flip(bool toRight)
    {
        facingRight = toRight;
        if (sr != null) sr.flipX = !toRight;
        if (coinSpawn != null)
        {
            Vector3 pos = coinSpawn.localPosition;
            pos.x = Mathf.Abs(pos.x) * (toRight ? 1f : -1f);
            coinSpawn.localPosition = pos;
        }
    }

    void Jump()
    {
        if (!Input.GetKeyDown(KeyCode.Space) || !isGrounded) return;
        float force = jumpForce;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
    }

    void Dash()
    {
        if (dashCooldownTimer > 0 || isDashing) return;

        bool shiftDown = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
        bool shiftHeld = Input.GetKey(KeyCode.LeftShift)     || Input.GetKey(KeyCode.RightShift);
        bool leftDown  = Input.GetKeyDown(KeyCode.A)         || Input.GetKeyDown(KeyCode.LeftArrow);
        bool rightDown = Input.GetKeyDown(KeyCode.D)         || Input.GetKeyDown(KeyCode.RightArrow);
        bool leftHeld  = Input.GetKey(KeyCode.A)             || Input.GetKey(KeyCode.LeftArrow);
        bool rightHeld = Input.GetKey(KeyCode.D)             || Input.GetKey(KeyCode.RightArrow);

        float dashDir = 0f;
        if      (shiftDown && rightHeld) dashDir =  1f;
        else if (shiftDown && leftHeld)  dashDir = -1f;
        else if (shiftHeld && rightDown) dashDir =  1f;
        else if (shiftHeld && leftDown)  dashDir = -1f;

        if (dashDir == 0f) return;

        isDashing         = true;
        dashTimer         = dashDuration;
        dashCooldownTimer = dashCooldown;

        if (anim != null)
            anim.Play(isGrounded ? "Dash" : "AirDash", 0, 0f);

        Flip(dashDir > 0);
        rb.linearVelocity = new Vector2(dashDir * dashForce, rb.linearVelocity.y);
    }

    void ThrowCoin()
    {
        if (!Input.GetKeyDown(KeyCode.E)) return;
        if (inventory != null && !inventory.HasCoins) { Debug.Log("[PlayerController] Sin monedas."); return; }
        if (reserve != null && !reserve.HasSteel)     { Debug.Log("[PlayerController] Sin Acero."); return; }

        inventory?.ConsumeCoin();
        reserve?.ConsumeCoin();

        GameObject coin = Instantiate(coinPrefab, coinSpawn.position, Quaternion.identity);
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        Vector2 dir = (mouseWorld - coinSpawn.position).normalized;
        Rigidbody2D coinRb = coin.GetComponent<Rigidbody2D>();
        if (coinRb != null) coinRb.AddForce(dir * coinThrowForce, ForceMode2D.Impulse);
    }

    void ClampSpeed()
    {
        if (!isDashing && rb.linearVelocity.magnitude > maxSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        string tag = collision.gameObject.tag;
        bool validSurface = tag == "Ground" || tag == "Coin"  || tag == "Metal" ||
                            tag == "Muro"   || tag == "Piso"  || tag == "Pasto" || tag == "Cristal";
        if (!validSurface) return;
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.3f) { isGrounded = true; currentGround = collision.gameObject; break; }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject == currentGround) { isGrounded = false; currentGround = null; }
    }
}