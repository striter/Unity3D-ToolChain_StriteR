using Unity.Mathematics;

namespace Runtime.Geometry.Explicit.Sphere
{
    using static math;
    
    public struct OctahedralHemisphere : IUVMapping
    {
        public float3 ToPosition(float2 _uv)
        {
            var oct = _uv * 2 - 1;
            oct = new float2( oct.x + oct.y, oct.x - oct.y ) *0.5f;
            return normalize(new float3( oct.x,1f - dot( 1.0f, abs(oct) ),oct.y ));
        }

        public float2 ToUV(float3 N)
        {
            N.xz /= dot( 1.0f, abs(N) );
            var oct = new float2(N.x + N.z, N.x - N.z);
            return oct * .5f + .5f;
        }

        public static readonly OctahedralHemisphere kDefault = default;
    }
}