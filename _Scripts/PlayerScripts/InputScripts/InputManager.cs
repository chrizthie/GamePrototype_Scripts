using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System;

[RequireComponent(typeof(PlayerLocomotion))]
public class InputManager : MonoBehaviour
{
    private PlayerInputState currentInput;
    public PlayerInputState CurrentInput => currentInput;

    [Header("Input Variables")]
    private float nextFlashlightTime;           // time when flashlight can be toggled again
    private float flashlightCooldown = 0.3f;    // to prevent rapid toggling
    private float lastCrouchTime = 0f;          // time of last crouch toggle
    private float crouchCooldown = 0.5f;        // to prevent rapid toggling
    private bool runRequested;                  // for controller toggle

    [Header("Inputs")]
    public bool PauseOpenCloseInput { get; private set; }
    [SerializeField] public bool isGamepad;
    [HideInInspector] public InputAction runAction;
    [HideInInspector] public InputAction _pauseOpenCloseAction;

    [Header("Required Components")]
    [SerializeField] PlayerInput _playerInput;
    [SerializeField] public static InputManager instance;
    [SerializeField] public InputSystem_Actions playerInputs;

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
        if(!value.isPressed) return;
        if (Time.time < nextFlashlightTime) return;

        currentInput.flashlight = !currentInput.flashlight;
        nextFlashlightTime = Time.time + flashlightCooldown;
    }

    #endregion

    #region Inputs Handled in Update for better control

    private void OnRun()
    {
        if (!isGamepad)
        {
            // keyboard = hold to run
            currentInput.run = runAction.IsPressed();
            return;
        }

        // gamepad logic
        bool isMoving = currentInput.move.sqrMagnitude > 0.01f;

        // press run ONCE to request running
        if (runAction.WasPressedThisFrame() && isMoving)
        {
            runRequested = true;
        }

        // stop running when movement stops
        if (!isMoving)
        {
            runRequested = false;
        }

        currentInput.run = runRequested && isMoving;
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
