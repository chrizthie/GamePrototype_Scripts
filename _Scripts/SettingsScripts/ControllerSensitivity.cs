using UnityEngine;
using UnityEngine.UI;

public class ControllerSensitivity : MonoBehaviour
{
    [SerializeField] PlayerLocomotionPreset preset;

    public void SetSensitivity(float sensitivity)
    {
        preset.controllerSensitivity = sensitivity;
    }
}
