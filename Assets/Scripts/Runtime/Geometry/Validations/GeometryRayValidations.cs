using Unity.Mathematics;

namespace Geometry.Validation
{
    public static partial class UGeometryValidation
    {
        public static class Ray
        {
        #region Projection
            public static float Projection(GRay _ray,float3 _point)
            {
                return math.dot(_point - _ray.origin, _ray.direction);
            }
            
            public static float2 Projection(GRay _ray, GRay _dstRay)
            {
                float3 diff = _ray.origin - _dstRay.origin;
                float a01 = -math.dot(_ray.direction, _dstRay.direction);
                float b0 = math.dot(diff, _ray.direction);
                float b1 = -math.dot(diff, _dstRay.direction);
                float det = 1f - a01 * a01;
                return new float2((a01 * b1 - b0) / det, (a01 * b0 - b1) / det);
            }

            public static float2 Projection(GRay _ray,GLine _line)
            {
                float2 projections = Projection(_line, _ray);
                projections.x = math.clamp(projections.x, 0, _line.length);
                projections.y = Projection(_ray,_line.GetPoint(projections.x));
                return projections;
            }
                
            public static bool Projection(GRay _ray, GPlane _plane, out float3 _hitPoint)
            {
                float distance = Distances(_ray,_plane);
                _hitPoint = _ray.GetPoint(distance);
                return distance != 0;
            }
        #endregion
            
        #region Distances
        
            static bool TriangleCalculate(float3 _vertex0, float3 _vertex1, float3 _vertex2, float3 _rayOrigin,float3 _rayDir, out float _t, out float _u, out float _v) //Möller-Trumbore
            {
                _t = 0;
                _u = 0;
                _v = 0;
                float3 E1 = _vertex1 - _vertex0;
                float3 E2 = _vertex2 - _vertex0;
                float3 P = math.cross(_rayDir, E2);
                float determination = math.dot(E1, P);
                float3 T;
                if (determination > 0)
                {
                    T = _rayOrigin - _vertex0;
                }
                else
                {
                    T = _vertex0 - _rayOrigin;
                    determination = -determination;
                }

                if (determination < float.Epsilon)
                    return false;

                _u = math.dot(T, P);
                if (_u < 0f || _u > determination)
                    return false;
                float3 Q = math.cross(T, E1);
                _v = math.dot(_rayDir, Q);
                if (_v < 0f || (_u + _v) > determination)
                    return false;

                _t = math.dot(E2, Q);
                float invDetermination = 1 / determination;
                _t *= invDetermination;
                _u *= invDetermination;
                _v *= invDetermination;
                return true;
            }

            public static bool Intersect(GTriangle _triangle, GRay _ray, bool _rayDirectionCheck, out float _distance)
            {
                if (!TriangleCalculate( _ray.origin, _triangle[0], _triangle[1], _triangle[2],_ray.direction, out _distance, out float u, out float v))
                    return false;
                return !_rayDirectionCheck || _distance > 0;
            }

            public static bool Intersect(GTriangle _triangle, GRay _ray, bool _rayDirectionCheck, bool _triangleDirectionCheck, out float _distance)
            {
                if (!TriangleCalculate(_triangle[0], _triangle[1], _triangle[2], _ray.origin, _ray.direction, out _distance, out float u, out float v))
                    return false;
                bool intersect = true;
                intersect &= !_rayDirectionCheck || _distance > 0;
                intersect &= !_triangleDirectionCheck || math.dot(_triangle.normal, _ray.direction) < 0;
                return intersect;
            }

            public static float Distances( GRay _ray,GPlane _plane)
            {
                float nrO = math.dot(_plane.normal, _ray.origin);
                float nrD = math.dot(_plane.normal, _ray.direction);
                return (_plane.distance - nrO) / nrD;
            }

            static void SphereCalculate(GRay _ray, GSphere _sphere, out float _dotOffsetDirection, out float _discriminant)
            {
                float3 shift = _ray.origin - _sphere.center;
                _dotOffsetDirection = math.dot(_ray.direction, shift);
                float sqrRadius = _sphere.radius * _sphere.radius;
                float radiusDelta = math.dot(shift, shift) - sqrRadius;
                _discriminant = -1;
                if (_dotOffsetDirection > 0 && radiusDelta > 0)
                    return;

                float dotOffset = math.dot(shift, shift);
                _discriminant = _dotOffsetDirection * _dotOffsetDirection - dotOffset + sqrRadius;
            }

            public static bool Intersect( GRay _ray,GSphere _sphere)
            {
                SphereCalculate(_ray,_sphere, out float dotOffsetDirection, out float discriminant);
                return discriminant >= 0;
            }

            public static float2 Distances( GRay _ray,GSphere _sphere)
            {
                SphereCalculate(_ray,_sphere, out float dotOffsetDirection, out float discriminant);
                if (discriminant < 0)
                    return -1;

                discriminant = math.sqrt(discriminant);
                float t0 = -dotOffsetDirection - discriminant;
                float t1 = -dotOffsetDirection + discriminant;
                if (t0 < 0)
                    t0 = t1;
                return new float2(t0, t1);
            }

