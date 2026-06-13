using UnityEngine;

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

    [Header("Zona muerta vertical")]
    [Tooltip("Altura (en unidades de mundo) de la banda central donde el jugador " +
             "puede moverse SIN que la cámara se mueva. La cámara solo se desplaza " +
             "cuando el jugador sale de la banda (ej. baja a otra plataforma o " +
             "salta muy alto). 0 = cámara pegada al jugador (siempre centrado). " +
             "Valores típicos: 3–6.")]
    public float deadZoneHeight = 4f;

    [Header("Límites verticales")]
    [Tooltip("Si está activo, la cámara no baja de 'minY' ni sube de 'maxY'. " +
             "Útil si hay un vacío abajo que no quieres mostrar. " +
             "Déjalo apagado para que baje libremente a plataformas inferiores.")]
    public bool useVerticalLimits = false;

    [Tooltip("Altura mínima de la cámara.")]
    public float minY = 0f;

    [Tooltip("Altura máxima de la cámara.")]
    public float maxY = 100f;

    [Header("Suavizado")]
    [Tooltip("Suavizado del eje Y al cambiar de altura. 0 = instantáneo. " +
             "Valores típicos 0.1–0.2. El eje X siempre es instantáneo.")]
    public float smoothTime = 0.08f;

    private float focusY;     // centro vertical de la zona muerta (en mundo)
    private float yVelocity;

    void Start()
    {
        focusY = player != null ? player.position.y : fixedY;
    }

    void LateUpdate()
    {
        if (player == null) return;

        float targetX = player.position.x + offset.x;

        // ── Altura objetivo con zona muerta ──────────────────────────────────
        float desiredY;
        if (followVertical)
        {
            float playerY = player.position.y;
            float half    = Mathf.Max(0f, deadZoneHeight * 0.5f);

            // El "foco" solo se mueve cuando el jugador sale de la banda.
            // Dentro de la banda la cámara permanece quieta en Y.
            if      (playerY > focusY + half) focusY = playerY - half;
            else if (playerY < focusY - half) focusY = playerY + half;

            desiredY = focusY + offset.y;

            if (useVerticalLimits)
                desiredY = Mathf.Clamp(desiredY, minY, maxY);
        }
        else
        {
            desiredY = fixedY;
        }

        // ── Aplicar: X instantáneo, Y suavizado ──────────────────────────────
        float newY = smoothTime > 0f
            ? Mathf.SmoothDamp(transform.position.y, desiredY, ref yVelocity, smoothTime)
            : desiredY;

        transform.position = new Vector3(targetX, newY, -10f);
    }

    // ── Visualizar la zona muerta en el editor ───────────────────────────────
    void OnDrawGizmosSelected()
    {
        if (!followVertical || deadZoneHeight <= 0f) return;

        Camera cam = GetComponent<Camera>();
        float width = cam != null && cam.orthographic ? cam.orthographicSize * 2f * cam.aspect : 20f;

        float centerY = Application.isPlaying ? focusY + offset.y : transform.position.y;
        Gizmos.color = new Color(1f, 0.9f, 0.2f, 0.6f);
        Vector3 c = new Vector3(transform.position.x, centerY, 0f);
        Gizmos.DrawWireCube(c, new Vector3(width, deadZoneHeight, 0f));
    }
}
