using Runtime.Geometry.Extension;
using Unity.Mathematics;

namespace Runtime.Geometry.Extension
{
    using static math;
    using static umath;
    public static partial class UGeometry
    {
        public static float3 Projection(this GPlane _projectionPlane,float3 _srcPoint, float3 _origin)
        {
            var ray = new GRay(_origin, _srcPoint - _origin);
            return ray.GetPoint(ray.IntersectDistance(_projectionPlane));
        }

        public static float3 Projection(this GPlane _projectionPlane,float3 _srcPoint)
        {
            var ray = new GRay(_srcPoint, _projectionPlane.normal);
            return ray.GetPoint(ray.IntersectDistance(_projectionPlane));
        }
        
        public static float3 Projection(this GTriangle _triangle,float3 _direction)
        {
            var normal = _triangle.normal;
            var center = _triangle.Origin;
            
            _direction -= dot(_direction, normal) * normal;    //Project on to plane
            _direction = _direction.normalize();
            
            var ray = new GRay(center, _direction);
            var hitPoint = center;
            var minDot = float.MaxValue;
            foreach (var edge in _triangle.GetEdges())
            {
                var projection = ray.Projection(edge);
                var projectionPoint = edge.GetPoint(projection.x);
                var projectDirection = projectionPoint - ray.origin;
                var dot = math.dot(projectDirection, _direction);

                if (!(dot >= 0) || !(minDot > dot)) 
                    continue;
                
                minDot = dot;
                hitPoint = projectionPoint;
            }
            
            return hitPoint;
        }
        
        public static float Projection(this GRay _ray, float3 _point) => dot(_point - _ray.origin, _ray.direction);

        public static float Projection(this GLine _line, float3 _point) => clamp(Projection(_line.ToRay(), _point), 0, _line.length);
        
        public static float3 Projection(this GCoordinates _axis,float3 _point) => ((GPlane)_axis).Projection(_point);

        public static float2 Projection(this GRay _ray, GRay _dstRay)   //x src ray projection, y dst ray projection
        {
            var diff = _ray.origin - _dstRay.origin;
            var a01 = -dot(_ray.direction, _dstRay.direction);
            var b0 = dot(diff, _ray.direction);
            var b1 = -dot(diff, _dstRay.direction);
            var det = 1f - a01 * a01;
            return new float2((a01 * b1 - b0) / det, (a01 * b0 - b1) / det);
        }

        public static float2 Projection(this GRay _ray, GLine _line)        //x line projection, y ray projection
        {
            var projections = Projection(_line, _ray);
            projections.y = clamp(projections.x, 0, _line.length);
            projections.x = Projection(_ray, _line.GetPoint(projections.y));
            return projections;
        }

        //&https://iquilezles.org/articles/sphereproj/
        public static float ScreenProjection(this GSphere _sphere, float4x4 _worldToCamera,float _pixelHeight, float _fovYInRad)
        {
            var o = mul(_worldToCamera,new float4(_sphere.Origin,1.0f)).xyz;
            return 0.5f *_pixelHeight* _sphere.radius / (tan(_fovYInRad / 2) * -o.z);
        }
    }
}
