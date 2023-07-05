using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalData : MonoBehaviour
{
    private static GlobalData _instance;
    public static GlobalData instance 
    { 
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<GlobalData>();
            return _instance;
        }
    }

    public GameObject[] shipModels;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}
