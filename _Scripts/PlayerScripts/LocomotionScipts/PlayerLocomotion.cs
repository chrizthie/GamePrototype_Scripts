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
    [Header("Animator Movement")]
    public Vector2 movementBlendVector { get; private set; }

    #region Movement Parameters
    [Header("Movement Parameters")]
    public float maxSpeed;
    public float maxAcceleration;
    public bool canMove = true;
    public bool canRun;
    public bool obstacleOverhead = false;
    public bool justStartedRunning { get; private set; }
    private bool wasRunning;
    private float overheadCheckTimer;
    private const float overheadCheckInterval = 0.05f;
    private AnimatorStateInfo currentAnimState;
    #endregion

    #region Movement Feel
    [Header("Movement Feel")]
    [SerializeField] private float walkAcceleration = 9f;
    [SerializeField] private float walkDeceleration = 12f;

    [SerializeField] private float runAcceleration = 7f;
    [SerializeField] private float runDeceleration = 10f;

    [SerializeField] private float crouchAcceleration = 6f;
    [SerializeField] private float crouchDeceleration = 14f;

    [SerializeField] private float airAccelerationMultiplier = 0.35f;
    [SerializeField] private float backwardSpeedMultiplier = 0.72f;
    [SerializeField] private float strafeSpeedMultiplier = 0.85f;

    [SerializeField] private float sprintForwardRequirement = 0.55f;
    [SerializeField] private float sprintSideLimit = 0.85f;

    [SerializeField] private float crouchMomentumDamping = 0.8f;
    [SerializeField] private float sprintExitDragDuration = 0.18f;
    [SerializeField] private float sprintExitDragMultiplier = 1.35f;

    private float sprintExitDragTimer;

    #endregion

    #region Landing Parameters
    [Header("Landing Parameters")]
    public bool isMovementPaused = false;
    public float landingPauseTime = 0.5f;
    #endregion

    #region Crouching Parameters
    [Header("Crouching Parameters")]
    public Transform headPoint;
    public LayerMask obstacleMask;
    private bool wasCrouching;
    private bool justStartedCrouching;
    private float standingHeight = 1.72f;
    private float crouchingHeight = 1.2f;
    private float crouchTransitionSpeed = 15f;
    private int idleToCrouchHash;
    private int crouchToIdleHash;
    private float checkRadius = 0.25f; // width of the check sphere
    private float checkDistance = 0.4f; // how far above head to check
    #endregion

    #region Movement Flags
    [Header("Movement Flags")]
    [SerializeField] private bool lookEnabled = true;
    public bool inPlace;
    public bool isGrounded;
    public bool isWalking;
    public bool isWalkingBackwards;
    public bool isRunning;
    public bool isCrouching;
    private bool landingLock;
    #endregion

    #region Looking Parameters
    [Header("Looking Parameters")]
    private float maxTiltAngle;
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
    #endregion

    #region Physics Parameters
    [Header("Physics Parameters")]
    public float movementSpeed;
    public float verticalVelocity = 0f;
    public float airTime;
    private float gravityScale;
    public Vector3 currentVelocity { get; private set; }
    public float currentSpeed { get; private set; }
    #endregion

    #region Input Parameters
    [Header("Input")]
    [HideInInspector] public Vector2 moveInput;
    private Vector2 lookInput;
    private bool runInput;
    private bool crouchInput;
    private Action<bool> pauseHandler;
    #endregion

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

    private void UpdateMovementBlendVector()
    {
        float x = 0f;
        float y = 0f;

        float snapValue = isRunning ? 2f : 1f;

        if (moveInput.x > 0.1f)
            x = snapValue;
        else if (moveInput.x < -0.1f)
            x = -snapValue;

        if (moveInput.y > 0.1f)
            y = snapValue;
        else if (moveInput.y < -0.1f)
            y = -snapValue;

        movementBlendVector = new Vector2(x, y);
    }

    private void MovementFlags()
    {
        isGrounded = characterController.isGrounded;

        Vector3 horizontalVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);
        bool hasActualMovement = horizontalVelocity.magnitude > 0.05f;

        inPlace = !hasActualMovement;
        isWalkingBackwards = moveInput.y < -0.1f && hasActualMovement;

        // run availability
        canRun = !blockAheadDetection.blocked;

        // stricter sprint rule:
        // must be clearly forward, not mostly strafing, not crouching, not blocked
        bool sprintDirectionValid =
            moveInput.y > sprintForwardRequirement &&
            Mathf.Abs(moveInput.x) < sprintSideLimit;

        // crouch from toggled input
        bool wantsToCrouch = crouchInput;

        if (wantsToCrouch)
        {
            isCrouching = true;
        }
        else if (!obstacleOverhead)
        {
            isCrouching = false;
        }

        // if player sprints while crouched, force stand and clear crouch toggle
        bool wantsToSprintFromCrouch =
            runInput &&
            sprintDirectionValid &&
            hasActualMovement &&
            isCrouching &&
            canRun &&
            staminaSystem.playerStamina > 0f &&
            !obstacleOverhead;

        if (wantsToSprintFromCrouch)
        {
            isCrouching = false;
            crouchInput = false;
            inputManager.ForceStandFromSprint();
        }

        bool canSprintNow =
            runInput &&
            sprintDirectionValid &&
            hasActualMovement &&
            !isCrouching &&
            canRun &&
            staminaSystem.playerStamina > 0f;

        isRunning = canSprintNow;
        isWalking = hasActualMovement && !isRunning && !isCrouching;

        // detect run start / exit
        justStartedRunning = !wasRunning && isRunning;

        if (wasRunning && !isRunning)
        {
            sprintExitDragTimer = sprintExitDragDuration;
        }

        wasRunning = isRunning;

        // detect crouch start
        justStartedCrouching = !wasCrouching && isCrouching;
        wasCrouching = isCrouching;
    }

    private void MoveUpdate()
    {
        // INPUT SHAPING
        if (inputManager.isGamepad)
        {
            if (moveInput.magnitude < 0.15f)
                moveInput = Vector2.zero;

            moveInput = Vector2.ClampMagnitude(moveInput, 1f);
        }
        else
        {
            moveInput.x = moveInput.x > 0 ? 1f : (moveInput.x < 0 ? -1f : 0f);
            moveInput.y = moveInput.y > 0 ? 1f : (moveInput.y < 0 ? -1f : 0f);
        }

        // DESIRED MOVE DIRECTION
        Vector3 rawMove = transform.forward * moveInput.y + transform.right * moveInput.x;
        rawMove.y = 0f;

        Vector3 motion = rawMove.sqrMagnitude > 0.001f ? rawMove.normalized : Vector3.zero;

        // BASE SPEED BY STATE
        float targetMaxSpeed;

        if (isCrouching)
        {
            targetMaxSpeed = preset.crouchSpeed;
        }
        else if (isRunning)
        {
            targetMaxSpeed = preset.runSpeed;
        }
        else
        {
            targetMaxSpeed = preset.walkSpeed;
        }

        // DIRECTIONAL PENALTIES
        // makes backwards and strafing feel less dominant / less shooter-like
        float directionalMultiplier = 1f;

        if (moveInput.y < -0.1f)
        {
            directionalMultiplier *= backwardSpeedMultiplier;
        }

        if (Mathf.Abs(moveInput.x) > 0.1f && moveInput.y <= 0.1f)
        {
            directionalMultiplier *= strafeSpeedMultiplier;
        }

        targetMaxSpeed *= directionalMultiplier;

        Vector3 targetVelocity = motion * targetMaxSpeed;

        // ACCEL / DECEL PER STATE
        float acceleration;
        float deceleration;

        if (isCrouching)
        {
            acceleration = crouchAcceleration;
            deceleration = crouchDeceleration;
        }
        else if (isRunning)
        {
            acceleration = runAcceleration;
            deceleration = runDeceleration;
        }
        else
        {
            acceleration = walkAcceleration;
            deceleration = walkDeceleration;
        }

        // weaker control in air
        if (!isGrounded)
        {
            acceleration *= airAccelerationMultiplier;
            deceleration *= airAccelerationMultiplier;
        }

        // crouch start: damp momentum instead of chopping it too harshly
        if (justStartedCrouching)
        {
            currentVelocity *= crouchMomentumDamping;
        }

        // sprint exit drag: adds a bit of commitment when leaving sprint
        if (sprintExitDragTimer > 0f)
        {
            sprintExitDragTimer -= Time.deltaTime;
            deceleration *= sprintExitDragMultiplier;
        }

        // APPLY MOVEMENT
        if (motion.sqrMagnitude > 0.001f)
        {
            currentVelocity = Vector3.MoveTowards(
                currentVelocity,
                targetVelocity,
                acceleration * Time.deltaTime
            );
        }
        else
        {
            currentVelocity = Vector3.MoveTowards(
                currentVelocity,
                Vector3.zero,
                deceleration * Time.deltaTime
            );
        }

        // GROUNDING
        if (isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -5f;
        }

        if (!isGrounded && airTime < 0.05f)
        {
            verticalVelocity = 0f;
        }

        // GRAVITY
        if (!isGrounded)
        {
            airTime += Time.deltaTime;
            gravityScale = Mathf.Min(
                gravityScale + preset.rampingGravity * Time.deltaTime,
                preset.maxGravity
            );

            verticalVelocity += Physics.gravity.y * gravityScale * Time.deltaTime;
        }
        else
        {
            gravityScale = preset.gravityScale;
            airTime = 0f;
        }

        // FINAL MOVE
        Vector3 fullVelocity = new Vector3(currentVelocity.x, verticalVelocity, currentVelocity.z);
        characterController.Move(fullVelocity * Time.deltaTime);

        currentSpeed = new Vector3(currentVelocity.x, 0f, currentVelocity.z).magnitude;
    }

    private void SetControllerHeightSmooth(float targetHeight)
    {
        float newHeight = Mathf.MoveTowards(
            characterController.height,
            targetHeight,
            crouchTransitionSpeed * Time.deltaTime
        );

        characterController.height = newHeight;

        Vector3 center = characterController.center;
        center.y = newHeight * 0.5f;
        characterController.center = center;
    }

    private void Crouch()
    {
        if (!isCrouching) return;

        if (currentAnimState.shortNameHash == idleToCrouchHash || currentAnimState.shortNameHash == crouchToIdleHash)
            return;

        SetControllerHeightSmooth(crouchingHeight);
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
        if (isCrouching || obstacleOverhead) return;

        if (currentAnimState.shortNameHash == idleToCrouchHash || currentAnimState.shortNameHash == crouchToIdleHash)
            return;

        SetControllerHeightSmooth(standingHeight);
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

        float speedRatio = Mathf.Clamp01(currentSpeed / preset.runSpeed);

        if (isRunning)
        {
        targetFOV = Mathf.Lerp(preset.cameraWalkFOV, preset.cameraRunFOV, speedRatio);
        }
        else
        {
        // slight carryover makes sprint exit feel less abrupt
        targetFOV = Mathf.Lerp(preset.cameraWalkFOV, preset.cameraRunFOV, speedRatio * 0.35f);
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
        movementSpeed = currentSpeed;
        ApplyInput(inputManager.CurrentInput);

        currentAnimState = animator.GetCurrentAnimatorStateInfo(0);
        
        MovementFlags();
        UpdateMovementBlendVector();
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
