using Unity.Mathematics;

namespace Runtime.Geometry.Explicit.Sphere
{
    using static math;
    using static kmath;
    public struct ConcentricHemisphere : IUVMapping
    {
        public float2 ToUV(float3 _direction)
        {
            var x = _direction.x;
            var z = _direction.z;
            var r = sqrt(x * x + z * z);
            var u = r * r;
            var v = atan2(x, z) / kPI2;
            return new float2(u, v);
        }

        public float3 ToPosition(float2 _uv)
        {
            var u = _uv.x;
            var v = _uv.y;
            var r = sqrt(u);
            var theta =  kPI2 * v;
            var sinTheta = sin(theta);
            var cosTheta = cos(theta);
            return new float3(sinTheta * r,  sqrt(1 - u),  cosTheta * r);
        }
        public static readonly ConcentricHemisphere kDefault = default;
    }
}