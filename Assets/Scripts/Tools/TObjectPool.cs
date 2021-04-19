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

#region Static Game Object Pool 
public interface IGameObjectPool_Static<T>{
    void OnPoolInit(T identity,Action<T,MonoBehaviour> OnRecycle);
    void OnPoolSpawn();
    void OnPoolRecycle();
}
public class CGameObjectPool_Static<T> :MonoBehaviour,IGameObjectPool_Static<T>
{
    public bool m_PoolItemInited { get; private set; }
    public T m_PoolID { get; private set; }
    private Action<T,MonoBehaviour> OnSelfRecycle;
    public virtual void OnPoolInit(T _identity,Action<T,MonoBehaviour> _OnSelfRecycle)
    {
        m_PoolID = _identity;
        m_PoolItemInited = true;
        OnSelfRecycle = _OnSelfRecycle;
    }
    public void DoRecycle() =>  OnSelfRecycle?.Invoke(m_PoolID, this);

    public virtual void OnPoolSpawn() {  }
    public virtual void OnPoolRecycle() { }
}
public class TGameObjectPool_Static
{
    protected static Transform tf_PoolSpawn { get; private set; }
    public static void Init()
    {
        tf_PoolSpawn= new GameObject("PoolSpawn").transform;
    }
}
public class TGameObjectPool_Static<T, Y> : TGameObjectPool_Static where Y : MonoBehaviour, IGameObjectPool_Static<T>
{
    class GameObjectStaticPoolItem
    {
        public T m_Identity;
        public Y m_SpawnItem;
        public Stack<Y> m_DeactiveStack = new Stack<Y>();
        public List<Y> m_ActiveList = new List<Y>();
        public Y Create(Vector3 position,Quaternion rotation)
        {
            Y item = GameObject.Instantiate(m_SpawnItem,position,rotation, tf_PoolSpawn); 
            item.name = m_SpawnItem.name + "_" + (m_DeactiveStack.Count + m_ActiveList.Count).ToString();
            item.OnPoolInit(m_Identity, (temp,mono)=>Recycle(temp, mono as Y));
            item.SetActive(false);
            return item;
        }

        public void Destroy()
        {
            for (; m_DeactiveStack.Count > 0;)
                GameObject.Destroy(m_DeactiveStack.Pop().gameObject);
            for (int i = 0; i < m_ActiveList.Count; i++)
                GameObject.Destroy(m_ActiveList[i].gameObject);

            m_DeactiveStack.Clear();
            m_ActiveList.Clear();
        }
    }

    static Dictionary<T, GameObjectStaticPoolItem> d_ItemInfos = new Dictionary<T, GameObjectStaticPoolItem>();
    public static bool Registed(T identity)
    {
        return d_ItemInfos.ContainsKey(identity);
    }
    public static List<T> GetRegistedList() => d_ItemInfos.Keys.ToList();
    public static Y GetRegistedSpawnItem(T identity)
    {
        if (!Registed(identity))
            Debug.LogError("Identity:" + identity + "Unregisted");
        return d_ItemInfos[identity].m_SpawnItem;
    }
    public static void Register(T _identity, Y _registerItem, int _poolStartAmount)
    {
        if (d_ItemInfos.ContainsKey(_identity))
        {
            Debug.LogError("Same Element Already Registed:" + _identity.ToString() + "/" + _registerItem.gameObject.name);
            return;
        }
        d_ItemInfos.Add(_identity, new GameObjectStaticPoolItem());
        GameObjectStaticPoolItem info = d_ItemInfos[_identity];
        info.m_SpawnItem = _registerItem;
        info.m_Identity = _identity;
        for (int i = 0; i < _poolStartAmount; i++)
            info.m_DeactiveStack.Push(info.Create(Vector3.zero,Quaternion.identity));
    }
    public static Y Spawn(T identity, Transform toTrans, Vector3 pos, Quaternion rot)
    {
        if (!d_ItemInfos.ContainsKey(identity))
        {
            Debug.LogError("PoolManager:" + typeof(T).ToString() + "," + typeof(Y).ToString() + " Error! Null Identity:" + identity + "Registed");
            return null;
        }
        GameObjectStaticPoolItem info = d_ItemInfos[identity];
        Y item;
        if (info.m_DeactiveStack.Count > 0)
        {
            item = info.m_DeactiveStack.Pop();
            item.transform.position = pos;
            item.transform.rotation = rot;
        }
        else
        {
            item = info.Create(pos, rot);
        }

        item.transform.SetParent(toTrans == null ? tf_PoolSpawn : toTrans);
        item.SetActive(true);
        info.m_ActiveList.Add(item);
        item.OnPoolSpawn();
        return item;
    }
    public static void Recycle(T identity, Y obj)
    {
        if (!d_ItemInfos.ContainsKey(identity))
        {
            Debug.LogWarning("Null Identity Of GameObject:" + obj.name + "/" + identity + " Registed(" + typeof(T).ToString() + "|" + typeof(Y).ToString() + ")");
            return;
        }
        GameObjectStaticPoolItem info = d_ItemInfos[identity];
        info.m_ActiveList.Remove(obj);
        obj.OnPoolRecycle();
        obj.SetActive(false);
        obj.transform.SetParent(tf_PoolSpawn);
        info.m_DeactiveStack.Push(obj);
    }
    public static void TraversalAllActive(Action<Y> OnEachItem,bool willActivateChange=false) => d_ItemInfos.Traversal((GameObjectStaticPoolItem info) => {
        if (willActivateChange)
            info.m_ActiveList.DeepCopy().Traversal(OnEachItem);
        else
            info.m_ActiveList.Traversal(OnEachItem);
    });
    public static void RecycleAll(T identity)
    {
        GameObjectStaticPoolItem info = d_ItemInfos[identity];
        info.m_ActiveList.TraversalMark(item => true, item=> Recycle(identity, item)) ;
    }
    public static void RecycleAll(Predicate<Y> predicate=null) 
    {
        d_ItemInfos.Traversal((T identity, GameObjectStaticPoolItem info) =>
        {
            info.m_ActiveList.TraversalMark(target => predicate == null || predicate(target), target => Recycle(identity, target));
        });
    }
    public static void OnSceneChange() => d_ItemInfos.Clear();
    public static void DestroyPoolItem()
    {
        RecycleAll();
        d_ItemInfos.Traversal((GameObjectStaticPoolItem info) => { info.Destroy(); });
    }

