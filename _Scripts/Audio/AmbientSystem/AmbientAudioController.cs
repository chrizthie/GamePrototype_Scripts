using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AmbientAudioController : MonoBehaviour
{
    [Header("Audio Routing")]
    [SerializeField] private AudioMixerGroup ambientMixerGroup;

    private AudioSource ambientSource;

    private AmbientProfile currentProfile;
    private AmbientZone currentZone;

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        ambientSource = gameObject.AddComponent<AudioSource>();

        ambientSource.playOnAwake = false;
        ambientSource.loop = true;
        ambientSource.spatialBlend = 0f;
        ambientSource.outputAudioMixerGroup =
            ambientMixerGroup;

        // Ambient starts empty.
        ambientSource.volume = 0f;
    }


    public void EnterZone(AmbientZone zone, AmbientProfile profile)
    {
        if (profile == null)
        {
            ClearAmbient();
            return;
        }

        currentZone = zone;

        // Already playing this profile.
        if (currentProfile == profile &&
            ambientSource.isPlaying)
        {
            return;
        }

        currentProfile = profile;

        ambientSource.clip =
            profile.AmbientClip;

        ambientSource.volume = 0f;

        ambientSource.Play();

        StartFade(
            profile.Volume,
            profile.FadeInSpeed
        );
    }


    public void ExitZone(AmbientZone zone)
    {
        // Ignore exits from zones that are no longer active.
        if (zone != currentZone)
            return;

        ClearAmbient();
    }


    public void ClearAmbient()
    {
        currentProfile = null;
        currentZone = null;

        StartFade(
            0f,
            1f
        );
    }


    private void StartFade(
        float targetVolume,
        float speed
    )
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine =
            StartCoroutine(
                FadeRoutine(
                    targetVolume,
                    speed
                )
            );
    }


    private IEnumerator FadeRoutine(
        float targetVolume,
        float speed
    )
    {
        while (
            ambientSource != null &&
            !Mathf.Approximately(
                ambientSource.volume,
                targetVolume
            )
        )
        {
            ambientSource.volume =
                Mathf.MoveTowards(
                    ambientSource.volume,
                    targetVolume,
                    speed * Time.deltaTime
                );

            yield return null;
        }

        if (ambientSource != null)
        {
            ambientSource.volume =
                targetVolume;

            if (targetVolume <= 0f)
            {
                ambientSource.Stop();
                ambientSource.clip = null;
            }
        }

        fadeCoroutine = null;
    }
}