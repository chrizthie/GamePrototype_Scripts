using JetBrains.Annotations;
using System;
using System.Collections;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

[RequireComponent(typeof(CharacterController))]

public class PlayerLocomotion : MonoBehaviour
{
    [Header("Movement Parameters")]
    public float maxSpeed;
    public float maxAcceleration;
    public bool canMove = true;
    public bool canRun;
    public bool obstacleOverhead = false;
    private float overheadCheckTimer;
    private const float overheadCheckInterval = 0.05f;
    private AnimatorStateInfo currentAnimState;

    [Header("Landing Parameters")]
    public bool isMovementPaused = false;
    public float landingPauseTime = 0.5f;

    [Header("Crouching Parameters")]
    public Transform headPoint;
    public LayerMask obstacleMask;
    private float standingHeight = 1.72f;
    private float crouchingHeight = 1.2f;
    private float crouchTransitionSpeed = 15f;
    private float standingCenter = 0.86f;
    private float crouchingCenter = 0.595f;
    private int idleToCrouchHash;
    private int crouchToIdleHash;
    private float checkRadius = 0.25f; // width of the check sphere
    private float checkDistance = 0.4f; // how far above head to check

    [Header("Movement Flags")]
    [SerializeField] private bool lookEnabled = true;
    public bool inPlace;
    public bool isGrounded;
    public bool isWalking;
    public bool isWalkingBackwards;
    public bool isRunning;
    public bool isCrouching;
    private bool landingLock;

    [Header("Looking Parameters")]
    public float maxTiltAngle;
    private float currentPitch = 0f;
    private float currentTilt = 0f;

    public float CurrentPitch
    {
        get => currentPitch;

        set
        {
            currentPitch = Mathf.Clamp(value, -preset.pitchUpLimit, preset.pitchDownLimit);
        }
    }

    [Header("Physics Parameters")]
    public float airTime;
    public float verticalVelocity = 0f;
    public Vector3 currentVelocity { get; private set; }
    public float currentSpeed { get; private set; }
    private float gravityScale;

    [Header("Input")]
    public Vector2 moveInput;
    public Vector2 lookInput;
    public bool runInput;
    public bool crouchInput;
    public bool isFlashlightOn = false;
    private Action<bool> pauseHandler;

    [Header("Required Components")]
    [SerializeField] PlayerLocomotionPreset preset;
    [SerializeField] InputManager inputManager;
    [SerializeField] CinemachineCamera firstPersonCamera;
    [SerializeField] CharacterController characterController;
    [SerializeField] Animator animator;

    [Header("Required Player Modules")]
    [SerializeField] StaminaSystem staminaSystem;
    [SerializeField] FootstepsHandler footstepsHandler;
    [SerializeField] BlockAheadDetection blockAheadDetection;

    public void ApplyInput(PlayerInputState input)
    {
        moveInput = input.move;
        lookInput = input.look;
        runInput = input.run;
        crouchInput = input.crouch;
    }

    #region Controller Methods

    private void MovementFlags()
    {
        // grounded
        isGrounded = characterController.isGrounded;

        // movement state
        inPlace = currentSpeed <= 0.01f;

        isWalkingBackwards = moveInput.y < 0f;

        isWalking = !runInput && !inPlace && moveInput.y > 0f;

        isRunning = runInput && moveInput.y > 0f && !inPlace;

        // crouch
        bool wantsToCrouch = crouchInput;

        if (wantsToCrouch)
        {
            isCrouching = true;
        }
        else if (!obstacleOverhead)
        {
            isCrouching = false;
        }

        // run availability
        canRun = !blockAheadDetection.blocked;
    }

    private void MoveUpdate()
    {
        // Gamepad = analog
        if (inputManager.isGamepad)
        {
            // deadzone
            if (moveInput.magnitude < 0.15f) moveInput = Vector2.zero;

            moveInput = Vector2.ClampMagnitude(moveInput, 1f);
        }
        // Keyboard = digital
        else
        {
            moveInput.x = moveInput.x > 0 ? 1f : (moveInput.x < 0 ? -1f : 0f);
            moveInput.y = moveInput.y > 0 ? 1f : (moveInput.y < 0 ? -1f : 0f);
        }

        Vector3 motion = transform.forward * moveInput.y + transform.right * moveInput.x;
        motion.y = 0f;
        motion.Normalize();

        // determine max speed
        if (isCrouching)
        {
            maxSpeed = preset.crouchSpeed;
        }
        else if (runInput && canRun && !isWalkingBackwards && staminaSystem.playerStamina > 0f)
        {
            maxSpeed = preset.runSpeed;
        }
        else
        {
            maxSpeed = preset.walkSpeed;
        }

        // accelerate to target speed
        if (motion.sqrMagnitude >= 0.01f)
        {
            currentVelocity = Vector3.MoveTowards(currentVelocity, motion * maxSpeed, maxAcceleration * Time.deltaTime);
        }
        else
        {
            currentVelocity = Vector3.MoveTowards(currentVelocity, Vector3.zero, maxAcceleration * Time.deltaTime);
        }

        // grounding safety
        if (isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -5f; // small stick force
        }

        if (!isGrounded && airTime < 0.05f)
        {
            verticalVelocity = 0f; // prevent micro-snaps
        }

        // gravity application
        if (!isGrounded)
        {
            airTime += Time.deltaTime;

            gravityScale = Mathf.Min(gravityScale + preset.rampingGravity * Time.deltaTime, preset.maxGravity);
            verticalVelocity += Physics.gravity.y * gravityScale * Time.deltaTime;
        }
        else
        {
            gravityScale = preset.gravityScale;
            airTime = 0f;
        }

        // move the character
        Vector3 fullVelocity = new Vector3(currentVelocity.x, verticalVelocity, currentVelocity.z);

        characterController.Move(fullVelocity * Time.deltaTime);

        // updating speed
        currentSpeed = currentVelocity.magnitude;
    }

