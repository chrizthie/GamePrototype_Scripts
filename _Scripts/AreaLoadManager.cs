using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AreaLoadManager : MonoBehaviour
{
    public static AreaLoadManager Instance { get; private set; }

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private CharacterController characterController;

    private string currentAreaSceneName;
    private bool isLoading;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetCurrentArea(string sceneName)
    {
        currentAreaSceneName = sceneName;
    }

    public void LoadArea(string targetSceneName, string targetSpawnPointName)
    {
        if (isLoading)
            return;

        StartCoroutine(LoadAreaRoutine(targetSceneName, targetSpawnPointName));
    }

    private IEnumerator LoadAreaRoutine(string targetSceneName, string targetSpawnPointName)
    {
        isLoading = true;

        Scene previousScene = default;

        if (!string.IsNullOrEmpty(currentAreaSceneName))
        {
            previousScene = SceneManager.GetSceneByName(currentAreaSceneName);

            if (previousScene.IsValid() && previousScene.isLoaded)
            {
                // Disable old scene environment immediately
                SetEnvironmentEnabled(previousScene, false);
            }
        }

        if (!SceneManager.GetSceneByName(targetSceneName).isLoaded)
        {
            AsyncOperation loadOperation =
                SceneManager.LoadSceneAsync(
                    targetSceneName,
                    LoadSceneMode.Additive);

            while (!loadOperation.isDone)
                yield return null;
        }

        Scene targetScene = SceneManager.GetSceneByName(targetSceneName);

        // Enable target scene environment
        SetEnvironmentEnabled(targetScene, true);

        SceneManager.SetActiveScene(targetScene);

        Debug.Log($"Active Scene: {targetScene.name}");

        // Give HDRP a moment to settle
        yield return null;
        yield return null;

        MovePlayerToSpawn(targetSpawnPointName);

        if (!string.IsNullOrEmpty(currentAreaSceneName) &&
            currentAreaSceneName != targetSceneName)
        {
            AsyncOperation unloadOperation =
                SceneManager.UnloadSceneAsync(currentAreaSceneName);

            while (unloadOperation != null &&
                   !unloadOperation.isDone)
            {
                yield return null;
            }
        }

        currentAreaSceneName = targetSceneName;
        isLoading = false;
    }

    private const string LightingRootTag = "LightingRoot";

    private void SetEnvironmentEnabled(Scene scene, bool enabled)
    {
        if (!scene.IsValid() || !scene.isLoaded)
            return;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.CompareTag(LightingRootTag))
            {
                root.SetActive(enabled);

                Debug.Log(
                    $"Environment {(enabled ? "Enabled" : "Disabled")}: " +
                    $"{root.name} ({scene.name})");
            }
        }
    }

    private void MovePlayerToSpawn(string spawnPointName)
    {
        GameObject spawnPoint = GameObject.Find(spawnPointName);

        if (spawnPoint == null)
        {
            Debug.LogWarning($"Spawn point not found: {spawnPointName}");
            return;
        }

        characterController.enabled = false;

        player.position = spawnPoint.transform.position;
        player.rotation = spawnPoint.transform.rotation;

        characterController.enabled = true;
    }
}