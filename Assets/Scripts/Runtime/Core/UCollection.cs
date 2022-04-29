using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TPoolStatic;
using UnityEngine;

public static class UCollection
{
    #region IEnumrable
        public static void Execute(this IEnumerator _iterator)
        {
            while (_iterator.MoveNext()) {
                //Empty;
            }
        }

        public static IEnumerable<T> Resolve<T>(this IEnumerable<IEnumerable<T>> _collections)
        {
            foreach (var collection in _collections)
            {
                foreach (var element in collection)
                {
                    yield return element;
                }
            }
        }

        public static IEnumerable<(int index,T value)> LoopIndex<T>(this IEnumerable<T> _collection)
        {
            int index = 0;
            foreach (T element in _collection)
                yield return (index++,element);
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
        
        public static  IEnumerable<Y> Collect<T,Y>(this IEnumerable<T> _collection) where T:class where Y:class
        {
            foreach (T element in _collection)
            {
                if (!(element is Y element1))
                    continue;
                yield return element1;
            }
        }

        public static IEnumerable<T> Collect<T>(this IEnumerable<T> _collections, Func<int,T,bool> _Predicate)
        {
            foreach (var (index,value) in _collections.LoopIndex())
            {
                if(!_Predicate(index,value))
                    continue;
                yield return value;
            }
        }
        
        public static IEnumerable<int> CollectIndex<T>(this IEnumerable<T> _collection, Predicate<T> _Predicate)
        {
            foreach (var (index,value) in _collection.LoopIndex())
            {
                if (!_Predicate(value))
                    continue;  
                yield return index;
            }
        }
        
        public static IEnumerable<int> CollectIndex<T>(this IEnumerable<T> _collection, Func<int,T,bool> _Predicate)
        {
            foreach (var (index,value) in _collection.LoopIndex())
            {
                if (!_Predicate(index,value))
                    continue;  
                yield return index;
            }
        }

        public static void Traversal<T>(this IEnumerable<T> _collection, Action<T> _onEach)
        {
            if (_collection is IList<T> list)
            {
                int count = list.Count;
                for (int i = 0; i < count; i++)
                    _onEach(list[i]);
            }
            else
            {
                foreach (T element in _collection)
                    _onEach(element);
            }
        }
        public static void Traversal<T>(this IEnumerable<T> _collection, Action<int,T> _onEach)
        {
            if (_collection is IList<T> list)
            {
                int count = list.Count;
                for (int i = 0; i < count; i++)
                    _onEach(i,list[i]);
            }
            else
            {
                int index = 0;
                foreach (T element in _collection)
                    _onEach(index++,element);
            }
        }

        public static int Count<T>(this IEnumerable<T> _collection, Predicate<T> _predicate)
        {
            int count = 0;
            foreach (var element in _collection)
            {
                if (!_predicate(element))
                    continue;
                count++;
            }

            return count;
        }
        
        public static int LastIndex<T>(this IEnumerable<T> _collection,Predicate<T> _OnEachElement )
        {
            int index = -1;
            int countIndex = -1;
            foreach (T element in _collection)
            {
                countIndex++;
                if (_OnEachElement(element))
                    index = countIndex;
            }
            return index;
        }

        public static int MinIndex<T>(this IEnumerable<T> _collection, Func<T, int> _sorting)
        {
            int minIndex = 0;
            int minValue = int.MaxValue;
            foreach (var (index,element) in _collection.LoopIndex())
            {
                int value = _sorting(element);
                if(minValue<value)
                    continue;
                minValue = value;
                minIndex = index;
            }
            return minIndex;
        }
        public static int MaxIndex<T>(this IEnumerable<T> _collection, Func<T, int> _sorting)
        {
            int maxIndex = 0;
            int maxValue = int.MinValue;
            foreach (var (index,element) in _collection.LoopIndex())
            {
                int value = _sorting(element);
                if(maxValue>value)
                    continue;
                maxValue = value;
                maxIndex = index;
            }
            return maxIndex;
        }

        public static T Last<T>(this IEnumerable<T> _collection, Func<T, Vector3> _getPosition,Vector3 _origin,bool _minimum)
        {
            T elementRecorded = default;
            float distanceRecord = _minimum?float.MaxValue:float.MinValue;
            foreach (var element in _collection)
            {
                Vector3 position = _getPosition(element);
                float sqrDistance = (position - _origin).sqrMagnitude;
                if (_minimum)
                {
                    if (sqrDistance > distanceRecord)
                        continue;
                }
                else
                {
                    if (sqrDistance < distanceRecord)
                        continue;
                }

                elementRecorded = element;
                distanceRecord = sqrDistance;
            }
            return elementRecorded;
        }
                
        public static T Find<T>(this IEnumerable<T> _collection,Predicate<T> _OnEachElement)
        {
            foreach (var element in _collection)
                if (_OnEachElement(element))
                    return element;
            return default;
        }

        public static bool TryFind<T>(this IEnumerable<T> _collection,Predicate<T> _OnEachElement, out T _element)
        {
            _element = default;
            foreach (var element in _collection)
                if (_OnEachElement(element))
                {
                    _element= element;
                    return true;
                }
            return false;
        }

        public static T Find<T>(this IEnumerable<T> _collection, Predicate<T> _validate,out int index)
        {
            index = -1;
            foreach (var element in _collection)
            {
                index++;
                if (_validate(element))
                    return element;
            }
            index = -1;
            return default;
        }
        
        public static int FindIndex<T>(this IEnumerable<T> _collection, Predicate<T> _OnEachElement)
        {
            int index = -1;
            foreach (T items in _collection)
            {
                index++;
                if (_OnEachElement(items))
                    return index;
            }
            return -1;
        }

        public static string ToString<T>(this IEnumerable<T> _collections, char breakAppend, Func<T, string> OnEachAppend=null)
        {
            StringBuilder builder = new StringBuilder();
            int maxIndex = _collections.Count() - 1;
            int curIndex = 0;
            foreach (var element in _collections)
            {
                builder.Append(OnEachAppend!=null?OnEachAppend(element):element.ToString());
                if (curIndex != maxIndex)
                    builder.Append(breakAppend);
                curIndex++;
            }
            return builder.ToString();
        }
        
        public static void FillArray<T>(this IList<T> _src, IList<T> _dst)
        {
            int length = _src.Count;
            for (int i = 0; i < length; i++)
                _dst[i] =  _src[i];
        }

        public static void FillArray<T, Y>(this IList<T> _src, IList<Y> _dst, Func<T, Y> _convert)
        {
            int length = _src.Count;
            for (int i = 0; i < length; i++)
            {
                var element = _src[i];
                _dst[i] = _convert(element);
            }
        }
        public static int FillArray<T>(this IList<T> _src, IList<T> _dst, Predicate<T> _validate) 
        {
            int index = 0;
            int length = _src.Count;
            for (int i = 0; i < length; i++)
            {
                var element = _src[i];
                if(!_validate(element))
                    continue;
                _dst[index++] = element;
            }
            return index;
        }        
        public static (Vector2 total,int count) Sum(this IEnumerable<Vector2> _collection)      //To Be Continued With Demical
        {
            Vector2 sum = Vector2.zero;
            int count = 0;
            foreach (var element in _collection)
            {
                sum += element;
                count += 1;
            }
            return (sum, count);
        }
        public static (Vector2 value, int count) Sum<T>(this IEnumerable<T> _collection, Func<T, Vector2> _OnEachElement)      //To Be Continued With Demical
        {
            Vector2 sum = Vector2.zero;
            int count = 0;
            foreach (var element in _collection)
            {
                sum += _OnEachElement(element);
                count += 1;
            }
            return (sum, count);
        }

        public static Vector2 Average<T>(this IEnumerable<T> _collection, Func<T, Vector2> _OnEachElement)      //To Be Continued With Demical
        {
            var tuple = _collection.Sum(_OnEachElement);
            return tuple.value / Mathf.Max(tuple.count,1);
        }
        public static Vector2 Average(this IEnumerable<Vector2> _collection)      //To Be Continued With Demical
        {
            var tuple = _collection.Sum();
            return tuple.total / Mathf.Max(tuple.count,1);
        }

        public static Vector3 Average(this IEnumerable<Vector3> _collection)      //To Be Continued With Demical
        {
            Vector3 sum = Vector3.zero;
            int index = 0;
            foreach (var element in _collection)
            {
                sum += element;
                index++;
            }
            return index==0?Vector3.zero:sum / index;
        }
        
        public static T Closest<T>(this IEnumerable<T> _collection,Vector3 _targetPoint,Func<T,Vector3> _onEachElement)
        {
            T target = default;
            float maxSQRdst = float.MaxValue;
            foreach (var element in _collection)
            {
                var pos = _onEachElement(element);
                float sqrDst = (_targetPoint - pos).sqrMagnitude;
                if (maxSQRdst > sqrDst)
                {
                    maxSQRdst = sqrDst;
                    target = element;
                }
            }
            return target;
        }
    #endregion
    #region Array
    public static IEnumerable<object> GetEnumerable(this Array _array)
    {
        foreach(var element in _array)
            yield return element;
    }
    public static T[] DeepCopy<T>(this T[] _srcArray)
    {
        T[] dstArray = new T[_srcArray.Length];
        for (int i = 0; i < _srcArray.Length; i++)
            dstArray[i] = _srcArray[i];
        return dstArray;
    }

    public static T[] MemberCopy<T>(this T[] _srcArray, T[] _dstArray)
    {
        for (int i = 0; i < _srcArray.Length; i++)
            _dstArray[i] = _srcArray[i];
        return _dstArray;
    }
    public static T[] Add<T>(this T[] _srcArray,T _element)
    {
        T[] newArray= new T[_srcArray.Length + 1];
        for (int i = 0; i < _srcArray.Length; i++)
            newArray[i] = _srcArray[i];
        newArray[_srcArray.Length] = _element;
        return newArray;
    }
    
    public static T[] Add<T>(this T[] _srcArray, T[] _tarArray)
    {
        int srcLength = _srcArray.Length;
        int tarLength = _tarArray.Length;
        T[] newArray = new T[srcLength + tarLength];
        for (int i = 0; i < srcLength; i++)
            newArray[i] = _srcArray[i];
        for (int i = 0; i < tarLength; i++)
            newArray[srcLength + i] = _tarArray[i];
        return newArray;
    }
    
    public static T[] RemoveLast<T>(this T[] _srcArray)
    {
        T[] dstArray = new T[_srcArray.Length-1];
        for (int i = 0; i < dstArray.Length; i++)
            dstArray[i] = _srcArray[i];
        return dstArray;
    }
    
    public static T[] Resize<T>(this T[] _srcArray,int _length,bool _fillWithLast=true)
    {
        T[] dstArray = new T[_length];
        if (_srcArray.Length == 0)
            return dstArray;
        
        for (int i = 0; i < dstArray.Length; i++)
        {
            if (i >= _srcArray.Length && !_fillWithLast)
                break;
            dstArray[i] = _srcArray[Mathf.Min(i, Mathf.Max(_srcArray.Length - 1))];
        }
        return dstArray;
    }
    
    public static T Sum<T>( this T[] _collection,Func<T,T,T> _add) where T:struct
    {
        T sum = default;
        foreach (T element in _collection)
            sum = _add(sum, element);
        return sum;
    }

    public static T Average<T>( this T[] _collection,Func<T,T,T> _add,Func<T,int,T> _divide) where T:struct
    {
        return _divide(Sum(_collection,_add), _collection.Count());
    }
    public static bool Contains<T>(this T[] _collection, T _comparer) where T:IEquatable<T>
    {
        foreach (T element in _collection)
            if (element.Equals(_comparer))
                return true;
        return false;
    }
    public static bool All<T>(this T[] _collection, Predicate<T> _comparer)
    {
        foreach (T element in _collection)
            if (!_comparer(element))
                return false;
        return true;
    }
    public static bool Any<T>(this T[] _collection, Predicate<T> _comparer)
    {
        foreach (T element in _collection)
            if (_comparer(element))
                return true;
        return false;
    }
    
    public static void SortIndex<T>(this T[] _array, IEnumerable<int> _indexes)
    {
        TSPoolList<T>.Spawn(out var tempList);
        foreach (var index in _indexes)
            tempList.Add(_array[index]);

        for (int i = 0; i < tempList.Count; i++)
            _array[i] = tempList[i];
        TSPoolList<T>.Recycle(tempList);
    }

    public static T LastElement<T>(this T[] _array)=> _array[_array.Length - 1];
    public static void FillArray<T>(this IEnumerable<T> _collection, T[] _array)
    {
        int index=0;
        foreach (var element in _collection)
            _array[index++] = element;
    }
    #endregion
    #region List
    public static List<T> DeepCopy<T>(this List<T> _list)
    {
        List<T> copyList = new List<T>(_list.Count);
        copyList.AddRange(_list);
        return copyList;
    }
    
    public static T LastElement<T>(this List<T> _list) => _list[_list.Count - 1];
    
    public static void RemoveLast<T>(this List<T> _list) => _list.RemoveAt(_list.Count - 1);
    
    public static void RemoveRange<T>(this List<T> _list, IEnumerable<T> _collections, bool containsCheck = false)
    {
        foreach (T element in _collections)
        {
            if(containsCheck&&!_list.Contains(element))
                continue;
            _list.Remove(element);
        }
    }
    
    public static void RemoveRange<T>(this List<T> _list, T[] _array,int length)
    {
        for(int i=0;i<length;i++)
            _list.Remove(_array[length]);
    }

    public static bool TryAdd<T>(this List<T> _list, T _element)
    {
        if (_list.Contains(_element))
            return false;
        _list.Add(_element);
        return true;
    }
    
    public static void TryAddRange<T>(this List<T> _list, IEnumerable<T> _collection)
    {
        foreach (var element in _collection)
            _list.TryAdd(element);
    }

    public static bool TryRemove<T>(this List<T> _list, T _element)
    {
        if (!_list.Contains(_element))
            return false;
        _list.Remove(_element);
        return true;
    }

    public static void SortIndex<T>(this List<T> _list, IEnumerable<int> _indexes)
    {
        TSPoolList<T>.Spawn(out var tempList);
        foreach (var index in _indexes)
            tempList.Add(_list[index]);
        _list.Clear();
        _list.AddRange(tempList);
        TSPoolList<T>.Recycle(tempList);
    }
    
    public static void Reindex<T>(this List<T> _list,int _index)
    {
        for (int i = 0; i < _index; i++)
        {
            var first = _list[0];
            _list.RemoveAt(0);
            _list.Add(first);
        }
    }

    public static int FindLastIndex<T>(this List<T> _list, Comparison<T> _compare)
    {
        T current = _list[0];
        int index = 0;
        var count = _list.Count;
        for (int i = 1; i < count; i++)
        {
            var compare = _list[i];
            if(_compare(current,compare)!=1)
                continue;
            current = compare;
            index = i;
        }
        return index;
    }
    
    public static bool Contains<T>(this IList<T> _collection1, IList<T> _collection2) where T:IEquatable<T>
    {
        foreach (T element1 in _collection1)
        {
            foreach (T element2 in _collection2)
                if (element1.Equals(element2))
                    return true;
        }
        return false;
    }
    
    public static void FillList<T>(this IEnumerable<T> _collection, List<T> _list,bool _sameCheck = false)
    {
        _list.Clear();
        foreach (var element in _collection)
        {
            if (_sameCheck)
                _list.TryAdd(element);
            else
                _list.Add(element);
        }
    }

    #endregion
    #region Dictionary

    public static T SelectKey<T, Y>(this Dictionary<T, Y> dictionary, int index) => dictionary.ElementAt(index).Key;
    
    public static Y SelectValue<T, Y>(this Dictionary<T, Y> dictionary, int index) => dictionary.ElementAt(index).Value;
    
    public static Dictionary<T, Y> DeepCopy<T, Y>(this Dictionary<T, Y> dictionary)
    {
        Dictionary<T, Y> copyDic = new Dictionary<T, Y>();
        foreach (var pair in copyDic)
            copyDic.Add(pair.Key,pair.Value);
        return copyDic;
    }
    
    public static void RemoveRange<T, Y>(this Dictionary<T, Y> _dic, IEnumerable<T> _keysCollections,
        bool containsCheck = false)
    {
        foreach (T key in _keysCollections)
        {
            if (containsCheck && !_dic.ContainsKey(key))
                continue;
            _dic.Remove(key);
        }
    }

    public static bool TryAdd<T, Y>(this Dictionary<T, Y> _dic, T _key, Y _value)
    {
        if (_dic.ContainsKey(_key))
            return false;
        _dic.Add(_key,_value);
        return true;
    }
    public static bool TryAdd<T, Y>(this Dictionary<T, Y> _dic, T _key, Func<Y> _getValue)
    {
        if (_dic.ContainsKey(_key))
            return false;
        _dic.Add(_key,_getValue());
        return true;
    }
    
    public static bool TryRemove<T, Y>(this Dictionary<T, Y> _dic,  T _key)
    {
        if (!_dic.ContainsKey(_key))
            return false;
        _dic.Remove(_key);
        return true;
    }
    public static IEnumerable<(T key, Y value)> SelectPairs<T,Y>(this Dictionary<T,Y> dictionary)
    {
        foreach (var pair in dictionary)
            yield return (pair.Key, pair.Value);
    }
    #endregion
    #region Queue
    public static void EnqueueRange<T>(this Queue<T> _queue,IEnumerable<T> _collection)
    {
        foreach (var element in _collection)
            _queue.Enqueue(element);
    }
    #endregion
    #region Stack
    public static void PushRange<T>(this Stack<T> _stack,IEnumerable<T> _collection)
    {
        foreach (var element in _collection)
            _stack.Push(element);
    }

    public static bool TryPush<T>(this Stack<T> _stack, T _item)
    {
        if (_stack.Contains(_item))
            return false;
        _stack.Push(_item);
        return true;
    }
    #endregion
    #region HashSet

    public static void AddRange<T>(this HashSet<T> _hashSet, IEnumerable<T> _collections)
    {
        foreach (var element in _collections)
            _hashSet.Add(element);
    }  

    
    public static bool TryAdd<T>(this HashSet<T> _hashSet, T _element)
    {
        if (_hashSet.Contains(_element))
            return false;

        _hashSet.Add(_element);
        return true;
    }  
    
    
    public static void TryAddRange<T>(this HashSet<T> _hashSet,IEnumerable<T> _collections)
    {
        foreach (var element in _collections)
            _hashSet.TryAdd(element);
    }  
    public static bool TryRemove<T>(this HashSet<T> _hashSet, T _element)
    {
        if (!_hashSet.Contains(_element))
            return false;

        _hashSet.Remove(_element);
        return true;
    }
    #endregion
}
