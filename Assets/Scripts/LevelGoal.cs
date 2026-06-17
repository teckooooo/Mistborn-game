using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Meta de fin de nivel. Cuando el jugador entra en este trigger, marca el
/// nivel actual como completado (desbloquea el siguiente) y avanza.
///
/// ─── Setup en Unity ───────────────────────────────────────────────────────
/// 1. Crear un GameObject al final del nivel (ej. "Meta").
/// 2. Add Component → Box Collider 2D (se marca Is Trigger solo al añadir este
///    script). Dimensiónalo como zona de meta (puerta, bandera, portal…).
/// 3. Add Component → Level Goal (este script).
/// 4. Poner 'Level Number' = número de ESTE nivel (1, 2, 3…).
/// ─────────────────────────────────────────────────────────────────────────
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class LevelGoal : MonoBehaviour
{
    public enum NextAction { LoadNextLevel, GoToLevelSelect, LoadSpecificScene }

    [Tooltip("Número de ESTE nivel (1, 2, 3…). Debe coincidir con el menú.")]
    public int levelNumber = 1;

    [Tooltip("Qué hacer al completar el nivel.")]
    public NextAction onComplete = NextAction.LoadNextLevel;

    [Tooltip("Nombre de la escena a cargar (solo si onComplete = Load Specific Scene).")]
    public string specificSceneName = "";

    [Tooltip("Tag del jugador.")]
    public string playerTag = "Player";

    private bool triggered;

    void Reset()
    {
        // Al añadir el componente, dejar el collider como trigger.
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag(playerTag)) return;
        triggered = true;

        Debug.Log($"[LevelGoal] Nivel {levelNumber} completado.");
        LevelManager.CompleteLevel(levelNumber);

        switch (onComplete)
        {
            case NextAction.LoadNextLevel:
                LevelManager.LoadNextLevel(levelNumber);
                break;

            case NextAction.GoToLevelSelect:
                LevelManager.LoadLevelSelect();
                break;

            case NextAction.LoadSpecificScene:
                if (!string.IsNullOrEmpty(specificSceneName))
                    SceneManager.LoadScene(specificSceneName);
                else
                    LevelManager.LoadLevelSelect();
                break;
        }
    }

    void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) return;
        Gizmos.color = new Color(0.2f, 1f, 0.35f, 0.25f);
        Gizmos.DrawCube(col.bounds.center, col.bounds.size);
        Gizmos.color = new Color(0.2f, 1f, 0.35f, 0.9f);
        Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
    }
}
