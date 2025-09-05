using System;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    public partial struct GSphere
    {
        public float3 center;
        [Clamp(0)] public float radius;
    }

    [Serializable]
    public partial struct GSphere : IVolume , IRayVolumeIntersection , ISDF
    {
        public GSphere(float3 _center, float _radius) { center = _center; radius = _radius; }
        public GSphere(float4 _centerAndRadius) : this(_centerAndRadius.xyz, _centerAndRadius.w) { }
        public float3 Origin => center;
        public static readonly GSphere kDefault = new (float3.zero,.5f);
        public static readonly GSphere kOne = new(float3.zero, .5f);
        public static readonly GSphere kZero = new(0,0);
        public float3 GetSupportPoint(float3 _direction) => center + _direction.normalize() * radius;
        public GSphere GetBoundingSphere() => this;
        public float SDF(float3 _position)
        {
            var p = _position - center;
            var r = radius;
            return math.length(p) - r;
        }

        public static GSphere operator +(GSphere _src, float3 _dst) => new(_src.center+_dst,_src.radius);
        public static GSphere operator -(GSphere _src, float3 _dst) => new(_src.center - _dst, _src.radius);
        
        public static implicit operator float4(GSphere _src) => new(_src.center,_src.radius);
        public bool Contains(float3 _p, float _bias = float.Epsilon) =>math.lengthsq(_p - center) < radius * radius + _bias;
        public GSphere Encapsulate(float3 _p)
        {
            var delta = _p - center;
            var length = math.length(delta);
            var direction = delta / length;
            return length > radius ? Minmax(center - direction * radius, center + direction * length) : this;
        }

        public bool Contains(GSphere _sphere) =>math.lengthsq(_sphere.center - center) < radius * radius + _sphere.radius;
        public GSphere Encapsulate(GSphere _sphere) => Minmax(this, _sphere);
        public GBox GetBoundingBox()=> GBox.Minmax(center - radius,center + radius);
        public bool RayIntersection(GRay _ray, out float2 distances)
        {
            distances = -1;
            var shift = _ray.origin - center;
            var dotOffsetDirection = math.dot(_ray.direction, shift);
            var sqrRadius = radius * radius;
            var radiusDelta = math.dot(shift, shift) - sqrRadius;
            if (dotOffsetDirection > 0 && radiusDelta > 0)
                return false;
            var dotOffset = math.dot(shift, shift);
            var discriminant = dotOffsetDirection * dotOffsetDirection - dotOffset + sqrRadius;
            if (discriminant < 0)
                return false;

            discriminant = math.sqrt(discriminant);
            var t0 = -dotOffsetDirection - discriminant;
            var t1 = discriminant * 2;
            distances =  new float2(t0, t1);
            return !(t0 < 0);
        }
        public void DrawGizmos() => Gizmos.DrawWireSphere(center,radius);
    }
}