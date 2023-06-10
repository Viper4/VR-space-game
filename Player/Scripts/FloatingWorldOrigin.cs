using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SpaceStuff;

public class FloatingWorldOrigin : MonoBehaviour
{
    [SerializeField] float shiftThreshold = 500;
    [SerializeField] Transform movingTransform;

    void LateUpdate()
    {
        if(Mathf.Abs(movingTransform.position.x) > shiftThreshold || Mathf.Abs(movingTransform.position.y) > shiftThreshold || Mathf.Abs(movingTransform.position.z) > shiftThreshold)
        {
            for(int i = 0; i < SceneManager.sceneCount; i++)
            {
                foreach (GameObject rootObject in SceneManager.GetSceneAt(i).GetRootGameObjects())
                {
                    if (!rootObject.transform.HasTag("StaticPosition"))
                    {
                        if(rootObject.TryGetComponent<ScaledTransform>(out var scaledTransform))
                        {
                            scaledTransform.position -= movingTransform.position.ToVector3d();
                        }
                        else
                        {
                            rootObject.transform.position -= movingTransform.position;
                        }
                    }
                }
            }
            movingTransform.position = Vector3.zero;
            Debug.Log("Shifted floating origin " + movingTransform.name);
        }
    }
}
