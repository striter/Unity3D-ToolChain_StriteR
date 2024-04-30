using Unity.Mathematics;

namespace Runtime.Geometry.Validation
{
    using static math;
    public static partial class UGeometry
    {
        public static float Distance(GRay _ray,GPlane _plane)
        {
            float nrO = dot(_plane.normal, _ray.origin);
            float nrD = dot(_plane.normal, _ray.direction);
            return (_plane.distance - nrO) / nrD;
        }
        
        public static float2 Distance(GRay _ray,GSphere _sphere)
        {
            RayIntersection.SphereCalculate(_ray,_sphere, out float dotOffsetDirection, out float discriminant);
            if (discriminant < 0)
                return -1;

            discriminant = sqrt(discriminant);
            float t0 = -dotOffsetDirection - discriminant;
            float t1 = -dotOffsetDirection + discriminant;
            if (t0 < 0)
                t0 = t1;
            return new float2(t0, t1);
        }
        public static float2 Distance(GRay _ray,GBox _box)
        {
            RayIntersection.AABBCalculate(_ray,_box, out float3 tmin, out float3 tmax);
            if (tmin.maxElement() > tmax.minElement())
                return -1;
            float dstA = max(max(tmin.x, tmin.y), tmin.z);
            float dstB = min(tmax.x, min(tmax.y, tmax.z));
            float dstToBox = max(0, dstA);
            float dstInsideBox = max(0, dstB - dstToBox);
            return new float2(dstToBox, dstInsideBox);
        }

        public static float2 Distance(G2Ray _ray,G2Box _box)
        {
            RayIntersection.AABBCalculate(_ray,_box, out float2 tmin, out float2 tmax);
            float dstA = max(tmin.x, tmin.y);
            float dstB = min(tmax.x, tmax.y);
            float dstToBox = max(0, dstA);
            float dstInsideBox = max(0, dstB - dstToBox);
            return new float2(dstToBox, dstInsideBox);
        }
        
        public static float2 Distance(GRay _ray,GConeUnheighted _cone)
        {
            float2 distances = RayIntersection.ConeCalculate(_ray,_cone);
            if (dot(_cone.normal, _ray.GetPoint(distances.x) - _cone.origin) < 0)
                distances.x = -1;
            if (dot(_cone.normal, _ray.GetPoint(distances.y) - _cone.origin) < 0)
                distances.y = -1;
            return distances;
        }

        public static float2 Distance(GRay _ray,GCone _cone)
        {
            var distances = RayIntersection.ConeCalculate(_ray,_cone);
            GPlane bottomPlane = new GPlane(_cone.normal, _cone.origin + _cone.normal * _cone.height);
            float rayPlaneDistance = Distance(_ray,bottomPlane);
            float sqrRadius = _cone.Radius;
            sqrRadius *= sqrRadius;
            if (lengthsq(_cone.Bottom - _ray.GetPoint(rayPlaneDistance)) > sqrRadius)
                rayPlaneDistance = -1;

            float surfaceDst = dot(_cone.normal, _ray.GetPoint(distances.x) - _cone.origin);
            if (surfaceDst < 0 || surfaceDst > _cone.height)
                distances.x = rayPlaneDistance;

            surfaceDst = dot(_cone.normal, _ray.GetPoint(distances.y) - _cone.origin);
            if (surfaceDst < 0 || surfaceDst > _cone.height)
                distances.y = rayPlaneDistance;
            return distances;
        }
        public static float2 Distance(GRay _ray,GEllipsoid _ellipsoid)
        {
            RayIntersection.EllipsoidCalculate(_ellipsoid,_ray,out var a,out var b,out var c,out var discriminant);
            if ( discriminant < 0 ) { return -1; }
            discriminant = sqrt(discriminant);
            float t0 = (-b - discriminant)/(2*a);
            float t1 = (-b + discriminant)/(2*a);

            if (t0 < 0)
                t0 = t1;
            return new float2(t0, t1);
        }
        public static float Distance(GLine _line, float3 _point)
        {
            var lineDirection = normalize(_line.end - _line.start);
            var pointToStart = _point - _line.start;
            return  length(cross(lineDirection, pointToStart));
        }

        public static float Distance(GPlane _plane, float3 _point)
        {
            // float nr = _point.x * _plane.normal.x + _point.y * _plane.normal.y + _point.z * _plane.normal.z +
            //            _plane.distance;
            // return nr / length(_plane.normal);
            
            return dot(_plane.normal, _point - _plane.position);
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