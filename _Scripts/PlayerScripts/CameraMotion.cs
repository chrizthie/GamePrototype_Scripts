using UnityEngine;

public class CameraMotion : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerLocomotion playerLocomotion;

    [Header("Head Bob")]
    [SerializeField] private float walkFrequency = 6.5f;
    [SerializeField] private float walkAmplitude = 0.01f;

    [SerializeField] private float runFrequency = 8.5f;
    [SerializeField] private float runAmplitude = 0.018f;

    [SerializeField] private float crouchFrequency = 4.5f;
    [SerializeField] private float crouchAmplitude = 0.005f;

    [SerializeField] private float smoothing = 10f;

    private Vector3 baseLocalPosition;
    private Vector3 currentOffset;
    private float bobTimer;

    private void Start()
    {
        baseLocalPosition = transform.localPosition;
    }

    private void LateUpdate()
    {
        if (playerLocomotion == null)
            return;

        Vector3 targetOffset = Vector3.zero;

        bool shouldBob =
            playerLocomotion.isGrounded &&
            !playerLocomotion.inPlace &&
            !playerLocomotion.isMovementPaused;

        if (shouldBob)
        {
            float frequency;
            float amplitude;

            if (playerLocomotion.isCrouching)
            {
                frequency = crouchFrequency;
                amplitude = crouchAmplitude;
            }
            else if (playerLocomotion.isRunning)
            {
                frequency = runFrequency;
                amplitude = runAmplitude;
            }
            else
            {
                frequency = walkFrequency;
                amplitude = walkAmplitude;
            }

            bobTimer += Time.deltaTime * frequency;

            float vertical = Mathf.Sin(bobTimer) * amplitude;
            float horizontal = Mathf.Cos(bobTimer * 0.5f) * amplitude * 0.35f;

            targetOffset = new Vector3(horizontal, vertical, 0f);
        }
        else
        {
            bobTimer = 0f;
        }

        currentOffset = Vector3.Lerp(currentOffset, targetOffset, smoothing * Time.deltaTime);
        transform.localPosition = baseLocalPosition + currentOffset;
    }
}