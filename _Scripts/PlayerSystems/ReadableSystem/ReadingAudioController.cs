using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class ReadingAudioController : MonoBehaviour
{
    [Header("Audio Routing")]
    [SerializeField] private AudioMixerGroup readingMixerGroup;

    private class ActiveAudio
    {
        public AudioSource source;
        public float fadeOutSpeed;
        public Coroutine fadeInCoroutine;
    }

    private readonly List<ActiveAudio> activeAudio =
        new List<ActiveAudio>();


    public void PlayState(
        Readable readable,
        ReadableAudioState state
    )
    {
        if (readable == null)
            return;

        // Fade out whatever was previously playing
        FadeOutCurrentState();

        ReadableAudioData audioData =
            readable.AudioStates.Find(
                x => x.state == state
            );

        if (audioData == null)
            return;


        foreach (AudioClip clip in audioData.clips)
        {
            if (clip == null)
                continue;

            AudioSource source =
                gameObject.AddComponent<AudioSource>();

            source.outputAudioMixerGroup =
                readingMixerGroup;

            source.spatialBlend = 0f;

            source.clip = clip;
            source.loop = audioData.loop;
            source.volume = 0f;

            source.Play();

            ActiveAudio newActiveAudio =
                new ActiveAudio
                {
                    source = source,
                    fadeOutSpeed = audioData.fadeOutSpeed
                };

            activeAudio.Add(newActiveAudio);

            newActiveAudio.fadeInCoroutine =
                StartCoroutine(
                    FadeIn(
                        source,
                        audioData.volume,
                        audioData.fadeInSpeed
                    )
                );
        }
    }


    public void FadeOutCurrentState()
    {
        foreach (ActiveAudio audio in activeAudio)
        {
            if (audio.source == null)
                continue;

            // Stop the fade-in before fading out
            if (audio.fadeInCoroutine != null)
            {
                StopCoroutine(
                    audio.fadeInCoroutine
                );
            }

            StartCoroutine(
                FadeOutAndDestroy(
                    audio.source,
                    audio.fadeOutSpeed
                )
            );
        }

        activeAudio.Clear();
    }


    private IEnumerator FadeIn(AudioSource source, float targetVolume, float fadeSpeed)
    {
        while (
            source != null &&
            !Mathf.Approximately(
                source.volume,
                targetVolume
            )
        )
        {
            source.volume = Mathf.MoveTowards(
                source.volume,
                targetVolume,
                fadeSpeed * Time.deltaTime
            );

            yield return null;
        }

        if (source != null)
        {
            source.volume = targetVolume;
        }
    }

    private IEnumerator FadeOutAndDestroy(
        AudioSource source,
        float fadeSpeed
    )
    {
        while (
            source != null &&
            source.volume > 0f
        )
        {
            source.volume = Mathf.MoveTowards(
                source.volume,
                0f,
                fadeSpeed * Time.deltaTime
            );

            yield return null;
        }

        if (source != null)
        {
            Destroy(source);
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        foreach (ActiveAudio audio in activeAudio)
        {
            if (audio.source != null)
            {
                Destroy(audio.source);
            }
        }

        activeAudio.Clear();
    }

}