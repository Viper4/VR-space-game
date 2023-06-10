using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformChange : MonoBehaviour
{
    public delegate void TransformChangeEventHandler();
    public event TransformChangeEventHandler TransformChangeEvent;
    public event TransformChangeEventHandler PositionChangeEvent;
    public event TransformChangeEventHandler RotationChangeEvent;

    [SerializeField] bool trackPosition;
    [SerializeField] bool trackRotation;

    Vector3 lastPosition;
    Quaternion lastRotation;

    public virtual void TransformChanged()
    {
        TransformChangeEvent?.Invoke();
    }

    public virtual void PositionChanged()
    {
        PositionChangeEvent?.Invoke();
    }

    public virtual void RotationChanged()
    {
        RotationChangeEvent?.Invoke();
    }

    void Start()
    {
        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

    void Update()
    {
        bool differentPosition = trackPosition && transform.position != lastPosition;
        bool differentRotation = trackRotation && transform.rotation != lastRotation;
        if(differentPosition || differentRotation)
        {
            TransformChanged();
        }
        if(differentPosition)
        {
            lastPosition = transform.position;
            PositionChanged();
        }
        if(differentRotation)
        {
            lastRotation = transform.rotation;
            RotationChanged();
        }
    }
}
