using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float jumpForce = 14f;
    public float maxSpeed  = 20f;

    [Header("Coin Throw")]
    public GameObject coinPrefab;
    public Transform  coinSpawn;
    public float      coinThrowForce = 20f;

    private Rigidbody2D    rb;
    private MetalReserve   reserve;
    private bool           isGrounded;
    private SpriteRenderer sr;
    private bool           facingRight = true;
    private GameObject     currentGround;

    public bool IsGrounded => isGrounded;

    void Start()
    {
        rb      = GetComponent<Rigidbody2D>();
        sr      = GetComponent<SpriteRenderer>();
        reserve = GetComponent<MetalReserve>();
    }

    void Update()
    {
        Move();
        Jump();
        ThrowCoin();
        ClampSpeed();
    }

    void Move()
    {
        float move = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(move * moveSpeed, rb.linearVelocity.y);

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        bool mouseIsRight  = mouseWorld.x > transform.position.x;

        if (move > 0)                        Flip(true);
        else if (move < 0)                   Flip(false);
        else if (mouseIsRight && !facingRight) Flip(true);
        else if (!mouseIsRight && facingRight) Flip(false);
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
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    void ThrowCoin()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (reserve != null && !reserve.HasSteel) { Debug.Log("[PlayerController] Sin Acero para lanzar moneda."); return; }
            reserve?.ConsumeCoin();
            GameObject coin = Instantiate(coinPrefab, coinSpawn.position, Quaternion.identity);

            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;
            Vector2 dir = (mouseWorld - coinSpawn.position).normalized;

            Rigidbody2D coinRb = coin.GetComponent<Rigidbody2D>();
            if (coinRb != null)
                coinRb.AddForce(dir * coinThrowForce, ForceMode2D.Impulse);
        }
    }

    void ClampSpeed()
    {
        if (rb.linearVelocity.magnitude > maxSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        string tag = collision.gameObject.tag;
        bool validSurface = tag == "Ground" || tag == "Coin" || tag == "Metal" ||
                            tag == "Muro"   || tag == "Piso" || tag == "Pasto" ||
                            tag == "Cristal";
        if (!validSurface) return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.3f)
            {
                isGrounded    = true;
                currentGround = collision.gameObject;
                Debug.Log($"[PlayerController] Parado sobre: {collision.gameObject.name} (Tag: {collision.gameObject.tag})");
                break;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject == currentGround)
        {
            isGrounded    = false;
            currentGround = null;
        }
    }
}