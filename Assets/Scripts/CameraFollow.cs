using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    public Transform player;

    [Header("Seguimiento")]
    [Tooltip("Si está activo la cámara sigue al jugador en el eje Y. " +
             "Si está apagado usa 'fixedY'.")]
    public bool followVertical = true;

    [Tooltip("Desplazamiento de la cámara respecto al jugador (X, Y). " +
             "Sube offset.y para encuadrar el piso más abajo en pantalla.")]
    public Vector2 offset = Vector2.zero;

    [Tooltip("Y fijo usado solo cuando 'followVertical' está apagado.")]
    public float fixedY = 0f;

    [Header("Zona muerta")]
    [Tooltip("Ancho (X, en unidades) de la banda donde el jugador se mueve SIN " +
             "que la cámara se mueva. 0 = la cámara va siempre centrada en X. " +
             "Valores típicos: 2–5.")]
    public float deadZoneWidth = 3f;

    [Tooltip("Alto (Y, en unidades) de la banda vertical. La cámara solo se " +
             "desplaza cuando el jugador sale de la banda (ej. baja a otra " +
             "plataforma). 0 = pegado al jugador. Valores típicos: 3–6.")]
    public float deadZoneHeight = 4f;

    [Header("Look-ahead vertical (al caer)")]
    [Tooltip("Al caer, la cámara se adelanta hacia abajo para mostrar el " +
             "aterrizaje. Permite tener el piso abajo (offset.y) al estar parado, " +
             "pero ver hacia dónde caes al saltar a una zona inferior.")]
    public bool fallLookAhead = true;

    [Tooltip("Cuánto baja la cámara (unidades) mientras el jugador cae.")]
    public float lookAheadAmount = 3f;

    [Tooltip("Velocidad de caída (módulo) a partir de la cual se activa el look-ahead.")]
    public float lookAheadFallSpeed = 2f;

    [Tooltip("Suavizado de entrada/salida del look-ahead. Más alto = más gradual.")]
    public float lookAheadSmooth = 0.25f;

    [Header("Límites del nivel (confiner)")]
    [Tooltip("BoxCollider2D que define los bordes del nivel. La cámara nunca " +
             "mostrará nada fuera de esta caja. Déjalo vacío para no usar límites. " +
             "Marca 'Is Trigger' en ese collider para que no bloquee al jugador.")]
    public BoxCollider2D bounds;

    [Header("Suavizado")]
    [Tooltip("Suavizado del movimiento de la cámara. 0 = instantáneo. " +
             "Valores típicos 0.05–0.15.")]
    public float smoothTime = 0.08f;

    private Camera      cam;
    private Rigidbody2D playerRb;
    private Vector2     focus;      // centro de la zona muerta (en mundo)
    private Vector2     velocity;
    private float       lookAheadY; // adelanto vertical actual (suavizado)
    private float       lookVel;

    void Start()
    {
        cam      = GetComponent<Camera>();
        playerRb = player != null ? player.GetComponent<Rigidbody2D>() : null;
        focus    = player != null
            ? (Vector2)player.position
            : new Vector2(transform.position.x, fixedY);
    }

    void LateUpdate()
    {
        if (player == null) return;
        if (cam == null) cam = GetComponent<Camera>();

        Vector2 p = player.position;

        // ── Zona muerta horizontal ───────────────────────────────────────────
        float halfW = Mathf.Max(0f, deadZoneWidth * 0.5f);
        if      (p.x > focus.x + halfW) focus.x = p.x - halfW;
        else if (p.x < focus.x - halfW) focus.x = p.x + halfW;

        // ── Zona muerta vertical ─────────────────────────────────────────────
        if (followVertical)
        {
            float halfH = Mathf.Max(0f, deadZoneHeight * 0.5f);
            if      (p.y > focus.y + halfH) focus.y = p.y - halfH;
            else if (p.y < focus.y - halfH) focus.y = p.y + halfH;
        }

        // ── Look-ahead vertical: adelantar la cámara hacia abajo al caer ──────
        float targetLook = 0f;
        if (fallLookAhead && followVertical && playerRb != null &&
            playerRb.linearVelocity.y < -lookAheadFallSpeed)
            targetLook = -lookAheadAmount;
        lookAheadY = Mathf.SmoothDamp(lookAheadY, targetLook, ref lookVel, lookAheadSmooth);

        float desiredX = focus.x + offset.x;
        float desiredY = followVertical ? focus.y + offset.y + lookAheadY : fixedY;

        // ── Confiner: mantener la vista dentro de los límites del nivel ───────
        if (bounds != null && cam != null && cam.orthographic)
        {
            Bounds b    = bounds.bounds;
            float  vExt = cam.orthographicSize;        // media altura visible
            float  hExt = vExt * cam.aspect;           // media anchura visible

            float minX = b.min.x + hExt, maxX = b.max.x - hExt;
            float minY = b.min.y + vExt, maxY = b.max.y - vExt;

            // Si el nivel es más chico que la vista, centrar en ese eje
            desiredX = minX <= maxX ? Mathf.Clamp(desiredX, minX, maxX) : b.center.x;
            desiredY = minY <= maxY ? Mathf.Clamp(desiredY, minY, maxY) : b.center.y;
        }

        // ── Aplicar con suavizado ────────────────────────────────────────────
        Vector2 target  = new Vector2(desiredX, desiredY);
        Vector2 current = transform.position;
        Vector2 next = smoothTime > 0f
            ? Vector2.SmoothDamp(current, target, ref velocity, smoothTime)
            : target;

        transform.position = new Vector3(next.x, next.y, -10f);
    }

    // ── Gizmos: zona muerta (amarillo) y límites del nivel (verde) ────────────
    void OnDrawGizmosSelected()
    {
        Vector3 c = transform.position;

        Gizmos.color = new Color(1f, 0.9f, 0.2f, 0.7f);
        Gizmos.DrawWireCube(
            new Vector3(c.x, c.y, 0f),
            new Vector3(Mathf.Max(deadZoneWidth, 0.05f),
                        followVertical ? Mathf.Max(deadZoneHeight, 0.05f) : 0.05f, 0f));

        if (bounds != null)
        {
            Gizmos.color = new Color(0.3f, 1f, 0.45f, 0.7f);
            Gizmos.DrawWireCube(bounds.bounds.center, bounds.bounds.size);
        }
    }
}
