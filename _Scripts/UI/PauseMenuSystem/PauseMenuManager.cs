using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager Instance;

    [SerializeField] private PauseMenu mainPauseMenu;

    private Stack<PauseMenu> menuStack = new Stack<PauseMenu>();
    private InputAction _cancelAction;

    [Header("State")]
    [SerializeField] private bool GameIsPaused;

    [Header("Audio Related Components")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private float sfxFadeDuration = 0.15f;
    private Coroutine sfxFadeRoutine;


    [Header("Required Components")]
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private InputManager inputManager;
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

    private void OnPause(InputAction.CallbackContext ctx)
    {
        if (menuStack.Count == 0)
            OpenMenu(mainPauseMenu);
        else
            CloseMenu();
    }

    private void OnCancel(InputAction.CallbackContext ctx)
    {
        if (!GamePause.IsPaused)
        {
            return;
        }

        PauseMenuManager.Instance.CloseMenu();
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
        GameIsPaused = GamePause.IsPaused;
        playerHUDCanvas.enabled = false;
        Time.timeScale = 0f;
        GamePause.SetPaused(true);
        _playerInput.actions.FindActionMap("Gameplay").Disable(); // disable gameplay input map
        _playerInput.actions.FindActionMap("UI").Enable(); // disable gameplay input map
        FadeSFX(-80f); // sound fx muted
    }

    private void ResumeGame()
    {
        GameIsPaused = !GamePause.IsPaused;
        playerHUDCanvas.enabled = true;
        Time.timeScale = 1f;
        GamePause.SetPaused(false);
        _playerInput.actions.FindActionMap("Gameplay").Enable(); // enable gameplay input map
        _playerInput.actions.FindActionMap("UI").Disable(); // disable gameplay input map
        FadeSFX(0f); // sound fx normal volume
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

    private void OnEnable()
    {
        var pauseAction = _playerInput.actions["PauseOpenClose"];
        pauseAction.performed += OnPause;

        _cancelAction = _playerInput.actions["Cancel"];
        _cancelAction.performed += OnCancel;
    }

    private void OnDisable()
    {
        var pauseAction = _playerInput.actions["PauseOpenClose"];
        pauseAction.performed -= OnPause;

        _cancelAction.performed -= OnCancel;
    }

    private void Update()
    {
        if (GamePause.IsPaused)
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

