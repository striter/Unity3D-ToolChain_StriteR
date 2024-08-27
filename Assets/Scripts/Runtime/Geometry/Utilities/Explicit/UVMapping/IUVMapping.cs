using System;
using Unity.Mathematics;
namespace Runtime.Geometry.Explicit.Sphere
{
    using static math;
    public interface IUVMapping
    {
        public float2 ToUV(float3 _position);
        public float3 ToPosition(float2 _uv);
        public int2 Tilling(int2 _pixelIndex, int _cellCount) => _pixelIndex % _cellCount;
    }

}