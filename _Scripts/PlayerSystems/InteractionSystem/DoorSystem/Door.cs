using UnityEngine;

public class Door : Interactable
{
    public enum DoorState
    {
        Closed,
        PartialOpen,
        FullyOpening,
        FullyOpen,
        Closing,
        LatchPause,
        LatchClosing
    }

    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Door")]
    [SerializeField] private Transform doorPivot;

    [Header("Audio")]
    [SerializeField] private AudioSource doorOneShotAudio;
    [SerializeField] private AudioSource doorCreakAudio;

    [SerializeField] private AudioClip openAudio;
    [SerializeField] private AudioClip closeAudio;
    [SerializeField] private AudioClip creakAudio;

    [Header("Creak Audio")]
    [Tooltip("Maximum volume of the creak.")]
    [SerializeField] private float creakMaxVolume = 0.8f;

    [Tooltip("Speed at which the creak reaches maximum volume.")]
    [SerializeField] private float creakFullSpeed = 40f;

    [Tooltip("How quickly the creak volume responds to speed changes.")]
    [SerializeField] private float creakFadeSpeed = 8f;

    [Header("Opening")]
    [Tooltip("First interaction opening angle.")]
    [SerializeField] private float partialOpenAngle = 15f;

    [Tooltip("Speed of the first opening.")]
    [SerializeField] private float partialOpenSpeed = 30f;

    [Tooltip("Maximum opening angle.")]
    [SerializeField] private float fullOpenAngle = 90f;

    [Tooltip("Speed of the second opening.")]
    [SerializeField] private float fullOpenSpeed = 70f;

    [Header("Closing")]
    [Tooltip("Speed when closing the door.")]
    [SerializeField] private float closeSpeed = 70f;

    [Tooltip("Angle where the latch-closing phase begins.")]
    [SerializeField] private float latchAngle = 5f;

    [Tooltip("How long the door pauses before the latch closes.")]
    [SerializeField] private float latchPauseTime = 0.2f;

    [Tooltip("Speed of the final latch-closing movement.")]
    [SerializeField] private float latchSpeed = 120f;

    [Header("Smoothing")]
    [Tooltip("How quickly the door accelerates and slows.")]
    [SerializeField] private float movementSmooth = 8f;

    [Header("State")]
    [SerializeField] private DoorState currentState = DoorState.Closed;

    private Quaternion closedRotation;

    private float currentSpeed;
    private float latchTimer;

    private float currentAngle;
    private float targetAngle;

