using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Sistema de mensajes de Armonía (la voz del dios). Muestra líneas de texto
/// con efecto máquina de escribir en un panel de UI. Soporta cola de líneas,
/// rich text (cursiva/color para la voz divina), auto-avance o avance manual,
/// y pausa opcional del juego.
///
/// Es un singleton: llámalo desde cualquier lado con
///     HarmonyDialogue.Instance.Speak("línea 1", "línea 2");
/// o desde un DialogueTrigger en el nivel.
///
/// ─── Setup en Unity ───────────────────────────────────────────────────────
/// 1. En el Canvas, crear un Panel (ej. "HarmonyPanel") con:
///      - Una Image de fondo semitransparente (opcional).
///      - Un TextMeshPro - Text (UI) para el mensaje.
///      - (Opcional) otro TMP para el nombre "Armonía" y un retrato.
/// 2. Crear un GameObject "HarmonyDialogue" con este script.
/// 3. Asignar 'panel' y 'text' (y 'speakerLabel' si lo usas).
/// ─────────────────────────────────────────────────────────────────────────
/// </summary>
public class HarmonyDialogue : MonoBehaviour
{
    public static HarmonyDialogue Instance { get; private set; }

    [Header("Referencias UI")]
    [Tooltip("Panel contenedor — se activa al hablar y se oculta al terminar.")]
    public GameObject panel;
    public TextMeshProUGUI text;
    [Tooltip("Opcional: etiqueta con el nombre de quien habla.")]
    public TextMeshProUGUI speakerLabel;
    public string speakerName = "Armonía";

    [Header("Máquina de escribir")]
    [Tooltip("Caracteres por segundo. Más bajo = más solemne.")]
    public float charsPerSecond = 28f;

    [Header("Avance")]
    [Tooltip("Si está activo, las líneas avanzan solas tras un tiempo de lectura. " +
             "Si está apagado, el jugador avanza manualmente.")]
    public bool autoAdvance = true;
    [Tooltip("Segundos de lectura antes de pasar a la siguiente línea (auto-avance).")]
    public float autoAdvanceDelay = 1.8f;
    [Tooltip("Tecla para avanzar/saltar (solo en avance manual).")]
    public KeyCode advanceKey = KeyCode.Space;
    public bool advanceOnClick = true;

    [Header("Pausa")]
    [Tooltip("Congela el juego mientras Armonía habla (útil para momentos clave).")]
    public bool pauseGameWhileTalking = false;

    private readonly Queue<string> lines = new Queue<string>();
    private Coroutine typeRoutine;
    private string    currentLine;
    private bool      isTyping;
    private bool      active;
    private int       beginFrame = -1;

    public bool IsActive => active;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (panel != null) panel.SetActive(false);
    }

    void Update()
    {
        if (!active || autoAdvance) return;

        // Ignorar el frame en que arrancó: evita que la tecla que inició el
        // diálogo salte también la primera línea.
        if (Time.frameCount == beginFrame) return;

        bool advance = Input.GetKeyDown(advanceKey) ||
                       (advanceOnClick && Input.GetMouseButtonDown(0));
        if (advance) OnAdvance();
    }

    // ── API pública ───────────────────────────────────────────────────────────

    /// <summary>Encola y muestra una o varias líneas de Armonía.</summary>
    public void Speak(params string[] newLines)
    {
        if (newLines == null || newLines.Length == 0) return;
        foreach (string l in newLines) lines.Enqueue(l);
        if (!active) BeginDialogue();
    }

    // ── Flujo interno ──────────────────────────────────────────────────────────

    void BeginDialogue()
    {
        active     = true;
        beginFrame = Time.frameCount;
        if (panel != null)        panel.SetActive(true);
        if (speakerLabel != null) speakerLabel.text = speakerName;
        if (pauseGameWhileTalking) Time.timeScale = 0f;
        ShowNext();
    }

    void ShowNext()
    {
        if (lines.Count == 0) { EndDialogue(); return; }
        currentLine = lines.Dequeue();
        if (typeRoutine != null) StopCoroutine(typeRoutine);
        typeRoutine = StartCoroutine(TypeLine(currentLine));
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;

        // Usar maxVisibleCharacters preserva el rich text (cursiva/color).
        text.text = line;
        text.ForceMeshUpdate();
        int total = text.textInfo.characterCount;
        text.maxVisibleCharacters = 0;

        float delay = charsPerSecond > 0f ? 1f / charsPerSecond : 0f;
        for (int i = 1; i <= total; i++)
        {
            text.maxVisibleCharacters = i;
            yield return new WaitForSecondsRealtime(delay);
        }

        text.maxVisibleCharacters = total;
        isTyping = false;

        if (autoAdvance)
        {
            yield return new WaitForSecondsRealtime(autoAdvanceDelay);
            ShowNext();
        }
    }

    void OnAdvance()
    {
        if (isTyping)
        {
            // Completar la línea de golpe.
            if (typeRoutine != null) StopCoroutine(typeRoutine);
            text.maxVisibleCharacters = text.textInfo.characterCount;
            isTyping = false;
        }
        else
        {
            ShowNext();
        }
    }

    void EndDialogue()
    {
        active = false;
        if (panel != null) panel.SetActive(false);
        if (pauseGameWhileTalking) Time.timeScale = 1f;
    }
}
