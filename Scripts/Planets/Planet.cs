using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    [Range(2, 256)]
    [SerializeField] int resolution = 8;

    ShapeGenerator shapeGenerator;

    public bool autoUpdate = true;
    public ShapeSettings shapeSettings;
    public ColorSettings colorSettings;

    [HideInInspector] public bool shapeSettingsFoldout;
    [HideInInspector] public bool colorSettingsFoldout;

    [SerializeField, HideInInspector] MeshFilter[] meshFilters;
    TerrainFace[] terrainFaces;

    private void Init()
    {
        shapeGenerator = new ShapeGenerator(shapeSettings);
        if(meshFilters == null || meshFilters.Length == 0)
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

                meshObject.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                meshFilters[i] = meshObject.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
            }

            terrainFaces[i] = new TerrainFace(shapeGenerator, meshFilters[i].sharedMesh, resolution, directions[i]);
        }
    }

    public void GeneratePlanet()
    {
        Init();
        GenerateMeshes();
        GenerateColors();
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
        foreach(TerrainFace face in terrainFaces)
        {
            face.ConstructMesh();
        }
    }

    void GenerateColors()
    {
        foreach(MeshFilter meshFilter in meshFilters)
        {
            meshFilter.GetComponent<MeshRenderer>().sharedMaterial.color = colorSettings.planetColor;
        }
    }
}
