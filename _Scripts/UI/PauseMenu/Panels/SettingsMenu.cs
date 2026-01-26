using UnityEngine;

public class SettingsMenu : PauseMenu
{
    [Header("Sub Menus")]
    [SerializeField] private PauseMenu settingsGraphics;
    [SerializeField] private PauseMenu settingsControls;
    [SerializeField] private PauseMenu settingsAudio;

    public void SettingsGraphics()
    {
        PauseMenuManager.Instance.OpenMenu(settingsGraphics);
    }

    public void SettingsControls()
    {
        PauseMenuManager.Instance.OpenMenu(settingsControls);
    }

    public void SettingsAudio()
    {
        PauseMenuManager.Instance.OpenMenu(settingsAudio);
    }

    public void Back()
    {
        PauseMenuManager.Instance.CloseMenu();
    }
}
