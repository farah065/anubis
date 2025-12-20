using UnityEngine;
using UnityEngine.Audio;

public class Settings : Singleton<Settings>
{
    [SerializeField] private AudioMixer _audioMixer;

    public void SetFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }

    public void SetMasterVolume(float volume)
    {
        float logVolume = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f;
        _audioMixer.SetFloat("MasterVolume", logVolume);
    }

    public void SetMusicVolume(float volume)
    {
        float logVolume = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f;
        _audioMixer.SetFloat("MusicVolume", logVolume);
    }

    public void SetSFXVolume(float volume)
    {
        float logVolume = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f;
        _audioMixer.SetFloat("SfxVolume", logVolume);
    }
}
