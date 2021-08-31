using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Procedural.Hexagon.Geometry
{
    public static class IterateArrayStorage<T>
    {
        public static T[] m_Array=new T[0];

        public static void CheckLength(int length)
        {
            if (IterateArrayStorage<T>.m_Array.Length != length)
                IterateArrayStorage<T>.m_Array = new T[length];
        }
    }
    public static class IterateArrayHelper
    {
        public static T[] ConstructIteratorArray<T>(this IEnumerable<T> helper,int _length)
        {
            IterateArrayStorage<T>.CheckLength(_length);
            foreach ((T value, int index) tuple in helper.LoopIndex())
                IterateArrayStorage<T>.m_Array[ tuple.index] = tuple.value;
            return IterateArrayStorage<T>.m_Array;
        }
        public static Y[] ConstructIteratorArray<T,Y>(this IEnumerable<T> helper,Func<T,Y> _convert,int _length)
        {
            IterateArrayStorage<Y>.CheckLength(_length);
            foreach ((T value, int index) tuple in helper.LoopIndex())
                IterateArrayStorage<Y>.m_Array[tuple.index] = _convert(tuple.value);
            return IterateArrayStorage<Y>.m_Array;
        }
    }
    [Serializable]
    public struct HexTriangle: IEquatable<HexTriangle>,IEnumerable<PHexCube>
    {
        public readonly PHexCube vertex0;
        public readonly PHexCube vertex1;
        public readonly PHexCube vertex2;
        public HexTriangle(PHexCube _vertex0,PHexCube _vertex1,PHexCube _vertex2) : this()
        {
            vertex0 = _vertex0;
            vertex1 = _vertex1;
            vertex2 = _vertex2;
        }
        public PHexCube this[int index]
        {
            get
            {
                switch (index)
                {
                    default: throw new Exception("Invalid Index:" + index);
                    case 0: return vertex0;
                    case 1: return vertex1;
                    case 2: return vertex2;
                }
            }
        }
        public bool Equals(HexTriangle other) => vertex0 == other.vertex0 && vertex1 == other.vertex1 && vertex2 == other.vertex2;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<PHexCube> GetEnumerator()
        {
            yield return vertex0;
            yield return vertex1;
            yield return vertex2;
        }
    }

    [Serializable]
    public struct HexQuad:IEquatable<HexQuad>,IEnumerable<PHexCube>
    {
        public readonly PHexCube vertex0;
        public readonly PHexCube vertex1;
        public readonly PHexCube vertex2;
        public readonly PHexCube vertex3;
        public HexQuad(PHexCube _vertex0,PHexCube _vertex1,PHexCube _vertex2,PHexCube _vertex3):this()
        {
            vertex0 = _vertex0;
            vertex1 = _vertex1;
            vertex2 = _vertex2;
            vertex3 = _vertex3;
        }
        public PHexCube this[int _index]
        {
            get
            {
                switch (_index)
                {
                    default:throw new Exception("Invalid Index:" + _index);
                    case 0: return vertex0;
                    case 1: return vertex1;
                    case 2: return vertex2;
                    case 3: return vertex3;
                }
            }
        }
        public bool Equals(HexQuad other) => vertex0 == other.vertex0 && vertex1 == other.vertex1 && vertex2 == other.vertex2&&vertex3==other.vertex3;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<PHexCube> GetEnumerator() 
        {
            yield return vertex0;
            yield return vertex1;
            yield return vertex2;
            yield return vertex3;
        }

    }
}