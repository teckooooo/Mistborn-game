using UnityEngine;

public class AllomanticLines : MonoBehaviour
{
    [Header("Detección")]
    public float detectionRadius = 10f;

    [Header("Colores")]
    public Color lineColorDefault  = new Color(0.2f, 0.6f, 1f, 0.3f);
    public Color lineColorPush     = new Color(1f,   0.5f, 0f, 1.0f); // naranja = push
    public Color lineColorPull     = new Color(0.2f, 0.9f, 1f, 1.0f); // azul brillante = pull
    public float lineWidthDefault  = 0.03f;
    public float lineWidthTarget   = 0.06f;
    public int   maxLines          = 20;

    private LineRenderer[]     lines;
    private AllomancyTargeting targeting;
    private MetalReserve       reserve;
    private AllomancyStats     stats;

    void Start()
    {
        targeting = GetComponent<AllomancyTargeting>();
        reserve   = GetComponent<MetalReserve>();
        stats     = GetComponent<AllomancyStats>();
        lines     = new LineRenderer[maxLines];

        for (int i = 0; i < maxLines; i++)
        {
            GameObject go    = new GameObject($"AllomanticLine_{i}");
            go.transform.SetParent(transform);
            LineRenderer lr  = go.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.material      = new Material(Shader.Find("Sprites/Default"));
            lr.sortingOrder  = 10;
            lr.enabled       = false;
            lines[i]         = lr;
        }
    }

    void Update()
    {
        bool pushActive = Input.GetKey(KeyCode.Q);
        bool pullActive = Input.GetMouseButton(1);

        if (!pushActive && !pullActive) { HideAll(); return; }

        var pushTargets = pushActive ? targeting.GetPushTargets() : new System.Collections.Generic.List<MetalObject>();
        var pullTargets = pullActive ? targeting.GetPullTargets() : new System.Collections.Generic.List<MetalObject>();

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, targeting.EffectiveRadius);
        int lineIndex = 0;

        foreach (Collider2D col in hits)
        {
            if (col.gameObject == gameObject) continue;
            MetalObject metal = col.GetComponent<MetalObject>();
            if (metal == null) continue;
            if (lineIndex >= maxLines) break;

            bool isPushTarget = pushActive && pushTargets.Contains(metal);
            bool isPullTarget = pullActive && pullTargets.Contains(metal);

            Color lineColor = lineColorDefault;
            float lineWidth = lineWidthDefault;

            if (isPushTarget && isPullTarget) lineColor = Color.white;
            else if (isPushTarget)            lineColor = lineColorPush;
            else if (isPullTarget)            lineColor = lineColorPull;

            if (isPushTarget || isPullTarget) lineWidth = lineWidthTarget;

            LineRenderer lr = lines[lineIndex];
            lr.enabled      = true;
            lr.startWidth   = lineWidth;
            lr.endWidth     = lineWidth * 0.5f;
            lr.startColor   = lineColor;
            lr.endColor     = new Color(lineColor.r, lineColor.g, lineColor.b, 0.1f);
            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, col.transform.position);

            lineIndex++;
        }

        for (int i = lineIndex; i < maxLines; i++)
            lines[i].enabled = false;
    }

    void HideAll()
    {
        foreach (var lr in lines) lr.enabled = false;
    }
}