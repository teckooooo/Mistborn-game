using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pega este componente a un Button. Cuando el usuario hace click suena el
/// efecto. Si no se asigna un clip propio, usa el defaultClickClip del
/// AudioManager.
///
/// Se hookea solo — no necesitas configurar el onClick en el Inspector.
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonClickSound : MonoBehaviour
{
    [Tooltip("Clip a reproducir al hacer click. Si lo dejas vacío, usa el " +
             "defaultClickClip del AudioManager.")]
    public AudioClip clickClip;

    void Awake()
    {
        Button btn = GetComponent<Button>();
        btn.onClick.AddListener(PlayClick);
    }

    void PlayClick()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayClick(clickClip);
    }
}
