﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class MapGenerator : MonoBehaviour {

    public enum DrawMode
    {
        NoiseMap,
        ColorMap,
        Mesh,
        FalloffMap
    }
    public DrawMode drawMode;

    [Range(0, 6)]
    public int levelOfDetail;
    public float noiseScale;

    public int octaves;

    [Range(0, 1)]
    public float persistence;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public bool autoUpdate;

    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    // Falloff
    public bool useFalloff;
    float[,] falloffMap;

    int mapChunkSize = 241;

    public TerrainType[] regions;

    private bool isHost = false;
    private GameObject[] playersGO;

    private void Awake()
    {
        playersGO = GameObject.FindGameObjectsWithTag("Player");

        if (NetworkManager.singleton.playerPrefab.GetComponent<PlayerSetup>().isServer)
            isHost = true;

        //mapChunkSize = 241;
        //MapData.mapChunkSize = mapChunkSize;

        falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
        if (isHost)
        {
            seed = Random.Range(int.MinValue, int.MaxValue);
            NetworkManager.singleton.playerPrefab.GetComponent<PlayerSetup>().mapSeed = seed;
        }
        else
        {
            for (int i = 0; i < playersGO.Length; ++i)
            {
                if (playersGO[i].GetComponent<PlayerSetup>().isServer)
                {
                    seed = playersGO[i].GetComponent<PlayerSetup>().mapSeed;
                    break;
                }
            }
        }

        MapData.seed = seed;
        GenerateMap();

        MapData.regions = this.regions;
    }

    public void GenerateMap()
    {
        //float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, noiseScale);
        //float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistence, lacunarity, offset);
        Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistence, lacunarity, offset);
        Color[] colorMap = new Color[mapChunkSize * mapChunkSize]; // color map for the different regions

        for (int y = 0; y < mapChunkSize; ++y) {
            for (int x = 0; x < mapChunkSize; ++x) {
                if (useFalloff) {
                    Noise.noiseMap[x, y] = Mathf.Clamp01(Noise.noiseMap[x, y] - falloffMap[x, y]);
                }
                float currentHeight = Noise.noiseMap[x, y];
                for (int i = 0; i < regions.Length; ++i)
                {
                    if (currentHeight <= regions[i].height) {
                        colorMap[y * mapChunkSize + x] = regions[i].color;
                        break;
                    }
                }
            }
        }

        MapDisplay display = FindObjectOfType<MapDisplay>();

        if (drawMode == DrawMode.NoiseMap)
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(Noise.noiseMap));
        else if (drawMode == DrawMode.ColorMap)
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
        else if (drawMode == DrawMode.Mesh)
        {
            Texture2D tex = TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize);
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(Noise.noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), tex);
            // save the texture
            GetTerrainTexture.SetTexture2D(tex);
        }
        else if (drawMode == DrawMode.FalloffMap)
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapChunkSize)));
    }

    void OnValidate() {
        if (lacunarity < 1)
            lacunarity = 1;

        if (octaves < 0)
            octaves = 0;

        falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
    }

}

[System.Serializable]
public struct TerrainType {
    public string name;
    public float height;
    public Color color;
}