using UnityEngine;
using UnityEngine.EventSystems;
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
        if (PauseMenu.IsPaused)
        {
            UnmarkAllProjectiles(); // soltar todo si se pausa mientras se mantiene botón
            return;
        }

        bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        if (Input.GetMouseButton(1) && !overUI)
            PullMetal();

        if (Input.GetMouseButtonUp(1))
            UnmarkAllProjectiles();
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

            // Notificar al MetalObject — 1 unidad/segundo independiente del strength
            metal.OnAllomancyForce?.Invoke(Time.deltaTime);

            if (debugLog)
                Debug.Log($"[IronPull] '{metal.name}' | static={metalIsStatic} anchoredMass={metal.anchoredMass} strength={strength:F1} playerShare={playerShare:F1}");
        }
    }

    void PullMetal()
    {
        if (stats == null) { Debug.LogError("[IronPull] AllomancyStats no encontrado."); return; }

        bool duraluActive = reserve != null && reserve.DuraluMinActive && reserve.duraluMinAffectsIron;

        if (!duraluActive)
        {
            if (reserve != null && !reserve.HasIron) { Debug.Log("[IronPull] Sin Hierro."); return; }
            reserve?.ConsumeIronPerSec(Time.deltaTime);
        }
        else
            reserve?.MarkIronUsed();

        float strength = stats.allomanticStrength;
        if (duraluActive) strength *= reserve.DuraluMinBoost;

        bool bothActive = Input.GetKey(KeyCode.Q);
        if (bothActive)
        {
            // Q + Click derecho: targeting original (útil para combos físicos / Guardian)
            var (_, pull) = targeting.GetPushAndPullTargets();
            if (pull.Count == 0) { if (debugLog) Debug.LogWarning("[IronPull] Sin targets."); return; }
            PullTargets(pull, strength);
        }
        else
        {
            // Click derecho: jalar SOLO lo que apuntas (1 objetivo).
            // Fuente de metal apuntada → te elevas/acercas a ella.
            // Moneda/clavo apuntado → lo llamas a ti.
            // Lo que no apuntas NO se mueve (las monedas cercanas ya no interfieren).
            List<MetalObject> targets = targeting.GetPullTargets();
            if (targets.Count == 0) { if (debugLog) Debug.LogWarning("[IronPull] Sin targets."); return; }
            PullTargets(targets, strength);
        }
    }

    /// <summary>
    /// Devuelve todos los Coins y Nails dentro del radio de detección,
    /// sin importar la dirección del cursor.
    /// </summary>
    List<MetalObject> GetAreaProjectiles()
    {
        float radius = targeting != null ? targeting.EffectiveRadius : detectionRadius;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);

        var result = new List<MetalObject>();
        var seen   = new HashSet<MetalObject>();

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            // Solo proyectiles del jugador (Coin o Nail)
            bool isCoin = hit.GetComponent<Coin>() != null;
            bool isNail = hit.GetComponent<Nail>() != null;
            if (!isCoin && !isNail) continue;

            MetalObject metal = hit.GetComponent<MetalObject>();
            if (metal == null) metal = hit.GetComponentInParent<MetalObject>();
            if (metal == null) continue;

            if (seen.Contains(metal)) continue;
            seen.Add(metal);
            result.Add(metal);
        }

        return result;
    }

    /// <summary>
    /// Recall forzado: desancla y lanza hacia el jugador todas las monedas y clavos
    /// de la lista, ignorando si estaban anclados al suelo.
    /// </summary>
    void RecallProjectiles(List<MetalObject> projectiles)
    {
        Vector2 pullTarget = coinSpawn != null ? (Vector2)coinSpawn.position : (Vector2)transform.position;
        const float recallSpeed = 14f;

        foreach (MetalObject metal in projectiles)
        {
            Rigidbody2D metalRb = metal.GetComponent<Rigidbody2D>();

            Coin coin = metal.GetComponent<Coin>();
            if (coin != null)
            {
                coin.Unanchor();          // pasa a Dynamic
                coin.beingPulled = true;  // Coin.Update() la destruye al llegar
                if (metalRb != null)
                {
                    Vector2 dir = (pullTarget - (Vector2)metal.transform.position).normalized;
                    metalRb.linearVelocity = dir * recallSpeed;
                }
                continue;
            }

            Nail nail = metal.GetComponent<Nail>();
            if (nail != null && !nail.Embedded)
            {
                nail.beingPulled = true;
                if (metalRb != null)
                {
                    Vector2 dir = (pullTarget - (Vector2)metal.transform.position).normalized;
                    metalRb.linearVelocity = dir * recallSpeed;
                }
            }
            // Clavos incrustados: requieren Q + click derecho (Duraluminio)
        }
    }

    void UnmarkAllProjectiles()
    {
        foreach (Coin coin in FindObjectsByType<Coin>(FindObjectsSortMode.None))
            coin.beingPulled = false;
        foreach (Nail nail in FindObjectsByType<Nail>(FindObjectsSortMode.None))
            nail.beingPulled = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.5f, 1f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}