    public static void Destroy()
    {
        DestroyPoolItem();
        d_ItemInfos.Clear();
    }
}
#endregion

#region Instance Game Object Pools
public class TGameObjectPool_Instance<T, Y> 
{
    public Transform transform { get; private set; }
    protected GameObject m_PoolItem;
    public Dictionary<T, Y> m_ActiveItemDic { get; private set; } = new Dictionary<T, Y>();
    public List<Y> m_InactiveItemList { get; private set; } = new List<Y>();
    public int Count => m_ActiveItemDic.Count;
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
    public bool ContainsItem(T identity) => m_ActiveItemDic.ContainsKey(identity);
    public Y GetItem(T identity) => m_ActiveItemDic[identity];
    public virtual Y AddItem(T identity)
    {
        Y targetItem;
        if (m_InactiveItemList.Count > 0)
        {
            targetItem = m_InactiveItemList[0];
            m_InactiveItemList.Remove(targetItem);
        }
        else
        {
            targetItem = CreateNewItem(UnityEngine.Object.Instantiate(m_PoolItem, transform).transform);
        }
        if (m_ActiveItemDic.ContainsKey(identity)) Debug.LogError(identity + "Already Exists In Grid Dic");
        else m_ActiveItemDic.Add(identity, targetItem);
        Transform trans = GetItemTransform(targetItem);
        trans.SetAsLastSibling();
        trans.name = identity.ToString();
        trans.SetActive(true);
        return targetItem;
    }

    public virtual void RemoveItem(T identity)
    {
        Y item = m_ActiveItemDic[identity];
        m_InactiveItemList.Add(item);
        Transform itemTransform = GetItemTransform(item);
        itemTransform.SetActive(false);
        itemTransform.SetParent(transform);
        m_ActiveItemDic.Remove(identity);
    }

    public void Sort(Comparison<KeyValuePair<T,Y>> Compare)
    {
        List<KeyValuePair<T, Y>> list = m_ActiveItemDic.ToList();
        list.Sort(Compare);
        m_ActiveItemDic.Clear();
        list.Traversal((KeyValuePair<T,Y> pair) =>
        {
            GetItemTransform(pair.Value).SetAsLastSibling();
            m_ActiveItemDic.Add(pair.Key,pair.Value);
        });
    }

    public void Clear()
    {
        m_ActiveItemDic.TraversalMark(item => true, RemoveItem);
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

#region Component
public class TGameObjectPool_Component<T, Y> : TGameObjectPool_Instance<T, Y> where Y : Component
{
    public TGameObjectPool_Component(Transform poolTrans, string itemName) : base(poolTrans, itemName) {  }
    protected override Y CreateNewItem(Transform instantiateTrans)=>instantiateTrans.GetComponent<Y>();
    protected override Transform GetItemTransform(Y targetItem) => targetItem.transform;
}
#endregion

#region Class
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
#endregion
#region Monobehaviour
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
#endregion
#endregion