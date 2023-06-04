using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class GenerationSettings : ScriptableObject
{
    public bool autoGenerate = true;
    public bool random = true;
    [ConditionalHide("random")] public Vector2 radiusRange;
    public float sphereField = 100;
    public bool calculateMass = true;
    [ConditionalHide("calculateMass")] public float density = 50; // In kg / m^3
}
