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
    [SerializeField] private float staminaMomentum = 8f;
    [SerializeField] private float staminaDegenTimer = 0f;
    [SerializeField] private float staminaRegenTimer = 0f;

    [Header("Stamina Regeneration Paramaters")]
    [SerializeField][Range(0, 40)] public float staminaDrain = 11f;
    [SerializeField][Range(0, 40)] public float staminaRegen = 14f;
    [SerializeField] private const float StaminaTimeToDegen = 1.5f;
    [SerializeField] private const float StaminaTimeToRegen = 2.5f;

    [Header("Required Components")]
    [SerializeField] Image staminaBar;
    [SerializeField] TextMeshProUGUI staminaTitle;
    [SerializeField] PlayerLocomotionPreset preset;
    [SerializeField] PlayerLocomotion playerLocomotion;
    [SerializeField] InputManager inputManager;

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
        if (playerStamina >= maxStamina)
            return;

        float regenMultiplier = 1f;

        // Horror rule: hiding helps you recover
        if (playerLocomotion.isCrouching)
        {
            regenMultiplier = 1.4f;
        }

        if (playerStamina >= 60f)
        {
            playerStamina += staminaRegen * regenMultiplier * Time.deltaTime;
        }
        else
        {
            if (staminaRegenTimer >= StaminaTimeToRegen)
            {
                playerStamina += staminaRegen * regenMultiplier * Time.deltaTime;
            }
            else
            {
                staminaRegenTimer += Time.deltaTime;
            }
        }

        staminaDegenTimer = 0f;
    }

    private void UpdateStaminaDisplay()
    {
        staminaTitle.text = ((int)playerStamina).ToString();
    }

    #region Unity Methods

    private void Update()
    {
        // UI fill
        staminaBar.fillAmount = playerStamina / maxStamina;

        // 1. Run start momentum cost (ONE FRAME ONLY)
        if (playerLocomotion.justStartedRunning)
        {
            playerStamina = Mathf.Max(playerStamina - staminaMomentum, 0f);
        }

        // 2. Continuous drain / regen
        if (playerLocomotion.isRunning)
        {
            HandleDegenStamina();
        }
        else
        {
            HandleRegenStamina();
        }

        // 3. Final clamp safety (optional but clean)
        playerStamina = Mathf.Clamp(playerStamina, 0f, maxStamina);

        // 4. Text update
        UpdateStaminaDisplay();
    }

    #endregion 
}
