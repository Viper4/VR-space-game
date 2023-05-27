using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FloatingWorldOrigin : MonoBehaviour
{
    [SerializeField] float shiftThreshold = 500;

    void LateUpdate()
    {
        if(transform.position.x > shiftThreshold || transform.position.y > shiftThreshold || transform.position.z > shiftThreshold)
        {
            for(int i = 0; i < SceneManager.sceneCount; i++)
            {
                foreach (GameObject rootObject in SceneManager.GetSceneAt(i).GetRootGameObjects())
                {
                    rootObject.transform.position -= transform.position;
                }
            }
            transform.position = Vector3.zero;
            Debug.Log("Shifted floating origin " + transform.name);
        }
    }
}
