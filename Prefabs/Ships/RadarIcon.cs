using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RadarIcon : MonoBehaviour
{
    Radar parentRadar;
    int iconID;

    LineRenderer lineRenderer;
    [SerializeField] int maxLinePositions = 1000;
    [SerializeField] float newLinePosThreshold;
    float sqrNewLinePosThreshold;
    List<Vector3> linePositions = new List<Vector3>();
    int positionIndex;

    TextMeshPro text3D;

    [SerializeField] float killTime = 0.5f;
    float killTimer;

    void Start()
    {
        TryGetComponent(out lineRenderer);
        transform.GetChild(0).TryGetComponent(out text3D);
        killTimer = killTime;
        sqrNewLinePosThreshold = newLinePosThreshold * newLinePosThreshold;
    }

    void Update()
    {
        killTimer -= Time.deltaTime;
        if (killTimer <= 0)
        {
            parentRadar.RemoveIcon(iconID);
            Destroy(gameObject);
        }
        text3D.transform.eulerAngles = new Vector3(Camera.main.transform.eulerAngles.x, Camera.main.transform.eulerAngles.y, Camera.main.transform.parent.eulerAngles.z);
    }

    public void CreateIcon(Radar radar, int instanceID, Vector3 position, Quaternion rotation, string text = "")
    {
        parentRadar = radar;
        iconID = instanceID;
        UpdateIcon(position, rotation, text);
    }

    public void UpdateIcon(Vector3 position, Quaternion rotation, string text = "")
    {
        killTimer = killTime;

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
            text3D.color = lineRenderer.sharedMaterial.color;
        }
        transform.position = position;
        transform.rotation = rotation;
    }
}
