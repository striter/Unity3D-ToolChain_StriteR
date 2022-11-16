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
    
    [Serializable]
    public struct GTriangle:ITriangle<Vector3>, IIterate<Vector3>,ISerializationCallbackReceiver
    {
        public Triangle<Vector3> triangle;
        [NonSerialized] public Vector3 normal;
        [NonSerialized] public Vector3 uOffset;
        [NonSerialized] public Vector3 vOffset;
        public Vector3 V0 => triangle.v0;
        public Vector3 V1 => triangle.v1;
        public Vector3 V2 => triangle.v2;
        public GTriangle((Vector3 v0,Vector3 v1,Vector3 v2) _tuple) : this(_tuple.v0,_tuple.v1,_tuple.v2) { }

        public GTriangle(Vector3 _vertex0, Vector3 _vertex1, Vector3 _vertex2)
        {
            this = default;
            triangle = new Triangle<Vector3>(_vertex0, _vertex1, _vertex2);
            GTriangle_Ctor();
        }

        void GTriangle_Ctor()
        {
            uOffset = V1-V0;
            vOffset = V2-V0;
            normal = Vector3.Cross(uOffset.normalized,vOffset.normalized).normalized;
        }
        public Vector3 GetNormalUnnormalized() {
            return Vector3.Cross(uOffset, vOffset);
        }
        public Vector3 GetUVPoint(float u,float v)=>(1f - u - v) * this[0] + u * uOffset + v * vOffset;

        public GPlane GetPlane() => new GPlane(normal,V0);
        public static GTriangle operator +(GTriangle _src, Vector3 _dst)=> new GTriangle(_src.V0 + _dst, _src.V1 + _dst, _src.V2 + _dst);
        public static GTriangle operator -(GTriangle _src, Vector3 _dst)=> new GTriangle(_src.V0 - _dst, _src.V1 - _dst, _src.V2 - _dst);
        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()=>GTriangle_Ctor();
        public int Length => 3;
        public Vector3 this[int index] => triangle[index];
    }
    
    [Serializable]
    public struct GQuad : IQuad<Vector3>,IIterate<Vector3>
    {
        public Quad<Vector3> quad;
        public Vector3 normal;
        public GQuad(Quad<Vector3> _quad)
        {
            quad = _quad;
            normal = Vector3.Cross(_quad.L - _quad.B, _quad.R - _quad.B).normalized;
        }
        public GQuad(Vector3 _vb, Vector3 _vl, Vector3 _vf, Vector3 _vr):this(new Quad<Vector3>(_vb,_vl,_vf,_vr)){}
        public GQuad((Vector3 _vb, Vector3 _vl, Vector3 _vf, Vector3 _vr) _tuple) : this(_tuple._vb, _tuple._vl, _tuple._vf, _tuple._vr) { }
        public static explicit operator GQuad(Quad<Vector3> _src) => new GQuad(_src);
        
        public Vector3 this[int _index] => quad[_index];
        public Vector3 this[EQuadCorner _corner] => quad[_corner];
        public Vector3 B => quad.B;
        public Vector3 L => quad.L;
        public Vector3 F => quad.F;
        public Vector3 R => quad.R;
        public int Length => quad.Length;

        public static GQuad operator +(GQuad _src, Vector3 _dst)=> new GQuad(_src.B + _dst, _src.L + _dst, _src.F + _dst,_src.R+_dst);
        public static GQuad operator -(GQuad _src, Vector3 _dst)=> new GQuad(_src.B - _dst, _src.L - _dst, _src.F - _dst,_src.R-_dst);
    }

    [Serializable]
    public struct GQube : IQube<Vector3>,IIterate<Vector3>
    {
        public Qube<Vector3> qube;

        public Vector3 this[int _index] => throw new NotImplementedException();

        public Vector3 DB => qube.vDB;
        public Vector3 DL => qube.vDL;
        public Vector3 DF => qube.vDF;
        public Vector3 DR => qube.vDR;
        public Vector3 TB => qube.vTB;
        public Vector3 TL => qube.vTL;
        public Vector3 TF => qube.vTF;
        public Vector3 TR => qube.vTR;
        public int Length => qube.Length;
    }
    

}