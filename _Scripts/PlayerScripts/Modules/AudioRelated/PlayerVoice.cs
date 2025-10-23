using System.Collections.Generic;
using UnityEngine;

public class PlayerVoice : MonoBehaviour
{
    [Header("Landing Grunts Audio Clips")]
    [SerializeField] private List<AudioClip> landingGruntSounds = new List<AudioClip>(); // Voice collection played when landing

    [Header("Required Components")]
    [SerializeField] AudioSource voiceAudioSource;
    public void PlayLandingGruntAudio()
    {
        int n = Random.Range(1, landingGruntSounds.Count);
        voiceAudioSource.clip = landingGruntSounds[n];
        voiceAudioSource.PlayOneShot(voiceAudioSource.clip);
        // Move picked sound to index 0 so it's not picked next time
        landingGruntSounds[n] = landingGruntSounds[0];
        landingGruntSounds[0] = voiceAudioSource.clip;
    }
}
