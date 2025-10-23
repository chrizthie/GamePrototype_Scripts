using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class StaminaSystem : MonoBehaviour
{
    [Header("Stamina Main Parameters")]
    [SerializeField][Range(0, 100)] public float playerStamina = 100f;
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaMomentum = 10f;
    [SerializeField] private float staminaDegenTimer = 0f;
    [SerializeField] private float staminaRegenTimer = 0f;

    [Header("Stamina Regeneration Paramaters")]
    [SerializeField][Range(0, 50)] public float staminaDrain = 17f;
    [SerializeField][Range(0, 50)] public float staminaRegen = 36f;
    [SerializeField] private const float StaminaTimeToDegen = 1.5f;
    [SerializeField] private const float StaminaTimeToRegen = 2.5f;

    [Header("Required Components")]
    [SerializeField] Image staminaBar;
    [SerializeField] PlayerLocomotion playerLocomotion;
    [SerializeField] PlayerLocomotionPreset preset;
    [SerializeField] InputManager inputManager;
    //[SerializeField] InputSystem_Actions playerInputs;
    //[SerializeField] InputAction runAction;

    private void Update()
    {
        staminaBar.fillAmount = playerStamina / maxStamina;

        if (inputManager.runAction.triggered)
        {
            playerStamina = playerStamina - staminaMomentum;
        }

        if (playerLocomotion.isRunning)
        {
            HandleDegenStamina();
        }
        else
        {
            HandleRegenStamina();
        }

        HandleNoStamina();
    }

    private void HandleDegenStamina()
    {
 
        if (staminaDegenTimer >= StaminaTimeToDegen)
        {
            playerStamina = Mathf.Clamp(playerStamina - (staminaDrain * Time.deltaTime), 0f, maxStamina);
        }
        else
        {
            staminaDegenTimer += Time.deltaTime;
        }

        if (playerStamina <= 0)
        {
            playerStamina = 0f;
        }

        staminaRegenTimer = 0f;
    }

    private void HandleRegenStamina()
    {
        if (playerStamina < maxStamina)
        {
            if (playerStamina > 60)
            {
                playerStamina = Mathf.Clamp(playerStamina + (staminaRegen * Time.deltaTime), 0f, maxStamina);
            }
            else if (playerStamina < 60)
            {
                if (staminaRegenTimer >= StaminaTimeToRegen)
                {
                    playerStamina = Mathf.Clamp(playerStamina + (staminaRegen * Time.deltaTime), 0f, maxStamina);
                }
                else
                {
                    staminaRegenTimer += Time.deltaTime;
                }
            }
        }

        staminaDegenTimer = 0f;
    }

    private void HandleNoStamina()
    {
        if (playerStamina == 0)
        {
            playerLocomotion.maxSpeed = preset.walkSpeed;
        }
        else
        {
            playerLocomotion.maxSpeed = preset.runSpeed;
        }
    }

    #region Unity Methods

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    private void Awake()
    {
        playerLocomotion = GetComponentInParent<PlayerLocomotion>();
        inputManager = GetComponentInParent<InputManager>();
    }

    private void Start()
    {
        
    }


    #endregion 
}
