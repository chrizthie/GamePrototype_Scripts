using UnityEngine;
using System.Collections.Generic;

public class FootstepsHandler : MonoBehaviour
{
    [Header("Footsteps Collections")]
    [SerializeField] private List<AudioClip> footstepSounds = new();
    [SerializeField] private AudioClip landingClip;

    [Header("Footsteps Parameters")]
    [SerializeField, Range(0f, 1f)] private float runStepLengthen = 0.7f;
    [SerializeField] private float stepInterval;
    [SerializeField] private float crouchStepSpeed = 1.75f;
    [SerializeField] private float walkStepSpeed = 1.72f;
    [SerializeField] private float runStepSpeed = 1.82f;

    [Header("Sync Tuning")]
    [SerializeField] private float minHorizontalSpeedForSteps = 0.15f;
    [SerializeField] private float postLandingStepDelay = 0.10f;
    [SerializeField] private float stateChangeStepOffset = 0.45f;

    private float stepCycle;
    private float nextStep;
    private float footstepLockTimer;

    [Header("Required Components")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private PlayerLocomotion playerLocomotion;
    [SerializeField] private PlayerLocomotionPreset playerLocomotionPreset;
    [SerializeField] private PlayerVoice playerVoice;
    [SerializeField] private AudioSource footstepAudioSource;
    [SerializeField] private FootstepsSwapper footstepsSwapper;

    private bool wasGrounded;

    private enum MoveState
    {
        Idle,
        Crouch,
        Walk,
        Run
    }

    private MoveState currentMoveState;
    private MoveState previousMoveState;

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

    private void Start()
    {
        stepCycle = 0f;
        nextStep = 0f;
        wasGrounded = characterController.isGrounded;
        currentMoveState = MoveState.Idle;
        previousMoveState = MoveState.Idle;
    }

    private void Update()
    {
        UpdateFootstepSettings();
        UpdateMoveState();
        HandleLanding();

        if (footstepLockTimer > 0f)
            footstepLockTimer -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        float speed = playerLocomotion.isRunning
            ? playerLocomotionPreset.runSpeed
            : playerLocomotionPreset.walkSpeed;

        ProgressStepCycle(speed);
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

    private void UpdateMoveState()
    {
        previousMoveState = currentMoveState;

        bool hasMoveInput = playerLocomotion.moveInput.sqrMagnitude > 0.01f;
        bool grounded = characterController.isGrounded;

        Vector3 horizontalVelocity = characterController.velocity;
        horizontalVelocity.y = 0f;
        float horizontalSpeed = horizontalVelocity.magnitude;

        if (!grounded || !hasMoveInput || horizontalSpeed < minHorizontalSpeedForSteps)
        {
            currentMoveState = MoveState.Idle;
        }
        else if (playerLocomotion.isCrouching)
        {
            currentMoveState = MoveState.Crouch;
        }
        else if (playerLocomotion.isRunning && !playerLocomotion.isWalkingBackwards)
        {
            currentMoveState = MoveState.Run;
        }
        else
        {
            currentMoveState = MoveState.Walk;
        }

        if (currentMoveState != previousMoveState)
        {
            // Re-align cadence when state changes so the next step feels intentional
            nextStep = stepCycle + (stepInterval * stateChangeStepOffset);
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

            // Slightly push the next step back so landing and footstep do not stack awkwardly
            nextStep = stepCycle + (stepInterval * 0.5f);
        }

        wasGrounded = isGrounded;
    }

    private void ProgressStepCycle(float speed)
    {
        if (!playerLocomotion.canMove)
            return;

        if (!characterController.isGrounded)
            return;

        if (footstepLockTimer > 0f)
            return;

        if (playerLocomotion.moveInput.sqrMagnitude <= 0.01f)
            return;

        Vector3 horizontalVelocity = characterController.velocity;
        horizontalVelocity.y = 0f;
        float horizontalSpeed = horizontalVelocity.magnitude;

        if (horizontalSpeed < minHorizontalSpeedForSteps)
            return;

        float movementMultiplier = (playerLocomotion.isWalking || playerLocomotion.isWalkingBackwards)
            ? 1f
            : runStepLengthen;

        stepCycle += (horizontalSpeed + (speed * movementMultiplier)) * Time.fixedDeltaTime;

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
        AudioClip chosenClip = footstepSounds[n];
        footstepAudioSource.PlayOneShot(chosenClip);

        // Move chosen clip to slot 0 so it will not repeat immediately
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