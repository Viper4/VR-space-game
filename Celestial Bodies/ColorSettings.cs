using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ColorSettings : ScriptableObject
{
    public Shader shader;
    [HideInInspector] public Material bodyMaterial;
    public BiomeColorSettings biomeColorSettings;

    [System.Serializable]
    public class BiomeColorSettings
    {
        public Biome[] biomes;
        public FilterSettings noise;
        public float noiseOffset;
        public float noiseStrength;
        [Range(0, 1)] public float blend;

        [System.Serializable]
        public class Biome
        {
            public Gradient gradient;
            public Color tint;
            [Range(0, 1)] public float startHeight;
            [Range(0, 1)] public float tintPercent;
            public float weightMultiplier = 1;
        }
    }
}
