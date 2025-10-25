using System.Collections.Generic;
using UnityEngine;

public class PlayerVoice : MonoBehaviour
{
    [Header("Landing Grunts Audio Clips")]
    [SerializeField] private List<AudioClip> landingGruntSounds = new List<AudioClip>(); // Voice collection played when landing

    [Header("Required Components")]
    [SerializeField] AudioSource voiceAudioSource;
    [SerializeField] PlayerBreathing playerBreathing;

    [Header("Fade Settings")]
    public float fadeDuration = 0.3f;     // seconds for smooth transition
    public float holdMuteTime = 0.3f;     // how long to stay muted after voice stops

    private float originalBreathVolume;
    private float fadeVelocity;
    private float targetVolume;
    private float lastVoiceTime;

    public void PlayLandingGruntAudio()
    {
        int n = Random.Range(1, landingGruntSounds.Count);
        voiceAudioSource.clip = landingGruntSounds[n];
        voiceAudioSource.PlayOneShot(voiceAudioSource.clip);
        // Move picked sound to index 0 so it's not picked next time
        landingGruntSounds[n] = landingGruntSounds[0];
        landingGruntSounds[0] = voiceAudioSource.clip;
    }

    #region Unity Methods

    private void Start()
    {
        if (playerBreathing.breathingAudioSource != null)
        {
            originalBreathVolume = playerBreathing.breathingAudioSource.volume;
        }
    }

    private void Update()
    {
        if (voiceAudioSource == null || playerBreathing.breathingAudioSource == null)
        {
            return;
        }

        // Record the last time the voice was playing
        if (voiceAudioSource.isPlaying)
        {
            lastVoiceTime = Time.time;
        }

        // Stay muted while voice is playing, or within the holdMuteTime after it stops
        bool shouldMute = voiceAudioSource.isPlaying || Time.time - lastVoiceTime < holdMuteTime;
        targetVolume = shouldMute ? 0f : originalBreathVolume;

        // Smooth transition to target volume
        playerBreathing.breathingAudioSource.volume = Mathf.SmoothDamp(playerBreathing.breathingAudioSource.volume, targetVolume, ref fadeVelocity, fadeDuration);
    }

    private void Awake()
    {
        if (playerBreathing == null)
        {
            playerBreathing = GetComponent<PlayerBreathing>();
        }
    }

    #endregion
}
