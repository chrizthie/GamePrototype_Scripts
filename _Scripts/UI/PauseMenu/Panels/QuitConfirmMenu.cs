using UnityEngine;
using UnityEngine.SceneManagement;

public class QuitConfirmMenu : PauseMenu
{
    public void QuitToMainMenu()
    {
        //Time.timeScale = 1f;
        //SceneManager.LoadScene("MainMenu");
        Debug.Log("Quit to Main Menu - Not Implemented");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void Cancel()
    {
        PauseMenuManager.Instance.CloseMenu();
    }
}