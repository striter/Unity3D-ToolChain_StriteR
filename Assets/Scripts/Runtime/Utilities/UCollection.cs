using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class UCollection
{

    public static T Find<T>(this T[] _array, Predicate<T> _predicate)
    {
        for (int i = 0; i < _array.Length; i++)
            if (_predicate(_array[i]))
                return _array[i];
        return default;
    }
    public static T GetIndexKey<T, Y>(this Dictionary<T, Y> dictionary, int index) => dictionary.ElementAt(index).Key;
    public static Y GetIndexValue<T, Y>(this Dictionary<T, Y> dictionary, int index) => dictionary.ElementAt(index).Value;
    public static List<T> DeepCopy<T>(this List<T> list)
    {
        List<T> copyList = new List<T>();
        list.Traversal((T value) => { copyList.Add(value); });
        return copyList;
    }
    public static Dictionary<T, Y> DeepCopy<T, Y>(this Dictionary<T, Y> dictionary)
    {
        Dictionary<T, Y> copyDic = new Dictionary<T, Y>();
        dictionary.Traversal((T key, Y value) => { copyDic.Add(key, value); });
        return copyDic;
    }
    public static Dictionary<T, List<Y>> DeepCopy<T, Y>(this Dictionary<T, List<Y>> dictionary) where T : struct where Y : struct
    {
        Dictionary<T, List<Y>> copyDic = new Dictionary<T, List<Y>>();
        dictionary.Traversal((T key, List<Y> value) => { copyDic.Add(key, value.DeepCopy()); });
        return copyDic;
    }
    public static T[] Copy<T>(this T[] _srcArray)
    {
        T[] dstArray = new T[_srcArray.Length];
        for (int i = 0; i < _srcArray.Length; i++)
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

    public static int LastIndex<T>(this IEnumerable<T> _ienumerable,Predicate<T> OnEachItem )
    {
        int index = -1;
        int countIndex = -1;
        foreach (T item in _ienumerable)
        {
            countIndex++;
            if (OnEachItem(item))
                index = countIndex;
        }
        return index;
    }

    public static int FindIndex<T>(this IEnumerable<T> _ienumerable, Predicate<T> OnEachItem)
    {
        int index = -1;
        foreach (T item in _ienumerable)
        {
            index++;
            if (OnEachItem(item))
                return index;
        }
        return -1;
    }
    public static T Find<T>(this IEnumerable<T> _ienumerable,Predicate<T> OnEachItem)
    {
        foreach (var item in _ienumerable)
            if (OnEachItem(item))
                return item;
        return default;
    }
    public static List<T> FindAll<T>(this IEnumerable<T> _ienumerable,Predicate<T> OnEachItem)
    {
        List<T> items = new List<T>();
        foreach(T item in _ienumerable)
        {
            if (OnEachItem(item))
                items.Add(item);
        }
        return items;
    }
    public static List<Y> FindAll<T,Y>(this IEnumerable<T> _ienumerable, Func<T,Y> OnEachItem) where T:class  where Y:class
    {
        List<Y> transformItems = new List<Y>();
        foreach (T item in _ienumerable)
        {
            Y newItem = OnEachItem(item);
            if (newItem!=null)
                transformItems.Add(newItem);
        }
        return transformItems;
    }
    public static void FindAllIndexes<T>(this IEnumerable<T> _ienumerable, List<int> _indexList, Predicate<T> OnEachItem)
    {
        _indexList.Clear();
        int index = -1;
        foreach (T item in _ienumerable)
        {
            index++;
            if (OnEachItem(item))
                _indexList.Add(index);
        }
    }
    public static void FindAllIndexes<T>(this IEnumerable<T> _ienumerable, List<int> _indexList, Func<int, T, bool> OnEachItem)
    {
        _indexList.Clear();
        int index = -1;
        foreach (T item in _ienumerable)
        {
            index++;
            if (OnEachItem(index, item))
                _indexList.Add(index);
        }
    }
    public static List<int> FindAllIndexes<T>(this IEnumerable<T> _ienumerable, Predicate<T> OnEachItem)
    {
        List<int> items = new List<int>();
        int index = -1;
        foreach (T item in _ienumerable)
        {
            index++;
            if (OnEachItem(item))
                items.Add(index);
        }
        return items;
    }
    public static void EnqueueRange<T>(this Queue<T> _queue,IEnumerable<T> _ienumerable)
    {
        foreach (var item in _ienumerable)
            _queue.Enqueue(item);
    }
    public static void PushRange<T>(this Stack<T> _stack,IEnumerable<T> _ienumerable)
    {
        foreach (var item in _ienumerable)
            _stack.Push(item);
    }
    public static void Traversal<T>(this IEnumerable<T> _ienumerable, Action<T> OnEachItem)
    {
        foreach (T item in _ienumerable)
            OnEachItem(item);
    }
    public static void Traversal<T>(this IEnumerable<T> _ienumerable, Action<int, T> OnEachItem)
    {
        int index = 0;
        foreach (T item in _ienumerable)
            OnEachItem(index++, item);
    }

    public static void TraversalMark<T>(this List<T> targetList, Predicate<T> DoMarkup, Action<T> OnEachMarkup)
    {
        List<T> markupList = new List<T>();
        foreach (T item in targetList)
        {
            if (!DoMarkup(item))
                continue;
            markupList.Add(item);
        }
        markupList.Traversal(item => OnEachMarkup(item));
    }
    public static void TraversalBreak<T>(this IEnumerable<T> _numerable, Predicate<T> OnEachItemBreak)
    {
        foreach (T item in _numerable)
            if (OnEachItemBreak(item))
                break;
    }
    public static void Traversal<T, Y>(this Dictionary<T, Y> dic, Action<T> OnEachKey)
    {
        foreach (T temp in dic.Keys)
            OnEachKey(temp);
    }
    public static void Traversal<T, Y>(this Dictionary<T, Y> dic, Action<Y> OnEachValue)
    {
        foreach (T temp in dic.Keys)
            OnEachValue(dic[temp]);
    }
    public static void Traversal<T, Y>(this Dictionary<T, Y> dic, Action<T, Y> OnEachPair)
    {
        foreach (T temp in dic.Keys)
            OnEachPair(temp, dic[temp]);
    }
    public static void TraversalBreak<T, Y>(this Dictionary<T, Y> dic, Predicate<Y> OnEachItem)
    {
        foreach (T temp in dic.Keys)
            if (OnEachItem(dic[temp]))
                break;
    }
    public static void TraversalMark<T, Y>(this Dictionary<T, Y> dic, Predicate<Y> DoMarkup, Action<T> OnMarkup)
    {
        List<T> markKeys = new List<T>();
        foreach (T key in dic.Keys)
        {
            if (!DoMarkup(dic[key]))
                continue;
            markKeys.Add(key);
        }
        markKeys.Traversal(key => { OnMarkup(key); });
    }
    public static void Traversal<T>(this T[] array, Action<int, T> OnEachItem)
    {
        int length = array.Length;
        for (int i = 0; i < length; i++)
            OnEachItem(i, array[i]);
    }
    public static void Traversal<T>(this T[,] array, Action<T> OnEachItem)
    {
        int length0 = array.GetLength(0);
        int length1 = array.GetLength(1);
        for (int i = 0; i < length0; i++)
            for (int j = 0; j < length1; j++)
                OnEachItem(array[i, j]);
    }
    public static void Traversal<T>(this T[,] array, Action<int, int, T> OnEachItem)
    {
        int length0 = array.GetLength(0);
        int length1 = array.GetLength(1);
        for (int i = 0; i < length0; i++)
            for (int j = 0; j < length1; j++)
                OnEachItem(i, j, array[i, j]);
    }

    public static void TraversalRandomBreak<T>(this List<T> list, Func<int, bool> OnRandomItemStop = null, System.Random seed = null) => TraversalEnumerableIndex(list.RandomIndex(seed), list.Count, OnRandomItemStop);
    public static void TraversalRandomBreak<T>(this T[] array, Func<int, bool> OnRandomItemStop = null, System.Random seed = null) => TraversalEnumerableIndex(array.RandomIndex(seed), array.Length, OnRandomItemStop);
    public static void TraversalRandomBreak<T>(this List<T> list, Func<T, bool> OnRandomItemStop = null, System.Random seed = null) => TraversalEnumerableIndex(list.RandomIndex(seed), list.Count, (int index) => OnRandomItemStop != null && OnRandomItemStop(list[index]));
    public static void TraversalRandomBreak<T>(this T[] array, Func<T, bool> OnRandomItemStop = null, System.Random seed = null) => TraversalEnumerableIndex(array.RandomIndex(seed), array.Length, (int index) => OnRandomItemStop != null && OnRandomItemStop(array[index]));
    public static void TraversalRandomBreak<T, Y>(this Dictionary<T, Y> dictionary, Func<T, Y, bool> OnRandomItemStop = null, System.Random seed = null) => TraversalEnumerableIndex(URandom.RandomInt(dictionary.Count, seed), dictionary.Count, (int index) => {
        KeyValuePair<T, Y> element = dictionary.ElementAt(index);
        return OnRandomItemStop != null && OnRandomItemStop(element.Key, element.Value);
    });
    public static void TraversalEnumerableIndex(int index, int count, Func<int, bool> OnItemBreak)
    {
        if (count == 0)
            return;
        for (int i = 0; i < count; i++)
        {
            if (OnItemBreak != null && OnItemBreak(index))
                break;

            index++;
            if (index == count)
                index = 0;
        }
    }

    public static string ToString<T>(this IEnumerable<T> inumerable, char breakAppend, Func<T, string> OnEachAppend=null)
    {
        StringBuilder builder = new StringBuilder();
        int maxIndex = inumerable.Count() - 1;
        inumerable.Traversal((int index, T temp) => {
            builder.Append(OnEachAppend!=null?OnEachAppend(temp):temp.ToString());
            if (index != maxIndex)
                builder.Append(breakAppend);
        });
        return builder.ToString();
    }

    public static void AddRange<T, Y>(this List<T> list, IEnumerable<Y> ienumerable, Func<Y, T> OnEachAddItem)
    {
        ienumerable.Traversal((item => list.Add(OnEachAddItem(item))));
    }
    public static T[] Add<T>(this T[] srcArray,T dstItem)
    {
        T[] newArray= new T[srcArray.Length + 1];
        for (int i = 0; i < srcArray.Length; i++)
            newArray[i] = srcArray[i];
        newArray[srcArray.Length] = dstItem;
        return newArray;
    }
    public static T[] Add<T>(this T[] srcArray, T[] tarArray)
    {
        int srcLength = srcArray.Length;
        int tarLength = tarArray.Length;
        T[] newArray = new T[srcLength + tarLength];
        for (int i = 0; i < srcLength; i++)
            newArray[i] = srcArray[i];
        for (int i = 0; i < tarLength; i++)
            newArray[srcLength + i] = tarArray[i];
        return newArray;
    }
    public static T[] RemoveLast<T>(this T[] srcArray)
    {
        T[] dstArray = new T[srcArray.Length-1];
        for (int i = 0; i < dstArray.Length; i++)
            dstArray[i] = srcArray[i];
        return dstArray;
    }
    public static Y[] ToArray<T, Y>(this IEnumerable<T> src, Func<T, Y> GetDstItem)
    {
        Y[] dstArray = new Y[src.Count()];
        src.Traversal((index, srcItem) => dstArray[index] = GetDstItem(srcItem));
        return dstArray;
    }
    public static Y[] ToArray<T, Y>(this IEnumerable<T> src, Func<int,T, Y> GetDstItem)
    {
        Y[] dstArray = new Y[src.Count()];
        src.Traversal((index, srcItem) => dstArray[index] = GetDstItem(index,srcItem));
        return dstArray;
    }
    public static IEnumerable<object> GetEnumerable(this Array array)
    {
        foreach(var item in array)
            yield return item;
    }

    public static Dictionary<T,List<Y>> ToListDictionary<T,Y>(this IEnumerable<Y> _items, Func<Y,T> OnEachItem)
    {
        Dictionary<T, List<Y>> dictionary = new Dictionary<T, List<Y>>();
        foreach(var item in _items)
        {
            T key = OnEachItem(item);
            if (!dictionary.ContainsKey(key))
                dictionary.Add(key, new List<Y>());
            dictionary[key].Add(item);
        }
        return dictionary;
    }
    public static List<Y> ToList<T, Y>(this IEnumerable<T> src, Func<T, Y> GetDstItem)
    {
        List<Y> dstList = new List<Y>();
        src.Traversal((index, srcItem) => dstList.Add(GetDstItem(srcItem)));
        return dstList;
    }

    public static IEnumerable<Y> TakeAll<T,Y>(this IEnumerable<T> _enumerable,Func<T,Y> _OnEachElement ) where T:class  where Y:class
    {
        foreach(var element in _enumerable)
        {
            Y item = _OnEachElement(element);
            if (item == null)
                continue;
            yield return item;
        }
    }
}
