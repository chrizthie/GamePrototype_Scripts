using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager Instance;

    [SerializeField] private PauseMenu mainPauseMenu;

    private Stack<PauseMenu> menuStack = new Stack<PauseMenu>();

    [Header("State")]
    [SerializeField] private bool GameIsPaused;

    [Header("Audio Related Components")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private float sfxFadeDuration = 0.15f;
    private Coroutine sfxFadeRoutine;

    [Header("Required Components")]
    [SerializeField] private Canvas playerHUDCanvas;



    public void OpenMenu(PauseMenu menu)
    {
        PauseGame();

        if (menuStack.Count > 0)
        {
            PauseMenu top = menuStack.Peek();

            // Only hide previous menu if the new one is NOT modal
            if (!menu.IsModal)
            {
                top.OnClose();
            }
        }

        menuStack.Push(menu);
        menu.OnOpen();
    }

    public void CloseMenu()
    {
        if (menuStack.Count == 0)
        {
            return;
        }

        menuStack.Pop().OnClose();

        if (menuStack.Count > 0)
        {
            menuStack.Peek().OnOpen();
        }
        else
        {
            ResumeGame();
        }
    }

    private void PauseGame()
    {
        GameIsPaused = true;
        playerHUDCanvas.enabled = false;
        Time.timeScale = 0f;
        GamePause.SetPaused(true);
        FadeSFX(-80f); // effectively silent
    }

    private void ResumeGame()
    {
        GameIsPaused = false;
        playerHUDCanvas.enabled = true;
        Time.timeScale = 1f;
        GamePause.SetPaused(false);
        FadeSFX(0f); // normal volume
    }

    #region Audio related functions and routines

    private void FadeSFX(float targetVolume)
    {
        if (sfxFadeRoutine != null)StopCoroutine(sfxFadeRoutine);

        sfxFadeRoutine = StartCoroutine(FadeSFXRoutine(targetVolume));
    }

    private IEnumerator FadeSFXRoutine(float targetVolume)
    {
        audioMixer.GetFloat("sfxHandle", out float currentVolume);

        float time = 0f;
        while (time < sfxFadeDuration)
        {
            time += Time.unscaledDeltaTime;
            float v = Mathf.Lerp(currentVolume, targetVolume, time / sfxFadeDuration);
            audioMixer.SetFloat("sfxHandle", v);
            yield return null;
        }

        audioMixer.SetFloat("sfxHandle", targetVolume);
    }

    #endregion

    #region Unity Methods

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (InputManager.instance.PauseOpenCloseInput)
        {
            if (menuStack.Count == 0)
            {
                OpenMenu(mainPauseMenu);
            }
            else
            {
                CloseMenu();
            }
        }

        if (GameIsPaused)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    #endregion
}

