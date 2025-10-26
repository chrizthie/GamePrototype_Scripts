using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class StaminaSystem : MonoBehaviour
{
    [Header("Stamina Main Parameters")]
    [SerializeField][Range(0, 100)] public float playerStamina = 100f;
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaMomentum = 5f;
    [SerializeField] private float staminaDegenTimer = 0f;
    [SerializeField] private float staminaRegenTimer = 0f;

    [Header("Stamina Regeneration Paramaters")]
    [SerializeField][Range(0, 40)] public float staminaDrain = 15f;
    [SerializeField][Range(0, 40)] public float staminaRegen = 10f;
    [SerializeField] private const float StaminaTimeToDegen = 1.5f;
    [SerializeField] private const float StaminaTimeToRegen = 2.5f;

    [Header("Required Components")]
    [SerializeField] Image staminaBar;
    [SerializeField] TextMeshProUGUI staminaTitle;
    [SerializeField] PlayerLocomotionPreset preset;
    [SerializeField] PlayerLocomotion playerLocomotion;
    [SerializeField] InputManager inputManager;

    private void Update()
    {
        staminaBar.fillAmount = playerStamina / maxStamina;

        if (inputManager.runAction.WasPressedThisFrame() && !playerLocomotion.obstacleOverhead)
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

        UpdateStaminaDisplay();
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

    private void UpdateStaminaDisplay()
    {
        staminaTitle.text = ((int)playerStamina).ToString();
    }

    #region Unity Methods

    private void OnValidate()
    {
        if (playerLocomotion == null)
        {
            playerLocomotion = GetComponentInParent<PlayerLocomotion>();
        }

        if (inputManager == null)
        {
            inputManager = GetComponentInParent<InputManager>();
        }
    }

    #endregion 
}
