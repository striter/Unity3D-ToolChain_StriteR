using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Geometry
{
    public interface ITriangle<T> where T : struct
    {
        T this[int _index] { get; }
        T V0 { get;}
        T V1 { get; }
        T V2 { get; }
    }

    public partial struct Triangle<T>
    {
        public T v0,v1,v2;
        public Triangle(T _v0, T _v1, T _v2) { v0 = _v0; v1 = _v1; v2 = _v2; }
        public Triangle((T v0,T v1,T v2) _tuple) : this(_tuple.v0,_tuple.v1,_tuple.v2) { }
        
        public Triangle<Y> Convert<T,Y>(Triangle<T> _vertices, Func<T, Y> _convert) where T:struct  where Y:struct=> new Triangle<Y>( _convert( _vertices[0]), _convert(_vertices[1]),_convert( _vertices[2]));
    }

    public partial struct PTriangle
    {
        public Triangle<int> triangle;
        public PTriangle(Triangle<int> _triangle) { triangle = _triangle;  }
        public PTriangle(int _index0, int _index1, int _index2):this(new Triangle<int>(_index0,_index1,_index2)){}
        
        public Triangle<T> Convert<T>(IList<T> _vertices) where T:struct => new Triangle<T>(_vertices[V0], _vertices[V1],_vertices[V2]);
        
        public static explicit operator PTriangle(Triangle<int> _src) => new PTriangle(_src);
    }

    public partial struct G2Triangle 
    {
        public Triangle<float2> triangle;
        
        [NonSerialized] public float2 uOffset;
        [NonSerialized] public float2 vOffset;

        public G2Triangle(float2 _vertex0, float2 _vertex1, float2 _vertex2)
        {
            this = default;
            triangle = new Triangle<float2>(_vertex0, _vertex1, _vertex2);
            Ctor();
        }

        void Ctor()
        {
            uOffset = V1 - V0;
            vOffset = V2 - V0;
        }
    }
    
    public partial struct GTriangle
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
            Ctor();
        }

        void Ctor()
        {
            uOffset = V1-V0;
            vOffset = V2-V0;
            normal = Vector3.Cross(uOffset.normalized,vOffset.normalized).normalized;
        }
        
        public Vector3 GetNormalUnnormalized() => Vector3.Cross(uOffset, vOffset);
        public Vector3 GetPoint(float _u,float _v)=>(1f - _u - _v) * this[0] + _u * uOffset + _v * vOffset;

        public GPlane GetPlane() => new GPlane(normal,V0);
    }

    #region Implements
    [Serializable]
    public partial struct Triangle<T> : ITriangle<T>, IEquatable<Triangle<T>>, IIterate<T>, IEnumerable<T> where T : struct
    {
        public int Length => 3;
        public T this[int _index]
        {
            get
            {
                switch (_index)
                {
                    default: Debug.LogError("Invalid Index:" + _index); return v0;
                    case 0: return v0;
                    case 1: return v1;
                    case 2: return v2;
                }
            }
        }

        public T V0 => v0;
        public T V1 => v1;
        public T V2 => v2;

        public bool Equals(Triangle<T> other)
        {
            return v0.Equals(other.v0) && v1.Equals(other.v1) && v2.Equals(other.v2);
        }

        public IEnumerator<T> GetEnumerator()
        {
            yield return v0;
            yield return v1;
            yield return v2;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = v0.GetHashCode();
                hashCode = (hashCode * 397) ^ v1.GetHashCode();
                hashCode = (hashCode * 397) ^ v2.GetHashCode();
                return hashCode;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Equals(Triangle<T> x, Triangle<T> y)
        {
            return x.v0.Equals(y.v0) && x.v1.Equals(y.v1) && x.v2.Equals(y.v2);
        }


        public int GetHashCode(Triangle<T> obj)
        {
            unchecked
            {
                int hashCode = obj.v0.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.v1.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.v2.GetHashCode();
                return hashCode;
            }
        }
    }
    [Serializable]
    public partial struct PTriangle:ITriangle<int>,IEnumerable<int>,IIterate<int>,IEquatable<PTriangle>
    {
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
        public static readonly PTriangle kInvalid = new PTriangle(-1, -1, -1);

        public bool Equals(PTriangle other)=>triangle.Equals(other.triangle);

        public override bool Equals(object obj)=>obj is PTriangle other && Equals(other);

        public override int GetHashCode()=> triangle.GetHashCode();
    }

    [Serializable]
    public partial struct G2Triangle : ITriangle<float2>, IEnumerable<float2>
    {
        public float2 V0 => triangle.v0;
        public float2 V1 => triangle.v1;
        public float2 V2 => triangle.v2;
        public float2 this[int _index] => triangle[_index];
        public IEnumerator<float2> GetEnumerator() => triangle.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()=> GetEnumerator();
    }
    
    
    [Serializable]
    public partial struct GTriangle :ITriangle<Vector3>, IIterate<Vector3>,ISerializationCallbackReceiver
    {
        public static explicit operator GTriangle(Triangle<Vector3> _src) => new GTriangle(_src.v0,_src.v1,_src.v2);
        public static GTriangle operator +(GTriangle _src, Vector3 _dst)=> new GTriangle(_src.V0 + _dst, _src.V1 + _dst, _src.V2 + _dst);
        public static GTriangle operator -(GTriangle _src, Vector3 _dst)=> new GTriangle(_src.V0 - _dst, _src.V1 - _dst, _src.V2 - _dst);
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()=>Ctor();
        public int Length => 3;
        public Vector3 this[int index] => triangle[index];
    }
    #endregion
}