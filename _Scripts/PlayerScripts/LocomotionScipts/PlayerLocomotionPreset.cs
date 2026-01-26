using UnityEngine;

[CreateAssetMenu(menuName = "Player/Locomotion Preset")]
public class PlayerLocomotionPreset : ScriptableObject
{
    [Header("Movement Parameters")]
    public float normalAcceleration = 6f;
    public float runningAcceleration = 8f;
    public float crouchAcceleration = 2f;
    public float crouchSpeed = 0.85f;
    public float walkSpeed = 1.45f;
    public float runSpeed = 4.45f;

    [Header("Mouse Parameters")]
    public float mouseSensitivity = 0.45f; // mouse sensitivity
    public float mouseMin = 0.02f;
    public float mouseMax = 0.18f;
    
    [Header("Controller Parameters")]
    public float controllerSensitivity = 0.45f; // controller sensitivity
    public float controllerMin = 0.8f;
    public float controllerMax = 1.8f;


    [Header("Camera Parameters")]
    public float cameraWalkFOV = 57f;
    public float cameraRunFOV = 62f;
    public float cameraFOVSmoothing = 5f;
    public float pitchUpLimit = 73f;
    public float pitchDownLimit = 73f;
    public float walkTiltAngle = 1f;
    public float runTiltAngle = 2f;
    public float tiltSmoothing = 3f;

    [Header("Physics Parameters")]
    public float gravityScale = 2f;
    public float maxGravity = 10f;
    public float rampingGravity = 0.2f;
}
