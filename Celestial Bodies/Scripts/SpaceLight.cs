using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceStuff;

[RequireComponent(typeof(Light))]
public class SpaceLight : MonoBehaviour
{
    Light _light;
    [SerializeField] ScaledTransform scaledTransform;
    [SerializeField] float strength = 1f;
    TransformChange target;

    private void OnEnable()
    {
        target = Camera.main.transform.GetComponent<TransformChange>();
        target.PositionChangeEvent += LookAtTarget;
    }

    private void OnDisable()
    {
        target.PositionChangeEvent -= LookAtTarget;
    }

    void Start()
    {
        _light = GetComponent<Light>();
    }

    private void LookAtTarget()
    {
        Vector3 difference = target.transform.position - scaledTransform.position.ToVector3();
        transform.rotation = Quaternion.LookRotation(difference);
        float sqrDistance = difference.sqrMagnitude;
        _light.intensity = strength * (1 / sqrDistance);
    }
}
