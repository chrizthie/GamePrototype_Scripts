using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public abstract class PauseMenu : MonoBehaviour
{
    [Header("Menu Behavior")]
    [SerializeField] private bool isModal = false;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.2f;

    public bool IsModal => isModal;
    private CanvasGroup canvasGroup;
    private Coroutine fadeRoutine;

    protected virtual void Awake()
    {
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
}