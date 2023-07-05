using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceStuff;
using System;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsHandler : MonoBehaviour
{
    public static float speedLimit = 1000;

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
            if (value)
            {
                velocity = attachedRigidbody.velocity.ToVector3d();
                angularVelocity = attachedRigidbody.angularVelocity.ToVector3d();
                attachedRigidbody.isKinematic = true;
            }
            else
            {
                attachedRigidbody.isKinematic = false;
                attachedRigidbody.velocity = velocity.ToVector3();
                attachedRigidbody.angularVelocity = angularVelocity.ToVector3();
            }
        }
    }

    [SerializeField] bool _isKinematic;
    public bool isKinematic
    {
        get
        {
            return _isKinematic;
        }
        set
        {
            _isKinematic = value;
            attachedRigidbody.isKinematic = value;
        }
    }

    ScaledTransform scaledTransform;
    public Rigidbody attachedRigidbody;
    [SerializeField] float lowerVelocityThreshold = -1;
    [SerializeField] float upperVelocityThreshold = -1;

    public Vector3d velocity;
    public Vector3d angularVelocity;

    void Awake()
    {
        TryGetComponent(out scaledTransform);
        attachedRigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (!_isKinematic)
        {
            if (_active)
            {
                if (scaledTransform != null)
                    scaledTransform.position += velocity * Time.fixedDeltaTime;
                else
                    transform.position += velocity.ToVector3() * Time.fixedDeltaTime;
                transform.eulerAngles += angularVelocity.ToVector3() * Time.fixedDeltaTime;

                if (lowerVelocityThreshold != -1)
                {
                    if (Math.Abs(velocity.x) < lowerVelocityThreshold || Math.Abs(velocity.y) < lowerVelocityThreshold || Math.Abs(velocity.z) < lowerVelocityThreshold)
                    {
                        active = false;
                    }
                }
            }
            else
            {
                Vector3 rigidbodyVelocity = attachedRigidbody.velocity;
                velocity = rigidbodyVelocity.ToVector3d();
                if (upperVelocityThreshold != -1)
                {
                    angularVelocity = attachedRigidbody.angularVelocity.ToVector3d();
                    if (Math.Abs(rigidbodyVelocity.x) > upperVelocityThreshold || Math.Abs(rigidbodyVelocity.y) > upperVelocityThreshold || Math.Abs(rigidbodyVelocity.z) > upperVelocityThreshold)
                    {
                        active = true;
                    }
                }
            }
        }
    }

    public void AddForce(Vector3d force, ForceMode forceMode)
    {
        if (!_isKinematic)
        {
            if (_active)
            {
                Vector3d newVelocity = forceMode switch
                {
                    ForceMode.Impulse => velocity + (force / attachedRigidbody.mass),
                    ForceMode.VelocityChange => velocity + force,
                    ForceMode.Acceleration => velocity + (force * Time.fixedDeltaTime),
                    _ => velocity + (force * Time.fixedDeltaTime / attachedRigidbody.mass),
                };
                if (newVelocity.magnitude < speedLimit)
                    velocity = newVelocity;
            }
            else
            {
                attachedRigidbody.AddForce(force.ToVector3(), forceMode);
            }
        }
    }

    public void AddRelativeForce(Vector3d force, ForceMode forceMode)
    {
        if (!_isKinematic)
        {
            if (_active)
            {
                double globalX = transform.right.x * force.x + transform.up.x * force.y + transform.forward.x * force.z;
                double globalY = transform.right.y * force.x + transform.up.y * force.y + transform.forward.y * force.z;
                double globalZ = transform.right.z * force.x + transform.up.z * force.y + transform.forward.z * force.z;
                AddForce(new Vector3d(globalX, globalY, globalZ), forceMode);
            }
            else
            {
                attachedRigidbody.AddRelativeForce(force.ToVector3(), forceMode);
            }
        }
    }

    public void AddTorque(Vector3d torque, ForceMode forceMode)
    {
        if (!_isKinematic)
        {
            if (_active)
            {
                Vector3d newVelocity = forceMode switch
                {
                    ForceMode.Impulse => angularVelocity + (torque / attachedRigidbody.mass),
                    ForceMode.VelocityChange => angularVelocity + torque,
                    ForceMode.Acceleration => angularVelocity + (torque * Time.fixedDeltaTime),
                    _ => angularVelocity + (torque * Time.fixedDeltaTime / attachedRigidbody.mass),
                };
                if (newVelocity.magnitude < speedLimit)
                    angularVelocity = newVelocity;
            }
            else
            {
                attachedRigidbody.AddTorque(torque.ToVector3(), forceMode);
            }
        }
    }

    public void AddRelativeTorque(Vector3d torque, ForceMode forceMode)
    {
        if (!_isKinematic)
        {
            if (_active)
            {
                double globalX = transform.right.x * torque.x + transform.up.x * torque.y + transform.forward.x * torque.z;
                double globalY = transform.right.y * torque.x + transform.up.y * torque.y + transform.forward.y * torque.z;
                double globalZ = transform.right.z * torque.x + transform.up.z * torque.y + transform.forward.z * torque.z;
                AddTorque(new Vector3d(globalX, globalY, globalZ), forceMode);
            }
            else
            {
                attachedRigidbody.AddRelativeTorque(torque.ToVector3(), forceMode);
            }
        }
    }
}
