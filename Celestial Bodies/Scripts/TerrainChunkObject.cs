using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TerrainChunkObject : MonoBehaviour
{
    TerrainChunk chunk;
    public MeshFilter meshFilter;
    public MeshCollider meshCollider;
    public MeshRenderer meshRenderer;
    BoxCollider boxCollider;
    int collidersInTrigger = 0;
    TransformChange targetTransformChange;

    public void Init(TerrainChunk chunk)
    {
        this.chunk = chunk;
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
    }

    private bool IsVisibleFrom(Camera camera)
    {
        if (meshRenderer.enabled)
        {
            return meshRenderer.isVisible;
        }
        else
        {
            foreach(Camera cameraInStack in camera.GetUniversalAdditionalCameraData().cameraStack)
            {
                Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(cameraInStack);
                if (GeometryUtility.TestPlanesAABB(frustumPlanes, meshRenderer.bounds))
                    return true;
            }
            return false;
        }
    }

    // Box collider detects colliders entering the chunk and generates/activates the mesh collider
    public void UpdateBoxCollider()
    {
        if(boxCollider == null)
            boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.bounds.Encapsulate(meshRenderer.bounds);
        boxCollider.size *= 1.1f;
        boxCollider.isTrigger = true;
    }

    private void UpdateMeshRenderer()
    {
        meshRenderer.enabled = IsVisibleFrom(Camera.main);
    }

    private void OnEnable()
    {
        targetTransformChange = Camera.main.transform.GetComponent<TransformChange>();
        targetTransformChange.TransformChangeEvent += UpdateMeshRenderer;
    }

    private void OnDisable()
    {
        targetTransformChange.TransformChangeEvent -= UpdateMeshRenderer;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collidersInTrigger == 0)
        {
            if(meshCollider != null)
            {
                meshCollider.enabled = true;
            }
            else
            {
                chunk.ConstructMeshCollider();
            }
        }
        collidersInTrigger++;
    }

    private void OnTriggerExit(Collider other)
    {
        collidersInTrigger--;
        if (collidersInTrigger < 0)
            collidersInTrigger = 0;
        if (collidersInTrigger == 0 && meshCollider != null)
        {
            meshCollider.enabled = false;
        }
    }
}
