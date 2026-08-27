using UnityEngine;
using TMPro;

public class ReadingWordEffect : MonoBehaviour
{
    [Header("Required Components")]
    [SerializeField] private TextMeshProUGUI flashWordText;
    [SerializeField] private CanvasGroup flashWordCanvasGroup;
    [SerializeField] private CanvasGroup glyphContainerCanvasGroup;

    [Header("Glyph Instability")]
    [SerializeField] private float instabilityEndProgress = 0.55f;
    [SerializeField] private float characterChangeSpeed = 15f;
    [SerializeField] private float jitterAmount = 15f;
    [SerializeField] private float glyphFlickerSpeed = 8f;

    [Header("Glyph Convergence")]
    [SerializeField] private float convergenceStartProgress = 0.55f;
    [SerializeField] private float convergenceEndProgress = 0.7f;
    [SerializeField] private float extraGlyphFadeSpeed = 10f;
    [SerializeField] private float letterSpacing = 20f;
    [SerializeField] private float letterHorizontalVariation = 2f;
    [SerializeField] private float letterVerticalVariation = 4f;

    [Header("Glyph Settings")]
    [SerializeField] private RectTransform glyphContainer;
    [SerializeField] private TextMeshProUGUI glyphPrefab;
    [SerializeField] private int glyphCount = 12;
    [SerializeField] private float spawnRadiusX = 250f;
    [SerializeField] private float spawnRadiusY = 140f;

    [Header("Flash Settings")]
    [SerializeField] private float flashStartProgress = 0.7f;
    [SerializeField] private float flashEndProgress = 0.9f;
    [SerializeField] private float wordFlickerSpeed = 20f;

    [Header("Effect Fade")]
    [SerializeField] private float effectFadeSpeed = 8f;

    private bool isFadingOut;
    private string selectedWord;

    private readonly System.Collections.Generic.List<TextMeshProUGUI> spawnedGlyphs
    = new System.Collections.Generic.List<TextMeshProUGUI>();
    private readonly System.Collections.Generic.List<Vector2> glyphBasePositions
    = new System.Collections.Generic.List<Vector2>();
    private readonly System.Collections.Generic.List<Vector2> letterTargetPositions
    = new System.Collections.Generic.List<Vector2>();

    private readonly string[] glyphCharacters =
    {
        "#", "@", "%", "&", "*",
        "?", "!", "7", "X",
        "ϟ", "∆", "∑", "◊"
    };

    private void Update()
    {
        if (!isFadingOut)
            return;

        flashWordCanvasGroup.alpha = Mathf.MoveTowards(
            flashWordCanvasGroup.alpha,
            0f,
            effectFadeSpeed * Time.deltaTime
        );

        glyphContainerCanvasGroup.alpha = Mathf.MoveTowards(
            glyphContainerCanvasGroup.alpha,
            0f,
            effectFadeSpeed * Time.deltaTime
        );

        if (flashWordCanvasGroup.alpha <= 0f &&
            glyphContainerCanvasGroup.alpha <= 0f)
        {
            flashWordText.gameObject.SetActive(false);
            glyphContainer.gameObject.SetActive(false);

            isFadingOut = false;
        }
    }

    public void StartEffect(string word)
    {
        selectedWord = word;

        flashWordText.text = selectedWord;

        flashWordCanvasGroup.alpha = 0f;
        flashWordText.gameObject.SetActive(false);

        glyphContainerCanvasGroup.alpha = 1f;
        glyphContainer.gameObject.SetActive(true);

        isFadingOut = false;

        GenerateLetterTargets();

        SpawnGlyphs();
        SortGlyphsForConvergence();
    }

    public void UpdateEffect(float transitionProgress)
    {
        // CHAOS
        if (transitionProgress <= instabilityEndProgress)
        {
            UpdateGlyphInstability();
        }

        // CONVERGENCE
        if (transitionProgress >= convergenceStartProgress)
        {
            UpdateGlyphConvergence(transitionProgress);
        }

        // WORD FLASH
        if (transitionProgress >= flashStartProgress &&
            transitionProgress <= flashEndProgress)
        {
            if (!flashWordText.gameObject.activeSelf)
            {
                flashWordText.gameObject.SetActive(true);
            }

            float flicker = Mathf.PingPong(
                Time.time * wordFlickerSpeed,
                1f
            );

            flashWordCanvasGroup.alpha = flicker;
        }
        else if (transitionProgress < flashStartProgress)
        {
            flashWordCanvasGroup.alpha = 0f;
            flashWordText.gameObject.SetActive(false);
        }
    }

    public void StopEffect()
    {
        isFadingOut = false;

        flashWordCanvasGroup.alpha = 0f;
        flashWordText.gameObject.SetActive(false);

        glyphContainerCanvasGroup.alpha = 0f;
        glyphContainer.gameObject.SetActive(false);

        ClearGlyphs();
    }

