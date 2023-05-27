using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ShapeSettings : ScriptableObject
{
    public const int numSupportedLODS = 6;
    public const int numSupportedMeshSizes = 9;
    public static readonly int[] supportedMeshSizes = { 48, 72, 96, 120, 144, 168, 192, 216, 240 };

    [Range(0, numSupportedMeshSizes - 1)]
    public int meshSizeIndex;
    [Range(1, numSupportedLODS)]
    public int levelOfDetail = 1;
    public float radius = 1;
    public NoiseLayer[] noiseLayers;

    [System.Serializable]
    public class NoiseLayer
    {
        public bool enabled = true;
        public bool useFirstLayerAsMask;
        public FilterSettings noiseSettings;
    }
}
