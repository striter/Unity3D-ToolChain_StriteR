using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using TObjectPool;
using UnityEngine;

namespace TObjectPool
{
#region ObjectPool

    public interface IObjectPool
    {
        void OnPoolCreate();
        void OnPoolInitialize();
        void OnPoolRecycle();
    }
    public static class TSPool_Extend
    {
        public static void Recycle<T>(this T poolItem) where T : IObjectPool, new()
        {
            ObjectPool<T>.Recycle(poolItem);
            poolItem.OnPoolRecycle();
        }
    }
    public static class ObjectPool<T> where T : new()
    {
        private static Stack<T> kPoolItems = new Stack<T>();
        public static T Spawn()
        {
            T item;
            if (kPoolItems.Count > 0)
            {
                item = kPoolItems.Pop();
            }
            else
            {
                item = new T();
                (item as IObjectPool)?.OnPoolCreate();
            }
            (item as IObjectPool)?.OnPoolInitialize();
            return item;
        }

        public static T Spawn(out T _data) => _data = Spawn();
        public static void Recycle(T item)
        {
            (item as IObjectPool)?.OnPoolRecycle();
            kPoolItems.Push(item);
        }
        public static void Dispose()
        {
            kPoolItems.Clear();
            kPoolItems = null;
        }

        public static void Clear() => kPoolItems.Clear();
    }

    public static class TSPoolCollection<T> where T:new()
    {
        private static Stack<T> m_PoolItems { get; set; } = new Stack<T>();
        public static T Spawn(Action<T> _clear)
        {
            T collection=m_PoolItems.Count > 0?m_PoolItems.Pop():new T();
            _clear.Invoke(collection);
            return collection;
        }
        public static void Recycle(T item)
        {
            m_PoolItems.Push(item);
        }

        public static void Clear() => m_PoolItems.Clear();
    }

    public static class TSPoolList<T>
    {
        public static List<T> Spawn() => TSPoolCollection<List<T>>.Spawn(_p=>_p.Clear());
        public static void Spawn(out List<T> _list) =>_list=TSPoolCollection<List<T>>.Spawn(_p=>_p.Clear());
        public static void Recycle(List<T> _list) => TSPoolCollection<List<T>>.Recycle(_list);
        public static void Clear()=>TSPoolCollection<List<T>>.Clear();
    }
    public static class TSPoolStack<T> 
    {
        public static Stack<T> Spawn() => TSPoolCollection<Stack<T>>.Spawn(_p=>_p.Clear());
        public static void Spawn(out Stack<T> _list) =>_list=TSPoolCollection<Stack<T>>.Spawn(_p=>_p.Clear());
        public static void Recycle(Stack<T> _stack) => TSPoolCollection<Stack<T>>.Recycle(_stack);
    }
    public static class TSPoolQueue<T> 
    {
        public static Queue<T> Spawn() => TSPoolCollection<Queue<T>>.Spawn(_p=>_p.Clear());
        public static void Spawn(out Queue<T> _list) =>_list=TSPoolCollection<Queue<T>>.Spawn(_p=>_p.Clear());
        public static void Recycle(Queue<T> _queue) => TSPoolCollection<Queue<T>>.Recycle(_queue);
    }
    public static class TSPoolLinkedList<T> 
    {
        public static LinkedList<T> Spawn() => TSPoolCollection<LinkedList<T>>.Spawn(_p=>_p.Clear());
        public static void Spawn(out LinkedList<T> _list) =>_list=TSPoolCollection<LinkedList<T>>.Spawn(_p=>_p.Clear());
        public static void Recycle(LinkedList<T> _queue) => TSPoolCollection<LinkedList<T>>.Recycle(_queue);
    }
    public static class TSPoolHashset<T> 
    {
        public static HashSet<T> Spawn() => TSPoolCollection<HashSet<T>>.Spawn(_p=>_p.Clear());
        public static void Spawn(out HashSet<T> _list) =>_list=TSPoolCollection<HashSet<T>>.Spawn(_p=>_p.Clear());
        public static void Recycle(HashSet<T> _queue) => TSPoolCollection<HashSet<T>>.Recycle(_queue);
    }
#endregion
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

