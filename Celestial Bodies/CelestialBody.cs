using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialBody : MonoBehaviour
{
    private const float v = 4 / 3 * Mathf.PI;

    CelestialBodyGenerator generator;
    Rigidbody _rigidbody;

    [HideInInspector] public bool generationSettingsFoldout;
    public GenerationSettings generationSettings;

    public bool gravity = true;
    [HideInInspector] public bool gravitySettingsFoldout;
    [ConditionalHide("gravity")] public GravitySettings gravitySettings;
    float gravityRadius;
    SphereCollider autoOrientField;

    private void Start()
    {
        generator = GetComponent<CelestialBodyGenerator>();
        float radius = Random.Range(generationSettings.radiusRange.x, generationSettings.radiusRange.y);
        if (generationSettings.autoGenerate)
        {
            if (generationSettings.random)
                generator.GenerateRandomCelestialBody(radius);
            else
                generator.GenerateCelestialBody(radius);
        }
        autoOrientField = GetComponent<SphereCollider>();
        if (gravity)
            gravityRadius = generator.shapeSettings.radius + gravitySettings.gravityField;
        if(TryGetComponent(out _rigidbody) && generationSettings.calculateMass)
        {
            _rigidbody.mass = generationSettings.density * v * radius * radius * radius;
        }
    }

    private void FixedUpdate()
    {
        if (gravity)
        {
            Collider[] collidersInGravity = Physics.OverlapSphere(transform.position, gravityRadius, gravitySettings.affectedLayers, QueryTriggerInteraction.Ignore);
            foreach (Collider collider in collidersInGravity)
            {
                if (collider.transform != transform)
                {
                    if (collider.transform.HasTag("MainCamera"))
                    {
                        generator.CalculateLODs(collider.GetComponent<Camera>());
                    }
                    if (collider.TryGetComponent<Rigidbody>(out var rigidbody) && !rigidbody.isKinematic)
                    {
                        Vector3 gravityDirection = (transform.position - collider.transform.position).normalized;
                        float gravityAcceleration = gravitySettings.surfaceGravity * generator.shapeSettings.radius * generator.shapeSettings.radius / (transform.position - collider.transform.position).sqrMagnitude;
                        rigidbody.AddForce(gravityDirection * gravityAcceleration, ForceMode.Acceleration);
                        if (gravitySettings.autoOrient && collider.transform.HasTag("AutoOrient") && autoOrientField.bounds.Contains(collider.transform.position))
                        {
                            collider.transform.rotation = Quaternion.Slerp(collider.transform.rotation, Quaternion.FromToRotation(-collider.transform.up, gravityDirection) * collider.transform.rotation, gravitySettings.autoOrientSpeed * Time.deltaTime);
                        }
                    }
                }
            }
        }
    }
}
