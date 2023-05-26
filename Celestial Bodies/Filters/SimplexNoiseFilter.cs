using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplexNoiseFilter : IFilter
{
    Noise noise = new Noise();
    FilterSettings.SimplexNoiseSettings settings;

    public SimplexNoiseFilter(FilterSettings.SimplexNoiseSettings settings)
    {
        this.settings = settings;
    }

    public float Evaluate(Vector3 point)
    {
        float noiseValue = 0;
        float frequency = settings.baseRoughness;
        float amplitude = 1;

        for(int i = 0; i < settings.layers; i++)
        {
            float value = noise.Evaluate(point * frequency + settings.seed);
            noiseValue += (value + 1) * 0.5f * amplitude;
            frequency *= settings.roughness;
            amplitude *= settings.persistence;
        }
        noiseValue = Mathf.Max(0, noiseValue - settings.minValue);
        return noiseValue * settings.strength;
    }
}
