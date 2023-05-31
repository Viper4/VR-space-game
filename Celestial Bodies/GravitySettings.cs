using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class GravitySettings : ScriptableObject
{
    public float surfaceGravity = 9.807f;
    public float gravityField = 5000;
    public LayerMask affectedLayers;
    public bool autoOrient = true;
    public float autoOrientSpeed = 5;
}
