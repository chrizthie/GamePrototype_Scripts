using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System;

[RequireComponent(typeof(PlayerLocomotion))]
public class InputManager : MonoBehaviour
{
    [Header("Input Cooldowns")]
    // flashlight
    public bool canFlashlightTurn = true;
    private float flashlightCooldown = 0.2f;
    // crouch
    private float lastCrouchTime = 0f;
    private float crouchCooldown = 0.5f;

    [Header("Inputs")]
    [HideInInspector] public InputAction runAction;
    [HideInInspector] public InputAction _pauseOpenCloseAction;

    [Header("Input Bools")]
    [SerializeField] public bool PauseOpenCloseInput { get; private set; }

    [Header("Required Components")]
    [SerializeField] PlayerLocomotion playerLocomotion;
    [SerializeField] FlashlightHandler flashlightHandler;
    [SerializeField] public static InputManager instance;
    [SerializeField] public InputSystem_Actions playerInputs;
    [SerializeField] PlayerInput _playerInput;

    #region Input Handling

    private void Update()
    {
        // Running input
        if (runAction.IsPressed() && playerLocomotion.canRun && !playerLocomotion.obstacleOverhead && (playerLocomotion.moveInput.y == 1f || playerLocomotion.moveInput.x != 0f))
        {
            Debug.Log("Running...");
            playerLocomotion.runInput = true;
            playerLocomotion.crouchInput = false;
        }
        else
        {
            playerLocomotion.runInput = false;
        }

        // when pause button is pressed
        PauseOpenCloseInput = _pauseOpenCloseAction.WasPressedThisFrame();
    }

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

        if (flashlightHandler == null)
        {
            flashlightHandler = FindAnyObjectByType<FlashlightHandler>();
        }
    }

    private void OnEnable()
    {
        if (playerInputs == null)
        {
            playerInputs = new InputSystem_Actions();
        }

        playerInputs.Enable();
        runAction.Enable();
    }

    private void OnDisable()
    {
        playerInputs.Disable();
        runAction.Disable();
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        _playerInput = gameObject.GetComponent<PlayerInput>();
        _pauseOpenCloseAction = _playerInput.actions["PauseOpenClose"];
    }

    private void Start()
    {
        runAction = playerInputs.Player.Run;
    }
    #endregion
}
