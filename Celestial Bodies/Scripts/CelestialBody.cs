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
    Vector3 scale = Vector3.zero;
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
            if(!generationSettings.random)
                scale = Vector3.one;

            if (generationSettings.autoGenerate)
            {
                if (generationSettings.random)
                    generator.GenerateRandomCelestialBody();
                else
                    generator.GenerateCelestialBody();
            }
        }
        else
        {
            if (!generationSettings.random)
                scale = transform.localScale;
        }

        if (generationSettings.random)
        {
            float randomX;
            float randomY;
            float randomZ;
            if (generationSettings.sphere)
            {
                randomX = randomY = randomZ = Random.Range(generationSettings.scaleRange[0].x, generationSettings.scaleRange[0].x);
            }
            else
            {
                randomX = Random.Range(generationSettings.scaleRange[0].x, generationSettings.scaleRange[1].x);
                randomY = Random.Range(generationSettings.scaleRange[0].y, generationSettings.scaleRange[1].y);
                randomZ = Random.Range(generationSettings.scaleRange[0].z, generationSettings.scaleRange[1].z);
            }
            scale = new Vector3(randomX, randomY, randomZ);
        }

        surfaceGravity = Random.Range(gravitySettings.surfaceGravityRange.x, gravitySettings.surfaceGravityRange.y);
        if (scaledTransform.inScaledSpace)
            scaledTransform.scale = scale.ToVector3d();
        else
            transform.localScale = scale;

        if (TryGetComponent(out sphereField))
        {
            sphereField.radius += generationSettings.sphereField;
            float worldRadius = sphereField.radius * transform.localScale.x;
            sqrFieldRadius = worldRadius * worldRadius;
        }
        if (gravity)
        {
            gravityRadius = scale.x * gravitySettings.gravityRadiusMultiplier;
        }
        if (TryGetComponent(out _rigidbody))
        {
            TryGetComponent(out physicsHandler);
            if(generationSettings.calculateMass)
                _rigidbody.mass = generationSettings.density * v * scale.x * scale.x * scale.x;
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
            spaceLight.Init(scale.x);
        }
    }

    public bool Initialized()
    {
        return scale != Vector3.zero;
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
        return surfaceGravity * scale.x * scale.x / (scaledTransform.position - point).sqrMagnitude;
    }
}
