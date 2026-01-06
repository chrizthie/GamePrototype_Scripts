using UnityEngine;

public class PauseMainMenu : PauseMenu
{
    public PauseMenu settingsMenu;

    public void ResumeGame()
    {
        PauseMenuManager.Instance.CloseMenu();
    }

    public void OpenSettings()
    {
        PauseMenuManager.Instance.OpenMenu(settingsMenu);
    }
}
