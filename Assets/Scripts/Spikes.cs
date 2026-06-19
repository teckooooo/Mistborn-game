using UnityEngine;

/// <summary>
/// Pinchos que dañan al jugador por contacto. Usa PlayerHealth (que ya maneja
/// invencibilidad y knockback). Funciona con collider trigger o sólido.
///
///   - instantKill = true  → mata al jugador al instante (pinchos mortales).
///   - instantKill = false → resta 'damage'; el ritmo de daño al quedarse encima
///                           lo limita la invencibilidad del jugador (invincibleTime).
///
/// ─── Setup en Unity ───────────────────────────────────────────────────────
/// 1. GameObject con SpriteRenderer (sprite de pinchos) y Collider2D.
///    - Trigger ON  → el jugador puede atravesarlos pero recibe daño.
///    - Trigger OFF → sólidos (el jugador choca/se para encima).
/// 2. Add Component → Spikes.
/// El jugador necesita PlayerHealth + tag "Player" + Collider2D + Rigidbody2D.
/// ─────────────────────────────────────────────────────────────────────────
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Spikes : MonoBehaviour
{
    [Header("Daño")]
    [Tooltip("Mata al jugador al instante, ignorando invencibilidad. Para pinchos mortales.")]
    public bool instantKill = false;

    [Tooltip("Daño por golpe (si no es instantKill).")]
    public float damage = 20f;

    [Tooltip("Tag del jugador.")]
    public string playerTag = "Player";

    // Soporta collider trigger y sólido. Enter = primer golpe; Stay = repite
    // (la invencibilidad del jugador evita el spam de daño).
    void OnTriggerEnter2D(Collider2D other)  => Damage(other);
    void OnTriggerStay2D(Collider2D other)   => Damage(other);
    void OnCollisionEnter2D(Collision2D c)   => Damage(c.collider);
    void OnCollisionStay2D(Collision2D c)    => Damage(c.collider);

    void Damage(Collider2D other)
    {
        if (other == null || !other.CompareTag(playerTag)) return;

        PlayerHealth hp = other.GetComponent<PlayerHealth>();
        if (hp == null) return;

        if (instantKill) hp.Kill();
        else             hp.TakeDamage(damage, transform);
    }
}
