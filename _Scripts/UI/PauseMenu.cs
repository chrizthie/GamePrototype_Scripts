using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [Header("Pause Menu Flags")]
    [SerializeField] public bool GameIsPaused = false;
    [SerializeField] public bool SettingsIsOpen = false;

    [Header("Required Components")]
    [SerializeField] private PlayerLocomotion playerLocomotion;
    [SerializeField] private GameObject playerUI;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject settingsMenu;

    public void Pause()
    {
        GameIsPaused = true;
        FreezeState();

        if (playerLocomotion != null)
        {
            playerLocomotion.enabled = false;
        }

        // enable
        pauseMenu.SetActive(true);
        // disable
        playerUI.SetActive(false);
    }

    public void Unpause()
    {
        GameIsPaused = false;
        UnfreezeState();

        if (playerLocomotion != null)
        {
            playerLocomotion.enabled = true;
        }

        // enable
        playerUI.SetActive(true);
        // disable
        pauseMenu.SetActive(false);
    }

    #region Settings

    public void OpenSettings()
    {
        Debug.Log("Open Settings Menu");
        SettingsIsOpen = true;
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(true);
    }

    public void SettingsBack()
    {
        SettingsIsOpen = false;
        settingsMenu.SetActive(false);
        pauseMenu.SetActive(true);
    }

    #endregion

    #region Quit

    public void OpenQuit()
    {
        Debug.Log("Open Quit Menu");
    }

    #endregion

    #region Freeze States

    public void FreezeState()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        Time.timeScale = 0f;
    }

    public void UnfreezeState()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1f;
    }


    #endregion

    #region Unity Methods

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        pauseMenu.SetActive(false);
        settingsMenu.SetActive(false);
    }

    private void Update()
    {
        if (InputManager.instance.PauseOpenCloseInput && !SettingsIsOpen)
        {
            if (!GameIsPaused)
            {
                Pause();
            }
            else
            {
                Unpause();
            }
        }

        if (SettingsIsOpen)
        {
            if (InputManager.instance.PauseOpenCloseInput)
            {
                SettingsBack();
            }
        }
    }

    #endregion
}
