using UnityEngine;

public class SettingsControls : PauseMenu
{
    public void Back()
    {
        PauseMenuManager.Instance.CloseMenu();
    }
}
