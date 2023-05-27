using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialBodyGenerator : MonoBehaviour
{
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
    MeshCollider[] meshColliders;
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
        if (colorSettings.bodyMaterial == null)
        {
            colorSettings.bodyMaterial = new Material(colorSettings.shader);
        }
        Material materialInstance = Instantiate(colorSettings.bodyMaterial);
        shapeGenerator.UpdateSettings(shapeSettings);
        colorGenerator.UpdateSettings(colorSettings, materialInstance);

        int numMeshes = 6 * shapeSettings.levelOfDetail * shapeSettings.levelOfDetail;
        if (meshFilters == null || meshFilters.Length != numMeshes || meshColliders == null || meshColliders.Length != numMeshes)
        {
            for (int i = 0; i < meshFilters.Length; i++)
            {
                if (meshFilters[i] != null)
                {
                    if (Application.isEditor)
                    {
                        DestroyImmediate(meshFilters[i].gameObject);
                    }
                    else
                    {
                        Destroy(meshFilters[i].gameObject);
                    }
                }
            }
            meshFilters = new MeshFilter[numMeshes];
            meshColliders = new MeshCollider[numMeshes];
        }
        terrainFaces = new TerrainFace[numMeshes];

        Vector3[] directions = new Vector3[] { Vector3.up, Vector3.down, Vector3.right, Vector3.left, Vector3.forward, Vector3.back };

        int index = 0;
        for(int i = 0; i < 6; i++)
        {
            for(int r = 0; r < shapeSettings.levelOfDetail; r++)
            {
                for(int c = 0; c < shapeSettings.levelOfDetail; c++)
                {
                    if (meshFilters[index] == null)
                    {
                        GameObject meshObject = new GameObject("Mesh" + i);
                        meshObject.transform.SetParent(transform, false);

                        meshObject.AddComponent<MeshRenderer>();
                        meshFilters[index] = meshObject.AddComponent<MeshFilter>();
                        meshFilters[index].sharedMesh = new Mesh();

                        meshColliders[index] = meshObject.AddComponent<MeshCollider>();
                        meshColliders[index].sharedMesh = new Mesh();
                    }
                    meshFilters[index].GetComponent<MeshRenderer>().sharedMaterial = materialInstance;

                    terrainFaces[index] = new TerrainFace(shapeGenerator, shapeSettings, meshFilters[index].sharedMesh, meshColliders[index].sharedMesh, r, c, directions[i]);
                    bool renderFace = renderMask == FaceRenderMask.All || (int)renderMask - 1 == i;
                    meshFilters[index].gameObject.SetActive(renderFace);
                    index++;
                }
            }
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
        for (int i = 0; i < meshFilters.Length; i++)
        {
            if (meshFilters[i].gameObject.activeSelf)
            {
                terrainFaces[i].ConstructMesh();
                meshColliders[i].convex = true;
            }
        }
        colorGenerator.UpdateElevation(shapeGenerator.elevationMinMax);
    }

    void GenerateColors()
    {
        colorGenerator.UpdateColors();
        for (int i = 0; i < meshFilters.Length; i++)
        {
            if (meshFilters[i].gameObject.activeSelf)
            {
                terrainFaces[i].UpdateUVs(colorGenerator);
            }
        }
    }
}
