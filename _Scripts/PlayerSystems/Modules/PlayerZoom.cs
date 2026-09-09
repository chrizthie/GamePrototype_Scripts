using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class PlayerZoom : MonoBehaviour
{
    [Header("Required Components")]
    [SerializeField] private CinemachineCamera playerCamera;
    [SerializeField] private PlayerLocomotionPreset preset;

    [Header("Vignette Settings")]
    [SerializeField] private float normalVignette = 0.32f;
    [SerializeField] private float zoomVignette = 0.37f;

    private float targetFOV;

    private Volume activeVolume;
    private Vignette vignette;

    private void Awake()
    {
        FindActiveVolume();
    }

    private void Update()
    {
        // Re-find the Volume if we changed scenes
        if (activeVolume == null || vignette == null)
        {
            FindActiveVolume();
        }

        bool isRunning = InputManager.instance.CurrentInput.run;
        bool isZooming = InputManager.instance.CurrentInput.zoom;

        // Running overrides zoom
        if (isRunning)
        {
            isZooming = false;
        }

        // FOV
        targetFOV = isZooming
            ? preset.cameraZoomFOV
            : preset.cameraWalkFOV;

        playerCamera.Lens.FieldOfView = Mathf.Lerp(
            playerCamera.Lens.FieldOfView,
            targetFOV,
            preset.cameraFOVChangeSpeed * Time.deltaTime
        );

        // Vignette
        if (vignette != null)
        {
            float targetVignette = isZooming
                ? zoomVignette
                : normalVignette;

            vignette.intensity.value = Mathf.Lerp(
                vignette.intensity.value,
                targetVignette,
                preset.cameraFOVChangeSpeed * Time.deltaTime
            );
        }
    }

    private void FindActiveVolume()
    {
        Volume[] volumes = FindObjectsByType<Volume>(
            FindObjectsSortMode.None
        );

        foreach (Volume volume in volumes)
        {
            if (volume.isGlobal)
            {
                activeVolume = volume;

                if (activeVolume.profile.TryGet(out vignette))
                {
                    Debug.Log("PlayerZoom: Global Volume found.");
                }
                else
                {
                    Debug.LogWarning(
                        "PlayerZoom: Global Volume found, but it has no Vignette override."
                    );
                }

                return;
            }
        }

        Debug.LogWarning("PlayerZoom: No Global Volume found.");
    }
}