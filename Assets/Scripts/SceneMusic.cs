using UnityEngine;

/// <summary>
/// Pega este componente a un GameObject de la escena (puede ser el mismo
/// MenuManager o el Background) y asigna el clip de música. Al cargar la
/// escena le pide al AudioManager que reproduzca ese clip.
///
/// Si la misma música ya está sonando (volviste a la misma escena), no se
/// reinicia — sigue donde iba.
/// </summary>
public class SceneMusic : MonoBehaviour
{
    [Tooltip("Música de fondo que sonará en esta escena.")]
    public AudioClip musicClip;

    [Tooltip("Si está marcado y no hay clip asignado, detiene la música actual " +
             "(útil para escenas sin música, ej. cinemáticas silenciosas).")]
    public bool stopIfNoClip = false;

    void Start()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("[SceneMusic] No hay AudioManager en la escena. " +
                             "Agrega un GameObject con AudioManager a MainMenu.");
            return;
        }

        if (musicClip != null)
            AudioManager.Instance.PlayMusic(musicClip);
        else if (stopIfNoClip)
            AudioManager.Instance.StopMusic();
    }
}
