using UnityEngine;
using UnityEngine.EventSystems;

public class SettingsControls : PauseMenu
{
    public void Back()
    {
        PauseMenuManager.Instance.CloseMenu();
    }
}
