using System;
using System.Collections;
using System.Collections.Generic;

namespace Runtime.Geometry
{
    public partial struct PQuad
    {
        public Quad<int> quad;
        public PQuad(Quad<int> _quad) { quad = _quad;  }
        public PQuad(int _index0, int _index1, int _index2, int _index3):this(new Quad<int>(_index0,_index1,_index2,_index3)){}
        public static readonly PQuad kDefault = new PQuad(0, 1, 2, 3);
    }

    
    [Serializable]
    public partial struct PQuad : IQuad<int>, IEnumerable<int>, IIterate<int>
    {
        public static explicit operator PQuad(Quad<int> _src) => new PQuad(_src);
        public static PQuad operator +(PQuad _src,int _add) => new PQuad(_src.B+_add,_src.L + _add,_src.F + _add,_src.R + _add);
        public static PQuad operator -(PQuad _src,int _min) => new PQuad(_src.B+_min,_src.L + _min,_src.F + _min,_src.R + _min);
        public (T v0, T v1, T v2, T v3) GetVertices<T>(IList<T> _vertices) => (_vertices[B], _vertices[L],_vertices[F],_vertices[R]);
        public (Y v0, Y v1, Y v2, Y v3) GetVertices<T,Y>(IList<T> _vertices, Func<T, Y> _getVertex) => ( _getVertex( _vertices[B]), _getVertex(_vertices[L]),_getVertex(_vertices[F]),_getVertex(_vertices[R]));
        
        public readonly IEnumerable<int> GetTriangleIndexes()
        {
            yield return quad.B;
            yield return quad.L;
            yield return quad.F;
            
            yield return quad.B;
            yield return quad.F;
            yield return quad.R;
        }

        
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

}