using System;
using System.Runtime.CompilerServices;
using Procedural.Hexagon.Geometry;

public static class UIterate
{
    #region Array
    static class ArrayStorage<T>
    {
        public static T[] m_Array=new T[0];
        public static void CheckLength(int length)
        {
            if (m_Array.Length != length)
                m_Array = new T[length];
        }
    }
    public static T[] ConstructIteratorArray<T>(this IIterate<T> helper)
    {
        ArrayStorage<T>.CheckLength(helper.Length);
        for (int i = 0; i < helper.Length; i++)
            ArrayStorage<T>.m_Array[ i] = helper.GetElement(i);
        return ArrayStorage<T>.m_Array;
    }
    public static Y[] ConstructIteratorArray<T,Y>(this IIterate<T> helper,Func<T,Y> _convert)
    {
        ArrayStorage<Y>.CheckLength(helper.Length);
        for(int i=0;i<helper.Length;i++)
            ArrayStorage<Y>.m_Array[i] = _convert(helper.GetElement(i));
        return ArrayStorage<Y>.m_Array;
    }
    #endregion

    public static void Traversal<T>(this IIterate<T> _src, Action<T> _OnEach)
    {
        int length = _src.Length;
        for(int i=0;i<length;i++)
            _OnEach(_src.GetElement(i));
    }
    
    public static void Traversal<T>(this IIterate<T> _src, Action<int,T> _OnEach)
    {
        int length = _src.Length;
        for(int i=0;i<length;i++)
            _OnEach(i,_src.GetElement(i));
    }

    public static T Find<T>(this IIterate<T> _src,Predicate<T> _predicate)
    {
        int length = _src.Length;
        for(int i=0;i<length;i++)
        {
            var element = _src.GetElement(i);
            if (_predicate(_src.GetElement(i)))
                return element;
        }
        return default;
    }
    
    public static int FindIndex<T>(this IIterate<T> _src,Predicate<T> _predicate)
    {
        int length = _src.Length;
        for(int i=0;i<length;i++)
            if (_predicate(_src.GetElement(i)))
                return i;
        return -1;
    }

    public static bool Contains<T>(this IIterate<T> _src, T _element) where T:IEquatable<T>
    {
        int length = _src.Length;
        for(int i=0;i<length;i++)
            if (_src.GetElement(i).Equals(_element))
                return true;
        return false;
    }
    public static bool Any<T>(this IIterate<T> _src, Predicate<T> _validate)
    {
        int length = _src.Length;
        for(int i=0;i<length;i++)
            if (_validate(_src.GetElement(i)))
                return true;
        return false;
    }
    public static bool All<T>(this IIterate<T> _src, Predicate<T> _validate)
    {
        int length = _src.Length;
        for(int i=0;i<length;i++)
            if (!_validate(_src.GetElement(i)))
                return false;
        return true;
    }
}