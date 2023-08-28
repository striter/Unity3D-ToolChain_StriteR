using Unity.Mathematics;

namespace Geometry.Validation
{
    public static partial class UGeometry
    {
        public static class Projection
        {
            public static float Eval(float3 _point, GPlane _plane)
            {
                return math.dot(_plane, _point.to4(1f));
                // float nr = _point.x * _plane.normal.x + _point.y * _plane.normal.y + _point.z * _plane.normal.z +
                //            _plane.distance;
                // return nr / math.length(_plane.normal);
            }
            public static float Eval(GRay _ray, float3 _point)
            {
                return math.dot(_point - _ray.origin, _ray.direction);
            }

            public static float2 Eval(GRay _ray, GRay _dstRay)
            {
                float3 diff = _ray.origin - _dstRay.origin;
                float a01 = -math.dot(_ray.direction, _dstRay.direction);
                float b0 = math.dot(diff, _ray.direction);
                float b1 = -math.dot(diff, _dstRay.direction);
                float det = 1f - a01 * a01;
                return new float2((a01 * b1 - b0) / det, (a01 * b0 - b1) / det);
            }

            public static float2 Eval(GRay _ray, GLine _line)
            {
                float2 projections = Eval(_line, _ray);
                projections.x = math.clamp(projections.x, 0, _line.length);
                projections.y = Eval(_ray, _line.GetPoint(projections.x));
                return projections;
            }
        }
    }
}
