using System;
using System.Collections;
using System.Collections.Generic;
using Geometry.Voxel;
using UnityEngine;
namespace Geometry.Index
{
    [Serializable]
    public struct GQuadIndex:IEnumerable<int>,IIterate<int>
    {
        public int index0 { get; set; }
        public int index1 { get; set; }
        public int index2 { get; set; }
        public int index3 { get; set; }

        public GQuadIndex(int _index0, int _index1, int _index2, int _index3)
        {
            index0 = _index0;
            index1 = _index1;
            index2 = _index2;
            index3 = _index3;
        }

        public (T v0, T v1, T v2, T v3) GetVertices<T>(IList<T> _vertices) => (_vertices[index0], _vertices[index1],_vertices[index2],_vertices[index3]);
        public (Y v0, Y v1, Y v2, Y v3) GetVertices<T,Y>(IList<T> _vertices, Func<T, Y> _getVertex) => ( _getVertex( _vertices[index0]), _getVertex(_vertices[index1]),_getVertex(_vertices[index2]),_getVertex(_vertices[index3]));
        
        public int Length => 4;
        public IEnumerable<T> GetEnumerator<T>(IList<T> _vertices)
        {
            yield return _vertices[index0];
            yield return _vertices[index1];
            yield return _vertices[index2];
            yield return _vertices[index3];
        }
        public IEnumerator<int> GetEnumerator()
        {
            yield return index0;
            yield return index1;
            yield return index2;
            yield return index3;
        }

        IEnumerator IEnumerable.GetEnumerator()=>GetEnumerator();

        public int GetElement(int index)
        {
            switch (index)
            {
                default: throw new Exception("Invalid Index:" + index);
                case 0: return index0;
                case 1: return index1;
                case 2: return index2;
                case 3: return index3;
            }
        }
    }
    [Serializable]
    public struct GTriangleIndex:IEnumerable<int>,IIterate<int>
    {
        public int index0;
        public int index1;
        public int index2;

        public GTriangleIndex(int _index0, int _index1, int _index2)
        {
            index0 = _index0;
            index1 = _index1;
            index2 = _index2;
        }
        public (T v0, T v1, T v2) GetVertices<T>(IList<T> _vertices) => (_vertices[index0], _vertices[index1],_vertices[index2]);
        public (Y v0, Y v1, Y v2) GetVertices<T,Y>(IList<T> _vertices, Func<T, Y> _getVertex) => ( _getVertex( _vertices[index0]), _getVertex(_vertices[index1]),_getVertex( _vertices[index2]));
        public IEnumerable<T> GetEnumerator<T>(IList<T> _vertices)
        {
            yield return _vertices[index0];
            yield return _vertices[index1];
            yield return _vertices[index2];
        }
        public IEnumerator<int> GetEnumerator()
        {
            yield return index0;
            yield return index1;
            yield return index2;
        }

        IEnumerator IEnumerable.GetEnumerator()=>GetEnumerator();

        public int Length => 3;
        public int GetElement(int index)
        {
            switch (index)
            {
                default: throw new Exception("Invalid Index:" + index);
                case 0: return index0;
                case 1: return index1;
                case 2: return index2;
            }
        }

    }
}