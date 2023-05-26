using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialBodyGenerator : MonoBehaviour
{
    [Range(2, 256)]
    [SerializeField] int resolution = 8;
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

    [HideInInspector] public bool shapeSettingsFoldout;
    [HideInInspector] public bool colorSettingsFoldout;

    ShapeGenerator shapeGenerator;
    ColorGenerator colorGenerator;

    [SerializeField, HideInInspector] MeshFilter[] meshFilters;
    TerrainFace[] terrainFaces;

    [SerializeField] Vector2 seedRange = new Vector2(-999, 999);
    [SerializeField] bool cloneSettings;

    private void Init()
    {
        shapeGenerator = new ShapeGenerator();
        colorGenerator = new ColorGenerator();
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
        }
        else
        {
            originalShapeSettings = null;
            originalColorSettings = null;
        }
        if(colorSettings.bodyMaterial == null)
        {
            colorSettings.bodyMaterial = new Material(colorSettings.shader);
        }
        Material materialInstance = Instantiate(colorSettings.bodyMaterial);
        shapeGenerator.UpdateSettings(shapeSettings);
        colorGenerator.UpdateSettings(colorSettings, materialInstance);

        if (meshFilters == null || meshFilters.Length == 0)
        {
            meshFilters = new MeshFilter[6];
        }
        terrainFaces = new TerrainFace[6];

        Vector3[] directions = new Vector3[] { Vector3.up, Vector3.down, Vector3.right, Vector3.left, Vector3.forward, Vector3.back };

        for (int i = 0; i < 6; i++)
        {
            if (meshFilters[i] == null)
            {
                GameObject meshObject = new GameObject("Mesh");
                meshObject.transform.SetParent(transform, false);

                meshObject.AddComponent<MeshRenderer>();
                meshFilters[i] = meshObject.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
            }
            meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial = materialInstance;

            terrainFaces[i] = new TerrainFace(shapeGenerator, meshFilters[i].sharedMesh, resolution, directions[i]);
            bool renderFace = renderMask == FaceRenderMask.All || (int)renderMask - 1 == i;
            meshFilters[i].gameObject.SetActive(renderFace);
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
        foreach (var noiseLayer in shapeSettings.noiseLayers)
        {
            switch (noiseLayer.noiseSettings.filterType)
            {
                case FilterSettings.FilterType.Simplex:
                    noiseLayer.noiseSettings.simplexNoiseSettings.seed = RandomSeed();
                    break;
                case FilterSettings.FilterType.Ridge:
                    noiseLayer.noiseSettings.ridgeNoiseSettings.seed = RandomSeed();
                    break;
                case FilterSettings.FilterType.Perlin:
                    noiseLayer.noiseSettings.perlinNoiseSettings.seed = RandomSeed();
                    break;
                case FilterSettings.FilterType.Crater:
                    noiseLayer.noiseSettings.craterSettings.seed = RandomSeed();
                    break;
            }
        }
        switch (colorSettings.biomeColorSettings.noise.filterType)
        {
            case FilterSettings.FilterType.Simplex:
                colorSettings.biomeColorSettings.noise.simplexNoiseSettings.seed = RandomSeed();
                break;
            case FilterSettings.FilterType.Ridge:
                colorSettings.biomeColorSettings.noise.ridgeNoiseSettings.seed = RandomSeed();
                break;
            case FilterSettings.FilterType.Perlin:
                colorSettings.biomeColorSettings.noise.perlinNoiseSettings.seed = RandomSeed();
                break;
            case FilterSettings.FilterType.Crater:
                colorSettings.biomeColorSettings.noise.craterSettings.seed = RandomSeed();
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
            Init();
            GenerateMeshes();
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
        for (int i = 0; i < 6; i++)
        {
            if (meshFilters[i].gameObject.activeSelf)
            {
                terrainFaces[i].ConstructMesh();
            }
        }
        colorGenerator.UpdateElevation(shapeGenerator.elevationMinMax);
    }

    void GenerateColors()
    {
        colorGenerator.UpdateColors();
        for (int i = 0; i < 6; i++)
        {
            if (meshFilters[i].gameObject.activeSelf)
            {
                terrainFaces[i].UpdateUVs(colorGenerator);
            }
        }
    }
}
