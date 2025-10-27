using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [Header("Pause Menu Flags")]
    [SerializeField] public bool GameIsPaused = false;

    [Header("Required Components")]
    [SerializeField] private PlayerLocomotion playerLocomotion;
    [SerializeField] private GameObject playerUI;
    [SerializeField] private GameObject pauseMenu;


    public void Pause()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        GameIsPaused = true;
        Time.timeScale = 0f;

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
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        GameIsPaused = false;
        Time.timeScale = 1f;

        if (playerLocomotion != null)
        {
            playerLocomotion.enabled = true;
        }

        // enable
        playerUI.SetActive(true);
        // disable
        pauseMenu.SetActive(false);
    }

    #region Unity Methods

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        pauseMenu.SetActive(false);
    }

    private void Update()
    {
        if (InputManager.instance.PauseOpenCloseInput)
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
    }

    #endregion
}
