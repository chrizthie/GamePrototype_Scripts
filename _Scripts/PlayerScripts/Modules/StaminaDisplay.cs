using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StaminaDisplay : MonoBehaviour
{
    [Header("Required Components")]
    [SerializeField] StaminaSystem staminaSystem;
    [SerializeField] TextMeshProUGUI staminaTitle;

    private void Awake()
    {
        staminaSystem = GetComponent<StaminaSystem>();
    }

    private void Update()
    {
        UpdateStaminaDisplay();
    }

    private void UpdateStaminaDisplay()
    {
        staminaTitle.text = ((int)staminaSystem.playerStamina).ToString();
    }
}
