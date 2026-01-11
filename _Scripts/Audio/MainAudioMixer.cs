using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using static UnityEngine.Rendering.DebugUI;

public class MainAudioMixer : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;

    public static float LinearToDecibel(float value)
    {
        return Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;
    }

    public void SetMasterVolume(float value)
    {
        audioMixer.SetFloat("masterVolume", LinearToDecibel(value));
    }

    public void SetSoundFXVolume(float value)
    {
        audioMixer.SetFloat("sfxVolume", LinearToDecibel(value));
    }

    public void SetMusicVolume(float value)
    {
        audioMixer.SetFloat("musicVolume", LinearToDecibel(value));
    }

}
