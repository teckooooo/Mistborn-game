using UnityEngine;

/// <summary>
/// Fondo con parallax para plataformero 2D.
/// Acoplar a cada capa de fondo (GameObject con SpriteRenderer).
///
/// ─── Cómo configurarlo ────────────────────────────────────────────────────
/// 1. Crear capas como hijas del nivel (NO de la cámara): cielo, montañas
///    lejanas, edificios, primer plano. Asignar Sorting Layer / Order in Layer
///    para apilarlas correctamente (cielo al fondo).
/// 2. En cada capa, agregar este componente.
/// 3. Ajustar parallaxFactor:
///       0.0  = capa totalmente estática (ej. cielo plano)
///       0.2  = montañas muy lejanas
///       0.5  = colina intermedia
///       0.8  = edificios cercanos
///       1.0  = se mueve igual que el mundo (no es parallax, es escenario)
/// 4. Marcar infiniteHorizontal en capas que deban repetirse al moverse la
///    cámara. El sprite debe ser tileable horizontalmente para que se vea bien.
/// ─────────────────────────────────────────────────────────────────────────
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class ParallaxBackground : MonoBehaviour
{
    [Header("Velocidad de parallax")]
    [Tooltip("0 = capa fija. 1 = se mueve igual que la cámara (sin parallax). " +
             "Valores típicos: cielo 0, montañas 0.2, edificios 0.6.")]
    [Range(0f, 1f)] public float parallaxFactor = 0.5f;

    [Tooltip("Si está activo aplica parallax también en el eje Y. " +
             "Como la cámara del juego tiene Y fijo, normalmente déjalo en false.")]
    public bool verticalParallax = false;

    [Header("Repetición infinita")]
    [Tooltip("Cuando la cámara se aleja, la capa se reposiciona para repetirse. " +
             "Activar solo si el sprite es tileable horizontalmente.")]
    public bool infiniteHorizontal = false;

    [Tooltip("Cuántos sprites idénticos hay puestos lado a lado para esta capa " +
             "(este sprite es uno de ellos). El teletransporte salta ese múltiplo " +
             "para no superponer copias. Si solo hay 1 sprite, deja 1.")]
    [Range(1, 9)] public int tileCount = 1;

    [Header("Cámara objetivo (opcional)")]
    [Tooltip("Si está vacío usa Camera.main automáticamente.")]
    public Transform targetCamera;

    private Vector3 startPos;
    private Vector3 lastCamPos;
    private float   spriteWidth;

    void Start()
    {
        if (targetCamera == null && Camera.main != null)
            targetCamera = Camera.main.transform;

        if (targetCamera == null)
        {
            Debug.LogError($"[ParallaxBackground] '{name}' no encontró Camera.main. " +
                           "Asignar 'targetCamera' manualmente o etiquetar la cámara como MainCamera.");
            enabled = false;
            return;
        }

        startPos     = transform.position;
        lastCamPos   = targetCamera.position;

        if (infiniteHorizontal)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            spriteWidth = sr.bounds.size.x;
            if (spriteWidth <= 0f)
            {
                Debug.LogWarning($"[ParallaxBackground] '{name}' tiene ancho 0; " +
                                 "desactivando infiniteHorizontal.");
                infiniteHorizontal = false;
            }
        }
    }

    void LateUpdate()
    {
        if (targetCamera == null) return;

        Vector3 camDelta = targetCamera.position - lastCamPos;

        // Desplazamiento — parallaxFactor 0 => la capa se queda fija respecto al
        // mundo (la cámara la "pasa"); parallaxFactor 1 => se mueve igual que
        // la cámara (efectivamente queda pegada).
        Vector3 move = new Vector3(
            camDelta.x * parallaxFactor,
            verticalParallax ? camDelta.y * parallaxFactor : 0f,
            0f);

        transform.position += move;
        lastCamPos = targetCamera.position;

        if (infiniteHorizontal)
            RecenterIfNeeded();
    }

    /// <summary>
    /// Reposiciona el sprite a la derecha o izquierda cuando la cámara se aleja
    /// más allá de la mitad de la "fila" de tiles. Salta tileCount × spriteWidth
    /// para aterrizar al otro lado de las copias hermanas sin superponerse.
    /// </summary>
    void RecenterIfNeeded()
    {
        if (tileCount < 1) tileCount = 1;

        float dx          = targetCamera.position.x - transform.position.x;
        float threshold   = spriteWidth * tileCount * 0.5f;
        float teleportBy  = spriteWidth * tileCount;

        // while por si la cámara se mueve mucho en un frame (debug/teleports)
        while (dx > threshold)
        {
            transform.position += new Vector3(teleportBy, 0f, 0f);
            dx -= teleportBy;
        }
        while (dx < -threshold)
        {
            transform.position -= new Vector3(teleportBy, 0f, 0f);
            dx += teleportBy;
        }
    }

    void OnDrawGizmosSelected()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;
        Gizmos.color = new Color(0.4f, 0.8f, 1f, 0.5f);
        Gizmos.DrawWireCube(transform.position, sr.bounds.size);
    }
}
