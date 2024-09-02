using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    public partial struct GTriangle
    {
        public Triangle<float3> triangle;
        [NonSerialized] public float3 baryCentre;
        [NonSerialized] public GAxis axis;
        [NonSerialized] public float3 normal;
        public float3 uOffset => axis.right;
        public float3 vOffset => axis.up;
        public GTriangle((float3 v0,float3 v1,float3 v2) _tuple) : this(_tuple.v0,_tuple.v1,_tuple.v2) { }

        public GTriangle(float3 _vertex0, float3 _vertex1, float3 _vertex2)
        {
            this = default;
            triangle = new Triangle<float3>(_vertex0, _vertex1, _vertex2);
            Ctor();
        }

        void Ctor()
        {
            axis = new GAxis(V0,V1-V0,V2-V0);
            normal = axis.forward;
            baryCentre = GetBarycenter();
        }
    }

    [Serializable]
    public partial struct GTriangle :IVolume, IConvex , ISurface , IRayIntersection , ITriangle<float3>, IIterate<float3>,ISerializationCallbackReceiver
    {
        public float3 V0 => triangle.v0;
        public float3 V1 => triangle.v1;
        public float3 V2 => triangle.v2;
        public float3 GetNormalUnnormalized() => axis.forward;
        public GPlane GetPlane() => new GPlane(normal,V0);
        public float3 GetForward()=> (V0 - (V1 + V2) / 2).normalize();
        public quaternion GetRotation() => quaternion.LookRotation(GetForward(),normal);

        public static readonly GTriangle kDefault = new GTriangle(new float3(0,0,.5f),new float3(-.5f,0,-.5f),new float3(.5f,0,-.5f));
        public static explicit operator GTriangle(Triangle<float3> _src) => new GTriangle(_src.v0,_src.v1,_src.v2);
        public static explicit operator GTriangle(Triangle<Vector3> _src) => new GTriangle(_src.v0,_src.v1,_src.v2);
        public Triangle<Vector3> ToVector3() => new Triangle<Vector3>(V0, V1, V2);
        public static GTriangle operator +(GTriangle _src, float3 _dst)=> new GTriangle(_src.V0 + _dst, _src.V1 + _dst, _src.V2 + _dst);
        public static GTriangle operator -(GTriangle _src, float3 _dst)=> new GTriangle(_src.V0 - _dst, _src.V1 - _dst, _src.V2 - _dst);
        public static GTriangle operator*(Matrix4x4 _matrix,GTriangle _triangle) => new GTriangle(
            _matrix.MultiplyPoint(_triangle.V0),
            _matrix.MultiplyPoint(_triangle.V1),
            _matrix.MultiplyPoint(_triangle.V2));
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()=>Ctor();
        public int Length => 3;

        public float3 this[int index]
        {
            get => triangle[index];
            set 
            {
                switch (index)
                {
                    default: throw new IndexOutOfRangeException();
                    case 0: triangle.v0 = value; break;
                    case 1: triangle.v1 = value; break;
                    case 2: triangle.v2 = value; break;
                }
            }
        }
        

        public float3 GetSupportPoint(float3 _direction) => this.MaxElement(_p => math.dot(_p, _direction));
        public GBox GetBoundingBox() => UGeometry.GetBoundingBox(this);
        public GSphere GetBoundingSphere() => UGeometry.GetBoundingSphere(this);

        public IEnumerable<float3> GetAxes()
        {
            yield return normal;
        }

        public IEnumerator<float3> GetEnumerator()
        {
            yield return V0;
            yield return V1;
            yield return V2;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public float3 Origin => baryCentre;
        public float3 Normal => normal;
        //https://iquilezles.org/articles/hackingintersector/
        public bool RayIntersection(GRay _ray, out float distance)
        {
            var ro = _ray.origin;
            var rd = _ray.direction;
            var v0 = V0;
            var v1v0 = V1 - v0;
            var v2v0 = V2 - v0;
            var rov0 = ro-v0;
 
            var n = math.cross( v1v0, v2v0 );
            var q = math.cross( rov0, rd );
            var d = 1.0f/math.dot(  n, rd );
            var u =   d*math.dot( -q, v2v0 );
            var v =   d*math.dot(  q, v1v0 );
            var t =   d*math.dot( -n, rov0 );
            distance = -1;
            if( u<0.0f || v<0.0f || (u+v)>1.0f ) 
                return false;

            distance = t;
            return true;
        }
        public void DrawGizmos()=>UGizmos.DrawLinesConcat(triangle);
    }

}