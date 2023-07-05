using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FlatCamera : MonoBehaviour
{
    private static Camera _instance;
    public static Camera instance { 
        get
        {
            if(_instance == null)
            {
                _instance = FindObjectOfType<FlatCamera>().GetComponent<Camera>();
            }
            return _instance;
        }
    }
    [SerializeField] Transform source;

    void LateUpdate()
    {
        if(source != null)
        {
            transform.localEulerAngles = new Vector3(source.localEulerAngles.x, source.localEulerAngles.y, 0);
        }
    }
}
