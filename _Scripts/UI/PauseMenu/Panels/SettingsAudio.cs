using UnityEngine;

public class SettingsAudio : PauseMenu
{
    public void Back()
    {
        PauseMenuManager.Instance.CloseMenu();
    }
}
