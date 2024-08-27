using Runtime.Geometry.Extension;
using Unity.Mathematics;

namespace Runtime.Geometry
{
    public partial struct GTriangle
    {
        
        public float3 GetBarycenter() => GetPoint(.25f);
        public float3 GetPoint(float2 _uv) => GetPoint(_uv.x, _uv.y);
        public float3 GetPoint(float _u,float _v) => V0 + _u * +uOffset + _v * vOffset;
        public float GetArea()
        {
#if true        //https://iquilezles.org/articles/trianglearea/
            var A = uOffset.sqrmagnitude();
            var B = vOffset.sqrmagnitude();
            var C = (V2 - V1).sqrmagnitude();
            var powS = (2 * A * B + 2 * B * C + 2 * C * A - A * A - B * B - C * C) / 16f;
            return math.sqrt(powS);
#else
            return math.length(math.cross(uOffset, vOffset)) / 2;
#endif
        }

        public float2 GetUV(float3 _point) => axis.GetUV(_point);
        public float3 GetWeightsToPoint(float3 _point)
        {
            var uv = GetUV(_point);
            return new float3(1 - uv.sum(),uv.x, uv.y);
        }
    }
    
    public static class GTriangle_Extension
    {
        public static GTriangle to3xz(this G2Triangle _triangle2 )=> new GTriangle(_triangle2.V0.to3xz(),_triangle2.V1.to3xz(),_triangle2.V2.to3xz());
        public static GTriangle shrink(this GTriangle _triangle,float _value) => new GTriangle(math.lerp(_triangle.baryCentre,_triangle.V0,_value)
            , math.lerp(_triangle.baryCentre,_triangle.V1,_value)
            , math.lerp(_triangle.baryCentre,_triangle.V2,_value));
    }
}