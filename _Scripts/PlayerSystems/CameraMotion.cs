using UnityEngine;

public class CameraMotion : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerLocomotion playerLocomotion;

    [Header("Head Bob - Walk")]
    [SerializeField] private float walkFrequency = 8.5f;

    [Tooltip("Vertical camera movement in centimeters.")]
    [SerializeField] private float walkAmplitude = 0.4f;

    [Header("Head Bob - Run")]
    [SerializeField] private float runFrequency = 11f;

    [Tooltip("Vertical camera movement in centimeters.")]
    [SerializeField] private float runAmplitude = 0.7f;

    [Header("Head Bob - Crouch")]
    [SerializeField] private float crouchFrequency = 5.5f;

    [Tooltip("Vertical camera movement in centimeters.")]
    [SerializeField] private float crouchAmplitude = 0.25f;

    [Header("Position")]
    [SerializeField] private float horizontalMultiplier = 0.15f;

    [Header("Smoothing")]
    [SerializeField] private float positionSmoothing = 8f;

    [Header("Lateral Arc Bob")]
    [Tooltip("Side-to-side distance of the head arc in centimeters.")]
    [SerializeField] private float arcAmplitude = 1.2f;

    [Tooltip("Vertical movement produced by the lateral arc in centimeters.")]
    [SerializeField] private float arcVerticalAmplitude = 0.35f;

    [SerializeField] private float arcFrequency = 0.5f;

    [Tooltip("How much the arc contributes while walking.")]
    [SerializeField] private float arcWalkMultiplier = 1f;

    [Tooltip("How much the arc contributes while running.")]
    [SerializeField] private float arcRunMultiplier = 1.25f;

    [Tooltip("How much the arc contributes while crouching.")]
    [SerializeField] private float arcCrouchMultiplier = 0.6f;

    [Header("Arc Smoothing")]
    [SerializeField] private float arcWalkSmoothing = 5f;
    [SerializeField] private float arcRunSmoothing = 12f;
    [SerializeField] private float arcCrouchSmoothing = 3f;

    [Header("Landing Impact")]
    [Tooltip("Maximum downward camera displacement in centimeters.")]
    [SerializeField] private float landingDrop = 2f;
    [SerializeField] private float landingRecoverySpeed = 10f;

    private Vector3 baseLocalPosition;
    private Vector3 currentOffset;
    private Vector3 currentArcOffset;

    private float bobTimer;
    private float landingOffset;
    private float arcTimer = 1.2f;

    private void Start()
    {
        baseLocalPosition = transform.localPosition;
    }

    private void LateUpdate()
    {
        if (playerLocomotion == null)
            return;

        bool shouldBob =
            playerLocomotion.isGrounded &&
            !playerLocomotion.inPlace &&
            !playerLocomotion.isMovementPaused;

        Vector3 targetOffset = Vector3.zero;

        if (shouldBob)
        {
            float frequency;
            float amplitude;
            float arcMultiplier;
            float arcSmoothing;

            if (playerLocomotion.isCrouching)
            {
                frequency = crouchFrequency;
                amplitude = crouchAmplitude;

                arcMultiplier = arcCrouchMultiplier;
                arcSmoothing = arcCrouchSmoothing;
            }
            else if (playerLocomotion.isRunning)
            {
                frequency = runFrequency;
                amplitude = runAmplitude;

                arcMultiplier = arcRunMultiplier;
                arcSmoothing = arcRunSmoothing;
            }
            else
            {
                frequency = walkFrequency;
                amplitude = walkAmplitude;

                arcMultiplier = arcWalkMultiplier;
                arcSmoothing = arcWalkSmoothing;
            }

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

            // Convert centimeters to Unity units.
            float amplitudeMeters = amplitude * 0.01f;

            float vertical =
                Mathf.Sin(bobTimer) *
                amplitudeMeters;

            float horizontal =
                Mathf.Cos(bobTimer * 0.05f) *
                amplitudeMeters *
                horizontalMultiplier;

            targetOffset = new Vector3(
                horizontal,
                vertical,
                0f
            );

            // ---------------------------------
            // LATERAL ARC
            // ---------------------------------

            if (playerLocomotion.isCrouching)
            {
                arcMultiplier = arcCrouchMultiplier;
            }
            else if (playerLocomotion.isRunning)
            {
                arcMultiplier = arcRunMultiplier;
            }
            else
            {
                arcMultiplier = arcWalkMultiplier;
            }

            arcTimer += Time.deltaTime * frequency * arcFrequency;

            float arcHorizontal =
                Mathf.Sin(arcTimer) *
                arcAmplitude *
                0.01f *
                arcMultiplier;

            // Creates the curved rise/fall of the head
            // as it travels toward either side.
            float arcCurve =
                Mathf.Sin(arcTimer * 2f);

            float arcVertical =
                arcCurve *
                arcVerticalAmplitude *
                0.01f *
                arcMultiplier;

            Vector3 targetArcOffset = new Vector3(
                arcHorizontal,
                arcVertical,
                0f
            );

            currentArcOffset = Vector3.Lerp(
                currentArcOffset,
                targetArcOffset,
                arcSmoothing * Time.deltaTime
            );

            targetOffset += currentArcOffset;

        }
        else
        {
            currentArcOffset = Vector3.Lerp(
                currentArcOffset,
                Vector3.zero,
                arcWalkSmoothing * Time.deltaTime
            );
        }

        landingOffset = Mathf.Lerp(
            landingOffset,
            0f,
            landingRecoverySpeed * Time.deltaTime
        );

        currentOffset = Vector3.Lerp(
            currentOffset,
            targetOffset,
            positionSmoothing * Time.deltaTime
        );

        transform.localPosition =
            baseLocalPosition +
            currentOffset +
            new Vector3(0f, landingOffset, 0f);
    }


    public void AddLandingImpact(float intensity)
    {
        intensity = Mathf.Clamp01(intensity);

        // Make weak landings considerably softer.
        float impactStrength = intensity * intensity;

        float impact =
            landingDrop *
            0.01f *
            impactStrength;

        landingOffset = -impact;
    }
}