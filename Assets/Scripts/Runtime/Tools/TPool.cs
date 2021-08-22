using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace ObjectPoolStatic
{
    public interface ISPoolItem
    {
        void OnPoolInit();
        void OnPoolSpawn();
        void OnPoolRecycle();
    }
    public static class TSPool_Extend
    {
        public static void Recycle<T>(this T poolItem) where T : ISPoolItem, new()
        {
            TSPool<T>.Recycle(poolItem);
            poolItem.OnPoolRecycle();
        }
    }
    public static class TSPool<T> where T : ISPoolItem, new()
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
}
namespace ObjectPool
{
        
    #region Reference 
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
    #endregion
    #region Object
    public interface IPoolCallback
    {
        void OnPoolInit(Action<int> _DoRecycle);
        void OnPoolSpawn(int identity);
        void OnPoolRecycle();
    }
    
    public abstract class AObjectPool<T>
    {
        private readonly Transform transform;
        private readonly GameObject m_PoolItem;
        public Dictionary<int, T> m_ActiveItems { get; } = new Dictionary<int, T>();
        private List<T> m_PooledItems { get; } = new List<T>();
        public int Count => m_ActiveItems.Count;
        protected AObjectPool(GameObject pooledItem,Transform _transform)
        {
            transform = _transform;
            m_PoolItem = pooledItem;
            m_PoolItem.gameObject.SetActive(false);
        }
        public T GetOrAddItem(int identity)
        {
            if (ContainsItem(identity))
                return GetItem(identity);
            return AddItem(identity);
        }
        public bool ContainsItem(int identity) => m_ActiveItems.ContainsKey(identity);
        public T GetItem(int identity) => m_ActiveItems[identity];
        public T AddItem() => AddItem(Count + 1);
        public T AddItem(int identity)
        {
            T targetItem;
            if (m_PooledItems.Count > 0)
            {
                targetItem = m_PooledItems[0];
                m_PooledItems.Remove(targetItem);
            }
            else
            {
                targetItem = CreateNewItem(UnityEngine.Object.Instantiate(m_PoolItem, transform).transform);
                if(targetItem is IPoolCallback iPoolInit)
                    iPoolInit.OnPoolInit(RemoveItem);
            }
            if (m_ActiveItems.ContainsKey(identity)) Debug.LogError(identity + "Already Exists In Grid Dic");
            else m_ActiveItems.Add(identity, targetItem);
            Transform trans = GetItemTransform(targetItem);
            trans.SetAsLastSibling();
            trans.name = identity.ToString();
            trans.SetActive(true);
            
            if(targetItem is IPoolCallback iPoolSpawn)
                iPoolSpawn.OnPoolSpawn(identity);
            return targetItem;
        }

        public void RemoveItem(int identity)
        {
            T item = m_ActiveItems[identity];
            m_PooledItems.Add(item);
            Transform itemTransform = GetItemTransform(item);
            itemTransform.SetActive(false);
            itemTransform.SetParent(transform);
            if(item is IPoolCallback iPoolItem)
                iPoolItem.OnPoolRecycle();
            m_ActiveItems.Remove(identity);
        }

        public void Sort(Comparison<KeyValuePair<int,T>> Compare)
        {
            List<KeyValuePair<int, T>> list = m_ActiveItems.ToList();
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

        protected virtual T CreateNewItem(Transform instantiateTrans)
        {
            Debug.LogError("Override This Please");
            return default(T);
        }
        protected virtual Transform GetItemTransform(T targetItem)
        {
            Debug.LogError("Override This Please");
            return null;
        }
    }

    public class TObjectPoolComponent<T> : AObjectPool<T> where T : Component
    {
        public TObjectPoolComponent(Transform poolItem) : base(poolItem.gameObject,poolItem.transform.parent) {  }
        protected override T CreateNewItem(Transform instantiateTrans)=>instantiateTrans.GetComponent<T>();
        protected override Transform GetItemTransform(T targetItem) => targetItem.transform;
    }

    public class TObjectPoolTransform : AObjectPool<Transform>
    {
        public TObjectPoolTransform(Transform _poolItem) : base(_poolItem.gameObject,_poolItem.transform.parent) {  }

        protected override Transform CreateNewItem(Transform instantiateTrans)
        {
            return instantiateTrans;
        }

        protected override Transform GetItemTransform(Transform targetItem) => targetItem;
    } 
    #endregion
    #region Implement

    public abstract class APoolItem:ITransform
    {
        public Transform iTransform { get; }
        private Action<int> DoRecycle { get; set; }
        public int m_Identity { get; private set; }
        public APoolItem(Transform _transform)
        {
            iTransform = _transform;
        }
        public virtual void OnPoolInit(Action<int> _DoRecycle)=>this.DoRecycle = _DoRecycle;
        public virtual void OnPoolSpawn(int identity)=> m_Identity = identity;
        public virtual void OnPoolRecycle()=>m_Identity = -1;
        public void Recycle()=>DoRecycle(m_Identity);
    }
    

    public class TObjectPoolClass<T> : AObjectPool<T> where T :ITransform
    {
        private readonly Type m_Type;
        public TObjectPoolClass(Transform _poolTrans, Type type) : base(_poolTrans.gameObject,_poolTrans.parent) { m_Type = type; }
        public TObjectPoolClass(Transform _poolTrans) : this(_poolTrans,typeof(T) ) {}
        protected override T CreateNewItem(Transform instantiateTrans)
        {
            T item = UReflection.CreateInstance<T>(m_Type, instantiateTrans);
            return item;
        }

        protected override Transform GetItemTransform(T targetItem) => targetItem.iTransform;
    }
    
    
    public abstract class APoolMono : MonoBehaviour,IPoolCallback
    {
        private Action<int> DoRecycle { get; set; }
        public int m_Identity { get; private set; }
        public virtual void OnPoolInit(Action<int> _DoRecycle)=>this.DoRecycle = _DoRecycle;
        public virtual void OnPoolSpawn(int identity)=> m_Identity = identity;
        public virtual void OnPoolRecycle()=>m_Identity = -1;
        public void Recycle()=>DoRecycle(m_Identity);
    }
    public class TObjectPoolMono<T> : AObjectPool<T> where T : APoolMono
    {
        private readonly Type m_Type;
        public TObjectPoolMono(Transform _poolTrans, Type type) : base(_poolTrans.gameObject,_poolTrans.parent) { m_Type = type; }
        public TObjectPoolMono(Transform _poolTrans) : this(_poolTrans,typeof(T) ) {}
        protected override T CreateNewItem(Transform instantiateTrans)
        {
            T item = instantiateTrans.GetComponent<T>();
            item.OnPoolInit(RemoveItem);
            return item;
        }
        protected override Transform GetItemTransform(T targetItem) => targetItem.transform;
    }
    #endregion
}