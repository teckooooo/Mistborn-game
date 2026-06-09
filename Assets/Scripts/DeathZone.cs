using UnityEngine;

/// <summary>
/// Zona de muerte instantánea. Cualquier objeto con tag "Player" que toque
/// este collider muere inmediatamente (la escena se reinicia).
///
/// Funciona con colliders normales o triggers — soporta ambos eventos.
///
/// ─── Setup en Unity ───────────────────────────────────────────────────────
/// 1. Agregar este componente al GameObject (debe tener un Collider2D).
/// 2. (Opcional) Marca "Is Trigger" en el Collider2D si quieres que el
///    jugador caiga dentro de la zona antes de morir (efecto vacío/lava).
///    Si lo dejas sin trigger, el jugador rebota contra él y muere al chocar.
/// ─────────────────────────────────────────────────────────────────────────
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DeathZone : MonoBehaviour
{
    [Tooltip("Tag del objeto que debe morir al tocar la zona.")]
    public string targetTag = "Player";

    void OnTriggerEnter2D(Collider2D other)
    {
        TryKill(other.gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        TryKill(collision.gameObject);
    }

    void TryKill(GameObject hit)
    {
        if (!hit.CompareTag(targetTag)) return;

        PlayerHealth health = hit.GetComponent<PlayerHealth>();
        if (health == null)
        {
            Debug.LogWarning($"[DeathZone] '{hit.name}' tiene tag '{targetTag}' " +
                             "pero no PlayerHealth — no se puede matar.");
            return;
        }

        health.Kill();
    }
}
