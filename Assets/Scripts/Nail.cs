using UnityEngine;

public class Nail : MonoBehaviour
{
    [HideInInspector] public bool  beingPulled     = false;
    [HideInInspector] public bool  duraluminPulled = false;
    [HideInInspector] public float duraluMinThreshold = 3f;

    [Header("Colección")]
    public float collectRadius = 1f;

    [Header("Superficies de incrustación")]
    public string[] anchorTags = new string[] { "Ground", "Muro", "Piso", "Pasto", "Cristal" };

    [Header("Embed Visual")]
    public float embedFraction = 0.45f;
    [Tooltip("Sorting Order cuando está incrustado (detrás de la superficie)")]
    public int embeddedSortingOrder = -1;

    public bool Embedded => embedded;

    private bool        embedded    = false;
    private bool        unanchoring = false;
    private Rigidbody2D rb;
    private Transform   playerTransform;
    private Transform   spawnTarget;
    private Vector2     impactNormal   = Vector2.up;
    private int         originalSortingOrder;
    private SpriteRenderer sr;

    private float pullTimer   = 0f;
    private float maxPullTime = 5f;
    private bool  collisionIgnored = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalSortingOrder = sr.sortingOrder;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            NailThrow nt = player.GetComponent<NailThrow>();
            spawnTarget = (nt != null && nt.nailSpawn != null) ? nt.nailSpawn : player.transform;
        }
    }

    void Update()
    {
        Transform target = spawnTarget != null ? spawnTarget : playerTransform;
        if (target == null) return;

        bool pulling = beingPulled || duraluminPulled;
        if (pulling && !collisionIgnored) { IgnorePlayerCollision(true); collisionIgnored = true; }
        else if (!pulling && collisionIgnored) { IgnorePlayerCollision(false); collisionIgnored = false; }

        if (duraluminPulled && !unanchoring)
        {
            Vector2 dir  = ((Vector2)target.position - (Vector2)transform.position).normalized;
            float   dist = Vector2.Distance(transform.position, target.position);
            if (rb != null) { rb.gravityScale = 0f; rb.linearVelocity = dir * 20f; }
            if (dist < collectRadius) { Destroy(gameObject); return; }
        }

        if (beingPulled && !unanchoring)
        {
            pullTimer += Time.deltaTime;
            if (rb != null) rb.gravityScale = 0f;
            float dist = Vector2.Distance(transform.position, target.position);
            if (dist < collectRadius) { Destroy(gameObject); return; }
            if (rb != null && rb.linearVelocity.magnitude < 0.1f && pullTimer > 0.5f)
            {
                beingPulled = false; duraluminPulled = false;
                pullTimer = 0f; rb.gravityScale = 1f; return;
            }
            if (pullTimer > maxPullTime)
            {
                beingPulled = false; duraluminPulled = false;
                pullTimer = 0f;
                if (rb != null) rb.gravityScale = 1f;
            }
        }
        else if (!beingPulled)
        {
            pullTimer = 0f;
            if (rb != null) rb.gravityScale = 1f;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (beingPulled && !unanchoring && collision.gameObject.CompareTag("Player"))
        { Destroy(gameObject); return; }

        if (!embedded && !collision.gameObject.CompareTag("Player") && IsValidSurface(collision.gameObject.tag))
        {
            if (collision.contactCount > 0)
                impactNormal = collision.GetContact(0).normal;
            Embed();
        }
    }

    void Embed()
    {
        embedded = true;
        MetalObject metal = GetComponent<MetalObject>();
        if (metal != null) metal.anchoredMass = 9999f;
        if (rb != null) { rb.linearVelocity = Vector2.zero; rb.bodyType = RigidbodyType2D.Static; }
        EmbedVisual();
    }

    void EmbedVisual()
    {
        Vector2 embedDir = -impactNormal;

        // Rotar punta hacia la superficie (-Y apunta a embedDir)
        float angle = Mathf.Atan2(embedDir.y, embedDir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // Hundir hacia adentro
        float spriteHeight = sr != null ? sr.bounds.size.y : 0.2f;
        transform.position += (Vector3)(embedDir * spriteHeight * embedFraction);

        // Poner detrás de la superficie — la superficie tapa la parte enterrada
        if (sr != null) sr.sortingOrder = embeddedSortingOrder;
    }

    public void RemoveEmbedMask()
    {
        if (sr != null)
        {
            sr.sortingOrder = originalSortingOrder;
            sr.enabled = true;
        }
    }

    bool IsValidSurface(string tag)
    {
        foreach (string t in anchorTags)
            if (t == tag) return true;
        return false;
    }

    public bool TryPullNormal() => !embedded;

    public bool TryRemove(float boost)
    {
        if (!embedded) return true;
        if (boost < duraluMinThreshold) return false;
        Unanchor(); return true;
    }

    public void Unanchor()
    {
        if (!embedded) return;
        embedded = false; unanchoring = true;
        RemoveEmbedMask();
        MetalObject metal = GetComponent<MetalObject>();
        if (metal != null) metal.anchoredMass = 0f;
        if (rb != null) rb.bodyType = RigidbodyType2D.Dynamic;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Collider2D pc = player.GetComponent<Collider2D>();
            Collider2D nc = GetComponent<Collider2D>();
            if (pc != null && nc != null) Physics2D.IgnoreCollision(nc, pc, true);
        }
        Invoke(nameof(RestoreCollision), 0.15f);
    }

    void IgnorePlayerCollision(bool ignore)
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;
        Collider2D pc = player.GetComponent<Collider2D>();
        Collider2D nc = GetComponent<Collider2D>();
        if (pc != null && nc != null) Physics2D.IgnoreCollision(nc, pc, ignore);
    }

    void RestoreCollision()
    {
        unanchoring = false;
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Collider2D pc = player.GetComponent<Collider2D>();
            Collider2D nc = GetComponent<Collider2D>();
            if (pc != null && nc != null) Physics2D.IgnoreCollision(nc, pc, false);
        }
    }
}