    // Direction chosen when the door first opens.
    private float openingDirection = 1f;

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player =
                    playerObject.transform;
            }
        }

        if (doorPivot == null)
        {
            Debug.LogError(
                "Door: Door Pivot is not assigned.",
                this
            );

            enabled = false;
            return;
        }

        closedRotation =
            doorPivot.localRotation;

        currentAngle = 0f;
        targetAngle = 0f;
        currentSpeed = 0f;
        latchTimer = 0f;

        if (doorCreakAudio != null)
        {
            doorCreakAudio.clip = creakAudio;
            doorCreakAudio.loop = true;
            doorCreakAudio.playOnAwake = false;
            doorCreakAudio.volume = 0f;
        }

        ApplyRotation();
    }

    private void Update()
    {
        switch (currentState)
        {
            case DoorState.PartialOpen:
                UpdatePartialOpen();
                break;

            case DoorState.FullyOpening:
                UpdateFullOpen();
                break;

            case DoorState.Closing:
                UpdateClosing();
                break;

            case DoorState.LatchPause:
                UpdateLatchPause();
                break;

            case DoorState.LatchClosing:
                UpdateLatchClosing();
                break;
        }

        UpdateCreakAudio();
    }

    // =========================================================
    // INTERACTION
    // =========================================================

    protected override void Interact()
    {
        switch (currentState)
        {
            case DoorState.Closed:
                StartPartialOpen();
                break;

            case DoorState.PartialOpen:
                StartFullOpen();
                break;

            case DoorState.FullyOpening:
            case DoorState.FullyOpen:
                StartClosing();
                break;
        }
    }

    // =========================================================
    // PARTIAL OPEN
    // =========================================================

    private void StartPartialOpen()
    {
        openingDirection =
            GetOpeningDirection();

        targetAngle =
            partialOpenAngle *
            openingDirection;

        currentSpeed = 0f;

        currentState =
            DoorState.PartialOpen;

        PlayOneShot(openAudio);
    }

    private void UpdatePartialOpen()
    {
        MoveDoorToTarget(
            targetAngle,
            partialOpenSpeed
        );

        if (Mathf.Abs(currentAngle - targetAngle) <= 0.001f)
        {
            currentAngle =
                targetAngle;

            currentSpeed = 0f;

            ApplyRotation();
        }
    }

    // =========================================================
    // FULL OPEN
    // =========================================================

    private void StartFullOpen()
    {
        targetAngle =
            fullOpenAngle *
            openingDirection;

        currentSpeed = 0f;

        currentState =
            DoorState.FullyOpening;
    }

    private void UpdateFullOpen()
    {
        MoveDoorToTarget(
            targetAngle,
            fullOpenSpeed
        );

        if (Mathf.Abs(currentAngle - targetAngle) <= 0.001f)
        {
            currentAngle =
                targetAngle;

            currentSpeed = 0f;

            currentState =
                DoorState.FullyOpen;

            ApplyRotation();
        }
    }

    // =========================================================
    // CLOSE
    // =========================================================

    private void StartClosing()
    {
        targetAngle = 0f;

        currentSpeed = 0f;

        currentState =
            DoorState.Closing;
    }

    private void UpdateClosing()
    {
        float distanceToClosed =
            Mathf.Abs(currentAngle);

        // Normal closing phase.
        if (distanceToClosed > latchAngle)
        {
            MoveDoorToTarget(
                0f,
                closeSpeed
            );

            return;
        }

        // Hold at the latch angle.
        currentAngle =
            openingDirection *
            latchAngle;

        currentSpeed = 0f;

        ApplyRotation();

        latchTimer = 0f;

        currentState =
            DoorState.LatchPause;
    }

    // =========================================================
    // LATCH PAUSE
    // =========================================================

    private void UpdateLatchPause()
    {
        currentSpeed = 0f;

        latchTimer +=
            Time.deltaTime;

        if (latchTimer >= latchPauseTime)
        {
            PlayOneShot(closeAudio);

            currentState =
                DoorState.LatchClosing;
        }
    }

    // =========================================================
    // LATCH CLOSING
    // =========================================================

    private void UpdateLatchClosing()
    {
        float closingDirection =
            -openingDirection;

        currentSpeed =
            closingDirection *
            latchSpeed;

        currentAngle =
            Mathf.MoveTowards(
                currentAngle,
                0f,
                latchSpeed *
                Time.deltaTime
            );

        ApplyRotation();

        if (Mathf.Abs(currentAngle) <= 0.001f)
        {
            currentAngle = 0f;
            currentSpeed = 0f;

            currentState =
                DoorState.Closed;

            ApplyRotation();
        }
    }

    // =========================================================
    // OPENING DIRECTION
    // =========================================================

    private float GetOpeningDirection()
    {
        if (player == null)
            return 1f;

        Vector3 toPlayer =
            player.position -
            doorPivot.position;

        toPlayer.y = 0f;

        if (toPlayer.sqrMagnitude <= 0.001f)
            return 1f;

        toPlayer.Normalize();

        Vector3 doorForward =
            doorPivot.parent != null
                ? doorPivot.parent.TransformDirection(
                    closedRotation *
                    Vector3.forward
                )
                : closedRotation *
                  Vector3.forward;

        doorForward.y = 0f;

        if (doorForward.sqrMagnitude <= 0.001f)
            return 1f;

        doorForward.Normalize();

        float side =
            Vector3.Dot(
                toPlayer,
                doorForward
            );

        // Open AWAY from the player.
        return side >= 0f
            ? -1f
            : 1f;
    }

    // =========================================================
    // ROTATION
    // =========================================================

    private void ApplyRotation()
    {
        doorPivot.localRotation =
            closedRotation *
            Quaternion.Euler(
                0f,
                currentAngle,
                0f
            );
    }

    // =========================================================
    // MOVEMENT
    // =========================================================

    private void MoveDoorToTarget(
        float destination,
        float maxSpeed)
    {
        float direction =
            Mathf.Sign(
                destination -
                currentAngle
            );

        float distance =
            Mathf.Abs(
                destination -
                currentAngle
            );

        // Slow down as we approach the destination.
        float speedFactor =
            Mathf.InverseLerp(
                0f,
                15f,
                distance
            );

        float targetSpeed =
            maxSpeed *
            speedFactor *
            direction;

        currentSpeed =
            Mathf.Lerp(
                currentSpeed,
                targetSpeed,
                movementSmooth *
                Time.deltaTime
            );

        currentAngle +=
            currentSpeed *
            Time.deltaTime;

        // Prevent overshooting.
        if ((direction > 0f &&
             currentAngle >= destination) ||
            (direction < 0f &&
             currentAngle <= destination))
        {
            currentAngle =
                destination;

            currentSpeed = 0f;
        }

        ApplyRotation();
    }

    // =========================================================
    // AUDIO
    // =========================================================

    private void PlayOneShot(AudioClip clip)
    {
        if (doorOneShotAudio == null)
            return;

        if (clip == null)
            return;

        doorOneShotAudio.PlayOneShot(clip);
    }

    private void UpdateCreakAudio()
    {
        if (doorCreakAudio == null ||
            creakAudio == null)
            return;

        float movementSpeed =
            Mathf.Abs(currentSpeed);

        float targetVolume =
            Mathf.InverseLerp(
                0f,
                creakFullSpeed,
                movementSpeed
            );

        targetVolume *=
            creakMaxVolume;

        doorCreakAudio.volume =
            Mathf.MoveTowards(
                doorCreakAudio.volume,
                targetVolume,
                creakFadeSpeed *
                Time.deltaTime
            );

        if (doorCreakAudio.volume > 0.01f)
        {
            if (!doorCreakAudio.isPlaying)
            {
                doorCreakAudio.Play();
            }
        }
        else
        {
            if (doorCreakAudio.isPlaying)
            {
                doorCreakAudio.Stop();
            }
        }
    }
}