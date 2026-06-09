using UnityEngine;

public class NailThrow : MonoBehaviour
{
    [Header("Clavo")]
    public GameObject nailPrefab;
    public Transform  nailSpawn;
    public float      nailForce = 25f;

    private MetalReserve       reserve;
    private PlayerInventory    inventory;
    private AllomancyTargeting targeting;
    private Rigidbody2D        playerRb;

    void Start()
    {
        reserve   = GetComponent<MetalReserve>();
        inventory = GetComponent<PlayerInventory>();
        targeting = GetComponent<AllomancyTargeting>();
        playerRb  = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        SyncSpawnFlip();

        if (PauseMenu.IsPaused) return;

        if (Input.GetKeyDown(KeyCode.R))
            ThrowNail();

        // F → activar Duraluminio general
        if (Input.GetKeyDown(KeyCode.F))
            reserve?.ActivateDuraluMin();
    }

    void SyncSpawnFlip()
    {
        if (nailSpawn == null) return;
        Vector3 mp = Input.mousePosition;
        mp.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mp);
        bool mouseIsRight  = mouseWorld.x > transform.position.x;
        Vector3 pos        = nailSpawn.localPosition;
        pos.x              = Mathf.Abs(pos.x) * (mouseIsRight ? 1f : -1f);
        nailSpawn.localPosition = pos;
    }

    void ThrowNail()
    {
        if (nailPrefab == null) { Debug.LogWarning("[NailThrow] nailPrefab no asignado."); return; }
        if (inventory != null && !inventory.HasNails) { Debug.Log("[NailThrow] Sin clavos."); return; }
        if (reserve != null && !reserve.HasSteel)     { Debug.Log("[NailThrow] Sin Acero."); return; }

        inventory?.ConsumeNail();
        reserve?.ConsumeNail();

        Vector3 spawnPos = nailSpawn != null ? nailSpawn.position : transform.position;
        GameObject nail  = Instantiate(nailPrefab, spawnPos, Quaternion.identity);

        Vector3 mp = Input.mousePosition;
        mp.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mp);
        Vector2 dir        = ((Vector2)mouseWorld - (Vector2)spawnPos).normalized;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        nail.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Rigidbody2D nailRb = nail.GetComponent<Rigidbody2D>();
        if (nailRb != null)
            nailRb.AddForce(dir * nailForce, ForceMode2D.Impulse);
    }
}