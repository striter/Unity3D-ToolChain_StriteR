using System;
using Unity.Mathematics;
using UnityEngine;

public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
    static T kInstance;
    public static T Instance => kInstance;

    protected virtual void Awake()
    {
        if (kInstance)
        {
            Debug.Log("SingletonMono " + typeof(T) + " Already Exists!");
            return;
        }
        
        kInstance = this.GetComponent<T>();
        this.name = typeof(T).Name;
    }
    protected  virtual void OnDestroy()
    {
        kInstance = null;
    }
}