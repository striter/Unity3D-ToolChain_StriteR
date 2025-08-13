using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Extensions;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    using static math;
    using static umath;
    using quaternion = quaternion;
    public partial struct GTriangle
    {
        public Triangle<float3> triangle;
        [NonSerialized] public float3 baryCentre;
        [NonSerialized] public GCoordinates axis;
        [NonSerialized] public float3 normal;
        [NonSerialized] public float3 uOffset;
        [NonSerialized] public float3 vOffset;
        public GTriangle(float3 _vertex0, float3 _vertex1, float3 _vertex2)
        {
            this = default;
            triangle = new Triangle<float3>(_vertex0, _vertex1, _vertex2);
            Ctor();
        }

        void Ctor()
        {
            uOffset = V1 - V0;
            vOffset = V2 - V0;
            axis = new GCoordinates(V0,uOffset,vOffset);
            normal = axis.forward.normalize();
            baryCentre = GetBarycenter();
        }
    }

    [Serializable]
    public partial struct GTriangle :IVolume, IConvex , ISurface , IRayIntersection , ITriangle<float3>, IIterate<float3>,ISerializationCallbackReceiver ,ISDF
    {
        public GTriangle(IList<float3> _vertices,PTriangle _src) => this = (GTriangle)_src.Convert(_vertices);
        public static GTriangle FromPositions<T>(IList<T> _vertices,PTriangle _src,Func<T,float3> _converter)  => new (_converter(_vertices[_src.V0]),_converter(_vertices[_src.V1]),_converter(_vertices[_src.V2]));
        public float3 V0 => triangle.v0;
        public float3 V1 => triangle.v1;
        public float3 V2 => triangle.v2;
        public float3 GetNormalUnnormalized() => axis.forward;
        public GPlane GetPlane() => new GPlane(normal,V0);
        public float3 GetForward()=> (V0 - (V1 + V2) / 2).normalize();
        public quaternion GetRotation() => quaternion.LookRotation(GetForward(),normal);

        public static readonly GTriangle kDefault = new (new float3(0,0,.5f),new float3(-.5f,0,-.5f),new float3(.5f,0,-.5f));
        public static explicit operator GTriangle(Triangle<float3> _src) => new (_src.v0,_src.v1,_src.v2);
        public static explicit operator GTriangle(Triangle<Vector3> _src) => new (_src.v0,_src.v1,_src.v2);
        public static GTriangle Convert<T>(ITriangle<T> _src,Func<T,float3> _converter) where T:struct => new (_converter(_src.V0),_converter(_src.V1),_converter(_src.V2)); 
        public Triangle<Vector3> ToVector3() => new(V0, V1, V2);
        public static GTriangle operator +(GTriangle _src, float3 _dst) => new (_src.V0 + _dst, _src.V1 + _dst, _src.V2 + _dst);
        public static GTriangle operator -(GTriangle _src, float3 _dst) => new (_src.V0 - _dst, _src.V1 - _dst, _src.V2 - _dst);
        public static GTriangle operator*(Matrix4x4 _matrix,GTriangle _triangle) => new (
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
        public GBox GetBoundingBox() => GBox.GetBoundingBox(this);
        public GSphere GetBoundingSphere() => GSphere.GetBoundingSphere(this);

        public IEnumerable<float3> GetAxis()
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
 
            var n = cross( v1v0, v2v0 );
            var q = cross( rov0, rd );
            var d = 1.0f/dot(  n, rd );
            var u =   d*dot( -q, v2v0 );
            var v =   d*dot(  q, v1v0 );
            var t =   d*dot( -n, rov0 );
            distance = -1;
            if( u<0.0f || v<0.0f || (u+v)>1.0f ) 
                return false;

            distance = t;
            return true;
        }
            
        public float SDF(float3 _position)
        {
            var a = V0; var b = V1; var c = V2; var p = _position;
            var ba = b - a; var pa = p - a;
            var cb = c - b; var pb = p - b;
            var ac = a - c; var pc = p - c;
            var nor = cross( ba, ac );

            return length(
                (sign(dot(cross(ba,nor),pa)) +
                    sign(dot(cross(cb,nor),pb)) +
                    sign(dot(cross(ac,nor),pc))<2.0f)
                    ?
                    min( min(
                            sqr(ba*clamp(dot(ba,pa)/dot2(ba),0.0f,1.0f)-pa),
                            dot2(cb*clamp(dot(cb,pb)/dot2(cb),0.0f,1.0f)-pb) ),
                        dot2(ac*clamp(dot(ac,pc)/dot2(ac),0.0f,1.0f)-pc) )
                    :
                    dot(nor,pa)*dot(nor,pa)/dot2(nor) );
        }

        public void DrawGizmos() => UGizmos.DrawLinesConcat(triangle);
    }

}