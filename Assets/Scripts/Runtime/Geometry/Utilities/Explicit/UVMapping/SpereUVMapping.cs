using System;
using Unity.Mathematics;

namespace Runtime.Geometry.Explicit.Sphere
{
    using static math;
    public enum ESphereMapping
    {
        Cube,
        Octahedral,
        ConcentricOctahedral,
        
        ConcentricHemisphere,
        OctahedralHemisphere,
    }
    public static class ISphereUVMapping_Extension
    {
        public static IUVMapping GetHelper(this ESphereMapping _mapping) =>_mapping switch {
            ESphereMapping.Cube => Cube.kDefault,
            ESphereMapping.Octahedral => Octahedral.kDefault,
            ESphereMapping.ConcentricOctahedral => ConcentricOctahedral.kDefault,
            ESphereMapping.ConcentricHemisphere => ConcentricHemisphere.kDefault,
            ESphereMapping.OctahedralHemisphere => OctahedralHemisphere.kDefault,
            _ => throw new ArgumentOutOfRangeException()
        };

        public static float3 UVToSphere(this ESphereMapping _mapping, float2 _uv) => _mapping.GetHelper().ToPosition(_uv);

        public static float2 SphereToUV(this ESphereMapping _mapping, float3 _direction) => _mapping.GetHelper().ToUV(_direction);

        public static void InvBilinearInterpolate(this ESphereMapping _mapping, float2 _uv,int _cellCount,out G2Quad _corners,out float4 _weights) //uv weights
        {
            var s = _uv.x;
            var t = _uv.y;
            var N = (float)_cellCount;
            var x = (int)floor(s * N - .5f) ;
            var y = (int)floor(t * N - .5f);
            var aX = (s * N - 0.5f) - x;
            var aY = (t * N - 0.5f) - y;

            var cellIndex = new int2(x, y);
            _corners = new G2Quad(cellIndex,cellIndex + kfloat2.right,cellIndex + kfloat2.right + kfloat2.up,cellIndex  + kfloat2.up);
            _weights = new float4((1 - aX) * (1 - aY), aX * (1 - aY) , aX * aY, ( 1 - aX ) * aY);

            for (var i = 0; i < 4; i++)
                _corners[i] = _mapping.GetHelper().Tilling((int2)_corners[i], _cellCount);
            _corners /= N;
        }
    }
}