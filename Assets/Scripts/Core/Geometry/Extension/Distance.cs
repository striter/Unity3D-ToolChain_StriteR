using System;
using Unity.Mathematics;

namespace Runtime.Geometry.Extension
{
    using static math;
    public static partial class UGeometry
    {
        public static float Distance(GRay ray1, GRay ray2,float _parallelEpslion = 0.9998f)
        {
            var rayOriginDiff = ray2.origin - ray1.origin;
            var dotProduct = dot(ray1.direction, ray2.direction);
            if (dotProduct > _parallelEpslion)
                return dot(rayOriginDiff, ray1.direction);
            
            var a = dot(ray1.direction, ray1.direction);
            var b = dot(ray1.direction, ray2.direction);
            var c = dot(ray2.direction, ray2.direction);
            var d = dot(ray1.direction, rayOriginDiff);
            var e = dot(ray2.direction, rayOriginDiff);

            var t1 = (e - b * d) / (a - b * b);
            var t2 = (d - b * e) / (a - b * b);

            var closestPointRay1 = ray1.origin + t1 * ray1.direction;
            var closestPointRay2 = ray2.origin + t2 * ray2.direction;

            return length(closestPointRay1 - closestPointRay2);
        }
        
        public static float Distance(GSphere _sphere, GBox _box)
        {
            var sphereCenter = _sphere.center;
            var sphereRadius = _sphere.radius;
            var boxMin = _box.min;
            var boxMax = _box.max;

            var closestPoint = new float3(
                max(boxMin.x, min(sphereCenter.x, boxMax.x)),
                max(boxMin.y, min(sphereCenter.y, boxMax.y)),
                max(boxMin.z, min(sphereCenter.z, boxMax.z))
            );

            return length(closestPoint - sphereCenter) - sphereRadius;
        }
    }
}