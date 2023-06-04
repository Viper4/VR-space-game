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
    float surfaceGravity;
    SphereCollider sphereField;
    float sqrFieldRadius;

    private void Start()
    {
        float radius = generationSettings.random ? Random.Range(generationSettings.radiusRange.x, generationSettings.radiusRange.y) : generator.shapeSettings.radius;
        if (TryGetComponent(out generator))
        {
            if (generationSettings.autoGenerate)
            {
                if (generationSettings.random)
                    generator.GenerateRandomCelestialBody(radius);
                else
                    generator.GenerateCelestialBody(radius);
            }
        }
        else
        {
            transform.localScale = new Vector3(radius, radius, radius);
        }
        surfaceGravity = Random.Range(gravitySettings.surfaceGravityRange.x, gravitySettings.surfaceGravityRange.y);

        if (TryGetComponent(out sphereField))
        {
            sphereField.radius = radius + generationSettings.sphereField;
            sqrFieldRadius = radius * radius;
        }
        if (gravity)
            gravityRadius = radius * gravitySettings.gravityRadiusMultiplier;
        if (TryGetComponent(out _rigidbody) && generationSettings.calculateMass)
        {
            _rigidbody.mass = generationSettings.density * v * radius * radius * radius;
        }
    }

    private void FixedUpdate()
    {
        if (gravity)
        {
            // NonAlloc generates no garbage, research this more since we could go past 50000 colliders and might want to use OverlapSphere
            Collider[] collidersInGravity = new Collider[50000];
            int numColliders = Physics.OverlapSphereNonAlloc(transform.position, gravityRadius, collidersInGravity, gravitySettings.affectedLayers);
            for (int i = 0; i < numColliders; i++)
            {
                Transform transformInGravity = collidersInGravity[i].transform;
                if (transformInGravity != transform)
                {
                    if (transformInGravity.HasTag("AutoOrient"))
                    {
                        generator.GenerateQuadTrees(transformInGravity.GetComponent<Camera>());
                    }
                    if (transformInGravity.TryGetComponent<Rigidbody>(out var rigidbody) && !rigidbody.isKinematic)
                    {
                        Vector3 gravityDirection = (transform.position - transformInGravity.position).normalized;
                        float gravityAcceleration = surfaceGravity * generator.shapeSettings.radius * generator.shapeSettings.radius / (transform.position - transformInGravity.position).sqrMagnitude;
                        rigidbody.AddForce(gravityDirection * gravityAcceleration, ForceMode.Acceleration);
                        if (gravitySettings.autoOrient && transformInGravity.HasTag("AutoOrient") && (transformInGravity.position - transform.position).sqrMagnitude < sqrFieldRadius)
                        {
                            transformInGravity.rotation = Quaternion.Slerp(transformInGravity.rotation, Quaternion.FromToRotation(-transformInGravity.up, gravityDirection) * transformInGravity.rotation, gravitySettings.autoOrientSpeed * Time.deltaTime);
                        }
                    }
                }
            }
        }
    }
}
