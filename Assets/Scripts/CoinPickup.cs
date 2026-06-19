using UnityEngine;

/// <summary>
/// Moneda coleccionable del mapa. Al tocarla el jugador, suma monedas a su
/// inventario y desaparece. Es distinta de la moneda proyectil (Coin): esta
/// solo se recoge, no se lanza ni hace daño.
///
/// Si el inventario está lleno, la moneda se queda en el mapa (no se recoge),
/// para que el jugador vuelva por ella cuando tenga espacio.
///
/// ─── Setup en Unity ───────────────────────────────────────────────────────
/// 1. GameObject con SpriteRenderer (sprite de moneda, opcional animado).
/// 2. Collider2D con 'Is Trigger' MARCADO (se marca solo al añadir este script).
/// 3. Add Component → Coin Pickup.
/// 4. Guardar como prefab aparte (ej. "MonedaMapa").
/// ─────────────────────────────────────────────────────────────────────────
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CoinPickup : MonoBehaviour
{
    [Tooltip("Cuántas monedas otorga al recogerla.")]
    public int amount = 1;

    [Tooltip("Tag del jugador.")]
    public string playerTag = "Player";

    [Tooltip("Efecto opcional al recoger (partículas/destello).")]
    public GameObject pickupEffect;

    void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        PlayerInventory inv = other.GetComponent<PlayerInventory>();
        if (inv == null) return;

        // AddCoins devuelve false si el inventario está lleno → la dejamos.
        if (inv.AddCoins(amount))
        {
            if (pickupEffect != null)
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
