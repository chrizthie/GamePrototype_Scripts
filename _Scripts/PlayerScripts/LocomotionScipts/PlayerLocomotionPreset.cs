using UnityEngine;

[CreateAssetMenu(menuName = "Player/Locomotion Preset")]
public class PlayerLocomotionPreset : ScriptableObject
{
    [Header("Movement Parameters")]
    public float normalAcceleration = 9.5f;
    public float crouchAcceleration = 2.5f;
    public float crouchSpeed = 0.85f;
    public float walkSpeed = 1.45f;
    public float runSpeed = 4.45f;

    [Header("Looking Parameters")]
    public Vector2 lookSensitivity = new Vector2(0.1f, 0.1f);
    public float pitchUpLimit = 73f;
    public float pitchDownLimit = 73f;

    [Header("Camera Parameters")]
    public float cameraWalkFOV = 57f;
    public float cameraRunFOV = 62f;
    public float cameraFOVSmoothing = 5f;
    public float walkTiltAngle = 1f;
    public float runTiltAngle = 2f;
    public float tiltSmoothing = 3f;

    [Header("Physics Parameters")]
    public float gravityScale = 2f;
    public float maxGravity = 10f;
    public float rampingGravity = 0.2f;
}
