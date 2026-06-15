using UnityEngine;

/// <summary>
/// Zona de cámara. Cada zona es un BoxCollider2D (trigger) que define los
/// límites (confiner) de la cámara mientras el jugador está dentro de ella.
/// Al entrar el jugador, la cámara pasa a confinarse a ESTA caja.
///
/// Permite, por ejemplo:
///   - Zona superior: borde inferior justo en el piso principal → no se ve
///     nada debajo del piso.
///   - Zona inferior: cubre la caída y el área de abajo → la cámara baja con
///     el jugador.
///
/// ─── Setup en Unity ───────────────────────────────────────────────────────
/// 1. Crear un GameObject vacío por zona (ej. "ZonaSuperior", "ZonaInferior").
/// 2. Add Component → Box Collider 2D (se marca Is Trigger solo).
/// 3. Add Component → Camera Zone (este script).
/// 4. Dimensionar cada caja para que cubra su área del nivel. El borde de la
///    caja = hasta dónde puede mostrar la cámara en esa zona.
/// 5. Que las zonas se SOLAPEN un poco en la transición (para que no quede un
///    hueco entre ambas).
/// 6. En la Main Camera (CameraFollow), asignar en 'bounds' la zona donde
///    empieza el jugador (ej. ZonaSuperior).
/// ─────────────────────────────────────────────────────────────────────────
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class CameraZone : MonoBehaviour
{
    [Tooltip("Tag del jugador.")]
    public string playerTag = "Player";

    private BoxCollider2D box;

    void Awake()
    {
        box = GetComponent<BoxCollider2D>();
        box.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        CameraFollow cam = Camera.main != null
            ? Camera.main.GetComponent<CameraFollow>()
            : null;

        if (cam != null)
            cam.bounds = box;
    }

    void OnDrawGizmos()
    {
        BoxCollider2D b = GetComponent<BoxCollider2D>();
        if (b == null) return;
        Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.5f);
        Gizmos.DrawWireCube(b.bounds.center, b.bounds.size);
    }
}
