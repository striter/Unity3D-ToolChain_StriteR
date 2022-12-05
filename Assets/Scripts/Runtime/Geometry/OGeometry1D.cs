using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry
{
    [Serializable]
    public struct PQuad:IQuad<int>, IEnumerable<int>,IIterate<int>
    {
        public Quad<int> quad;
        public PQuad(Quad<int> _quad) { quad = _quad;  }
        public PQuad(int _index0, int _index1, int _index2, int _index3):this(new Quad<int>(_index0,_index1,_index2,_index3)){}
        public static explicit operator PQuad(Quad<int> _src) => new PQuad(_src);
        
        public (T v0, T v1, T v2, T v3) GetVertices<T>(IList<T> _vertices) => (_vertices[B], _vertices[L],_vertices[F],_vertices[R]);
        public (Y v0, Y v1, Y v2, Y v3) GetVertices<T,Y>(IList<T> _vertices, Func<T, Y> _getVertex) => ( _getVertex( _vertices[B]), _getVertex(_vertices[L]),_getVertex(_vertices[F]),_getVertex(_vertices[R]));
        
        public int Length => 4;
        public IEnumerable<T> GetEnumerator<T>(IList<T> _vertices)
        {
            yield return _vertices[quad[0]];
            yield return _vertices[quad[1]];
            yield return _vertices[quad[2]];
            yield return _vertices[quad[3]];
        }

        public IEnumerator<int> GetEnumerator() => quad.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()=>GetEnumerator();

        public int this[int _index] => quad[_index];
        public int this[EQuadCorner _corner] => quad[_corner];

        public int B => quad.B;
        public int L => quad.L;
        public int F => quad.F;
        public int R => quad.R;
    }
    
    [Serializable]
    public struct PTriangle:ITriangle<int>,IEnumerable<int>,IIterate<int>
    {
        public Triangle<int> triangle;
        public PTriangle(Triangle<int> _triangle) { triangle = _triangle;  }
        public PTriangle(int _index0, int _index1, int _index2):this(new Triangle<int>(_index0,_index1,_index2)){}
        public static explicit operator PTriangle(Triangle<int> _src) => new PTriangle(_src);
        
        public (T v0, T v1, T v2) GetVertices<T>(IList<T> _vertices) => (_vertices[V0], _vertices[V1],_vertices[V2]);
        public (Y v0, Y v1, Y v2) GetVertices<T,Y>(IList<T> _vertices, Func<T, Y> _getVertex) => ( _getVertex( _vertices[V0]), _getVertex(_vertices[V1]),_getVertex( _vertices[V2]));
        public IEnumerable<T> GetEnumerator<T>(IList<T> _vertices)
        {
            yield return _vertices[V0];
            yield return _vertices[V1];
            yield return _vertices[V2];
        }

        public IEnumerator<int> GetEnumerator() => triangle.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()=>GetEnumerator();

        public int Length => 3;
        public int this[int _index] => triangle[_index];

        public int V0 => triangle.v0;
        public int V1 => triangle.v1;
        public int V2 => triangle.v2;
    }
    

}