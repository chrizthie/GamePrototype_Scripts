using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager Instance;

    [Header("Menus")]
    [SerializeField] private PauseMenu mainPauseMenu;
    private readonly Stack<PauseMenu> menuStack = new();

    [Header("State")]
    [SerializeField] private bool gameIsPaused;

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private float sfxFadeDuration = 0.15f;
    private Coroutine sfxFadeRoutine;

    [Header("Required Components")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Canvas playerHUDCanvas;
    [SerializeField] private InputManager inputManager;
    public InputManager InputManager => inputManager;

    private InputAction pauseAction;
    private InputAction cancelAction;

    #region Menu Logic

    public void OpenMenu(PauseMenu menu)
    {
        PauseGame();

        if (menuStack.Count > 0 && !menu.IsModal)
        {
            menuStack.Peek().OnClose();
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

    #endregion

    #region Input Callbacks

    private void OnPause(InputAction.CallbackContext ctx)
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

    private void OnCancel(InputAction.CallbackContext ctx)
    {
        if (!GamePause.IsPaused)
        {
            return;
        }

        CloseMenu();
    }

    #endregion

    #region Game State

    private void PauseGame()
    {
        gameIsPaused = GamePause.IsPaused;

        playerHUDCanvas.enabled = false;
        Time.timeScale = 0f;
        GamePause.SetPaused(true);

        playerInput.actions.FindActionMap("Gameplay").Disable();
        playerInput.actions.FindActionMap("UI").Enable();

        FadeSFX(-80f);
    }

    private void ResumeGame()
    {
        gameIsPaused = !GamePause.IsPaused;

        playerHUDCanvas.enabled = true;
        Time.timeScale = 1f;
        GamePause.SetPaused(false);

        playerInput.actions.FindActionMap("Gameplay").Enable();
        playerInput.actions.FindActionMap("UI").Disable();

        FadeSFX(0f);
    }

    #endregion

    #region Audio

    private void FadeSFX(float targetVolume)
    {
        if (sfxFadeRoutine != null)
        {
            StopCoroutine(sfxFadeRoutine);
        }

        sfxFadeRoutine = StartCoroutine(FadeSFXRoutine(targetVolume));
    }

    private IEnumerator FadeSFXRoutine(float targetVolume)
    {
        audioMixer.GetFloat("sfxHandle", out float startVolume);

        float elapsed = 0f;
        while (elapsed < sfxFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float volume = Mathf.Lerp(startVolume, targetVolume, elapsed / sfxFadeDuration);
            audioMixer.SetFloat("sfxHandle", volume);
            yield return null;
        }

        audioMixer.SetFloat("sfxHandle", targetVolume);
    }

    #endregion

    #region Unity Methods

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        pauseAction = playerInput.actions["PauseOpenClose"];
        cancelAction = playerInput.actions["Cancel"];

        pauseAction.performed += OnPause;
        cancelAction.performed += OnCancel;
    }

    private void OnDisable()
    {
        if (pauseAction != null)
        {
            pauseAction.performed -= OnPause;
        }

        if (cancelAction != null)
        {
            cancelAction.performed -= OnCancel;
        }
    }

    private void Update()
    {
        Cursor.visible = GamePause.IsPaused && !inputManager.isGamepad;
        Cursor.lockState = Cursor.visible ? CursorLockMode.Confined : CursorLockMode.Locked;
    }

    #endregion
}
