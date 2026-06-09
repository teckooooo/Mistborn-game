using UnityEngine;

/// <summary>
/// Singleton de audio. Persiste entre escenas para que la música no se corte
/// al cambiar de menú/nivel. Maneja una sola pista de música y SFX one-shot.
///
/// ─── Setup ────────────────────────────────────────────────────────────────
/// 1. Crear un GameObject vacío "AudioManager" en la escena MainMenu.
/// 2. Agregarle este componente. Los AudioSource hijos se crean solos.
/// 3. (Opcional) Asignar defaultClickClip en el Inspector — los ButtonClickSound
///    que no tengan clip propio usarán este.
/// ─────────────────────────────────────────────────────────────────────────
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Volúmenes (0-1)")]
    [Range(0f, 1f)] public float musicVolume = 0.6f;
    [Range(0f, 1f)] public float sfxVolume   = 0.8f;

    [Header("Click por defecto (opcional)")]
    [Tooltip("Si un ButtonClickSound no tiene clip propio asignado, usa este.")]
    public AudioClip defaultClickClip;

    private AudioSource musicSource;
    private AudioSource sfxSource;
    private AudioClip   currentMusicClip;

    private const string PrefMusic = "audio_music_volume";
    private const string PrefSFX   = "audio_sfx_volume";

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Cargar volúmenes guardados (si existen)
        if (PlayerPrefs.HasKey(PrefMusic)) musicVolume = PlayerPrefs.GetFloat(PrefMusic);
        if (PlayerPrefs.HasKey(PrefSFX))   sfxVolume   = PlayerPrefs.GetFloat(PrefSFX);

        // Crear AudioSources hijos
        GameObject musicGO = new GameObject("MusicSource");
        musicGO.transform.SetParent(transform);
        musicSource              = musicGO.AddComponent<AudioSource>();
        musicSource.loop         = true;
        musicSource.playOnAwake  = false;
        musicSource.volume       = musicVolume;

        GameObject sfxGO = new GameObject("SFXSource");
        sfxGO.transform.SetParent(transform);
        sfxSource             = sfxGO.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.volume      = sfxVolume;
    }

    void Update()
    {
        // Mantener volúmenes en vivo (útil para ajustar desde Inspector)
        if (musicSource != null) musicSource.volume = musicVolume;
        if (sfxSource   != null) sfxSource.volume   = sfxVolume;
    }

    // ── API pública ───────────────────────────────────────────────────────────

    /// <summary>Reproduce música en loop. Si ya está sonando el mismo clip, no reinicia.</summary>
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null) return;
        if (currentMusicClip == clip && musicSource.isPlaying) return;

        currentMusicClip = clip;
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
        currentMusicClip = null;
    }

    /// <summary>Reproduce un efecto one-shot. No interrumpe otros SFX.</summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    /// <summary>Conveniencia para ButtonClickSound — usa el clip por defecto si no se le pasa uno.</summary>
    public void PlayClick(AudioClip overrideClip = null)
    {
        PlaySFX(overrideClip != null ? overrideClip : defaultClickClip);
    }

    // ── Setters con persistencia ──────────────────────────────────────────────

    /// <summary>Cambia el volumen de la música (0-1) y lo guarda en PlayerPrefs.</summary>
    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        if (musicSource != null) musicSource.volume = musicVolume;
        PlayerPrefs.SetFloat(PrefMusic, musicVolume);
    }

    /// <summary>Cambia el volumen de los SFX (0-1) y lo guarda en PlayerPrefs.</summary>
    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        if (sfxSource != null) sfxSource.volume = sfxVolume;
        PlayerPrefs.SetFloat(PrefSFX, sfxVolume);
    }
}
