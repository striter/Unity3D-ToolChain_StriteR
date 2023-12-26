using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Geometry
{
    public partial struct GTriangle
    {
        public Triangle<float3> triangle;
        [NonSerialized] public float3 normal;
        [NonSerialized] public float3 uOffset;
        [NonSerialized] public float3 vOffset;
        // [NonSerialized] public float3 wOffset;
        public GTriangle((float3 v0,float3 v1,float3 v2) _tuple) : this(_tuple.v0,_tuple.v1,_tuple.v2) { }

        public GTriangle(float3 _vertex0, float3 _vertex1, float3 _vertex2)
        {
            this = default;
            triangle = new Triangle<float3>(_vertex0, _vertex1, _vertex2);
            Ctor();
        }

        void Ctor()
        {
            uOffset = triangle.v1 - triangle.v0;
            vOffset = triangle.v2 - triangle.v0;
            // wOffset = V0 - V2;
            normal = math.cross(uOffset.normalize(),vOffset.normalize()).normalize();
        }
    }

    [Serializable]
    public partial struct GTriangle :ITriangle<float3>, IIterate<float3>,ISerializationCallbackReceiver, IShape3D
    {
        public float3 V0 => triangle.v0;
        public float3 V1 => triangle.v1;
        public float3 V2 => triangle.v2;
        public float3 GetNormalUnnormalized() => math.cross(uOffset, vOffset);
        public float3 GetBarycenter() => GetPoint(.25f);
        public float3 GetPoint(float2 _uv) => GetPoint(_uv.x, _uv.y);
        public float3 GetPoint(float _u,float _v) => V0 + _u * +uOffset + _v * vOffset;
        public GPlane GetPlane() => new GPlane(normal,V0);
        public float3 GetForward()=> (V0 - (V1 + V2) / 2).normalize();
        public quaternion GetRotation() => quaternion.LookRotation(GetForward(),normal);

        public static readonly GTriangle kDefault = new GTriangle(new float3(0,0,1),new float3(-.5f,0,-1),new float3(.5f,0,-1));
        public static explicit operator GTriangle(Triangle<float3> _src) => new GTriangle(_src.v0,_src.v1,_src.v2);
        public static explicit operator GTriangle(Triangle<Vector3> _src) => new GTriangle(_src.v0,_src.v1,_src.v2);
        public Triangle<Vector3> ToVector3() => new Triangle<Vector3>(V0, V1, V2);
        public static GTriangle operator +(GTriangle _src, float3 _dst)=> new GTriangle(_src.V0 + _dst, _src.V1 + _dst, _src.V2 + _dst);
        public static GTriangle operator -(GTriangle _src, float3 _dst)=> new GTriangle(_src.V0 - _dst, _src.V1 - _dst, _src.V2 - _dst);
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()=>Ctor();
        public int Length => 3;

        public float3 this[int index]
        {
            get =>triangle[index];
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
        
        public static GTriangle operator*(Matrix4x4 _matrix,GTriangle _triangle) => new GTriangle(
            _matrix.MultiplyPoint(_triangle.V0),
                _matrix.MultiplyPoint(_triangle.V1),
                _matrix.MultiplyPoint(_triangle.V2));

        public float3 GetSupportPoint(float3 _direction) => triangle.Max(_p => math.dot(_p, _direction));
        public float3 Center => (V0 + V1 + V2) / 3;
    }
}