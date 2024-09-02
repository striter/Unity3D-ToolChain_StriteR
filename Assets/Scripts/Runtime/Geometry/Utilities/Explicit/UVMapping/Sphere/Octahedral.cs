using Unity.Mathematics;

namespace Runtime.Geometry.Extension.Sphere
{
    using static math;
    public struct Octahedral : ISphereUVMapping
    {
        public float3 ToPosition(float2 _uv)
        {
            var oct =  _uv * 2 - 1;
            var N = float3( oct, 1.0f - dot( 1.0f, abs(oct) ) );
            if( N.z < 0 )
                N.xy = ( 1 - abs(N.yx) ) * sign( N.xy );
            return normalize(N);
        }
        
        public float2 ToUV(float3 N)
        {
            N /= dot( 1.0f, abs(N) );
            if (N.z <= 0)
                N.xy = (1 - abs(N.yx)) * sign(N.xy);
            var oct = N.xy;
            return oct * .5f + .5f;
        }
        public static readonly Octahedral kDefault = default;
        public bool IsHemisphere => false;
    }
}