using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapLoader : MonoBehaviour
{
    [Header("First Level To Load")]
    [SerializeField] private string firstLevelSceneName = "TestLevel";

    private IEnumerator Start()
    {
        if (SceneManager.GetSceneByName(firstLevelSceneName).isLoaded)
        {
            AreaLoadManager.Instance.SetCurrentArea(firstLevelSceneName);
            yield break;
        }

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(
            firstLevelSceneName,
            LoadSceneMode.Additive
        );

        while (!loadOperation.isDone)
            yield return null;

        Scene loadedScene = SceneManager.GetSceneByName(firstLevelSceneName);
        SceneManager.SetActiveScene(loadedScene);

        AreaLoadManager.Instance.SetCurrentArea(firstLevelSceneName);
    }
}