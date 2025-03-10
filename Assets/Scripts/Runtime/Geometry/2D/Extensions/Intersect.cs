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
        public static bool Intersect(this G2Box _box,G2Circle _sphere) => Intersect(_sphere, _box);
    }
}