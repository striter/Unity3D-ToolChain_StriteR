using System;
using System.Collections.Generic;

namespace LinqExtension
{
    public static class Linq_Extend
    {
        public static void VoidExecute<T>(this IEnumerable<T> _collection)
        {
            foreach (var item in _collection)
            {
                //Empty;
            }
        }
        public static IEnumerable<(T value, int index)> LoopIndex<T>(this IEnumerable<T> _collection)
        {
            int index = 0;
            foreach (T element in _collection)
                yield return (element,index++);
        }
        public static IEnumerable<T> Extend<T>(this IEnumerable<T> _collection, IEnumerable<T> _extend)
        {
            foreach (T element in _collection)
                yield return element;
            foreach (T element in _extend)
                yield return element;
        }
        public static IEnumerable<T> Extend<T>(this IEnumerable<T> _collection, T _extend)
        {
            foreach (T element in _collection)
                yield return element;
            yield return _extend;
        }

        public static  IEnumerable<T> Collect<T>(this IEnumerable<T> _collection, Predicate<T> _Predicate)
        {
            foreach (T element in _collection)
            {
                if (!_Predicate(element))
                    continue;
                yield return element;
            }
        }
        
        public static  IEnumerator<T> CollectItem<T>(this IEnumerable<T> _collection, Predicate<T> _Predicate)
        {
            using var enumerator = _collection.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var element = enumerator.Current;
                if (!_Predicate(element))
                    continue;
                yield return element;
            }
        }

        public static IEnumerable<T> Collect<T>(this IEnumerable<T> _collections, Func<int,T,bool> _Predicate)
        {
            foreach ((T element, int index) in _collections.LoopIndex())
            {
                if(!_Predicate(index,element))
                    continue;
                yield return element;
            }
        }
        
        public static IEnumerable<int> CollectIndex<T>(this IEnumerable<T> _collection, Predicate<T> _Predicate)
        {
            foreach ((T item, int index) in _collection.LoopIndex())
            {
                if (!_Predicate(item))
                    continue;  
                yield return index;
            }
        }
        
        public static IEnumerable<int> CollectIndex<T>(this IEnumerable<T> _collection, Func<int,T,bool> _Predicate)
        {
            foreach ((T item, int index) in _collection.LoopIndex())
            {
                if (!_Predicate(index,item))
                    continue;  
                yield return index;
            }
        }

        public static void FillCollection<T>(this IEnumerable<T> _collection, List<T> _list)
        {
            _list.Clear();
            foreach (var element in _collection)
                _list.Add(element);
        }
    }
}