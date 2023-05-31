using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainFace
{
    ShapeGenerator shapeGenerator;
    Mesh filterMesh;
    Mesh colliderMesh;
    int filterResolution;
    int colliderResolution;
    int numMeshesPerSide;
    int row;
    int col;
    Vector3 localUp;
    Vector3 localRight;
    Vector3 localForward;

    public TerrainFace(ShapeGenerator shapeGenerator, ShapeSettings settings, Mesh filterMesh, Mesh colliderMesh, int row, int col, Vector3 localUp)
    {
        this.shapeGenerator = shapeGenerator;
        this.filterMesh = filterMesh;
        this.colliderMesh = colliderMesh;
        this.filterResolution = ShapeSettings.supportedMeshSizes[settings.meshSizeIndex];
        this.colliderResolution = settings.complexMeshCollider ? ShapeSettings.supportedMeshSizes[settings.meshColliderSizeIndex] : ShapeSettings.simpleMeshColliderSize;
        this.row = row;
        this.col = col;
        numMeshesPerSide = settings.levelOfDetail;
        this.localUp = localUp;
        localRight = new Vector3(localUp.y, localUp.z, localUp.x);
        localForward = Vector3.Cross(localUp, localRight);
    }

    public Vector3 GetPointOnCubeSphere(Vector3 origin, Vector3 percent)
    {
        Vector3 pointOnUnitCube = origin + (2 * percent.x / numMeshesPerSide * localRight + 2 * percent.y / numMeshesPerSide * localForward);
        float x2 = pointOnUnitCube.x * pointOnUnitCube.x;
        float y2 = pointOnUnitCube.y * pointOnUnitCube.y;
        float z2 = pointOnUnitCube.z * pointOnUnitCube.z;
        Vector3 pointOnUnitSphere;
        pointOnUnitSphere.x = pointOnUnitCube.x * Mathf.Sqrt(1f - y2 / 2f - z2 / 2f + y2 * z2 / 3f);
        pointOnUnitSphere.y = pointOnUnitCube.y * Mathf.Sqrt(1f - x2 / 2f - z2 / 2f + x2 * z2 / 3f);
        pointOnUnitSphere.z = pointOnUnitCube.z * Mathf.Sqrt(1f - x2 / 2f - y2 / 2f + x2 * y2 / 3f);
        return pointOnUnitSphere;
    }

    public void ConstructMesh()
    {
        Vector3[] filterVertices = new Vector3[filterResolution * filterResolution]; // resolution vertices on each side of the mesh
        int[] filterTriangles = new int[(filterResolution - 1) * (filterResolution - 1) * 6]; // 2 triangles per sqaure so (resolution - 1)^2 faces * 2 tris * 3 vertices per tri
        int filterTriangleIndex = 0;
        Vector2[] uv = filterMesh.uv;


        Vector3[] colliderVertices = new Vector3[colliderResolution * colliderResolution];
        int[] colliderTriangles = new int[(colliderResolution - 1) * (colliderResolution - 1) * 6];
        int colliderTriangleIndex = 0;

        Vector3 startVertex = localUp + ((2f * row / numMeshesPerSide) - 1f) * localRight + ((2f * col / numMeshesPerSide) - 1f) * localForward;
        int i = 0;
        int j = 0;
        for (int y = 0; y < filterResolution; y++)
        {
            for (int x = 0; x < filterResolution; x++)
            {
                filterVertices[i] = shapeGenerator.CalculatePointOnSphere(GetPointOnCubeSphere(startVertex, new Vector2(x, y) / (filterResolution - 1)));

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

                if (colliderMesh != null)
                {
                    if (y < colliderResolution && x < colliderResolution)
                    {
                        colliderVertices[j] = shapeGenerator.CalculatePointOnSphere(GetPointOnCubeSphere(startVertex, new Vector2(x, y) / (colliderResolution - 1)));

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
                }
                i++;
            }
        }

        filterMesh.Clear();
        filterMesh.vertices = filterVertices;
        filterMesh.triangles = filterTriangles;
        filterMesh.RecalculateNormals();
        if(filterMesh.uv.Length == uv.Length)
            filterMesh.uv = uv;

        if(colliderMesh != null)
        {
            colliderMesh.Clear();
            colliderMesh.vertices = colliderVertices;
            colliderMesh.triangles = colliderTriangles;
            colliderMesh.RecalculateNormals();
        }
    }

    public void UpdateUVs(ColorGenerator colorGenerator)
    {
        Vector2[] uv = new Vector2[filterResolution * filterResolution];
        Vector3 startVertex = localUp + ((2f * row / numMeshesPerSide) - 1f) * localRight + ((2f * col / numMeshesPerSide) - 1f) * localForward;
        int i = 0;
        for (int y = 0; y < filterResolution; y++)
        {
            for (int x = 0; x < filterResolution; x++)
            {
                Vector2 percent = new Vector2(x, y) / (filterResolution - 1);
                Vector3 pointOnUnitSphere = GetPointOnCubeSphere(startVertex, percent);

                uv[i] = new Vector2(colorGenerator.BiomePercentFromPoint(pointOnUnitSphere), 0);

                i++;
            }
        }
        filterMesh.uv = uv;
    }
}
