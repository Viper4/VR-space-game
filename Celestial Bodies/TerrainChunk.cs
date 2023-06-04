using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk
{
    TerrainChunk[] children;
    ShapeGenerator shapeGenerator;
    Vector3 localPosition;
    float width;
    int maxLOD;
    int detailLevel;
    int filterResolution;
    int colliderResolution;
    Vector3 localUp;
    Vector3 localRight;
    Vector3 localForward;
    MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public TerrainChunk(ShapeGenerator shapeGenerator, ShapeSettings settings, Vector3 localUp)
    {
        this.shapeGenerator = shapeGenerator;
        filterResolution = ShapeSettings.supportedMeshSizes[settings.meshSizeIndex];
        colliderResolution = settings.complexMeshCollider ? ShapeSettings.supportedMeshSizes[settings.meshColliderSizeIndex] : ShapeSettings.simpleMeshColliderSize;
        localPosition = localUp;
        width = settings.radius;
        maxLOD = settings.levelOfDetail;
        detailLevel = 0;
        this.localUp = localUp;
        localRight = new Vector3(localUp.y, localUp.z, localUp.x);
        localForward = Vector3.Cross(localUp, localRight);
    }

    public TerrainChunk(ShapeGenerator shapeGenerator, Vector3 localPosition, float width, int filterResolution, int colliderResolution, int maxLOD, int detailLevel, Vector3 localUp, Vector3 localRight, Vector3 localForward)
    {
        this.shapeGenerator = shapeGenerator;
        this.localPosition = localPosition;
        this.width = width;
        this.filterResolution = filterResolution;
        this.colliderResolution = colliderResolution;
        this.maxLOD = maxLOD;
        this.detailLevel = detailLevel;
        this.localUp = localUp;
        this.localRight = localRight;
        this.localForward = localForward;
    }

    private Vector3 GetPointOnCubeSphere(Vector2 percent)
    {
        /*
        Vector3 pointOnUnitCube = position + (2 * (percent.x - 0.5f) * localRight + 2 * (percent.y - 0.5f) * localForward) * offset;
        float x2 = pointOnUnitCube.x * pointOnUnitCube.x;
        float y2 = pointOnUnitCube.y * pointOnUnitCube.y;
        float z2 = pointOnUnitCube.z * pointOnUnitCube.z;
        Vector3 pointOnUnitSphere;
        pointOnUnitSphere.x = pointOnUnitCube.x * Mathf.Sqrt(1f - y2 / 2f - z2 / 2f + y2 * z2 / 3f);
        pointOnUnitSphere.y = pointOnUnitCube.y * Mathf.Sqrt(1f - x2 / 2f - z2 / 2f + x2 * z2 / 3f);
        pointOnUnitSphere.z = pointOnUnitCube.z * Mathf.Sqrt(1f - x2 / 2f - y2 / 2f + x2 * y2 / 3f);
        return pointOnUnitSphere;
        */

        Vector3 pointOnUnitCube = localPosition + (2f * (percent.x - 0.5f) * localRight + 2f * (percent.y - 0.5f) * localForward);
        return pointOnUnitCube.normalized;
    }

    /* TODO:
     * Add option to apply coloring based on random noise instead of elevation and/or biome
     */
    public void ConstructMesh(Camera camera = null, bool forceMeshCollider = false)
    {
        Mesh filterMesh = meshFilter.sharedMesh;
        Vector3[] filterVertices = new Vector3[filterResolution * filterResolution]; // resolution vertices on each side of the mesh
        int[] filterTriangles = new int[(filterResolution - 1) * (filterResolution - 1) * 6]; // 2 triangles per square so (resolution - 1)^2 faces * 2 tris * 3 vertices per tri
        int filterTriangleIndex = 0;
        Vector2[] uv = filterMesh.uv;

        Vector3[] colliderVertices = new Vector3[colliderResolution * colliderResolution];
        int[] colliderTriangles = new int[(colliderResolution - 1) * (colliderResolution - 1) * 6];
        int colliderTriangleIndex = 0;

        bool generateCollider = forceMeshCollider || (camera != null && meshRenderer.bounds.SqrDistance(camera.transform.position) < 100);
        int i = 0;
        int j = 0;
        for (int y = 0; y < filterResolution; y++)
        {
            for (int x = 0; x < filterResolution; x++)
            {
                Vector3 pointOnUnitSphere = GetPointOnCubeSphere(new Vector2(x, y) / (filterResolution - 1));
                filterVertices[i] = shapeGenerator.CalculatePointOnSphere(pointOnUnitSphere);

                if (x != filterResolution - 1 && y != filterResolution - 1)
                {
                    filterTriangles[filterTriangleIndex] = i;
                    filterTriangles[filterTriangleIndex + 1] = i + filterResolution + 1;
                    filterTriangles[filterTriangleIndex + 2] = i + filterResolution;

                    filterTriangles[filterTriangleIndex + 3] = i;
                    filterTriangles[filterTriangleIndex + 4] = i + 1;
                    filterTriangles[filterTriangleIndex + 5] = i + filterResolution + 1;
                    filterTriangleIndex += 6;
                }

                if (generateCollider && y < colliderResolution && x < colliderResolution)
                {
                    colliderVertices[j] = shapeGenerator.CalculatePointOnSphere(GetPointOnCubeSphere(new Vector2(x, y) / (colliderResolution - 1)));

                    if (x != colliderResolution - 1 && y != colliderResolution - 1)
                    {
                        colliderTriangles[colliderTriangleIndex] = j;
                        colliderTriangles[colliderTriangleIndex + 1] = j + colliderResolution + 1;
                        colliderTriangles[colliderTriangleIndex + 2] = j + colliderResolution;

                        colliderTriangles[colliderTriangleIndex + 3] = j;
                        colliderTriangles[colliderTriangleIndex + 4] = j + 1;
                        colliderTriangles[colliderTriangleIndex + 5] = j + colliderResolution + 1;
                        colliderTriangleIndex += 6;
                    }
                    j++;
                }
                i++;
            }
        }

        filterMesh.Clear();
        filterMesh.vertices = filterVertices;
        filterMesh.triangles = filterTriangles;
        filterMesh.RecalculateNormals();
        if (filterMesh.uv.Length == uv.Length)
            filterMesh.uv = uv; 

        if (generateCollider)
        {
            MeshCollider meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = new Mesh();
            meshCollider.sharedMesh.Clear();
            meshCollider.sharedMesh.vertices = colliderVertices;
            meshCollider.sharedMesh.triangles = colliderTriangles;
            meshCollider.sharedMesh.RecalculateNormals();
            meshCollider.convex = true;
        }
    }

    public void UpdateUVs(ColorGenerator colorGenerator)
    {
        Vector2[] uv = new Vector2[filterResolution * filterResolution];
        int i = 0;
        for (int y = 0; y < filterResolution; y++)
        {
            for (int x = 0; x < filterResolution; x++)
            {
                uv[i] = new Vector2(colorGenerator.BiomePercentFromPoint(GetPointOnCubeSphere(new Vector2(x, y) / (filterResolution - 1))), 0);

                i++;
            }
        }
        meshFilter.sharedMesh.uv = uv;
    }

    public void GenerateTree(Camera camera, Transform parent, ColorGenerator colorGenerator)
    {
        if (camera != null && detailLevel >= 0 && detailLevel < maxLOD && (parent.TransformPoint(shapeGenerator.CalculatePointOnSphere(GetPointOnCubeSphere(new Vector2(0.5f, 0.5f)))) - camera.transform.position).sqrMagnitude < width * width + 1000)
        {
            if(meshRenderer != null)
            {
                meshRenderer.enabled = false;
            }
            children = new TerrainChunk[4];
            float nextWidth = width * 0.5f;
            Vector3 nextLocalRight = localRight * 0.5f;
            Vector3 nextLocalForward = localForward * 0.5f;
            children[0] = new TerrainChunk(shapeGenerator, localPosition + nextLocalRight - nextLocalForward, nextWidth, filterResolution, colliderResolution, maxLOD, detailLevel + 1, localUp, nextLocalRight, nextLocalForward); // Top left
            children[1] = new TerrainChunk(shapeGenerator, localPosition + nextLocalRight + nextLocalForward, nextWidth, filterResolution, colliderResolution, maxLOD, detailLevel + 1, localUp, nextLocalRight, nextLocalForward); // Top right
            children[2] = new TerrainChunk(shapeGenerator, localPosition - nextLocalRight + nextLocalForward, nextWidth, filterResolution, colliderResolution, maxLOD, detailLevel + 1, localUp, nextLocalRight, nextLocalForward); // Bottom right
            children[3] = new TerrainChunk(shapeGenerator, localPosition - nextLocalRight - nextLocalForward, nextWidth, filterResolution, colliderResolution, maxLOD, detailLevel + 1, localUp, nextLocalRight, nextLocalForward); // Bottom left
            foreach (TerrainChunk chunk in children)
            {
                chunk.GenerateTree(camera, parent, colorGenerator);
            }
        }
        else
        {
            if(meshRenderer == null)
            {
                GameObject meshGO = new GameObject("Mesh (" + detailLevel + " LOD)");
                meshGO.transform.SetParent(parent, false);
                meshRenderer = meshGO.AddComponent<MeshRenderer>();
                meshFilter = meshGO.AddComponent<MeshFilter>();
            }

            meshRenderer.sharedMaterial = colorGenerator.materialInstance;
            meshFilter.sharedMesh = new Mesh();
            if (camera != null)
            {
                ConstructMesh(camera);
                UpdateUVs(colorGenerator);
            }
        }
    }

    private bool IsVisibleFrom(Camera camera)
    {
        if (meshRenderer.enabled)
        {
            return meshRenderer.isVisible;
        }
        else
        {
            Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
            return GeometryUtility.TestPlanesAABB(frustumPlanes, meshRenderer.bounds);
        }
    }
}
