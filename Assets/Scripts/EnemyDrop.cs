using UnityEngine;

/// <summary>
/// Suelta monedas, clavos y frascos de metal al morir el enemigo.
/// Adjuntar al mismo GameObject que EnemyHealth.
///
/// ─── Setup recomendado ───────────────────────────────────────────────────
///   Cultista : coins 1-2 | sin clavos | frasco Steel/Iron ocasional
///   Guardián : coins 1-3 | 1 clavo   | frasco Pewter ocasional
/// ─────────────────────────────────────────────────────────────────────────
/// </summary>
[RequireComponent(typeof(EnemyHealth))]
public class EnemyDrop : MonoBehaviour
{
    [System.Serializable]
    public struct FlaskDrop
    {
        public GameObject prefab;
        [Range(0f, 1f)] public float dropChance; // 0 = nunca, 1 = siempre
    }

    [Header("Prefabs de proyectiles")]
    public GameObject coinPrefab;
    public GameObject nailPrefab;

    [Header("Monedas")]
    public int minCoins = 1;
    public int maxCoins = 2;

    [Header("Clavos")]
    public int minNails = 0;
    public int maxNails = 0;

    [Header("Frascos (chance 0-1)")]
    public FlaskDrop steelFlask;
    public FlaskDrop ironFlask;
    public FlaskDrop pewterFlask;
    public FlaskDrop duraluminFlask;

    [Header("Dispersión")]
    [Tooltip("Fuerza con la que salen despedidos los drops")]
    public float scatterForce = 3.5f;

    private EnemyHealth health;

    void Start()
    {
        health = GetComponent<EnemyHealth>();
        health.OnDeath.AddListener(SpawnDrops);
    }

    void OnDestroy()
    {
        if (health != null)
            health.OnDeath.RemoveListener(SpawnDrops);
    }

    void SpawnDrops()
    {
        int coins = Random.Range(minCoins, maxCoins + 1);
        int nails = Random.Range(minNails, maxNails + 1);

        for (int i = 0; i < coins; i++) SpawnItem(coinPrefab, i, coins);
        for (int i = 0; i < nails; i++) SpawnItem(nailPrefab, i, nails);

        // Intentar drops por probabilidad
        bool anyDropped = false;
        anyDropped |= TryDropFlask(steelFlask);
        anyDropped |= TryDropFlask(ironFlask);
        anyDropped |= TryDropFlask(pewterFlask);
        anyDropped |= TryDropFlask(duraluminFlask);

        // Garantizar al menos uno si ninguno salió
        if (!anyDropped)
            ForceDropOneFlask();
    }

    bool TryDropFlask(FlaskDrop drop)
    {
        if (drop.prefab == null || drop.dropChance <= 0f) return false;
        if (Random.value <= drop.dropChance)
        {
            SpawnItem(drop.prefab, 0, 1);
            return true;
        }
        return false;
    }

    void ForceDropOneFlask()
    {
        var available = new System.Collections.Generic.List<FlaskDrop>();
        if (steelFlask.prefab     != null) available.Add(steelFlask);
        if (ironFlask.prefab      != null) available.Add(ironFlask);
        if (pewterFlask.prefab    != null) available.Add(pewterFlask);
        if (duraluminFlask.prefab != null) available.Add(duraluminFlask);

        if (available.Count == 0) return;
        SpawnItem(available[Random.Range(0, available.Count)].prefab, 0, 1);
    }

    void SpawnItem(GameObject prefab, int index, int total)
    {
        if (prefab == null) return;

        GameObject obj = Instantiate(prefab, transform.position, Quaternion.identity);

        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float spread = total > 1 ? 80f : 0f;
            float angle  = 90f + spread * (index / Mathf.Max(total - 1f, 1f) - 0.5f);
            float rad    = angle * Mathf.Deg2Rad;
            Vector2 dir  = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            rb.AddForce(dir * scatterForce, ForceMode2D.Impulse);
        }
    }
}
