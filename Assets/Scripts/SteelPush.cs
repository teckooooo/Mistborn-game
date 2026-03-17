using UnityEngine;
using System.Collections.Generic;

public class SteelPush : MonoBehaviour
{
    [Header("Detección")]
    public float detectionRadius = 10f;

    [Header("Debug")]
    public bool debugLog = true;

    private Rigidbody2D        playerRb;
    private AllomancyTargeting targeting;
    private MetalReserve       reserve;
    private AllomancyStats     stats;

    void Start()
    {
        playerRb  = GetComponent<Rigidbody2D>();
        targeting = GetComponent<AllomancyTargeting>();
        reserve   = GetComponent<MetalReserve>();
        stats     = GetComponent<AllomancyStats>();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Q))
            PushMetal();
    }

    public void PushTargets(List<MetalObject> targets, float strength)
    {
        foreach (MetalObject metal in targets)
        {
            Rigidbody2D metalRb       = metal.GetComponent<Rigidbody2D>();
            bool        metalIsStatic = metalRb == null || metalRb.bodyType == RigidbodyType2D.Static;

            Vector2 dir           = ((Vector2)metal.transform.position - (Vector2)transform.position).normalized;
            float   effectiveMass = Mathf.Max(metal.EffectiveMass, 0.01f);
            float   totalMass     = playerRb.mass + effectiveMass;
            float   totalForce    = strength * (effectiveMass / playerRb.mass);
            float   metalShare    = totalForce * (playerRb.mass   / totalMass);
            float   playerShare   = Mathf.Max(
                Mathf.Min(totalForce * (effectiveMass / totalMass), strength * 5f),
                strength * 0.5f);

            if (!metalIsStatic && metalRb != null)
                metalRb.AddForce(dir * metalShare, ForceMode2D.Force);

            ForceMode2D mode = metalIsStatic ? ForceMode2D.Impulse : ForceMode2D.Force;
            playerRb.AddForce(-dir * playerShare, mode);

            if (debugLog)
                Debug.Log($"[SteelPush] '{metal.name}' | static={metalIsStatic} strength={strength} playerShare={playerShare:F1}");
        }
    }

    void PushMetal()
    {
        if (stats == null) { Debug.LogError("[SteelPush] AllomancyStats no encontrado."); return; }

        bool bothActive = Input.GetMouseButton(1);
        List<MetalObject> targets;
        if (bothActive)
        {
            var (push, _) = targeting.GetPushAndPullTargets();
            targets = push;
        }
        else
            targets = targeting.GetPushTargets();

        if (targets.Count == 0) { if (debugLog) Debug.LogWarning("[SteelPush] Sin targets."); return; }

        bool duraluActive = reserve != null && reserve.DuraluMinActive && reserve.duraluMinAffectsSteel;

        if (!duraluActive)
        {
            if (reserve != null && !reserve.HasSteel) { Debug.Log("[SteelPush] Sin Acero."); return; }
            reserve?.ConsumeSteelPerSec(Time.deltaTime);
        }
        else
        {
            reserve?.MarkSteelUsed(); // marcar acero como usado durante boost
        }

        float strength = stats.allomanticStrength;
        if (duraluActive) strength *= reserve.DuraluMinBoost;

        PushTargets(targets, strength);
    }
}