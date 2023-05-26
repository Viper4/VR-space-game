using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FilterCreator
{
    public static IFilter CreateFilter(FilterSettings settings)
    {
        switch (settings.filterType)
        {
            case FilterSettings.FilterType.Simplex:
                return new SimplexNoiseFilter(settings.simplexNoiseSettings);
            case FilterSettings.FilterType.Ridge:
                return new RidgeNoiseFilter(settings.ridgeNoiseSettings);
            case FilterSettings.FilterType.Perlin:
                return new PerlinNoiseFilter(settings.perlinNoiseSettings);
            case FilterSettings.FilterType.Crater:
                return new CraterFilter(settings.craterSettings);
        }
        return null;
    }
}