    public static class ObjectPoolHelper
    {
        public static T Spawn<T>(this GameObjectPool<int, T> _pool) => _pool.Spawn(_pool.m_SpawnTimes);

        public static int Spawn<T>(this GameObjectPool<int, T> _pool, out T _object)
        {
            var index = _pool.m_SpawnTimes;
            _object = _pool.Spawn(index);
            return index;
        }
        public static Y TrySpawn<T, Y>(this GameObjectPool<T, Y> _pool, T _identity) => _pool.Contains(_identity) ? _pool.Get(_identity) : _pool.Spawn(_identity);
        public static void Recycle<Key>(this IPoolCallback<Key> _callback) => _callback.DoRecycle(_callback.identity);
    }
    public interface IPoolCallback<T>
    {
        Action<T> DoRecycle { get; set; }
        T identity { get; set; }
        virtual void OnPoolCreate(){}
        virtual void OnPoolSpawn() {}
        virtual void OnPoolRecycle() {}
        virtual void OnPoolDispose() {}
    }

    public interface IPoolCallback : IPoolCallback<int>
    {
        
    }

    public abstract class APoolElement : APoolElement<int>
    {
        public APoolElement(Transform _transform) : base(_transform)
        {
        }
    }

    public abstract class APoolElement<Key> : ITransform, IPoolCallback<Key>
    {
        public Transform transform { get; }
        public APoolElement(Transform _transform) { transform = _transform; }
        public Action<Key> DoRecycle { get; set; }
        public Key identity { get; set; }
        public virtual void OnPoolCreate() { }
        public virtual void OnPoolSpawn() {}
        public virtual void OnPoolRecycle() {}
        public virtual void OnPoolDispose() {}
        
    }
    
    public abstract class APoolBehaviour<T> : MonoBehaviour , IPoolCallback<T>
    {
        public Action<T> DoRecycle { get; set; }
        public T identity { get; set; }
        public virtual void OnPoolCreate() { }
        public virtual void OnPoolSpawn() {}
        public virtual void OnPoolRecycle() {}
        public virtual void OnPoolDispose() {}
    }

    public abstract class APoolBehaviour : APoolBehaviour<int> 
    {
        
    }
    public class GameObjectPool<Element> : GameObjectPool<int, Element>
    {
        public GameObjectPool(Element _pooledItem, Transform _transform = null) : base(_pooledItem, _transform)
        {
        }
    }

    public class GameObjectPool : GameObjectPool<int, Transform>
    {
        public GameObjectPool(Transform _pooledItem, Transform _transform = null) : base(_pooledItem, _transform)
        {
        }
    }
    public class GameObjectPool<Key,Element>:IEnumerable<Element>
    {
        public readonly Dictionary<Key, Element> m_Dic = new Dictionary<Key, Element>();
        readonly Stack<Element> m_PooledItems = new Stack<Element>();
        public readonly Transform transform;
        private readonly Element m_Template;
        public int Count => m_Dic.Count;
        public int m_SpawnTimes { get; private set; } = 0;
        public Action<Element> createCallback = null;
        public GameObjectPool(Element _pooledItem,Transform _transform = null)
        {
            var templateTransform = GetTransform(_pooledItem);
            _transform ??= templateTransform.parent;
            transform = _transform;
            m_Template = _pooledItem;
            templateTransform.gameObject.SetActive(false);
            m_SpawnTimes = 0;
        }
        public bool Contains(Key identity) => m_Dic.ContainsKey(identity);
        public Element Get(Key _identity) => m_Dic[_identity];
        public Element this[Key _identity] => m_Dic[_identity];
        public Element Spawn(Key _identity)
        {
            Element targetItem;
            if (m_PooledItems.Count > 0)
                targetItem = m_PooledItems.Pop();
            else
            {
                targetItem = CreateElement(UnityEngine.Object.Instantiate(GetTransform(m_Template).gameObject, transform).transform);
                createCallback?.Invoke(targetItem);
                if (targetItem is IPoolCallback<Key> iPoolInit)
                {
                    iPoolInit.DoRecycle = DoRecycle;
                    iPoolInit.OnPoolCreate();
                }
            }
            if (m_Dic.ContainsKey(_identity)){
                Debug.LogError(_identity + "Already Exists In Grid Dic");
            }
            else
                m_Dic.Add(_identity, targetItem);
            Transform trans = GetTransform(targetItem);
            trans.SetAsLastSibling();
            trans.name = _identity.ToString();
            trans.gameObject.SetActive(true);

            if (targetItem is IPoolCallback<Key> iPoolSpawn)
            {
                iPoolSpawn.identity = _identity;
                iPoolSpawn.OnPoolSpawn();
            }
            m_SpawnTimes++;
            return targetItem;
        }

