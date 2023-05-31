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
    [SerializeField] bool cloneSettings;

    [HideInInspector] public bool shapeSettingsFoldout;
    [HideInInspector] public bool colorSettingsFoldout;

    ShapeGenerator shapeGenerator;
    ColorGenerator colorGenerator;

    [SerializeField, HideInInspector] MeshFilter[] meshFilters;
    MeshCollider[] meshColliders;
    TerrainFace[] terrainFaces;

    [SerializeField] Vector2 seedRange = new Vector2(-999, 999);

    [SerializeField] bool generateMeshColliders;

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
            if (generateMeshColliders)
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

                        if (generateMeshColliders)
                        {
                            meshColliders[index] = meshObject.AddComponent<MeshCollider>();
                            meshColliders[index].sharedMesh = new Mesh();
                        }
                    }
                    meshFilters[index].GetComponent<MeshRenderer>().sharedMaterial = bodyMaterial;
                    terrainFaces[index] = generateMeshColliders ? new TerrainFace(shapeGenerator, shapeSettings, meshFilters[index].sharedMesh, meshColliders[index].sharedMesh, r, c, directions[i]) : new TerrainFace(shapeGenerator, shapeSettings, meshFilters[index].sharedMesh, null, r, c, directions[i]);

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
        for (int i = 0; i < meshFilters.Length; i++)
        {
            if (meshFilters[i].gameObject.activeSelf)
            {
                terrainFaces[i].ConstructMesh();
                if(generateMeshColliders)
                    meshColliders[i].convex = true;
            }
        }
        colorGenerator.UpdateElevation(shapeGenerator.elevationMinMax);
    }

    /* TODO:
     * Add option to apply coloring based on random noise instead of elevation
     */
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

    /* TODO:
     * Split up meshes more when closer to the camera 
     * Maybe only activate mesh colliders directly adjacent to the mesh the camera is on
     */
    public void CalculateLODs(Camera camera)
    {
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
        for (int i = 0; i < meshFilters.Length; i++)
        {
            if (!GeometryUtility.TestPlanesAABB(frustumPlanes, meshFilters[i].mesh.bounds))
            {
                meshFilters[i].gameObject.SetActive(false);
            }
            else
            {

            }
        }
    }
}
