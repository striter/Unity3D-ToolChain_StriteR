using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = System.Random;

public static class UCollection
{
    #region Linq
    public static void Traversal<T>(this IEnumerable<T> _collection, Action<T> _OnEach)
    {
        foreach (T element in _collection)
            _OnEach(element);
    }
    
    public static IEnumerable<(T value, int index)> LoopIndex<T>(this IEnumerable<T> _collection)
    {
        int index = 0;
        foreach (T element in _collection)
            yield return (element,index++);
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
    
    public static T Find<T>(this IEnumerable<T> _collection,Predicate<T> _OnEachElement)
    {
        foreach (var element in _collection)
            if (_OnEachElement(element))
                return element;
        return default;
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

    
    public static IEnumerable<T> Collect<T>(this IEnumerable<T> _collection, Predicate<T> _Predicate)
    {
        foreach (T element in _collection)
        {
            if(!_Predicate(element))
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


    public static string ToString<T>(this IEnumerable<T> _collections, char breakAppend, Func<T, string> OnEachAppend=null)
    {
        StringBuilder builder = new StringBuilder();
        int maxIndex = _collections.Count() - 1;
        foreach ((T item,int index) in _collections.LoopIndex())
        {
            builder.Append(OnEachAppend!=null?OnEachAppend(item):item.ToString());
            if (index != maxIndex)
                builder.Append(breakAppend);
        }
        return builder.ToString();
    }

    public static void FillList<T>(this IEnumerable<T> _collections, List<T> _list)
    {
        _list.Clear();
        foreach (var element in _collections)
            _list.Add(element);
    }
    public static Vector2 Average<T>(this IEnumerable<T> _collection, Func<T, Vector2> _OnEachElement)      //To Be Continued With Demical
    {
        Vector2 sum = Vector2.zero;
        int index = 0;
        foreach (var element in _collection)
        {
            sum += _OnEachElement(element);
            index++;
        }
        return index==0?Vector2.zero:sum / index;
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

    public static T Average<T>( this IEnumerable<T> _collection,Func<T,T,T> _add,Func<T,int,T> _divide) where T:struct
    {
        T sum = default;
        int count = 0;
        foreach (T element in _collection)
        {
            sum = _add(sum, element);
            count++;
        }
        return _divide(sum, count);
    }
    
    public static bool Contains<T>(this IEnumerable<T> _collection1, IEnumerable<T> _collection2)
    {
        foreach (T element1 in _collection1)
        {
            foreach (T element2 in _collection2)
                if (element1.Equals(element2))
                    return true;
        }

        return false;
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
        for (int i = 0; i < dstArray.Length; i++)
        {
            if (i >= _srcArray.Length && !_fillWithLast)
                break;
            dstArray[i] = _srcArray[Mathf.Min(i, Mathf.Max(_srcArray.Length - 1))];
        }
        return dstArray;
    }
    #endregion
    #region List
    public static List<T> DeepCopy<T>(this List<T> _list)
    {
        List<T> copyList = new List<T>();
        copyList.AddRange(_list);
        return copyList;
    }
    
    public static void RemoveRange<T>(this List<T> _list, IEnumerable<T> _collections, bool containsCheck = false)
    {
        foreach (T element in _collections)
        {
            if(containsCheck&&!_list.Contains(element))
                continue;
            _list.Remove(element);
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
    #endregion
    
}
