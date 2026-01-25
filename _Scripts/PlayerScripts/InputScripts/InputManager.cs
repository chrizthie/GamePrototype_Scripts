using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System;

[RequireComponent(typeof(PlayerLocomotion))]
public class InputManager : MonoBehaviour
{
    bool sprintToggled;        // for controller toggle
    bool wasRunPressedLastFrame;

    [Header("Input Cooldowns")]
    // flashlight
    public bool canFlashlightTurn = true;
    private float flashlightCooldown = 0.2f;
    // crouch
    private float lastCrouchTime = 0f;
    private float crouchCooldown = 0.5f;
    
    public bool PauseOpenCloseInput { get; private set; }

    [Header("Inputs")]
    [SerializeField] public bool isGamepad;
    [HideInInspector] public InputAction runAction;
    [HideInInspector] public InputAction _pauseOpenCloseAction;

    [Header("Required Components")]
    [SerializeField] PlayerLocomotion playerLocomotion;
    [SerializeField] PlayerInput _playerInput;
    [SerializeField] public static InputManager instance;
    [SerializeField] public InputSystem_Actions playerInputs;


    [Header("Required Outside Components")]
    [SerializeField] FlashlightHandler flashlightHandler;

    #region Input Handling

    private void OnMove(InputValue value)
    {
        playerLocomotion.moveInput = value.Get<Vector2>();
    }

    private void OnLook(InputValue value)
    {
        playerLocomotion.lookInput = value.Get<Vector2>();
    }

    private void OnCrouch(InputValue value)
    {
        if (value.isPressed && !playerLocomotion.isRunning)
        {
            // Prevent standing up if crouched and blocked overhead
            if (playerLocomotion.isCrouching && playerLocomotion.obstacleOverhead)
            {
                // Stay crouched, do nothing
                return;
            }

            // Check cooldown before toggling crouch
            if (Time.time - lastCrouchTime >= crouchCooldown)
            {
                playerLocomotion.crouchInput = !playerLocomotion.crouchInput;
                lastCrouchTime = Time.time; // Reset cooldown timer
            }
        }
    }

    private void OnFlashlight(InputValue value)
    {
        if (!canFlashlightTurn) return;

        canFlashlightTurn = false;

        if (value.isPressed)
        {
            playerLocomotion.isFlashlightOn = !playerLocomotion.isFlashlightOn;
            flashlightHandler.flashlightAudioSource.PlayOneShot(flashlightHandler.flashlightTurnSound);
        }

        StartCoroutine(ResetFlashlightCooldown());
    }

    #endregion

    #region Inputs Handled in Update for better control

    private void OnRun()
    {
        bool canSprint = playerLocomotion.canRun && !playerLocomotion.obstacleOverhead && playerLocomotion.moveInput.sqrMagnitude > 0.01f;

        if (!canSprint)
        {
            playerLocomotion.runInput = false;
            if (isGamepad) sprintToggled = false;
            return;
        }

        if (isGamepad)
        {
            bool runPressed = runAction.IsPressed();

            if (runPressed && !wasRunPressedLastFrame)
            {
                sprintToggled = !sprintToggled;
            }

            wasRunPressedLastFrame = runPressed;

            playerLocomotion.runInput = sprintToggled;
        }
        else
        {
            playerLocomotion.runInput = runAction.IsPressed();
        }

        if (!playerLocomotion.runInput) return;

        Debug.Log("Running...");
        playerLocomotion.crouchInput = false;
    }

    #endregion

    #region Input Cooldowns

    private IEnumerator ResetFlashlightCooldown()
    {
        yield return new WaitForSeconds(flashlightCooldown);
        canFlashlightTurn = true;
    }

    #endregion

    #region Unity Methods

    private void OnValidate()
    {
        if (playerLocomotion == null)
        {
            playerLocomotion = GetComponent<PlayerLocomotion>();
        }

        if (_playerInput == null)
        {
            _playerInput = GetComponent<PlayerInput>();
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        UpdateControlScheme();
        _pauseOpenCloseAction = _playerInput.actions["PauseOpenClose"];
    }

    private void OnEnable()
    {
        if (playerInputs == null)
        {
            playerInputs = new InputSystem_Actions();
        }

        _playerInput.onControlsChanged += OnControlsChanged;
        playerInputs.Enable();
        runAction = playerInputs.Player.Run;
        runAction.Enable();
    }

    private void OnDisable()
    {
        _playerInput.onControlsChanged -= OnControlsChanged;
        playerInputs.Disable();
        runAction.Disable();
    }

    private void OnControlsChanged(PlayerInput input)
    {
        UpdateControlScheme();
    }

    private void UpdateControlScheme()
    {
        isGamepad = _playerInput.currentControlScheme == "Gamepad";
        //Debug.Log("Active scheme: " + _playerInput.currentControlScheme);
    }


    #endregion

    private void Update()
    {
        OnRun();

        // when pause button is pressed
        PauseOpenCloseInput = _pauseOpenCloseAction.WasPressedThisFrame();
    }
}
