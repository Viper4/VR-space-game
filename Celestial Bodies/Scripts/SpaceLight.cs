using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceStuff;

public class SpaceLight : MonoBehaviour
{
    private const double k = 5.67e-8;

    [SerializeField] Light scaledLight;
    [SerializeField] Light worldLight;
    ScaledTransform scaledTransform;
    TransformChange target;
    [SerializeField] float temperature = 5780f;
    double luminosity;

    private void OnEnable()
    {
        target = Camera.main.transform.GetComponent<TransformChange>();
        target.PositionChangeEvent += UpdateLight;
    }

    private void OnDisable()
    {
        target.PositionChangeEvent -= UpdateLight;
    }

    public void Init(float radius)
    {
        scaledTransform = GetComponent<ScaledTransform>();
        // L = 4piR^2sigmaT^4
        luminosity = k * radius * radius * temperature * temperature * temperature * temperature;
        scaledLight.intensity = (float)(luminosity / (scaledTransform.scaleFactor * scaledTransform.scaleFactor));
        UpdateLight();
    }

    private void UpdateLight()
    {
        Vector3 difference = target.transform.position - scaledTransform.position.ToVector3();
        worldLight.transform.rotation = Quaternion.LookRotation(difference);

        // Inverse square law of light: f = L / (4piD^2)
        float sqrDistance = difference.sqrMagnitude;
        worldLight.intensity = (float)(luminosity / sqrDistance);
        if (scaledTransform.inScaledSpace)
            worldLight.shadows = LightShadows.Soft;
        else
            worldLight.shadows = LightShadows.None;
    }
}
