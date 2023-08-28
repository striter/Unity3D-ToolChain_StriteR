using System;
using Unity.Mathematics;
using UnityEngine;

namespace Geometry
{
    [Serializable]
    public struct G2Line
    {
        public float2 start;
        public float2 end;

        public G2Line(float2 _start,float2 _end)
        {
            start = _start;
            end = _end;
        }
    }
    
    [Serializable]
    public struct GLine:ISerializationCallbackReceiver
    {
        public float3 start;
        public float3 end;
        [HideInInspector]public float3 direction;
        [HideInInspector]public float length;
        public float3 GetPoint(float _distance) => start + direction * _distance;

        public GLine(float3 _start, float3 _end)
        {
            start = _start;
            end = _end;
            direction = default;
            length = default;
            Ctor();
        }

        public GLine(float3 _start, float3 _direction, float _length) { 
            start = _start; 
            direction = _direction; 
            length = _length;
            end = start + direction * length;
        }
        
        void Ctor()
        {
            var offset = end - start;
            direction = offset.normalize();
            length = offset.magnitude();
        }
        
        public static implicit operator GRay(GLine _line)=>new GRay(_line.start,_line.direction);
        
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize() { Ctor(); }
    }

    [Serializable]
    public struct PLine:IEquatable<PLine>
    {
        public int start;
        public int end;

        public PLine(int _start,int _end)
        {
            start = _start;
            end = _end;
        }

        
        #region Implements
        public bool Equals(PLine other) => start == other.start && end == other.end;
        public bool EqualsNonVector(PLine other) => (start == other.start && end == other.end) 
                                                    || (end == other.start && start == other.end);
        public override bool Equals(object obj) => obj is PLine other && Equals(other);
        public override int GetHashCode()=> HashCode.Combine(start, end);
        #endregion
    }
    
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
        #endregion
    }

    [Serializable]
    public struct G2Ray
    {
        public float2 origin;
        public float2 direction;

        public G2Ray(float2 _position, float2 _direction)
        {
            origin = _position;
            direction = _direction;
        }

        public float2 GetPoint(float _distance) => origin + direction * _distance;
    }
}