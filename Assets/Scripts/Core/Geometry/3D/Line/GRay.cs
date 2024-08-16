using System;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    [Serializable]
    public struct GRay : ILine , ISDF
    {
        public float3 origin;
        [PostNormalize] public float3 direction;
        public GRay(float3 _position, float3 _direction) { origin = _position; direction = _direction; }
        public float3 GetPoint(float _distance) => origin + direction * _distance;

        public static GRay StartEnd(float3 _start,float3 _end,bool _normalize) => new GRay(_start, _normalize?(_end - _start).normalize():(_end - _start));
        
        #region Implements
        public GLine ToLine(float _length)=>new GLine(origin,direction,_length);
        public static implicit operator Ray(GRay _ray)=>new Ray(_ray.origin,_ray.direction);
        public static implicit operator GRay(Ray _ray) => new GRay(_ray.origin, _ray.direction);
        public static GRay operator *(float4x4 _src, GRay _dst) => new GRay(math.mul(_src,_dst.origin.to4(1)).xyz, math.mul(_src,_dst.direction.to4(0)).xyz);
        public static GRay operator +(GRay _src, GRay _dst) => new GRay(_src.origin + _dst.origin,_src.direction+_dst.direction);
        public static GRay operator /(GRay _src, float _dst) => new GRay(_src.origin / _dst,_src.direction / _dst);
        public static GRay Lerp(GRay _src, GRay _dst, float _value) => new GRay(math.lerp(_src.origin,_dst.origin,_value), math.lerp(_src.direction , _dst.direction,_value));
        public GRay Inverse() => new GRay(origin, -direction);
        public GRay Forward(float _value)=> new GRay(origin + direction*_value, direction);
        #endregion

        public static readonly GRay kDefault = new GRay(0,kfloat3.forward);
        public float3 Origin => origin;
        public void DrawGizmos() => Gizmos.DrawRay(origin, direction);
        public float SDF(float3 _position)
        {
            var pointToStart = _position - origin;
            return math.length(math.cross(direction, pointToStart));
        }
    }

}