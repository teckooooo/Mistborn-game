using UnityEngine;

public class Coin : MonoBehaviour
{
    [HideInInspector] public bool beingPulled     = false;
    [HideInInspector] public bool duraluminPulled = false;

    [Header("Colección")]
    public float collectRadius = 1f;

    [Header("Superficies de anclaje")]
    public string[] anchorTags = new string[] { "Ground" };

    [Header("Embed Visual")]
    public float embedFraction = 0.4f;
    [Tooltip("Sorting Order cuando está anclada (detrás de la superficie)")]
    public int embeddedSortingOrder = -1;

    private bool        anchored    = false;
    private bool        unanchoring = false;
    private Transform   playerTransform;
    private Transform   spawnTarget;
    private Vector2     impactNormal = Vector2.up;
    private int         originalSortingOrder;
    private SpriteRenderer sr;
    private bool        collisionIgnored = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalSortingOrder = sr.sortingOrder;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            PlayerController pc = player.GetComponent<PlayerController>();
            spawnTarget = (pc != null && pc.coinSpawn != null) ? pc.coinSpawn : player.transform;
        }
    }

    void Update()
    {
        Transform target = spawnTarget != null ? spawnTarget : playerTransform;
        if (target == null) return;

        if (beingPulled && !collisionIgnored) { IgnorePlayerCollision(true); collisionIgnored = true; }
        else if (!beingPulled && collisionIgnored) { IgnorePlayerCollision(false); collisionIgnored = false; }

        if (beingPulled && !unanchoring)
        {
            float dist = Vector2.Distance(transform.position, target.position);
            if (dist < collectRadius) { Destroy(gameObject); return; }
        }
    }

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
        { Destroy(gameObject); return; }

        if (collision.contactCount > 0)
            impactNormal = collision.GetContact(0).normal;

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
        MetalObject metal = GetComponent<MetalObject>();
        if (metal != null)
        {
            Rigidbody2D hitRb = hitObject.GetComponent<Rigidbody2D>();
            metal.anchoredMass = hitRb != null ? hitRb.mass : 9999f;
        }
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) { rb.linearVelocity = Vector2.zero; rb.bodyType = RigidbodyType2D.Static; }

        EmbedVisual();
    }

    void EmbedVisual()
    {
        Vector2 embedDir = -impactNormal;
        float spriteHeight = sr != null ? sr.bounds.size.y : 0.1f;
        transform.position += (Vector3)(embedDir * spriteHeight * embedFraction);

        // Poner detrás de la superficie
        if (sr != null) sr.sortingOrder = embeddedSortingOrder;
    }

    public void Unanchor()
    {
        if (!anchored) return;
        anchored = false; unanchoring = true;

        // Restaurar sorting order
        if (sr != null) sr.sortingOrder = originalSortingOrder;

        MetalObject metal = GetComponent<MetalObject>();
        if (metal != null) metal.anchoredMass = 0f;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
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