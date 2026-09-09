using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Footsteps Collection", menuName = "Create New Footsteps Collection")]
public class FootstepsCollection : ScriptableObject
{
    [Header("Walking")]
    public List<AudioClip> walkFootstepSounds = new List<AudioClip>();

    [Header("Running")]
    public List<AudioClip> runFootstepSounds = new List<AudioClip>();

    public AudioClip landSound;
}
