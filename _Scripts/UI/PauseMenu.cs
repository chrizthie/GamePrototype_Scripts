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
    [SerializeField] private GameObject PlayerUI;
    [SerializeField] private GameObject PauseMenuCanvas;


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
    }

    #region Unity Methods

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        PauseMenuCanvas.SetActive(false);
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
