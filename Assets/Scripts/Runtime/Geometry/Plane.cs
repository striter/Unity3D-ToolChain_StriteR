using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Geometry
{
    [Serializable]
    public struct GPlane:IEquatable<GPlane>,IEqualityComparer<GPlane>,ISerializationCallbackReceiver
    {
        public float3 normal;
        public float distance;
        [HideInInspector]public float3 position;
        public GPlane(float3 _normal, float _distance) 
        { 
            this = default;
            normal = _normal;
            distance = _distance;
            GPlane_Ctor(true);
        }

        public GPlane(float3 _normal, float3 _position)
        {
            this = default;
            normal = _normal;
            position = _position;
            GPlane_Ctor(false);
        }

        void GPlane_Ctor(bool _position)
        {
            if (_position)
                position = normal * distance;
            else
                distance = math.dot(normal, position);// UGeometryIntersect.PointPlaneDistance(_position, new GPlane(_normal, 0));
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            normal = math.normalize(normal);
            GPlane_Ctor(true);
        }
        public static implicit operator float4(GPlane _plane)=>_plane.normal.to4(_plane.distance);
        public bool IsFront(float3 _point) => math.dot(this,_point.to4(-1)) > 0;

        public static bool operator ==(GPlane _src, GPlane _dst) =>  _src.normal.Equals( _dst.normal) && math.abs(_src.distance - _dst.distance) < float.Epsilon;
        public static bool operator !=(GPlane _src, GPlane _dst) => !(_src == _dst);
        public bool Equals(GPlane _dst) => this == _dst;
        public bool Equals(GPlane _src, GPlane _dst) => _src == _dst;
        public override bool Equals(object obj)=> obj is GPlane other && Equals(other);
        public int GetHashCode(GPlane _target)=>_target.normal.GetHashCode()+_target.distance.GetHashCode();
        public override int GetHashCode()=> normal.GetHashCode()+distance.GetHashCode()+position.GetHashCode();

        public static readonly GPlane kComparer = new GPlane();
        public static readonly GPlane kZeroPlane = new GPlane(Vector3.up, 0f);
        public override string ToString() => $"{normal},{distance}";
        
        public Matrix4x4 GetMirrorMatrix()
        {
            Matrix4x4 mirrorMatrix = Matrix4x4.identity;
            mirrorMatrix.m00 = 1 - 2 * normal.x * normal.x;
            mirrorMatrix.m01 = -2 * normal.x * normal.y;
            mirrorMatrix.m02 = -2 * normal.x * normal.z;
            mirrorMatrix.m03 = 2 * normal.x * distance;
            mirrorMatrix.m10 = -2 * normal.x * normal.y;
            mirrorMatrix.m11 = 1 - 2 * normal.y * normal.y;
            mirrorMatrix.m12 = -2 * normal.y * normal.z;
            mirrorMatrix.m13 = 2 * normal.y * distance;
            mirrorMatrix.m20 = -2 * normal.x * normal.z;
            mirrorMatrix.m21 = -2 * normal.y * normal.z;
            mirrorMatrix.m22 = 1 - 2 * normal.z * normal.z;
            mirrorMatrix.m23 = 2 * normal.z * distance;
            mirrorMatrix.m30 = 0;
            mirrorMatrix.m31 = 0;
            mirrorMatrix.m32 = 0;
            mirrorMatrix.m33 = 1;
            return mirrorMatrix;
        }

    }

    [Serializable]
    public struct G2Plane:ISerializationCallbackReceiver
    {
        public float2 normal;
        public float distance;
        [HideInInspector] public float2 position;
        public G2Plane(float2 _normal,float _distance)
        {
            normal = _normal;
            distance = _distance;
            position = default;
            Ctor();
        }

        public G2Plane(float2 _normal, float2 _position)
        {
            normal = _normal;
            position = _position;
            distance = math.dot(_normal, _position);
        }

        void Ctor()
        {
            position = normal * distance;
        }
        
        public static implicit operator float3(G2Plane _plane)=>_plane.normal.to3xy(_plane.distance);
        public bool IsPointFront(float2 _point) => math.dot(_point.to3xy(-1),this)>0;
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize() => Ctor();
        public static readonly G2Plane kDefault = new(new float2(0,1),0f);
    }
}