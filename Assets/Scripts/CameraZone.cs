using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Zona de cámara. Cada zona es un BoxCollider2D (trigger) que define los
/// límites (confiner) de la cámara mientras el jugador está dentro de ella.
///
/// Soporta MÚLTIPLES zonas, incluso solapadas: la cámara usa siempre la zona
/// de mayor 'priority' entre todas en las que el jugador está dentro en ese
/// momento. Así caminar/caer de una zona a otra (y volver) siempre deja la
/// cámara con la zona correcta.
///
/// ─── Setup en Unity ───────────────────────────────────────────────────────
/// 1. Un GameObject vacío por zona (ej. "ZonaSuperior", "ZonaInferior", …).
/// 2. Add Component → Box Collider 2D (se marca Is Trigger solo).
/// 3. Add Component → Camera Zone.
/// 4. Dimensionar cada caja a su área del nivel. El borde de la caja = hasta
///    dónde puede mostrar la cámara en esa zona.
/// 5. Que las zonas adyacentes se SOLAPEN un poco en la transición.
/// 6. (Opcional) En solapes, sube 'priority' a la zona que debe ganar.
/// 7. En la Main Camera (CameraFollow), asignar en 'bounds' la zona donde
///    empieza el jugador.
/// ─────────────────────────────────────────────────────────────────────────
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class CameraZone : MonoBehaviour
{
    [Tooltip("En zonas solapadas, gana la de mayor prioridad. " +
             "Ej: una sala pequeña dentro de un área grande va con prioridad mayor.")]
    public int priority = 0;

    [Tooltip("Tag del jugador.")]
    public string playerTag = "Player";

    private BoxCollider2D box;

    // Zonas en las que el jugador está dentro ahora mismo (todas las escenas).
    private static readonly List<CameraZone> active = new List<CameraZone>();

    void Awake()
    {
        box = GetComponent<BoxCollider2D>();
        box.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (!active.Contains(this)) active.Add(this);
        Apply();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        active.Remove(this);
        Apply();
    }

    void OnDisable()
    {
        // Evita que una zona destruida (cambio de escena) quede "activa".
        active.Remove(this);
    }

    /// <summary>Elige la zona activa de mayor prioridad y la aplica a la cámara.</summary>
    private static void Apply()
    {
        if (active.Count == 0) return;

        CameraFollow cam = Camera.main != null
            ? Camera.main.GetComponent<CameraFollow>()
            : null;
        if (cam == null) return;

        CameraZone best = null;
        foreach (CameraZone z in active)
            if (best == null || z.priority > best.priority)
                best = z;

        if (best != null)
            cam.bounds = best.box;
    }

    void OnDrawGizmos()
    {
        BoxCollider2D b = GetComponent<BoxCollider2D>();
        if (b == null) return;
        Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.5f);
        Gizmos.DrawWireCube(b.bounds.center, b.bounds.size);
    }
}
