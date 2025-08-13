using System;
using System.Collections.Generic;
using System.Linq.Extensions;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    public partial struct GPlane 
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

    }

    [Serializable]
    public partial struct GPlane:IEquatable<GPlane>,IEqualityComparer<GPlane>,ISerializationCallbackReceiver ,IRayIntersection , ISDF , ISurface
    {
        public float3 Origin => position;
        public float3 Normal => normal;
        public static readonly GPlane kComparer = new GPlane();
        public static readonly GPlane kZero = new GPlane(Vector3.up, 0f);
        public static readonly GPlane kDefault = new GPlane(Vector3.up, 0f);
        public static readonly GPlane kUp = new GPlane(Vector3.up, 0f);
        
        public bool IsFront(float3 _point) => this.dot(_point) > 0;

        
        static float3[] kTempArray = new float3[3];
        public static GPlane FromPositions(IEnumerable<float3> _ienumerable)
        {
            _ienumerable.Fill(kTempArray);
            return FromPositions(kTempArray[0],kTempArray[1],kTempArray[2]);
        }
        public static GPlane FromPositions(float3 _V0, float3 _V1, float3 _V2) => new GPlane(math.cross((_V1 - _V0).normalize(),(_V2 - _V0).normalize()).normalize(),_V0);
        public static GPlane operator *(Matrix4x4 _matrix, GPlane _plane) => new GPlane(_matrix.MultiplyVector(_plane.normal),_matrix.MultiplyPoint(_plane.position));
        public static GPlane operator *(float4x4 _matrix, GPlane _plane) => new GPlane(math.mul(_matrix,_plane.normal.to4(0)).xyz,math.mul(_matrix,_plane.position.to4(1)).xyz);

        //&https://www.geometrictools.com/GTE/Mathematics/ApprHeightPlane3.h
        public static GPlane LeastSquaresRegression(IEnumerable<float3> _positions)
        {
            var count = 0;
            var mean = kfloat3.zero;
            foreach (var position in _positions)
            {
                mean += position;
                count += 1;
            }
            mean /= count;
            if (count <= 3)
            {
                Debug.Log($"[LeastSquaresRegression] {nameof(_positions)} Count Invalid : {count}");
                return kZero;
            }
            var covar00 = 0f;
            var covar01 = 0f;
            var covar02 = 0f;
            var covar11 = 0f;
            var covar12 = 0f;
            foreach (var position in _positions)
            {
                var diff = position - mean;
                covar00 += diff[0] * diff[0];
                covar01 += diff[0] * diff[1];
                covar02 += diff[0] * diff[2];
                covar11 += diff[1] * diff[1];
                covar12 += diff[1] * diff[2];
            }
            
            var det = covar00 * covar11 - covar01 * covar01;
            if (det != 0f)
            {
                var invDet = 1f / det;
                return new GPlane(new float3(
                        (covar11 * covar02 - covar01*covar12) * invDet,
                        (covar00 * covar12 - covar01 * covar02) * invDet,
                        -1f).normalize(), mean);
            }

            return kZero;
        }
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

        public float Distance(GRay _ray)
        {
            var nrO = math.dot(normal, _ray.origin);
            var nrD = math.dot(normal, _ray.direction);
            return (distance - nrO) / nrD;
        }
        
        public float SDF(float3 _position) => math.dot(normal, _position - position);

        public static implicit operator float4(GPlane _plane)=>_plane.normal.to4(_plane.distance);
        
        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            normal = math.normalize(normal);
            Ctor(true);
        }
        
        public void DrawGizmos() => DrawGizmos(5f);
        public void DrawGizmos(float _radius)
        {
            UGizmos.DrawWireDisk(position, normal, _radius);
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