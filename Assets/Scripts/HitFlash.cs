using System.Collections;
using UnityEngine;

/// <summary>
/// Parpadeo de color al recibir daño.
/// Agregar al mismo GameObject que SpriteRenderer.
/// Llamar Flash() desde PlayerHealth.OnDamaged o EnemyHealth.OnDamaged.
/// </summary>
public class HitFlash : MonoBehaviour
{
    [Header("Flash")]
    public Color     flashColor    = Color.red;
    public float     flashDuration = 0.15f;
    public int       flashCount    = 2;

    private SpriteRenderer sr;
    private Color          originalColor;
    private Coroutine      flashCoroutine;

    void Awake()
    {
        sr            = GetComponent<SpriteRenderer>();
        originalColor = sr.color;
    }

    public void Flash()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(DoFlash());
    }

    IEnumerator DoFlash()
    {
        for (int i = 0; i < flashCount; i++)
        {
            sr.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            sr.color = originalColor;
            yield return new WaitForSeconds(flashDuration * 0.5f);
        }
        sr.color = originalColor;
        flashCoroutine = null;
    }
}