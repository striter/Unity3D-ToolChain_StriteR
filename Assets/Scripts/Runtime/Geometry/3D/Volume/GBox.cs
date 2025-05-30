using System;
using System.Collections;
using System.Collections.Generic;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    public partial struct GBox
    {
        public float3 center;
        public float3 extent;
        [NonSerialized] public float3 size;
        [NonSerialized] public float3 min;
        [NonSerialized] public float3 max;
        public GBox(float3 _center, float3 _extent)
        {
            this = default;
            center = _center;
            extent = _extent;
            Ctor();
        }
        
        void Ctor()
        {
            size = extent * 2f;
            min = center - extent;
            max = center + extent;
        }
    }

    [Serializable]
    public partial struct GBox : ISerializationCallbackReceiver, IVolume , IConvex , IRayVolumeIntersection , ISDF
    {
        public void OnBeforeSerialize(){  }
        public void OnAfterDeserialize()=>Ctor();
        public GSphere GetBoundingSphere() => GSphere.Minmax(min,max);
        public float SDF(float3 _position)
        {
            var p = _position - center;
            var b = extent;
            var q = math.abs(p) - b;
            return math.length(math.max(q,0.0f)) + math.min(math.max(q.x,math.max(q.y,q.z)),0.0f);
        }

        public GBox GetBoundingBox() => this;
        public readonly float3 GetUVW(float3 _pos) => (_pos - min) / size;
        public float3 GetPoint(float3 _uvw) => min + _uvw * size;
        public float3 GetPoint(float _u, float _v, float _w) => GetPoint(new float3(_u, _v, _w));
        public float3 GetCenteredPoint(float3 _uvw) => center + _uvw * size;
        public float3 GetCenteredPoint(float _u, float _v, float _w) => GetCenteredPoint(new float3(_u, _v, _w));
        public float3 Origin => center;

        public bool Contains(float3 _point,float _bias = float.Epsilon)
        {
            float3 absOffset = math.abs(center-_point) + _bias;
            return absOffset.x < extent.x && absOffset.y < extent.y && absOffset.z < extent.z;
        }

        public IEnumerable<GPlane> GetPlanes(bool _x = true,bool _y = true,bool _z = true)
        {
            if (_x)
            {
                yield return new GPlane(kfloat3.left,GetCenteredPoint(kfloat3.right*.5f));
                yield return new GPlane(kfloat3.right,GetCenteredPoint(kfloat3.left*.5f));
            }

            if (_y)
            {
                yield return new GPlane(kfloat3.down,GetCenteredPoint(kfloat3.up*.5f));
                yield return new GPlane(kfloat3.up,GetCenteredPoint(kfloat3.down*.5f));
            }

            if (_z)
            {
                yield return new GPlane(kfloat3.forward,GetCenteredPoint(kfloat3.back*.5f));
                yield return new GPlane(kfloat3.back,GetCenteredPoint(kfloat3.forward*.5f));
            }
        }

        public IEnumerable<GQuad> GetQuads()
        {
            yield return new GQuad(GetCenteredPoint(.5f,-.5f,-.5f),GetCenteredPoint(-.5f,-.5f,-.5f),GetCenteredPoint(-.5f,.5f,-.5f),GetCenteredPoint(.5f,.5f,-.5f));
            yield return new GQuad(GetCenteredPoint(-.5f,-.5f,-.5f),GetCenteredPoint(-.5f,-.5f,.5f),GetCenteredPoint(-.5f,.5f,.5f),GetCenteredPoint(-.5f,.5f,-.5f));
            yield return new GQuad(GetCenteredPoint(-.5f,-.5f,.5f),GetCenteredPoint(.5f,-.5f,.5f),GetCenteredPoint(.5f,.5f,.5f),GetCenteredPoint(-.5f,.5f,.5f));
            yield return new GQuad(GetCenteredPoint(.5f,-.5f,.5f),GetCenteredPoint(.5f,-.5f,-.5f),GetCenteredPoint(.5f,.5f,-.5f),GetCenteredPoint(.5f,.5f,.5f));
            yield return new GQuad(GetCenteredPoint(.5f,-.5f,.5f),GetCenteredPoint(-.5f,-.5f,.5f),GetCenteredPoint(-.5f,-.5f,-.5f),GetCenteredPoint(.5f,-.5f,-.5f));
            yield return new GQuad(GetCenteredPoint(.5f,.5f,-.5f),GetCenteredPoint(-.5f,.5f,-.5f),GetCenteredPoint(-.5f,.5f,.5f),GetCenteredPoint(.5f,.5f,.5f));
        }
        
        public IEnumerable<float3> GetAxis()
        {
            yield return kfloat3.right;
            yield return kfloat3.up;
            yield return kfloat3.forward;
            yield return kfloat3.left;
            yield return kfloat3.down;
            yield return kfloat3.back;
        }
        
        public static GBox operator +(GBox _src, float3 _dst) => new GBox(_src.center+_dst,_src.extent);
        public static GBox operator -(GBox _src, float3 _dst) => new GBox(_src.center-_dst,_src.extent);
        public static GBox operator *(GBox _src, float _dst) => new GBox(_src.center * _dst,_src.extent*_dst);
        public static GBox operator /(GBox _src, float _dst) => new GBox(_src.center / _dst,_src.extent/_dst);
        public static GBox operator *(float4x4 _dst,GBox _src)
        {
            var min = kfloat3.max;
            var max = kfloat3.min;
            foreach (var point in _src.GetCorners())
            {
                var pointTransformed = math.mul(_dst, point.to4(1)).xyz;
                min = math.min(min, pointTransformed);
                max = math.max(max, pointTransformed);
            }
            return Minmax(min,max);
        }
        
        public static implicit operator Bounds(GBox _box)=> new Bounds(_box.center, _box.size);
        public static implicit operator GBox(Bounds _bounds) => new GBox(_bounds.center,_bounds.extents);

        public static readonly GBox kZero = new GBox(0, 0);
        public static readonly GBox kOne = new GBox(.5f,.5f);
        public static readonly GBox kDefault = new GBox(0f,.5f);
        public IEnumerator<float3> GetEnumerator()
        {
            yield return GetCenteredPoint(-.5f, -.5f, -.5f);
            yield return GetCenteredPoint(-.5f, .5f, -.5f);
            yield return GetCenteredPoint(-.5f, -.5f, .5f);
            yield return GetCenteredPoint(-.5f, .5f, .5f);
            yield return GetCenteredPoint(.5f, -.5f, -.5f);
            yield return GetCenteredPoint(.5f, .5f, -.5f);
            yield return GetCenteredPoint(.5f, -.5f, .5f);
            yield return GetCenteredPoint(.5f, .5f, .5f);
        }
        public IEnumerable<float3> GetCorners() => this;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public float3 GetSupportPoint(float3 _direction)
        {
            var invRayDir = 1f/_direction;
            var t0 = extent*invRayDir;
            var t1 = -extent*invRayDir;
            var tmax = math.max(t0, t1);
            var dstB = math.min(tmax.x, math.min(tmax.y, tmax.z));
            var dstInsideBox = math.max(0, dstB);
            return center + _direction*dstInsideBox;
        }
        public bool RayIntersection(GRay _ray, out float2 distances)
        {
            distances = -1;
            var invRayDir = 1f/_ray.direction;
            var t0 = (min - _ray.origin)*invRayDir;
            var t1 = (max - _ray.origin)*invRayDir;
            var tmin = math.min(t0, t1);
            var tmax = math.max(t0, t1);
            if (tmin.maxElement() > tmax.minElement())
                return false;
            
            var dstA = math.max(math.max(tmin.x, tmin.y), tmin.z);
            var dstB = math.min(tmax.x, math.min(tmax.y, tmax.z));
            var dstToBox = math.max(0, dstA);
            var dstInsideBox = math.max(0, dstB - dstToBox);
            distances = new float2(dstToBox, dstInsideBox);
            return true;
        }

        public void DrawGizmos() => Gizmos.DrawWireCube(center,size);
        public override string ToString() => $"GBox {center} {size}";
    }


    public static class GBox_Extension
    {
        public static GBox Resize(this GBox _box, float _normalizedValue) => new GBox(_box.center, _box.extent * _normalizedValue);
        public static float3 Clamp(this GBox _clamp,float3 _point) => math.clamp(_point, _clamp.min, _clamp.max);
        public static bool Clamp(this GBox _clamp,float3 _point, out float3 _clamped)
        {
            _clamped = _clamp.Clamp(_point);
            return !_clamp.Contains(_point);
        }
    }
}