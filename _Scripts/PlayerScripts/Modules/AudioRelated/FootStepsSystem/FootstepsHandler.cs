using UnityEngine;
using System.Collections.Generic;

public class FootstepsHandler : MonoBehaviour
{
    [Header("Footsteps Collections")]
    [SerializeField] private List<AudioClip> footstepSounds = new();
    [SerializeField] private AudioClip landingClip;

    [Header("Footsteps Parameters")]
    [SerializeField, Range(0f, 1f)] private float runStepLengthen = 0.7f;
    [SerializeField] private float stepInterval = 1.72f;
    [SerializeField] private float crouchStepSpeed = 1.75f;
    [SerializeField] private float walkStepSpeed = 1.72f;
    [SerializeField] private float runStepSpeed = 1.82f;

    private float stepCycle;
    private float nextStep;

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

        if (characterController == null ||
            playerLocomotion == null ||
            playerLocomotionPreset == null ||
            footstepAudioSource == null)
        {
            Debug.LogError("FootstepsHandler is missing required references.", this);
            enabled = false;
            return;
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

    private void Start()
    {
        stepCycle = 0f;
        nextStep = stepCycle / 2f;
        wasGrounded = characterController != null && characterController.isGrounded;
    }

    private void Update()
    {
        if (!enabled) return;

        UpdateFootstepSettings();
        HandleLanding();
    }

    private void FixedUpdate()
    {
        if (!enabled) return;

        float moveSpeed = playerLocomotion.isRunning
            ? playerLocomotionPreset.runSpeed
            : playerLocomotionPreset.walkSpeed;

        ProgressStepCycle(moveSpeed);
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

        if (playerLocomotion.isRunning && !playerLocomotion.isWalkingBackwards)
            stepInterval = runStepSpeed;
        else if (playerLocomotion.isCrouching)
            stepInterval = crouchStepSpeed;
        else
            stepInterval = walkStepSpeed;
    }

    private void HandleLanding()
    {
        bool isGrounded = characterController.isGrounded;
        bool justLanded = !wasGrounded && isGrounded;

        if (justLanded && playerLocomotion.airTime > 0.4f)
        {
            PlayLandingAudio();
        }

        wasGrounded = isGrounded;
    }

    private void ProgressStepCycle(float speed)
    {
        if (!playerLocomotion.canMove)
            return;

        if (!characterController.isGrounded)
            return;

        if (characterController.velocity.sqrMagnitude <= 0.01f)
            return;

        if (playerLocomotion.moveInput.sqrMagnitude <= 0.01f)
            return;

        float speedMultiplier = (playerLocomotion.isWalking || playerLocomotion.isWalkingBackwards)
            ? 1f
            : runStepLengthen;

        stepCycle += (characterController.velocity.magnitude + (speed * speedMultiplier)) * Time.fixedDeltaTime;

        if (stepCycle <= nextStep)
            return;

        nextStep = stepCycle + stepInterval;
        PlayFootStepAudio();
    }

    private void PlayFootStepAudio()
    {
        if (!playerLocomotion.canMove)
            return;

        if (!characterController.isGrounded)
            return;

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
        AudioClip clip = footstepSounds[n];

        footstepAudioSource.PlayOneShot(clip);

        // Move picked sound to index 0 so it won't repeat immediately
        footstepSounds[n] = footstepSounds[0];
        footstepSounds[0] = clip;
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