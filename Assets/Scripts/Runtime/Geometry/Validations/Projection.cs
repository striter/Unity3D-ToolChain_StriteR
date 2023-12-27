using Unity.Mathematics;

namespace Geometry.Validation
{
    using static math;
    public static partial class UGeometry
    {
        public static float3 Projection(this GPlane _projectionPlane,float3 _srcPoint, float3 _origin)
        {
            var ray = new GRay(_origin, _srcPoint - _origin);
            return ray.GetPoint(Distance(ray, _projectionPlane));
        }
            
        public static float Projection(this float3 _point, GPlane _plane)
        {
            return dot(_plane, _point.to4(1f));
            // float nr = _point.x * _plane.normal.x + _point.y * _plane.normal.y + _point.z * _plane.normal.z +
            //            _plane.distance;
            // return nr / math.length(_plane.normal);
        }
        public static float Projection(this GRay _ray, float3 _point)
        {
            return dot(_point - _ray.origin, _ray.direction);
        }

        public static float Projection(this GLine _line, float3 _point)
        {
            return clamp(Projection(_line.ToRay(), _point), 0, _line.length);
        }
        
        public static float2 Projection(this GRay _ray, GRay _dstRay)
        {
            var diff = _ray.origin - _dstRay.origin;
            var a01 = -dot(_ray.direction, _dstRay.direction);
            var b0 = dot(diff, _ray.direction);
            var b1 = -dot(diff, _dstRay.direction);
            var det = 1f - a01 * a01;
            return new float2((a01 * b1 - b0) / det, (a01 * b0 - b1) / det);
        }

        public static float2 Projection(this GRay _ray, GLine _line)
        {
            var projections = Projection(_line, _ray);
            projections.x = math.clamp(projections.x, 0, _line.length);
            projections.y = Projection(_ray, _line.GetPoint(projections.x));
            return projections;
        }
    }
}
