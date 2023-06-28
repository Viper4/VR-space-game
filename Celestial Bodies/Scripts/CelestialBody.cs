using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceStuff;
using System;
using Random = UnityEngine.Random;

[RequireComponent(typeof(ScaledTransform))]
public class CelestialBody : MonoBehaviour
{
    private const float v = 4 / 3 * Mathf.PI;

    CelestialBodyGenerator generator;
    Rigidbody _rigidbody;
    PhysicsHandler physicsHandler;

    [HideInInspector] public bool generationSettingsFoldout;
    public GenerationSettings generationSettings;

    public bool gravity = true;
    [HideInInspector] public bool gravitySettingsFoldout;
    [ConditionalHide("gravity")] public GravitySettings gravitySettings;
    float radius = -1;
    float gravityRadius;
    float surfaceGravity;
    SphereCollider sphereField;
    float sqrFieldRadius;
    public CelestialBody systemCenter;
    [HideInInspector] public ScaledTransform scaledTransform;

    private void Start()
    {
        scaledTransform = GetComponent<ScaledTransform>();
        if (TryGetComponent(out generator))
        {
            radius = generationSettings.random ? Random.Range(generationSettings.radiusRange.x, generationSettings.radiusRange.y) : generator.shapeSettings.radius;
            if (generationSettings.autoGenerate)
            {
                if (generationSettings.random)
                    generator.GenerateRandomCelestialBody(1f);
                else
                    generator.GenerateCelestialBody(1f);
            }
        }
        else
        {
            if (generationSettings.random)
            {
                radius = Random.Range(generationSettings.radiusRange.x, generationSettings.radiusRange.y);
            }
            else
            {
                radius = transform.localScale.x;
            }
        }
        surfaceGravity = Random.Range(gravitySettings.surfaceGravityRange.x, gravitySettings.surfaceGravityRange.y);
        scaledTransform.scale = new Vector3d(radius, radius, radius);

        if (TryGetComponent(out sphereField))
        {
            sphereField.radius += generationSettings.sphereField;
            float worldRadius = sphereField.radius * transform.localScale.x;
            sqrFieldRadius = worldRadius * worldRadius;
        }
        if (gravity)
        {
            gravityRadius = radius * gravitySettings.gravityRadiusMultiplier;
        }
        if (TryGetComponent(out _rigidbody))
        {
            TryGetComponent(out physicsHandler);
            if(generationSettings.calculateMass)
                _rigidbody.mass = generationSettings.density * v * radius * radius * radius;
            if(systemCenter != null)
            {
                StartCoroutine(SetOrbitalVelocity());
            }
            Vector3 min = generationSettings.initialAngularVelocityRange[0];
            Vector3 max = generationSettings.initialAngularVelocityRange[1];

            _rigidbody.angularVelocity = new Vector3(Random.Range(min.x, max.x), Random.Range(min.y, max.y), Random.Range(min.z, max.z));
        }
        if(TryGetComponent<SpaceLight>(out var spaceLight))
        {
            spaceLight.Init(radius);
        }
    }

    public bool Initialized()
    {
        return radius != -1;
    }

    /* shipMass * g = (shipMass * v^2) / distance
     * v = sqrt(distance * g)
     */
    IEnumerator SetOrbitalVelocity()
    {
        yield return new WaitUntil(() => systemCenter.Initialized());

        Vector3d toCenter = systemCenter.scaledTransform.position - scaledTransform.position;
        Vector3d perpendicular = Vector3d.Cross(toCenter, systemCenter.transform.up.ToVector3d()).normalized;
        double g = systemCenter.CalculateGravityAcceleration(scaledTransform.position);
        double distance = Vector3d.Distance(scaledTransform.position, systemCenter.scaledTransform.position);
        Vector3d orbitVelocity = Math.Sqrt(distance * g) * perpendicular;

        if (physicsHandler != null && physicsHandler.active)
        {
            physicsHandler.velocity = orbitVelocity;
        }
        else
        {
            _rigidbody.velocity = orbitVelocity.ToVector3();
        }
    }

    private void FixedUpdate()
    {
        if (gravity || !generationSettings.simple)
        {
            // NonAlloc generates no garbage, research this more
            Collider[] collidersInGravity = Physics.OverlapSphere(transform.position, gravityRadius, gravitySettings.affectedLayers);
            foreach (Collider colliderInGravity in collidersInGravity)
            {
                Transform transformInGravity = colliderInGravity.transform;
                if (transformInGravity != transform)
                {
                    if (!scaledTransform.inScaledSpace && !generationSettings.simple && transformInGravity.HasTag("MainCamera"))
                    {
                        generator.UpdateQuadTrees(transformInGravity.GetComponent<Camera>());
                    }
                    if (gravity)
                    {
                        Vector3 gravityDirection = (scaledTransform.position.ToVector3() - transformInGravity.position).normalized;
                        Vector3d realPosition = transformInGravity.TryGetComponent<ScaledTransform>(out var otherScaledTransform) ? otherScaledTransform.position : transformInGravity.position.ToVector3d();
                        if (transformInGravity.TryGetComponent<PhysicsHandler>(out var otherPhysicsHandler))
                        {
                            if(!otherPhysicsHandler.isKinematic)
                                otherPhysicsHandler.AddForce(gravityDirection.ToVector3d() * CalculateGravityAcceleration(realPosition), ForceMode.Acceleration);
                        }
                        else if (transformInGravity.TryGetComponent<Rigidbody>(out var otherRigidbody))
                        {
                            if(!otherRigidbody.isKinematic)
                                otherRigidbody.AddForce(gravityDirection * (float)CalculateGravityAcceleration(realPosition), ForceMode.Acceleration);
                        }

                        if (gravitySettings.autoOrient && transformInGravity.HasTag("AutoOrient") && (realPosition - scaledTransform.position).sqrMagnitude < sqrFieldRadius)
                        {
                            transformInGravity.rotation = Quaternion.Slerp(transformInGravity.rotation, Quaternion.FromToRotation(-transformInGravity.up, gravityDirection) * transformInGravity.rotation, gravitySettings.autoOrientSpeed * Time.fixedDeltaTime);
                        }
                    }
                }
            }
        }
    }

    
    /* surfaceGravity = bodyMass / radius^2
     * bodyMass = surfaceGravity * radius^2
     * shipMass * g = (bodyMass * shipMass) / distance^2
     * g = (surfaceGravity * radius^2) / distance^2
     */
    public double CalculateGravityAcceleration(Vector3d point)
    {
        return surfaceGravity * radius * radius / (scaledTransform.position - point).sqrMagnitude;
    }
}
