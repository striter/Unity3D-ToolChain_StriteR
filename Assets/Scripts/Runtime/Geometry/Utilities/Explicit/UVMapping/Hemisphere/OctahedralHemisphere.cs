using Unity.Mathematics;

namespace Runtime.Geometry.Explicit.Sphere
{
    using static math;
    
    public struct OctahedralHemisphere : ISphereUVMapping
    {
        public float3 ToPosition(float2 _uv)
        {
            var oct = _uv * 2 - 1;
            oct = new float2( oct.x + oct.y, oct.x - oct.y ) *0.5f;
            return normalize(new float3( oct.x,1f - dot( 1.0f, abs(oct) ),oct.y ));
        }

        public float2 ToUV(float3 _direction)
        {
            _direction.y = max(0.01f, _direction.y);

            _direction.xz /= dot( 1.0f, abs(_direction) );
            var oct = new float2(_direction.x + _direction.z, _direction.x - _direction.z);
            return oct * .5f + .5f;
        }

        public int2 Tilling(int2 _pixelIndex, int _cellCount)
        {
            _pixelIndex = clamp(_pixelIndex,0,_cellCount - 1);
            return _pixelIndex;
        }

        public static readonly OctahedralHemisphere kDefault = default;
        public bool IsHemisphere => true;
    }
}