using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialBodyGenerator : MonoBehaviour
{
    [SerializeField] Camera targetCamera;
    public bool autoUpdate = true;
    public enum FaceRenderMask
    {
        All,
        Top,
        Bottom,
        Left,
        Right,
        Front,
        Back
    }
    public FaceRenderMask renderMask;

    private ShapeSettings originalShapeSettings;
    public ShapeSettings shapeSettings;
    private ColorSettings originalColorSettings;
    public ColorSettings colorSettings;
    [SerializeField] bool cloneSettings;

    [HideInInspector] public bool shapeSettingsFoldout;
    [HideInInspector] public bool colorSettingsFoldout;

    ShapeGenerator shapeGenerator;
    ColorGenerator colorGenerator;

    [SerializeField, Range(1, 32)] int rootLOD = 1;
    TerrainChunk[] rootChunks;

    [SerializeField] Vector2 seedRange = new Vector2(-999, 999);

    [SerializeField] bool forceGenerateMeshColliders;

    private void Init()
    {
        shapeGenerator = new ShapeGenerator();
        colorGenerator = new ColorGenerator();
        Material bodyMaterial;
        if (cloneSettings)
        {
            if (originalShapeSettings == null)
            {
                originalShapeSettings = shapeSettings;
                shapeSettings = Instantiate(originalShapeSettings);
            }
            if (originalColorSettings == null)
            {
                originalColorSettings = colorSettings;
                colorSettings = Instantiate(originalColorSettings);
            }
            bodyMaterial = Instantiate(colorSettings.material);
        }
        else
        {
            originalShapeSettings = null;
            originalColorSettings = null;
            bodyMaterial = colorSettings.material;
        }

        foreach(ShapeSettings.FilterLayer filterLayer in shapeSettings.filterLayers)
        {
            if (filterLayer.applyScale)
            {
                filterLayer.filterSettings.simplexNoiseSettings.scale = transform.localScale;
                filterLayer.filterSettings.ridgeNoiseSettings.scale = transform.localScale;
                filterLayer.filterSettings.perlinNoiseSettings.scale = transform.localScale;
                filterLayer.filterSettings.craterSettings.scale = transform.localScale;
            }
        }

        shapeGenerator.UpdateSettings(shapeSettings);
        colorGenerator.UpdateSettings(colorSettings, bodyMaterial);

        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            if (Application.isEditor)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
            else
            {
                Destroy(transform.GetChild(0).gameObject);
            }
        }

        rootChunks = new TerrainChunk[6 * rootLOD * rootLOD];

        Vector3[] directions = new Vector3[] { Vector3.up, Vector3.down, Vector3.right, Vector3.left, Vector3.forward, Vector3.back };

        int arrayIndex = 0;
        for (int i = 0; i < 6; i++)
        {
            for(int r = 0; r < rootLOD; r++)
            {
                for (int c = 0; c < rootLOD; c++)
                {
                    rootChunks[arrayIndex] = new TerrainChunk(shapeGenerator, shapeSettings, directions[i], r, c, rootLOD);
                    if (renderMask == FaceRenderMask.All || (int)renderMask - 1 == i)
                        rootChunks[arrayIndex].GenerateTree(null, transform, colorGenerator);
                    arrayIndex++;
                }
            }

        }
    }

    public void UpdateQuadTrees(Camera camera)
    {
        foreach (TerrainChunk rootChunk in rootChunks)
        {
            rootChunk.UpdateTree(camera, transform, colorGenerator);
        }
    }

    public void GenerateCelestialBody()
    {
        Init();
        GenerateMeshes();
        GenerateColors();
    }

    public void GenerateRandomCelestialBody()
    {
        Init();
        foreach (var noiseLayer in shapeSettings.filterLayers)
        {
            switch (noiseLayer.filterSettings.filterType)
            {
                case FilterSettings.FilterType.Simplex:
                    noiseLayer.filterSettings.simplexNoiseSettings.seed = RandomSeed();
                    break;
                case FilterSettings.FilterType.Ridge:
                    noiseLayer.filterSettings.ridgeNoiseSettings.seed = RandomSeed();
                    break;
                case FilterSettings.FilterType.Perlin:
                    noiseLayer.filterSettings.perlinNoiseSettings.seed = RandomSeed();
                    break;
                case FilterSettings.FilterType.Crater:
                    noiseLayer.filterSettings.craterSettings.seed = RandomSeed();
                    break;
            }
        }
        switch (colorSettings.biomeColorSettings.filter.filterType)
        {
            case FilterSettings.FilterType.Simplex:
                colorSettings.biomeColorSettings.filter.simplexNoiseSettings.seed = RandomSeed();
                break;
            case FilterSettings.FilterType.Ridge:
                colorSettings.biomeColorSettings.filter.ridgeNoiseSettings.seed = RandomSeed();
                break;
            case FilterSettings.FilterType.Perlin:
                colorSettings.biomeColorSettings.filter.perlinNoiseSettings.seed = RandomSeed();
                break;
            case FilterSettings.FilterType.Crater:
                colorSettings.biomeColorSettings.filter.craterSettings.seed = RandomSeed();
                break;
        }
        GenerateMeshes();
        GenerateColors();
    }

    public void GenerateCelestialBody(float radius)
    {
        Init();
        shapeSettings.radius = radius;
        GenerateMeshes();
        GenerateColors();
    }

    public void GenerateRandomCelestialBody(float radius)
    {
        Init();
        shapeSettings.radius = radius;
        foreach (var noiseLayer in shapeSettings.filterLayers)
        {
            switch (noiseLayer.filterSettings.filterType)
            {
                case FilterSettings.FilterType.Simplex:
                    noiseLayer.filterSettings.simplexNoiseSettings.seed = RandomSeed();
                    break;
                case FilterSettings.FilterType.Ridge:
                    noiseLayer.filterSettings.ridgeNoiseSettings.seed = RandomSeed();
                    break;
                case FilterSettings.FilterType.Perlin:
                    noiseLayer.filterSettings.perlinNoiseSettings.seed = RandomSeed();
                    break;
                case FilterSettings.FilterType.Crater:
                    noiseLayer.filterSettings.craterSettings.seed = RandomSeed();
                    break;
            }
        }
        switch (colorSettings.biomeColorSettings.filter.filterType)
        {
            case FilterSettings.FilterType.Simplex:
                colorSettings.biomeColorSettings.filter.simplexNoiseSettings.seed = RandomSeed();
                break;
            case FilterSettings.FilterType.Ridge:
                colorSettings.biomeColorSettings.filter.ridgeNoiseSettings.seed = RandomSeed();
                break;
            case FilterSettings.FilterType.Perlin:
                colorSettings.biomeColorSettings.filter.perlinNoiseSettings.seed = RandomSeed();
                break;
            case FilterSettings.FilterType.Crater:
                colorSettings.biomeColorSettings.filter.craterSettings.seed = RandomSeed();
                break;
        }
        GenerateMeshes();
        GenerateColors();
    }

    private Vector3 RandomSeed()
    {
        return new Vector3(Random.Range(seedRange.x, seedRange.y), Random.Range(seedRange.x, seedRange.y), Random.Range(seedRange.x, seedRange.y));
    }

    public void OnShapeSettingsUpdated()
    {
        if (autoUpdate)
        {
            GenerateCelestialBody(); // Meshes get reset when changing LOD or mesh size
        }
    }

    public void OnColorSettingsUpdated()
    {
        if (autoUpdate)
        {
            Init();
            GenerateColors();
        }
    }

    void GenerateMeshes()
    {
        for (int i = 0; i < 6 * rootLOD * rootLOD; i++)
        {
            int faceIndex = i / (rootLOD * rootLOD);
            if (renderMask == FaceRenderMask.All || (int)renderMask - 1 == faceIndex)
                rootChunks[i].ConstructMesh(null, forceGenerateMeshColliders);
        }
        colorGenerator.UpdateElevation(shapeGenerator.elevationMinMax);
    }

    /* TODO:
     * Add option to apply coloring based on random noise instead of elevation and/or biome
     */
    void GenerateColors()
    {
        colorGenerator.UpdateColors();
        for (int i = 0; i < 6 * rootLOD * rootLOD; i++)
        {
            int faceIndex = i / (rootLOD * rootLOD);
            if (renderMask == FaceRenderMask.All || (int)renderMask - 1 == faceIndex)
                rootChunks[i].UpdateUVs(colorGenerator);
        }
    }
}
