using System;
using System.Collections.Generic;
public interface IIterate<T>
{
    T this[int _index] { get; }
    int Length { get; }
}
public static class UIterate
{
    #region List

    static class ListStorage<T>
    {
        public static List<T> m_List = new List<T>();
    }

    public static List<T> EmptyList<T>(IEnumerable<T> _src)
    {
        var list = ListStorage<T>.m_List;
        list.Clear();
        return list;
    }
    public static List<T> Iterate<T>(IEnumerable<T> _src)
    {
        var list = ListStorage<T>.m_List;
        list.Clear();
        list.AddRange(_src);
        return list;
    }
    
    #endregion
    
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

    public static bool IterateFindIndex<T>(this IIterate<T> _src, T _predicate, out int _index)
    {
        _index = IterateFindIndex(_src, _predicate);
        return _index != -1;
    }
    
    public static int IterateFindIndex<T>(this IIterate<T> _src,T _predicate)
    {
        int length = _src.Length;
        for(int i=0;i<length;i++)
            if (_predicate.Equals(_src[i]))
                return i;
        return -1;
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


    public static T[] IterateToArray<T>(this IIterate<T> _iterate)
    {
        int length = _iterate.Length;
        T[] array = new T[length];
        for(int i=0;i<length;i++)
            array[i]=_iterate[i];
        return array;
    }
}