using UnityEngine;

public class SettingsGraphics : PauseMenu
{
    public void Back()
    {
        PauseMenuManager.Instance.CloseMenu();
    }
}
