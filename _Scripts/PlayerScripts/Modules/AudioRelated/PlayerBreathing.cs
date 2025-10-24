using UnityEngine;
using System.Collections;

public class PlayerBreathing : MonoBehaviour
{
    [Header("Audio Settings")]
    [Range(0.1f, 2f)] public float fadeDuration = 0.5f;

    [Header("Breathing Audio Clips")]
    [SerializeField] AudioClip normalBreathing;
    [SerializeField] AudioClip moderateBreathing;
    [SerializeField] AudioClip heavyBreathing;

    [Header("Required Components")]
    [SerializeField] StaminaSystem staminaSystem;
    [SerializeField] AudioSource breathingAudioSource;

    private AudioClip currentClip;
    private Coroutine fadeRoutine;

    private void UpdateBreathing()
    {
        AudioClip selectedClip = null;

        if (staminaSystem.playerStamina > 70f)
        {
            selectedClip = normalBreathing;
        }
        else if (staminaSystem.playerStamina > 30f)
        {
            selectedClip = moderateBreathing;
        }
        else
        {
            selectedClip = heavyBreathing;
        }

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

        // Fade out
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            breathingAudioSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }

        breathingAudioSource.volume = 0f;
        breathingAudioSource.clip = newClip;
        breathingAudioSource.loop = true;
        breathingAudioSource.Play();

        // Fade in
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            breathingAudioSource.volume = Mathf.Lerp(0f, startVolume, t / fadeDuration);
            yield return null;
        }

        breathingAudioSource.volume = startVolume;
    }

    #region Unity Methods 

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
