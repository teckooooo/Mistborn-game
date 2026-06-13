using UnityEngine;

public class AllomancyStats : MonoBehaviour
{
    [Header("Fuerza Alomántica")]
    [Tooltip("Fuerza base del jugador al quemar Acero o Hierro")]
    [Range(1f, 100f)]
    public float allomanticStrength = 6f;

}