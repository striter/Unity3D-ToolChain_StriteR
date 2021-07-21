using UnityEngine;
public class SimpleBehaviour
{
    public  Transform transform { get; private set; }
    public SimpleBehaviour(Transform _transform) { transform = _transform; }

}
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