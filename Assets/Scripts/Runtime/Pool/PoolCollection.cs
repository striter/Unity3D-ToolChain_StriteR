using System;
using System.Collections.Generic;

namespace Runtime.Pool
{
    public abstract class APoolCollection<InstanceClass,PoolElement> : Pool<PoolElement>
        where InstanceClass : APoolCollection<InstanceClass,PoolElement>, new()
        where PoolElement : new()
    {
        protected virtual void ClearCollection(PoolElement _collection)
        {
            throw new NotImplementedException();
        }
        public sealed override void Despawn(PoolElement _element)
        {
            base.Despawn(_element);
            ClearCollection(_element);
        }

        private static InstanceClass Instance { get; } = new ();
        public static PoolElement ISpawn() => Instance.Spawn();
        public static void ISpawn(out PoolElement _list) => _list = Instance.Spawn();
        public static void IDespawn(PoolElement _list) => Instance.Despawn(_list);
        
        public static Dictionary<int,PoolElement> kEmptyCollections = new( ){ {-1,new PoolElement()} };
        public static PoolElement Empty(string _uniqueKey) => Empty(_uniqueKey.GetHashCode());
        public static PoolElement Empty(int _uniqueId)
        {
            if (kEmptyCollections.TryGetValue(_uniqueId, out var existList))
            {
                Instance.ClearCollection(existList);
                return existList; 
            }
            
            var list = new PoolElement();
            kEmptyCollections.Add(_uniqueId, list);
            return list;
        }
    }
    
    public class PoolList<T> : APoolCollection<PoolList<T>,List<T>>
    {
        protected override void ClearCollection(List<T> _collection) => _collection.Clear();
        
        private static readonly int kTraversalKey = nameof(Traversal).GetHashCode();
        public static List<T> Traversal(IEnumerable<T> _src)
        {
            var list = Empty(kTraversalKey);
            list.AddRange(_src);
            return list;
        }
    }

    public class PoolStack<T> : APoolCollection<PoolStack<T>,Stack<T>>
    {
        protected override void ClearCollection(Stack<T> _collection) => _collection.Clear();
    }

    public class PoolQueue<T> : APoolCollection<PoolQueue<T>,Queue<T>>
    {
        protected override void ClearCollection(Queue<T> _collection) => _collection.Clear();
    }
    
    public class PoolHashSet<T> : APoolCollection<PoolHashSet<T>,HashSet<T>>
    {
        protected override void ClearCollection(HashSet<T> _collection) => _collection.Clear();
    }
}