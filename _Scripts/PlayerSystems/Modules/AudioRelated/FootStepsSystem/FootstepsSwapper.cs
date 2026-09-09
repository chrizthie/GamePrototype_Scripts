using System.Collections.Generic;
using UnityEngine;

public class FootstepsSwapper : MonoBehaviour
{
    [Header("Footsteps Parameters")]
    [SerializeField] public FootstepsCollection[] footstepsCollections;
    [SerializeField] private string currentTerrainLayer;
    [SerializeField] private Dictionary<string, FootstepsCollection> layerFootstepMap = new Dictionary<string, FootstepsCollection>();
    [SerializeField] private TerrainChecker terrainChecker;

    [Header("Required Components")]
    [SerializeField] private FootstepsHandler footstepsHandler;
    
    public void CheckLayers()
    {
        RaycastHit hit;
        Vector3 start = transform.position + Vector3.up * 1f;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 3))
        {
            if (hit.transform.GetComponent<Terrain>() != null)
            {
                Terrain terrain = hit.transform.GetComponent<Terrain>();
                string newTerrainLayer = terrainChecker.GetLayerName(transform.position, terrain);

                if (currentTerrainLayer != newTerrainLayer)
                {
                    currentTerrainLayer = newTerrainLayer;

                    // Try to find a matching footsteps collection
                    if (layerFootstepMap.TryGetValue(currentTerrainLayer, out FootstepsCollection collection))
                    {
                        footstepsHandler.SwapFootsteps(collection);
                    }
                    else
                    {
                        Debug.LogWarning($"No footsteps collection found for layer: {currentTerrainLayer}");
                    }
                }
            }
            if (hit.transform.GetComponent<SurfaceType>() != null)
            {
                FootstepsCollection collection = hit.transform.GetComponent<SurfaceType>().footstepsCollection;
                currentTerrainLayer = collection.name;
                footstepsHandler.SwapFootsteps(collection);
            }
        }
    }

    public void UpdateFootstepType(bool isRunning)
    {
        if (currentTerrainLayer != null && layerFootstepMap.TryGetValue(currentTerrainLayer, out FootstepsCollection collection))
        {
            footstepsHandler.SwapFootsteps(collection);
        }
    }

    #region Unity Methods

    private void OnValidate()
    {
        if (footstepsHandler == null)
        {
            footstepsHandler = GetComponent<FootstepsHandler>();
        }
    }

    private void Awake()
    {
        // Example mapping: assign terrain layer names to specific collections
        layerFootstepMap.Add("0_LushGrass", footstepsCollections[0]);
        layerFootstepMap.Add("1_DryGrass", footstepsCollections[0]);
        layerFootstepMap.Add("2_Moss", footstepsCollections[0]);
        layerFootstepMap.Add("3_Dirt", footstepsCollections[1]);
        layerFootstepMap.Add("4_PebblesDirt", footstepsCollections[1]);
        layerFootstepMap.Add("5_PebblesSmall", footstepsCollections[2]);
        layerFootstepMap.Add("6_PebblesLarge", footstepsCollections[3]);
        layerFootstepMap.Add("7_Rock", footstepsCollections[3]);
    }

    private void Start()
    {
        terrainChecker = new TerrainChecker();
    }
    #endregion
}
