using UnityEngine;

public class FlashlightHandler : MonoBehaviour
{
    [Header("Flashlight Parameters")]
    private bool lastFlashlightState;
    public Light flashLight;
    public Transform cameraAnchor;        // The player's camera or hand anchor
    public float walkSmoothing = 12f;
    public float runSmoothing = 6f;
    private float rotationSmooth;

    [Header("Flashlight Audio Components")]
    public AudioSource flashlightAudioSource;   // Audio source for flashlight sounds
    public AudioClip flashlightTurnSound;       // Sound played when toggling the flashlight

    [Header("Required Outside Components")]
    [SerializeField] InputManager inputManager;
    [SerializeField] PlayerLocomotion playerLocomotion;

    private void Update()
    {
        bool flashlightRequested = inputManager.CurrentInput.flashlight;

        // Toggle light
        flashLight.enabled = flashlightRequested;

        // Play sound only on change
        if (flashlightRequested != lastFlashlightState)
        {
            flashlightAudioSource.PlayOneShot(flashlightTurnSound);
            lastFlashlightState = flashlightRequested;
        }

        rotationSmooth = playerLocomotion.isRunning ? runSmoothing : walkSmoothing;
    }

    private void LateUpdate()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, cameraAnchor.rotation, rotationSmooth * Time.deltaTime);

        transform.position = cameraAnchor.position;
    }
}
