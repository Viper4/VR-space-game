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
            if (!inScaledSpace)
                transform.position = _position.ToVector3();
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
    public double worldSpaceThreshold = 5900;
    public double scaledSpaceThreshold = 6100;
    [SerializeField] int worldLayer;
    [SerializeField] int scaledLayer;

    private void Start()
    {
        if (celestialBody.TryGetComponent(out physicsHandler))
            physicsHandler.active = inScaledSpace;
        UpdateTransform();
    }

    /*private void FixedUpdate()
    {
        if(!inScaledSpace)
            position = transform.position.ToVector3d();
    }*/

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
            double distanceToOrigin = Vector3d.Distance(_position, origin);
            if (distanceToOrigin < worldSpaceThreshold)
                SwitchToWorldSpace();
        }
        else
        {
            UpdateInWorldSpace();
            double distanceToOrigin = Vector3d.Distance(_position, origin);
            if (distanceToOrigin > scaledSpaceThreshold)
                SwitchToScaledSpace();
        }
    }

    private void UpdateInScaledSpace(Vector3d origin)
    {
        transform.position = new Vector3((float)((_position.x - origin.x) / scaleFactor + origin.x), (float)((_position.y - origin.y) / scaleFactor + origin.y), (float)((_position.z - origin.z) / scaleFactor + origin.z));
        transform.localScale = new Vector3((float)(_scale.x / scaleFactor), (float)(_scale.y / scaleFactor), (float)(_scale.z / scaleFactor));
    }

    private void UpdateInWorldSpace()
    {
        _position = transform.position.ToVector3d();
        _scale = transform.localScale.ToVector3d();
    }

    private void SwitchToScaledSpace()
    {
        gameObject.layer = scaledLayer;
        foreach (Transform child in transform)
        {
            child.gameObject.layer = scaledLayer;
        }
        inScaledSpace = true;
        if (physicsHandler != null)
            physicsHandler.active = true;
    }

    private void SwitchToWorldSpace()
    {
        gameObject.layer = worldLayer;
        foreach (Transform child in transform)
        {
            child.gameObject.layer = worldLayer;
        }
        inScaledSpace = false;
        transform.position = _position.ToVector3();
        transform.localScale = _scale.ToVector3();
        if (physicsHandler != null)
            physicsHandler.active = false;
    }
}
