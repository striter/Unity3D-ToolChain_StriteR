using UnityEngine;

public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
    static T instance;
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
    public T SingletonInit()
    {
        return this as T;
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