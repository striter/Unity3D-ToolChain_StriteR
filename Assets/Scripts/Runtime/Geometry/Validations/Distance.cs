using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Geometry.Validation
{
    public static partial class UGeometry
    {
  #region Ray
            public static float Distance(GRay _ray,GPlane _plane)
            {
                float nrO = math.dot(_plane.normal, _ray.origin);
                float nrD = math.dot(_plane.normal, _ray.direction);
                return (_plane.distance - nrO) / nrD;
            }
            
            public static float2 Distance(GRay _ray,GSphere _sphere)
            {
                RayIntersection.SphereCalculate(_ray,_sphere, out float dotOffsetDirection, out float discriminant);
                if (discriminant < 0)
                    return -1;

                discriminant = math.sqrt(discriminant);
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
                float dstA = math.max(math.max(tmin.x, tmin.y), tmin.z);
                float dstB = math.min(tmax.x, math.min(tmax.y, tmax.z));
                float dstToBox = math.max(0, dstA);
                float dstInsideBox = math.max(0, dstB - dstToBox);
                return new float2(dstToBox, dstInsideBox);
            }

            public static float2 Distance(G2Ray _ray,G2Box _box)
            {
                RayIntersection.AABBCalculate(_ray,_box, out float2 tmin, out float2 tmax);
                float dstA =math.max(tmin.x, tmin.y);
                float dstB = math.min(tmax.x, tmax.y);
                float dstToBox = math.max(0, dstA);
                float dstInsideBox = math.max(0, dstB - dstToBox);
                return new float2(dstToBox, dstInsideBox);
            }
            
            public static float2 Distance(GRay _ray,GConeUnheighted _cone)
            {
                float2 distances = RayIntersection.ConeCalculate(_ray,_cone);
                if (math.dot(_cone.normal, _ray.GetPoint(distances.x) - _cone.origin) < 0)
                    distances.x = -1;
                if (math.dot(_cone.normal, _ray.GetPoint(distances.y) - _cone.origin) < 0)
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
                if (math.lengthsq(_cone.Bottom - _ray.GetPoint(rayPlaneDistance)) > sqrRadius)
                    rayPlaneDistance = -1;

                float surfaceDst = math.dot(_cone.normal, _ray.GetPoint(distances.x) - _cone.origin);
                if (surfaceDst < 0 || surfaceDst > _cone.height)
                    distances.x = rayPlaneDistance;

                surfaceDst = math.dot(_cone.normal, _ray.GetPoint(distances.y) - _cone.origin);
                if (surfaceDst < 0 || surfaceDst > _cone.height)
                    distances.y = rayPlaneDistance;
                return distances;
            }
            public static float2 Distance(GRay _ray,GEllipsoid _ellipsoid)
            {
                RayIntersection.EllipsoidCalculate(_ellipsoid,_ray,out var a,out var b,out var c,out var discriminant);
                if ( discriminant < 0 ) { return -1; }
                discriminant = math.sqrt(discriminant);
                float t0 = (-b - discriminant)/(2*a);
                float t1 = (-b + discriminant)/(2*a);

                if (t0 < 0)
                    t0 = t1;
                return new float2(t0, t1);
            }
            #endregion


    }
}