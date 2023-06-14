using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FlatCamera : MonoBehaviour
{
    public static Camera instance;
    [SerializeField] Transform source;

    void Start()
    {
        instance = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if(source != null)
        {
            transform.localEulerAngles = new Vector3(source.localEulerAngles.x, source.localEulerAngles.y, 0);
        }
    }
}
