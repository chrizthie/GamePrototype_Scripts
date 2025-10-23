using UnityEngine;

public class TerrainChecker
{
    private float[] GetTextureMix(Vector3 playerPos, Terrain terrain)
    {
        Vector3 terrainPos = terrain.transform.position;
        TerrainData terrainData = terrain.terrainData;
        int mapX = Mathf.RoundToInt((playerPos.x - terrainPos.x) / terrainData.size.x * terrainData.alphamapWidth);
        int mapZ = Mathf.RoundToInt((playerPos.z - terrainPos.z) / terrainData.size.z * terrainData.alphamapHeight);
        float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

        float[] cellMix = new float[splatmapData.GetUpperBound(2) + 1];
        for (int i = 0; i < cellMix.Length; i++)
        {
            cellMix[i] = splatmapData[0, 0, i];
        }
        return cellMix;
    }

    public string GetLayerName(Vector3 playerPos, Terrain terrain)
    {
        float[] cellMix = GetTextureMix(playerPos, terrain);
        float strongestMix = 0;
        int strongestIndex = 0;
        for (int i = 0; i < cellMix.Length; i++)
        {
            if (cellMix[i] > strongestMix)
            {
                strongestIndex = i;
                strongestMix = cellMix[i];
            }
        }
        return terrain.terrainData.terrainLayers[strongestIndex].name;
    }
}
