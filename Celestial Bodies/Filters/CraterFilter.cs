using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraterFilter : IFilter
{
    FilterSettings.CraterSettings settings;

    public CraterFilter(FilterSettings.CraterSettings settings)
    {
        this.settings = settings;
    }

    public float Evaluate(Vector3 point)
    {
        return 0;
    }
}
