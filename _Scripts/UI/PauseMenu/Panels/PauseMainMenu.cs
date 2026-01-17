using UnityEngine;

public class PauseMainMenu : PauseMenu
{
    [Header("Sub Menus")]
    [SerializeField] private PauseMenu settingsMenu;
    [SerializeField] private PauseMenu quitConfirmMenu;

    public void ResumeGame()
    {
        PauseMenuManager.Instance.CloseMenu();
    }

    public void OpenSettings()
    {
        PauseMenuManager.Instance.OpenMenu(settingsMenu);
    }

    public void OpenQuitConfirm()
    {
        PauseMenuManager.Instance.OpenMenu(quitConfirmMenu);
    }
}
