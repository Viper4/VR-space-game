using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceStuff;
using UnityEngine.Animations;

public class ScaledTransform : MonoBehaviour
{
    public bool inScaledSpace = true;
    [SerializeField] private Vector3d _position;
    public Vector3d position 
    { 
        get
        {
            return _position;
        }
        set
        {
            _position = value;
            UpdateTransform();
        }
    }

    [SerializeField] private Vector3d _scale;
    public Vector3d scale
    {
        get
        {
            return _scale;
        }
        set
        {
            _scale = value;
            UpdateTransform();
        }
    }
    [SerializeField] TransformChange floatingOrigin;
    [SerializeField] CelestialBody celestialBody;
    PhysicsHandler physicsHandler;
    public double scaleFactor = 1000;
    public double worldSpaceThreshold = 6100;
    public double scaledSpaceThreshold = 5900;
    [SerializeField] int worldLayer;
    [SerializeField] int scaledLayer;

    private void Start()
    {
        if (celestialBody.TryGetComponent(out physicsHandler))
            physicsHandler.active = inScaledSpace;
    }

    private void FixedUpdate()
    {
        if(!inScaledSpace)
            position = transform.position.ToVector3d();
    }

    private void OnValidate()
    {
        if(floatingOrigin != null)
            UpdateTransform();
    }

    private void OnEnable()
    {
        floatingOrigin = Camera.main.transform.GetComponent<TransformChange>();
        floatingOrigin.PositionChangeEvent += UpdateTransform;
    }

    private void OnDisable()
    {
        floatingOrigin.PositionChangeEvent -= UpdateTransform;
    }

    private void UpdateTransform()
    {
        Vector3d origin = floatingOrigin.transform.position.ToVector3d();
        if (inScaledSpace)
        {
            UpdateInScaledSpace(origin);
            double distanceToOrigin = Vector3d.Distance(position, origin);
            if (distanceToOrigin < worldSpaceThreshold)
                SwitchToWorldSpace();
        }
        else
        {
            UpdateInWorldSpace();
            double distanceToOrigin = Vector3d.Distance(position, origin);
            if (distanceToOrigin > scaledSpaceThreshold)
                SwitchToScaledSpace(origin);
        }
    }

    private void UpdateInScaledSpace(Vector3d origin)
    {
        transform.position = new Vector3((float)((position.x - origin.x) / scaleFactor + origin.x), (float)((position.y - origin.y) / scaleFactor + origin.y), (float)((position.z - origin.z) / scaleFactor + origin.z));
        transform.localScale = new Vector3((float)(scale.x / scaleFactor), (float)(scale.y / scaleFactor), (float)(scale.z / scaleFactor));
    }

    private void UpdateInWorldSpace()
    {
        transform.position = position.ToVector3();
        transform.localScale = scale.ToVector3();
    }

    private void SwitchToScaledSpace(Vector3d origin)
    {
        gameObject.layer = scaledLayer;
        inScaledSpace = true;
        UpdateInScaledSpace(origin);
        if(physicsHandler != null)
            physicsHandler.active = true;
    }

    private void SwitchToWorldSpace()
    {
        gameObject.layer = worldLayer;
        inScaledSpace = false;
        UpdateInWorldSpace();
        if (physicsHandler != null)
            physicsHandler.active = false;
    }
}
