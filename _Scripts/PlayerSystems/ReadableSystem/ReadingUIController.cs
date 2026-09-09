using System.Collections;
using UnityEngine;
using TMPro;

public class ReadingUIController : MonoBehaviour
{
    [Header("Required Components")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject readingContent;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI bodyText;

    [Header("Fade Settings")]
    [SerializeField] private float fadeInSpeed = 6f;
    [SerializeField] private float fadeOutSpeed = 10f;

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        readingContent.SetActive(false);
    }

    public void Show(Readable readable)
    {
        titleText.text = readable.Title;
        bodyText.text = readable.BodyText;

        if (readable.TitleFont != null)
        {
            titleText.font = readable.TitleFont;
        }

        if (readable.BodyFont != null)
        {
            bodyText.font = readable.BodyFont;
        }

        readingContent.SetActive(true);

        StartFade(1f, fadeInSpeed);
    }

    public void Hide()
    {
        StartFade(0f, fadeOutSpeed);
    }

    private void StartFade(float targetAlpha, float speed)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(
            FadeCanvasGroup(targetAlpha, speed)
        );
    }

    private IEnumerator FadeCanvasGroup(
        float targetAlpha,
        float speed
    )
    {
        canvasGroup.blocksRaycasts = false;

        while (!Mathf.Approximately(
            canvasGroup.alpha,
            targetAlpha
        ))
        {
            canvasGroup.alpha = Mathf.MoveTowards(
                canvasGroup.alpha,
                targetAlpha,
                speed * Time.deltaTime
            );

            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

        if (targetAlpha <= 0f)
        {
            readingContent.SetActive(false);

            titleText.text = string.Empty;
            bodyText.text = string.Empty;
        }

        fadeCoroutine = null;
    }
}