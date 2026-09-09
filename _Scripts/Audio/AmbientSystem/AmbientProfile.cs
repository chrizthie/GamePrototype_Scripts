using UnityEngine;

[CreateAssetMenu(
    fileName = "AmbientProfile",
    menuName = "Audio/Ambient Profile"
)]
public class AmbientProfile : ScriptableObject
{
    [Header("Ambient Audio")]
    [SerializeField] private AudioClip ambientClip;

    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    [Header("Fade")]
    [SerializeField] private float fadeInSpeed = 1f;
    [SerializeField] private float fadeOutSpeed = 1f;

    public AudioClip AmbientClip => ambientClip;
    public float Volume => volume;
    public float FadeInSpeed => fadeInSpeed;
    public float FadeOutSpeed => fadeOutSpeed;
}