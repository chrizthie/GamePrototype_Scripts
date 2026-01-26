using UnityEngine;
using UnityEngine.UI;

public class MouseSensitivity : MonoBehaviour
{
    [SerializeField] PlayerLocomotionPreset preset;

    public void SetSensitivity(float sensitivity)
    {
        preset.mouseSensitivity = sensitivity;
    }
}
