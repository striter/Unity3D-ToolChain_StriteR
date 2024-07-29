using System;
using System.Collections.Generic;
using System.Linq.Extensions;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    public partial struct GPlane : ISurface
    {
        public float3 normal;
        public float distance;
        [HideInInspector]public float3 position;
        public GPlane(float3 _normal, float _distance) 
        { 
            this = default;
            normal = _normal;
            distance = _distance;
            Ctor(true);
        }

        public GPlane(float3 _normal, float3 _position)
        {
            this = default;
            normal = _normal;
            position = _position;
            Ctor(false);
        }

        void Ctor(bool _position)
        {
            if (_position)
                position = normal * distance;
            else
                distance = math.dot(normal, position);// UGeometryIntersect.PointPlaneDistance(_position, new GPlane(_normal, 0));
        }

        public float3 Origin => position;
        public float3 Normal => normal;
    }

    [Serializable]
    public partial struct GPlane:IEquatable<GPlane>,IEqualityComparer<GPlane>,ISerializationCallbackReceiver ,IRayIntersection
    {
        public static readonly GPlane kComparer = new GPlane();
        public static readonly GPlane kDefault = new GPlane(Vector3.up, 0f);
        public static readonly GPlane kUp = new GPlane(Vector3.up, 0f);
        
        public bool IsFront(float3 _point) => this.dot(_point) > 0;

        
        static float3[] kTempArray = new float3[3];
        public static GPlane FromPositions(IEnumerable<float3> _ienumerable)
        {
            _ienumerable.FillArray(kTempArray);
            return FromPositions(kTempArray[0],kTempArray[1],kTempArray[2]);
        }
        public static GPlane FromPositions(float3 _V0, float3 _V1, float3 _V2) => new GPlane(math.cross((_V1 - _V0).normalize(),(_V2 - _V0).normalize()).normalize(),_V0);
        public static GPlane operator *(Matrix4x4 _matrix, GPlane _plane) => new GPlane(_matrix.MultiplyVector(_plane.normal),_matrix.MultiplyPoint(_plane.position));
        public static GPlane operator *(float4x4 _matrix, GPlane _plane) => new GPlane(math.mul(_matrix,_plane.normal.to4(0)).xyz,math.mul(_matrix,_plane.position.to4(1)).xyz);

        public static bool operator ==(GPlane _src, GPlane _dst) =>  _src.normal.Equals( _dst.normal) && math.abs(_src.distance - _dst.distance) < float.Epsilon;
        public static bool operator !=(GPlane _src, GPlane _dst) => !(_src == _dst);
        public bool Equals(GPlane _dst) => this == _dst;
        public bool Equals(GPlane _src, GPlane _dst) => _src == _dst;
        public override bool Equals(object obj)=> obj is GPlane other && Equals(other);
        public int GetHashCode(GPlane _target)=>_target.normal.GetHashCode()+_target.distance.GetHashCode();
        public override int GetHashCode()=> normal.GetHashCode()+distance.GetHashCode()+position.GetHashCode();

        public override string ToString() => $"{normal},{distance}";
        public bool RayIntersection(GRay _ray, out float _distance)
        {
            _distance = -1;
            var nrD = math.dot(normal, _ray.direction);
            if (nrD == 0)
                return false;
            
            var nrO = math.dot(normal, _ray.origin);
            _distance = (distance - nrO) / nrD;
            return true;
        }

        public float Ditance(GRay _ray)
        {
            var nrO = math.dot(normal, _ray.origin);
            var nrD = math.dot(normal, _ray.direction);
            return (distance - nrO) / nrD;
        }

        public static implicit operator float4(GPlane _plane)=>_plane.normal.to4(_plane.distance);
        
        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            normal = math.normalize(normal);
            Ctor(true);
        }
        
        public void DrawGizmos() => DrawGizmos(5f);
        public void DrawGizmos(float radius)
        {
            UGizmos.DrawWireDisk(position, normal, 5f);
            UGizmos.DrawArrow(position,normal,.5f,.1f);
        }
    }

    public static class UPlane
    {
        public static float dot(this GPlane _plane, float3 _point) => math.dot(_point.to4(-1), _plane);
        public static Matrix4x4 GetMirrorMatrix(this GPlane _plane)
        {
            var normal = _plane.normal;
            var distance = _plane.distance;
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
}