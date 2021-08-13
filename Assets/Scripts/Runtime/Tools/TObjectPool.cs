using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#region Static Object Pool
public interface IObjectPool
{
    void OnPoolInit();
    void OnPoolSpawn();
    void OnPoolRecycle();
}
public static class TObjectPool_Extend
{
    public static void Recycle<T>(this T poolItem) where T : IObjectPool, new()
    {
        TObjectPool<T>.Recycle(poolItem);
        poolItem.OnPoolRecycle();
    }
}
public static class TObjectPool<T> where T : IObjectPool, new()
{
    public static Stack<T> m_PoolItems { get; private set; } = new Stack<T>();
    public static T Spawn()
    {
        T item;
        if (m_PoolItems.Count > 0)
        {
            item = m_PoolItems.Pop();
        }
        else
        {
            item = new T();
            item.OnPoolInit();
        }
        item.OnPoolSpawn();
        return item;
    }
    public static void Recycle(T item)
    {
        item.OnPoolRecycle();
        m_PoolItems.Push(item);
    }
    public static void Dispose()
    {
        m_PoolItems.Clear();
        m_PoolItems = null;
    }
}
#endregion

public class TPool<T> where T:class ,new()
{
    public Stack<T> m_Pooled { get; private set; } = new Stack<T>();
    public Stack<T> m_Activated { get; private set; } = new Stack<T>();
    public T Pop()
    {
        if (m_Pooled.Count != 0)
            return m_Pooled.Pop();

        T newElement = new T();
        m_Activated.Push(newElement);
        return new T();
    }
    public void Push(T _poolElement)
    {
        if (!m_Activated.Contains(_poolElement))
            throw new Exception("None Pooled Item Found!"+_poolElement);
        m_Pooled.Push(_poolElement);
    }

    public void Clear()
    {
        foreach (T _activate in m_Activated)
            m_Pooled.Push(_activate);
        m_Activated.Clear();
    }

    public void Dispose()
    {
        Clear();
        m_Pooled = null;
        m_Activated = null;
        GC.SuppressFinalize(this);
    }
}

public class TGameObjectPool_Instance<T, Y> 
{
    public Transform transform { get; private set; }
    protected GameObject m_PoolItem;
    public Dictionary<T, Y> m_ActiveItems { get; private set; } = new Dictionary<T, Y>();
    public List<Y> m_PooledItems { get; private set; } = new List<Y>();
    public int Count => m_ActiveItems.Count;
    public TGameObjectPool_Instance(Transform poolTrans, string itemName)
    {
        transform = poolTrans;
        m_PoolItem = poolTrans.Find(itemName).gameObject;
        m_PoolItem.gameObject.SetActive(false);
    }
    public Y GetOrAddItem(T identity)
    {
        if (ContainsItem(identity))
            return GetItem(identity);
        return AddItem(identity);
    }
    public bool ContainsItem(T identity) => m_ActiveItems.ContainsKey(identity);
    public Y GetItem(T identity) => m_ActiveItems[identity];
    public virtual Y AddItem(T identity)
    {
        Y targetItem;
        if (m_PooledItems.Count > 0)
        {
            targetItem = m_PooledItems[0];
            m_PooledItems.Remove(targetItem);
        }
        else
        {
            targetItem = CreateNewItem(UnityEngine.Object.Instantiate(m_PoolItem, transform).transform);
        }
        if (m_ActiveItems.ContainsKey(identity)) Debug.LogError(identity + "Already Exists In Grid Dic");
        else m_ActiveItems.Add(identity, targetItem);
        Transform trans = GetItemTransform(targetItem);
        trans.SetAsLastSibling();
        trans.name = identity.ToString();
        trans.SetActive(true);
        return targetItem;
    }

    public virtual void RemoveItem(T identity)
    {
        Y item = m_ActiveItems[identity];
        m_PooledItems.Add(item);
        Transform itemTransform = GetItemTransform(item);
        itemTransform.SetActive(false);
        itemTransform.SetParent(transform);
        m_ActiveItems.Remove(identity);
    }

    public void Sort(Comparison<KeyValuePair<T,Y>> Compare)
    {
        List<KeyValuePair<T, Y>> list = m_ActiveItems.ToList();
        list.Sort(Compare);
        m_ActiveItems.Clear();
        foreach (var pair in list)
        {
            GetItemTransform(pair.Value).SetAsLastSibling();
            m_ActiveItems.Add(pair.Key,pair.Value);
        }
    }

    public void Clear()
    {
        foreach (var item in m_ActiveItems.ToArray())
            RemoveItem(item.Key);
    } 

