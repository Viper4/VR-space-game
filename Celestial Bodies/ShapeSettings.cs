using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ShapeSettings : ScriptableObject
{
    public const int numSupportedLODS = 24;
    public const int numSupportedMeshSizes = 9;
    public static readonly int[] supportedMeshSizes = { 48, 72, 96, 120, 144, 168, 192, 216, 240 };
    public static int simpleMeshColliderSize = 6;

    [Range(0, numSupportedMeshSizes - 1)]
    public int meshSizeIndex;
    [Range(1, numSupportedLODS)]
    public int levelOfDetail = 1;
    public bool complexMeshCollider = true;
    [ConditionalHide("complexMeshCollider"), Range(0, numSupportedMeshSizes - 1)]
    public int meshColliderSizeIndex;
    public float radius = 1;
    public FilterLayer[] filterLayers;

    [System.Serializable]
    public class FilterLayer
    {
        public bool enabled = true;
        public bool useFirstLayerAsMask;
        public bool applyScale;
        public FilterSettings filterSettings;
    }
}
