using UnityEngine;

public interface ITransform
{
    Transform iTransform { get; }
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