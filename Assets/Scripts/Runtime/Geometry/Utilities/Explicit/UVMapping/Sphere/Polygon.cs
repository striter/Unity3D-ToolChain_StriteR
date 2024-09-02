using Unity.Mathematics;

namespace Runtime.Geometry.Extension.Sphere
{
    using static math;
    using static kmath;
    public struct Polygon : ISphereUVMapping
    {
        public bool geodesic;
        public int axisCount;
        
        public float2 ToUV(float3 _position)
        {
            throw new System.NotImplementedException();
        }

        public float3 ToPosition(float2 _uv)
        {
            var k = (int)(_uv.x * (axisCount - 1));
            var axis = UCubeExplicit.GetOctahedronRhombusAxis(k,axisCount);
            // _uv.x = umath.invLerp(k, k + 1, _uv.x);
            return GetPoint(_uv,axis,geodesic);
        }
        
        public static float3 GetPoint(float2 _uv,Axis _axis,bool _geodesic)        //uv [0,1)
        {
            float3 position = 0;
            if (_geodesic)
            {
                sincos(kPI*_uv.y,out var sine,out position.y);
                position += _axis.origin + math.lerp(_axis.uDir * sine, _axis.vDir * sine,_uv.x);
            }
            else
            {
                var posUV = (_uv.x+_uv.y)<=1?_uv:(1f-_uv.yx);
                position = new float3(0,_uv.x+_uv.y-1, 0) + _axis.origin + posUV.x * _axis.uDir + posUV.y * _axis.vDir;
            }
            return normalize(position);
        }
        
        public static Polygon kDefault = new Polygon(){axisCount = 4,geodesic = true};
        public static Polygon kGeodesic = new Polygon(){axisCount = 4,geodesic = false};
        public static Polygon GetHelper(int _axisCount,bool _geodesic) => new Polygon(){axisCount = _axisCount,geodesic = _geodesic};
        public bool IsHemisphere => false;
    }
}