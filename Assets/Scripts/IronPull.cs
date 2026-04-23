using UnityEngine;
using System.Collections.Generic;

public class IronPull : MonoBehaviour
{
    [Header("Detección")]
    public float detectionRadius = 10f;

    [Header("Debug")]
    public bool debugLog = true;

    private Rigidbody2D        playerRb;
    private AllomancyTargeting targeting;
    private MetalReserve       reserve;
    private AllomancyStats     stats;
    private Transform          coinSpawn;

    void Start()
    {
        playerRb  = GetComponent<Rigidbody2D>();
        targeting = GetComponent<AllomancyTargeting>();
        reserve   = GetComponent<MetalReserve>();
        stats     = GetComponent<AllomancyStats>();
        PlayerController pc = GetComponent<PlayerController>();
        if (pc != null) coinSpawn = pc.coinSpawn;
    }

    void Update()
    {
        if (Input.GetMouseButton(1))
            PullMetal();

        if (Input.GetMouseButtonUp(1))
            UnmarkAllCoins();
    }

    public void PullTargets(List<MetalObject> targets, float strength)
    {
        foreach (MetalObject metal in targets)
        {
            Rigidbody2D metalRb       = metal.GetComponent<Rigidbody2D>();
            bool        metalIsStatic = metalRb == null || metalRb.bodyType == RigidbodyType2D.Static;

            if (metalRb != null && !metalIsStatic && metalRb.IsSleeping()) metalRb.WakeUp();

            Nail nail = metal.GetComponent<Nail>();
            if (nail != null)
            {
                bool isDuralumin = reserve != null && reserve.DuraluMinActive && reserve.duraluMinAffectsIron;
                if (nail.Embedded)
                {
                    if (isDuralumin)
                    {
                        bool removed = nail.TryRemove(999f);
                        if (!removed) continue;
                        nail.RemoveEmbedMask();
                        nail.beingPulled = true;
                    }
                    else
                    {
                        Vector2 dirToNail = ((Vector2)metal.transform.position - (coinSpawn != null ? (Vector2)coinSpawn.position : (Vector2)transform.position)).normalized;
                        float   nailForce = Mathf.Max(strength * 5f, strength * 0.5f);
                        playerRb.AddForce(dirToNail * nailForce, ForceMode2D.Impulse);
                        if (debugLog) Debug.Log($"[IronPull] Clavo incrustado — jugador atraido | force={nailForce:F1}");
                        continue;
                    }
                }
                else
                    nail.beingPulled = true;
            }

            Coin coin = metal.GetComponent<Coin>();
            if (coin != null)
            {
                // Si la moneda está anclada en superficie estática (masa 9999),
                // no desanclarla — el jugador se mueve hacia ella como ancla
                bool anchoredToStatic = metal.anchoredMass >= 9999f;
                if (!anchoredToStatic)
                {
                    coin.beingPulled = true;
                    coin.Unanchor();
                }
    else
    {
        // Moneda anclada en suelo — no atraer al jugador hacia ella,
        // simplemente ignorar hasta que Steel Push la desancle
        continue;
    }
            }

            Vector2 pullTarget    = coinSpawn != null ? (Vector2)coinSpawn.position : (Vector2)transform.position;
            Vector2 dir           = (pullTarget - (Vector2)metal.transform.position).normalized;
            float   effectiveMass = Mathf.Max(metal.EffectiveMass, 0.01f);
            float   totalMass     = playerRb.mass + effectiveMass;
            float   totalForce    = strength * (effectiveMass / playerRb.mass);
            float   metalShare    = totalForce * (playerRb.mass / totalMass);
            float   playerShare   = Mathf.Max(
                Mathf.Min(totalForce * (effectiveMass / totalMass), strength * 5f),
                strength * 0.5f);

            if (metalIsStatic)
                playerRb.AddForce(-dir * playerShare, ForceMode2D.Impulse);
            else
            {
                if (metalRb != null)
                    metalRb.AddForce(dir * metalShare, ForceMode2D.Impulse);
                playerRb.AddForce(-dir * playerShare, ForceMode2D.Force);
            }

            if (debugLog)
                Debug.Log($"[IronPull] '{metal.name}' | static={metalIsStatic} anchoredMass={metal.anchoredMass} strength={strength:F1} playerShare={playerShare:F1}");
        }
    }

    void PullMetal()
    {
        if (stats == null) { Debug.LogError("[IronPull] AllomancyStats no encontrado."); return; }

        bool bothActive = Input.GetKey(KeyCode.Q);
        List<MetalObject> targets;
        if (bothActive)
        {
            var (_, pull) = targeting.GetPushAndPullTargets();
            targets = pull;
        }
        else
            targets = targeting.GetPullTargets();

        if (targets.Count == 0) { if (debugLog) Debug.LogWarning("[IronPull] Sin targets."); return; }

        bool duraluActive = reserve != null && reserve.DuraluMinActive && reserve.duraluMinAffectsIron;

        if (!duraluActive)
        {
            if (reserve != null && !reserve.HasIron) { Debug.Log("[IronPull] Sin Hierro."); return; }
            reserve?.ConsumeIronPerSec(Time.deltaTime);
        }
        else
        {
            reserve?.MarkIronUsed();
        }

        float strength = stats.allomanticStrength;
        if (duraluActive) strength *= reserve.DuraluMinBoost;

        PullTargets(targets, strength);
    }

    void UnmarkAllCoins()
    {
        foreach (Coin coin in FindObjectsByType<Coin>(FindObjectsSortMode.None))
            coin.beingPulled = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.5f, 1f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}