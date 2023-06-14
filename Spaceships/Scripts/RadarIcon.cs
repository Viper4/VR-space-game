using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RadarIcon : MonoBehaviour
{
    Radar parentRadar;
    int ID;

    LineRenderer lineRenderer;
    [SerializeField] int maxLinePositions = 1000;
    [SerializeField] float newLinePosThreshold;
    float sqrNewLinePosThreshold;
    List<Vector3> linePositions = new List<Vector3>();
    int positionIndex;

    public Transform model;
    [SerializeField] TextMeshPro text3D;

    [SerializeField] float killTime = 0.5f;
    float killTimer;

    Color baseColor;

    void LateUpdate()
    {
        killTimer -= Time.deltaTime;
        if (killTimer <= 0)
        {
            parentRadar.RemoveIcon(ID);
            Destroy(gameObject);
        }
        text3D.transform.rotation = Quaternion.LookRotation(text3D.transform.position - FlatCamera.instance.transform.position, FlatCamera.instance.transform.up);
    }

    public void Init(Radar radar, int instanceID, Vector3 position, Quaternion rotation, Color baseColor, Color emission, string text = "")
    {
        this.baseColor = baseColor;
        MeshRenderer modelMeshRenderer = model.GetComponent<MeshRenderer>();
        Material clonedMaterial = Instantiate(modelMeshRenderer.sharedMaterial);
        clonedMaterial.color = baseColor;
        clonedMaterial.SetColor("_EmissionColor", emission);
        modelMeshRenderer.sharedMaterial = clonedMaterial;
        if (TryGetComponent(out lineRenderer))
        {
            lineRenderer.sharedMaterial = clonedMaterial;
        }

        killTimer = killTime;
        sqrNewLinePosThreshold = newLinePosThreshold * newLinePosThreshold;
        parentRadar = radar;
        ID = instanceID;
        UpdateIcon(position, rotation, text);
    }

    public void UpdateIcon(Vector3 position, Quaternion rotation, string text = "")
    {
        killTimer = killTime;

        transform.position = position;
        model.rotation = rotation;
        if (lineRenderer != null)
        {
            if((position - transform.position).sqrMagnitude > sqrNewLinePosThreshold)
            {
                positionIndex++;
                linePositions.Add(position);
                if (positionIndex + 1 > maxLinePositions)
                {
                    linePositions.RemoveAt(0);
                    positionIndex--;
                }
                lineRenderer.SetPositions(linePositions.ToArray());
            }
            else
            {
                lineRenderer.SetPosition(positionIndex, position);
            }
        }

        if (text3D != null)
        {
            text3D.text = text;
            text3D.color = baseColor;
        }
    }
}