    private void SpawnGlyphs()
    {
        ClearGlyphs();

        int requiredGlyphCount = Mathf.Max(glyphCount, selectedWord.Length);

        for (int i = 0; i < requiredGlyphCount; i++)
        {
            TextMeshProUGUI glyph = Instantiate(
                glyphPrefab,
                glyphContainer
            );

            glyph.text = glyphCharacters[
                Random.Range(0, glyphCharacters.Length)
            ];

            RectTransform glyphRect = glyph.rectTransform;

            float randomX = Random.Range(
                -spawnRadiusX,
                spawnRadiusX
            );

            float randomY = Random.Range(
                -spawnRadiusY,
                spawnRadiusY
            );

            Vector2 spawnPosition = new Vector2(randomX, randomY);

            glyphRect.anchoredPosition = spawnPosition;

            spawnedGlyphs.Add(glyph);
            glyphBasePositions.Add(spawnPosition);
        }
    }

    private void ClearGlyphs()
    {
        foreach (TextMeshProUGUI glyph in spawnedGlyphs)
        {
            if (glyph != null)
            {
                Destroy(glyph.gameObject);
            }
        }

        spawnedGlyphs.Clear();
        glyphBasePositions.Clear();
    }

    private void UpdateGlyphInstability()
    {
        for (int i = 0; i < spawnedGlyphs.Count; i++)
        {
            TextMeshProUGUI glyph = spawnedGlyphs[i];

            if (glyph == null)
                continue;

            // Randomly change character
            if (Random.value < characterChangeSpeed * Time.deltaTime)
            {
                glyph.text = glyphCharacters[
                    Random.Range(0, glyphCharacters.Length)
                ];
            }

            // Jitter around its original spawn position
            Vector2 jitter =
                Random.insideUnitCircle * jitterAmount;

            glyph.rectTransform.anchoredPosition =
                glyphBasePositions[i] + jitter;

            // Individual flickering
            float flicker = Mathf.PingPong(
                (Time.time + i) * glyphFlickerSpeed,
                1f
            );

            Color glyphColor = glyph.color;

            glyphColor.a = Mathf.Lerp(
                0.2f,
                1f,
                flicker
            );

            glyph.color = glyphColor;
        }
    }

    private void GenerateLetterTargets()
    {
        letterTargetPositions.Clear();

        int letterCount = selectedWord.Length;

        float totalWidth =
            (letterCount - 1) * letterSpacing;

        float startX = -totalWidth * 0.5f;

        // Prevent horizontal variation from becoming large enough
        // to visually cross into another letter's space.
        float safeHorizontalVariation = Mathf.Min(
            letterHorizontalVariation,
            letterSpacing * 0.25f
        );

        for (int i = 0; i < letterCount; i++)
        {
            float xPosition =
                startX + (i * letterSpacing);

            float offsetX = Random.Range(
                -safeHorizontalVariation,
                safeHorizontalVariation
            );

            float offsetY = Random.Range(
                -letterVerticalVariation,
                letterVerticalVariation
            );

            Vector2 targetPosition = new Vector2(
                xPosition + offsetX,
                offsetY
            );

            letterTargetPositions.Add(targetPosition);
        }

        Debug.Log(
            "Generated " +
            letterTargetPositions.Count +
            " letter targets for: " +
            selectedWord
        );
    }

    private void UpdateGlyphConvergence(float transitionProgress)
    {
        if (letterTargetPositions.Count == 0)
            return;

        float convergenceProgress = Mathf.InverseLerp(
            convergenceStartProgress,
            convergenceEndProgress,
            transitionProgress
        );

        for (int i = 0; i < spawnedGlyphs.Count; i++)
        {
            TextMeshProUGUI glyph = spawnedGlyphs[i];

            if (glyph == null)
                continue;

            // This glyph belongs to a letter
            if (i < letterTargetPositions.Count)
            {
                RectTransform glyphRect = glyph.rectTransform;

                glyphRect.anchoredPosition = Vector2.Lerp(
                    glyphBasePositions[i],
                    letterTargetPositions[i],
                    convergenceProgress
                );

                // Gradually stabilize into the correct letter
                if (convergenceProgress > 0.85f)
                {
                    glyph.text = selectedWord[i].ToString();
                }

                // Fade fully back in
                Color color = glyph.color;
                color.a = Mathf.Lerp(
                    color.a,
                    1f,
                    convergenceProgress
                );

                glyph.color = color;
            }
            else
            {
                // Extra glyphs disappear
                Color color = glyph.color;

                color.a = Mathf.Lerp(
                    color.a,
                    0f,
                    extraGlyphFadeSpeed * Time.deltaTime
                );

                glyph.color = color;
            }
        }
    }

    private void SortGlyphsForConvergence()
    {
        spawnedGlyphs.Sort(
            (a, b) =>
                a.rectTransform.anchoredPosition.x.CompareTo(
                    b.rectTransform.anchoredPosition.x
                )
        );

        glyphBasePositions.Clear();

        foreach (TextMeshProUGUI glyph in spawnedGlyphs)
        {
            glyphBasePositions.Add(
                glyph.rectTransform.anchoredPosition
            );
        }
    }

    public void FadeOutEffect()
    {
        isFadingOut = true;
    }
}