        private void DoRecycle(Key identity)
        {
            RecycleItem(m_Dic[identity]);
            m_Dic.Remove(identity);
        }

        void RecycleItem(Element item)
        {
            m_PooledItems.Push(item);
            Transform itemTransform = GetTransform(item);
            itemTransform.gameObject.SetActive(false);
            itemTransform.SetParent(transform);
            if (item is IPoolCallback<Key> iPoolItem)
                iPoolItem.OnPoolRecycle();
        }

        public Element Recycle(Key _identity)
        {
            Element item = m_Dic[_identity];
            DoRecycle(_identity);
            return item;
        }

        public bool TryGet(Key _identity,out Element _element)
        {
            _element = default;
            if (!Contains(_identity))
                return false;
            _element = this[_identity];
            return true;
        }
        
        public bool TryRecycle(Key _identity)
        {
            if (!Contains(_identity))
                return false;

            Recycle(_identity);
            return true;
        }

        public void Sort(Comparison<KeyValuePair<Key,Element>> Compare)
        {
            List<KeyValuePair<Key, Element>> list = m_Dic.ToList();
            list.Sort(Compare);
            m_Dic.Clear();
            foreach (var pair in list)
            {
                GetTransform(pair.Value).SetAsLastSibling();
                m_Dic.Add(pair.Key,pair.Value);
            }
        }

        public void Clear()
        {
            m_SpawnTimes = 0;
            foreach (var item in m_Dic.Values)
                RecycleItem(item);
            m_Dic.Clear();
        }

        public void Dispose()
        {
            Clear();
            foreach (var item in m_PooledItems)
            {
                if (item is IPoolCallback<Key> iPoolItem)
                    iPoolItem.OnPoolDispose();
                UnityEngine.Object.Destroy(GetTransform(item).gameObject);
            }
        }

        private static readonly object[] kParamsHelper = new object[1];
        protected virtual Element CreateElement(Transform _instantiateTrans)
        {
            if (m_Template is Component)
                return _instantiateTrans.GetComponent<Element>();
            if (m_Template is MonoBehaviour)
                return _instantiateTrans.GetComponent<Element>();
            if (m_Template is ITransform)
            {
                kParamsHelper[0] = _instantiateTrans;
                return (Element)Activator.CreateInstance(m_Template.GetType(),kParamsHelper);
            }
            throw new ArgumentException($"[]:{m_Template.GetType()} Type Mis Match");
        }
        protected virtual Transform GetTransform(Element _targetItem)
        {
            if (_targetItem is Component component)
                return component.transform;
            if (_targetItem is MonoBehaviour monoBehaviour)
                return monoBehaviour.transform;
            if (_targetItem is ITransform transform)
                return transform.transform;
            throw new ArgumentException("Type Mis Match");
        }

        public IEnumerator<Element> GetEnumerator()
        {
            foreach (var value in m_Dic.Values)
                yield return value;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
