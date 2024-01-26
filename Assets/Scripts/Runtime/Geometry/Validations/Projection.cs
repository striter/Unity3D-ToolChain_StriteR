using Unity.Mathematics;

namespace Runtime.Geometry.Validation
{
    using static math;
    public static partial class UGeometry
    {
        public static float3 Projection(this GPlane _projectionPlane,float3 _srcPoint, float3 _origin)
        {
            var ray = new GRay(_origin, _srcPoint - _origin);
            return ray.GetPoint(Distance(ray, _projectionPlane));
        }

        public static float3 Projection(this GPlane _projectionPlane,float3 _srcPoint)
        {
            var ray = new GRay(_srcPoint, _projectionPlane.normal);
            return ray.GetPoint(Distance(ray, _projectionPlane));
        }
        
        public static float3 Projection(this GTriangle _triangle,float3 _direction)
        {
            var normal = _triangle.normal;
            var center = _triangle.Center;
            
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
        
        public static float Projection(this GRay _ray, float3 _point)
        {
            return dot(_point - _ray.origin, _ray.direction);
        }

        public static float Projection(this GLine _line, float3 _point)
        {
            return clamp(Projection(_line.ToRay(), _point), 0, _line.length);
        }
        
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
            projections.x = clamp(projections.x, 0, _line.length);
            projections.y = Projection(_ray, _line.GetPoint(projections.x));
            return projections;
        }

        public static float2 Projection(this G2Box _box, float2 _direction)
        {
            var ray = new G2Ray(_box.center, _direction.normalize());
            return ray.GetPoint(Distance(ray, _box).sum());
        }
    }
}
