using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System;

[RequireComponent(typeof(PlayerLocomotion))]
public class InputManager : MonoBehaviour
{
    public InputSystem_Actions playerInputs;

    [Header("Components")]
    [SerializeField] PlayerLocomotion playerLocomotion;
    [SerializeField] FlashlightHandler flashlightHandler;

    [Header("Input Cooldowns")]
    // flashlight
    public bool canFlashlightTurn = true;
    private float flashlightCooldown = 0.2f;
    // crouch
    private float lastCrouchTime = 0f;
    private float crouchCooldown = 0.5f;

    [SerializeField] private InputAction runAction;

    #region Input Handling

    private void Update()
    {
        // Running input
        if (runAction.IsPressed() && (playerLocomotion.moveInput.y == 1f || playerLocomotion.moveInput.x != 0f))
        {
            Debug.Log("Running...");
            playerLocomotion.runInput = true;
            playerLocomotion.crouchInput = false;
        }
        else
        {
            playerLocomotion.runInput = false;
        }
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
        //if (value.isPressed && !playerLocomotion.isRunning)
        {
            //playerLocomotion.crouchInput = !playerLocomotion.crouchInput;
        }
        // Only proceed if input was pressed and player isn't running
        if (value.isPressed && !playerLocomotion.isRunning)
        {
            // Check if enough time has passed since last crouch
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
            flashlightHandler = GetComponent<FlashlightHandler>();
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

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        runAction = playerInputs.Player.Run;
    }
    #endregion
}
