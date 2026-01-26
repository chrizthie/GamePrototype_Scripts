using UnityEngine;
using UnityEngine.EventSystems;

public class SettingsGraphics : PauseMenu
{
    public void Back()
    {
        PauseMenuManager.Instance.CloseMenu();
    }
}
