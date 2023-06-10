using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceStuff;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsHandler : MonoBehaviour
{
    private bool _active;
    public bool active 
    {
        get
        {
            return _active;
        }
        set
        {
            _active = value;
            attachedRigidbody.isKinematic = value;
        }
    }
    ScaledTransform scaledTransform;
    Rigidbody attachedRigidbody;
    public Vector3d velocity;

    void Start()
    {
        TryGetComponent(out scaledTransform);
        attachedRigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (_active)
        {
            if (scaledTransform != null)
            {
                scaledTransform.position += velocity * Time.fixedDeltaTime;
            }
            else
            {
                transform.position += velocity.ToVector3() * Time.fixedDeltaTime;
            }
        }
    }

    public void AddForce(Vector3d force, ForceMode forceMode)
    {
        switch (forceMode)
        {
            case ForceMode.Force:
                velocity += force * Time.fixedDeltaTime / attachedRigidbody.mass;
                break;
            case ForceMode.Impulse:
                velocity += force / attachedRigidbody.mass;
                break;
            case ForceMode.VelocityChange:
                velocity += force;
                break;
            case ForceMode.Acceleration:
                velocity += force * Time.fixedDeltaTime;
                break;
        }
    }
}
