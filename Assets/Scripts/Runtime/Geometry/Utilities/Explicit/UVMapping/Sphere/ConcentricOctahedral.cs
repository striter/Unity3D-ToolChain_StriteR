using Unity.Mathematics;

namespace Runtime.Geometry.Explicit.Sphere
{
    using static kmath;
    using static math;
    using static umath;

    //https://fileadmin.cs.lth.se/graphics/research/papers/2008/simdmapping/clarberg_simdmapping08_preprint.pdf
    public struct ConcentricOctahedral : ISphereUVMapping
    {
        public float3 ToPosition(float2 _uv)
        {
            var oct = 2 * _uv - kfloat2.one;
            var u = oct.x;
            var v = oct.y;
            var d = 1 - (abs(u) + abs(v));
            var r = 1 - abs(d);
            var z = signNonZero(d) * (1 - r * r);
    
            var theta = kPIDiv4 * ( r == 0 ? 1: ( abs(v) - abs(u)) / r + 1 );
            var sinTheta = signNonZero(v) * sin(theta);
            var cosTheta = signNonZero(u) * cos(theta);
            var radius = sqrt(2 - r * r);
            return new float3(cosTheta * r * radius, sinTheta * r * radius, z);
        }

        public float2 ToUV(float3 _direction)
        {
            var absD = abs(_direction);
            var x = absD.x;
            var y = absD.y;
            var z = absD.z;

            var r = sqrt(1.0f - z);
            var a = absD.xy.maxElement();
            var b = absD.xy.minElement();
            b = a == 0f ? 0f : b / a;

            var phi = atan_Fast_2DivPI(b);

            if (x < y) phi = 1.0f - phi;
        
            var v = phi * r;
            var u = r - v;
            if (_direction.z < 0f)
            {
                var tmp = u;
                u = 1f - v;
                v = 1f - tmp;
            }
            u = flipSign(u, signNonZero(_direction.x));
            v = flipSign(v, signNonZero(_direction.y));
            return new float2(u, v) * .5f + .5f;
        }

        public int2 Tilling(int2 _pixelIndex, int _cellCount)
        {
            var N = _cellCount;
            var mirror = (_pixelIndex.x ^ _pixelIndex.y) & N;
            if (mirror != 0)
            {
                _pixelIndex.x = N - 1 - _pixelIndex.x;
                _pixelIndex.y = N - 1 - _pixelIndex.y;
            }
            _pixelIndex &= (N - 1);
            return _pixelIndex;
        }

        public static readonly ConcentricOctahedral kDefault = default;
        public bool IsHemisphere => false;
    }
}