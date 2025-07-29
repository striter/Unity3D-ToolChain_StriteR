using Unity.Mathematics;

namespace Runtime.Geometry.Extension
{
    using static math;
    public static partial class UGeometry
    {
        public static bool Intersect(this G2Circle _sphere, G2Box _box)
        {
            var sphereCenter = _sphere.center;
            var sphereRadius = _sphere.radius;
            var boxMin = _box.min;
            var boxMax = _box.max;

            var closestPoint = new float2(
                max(boxMin.x, min(sphereCenter.x, boxMax.x)),
                max(boxMin.y, min(sphereCenter.y, boxMax.y))
            );

            return (closestPoint - sphereCenter).sqrmagnitude() <= sphereRadius * sphereRadius;
        }
        
        public static bool Intersect(this G2Line _line, G2Line _line2)
        {
            var proj = ((G2Ray)_line).Projection(_line2);
            return proj.x >= 0 && proj.x <= _line.length && proj.y >= 0 && proj.y <= _line2.length;
        }

        public static bool Intersect(this G2Box _box,G2Circle _sphere) => Intersect(_sphere, _box);
        public static bool Intersect(this G2Circle _circle, float2 _point) => _circle.SDF(_point) <= float.Epsilon;
    }
}