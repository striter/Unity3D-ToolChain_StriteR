using System;
using System.Collections.Generic;

public static class UIterate
{
    #region Array
    static class ArrayStorage<T>
    {
        public static T[] m_Array=Array.Empty<T>();
        public static void CheckLength(int length)
        {
            if (m_Array.Length != length)
                m_Array = new T[length];
        }
    }
    public static T[] Iterate<T>(this IIterate<T> helper)
    {
        ArrayStorage<T>.CheckLength(helper.Length);
        for (int i = 0; i < helper.Length; i++)
            ArrayStorage<T>.m_Array[ i] = helper[i];
        return ArrayStorage<T>.m_Array;
    }
    public static Y[] Iterate<T,Y>(this IIterate<T> helper,Func<T,Y> _convert)
    {
        ArrayStorage<Y>.CheckLength(helper.Length);
        for(int i=0;i<helper.Length;i++)
            ArrayStorage<Y>.m_Array[i] = _convert(helper[i]);
        return ArrayStorage<Y>.m_Array;
    }
    #endregion

    public static void Iterate<T>(this IIterate<T> _src, Action<T> _OnEach)
    {
        int length = _src.Length;
        for(int i=0;i<length;i++)
            _OnEach(_src[i]);
    }
    
    public static void Iterate<T>(this IIterate<T> _src, Action<int,T> _OnEach)
    {
        int length = _src.Length;
        for(int i=0;i<length;i++)
            _OnEach(i,_src[i]);
    }

    public static T IterateFind<T>(this IIterate<T> _src,Predicate<T> _predicate)
    {
        int length = _src.Length;
        for(int i=0;i<length;i++)
        {
            var element = _src[i];
            if (_predicate(_src[i]))
                return element;
        }
        return default;
    }
    
    public static int IterateFindIndex<T>(this IIterate<T> _src,Predicate<T> _predicate)
    {
        int length = _src.Length;
        for(int i=0;i<length;i++)
            if (_predicate(_src[i]))
                return i;
        return -1;
    }

    public static bool IterateContains<T>(this IIterate<T> _src, T _element) 
    {
        int length = _src.Length;
        for(int i=0;i<length;i++)
            if (_src[i].Equals(_element))
                return true;
        return false;
    }
    public static bool IterateAny<T>(this IIterate<T> _src, Predicate<T> _validate)
    {
        int length = _src.Length;
        for(int i=0;i<length;i++)
            if (_validate(_src[i]))
                return true;
        return false;
    }
    
    public static bool IterateAll<T>(this IIterate<T> _src, Predicate<T> _validate)
    {
        int length = _src.Length;
        for(int i=0;i<length;i++)
            if (!_validate(_src[i]))
                return false;
        return true;
    }
    
    public static void IterateAddRange<T>(this IList<T> _src,IIterate<T> _iterate)
    {
        int length = _iterate.Length;
        for(int i=0;i<length;i++)
            _src.Add(_iterate[i]);
    }

    public static void AddRange<T>(this IList<T> _src, IList<T> _dst)
    {
        int count = _dst.Count;
        for (int i = 0; i < count; i++)
            _src.Add(_dst[i]);
    }
    
    public static T[] IterateToArray<T>(this IIterate<T> _iterate)
    {
        int length = _iterate.Length;
        T[] array = new T[length];
        for(int i=0;i<length;i++)
            array[i]=_iterate[i];
        return array;
    }
}