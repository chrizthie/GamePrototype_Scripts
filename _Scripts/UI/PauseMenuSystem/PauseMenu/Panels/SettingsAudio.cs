using UnityEngine;
using UnityEngine.EventSystems;

public class SettingsAudio : PauseMenu
{
    public void Back()
    {
        PauseMenuManager.Instance.CloseMenu();
    }
}
