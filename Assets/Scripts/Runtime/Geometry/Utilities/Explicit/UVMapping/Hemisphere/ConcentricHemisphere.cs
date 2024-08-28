using Unity.Mathematics;

namespace Runtime.Geometry.Explicit.Sphere
{
    using static math;
    using static kmath;
    public struct ConcentricHemisphere : ISphereUVMapping
    {
        public float2 ToUV(float3 _direction)
        {
            var x = _direction.x;
            var z = _direction.z;

            var r = sqrt(x * x + z * z);
            var u = r * r;
            var v = atan2(x, z) / kPI2;
            v += v < 0 ? 1.0f : 0;
            return new float2(u, v);
        }

        public float3 ToPosition(float2 _uv)
        {
            var u = _uv.x;
            var v = _uv.y;
            var r = sqrt(u);
            sincos(kPI2 * v,out var sinTheta,out var cosTheta);
            return new float3(sinTheta * r,  sqrt(1 - u),  cosTheta * r);
        }

        public int2 Tilling(int2 _pixelIndex, int _cellCount)
        {
            _pixelIndex.x = clamp(_pixelIndex.x,0,_cellCount - 1);
            return _pixelIndex;
        }

        public bool IsHemisphere => true;
        public static readonly ConcentricHemisphere kDefault = default;
    }
}