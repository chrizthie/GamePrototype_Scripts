using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using System;

[RequireComponent(typeof(CharacterController))]

public class PlayerLocomotion : MonoBehaviour
{
    [Header("Movement Parameters")]
    public float maxSpeed;
    public float maxAcceleration;
    public bool canMove = true;
    public bool canRun;
    public bool obstacleOverhead = false;

    [Header("Landing Parameters")]
    public bool isMovementPaused = false;
    public float landingPauseTime = 0.5f;

    [Header("Crouching Parameters")]
    public Transform headPoint;
    public LayerMask obstacleMask;
    private float standingHeight = 1.7f;
    private float crouchingHeight = 1.2f;
    private float crouchTransitionSpeed = 15f;
    private float standingCenter = 0.845f;
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

    [Header("Input")]
    public Vector2 moveInput;
    public Vector2 lookInput;
    public bool runInput;
    public bool crouchInput;
    public bool isFlashlightOn = false;

    [Header("Required Components")]
    [SerializeField] PlayerLocomotionPreset preset;
    [SerializeField] CinemachineCamera firstPersonCamera;
    [SerializeField] CharacterController characterController;
    [SerializeField] Animator animator;

    [Header("Required Player Modules")]
    [SerializeField] StaminaSystem staminaSystem;
    [SerializeField] FootstepsHandler footstepsHandler;
    [SerializeField] BlockAheadDetection blockAheadDetection;

    #region Controller Methods

    private void MovementFlags()
    {
        // in place flag
        if (currentSpeed == 0f)
        {
            inPlace = true;
        }
        else
        {
            inPlace = false;
        }

        // is walking flag
        if (!runInput && currentSpeed > 0.1f)
        {
            isWalking = true;
        }
        else
        {
            isWalking = false;
        }

        // is running flag
        if (runInput && moveInput.y > 0f && !inPlace)
        {
            isRunning = true;
        }
        else
        {
            isRunning = false;
        }

        // is walking backwards flag
        if (moveInput.y < 0f)
        {
            isWalkingBackwards = true;
            isWalking = false;
        }
        else
        {
            isWalkingBackwards = false;
        }

        // is grounded flag
        if (characterController.isGrounded)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }

        // can run flag
        if (blockAheadDetection.blocked == false)
        {
            canRun = true;
        }
        else
        {
            canRun = false;
        }

        // is crouching flag
        if (crouchInput)
        {
            isCrouching = true; 
        }
        else
        {
            isCrouching = false;
        }
    }

    private void MoveUpdate()
    {
        // Snap moveInput to -1, 0, or 1 for each axis
        moveInput.x = moveInput.x > 0 ? 1f : (moveInput.x < 0 ? -1f : 0f);
        moveInput.y = moveInput.y > 0 ? 1f : (moveInput.y < 0 ? -1f : 0f);

        Vector3 motion = transform.forward * moveInput.y + transform.right * moveInput.x;
        motion.y = 0f;
        motion.Normalize();

        // determine max speed
        if (runInput && canRun == true && !isWalkingBackwards && staminaSystem.playerStamina != 0)
        {
            maxSpeed = preset.runSpeed;
        }
        else if (crouchInput)
        {
            maxSpeed = preset.crouchSpeed;
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

        if (isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -3f;
        }

        // apply gravity, with ramping when in air
        verticalVelocity = Physics.gravity.y * preset.gravityScale * Time.deltaTime;

        if (!isGrounded)
        {
            airTime += Time.deltaTime;
            preset.gravityScale = Mathf.Min(preset.gravityScale + airTime * preset.rampingGravity, preset.maxGravity);
        }
        else
        {
            preset.gravityScale = 25;
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
            if (animator.GetCurrentAnimatorStateInfo(0).shortNameHash != idleToCrouchHash &&
                animator.GetCurrentAnimatorStateInfo(0).shortNameHash != crouchToIdleHash)
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
        if (headPoint == null)
            headPoint = transform; // fallback

        // Perform spherecast upwards
        if (Physics.SphereCast(headPoint.position, checkRadius, Vector3.up, out RaycastHit hit, checkDistance, obstacleMask))
        {
            obstacleOverhead = true;
        }
        else
        {
            obstacleOverhead = false;
        }

        // Debug visualization
        Color color = obstacleOverhead ? Color.red : Color.green;
        Debug.DrawRay(headPoint.position, Vector3.up * checkDistance, color);
    }

    private void StandUp()
    {
        if(!isCrouching)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).shortNameHash != idleToCrouchHash &&
                animator.GetCurrentAnimatorStateInfo(0).shortNameHash != crouchToIdleHash)
            {
                // center transition
                Vector3 center = characterController.center;
                center.y = standingCenter;
                characterController.center = center;
                // height transition
                characterController.height = Mathf.Lerp(characterController.height, standingHeight, crouchTransitionSpeed * Time.deltaTime);
                maxAcceleration = preset.normalAcceleration;
            }
        }
    }

    private void PlayerLanding()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // Check if we are in the landing animation
        if (stateInfo.IsName("NormalLanding") || stateInfo.IsName("LandHarder"))
        {
            canMove = false; // Disable movement
            currentVelocity = Vector3.zero; // Stop movement immediately
        }
        else
        {
            canMove = true; // Enable movement
        }
    }

    public void LookUpdate()
    {
        if (!lookEnabled)
        {
            return;
        }

        Vector2 input = new Vector2(lookInput.x * preset.lookSensitivity.x, lookInput.y * preset.lookSensitivity.y);

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
        GamePause.OnPauseChanged += (isPaused) => lookEnabled = !isPaused;
    }

    private void OnDisable()
    {
        // Unsubscribe from pause event
        GamePause.OnPauseChanged -= (isPaused) => lookEnabled = !isPaused;
    }

    private void Start()
    {
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
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0); // 0 = base layer

        if (canMove)
        {
            MoveUpdate();
        }
        Crouch();
        CheckOverhead();
        StandUp();

        MovementFlags();
        PlayerLanding();
        LookUpdate();
        CameraUpdate();
    }

    #endregion

}
