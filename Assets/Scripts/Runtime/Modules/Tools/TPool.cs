using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Pool;
using Object = System.Object;

namespace TPoolStatic
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
        static Stack<T> m_PoolItems = new Stack<T>();
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

    public static class TSPoolObject<T> where T : UnityEngine.Object, new()
    {
        static Stack<T> m_PoolItems = new Stack<T>();
        public static T Spawn()
        {
            T item;
            if (m_PoolItems.Count > 0)
                item = m_PoolItems.Pop();
            else
                item = new T();
            return item;
        }
        public static void Recycle(T item)
        {
            m_PoolItems.Push(item);
        }
        public static void Dispose()
        {
            foreach (var item in m_PoolItems)
                UnityEngine.Object.DestroyImmediate(item);
            m_PoolItems.Clear();
            m_PoolItems = null;
        }
    }

    public static class TSPoolCollection<T,Y> where T:ICollection,new()
    {
        private static readonly MethodInfo kClearMethod = typeof(T).GetMethod("Clear");
        private static Stack<T> m_PoolItems { get; set; } = new Stack<T>();
        public static T Spawn()
        {
            T collection=m_PoolItems.Count > 0?m_PoolItems.Pop():new T();
            kClearMethod.Invoke(collection,null);
            return collection;
        }
        public static void Recycle(T item)
        {
            m_PoolItems.Push(item);
        }
    }

    public static class TSPoolList<T>
    {
        public static List<T> Spawn() => TSPoolCollection<List<T>, T>.Spawn();
        public static void Recycle(List<T> _list) => TSPoolCollection<List<T>, T>.Recycle(_list);
    }
    public static class TSPoolStack<T> 
    {
        public static Stack<T> Spawn() => TSPoolCollection<Stack<T>, T>.Spawn();
        public static void Recycle(Stack<T> _stack) => TSPoolCollection<Stack<T>, T>.Recycle(_stack);
    }
    public static class TSPoolQueue<T> 
    {
        public static Queue<T> Spawn() => TSPoolCollection<Queue<T>, T>.Spawn();
        public static void Recycle(Queue<T> _queue) => TSPoolCollection<Queue<T>, T>.Recycle(_queue);
    }
}
namespace TPool
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

    public static class ObjectPoolHelper
    {
        public static T Spawn<T>(this AObjectPool<int, T> _pool)=>_pool.Spawn(_pool.Count);
        public static Y TrySpawn<T, Y>(this AObjectPool<T, Y> _pool, T _identity)=> _pool.Contains(_identity) ? _pool.Get(_identity) : _pool.Spawn(_identity);
    }
    public interface IPoolCallback<T>
    {
        void OnPoolInit(Action<T> _DoRecycle);
        void OnPoolSpawn(T identity);
        void OnPoolRecycle();
    }
    
    public abstract class AObjectPool<T,Y>:IEnumerable<Y>
    {
        public readonly Dictionary<T, Y> m_Dic = new Dictionary<T, Y>();
        readonly Stack<Y> m_PooledItems = new Stack<Y>();
        private readonly Transform transform;
        private readonly GameObject m_PoolItem;
        public int Count => m_Dic.Count;
        protected AObjectPool(GameObject pooledItem,Transform _transform)
        {
            transform = _transform;
            m_PoolItem = pooledItem;
            m_PoolItem.gameObject.SetActive(false);
        }
        public bool Contains(T identity) => m_Dic.ContainsKey(identity);
        public Y Get(T identity) => m_Dic[identity];
        public Y this[T identity] => m_Dic[identity];
        public Y Spawn(T identity)
        {
            Y targetItem;
            if (m_PooledItems.Count > 0)
                targetItem = m_PooledItems.Pop();
            else
            {
                targetItem = CreateNewItem(UnityEngine.Object.Instantiate(m_PoolItem, transform).transform);
                if(targetItem is IPoolCallback<T> iPoolInit)
                    iPoolInit.OnPoolInit(DoRecycle);
            }
            if (m_Dic.ContainsKey(identity)) Debug.LogError(identity + "Already Exists In Grid Dic");
            else m_Dic.Add(identity, targetItem);
            Transform trans = GetItemTransform(targetItem);
            trans.SetAsLastSibling();
            trans.name = identity.ToString();
            trans.SetActive(true);
            
            if(targetItem is IPoolCallback<T> iPoolSpawn)
                iPoolSpawn.OnPoolSpawn(identity);
            return targetItem;
        }

        protected void DoRecycle(T identity)
        {
            var item = m_Dic[identity];
            m_PooledItems.Push(item);
            Transform itemTransform = GetItemTransform(item);
            itemTransform.SetActive(false);
            itemTransform.SetParent(transform);
            if(item is IPoolCallback<T> iPoolItem)
                iPoolItem.OnPoolRecycle();
            m_Dic.Remove(identity);
        }

        public Y Recycle(T identity)
        {
            Y item = m_Dic[identity];
            DoRecycle(identity);
            return item;
        }

        public void Sort(Comparison<KeyValuePair<T,Y>> Compare)
        {
            List<KeyValuePair<T, Y>> list = m_Dic.ToList();
            list.Sort(Compare);
            m_Dic.Clear();
            foreach (var pair in list)
            {
                GetItemTransform(pair.Value).SetAsLastSibling();
                m_Dic.Add(pair.Key,pair.Value);
            }
        }

        public void Clear()
        {
            foreach (var item in m_Dic.ToArray())
                Recycle(item.Key);
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

        public IEnumerator<Y> GetEnumerator()
        {
            foreach (var value in m_Dic.Values)
                yield return value;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class TObjectPoolComponent<T> : AObjectPool<int,T> where T : Component
    {
        public TObjectPoolComponent(Transform poolItem) : base(poolItem.gameObject,poolItem.transform.parent) {  }
        protected override T CreateNewItem(Transform instantiateTrans)=>instantiateTrans.GetComponent<T>();
        protected override Transform GetItemTransform(T targetItem) => targetItem.transform;
    }

    public class TObjectPoolTransform : AObjectPool<int,Transform>
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

    public abstract class APoolItem:ITransform,IPoolCallback<int>
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
    

    public class TObjectPoolClass<T> : AObjectPool<int,T> where T :ITransform
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
    
    
    public abstract class PoolBehaviour<T> : MonoBehaviour,IPoolCallback<T>
    {
        private Action<T> DoRecycle { get; set; }
        public T m_PoolID { get; private set; }
        public virtual void OnPoolInit(Action<T> _DoRecycle)=>this.DoRecycle = _DoRecycle;
        public virtual void OnPoolSpawn(T identity)=> m_PoolID = identity;
        public virtual void OnPoolRecycle()=> m_PoolID = default;
        public void Recycle()=>DoRecycle(m_PoolID);
    }
    public class TObjectPoolMono<T,Y> : AObjectPool<T,Y> where Y : PoolBehaviour<T>
    {
        public TObjectPoolMono(Transform _poolTrans) : base(_poolTrans.gameObject,_poolTrans.parent) {  }
        protected override Y CreateNewItem(Transform instantiateTrans)=>instantiateTrans.GetComponent<Y>();
        protected override Transform GetItemTransform(Y targetItem) => targetItem.transform;
    }
    
    #endregion
}