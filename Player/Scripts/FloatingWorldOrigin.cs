using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SpaceStuff;

public class FloatingWorldOrigin : MonoBehaviour
{
    [SerializeField] float shiftThreshold = 500;

    void LateUpdate()
    {
        if(Mathf.Abs(transform.position.x) > shiftThreshold || Mathf.Abs(transform.position.y) > shiftThreshold || Mathf.Abs(transform.position.z) > shiftThreshold)
        {
            Vector3 positionOffset = transform.position;
            for(int i = 0; i < SceneManager.sceneCount; i++)
            {
                foreach(GameObject rootObject in SceneManager.GetSceneAt(i).GetRootGameObjects())
                {
                    if(!rootObject.transform.HasTag("StaticPosition"))
                    {
                        if(rootObject.TryGetComponent<ScaledTransform>(out var scaledTransform))
                        {
                            scaledTransform.position -= positionOffset.ToVector3d();
                        }
                        else
                        {
                            rootObject.transform.position -= positionOffset;
                        }
                    }
                }
            }
            transform.position = Vector3.zero;
            Debug.Log("Shifted floating origin " + transform.name);
        }
    }
}
