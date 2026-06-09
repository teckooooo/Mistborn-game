using UnityEngine;

public class MetalObject : MonoBehaviour
{
    public float metalMass = 0.5f;

    [HideInInspector] public float anchoredMass = 0f; // masa del objeto en que está clavado

    /// <summary>
    /// Disparado cuando Steel Push o Iron Pull aplican fuerza sobre este objeto.
    /// El parámetro es allomanticStrength * Time.deltaTime del frame actual.
    /// GuardianWeapon lo usa para acumular fuerza y detectar el desarme.
    /// </summary>
    public System.Action<float> OnAllomancyForce;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void ApplyForce(Vector2 force)
    {
        if (rb != null)
            rb.AddForce(force, ForceMode2D.Impulse);
    }

    // Masa efectiva = masa propia + masa del objeto anclado
    public float EffectiveMass => metalMass + anchoredMass;
}