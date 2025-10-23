using UnityEngine;

public class PlayerBreathing : MonoBehaviour
{
    [Header("Breathing Audio Clips")]
    [SerializeField] AudioClip normalBreathing;
    [SerializeField] AudioClip moderateBreathing;
    [SerializeField] AudioClip heavyBreathing;

    [Header("Required Components")]
    [SerializeField] StaminaSystem staminaSystem;
    [SerializeField] AudioSource breathAudioSource;

    #region Unity Methods 

    private void Update()
    {
        
    }

    private void OnValidate()
    {
        if (staminaSystem == null)
        {
            staminaSystem = GetComponentInParent<StaminaSystem>();
        }
    }

    #endregion
}
