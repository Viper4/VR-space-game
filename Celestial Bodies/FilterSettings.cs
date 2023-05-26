using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FilterSettings
{
    public enum FilterType
    {
        Simplex,
        Ridge,
        Perlin,
        Crater
    }
    public FilterType filterType;

    [ConditionalHide("filterType", 0)] public SimplexNoiseSettings simplexNoiseSettings;
    [ConditionalHide("filterType", 1)] public RidgeNoiseSettings ridgeNoiseSettings;
    [ConditionalHide("filterType", 2)] public PerlinNoiseSettings perlinNoiseSettings;
    [ConditionalHide("filterType", 3)] public CraterSettings craterSettings;

    public class BaseSettings
    {
        public Vector3 seed;
        public float strength = 1;
    }

    [System.Serializable]
    public class SimplexNoiseSettings : BaseSettings
    {
        [Range(1, 8)]
        public int layers = 1;
        public float baseRoughness = 1;
        public float roughness = 2;
        public float persistence = 0.5f;
        public float minValue;
    }

    [System.Serializable]
    public class RidgeNoiseSettings : SimplexNoiseSettings
    {
        public float weightMultiplier = 1;
    }

    [System.Serializable]
    public class PerlinNoiseSettings : SimplexNoiseSettings
    {
        public float scale = 1;
    }

    [System.Serializable]
    public class CraterSettings : BaseSettings
    {
        int craters;
        [Range(0, 1)]
        float sizeDistribution = 0.5f;
    }
}
