using UnityEngine;

public class Coin : MonoBehaviour
{
    [HideInInspector] public bool beingPulled     = false;
    [HideInInspector] public bool duraluminPulled = false;

    [Header("Colección")]
    public float collectRadius = 1f;

    [Header("Daño a enemigos")]
    [Tooltip("Daño base que hace la moneda al impactar un enemigo")]
    public float impactDamage = 20f;
    [Tooltip("Velocidad mínima para que el impacto haga daño")]
    public float minSpeedForDamage = 3f;

    [Header("Superficies de anclaje")]
    public string[] anchorTags = new string[] { "Ground" };

    [Header("Embed Visual")]
    public float embedFraction = 0.4f;
    [Tooltip("Sorting Order cuando está anclada (detrás de la superficie)")]
    public int embeddedSortingOrder = -1;

    private bool           anchored    = false;
    private bool           unanchoring = false;
    private Transform      playerTransform;
    private Transform      spawnTarget;
    private Vector2        impactNormal = Vector2.up;
    private int            originalSortingOrder;
    private SpriteRenderer sr;
    private bool           collisionIgnored = false;
    private Rigidbody2D    rb;
    private int            enemyLayer;
    private LayerMask      enemyMask;
    private PlayerInventory inventory;

    // ── Inicialización ────────────────────────────────────────────────────────

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalSortingOrder = sr.sortingOrder;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            PlayerController pc = player.GetComponent<PlayerController>();
            spawnTarget = (pc != null && pc.coinSpawn != null) ? pc.coinSpawn : player.transform;
            inventory = player.GetComponent<PlayerInventory>();
        }

        enemyLayer = LayerMask.NameToLayer("Enemy");
        enemyMask  = 1 << enemyLayer;
    }

    // ── Update ────────────────────────────────────────────────────────────────

    void Update()
    {
        Transform target = spawnTarget != null ? spawnTarget : playerTransform;
        if (target == null) return;

        if (beingPulled && !collisionIgnored) { IgnorePlayerCollision(true); collisionIgnored = true; }
        else if (!beingPulled && !anchored && collisionIgnored) { IgnorePlayerCollision(false); collisionIgnored = false; }

        if (beingPulled && !unanchoring)
        {
            float dist = Vector2.Distance(transform.position, target.position);
            if (dist < collectRadius) { inventory?.AddCoins(1); Destroy(gameObject); return; }
        }
    }

    // ── Colisión física ───────────────────────────────────────────────────────

    void IgnorePlayerCollision(bool ignore)
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;
        Collider2D pc = player.GetComponent<Collider2D>();
        Collider2D cc = GetComponent<Collider2D>();
        if (pc != null && cc != null)
            Physics2D.IgnoreCollision(cc, pc, ignore);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (beingPulled && !unanchoring && collision.gameObject.CompareTag("Player"))
        {
            inventory?.AddCoins(1);
            Destroy(gameObject);
            return;
        }

        if (collision.contactCount > 0)
            impactNormal = collision.GetContact(0).normal;

        // Detectar enemigos cercanos con OverlapCircle en el punto de impacto
        // Funciona independientemente de la matriz de Physics 2D
        if (!anchored)
        {
            float speed = rb != null ? rb.linearVelocity.magnitude : 0f;
            if (speed >= minSpeedForDamage)
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(
                    transform.position, 0.4f, enemyMask);

                foreach (Collider2D hit in hits)
                {
                    EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(impactDamage);
                        Debug.Log($"[Coin] Impactó a {hit.name} por {impactDamage:F1} (vel: {speed:F1})");
                    }
                }

                if (hits.Length > 0)
                {
                    Destroy(gameObject);
                    return;
                }
            }
        }

        if (!anchored && IsValidSurface(collision.gameObject.tag))
            Anchor(collision.gameObject);
    }

    bool IsValidSurface(string tag)
    {
        foreach (string t in anchorTags)
            if (t == tag) return true;
        return false;
    }

    void Anchor(GameObject hitObject)
    {
        anchored = true;

        // No desactivar el collider — ignorar colisión con enemigos directamente
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            int enemyLayerIndex = LayerMask.NameToLayer("Enemy");
            foreach (Collider2D enemyCol in Physics2D.OverlapCircleAll(
                transform.position, 50f, 1 << enemyLayerIndex))
            {
                Physics2D.IgnoreCollision(col, enemyCol, true);
            }
        }

        MetalObject metal = GetComponent<MetalObject>();
        if (metal != null)
        {
            Rigidbody2D hitRb = hitObject.GetComponent<Rigidbody2D>();
            metal.anchoredMass = hitRb != null ? hitRb.mass : 9999f;
        }

        if (rb != null) { rb.linearVelocity = Vector2.zero; rb.bodyType = RigidbodyType2D.Static; }

        IgnorePlayerCollision(true);
        collisionIgnored = true;

        EmbedVisual();
    }

    void EmbedVisual()
    {
        Vector2 embedDir   = -impactNormal;
        float spriteHeight = sr != null ? sr.bounds.size.y : 0.1f;
        transform.position += (Vector3)(embedDir * spriteHeight * embedFraction);
        if (sr != null) sr.sortingOrder = embeddedSortingOrder;
    }

    public void Unanchor()
    {
        if (!anchored) return;
        anchored    = false;
        unanchoring = true;

        if (sr != null) sr.sortingOrder = originalSortingOrder;

        MetalObject metal = GetComponent<MetalObject>();
        if (metal != null) metal.anchoredMass = 0f;

        if (rb != null) rb.bodyType = RigidbodyType2D.Dynamic;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Collider2D pc = player.GetComponent<Collider2D>();
            Collider2D cc = GetComponent<Collider2D>();
            if (pc != null && cc != null) Physics2D.IgnoreCollision(cc, pc, true);
        }

        Invoke(nameof(RestoreCollision), 0.15f);
    }

    void RestoreCollision()
    {
        unanchoring = false;
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Collider2D pc = player.GetComponent<Collider2D>();
            Collider2D cc = GetComponent<Collider2D>();
            if (pc != null && cc != null) Physics2D.IgnoreCollision(cc, pc, false);
        }
    }
}