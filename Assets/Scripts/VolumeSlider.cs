using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Conecta un Slider de UI con el AudioManager para controlar volumen.
/// Pega este componente al mismo GameObject que tiene el Slider y elige
/// el tipo (Music o SFX). Al mover el slider, el volumen cambia en vivo
/// y se guarda en PlayerPrefs.
///
/// El Slider debe configurarse con: Min Value = 0, Max Value = 1.
/// </summary>
[RequireComponent(typeof(Slider))]
public class VolumeSlider : MonoBehaviour
{
    public enum AudioChannel { Music, SFX }

    [Tooltip("Qué volumen controla este slider.")]
    public AudioChannel channel = AudioChannel.Music;

    private Slider slider;

    void Awake()
    {
        slider = GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.onValueChanged.AddListener(OnValueChanged);
    }

    void Start()
    {
        // Sincronizar el slider con el valor actual del AudioManager
        if (AudioManager.Instance == null) return;

        float current = channel == AudioChannel.Music
            ? AudioManager.Instance.musicVolume
            : AudioManager.Instance.sfxVolume;

        // SetValueWithoutNotify para que no dispare el callback al cargar
        slider.SetValueWithoutNotify(current);
    }

    void OnValueChanged(float value)
    {
        if (AudioManager.Instance == null) return;

        if (channel == AudioChannel.Music)
            AudioManager.Instance.SetMusicVolume(value);
        else
            AudioManager.Instance.SetSFXVolume(value);
    }
}