    protected virtual Y CreateNewItem(Transform instantiateTrans)
    {
        Debug.LogError("Override This Please");
        return default(Y);
    }
    protected virtual Transform GetItemTransform(Y targetItem)
    {
        Debug.LogError("Override This Please");
        return null;
    }
}

public class TGameObjectPool_Component<T, Y> : TGameObjectPool_Instance<T, Y> where Y : Component
{
    public TGameObjectPool_Component(Transform poolTrans, string itemName) : base(poolTrans, itemName) {  }
    protected override Y CreateNewItem(Transform instantiateTrans)=>instantiateTrans.GetComponent<Y>();
    protected override Transform GetItemTransform(Y targetItem) => targetItem.transform;
}

public interface IGameObjectPool_Instance<T>
{
    void OnInitItem(Action<T> DoRecycle);
    void OnAddItem(T identity);
    void OnRemoveItem();
    Transform GetTransform();
}

public class TGameObjectPool_Instance_Interface<T,Y>:TGameObjectPool_Instance<T,Y> where Y:IGameObjectPool_Instance<T>
{
    public TGameObjectPool_Instance_Interface(Transform poolTrans, string itemName) : base(poolTrans, itemName) { }
    public override Y AddItem(T identity)
    {
        Y item = base.AddItem(identity);
        item.OnAddItem(identity);
        return item;
    }

    public override void RemoveItem(T identity)
    {
        GetItem(identity).OnRemoveItem();
        base.RemoveItem(identity);
    }
}

public abstract class CGameObjectPool_Instance_Class<T>:IGameObjectPool_Instance<T>
{
    public Transform transform { get; private set; }
    protected Action<T> DoRecycle { get; private set; }
    public T m_Identity { get; private set; }
    public Transform GetPoolItemTransform() => transform;
    public CGameObjectPool_Instance_Class(Transform _transform)
    {
        transform = _transform;
    }
    public virtual void OnInitItem(Action<T> DoRecycle)
    {
    }

    public virtual void OnAddItem(T identity)
    {
        m_Identity = identity;
    }

    public virtual void OnRemoveItem()
    {

    }

    public Transform GetTransform() => transform;
}
public class TGameObjectPool_Instance_Class<T, Y> : TGameObjectPool_Instance_Interface<T, Y> where Y : CGameObjectPool_Instance_Class<T>
{
    public Type m_Type;
    public TGameObjectPool_Instance_Class(Transform poolTrans, Type type, string itemName) : base(poolTrans, itemName) { m_Type = type; }
    public TGameObjectPool_Instance_Class(Transform poolTrans, string itemName) : base(poolTrans, itemName) { m_Type = typeof(Y); }
    protected override Y CreateNewItem(Transform instantiateTrans)
    {
        Y item = UReflection.CreateInstance<Y>(m_Type, instantiateTrans);
        item.OnInitItem(RemoveItem);
        return item;
    } 
    protected override Transform GetItemTransform(Y targetItem) => targetItem.transform;
}
public class CGameObjectPool_Instance_Monobehaviour<T> : MonoBehaviour, IGameObjectPool_Instance<T>
{
    public T m_Identity { get; private set; }
    protected Action<T> DoRecycle { get; private set; }
    public Transform GetTransform() => transform;

    public virtual void OnInitItem(Action<T> DoRecycle)
    {
        this.DoRecycle = DoRecycle;
    }

    public virtual void OnAddItem(T identity)
    {
        m_Identity = identity;
    }
    public virtual void OnRemoveItem()
    {
    }
}

public class TGameObjectPool_Instance_Monobehaviour<T, Y> : TGameObjectPool_Instance_Interface<T, Y> where Y : CGameObjectPool_Instance_Monobehaviour<T>
{
    public TGameObjectPool_Instance_Monobehaviour(Transform poolTrans, string itemName) : base(poolTrans, itemName)
    {
    }
    protected override Y CreateNewItem(Transform instantiateTrans)
    {
        Y item = instantiateTrans.GetComponent<Y>();
        item.OnInitItem(RemoveItem);
        return item;
    }
    protected override Transform GetItemTransform(Y targetItem) => targetItem.transform;
}

public class TObjectPool_Transform : TGameObjectPool_Instance<int, Transform>
{
    public TObjectPool_Transform(Transform poolTrans, string itemName) : base(poolTrans, itemName)
    {
    }

    protected override Transform CreateNewItem(Transform instantiateTrans)
    {
        return instantiateTrans;
    }

    protected override Transform GetItemTransform(Transform targetItem) => targetItem;
} 