            private static void EllipsoidCalculate(GEllipsoid _ellipsoid, GRay _ray,out float _a,out float _b,out float _c,out float _discriminant)
            {
                var shift = _ray.origin - _ellipsoid.center;
                _a = _ray.direction.x*_ray.direction.x/(_ellipsoid.radius.x*_ellipsoid.radius.x)
                    + _ray.direction.y*_ray.direction.y/(_ellipsoid.radius.y*_ellipsoid.radius.y)
                    + _ray.direction.z*_ray.direction.z/(_ellipsoid.radius.z*_ellipsoid.radius.z);
                _b = 2*shift.x*_ray.direction.x/(_ellipsoid.radius.x*_ellipsoid.radius.x)
                    + 2*shift.y*_ray.direction.y/(_ellipsoid.radius.y*_ellipsoid.radius.y)
                    + 2*shift.z*_ray.direction.z/(_ellipsoid.radius.z*_ellipsoid.radius.z);
                _c = shift.x*shift.x/(_ellipsoid.radius.x*_ellipsoid.radius.x)
                    + shift.y*shift.y/(_ellipsoid.radius.y*_ellipsoid.radius.y)
                    + shift.z*shift.z/(_ellipsoid.radius.z*_ellipsoid.radius.z) 
                     - 1;
                _discriminant = ((_b*_b)-(4*_a*_c));
            }
            public static bool Intersect(GRay _ray,GEllipsoid _ellipsoid)
            {
                EllipsoidCalculate(_ellipsoid,_ray,out var a,out var b,out var c,out var discriminant);
                return discriminant >= 0;
            }
            public static float2 Distances(GRay _ray,GEllipsoid _ellipsoid)
            {
                EllipsoidCalculate(_ellipsoid,_ray,out var a,out var b,out var c,out var discriminant);
                if ( discriminant < 0 ) { return -1; }
                discriminant = math.sqrt(discriminant);
                float t0 = (-b - discriminant)/(2*a);
                float t1 = (-b + discriminant)/(2*a);

                if (t0 < 0)
                    t0 = t1;
                return new float2(t0, t1);
            }
            
            static void AABBCalculate( GRay _ray,GBox _box, out float3 _tmin, out float3 _tmax)
            {
                var invRayDir = 1f/(_ray.direction);
                var t0 = (_box.min - _ray.origin)*(invRayDir);
                var t1 = (_box.max - _ray.origin)*(invRayDir);
                _tmin = math.min(t0, t1);
                _tmax = math.max(t0, t1);
            }

            public static bool Intersect(GRay _ray,GBox _box)
            {
                AABBCalculate(_ray,_box, out var tmin, out var tmax);
                return tmin.maxElement() <= tmax.minElement();
            }

            public static float2 Distances( GRay _ray,GBox _box)
            {
                AABBCalculate(_ray,_box, out float3 tmin, out float3 tmax);
                float dstA = math.max(math.max(tmin.x, tmin.y), tmin.z);
                float dstB = math.min(tmax.x, math.min(tmax.y, tmax.z));
                float dstToBox = math.max(0, dstA);
                float dstInsideBox = math.max(0, dstB - dstToBox);
                return new float2(dstToBox, dstInsideBox);
            }

            static float2 ConeCalculate(GRay _ray,GCone _cone)
            {
                float2 distances = -1f;
                float3 offset = _ray.origin - _cone.origin;

                float RDV = math.dot(_ray.direction, _cone.normal);
                float ODN = math.dot(offset, _cone.normal);
                float cosA = math.cos(kmath.kDeg2Rad * _cone.angle);
                float sqrCosA = cosA * cosA;

                float a = RDV * RDV - sqrCosA;
                float b = 2f * (RDV * ODN - math.dot(_ray.direction, offset) * sqrCosA);
                float c = ODN * ODN - math.dot(offset, offset) * sqrCosA;
                float determination = b * b - 4f * a * c;
                if (determination < 0)
                    return distances;
                determination = math.sqrt(determination);
                distances.x = (-b + determination) / (2f * a);
                distances.y = (-b - determination) / (2f * a);
                return distances;
            }
            public static float2 Distances(GRay _ray,GCone _cone)
            {
                float2 distances = ConeCalculate(_ray,_cone);
                if (math.dot(_cone.normal, _ray.GetPoint(distances.x) - _cone.origin) < 0)
                    distances.x = -1;
                if (math.dot(_cone.normal, _ray.GetPoint(distances.y) - _cone.origin) < 0)
                    distances.y = -1;
                return distances;
            }

            public static float2 Distances(GRay _ray,GHeightCone _cone)
            {
                var distances = ConeCalculate(_ray,_cone);
                GPlane bottomPlane = new GPlane(_cone.normal, _cone.origin + _cone.normal * _cone.height);
                float rayPlaneDistance = Distances(_ray,bottomPlane);
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

            
        #endregion
        }
    }
}