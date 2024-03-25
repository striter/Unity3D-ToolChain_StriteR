using System;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    [Serializable]
    public struct GRay
    {
        public float3 origin;
        public float3 direction;
        public GRay(float3 _position, float3 _direction) { origin = _position; direction = _direction; }
        public float3 GetPoint(float _distance) => origin + direction * _distance;

        #region Implements
        public GLine ToLine(float _length)=>new GLine(origin,direction,_length);
        public static implicit operator Ray(GRay _ray)=>new Ray(_ray.origin,_ray.direction);
        public static implicit operator GRay(Ray _ray) => new GRay(_ray.origin, _ray.direction);
        public static GRay operator *(float4x4 _src, GRay _dst) => new GRay(math.mul(_src,_dst.origin.to4(1)).xyz, math.mul(_src,_dst.direction.to4(0)).xyz);
        public static GRay operator +(GRay _src, GRay _dst) => new GRay(_src.origin + _dst.origin,_src.direction+_dst.direction);
        public static GRay operator /(GRay _src, float _dst) => new GRay(_src.origin / _dst,_src.direction / _dst);
        public static GRay Lerp(GRay _src, GRay _dst, float _value) => new GRay(math.lerp(_src.origin,_dst.origin,_value), math.lerp(_src.direction , _dst.direction,_value));
        public GRay Inverse() => new GRay(origin, -direction);
        #endregion
    }

}