    private void Crouch()
    {
        if (isCrouching)
        {
            if (currentAnimState.shortNameHash != idleToCrouchHash && currentAnimState.shortNameHash != crouchToIdleHash)
            {
                // center transition
                Vector3 center = characterController.center;
                center.y = crouchingCenter;
                characterController.center = center;
                // height transition
                characterController.height = Mathf.Lerp(characterController.height, crouchingHeight, crouchTransitionSpeed * Time.deltaTime);
                maxAcceleration = preset.crouchAcceleration;
            }
        }
    }

    private void CheckOverhead()
    {
        overheadCheckTimer -= Time.deltaTime;
        if (overheadCheckTimer > 0f) return;
        overheadCheckTimer = overheadCheckInterval;

        obstacleOverhead = Physics.SphereCast(headPoint.position, checkRadius, Vector3.up, out _, checkDistance, obstacleMask);
    }

    private void StandUp()
    {
        if (!isCrouching && !obstacleOverhead)
        {
            if (currentAnimState.shortNameHash != idleToCrouchHash && currentAnimState.shortNameHash != crouchToIdleHash)
            {
                Vector3 center = characterController.center;
                center.y = standingCenter;
                characterController.center = center;

                characterController.height = Mathf.Lerp(characterController.height, standingHeight, crouchTransitionSpeed * Time.deltaTime);

                if (isRunning)
                {
                    maxAcceleration = preset.runningAcceleration;
                }
                else
                {
                    maxAcceleration = preset.normalAcceleration;
                }
            }
        }
    }

    private void PlayerLanding()
    {
        AnimatorStateInfo stateInfo = currentAnimState;

        landingLock = stateInfo.IsName("NormalLanding") || stateInfo.IsName("LandHarder");

        if (landingLock)
        {
            currentVelocity = Vector3.zero;
        }
    }

    public void LookUpdate()
    {
        float lookSensitivity;

        if (inputManager.isGamepad)
        {
            lookSensitivity = Mathf.Lerp(preset.controllerMin, preset.controllerMax, preset.controllerSensitivity);
        } else
        {
            lookSensitivity = Mathf.Lerp(preset.mouseMin, preset.mouseMax, preset.mouseSensitivity);
        }

        if (!lookEnabled)
        {
            return;
        }
        
        Vector2 input = new Vector2(lookInput.x * lookSensitivity, lookInput.y * lookSensitivity);

        // looking up and down
        CurrentPitch -= input.y;

        firstPersonCamera.transform.localRotation = Quaternion.Euler(CurrentPitch, 0f, currentTilt);

        // looking left and right
        transform.Rotate(Vector3.up * input.x);
    }

    private void CameraUpdate()
    {   
        float targetFOV = preset.cameraWalkFOV;

        if (isRunning)
        {   
            float speedRatio = currentSpeed / preset.runSpeed;

            targetFOV = Mathf.Lerp(preset.cameraWalkFOV, preset.cameraRunFOV, speedRatio);
        }

        firstPersonCamera.Lens.FieldOfView = Mathf.Lerp(firstPersonCamera.Lens.FieldOfView, targetFOV, preset.cameraFOVSmoothing * Time.deltaTime);

        // Camera tilt when strafing
        if (isRunning)
        {
            maxTiltAngle = preset.runTiltAngle;
        }
        else
        {
            maxTiltAngle = preset.walkTiltAngle;
        }

        float targetTilt = -moveInput.x * maxTiltAngle;
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, preset.tiltSmoothing * Time.deltaTime);
    }

    private void OnPauseChanged(bool paused)
    {
        lookEnabled = !paused;
    }

    #endregion

    #region Unity Methods

    private void OnEnable()
    {
        // Subscribe to pause event
        pauseHandler = (isPaused) => lookEnabled = !isPaused;
        GamePause.OnPauseChanged += pauseHandler;
    }

    private void OnDisable() 
    {
        // Unsubscribe from pause event
        GamePause.OnPauseChanged -= pauseHandler;
    }

    private void Start()
    {
        gravityScale = preset.gravityScale;
        idleToCrouchHash = Animator.StringToHash("Idle2Crouch");
        crouchToIdleHash = Animator.StringToHash("Crouch2Idle");
    }

    private void OnValidate()
    {
        //character controller
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }
        
        // animator
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void Update()
    {
        ApplyInput(inputManager.CurrentInput);

        currentAnimState = animator.GetCurrentAnimatorStateInfo(0);
        
        MovementFlags();
        CheckOverhead();
        PlayerLanding();

        if (canMove && !landingLock)
        {
            MoveUpdate();
        }

        Crouch();
        StandUp();

        if (lookEnabled)
        {
            LookUpdate();
        }

        CameraUpdate();
    }

    #endregion

}
