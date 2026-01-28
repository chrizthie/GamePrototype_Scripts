using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System;

[RequireComponent(typeof(PlayerLocomotion))]
public class InputManager : MonoBehaviour
{
    private PlayerInputState currentInput;
    public PlayerInputState CurrentInput => currentInput;

    private bool sprintToggled;        // for controller toggle
    private bool wasRunPressedLastFrame; // to detect button press edge

    [Header("Input Cooldowns")]
    // flashlight
    private float nextFlashlightTime;
    public bool canFlashlightTurn = true;
    private float flashlightCooldown = 0.2f;
    // crouch
    private float lastCrouchTime = 0f;
    private float crouchCooldown = 0.5f;

    [Header("Inputs")]
    public bool PauseOpenCloseInput { get; private set; }
    [SerializeField] public bool isGamepad;
    [SerializeField] public bool isFlashlightOn = false;
    [HideInInspector] public InputAction runAction;
    [HideInInspector] public InputAction _pauseOpenCloseAction;

    [Header("Required Components")]
    [SerializeField] PlayerInput _playerInput;
    [SerializeField] public static InputManager instance;
    [SerializeField] public InputSystem_Actions playerInputs;


    [Header("Required Outside Components")]
    [SerializeField] FlashlightHandler flashlightHandler;

    #region Input Handling

    private void OnMove(InputValue value)
    {
        currentInput.move = value.Get<Vector2>();
    }

    private void OnLook(InputValue value)
    {
        currentInput.look = value.Get<Vector2>();
    }

    private void OnCrouch(InputValue value)
    {
        if (!value.isPressed) return;
        if (Time.time - lastCrouchTime < crouchCooldown) return;

        currentInput.crouch = !CurrentInput.crouch;
        lastCrouchTime = Time.time;
    }

    private void OnFlashlight(InputValue value)
    {
        if (!value.isPressed) return;
        if (Time.time < nextFlashlightTime) return;

        isFlashlightOn = !isFlashlightOn; flashlightHandler.flashlightAudioSource.PlayOneShot(flashlightHandler.flashlightTurnSound);

        nextFlashlightTime = Time.time + flashlightCooldown;
    }

    #endregion

    #region Inputs Handled in Update for better control

    private void OnRun()
    {
        if (isGamepad)
        {
            bool pressed = runAction.IsPressed();

            if (pressed && !wasRunPressedLastFrame)
            {
                sprintToggled = !sprintToggled;
            }

            wasRunPressedLastFrame = pressed;
            currentInput.run = sprintToggled;
        }
        else
        {
            currentInput.run = runAction.IsPressed();
        }
    }

    #endregion

    #region Unity Methods

    private void OnValidate()
    {
        if (_playerInput == null)
        {
            _playerInput = GetComponent<PlayerInput>();
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

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
        runAction = playerInputs.Gameplay.Run;
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
