using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialBody : MonoBehaviour
{
    //private const float G = 6.6743e-11f;

    CelestialBodyGenerator generator;
    [SerializeField] bool autoGenerate = true;

    [SerializeField] float surfaceGravity = 9.807f;
    [SerializeField] float gravityField = 5000;
    float gravityRadius;
    [SerializeField] LayerMask affectedLayers;

    [SerializeField] bool autoOrient = true;
    [SerializeField] float autoOrientSpeed = 5;
    [SerializeField] SphereCollider autoOrientField;

    private void Start()
    {
        generator = GetComponent<CelestialBodyGenerator>();
        if (autoGenerate)
            generator.GenerateCelestialBody();
        gravityRadius = generator.shapeSettings.radius + gravityField;
    }

    private void FixedUpdate()
    {
        Collider[] collidersInGravity = Physics.OverlapSphere(transform.position, gravityRadius, affectedLayers, QueryTriggerInteraction.Ignore);
        foreach(Collider collider in collidersInGravity)
        {
            if (collider.transform != transform && collider.TryGetComponent<Rigidbody>(out var rigidbody) && !rigidbody.isKinematic)
            {
                Vector3 gravityDirection = (transform.position - collider.transform.position).normalized;
                float gravityAcceleration = surfaceGravity * generator.shapeSettings.radius * generator.shapeSettings.radius / CustomExtensions.SqrDistance(transform.position, collider.transform.position);
                rigidbody.AddForce(gravityDirection * gravityAcceleration, ForceMode.Acceleration);
                if(collider.transform.HasTag("AutoOrient") && autoOrientField.bounds.Contains(collider.transform.position))
                {
                    collider.transform.rotation = Quaternion.Slerp(collider.transform.rotation, Quaternion.FromToRotation(-collider.transform.up, gravityDirection) * collider.transform.rotation, autoOrientSpeed * Time.deltaTime);
                }
            }
        }
    }
}
