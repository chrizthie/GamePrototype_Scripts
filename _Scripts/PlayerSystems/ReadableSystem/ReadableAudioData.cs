using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ReadableAudioData
{
    public ReadableAudioState state;

    [Header("Audio Clips")]
    public List<AudioClip> clips =
        new List<AudioClip>();

    [Header("Playback")]
    [Range(0f, 1f)]
    public float volume = 1f;

    public bool loop;

    [Header("Fade")]
    public float fadeInSpeed = 2f;
    public float fadeOutSpeed = 2f;
}