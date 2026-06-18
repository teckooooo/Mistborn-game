using UnityEngine;

/// <summary>
/// Zona que dispara un mensaje de Armonía. Dos modos:
///   - AlEntrar: el mensaje sale solo cuando el jugador entra.
///   - PresionarTecla: al entrar muestra un aviso ("Presiona [tecla]"); el
///     mensaje empieza cuando el jugador pulsa la tecla.
///
/// El avance entre líneas lo maneja HarmonyDialogue (pon su 'Auto Advance' en
/// off y su 'Advance Key' a la misma tecla para una experiencia coherente).
///
/// ─── Setup en Unity ───────────────────────────────────────────────────────
/// 1. GameObject vacío + Box Collider 2D (se marca Is Trigger solo).
/// 2. Add Component → Dialogue Trigger.
/// 3. Modo PresionarTecla → asignar 'Prompt Object' (un texto/UI "Presiona F").
/// 4. Escribir las líneas en 'Lines'.
/// Requiere un HarmonyDialogue en la escena.
/// ─────────────────────────────────────────────────────────────────────────
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DialogueTrigger : MonoBehaviour
{
    public enum Mode { AlEntrar, PresionarTecla }

    [Header("Activación")]
    public Mode mode = Mode.PresionarTecla;

    [Tooltip("Tecla para iniciar el diálogo (modo PresionarTecla). " +
             "Evita Space/E (saltar/lanzar). F o Return funcionan bien.")]
    public KeyCode startKey = KeyCode.F;

    [Tooltip("Objeto de aviso (ej. texto 'Presiona F'). Se muestra al entrar y " +
             "se oculta al iniciar el diálogo o al salir de la zona.")]
    public GameObject promptObject;

    [Header("Mensaje de Armonía")]
    [Tooltip("Cada entrada es una línea. Soporta rich text: " +
             "<i>cursiva</i>, <color=#9ad8ff>color</color>.")]
    [TextArea(2, 4)]
    public string[] lines;

    [Tooltip("Si está activo, solo se dispara una vez.")]
    public bool once = true;

    [Tooltip("Retraso (seg) antes de mostrar el mensaje (solo modo AlEntrar).")]
    public float delay = 0f;

    public string playerTag = "Player";

    private bool used;
    private bool playerInRange;

    void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void Start()
    {
        if (promptObject != null) promptObject.SetActive(false);
    }

    void Update()
    {
        if (mode != Mode.PresionarTecla) return;
        if (!playerInRange || (used && once)) return;

        if (Input.GetKeyDown(startKey))
        {
            HidePrompt();
            Fire();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (used && once) return;

        playerInRange = true;

        if (mode == Mode.AlEntrar)
        {
            if (delay > 0f) Invoke(nameof(Fire), delay);
            else            Fire();
        }
        else // PresionarTecla
        {
            if (promptObject != null) promptObject.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInRange = false;
        HidePrompt();
    }

    void Fire()
    {
        if (used && once) return;
        used = true;
        HidePrompt();

        if (HarmonyDialogue.Instance == null)
        {
            Debug.LogWarning("[DialogueTrigger] No hay HarmonyDialogue en la escena.");
            return;
        }
        if (lines != null && lines.Length > 0)
            HarmonyDialogue.Instance.Speak(lines);
    }

    void HidePrompt()
    {
        if (promptObject != null) promptObject.SetActive(false);
    }

    void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) return;
        Gizmos.color = new Color(0.5f, 0.8f, 1f, 0.25f);
        Gizmos.DrawCube(col.bounds.center, col.bounds.size);
        Gizmos.color = new Color(0.5f, 0.8f, 1f, 0.8f);
        Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
    }
}
