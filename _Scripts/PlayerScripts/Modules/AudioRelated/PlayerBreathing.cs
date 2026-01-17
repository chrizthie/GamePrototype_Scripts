using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

public class PlayerBreathing : MonoBehaviour
{
    [Header("Audio Settings")]
    [Range(0.1f, 2f)] public float fadeDuration = 0.6f;
    [Range(0f, 1f)] public float targetVolume = 0.7f;

    [Header("Breathing Audio Clips")]
    [SerializeField] AudioClip normalBreathing;
    [SerializeField] AudioClip moderateBreathing;
    [SerializeField] AudioClip heavyBreathing;

    [Header("Noise Transition Smoothing")]
    [SerializeField] public float smoothSpeed = 5f;
    private float targetAmplitude;
    private float targetFrequency;

    [Header("Required Components")]
    [SerializeField] StaminaSystem staminaSystem;
    [SerializeField] public AudioSource breathingAudioSource;
    [SerializeField] public CinemachineCamera virtualCamera;
    [SerializeField] CinemachineBasicMultiChannelPerlin cameraNoise;

    private AudioClip currentClip;
    private Coroutine fadeRoutine;

    private void UpdateBreathing()
    {
        AudioClip selectedClip = null;

        if (staminaSystem.playerStamina > 50f)
        {
            selectedClip = normalBreathing;
            targetAmplitude = 0.5f;
            targetFrequency = 0.5f;
        }
        else if (staminaSystem.playerStamina < 50f)
        {
            selectedClip = moderateBreathing;
            targetAmplitude = 0.6f;
            targetFrequency = 0.6f;
        }
        //else
        {
            //selectedClip = heavyBreathing;
            //targetAmplitude = 0.7f;
            //targetFrequency = 0.7f;
        }

        // Smoothly move current toward target
        cameraNoise.AmplitudeGain = Mathf.Lerp(cameraNoise.AmplitudeGain, targetAmplitude, Time.deltaTime * smoothSpeed);
        cameraNoise.FrequencyGain = Mathf.Lerp(cameraNoise.FrequencyGain, targetFrequency, Time.deltaTime * smoothSpeed);

        /// Only change clip if it’s actually different
        if (selectedClip != currentClip)
        {
            currentClip = selectedClip;
            FadeToNewClip(selectedClip);
        }
    }

    private void FadeToNewClip(AudioClip newClip)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeAudio(newClip));
    }

    private IEnumerator FadeAudio(AudioClip newClip)
    {
        float startVolume = breathingAudioSource.volume;

        // fade out
        for (float t = 0f; t < fadeDuration; t += Time.deltaTime)
        {
            breathingAudioSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }

        // ensure it’s at 0
        breathingAudioSource.volume = 0f;

        // change clip
        breathingAudioSource.clip = newClip;
        breathingAudioSource.loop = true;
        breathingAudioSource.Play();

        // fade in from 0 to targetVolume
        for (float t = 0f; t < fadeDuration; t += Time.deltaTime)
        {
            breathingAudioSource.volume = Mathf.Lerp(0f, targetVolume, t / fadeDuration);
            yield return null;
        }

        breathingAudioSource.volume = targetVolume;
        fadeRoutine = null;
    }

    #region Unity Methods 

    private void Start()
    {
        if (virtualCamera != null)
        {
            cameraNoise = virtualCamera.GetComponentInParent<CinemachineBasicMultiChannelPerlin>();
        }
    }

    private void Update()
    {
        if (staminaSystem == null || breathingAudioSource == null)
        {
            return;
        }

        UpdateBreathing();
    }

    private void OnValidate()
    {
        if (staminaSystem == null)
        {
            staminaSystem = GetComponentInParent<StaminaSystem>();
        }
    }

    #endregion
}
