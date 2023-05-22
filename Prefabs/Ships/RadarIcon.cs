using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadarIcon : MonoBehaviour
{
    Radar parentRadar;
    Transform radarOrigin;
    int iconID;

    LineRenderer lineRenderer;
    [SerializeField] float killTime = 0.5f;
    float killTimer = 0;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        killTimer = killTime;
    }

    void Update()
    {
        if (lineRenderer)
        {
            lineRenderer.SetPosition(0, radarOrigin.position);
            lineRenderer.SetPosition(1, transform.position);
        }
        killTimer -= Time.deltaTime;
        if (killTimer <= 0)
        {
            parentRadar.RemoveIcon(iconID);
            Destroy(gameObject);
        }
    }

    public void CreateIcon(Radar radar, Transform origin, int instanceID, Vector3 position, Quaternion rotation)
    {
        parentRadar = radar;
        radarOrigin = origin;
        iconID = instanceID;
        UpdateIcon(position, rotation);
    }

    public void UpdateIcon(Vector3 position, Quaternion rotation)
    {
        killTimer = killTime;
        transform.position = position;
        transform.rotation = rotation;
    }
}
