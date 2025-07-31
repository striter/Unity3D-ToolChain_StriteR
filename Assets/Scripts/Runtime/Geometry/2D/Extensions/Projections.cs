using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Runtime.Geometry.Extension
{
    public static partial class UGeometry
    {
        public static float Projection(this G2Ray _ray,float2 _point) => dot(_point - _ray.origin, _ray.direction);
        
        public static float2 Projection(this G2Ray _ray, G2Ray _dstRay)
        {
            var diff = _ray.origin - _dstRay.origin;
            var a01 = -dot(_ray.direction, _dstRay.direction);
            var b0 = dot(diff, _ray.direction);
            var b1 = -dot(diff, _dstRay.direction);
            var det = 1f - a01 * a01;
            return new float2((a01 * b1 - b0) / det, (a01 * b0 - b1) / det);
        }

        public static float2 Projection(this G2Line _line, G2Line _line2)
        {
            var proj = ((G2Ray)_line).Projection(_line2);
            return new float2(math.clamp(proj.x, 0, _line.length), math.clamp(proj.y, 0, _line2.length));
        }

        public static float2 Projection(this G2Box _box, float2 _direction)
        {
            var ray = new G2Ray(_box.center, _direction.normalize());
            return ray.GetPoint(ray.Distance(_box).sum());
        }

    }
}