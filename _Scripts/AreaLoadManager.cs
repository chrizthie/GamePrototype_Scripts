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

    private void DisableDirectionalLightShadowsInScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return;

        Scene scene = SceneManager.GetSceneByName(sceneName);

        if (!scene.isLoaded)
            return;

        GameObject[] rootObjects = scene.GetRootGameObjects();

        foreach (GameObject root in rootObjects)
        {
            Light[] lights = root.GetComponentsInChildren<Light>(true);

            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    light.shadows = LightShadows.None;
                }
            }
        }
    }

    public void SetCurrentArea(string sceneName)
    {
        currentAreaSceneName = sceneName;
    }

    public void LoadArea(string targetSceneName, string targetSpawnPointName)
    {
        if (isLoading) return;

        StartCoroutine(LoadAreaRoutine(targetSceneName, targetSpawnPointName));
    }

    private IEnumerator LoadAreaRoutine(string targetSceneName, string targetSpawnPointName)
    {
        isLoading = true;

        DisableDirectionalLightShadowsInScene(currentAreaSceneName);

        if (!SceneManager.GetSceneByName(targetSceneName).isLoaded)
        {
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(
                targetSceneName,
                LoadSceneMode.Additive
            );

            while (!loadOperation.isDone)
                yield return null;
        }

        Scene targetScene = SceneManager.GetSceneByName(targetSceneName);
        SceneManager.SetActiveScene(targetScene);

        MovePlayerToSpawn(targetSpawnPointName);

        if (!string.IsNullOrEmpty(currentAreaSceneName) && currentAreaSceneName != targetSceneName)
        {
            AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(currentAreaSceneName);

            while (unloadOperation != null && !unloadOperation.isDone)
                yield return null;
        }

        currentAreaSceneName = targetSceneName;
        isLoading = false;
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