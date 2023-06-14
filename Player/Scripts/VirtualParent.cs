using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualParent : MonoBehaviour
{
    [SerializeField] Transform parent;
    [SerializeField] bool position;
    [SerializeField] bool rotation;

    void FixedUpdate()
    {
        if(parent != null)
        {
            if(position)
                transform.position = parent.position;
            if(rotation)
                transform.rotation = parent.rotation;
        }
    }
}
