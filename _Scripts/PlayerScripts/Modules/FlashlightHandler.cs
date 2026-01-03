using UnityEngine;

public class FlashlightHandler : MonoBehaviour
{
    [Header("Flashlight Parameters")]
    public Light flashLight;
    public Transform cameraAnchor;        // The player's camera or hand anchor
    public float walkSmoothing = 12f;
    public float runSmoothing = 6f;
    private float rotationSmooth;

    [Header("Flashlight Audio Components")]
    public AudioSource flashlightAudioSource;   // Audio source for flashlight sounds
    public AudioClip flashlightTurnSound;       // Sound played when toggling the flashlight

    [Header("Required Outside Components")]
    [SerializeField] PlayerLocomotion playerLocomotion;

    private void Update()
    {
        if (playerLocomotion.isFlashlightOn)
        {
            flashLight.enabled = true;
        }
        else
        {
            flashLight.enabled = false;
        }

        if (playerLocomotion.isRunning)
        {
            rotationSmooth = runSmoothing;
        }
        else
        {
            rotationSmooth = walkSmoothing;
        }
    }

    private void LateUpdate()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, cameraAnchor.rotation, rotationSmooth * Time.deltaTime);

        transform.position = cameraAnchor.position;
    }
}
