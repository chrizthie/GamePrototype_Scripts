using System.Collections.Generic;
using UnityEngine;

public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager Instance;

    [SerializeField] private PauseMenu mainPauseMenu;

    private Stack<PauseMenu> menuStack = new Stack<PauseMenu>();

    [Header("State")]
    [SerializeField] private bool GameIsPaused;

    [Header("Required Components")]
    [SerializeField] private Canvas playerHUDCanvas;

    public void OpenMenu(PauseMenu menu)
    {
        if (menuStack.Count == 0)
        {
            PauseGame();
        }

        if (menuStack.Count > 0)
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

    private void PauseGame()
    {
        GameIsPaused = true;
        
        playerHUDCanvas.enabled = false;
        Time.timeScale = 0f;
    }

    private void ResumeGame()
    {
        GameIsPaused = false;
        
        playerHUDCanvas.enabled = true;
        Time.timeScale = 1f;
    }

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

