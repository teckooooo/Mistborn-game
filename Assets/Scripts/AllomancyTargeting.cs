using UnityEngine;
using System.Collections.Generic;

public class AllomancyTargeting : MonoBehaviour
{
    [Header("Targeting")]
    public float detectionRadius = 10f;
    public int   maxPushTargets  = 1;
    public int   maxPullTargets  = 1;

    private List<(MetalObject metal, float score)> candidates = new();

    public float EffectiveRadius
    {
        get
        {
            MetalReserve   r = GetComponent<MetalReserve>();
            AllomancyStats s = GetComponent<AllomancyStats>();
            if (r != null && r.DuraluMinActive && s != null)
                return detectionRadius * s.duraluMinRadiusMultiplier;
            return detectionRadius;
        }
    }

    public List<MetalObject> GetPushTargets() => GetTargets(maxPushTargets);
    public List<MetalObject> GetPullTargets() => GetTargets(maxPullTargets);

    public (List<MetalObject> push, List<MetalObject> pull) GetPushAndPullTargets()
    {
        RefreshCandidates();
        var pushList = new List<MetalObject>();
        var pullList = new List<MetalObject>();
        var used     = new HashSet<MetalObject>();

        for (int i = 0; i < candidates.Count && pushList.Count < maxPushTargets; i++)
        { pushList.Add(candidates[i].metal); used.Add(candidates[i].metal); }

        for (int i = 0; i < candidates.Count && pullList.Count < maxPullTargets; i++)
        {
            if (!used.Contains(candidates[i].metal))
            { pullList.Add(candidates[i].metal); used.Add(candidates[i].metal); }
        }

        if (pullList.Count == 0 && candidates.Count > 0)
            pullList.Add(candidates[0].metal);

        return (pushList, pullList);
    }

    private List<MetalObject> GetTargets(int max)
    {
        RefreshCandidates();
        var result = new List<MetalObject>();
        foreach (var (metal, _) in candidates)
        {
            if (result.Count >= max) break;
            result.Add(metal);
        }
        return result;
    }

    private void RefreshCandidates()
    {
        candidates.Clear();

        Vector3 mp    = Input.mousePosition;
        mp.z          = Mathf.Abs(Camera.main.transform.position.z);
        Vector2 mouse = Camera.main.ScreenToWorldPoint(mp);
        Vector2 origin = (Vector2)transform.position;
        Vector2 aimDir = (mouse - origin).normalized;

        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, EffectiveRadius, ~0);

        // Usar HashSet para evitar agregar el mismo MetalObject dos veces
        // (puede ocurrir si el objeto tiene múltiples colliders detectados)
        var seen = new HashSet<MetalObject>();
Debug.Log($"[Targeting] Hits detectados: {hits.Length}");
        foreach (Collider2D col in hits)
        {
             Debug.Log($"  - {col.gameObject.name} | Layer: {col.gameObject.layer} | IsTrigger: {col.isTrigger} | Parent: {col.transform.parent?.name}");
            if (col.gameObject == gameObject) continue;

            // Buscar MetalObject en el propio GameObject o en su padre
            // Necesario para detectar colliders hijos como CoinAllomancyDetector
            MetalObject metal = col.GetComponent<MetalObject>();
            if (metal == null) metal = col.GetComponentInParent<MetalObject>();
            if (metal == null) continue;

            // Evitar duplicados
            if (seen.Contains(metal)) continue;
            seen.Add(metal);

            Vector2 toMetal = (Vector2)metal.transform.position - origin;
            float   dot     = Vector2.Dot(toMetal.normalized, aimDir);
            if (dot < 0) continue;

            candidates.Add((metal, 1f - dot));
        }

        candidates.Sort((a, b) => a.score.CompareTo(b.score));
    }
}