using System;
using Unity.Mathematics;

namespace Geometry
{
    public partial struct GSphere
    {
        public float3 center;
        [Clamp(0)] public float radius;
        public GSphere(float3 _center,float _radius) { center = _center;radius = _radius; }
    }

    [Serializable]
    public partial struct GSphere : IShape3D
    {
        public static readonly GSphere kOne = new GSphere(float3.zero, .5f);
        public static readonly GSphere kZero = new GSphere(0,0);
        public float3 Center => center;
        public float3 GetSupportPoint(float3 _direction) => center + _direction.normalize() * radius;
        
        
        public static GSphere operator +(GSphere _src, float3 _dst) => new GSphere(_src.center+_dst,_src.radius);
        public static implicit operator float4(GSphere _src) => new float4(_src.center,_src.radius);
        public bool Contains(float3 _p, float _bias = float.Epsilon) =>math.lengthsq(_p - center) < radius * radius + _bias;
        public bool Contains(GSphere _sphere) =>math.lengthsq(_sphere.center - center) < radius * radius + _sphere.radius;
    }

}