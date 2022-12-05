using Unity.Mathematics;
namespace Geometry
{
    public static class UGeometryMesh
    {
        public static float3 CubeToSphere(float3 _point)
        {
            float3 sqrP = _point * _point;
            return _point * math.sqrt(1f - (sqrP.yxx + sqrP.zzy) / 2f + sqrP.yxx * sqrP.zzy / 3f);
        }

        public static float2 SphereToUV(float3 _point,bool _poleValidation=false)
        {
            float2 texCoord=new float2(math.atan2(_point.x, -_point.z) / -KMath.kPI2, math.asin(_point.y) / KMath.kPI)+.5f;
            if (_poleValidation&&texCoord.x<1e-6f)
                texCoord.x = 1f;
            return texCoord;
        }
    }

}