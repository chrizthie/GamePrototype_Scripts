using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public abstract class PauseMenu : MonoBehaviour
{
    [Header("Menu Behavior")]
    [SerializeField] private bool isModal = false;

    [Header("Navigation")]
    [SerializeField] private GameObject defaultSelected;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.2f;

    public bool IsModal => isModal;
    private CanvasGroup canvasGroup;
    private Coroutine fadeRoutine;
    private EventSystem eventSystem;

    protected virtual void Awake()
    {
        eventSystem = EventSystem.current;
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    public virtual void OnOpen()
    {
        gameObject.SetActive(true);
        StartFade(1f);

        // Clear selection always
        eventSystem.SetSelectedGameObject(null);

        // Only auto-select when using a gamepad
        if (defaultSelected != null && PauseMenuManager.Instance != null)
        {
            if (PauseMenuManager.Instance.InputManager.isGamepad)
            {
                eventSystem.SetSelectedGameObject(defaultSelected);
            }
        }
    }

    public virtual void OnClose()
    {
        StartFade(0f, () => gameObject.SetActive(false));
    }

    private void StartFade(float targetAlpha, System.Action onComplete = null)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha, onComplete));
    }

    private IEnumerator FadeRoutine(float targetAlpha, System.Action onComplete)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = false;

        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

        bool visible = targetAlpha > 0.9f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;

        onComplete?.Invoke();
    }

    protected virtual void Update()
    {
        if (!GamePause.IsPaused || !gameObject.activeInHierarchy || PauseMenuManager.Instance == null)
        {
            return;
        }

        var inputManager = PauseMenuManager.Instance.InputManager;

        if (!inputManager.isGamepad)
        {
            return;
        }

        if (defaultSelected != null && eventSystem.currentSelectedGameObject == null)
        {
            eventSystem.SetSelectedGameObject(defaultSelected);
        }
    }
}