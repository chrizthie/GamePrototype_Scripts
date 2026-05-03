using UnityEngine;
using System.Collections.Generic;

public class FootstepsHandler : MonoBehaviour
{
    [Header("Animation Event Protection")]
    [SerializeField] private float minTimeBetweenFootsteps = 0.12f;

    private float lastFootstepTime = -999f;

    [Header("Footsteps Collections")]
    [SerializeField] private List<AudioClip> footstepSounds = new();
    [SerializeField] private AudioClip landingClip;

    [Header("Sync Tuning")]
    [SerializeField] private float postLandingStepDelay = 0.10f;

    private float footstepLockTimer;

    [Header("Required Components")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private PlayerLocomotion playerLocomotion;
    [SerializeField] private PlayerLocomotionPreset playerLocomotionPreset;
    [SerializeField] private PlayerVoice playerVoice;
    [SerializeField] private AudioSource footstepAudioSource;
    [SerializeField] private FootstepsSwapper footstepsSwapper;

    private bool wasGrounded;

    private void Awake()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        if (playerLocomotion == null)
            playerLocomotion = GetComponent<PlayerLocomotion>();

        if (playerVoice == null)
            playerVoice = GetComponent<PlayerVoice>();

        if (footstepsSwapper == null)
            footstepsSwapper = GetComponent<FootstepsSwapper>();

        if (footstepAudioSource == null)
            footstepAudioSource = GetComponent<AudioSource>();

        if (characterController == null || playerLocomotion == null || playerLocomotionPreset == null || footstepAudioSource == null)
        {
            Debug.LogError("FootstepsHandler is missing required references.", this);
            enabled = false;
        }
    }

    private void OnValidate()
    {
        if (footstepsSwapper == null)
            footstepsSwapper = GetComponent<FootstepsSwapper>();

        if (playerVoice == null)
            playerVoice = GetComponent<PlayerVoice>();

        if (footstepAudioSource == null)
            footstepAudioSource = GetComponent<AudioSource>();

        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        if (playerLocomotion == null)
            playerLocomotion = GetComponent<PlayerLocomotion>();
    }

    private void Update()
    {
        UpdateFootstepSettings();
        HandleLanding();

        if (footstepLockTimer > 0f)
            footstepLockTimer -= Time.deltaTime;
    }

    private void UpdateFootstepSettings()
    {
        if (playerLocomotion.isCrouching)
        {
            footstepAudioSource.pitch = 1.2f;
            footstepAudioSource.volume = 0.6f;
        }
        else
        {
            footstepAudioSource.pitch = 1.1f;
            footstepAudioSource.volume = 0.9f;
        }
    }

    private void HandleLanding()
    {
        bool isGrounded = characterController.isGrounded;
        bool justLanded = !wasGrounded && isGrounded;

        if (justLanded && playerLocomotion.airTime > 0.4f)
        {
            PlayLandingAudio();
            footstepLockTimer = postLandingStepDelay;
        }

        wasGrounded = isGrounded;
    }

    public void PlayFootStepAudio()
    {
        if (Time.time - lastFootstepTime < minTimeBetweenFootsteps)
            return;

        if (!playerLocomotion.canMove)
            return;

        if (!characterController.isGrounded)
            return;

        if (footstepLockTimer > 0f)
            return;

        // Use input, not actual velocity.
        // This allows footsteps to play when pushing against a wall.
        if (playerLocomotion.moveInput.sqrMagnitude <= 0.01f)
            return;

        lastFootstepTime = Time.time;

        if (footstepsSwapper != null)
        {
            footstepsSwapper.CheckLayers();
            footstepsSwapper.UpdateFootstepType(playerLocomotion.isRunning);
        }

        if (footstepSounds == null || footstepSounds.Count == 0)
            return;

        if (footstepSounds.Count == 1)
        {
            footstepAudioSource.PlayOneShot(footstepSounds[0]);
            return;
        }

        int n = Random.Range(1, footstepSounds.Count);
        AudioClip chosenClip = footstepSounds[n];

        footstepAudioSource.PlayOneShot(chosenClip);

        footstepSounds[n] = footstepSounds[0];
        footstepSounds[0] = chosenClip;
    }

    private void PlayLandingAudio()
    {
        if (footstepsSwapper != null)
            footstepsSwapper.CheckLayers();

        if (landingClip != null)
            footstepAudioSource.PlayOneShot(landingClip);

        if (playerVoice != null)
            playerVoice.PlayLandingGruntAudio();
    }

    public void SwapFootsteps(FootstepsCollection collection)
    {
        if (collection == null)
            return;

        footstepSounds.Clear();

        List<AudioClip> sourceList = playerLocomotion.isRunning
            ? collection.runFootstepSounds
            : collection.walkFootstepSounds;

        if (sourceList != null && sourceList.Count > 0)
            footstepSounds.AddRange(sourceList);

        landingClip = collection.landSound;
    }
}