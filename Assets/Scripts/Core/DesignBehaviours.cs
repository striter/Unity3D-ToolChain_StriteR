using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SimpleBehaviour
{
    protected Transform transform;
    public Transform Transform
    {
        get
        {
            return transform;
        }
    }
    public SimpleBehaviour(Transform _transform)
    {
        transform = _transform;
    }
}

public class SimpleMonoLifetime
{
    public virtual void Awake() { }
    public virtual void Update() { }
    public virtual void OnLateUpdate() { }
    public virtual void OnFixedUpdate() { }
    public virtual void OnDestroy() { }
    public virtual void OnEnable() { }
    public virtual void OnDisable() { }
}

public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
    static T instance;
    public static bool m_HaveInstance => instance;

    public static T Instance
    {
        get
        {
            if (!instance)
                instance = new GameObject().AddComponent<T>();
            return instance;
        }
    }

    protected virtual void Awake()
    {
        instance = this.GetComponent<T>();
        this.name = typeof(T).Name;
    }
    protected  virtual void OnDestroy()
    {
        instance = null;
    }
}


public class SingleTon<T> where T : new()
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new T();
            }
            return instance;
        }
    }
}