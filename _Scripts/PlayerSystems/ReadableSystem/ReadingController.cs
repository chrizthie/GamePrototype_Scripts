using UnityEngine;

public class ReadingController : MonoBehaviour
{
    public enum ReadingState
    {
        Idle,
        Transitioning,
        Reading,
        Exiting,
        Cooldown
    }

    private float transitionProgress;
    private Readable activeReadable;
    private string selectedWord;
    private float cooldownTimer;
    private Vector3 readingLookTarget;
    private bool glyphChaosAudioPlayed;
    private bool glyphConvergenceAudioPlayed;

    [Header("Reading Cooldown")]
    [SerializeField] private float readingCooldown = 0.5f;

    [Header("Reading State")]
    [SerializeField] private ReadingState currentState = ReadingState.Idle;

    [Header("Required Components")]
    [SerializeField] private PlayerInteract playerInteract;
    [SerializeField] private PlayerLocomotionPreset preset;
    [SerializeField] private ReadableWordDatabase wordDatabase;
    [SerializeField] private ReadingWordEffect readingWordEffect;
    [SerializeField] private ReadingUIController readingUIController;
    [SerializeField] private PlayerLocomotion playerLocomotion;
    [SerializeField] private ReadingAudioController readingAudioController;

    private void Update()
    {
        Readable currentReadable = playerInteract.currentReadable;

        bool isHoldingZoom =
            InputManager.instance.CurrentInput.zoom;

        switch (currentState)
        {
            case ReadingState.Idle:

                if (currentReadable != null && isHoldingZoom)
                {
                    StartReading(currentReadable);
                }

                break;


            case ReadingState.Transitioning:

                // Player released RMB
                if (!isHoldingZoom)
                {
                    EndReading();
                    break;
                }

                // Player looked away or is looking at another readable
                if (currentReadable != activeReadable)
                {
                    EndReading();
                    break;
                }

                CompleteTransition();

                break;


            case ReadingState.Reading:

                // Keep camera completely locked on the readable
                if (activeReadable != null)
                {
                    playerLocomotion.SetReadingLookTarget(
                        readingLookTarget,
                        1f
                    );
                }

                if (!isHoldingZoom || currentReadable != activeReadable)
                {
                    EndReading();
                }

                break;


            case ReadingState.Exiting:

                CompleteExit();

                break;

            case ReadingState.Cooldown:

                cooldownTimer -= Time.deltaTime;

                if (cooldownTimer <= 0f)
                {
                    currentState = ReadingState.Idle;
                }

                break;
        }
    }

    private void StartReading(Readable readable)
    {
        activeReadable = readable;

        if (!playerInteract.TryGetReadableCenter(readable, out readingLookTarget))
        {
            readingLookTarget = readable.transform.position;
        }

        selectedWord = GetSelectedWord(activeReadable);

        // If this readable uses already-read behavior
        // and has already been read, skip the glyph transition.
        if (activeReadable.AlreadyReadBehavior &&
            activeReadable.HasBeenRead)
        {
            readingUIController.Show(activeReadable);

            readingAudioController.PlayState(
                activeReadable,
                ReadableAudioState.ReadingOpen
            );

            currentState = ReadingState.Reading;

            Debug.Log(
                "Already read. Skipping reading transition."
            );

            return;
        }

        // First read or repeatable readable
        readingAudioController.PlayState(
            activeReadable,
            ReadableAudioState.TransitionStart
        );

        glyphChaosAudioPlayed = false;
        glyphConvergenceAudioPlayed = false;

        readingWordEffect.StartEffect(selectedWord);

        currentState = ReadingState.Transitioning;
        transitionProgress = 0f;

        Debug.Log(
            "Reading Transition Started: " +
            activeReadable.name +
            " | Selected Word: " +
            selectedWord
        );

    }

    private string GetSelectedWord(Readable readable)
    {
        // Custom word always takes priority
        if (readable.UseCustomWord)
        {
            return readable.CustomWord;
        }

        if (wordDatabase == null)
        {
            Debug.LogWarning("ReadingController: Word Database is not assigned.");
            return string.Empty;
        }

        string[] wordPool = null;

        switch (readable.Type)
        {
            case Readable.ReadableType.Safe:
                wordPool = wordDatabase.safeWords;
                break;

            case Readable.ReadableType.Lore:
                wordPool = wordDatabase.loreWords;
                break;

            case Readable.ReadableType.Warning:
                wordPool = wordDatabase.warningWords;
                break;

            case Readable.ReadableType.Entity:
                wordPool = wordDatabase.entityWords;
                break;

            case Readable.ReadableType.Corrupted:
                wordPool = wordDatabase.corruptedWords;
                break;

            case Readable.ReadableType.StoryCritical:
                Debug.LogWarning(
                    "StoryCritical Readable has no Custom Word."
                );
                return string.Empty;
        }

        if (wordPool == null || wordPool.Length == 0)
        {
            Debug.LogWarning(
                "ReadingController: No words found for type: " +
                readable.Type
            );

            return string.Empty;
        }

        return wordPool[Random.Range(0, wordPool.Length)];
    }

    private void CompleteTransition()
    {
        transitionProgress = Mathf.MoveTowards(
            transitionProgress,
            1f,
            preset.cameraFOVChangeSpeed * Time.deltaTime
        );

        // Glyph chaos
        if (!glyphChaosAudioPlayed &&
            transitionProgress >= 0.1f)
        {
            readingAudioController.PlayState(
                activeReadable,
                ReadableAudioState.GlyphChaos
            );

            glyphChaosAudioPlayed = true;
        }

        // Glyph convergence
        if (!glyphConvergenceAudioPlayed &&
            transitionProgress >=
            readingWordEffect.ConvergenceStartProgress)
        {
            readingAudioController.PlayState(
                activeReadable,
                ReadableAudioState.GlyphConvergence
            );

            glyphConvergenceAudioPlayed = true;
        }

        readingWordEffect.UpdateEffect(
            transitionProgress
        );

        // Gradually lock camera to readable
        playerLocomotion.SetReadingLookTarget(
            readingLookTarget,
            transitionProgress
        );

        if (transitionProgress >= 1f)
        {
            Debug.Log("Transition Complete");

            readingWordEffect.FadeOutEffect();

            readingUIController.Show(activeReadable);

            readingAudioController.PlayState(
                activeReadable,
                ReadableAudioState.ReadingOpen
            );

            if (activeReadable.AlreadyReadBehavior)
            {
                activeReadable.MarkAsRead();
            }

            currentState = ReadingState.Reading;
        }
    }

    private void EndReading()
    {
        currentState = ReadingState.Exiting;

        cooldownTimer = readingCooldown;

        readingAudioController.PlayState(
            activeReadable,
            ReadableAudioState.ReadingClose
        );

        // Restore normal camera control
        playerLocomotion.ClearReadingLookTarget();

        readingUIController.Hide();

        readingWordEffect.StopEffect();

        Debug.Log("Reading Ended");
    }

    private void CompleteExit()
    {
        Debug.Log("Exit Complete");

        activeReadable = null;
        currentState = ReadingState.Cooldown;
    }
}