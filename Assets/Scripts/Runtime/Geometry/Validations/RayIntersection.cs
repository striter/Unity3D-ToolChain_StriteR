using Unity.Mathematics;
using static Unity.Mathematics.math;
namespace Runtime.Geometry.Validation
{
    public static class RayIntersection
    {
        //https://iquilezles.org/articles/hackingintersector/
        public static void TriangleCalculate(GRay _ray, GTriangle _triangle, out float _u, out float _v)
        {
            var v1v0 = _triangle.V1 - _triangle.V0;
            var v2v0 = _triangle.V2 - _triangle.V0;
            var rov0 = _ray.origin - _triangle.V0;
            var rd = _ray.direction;
            var n = cross(v1v0, v2v0);
            var q = cross(rov0, rd);
            var d = 1.0f / dot(n, rd);
            _u = d * dot(-q, v2v0);
            _v = d * dot(q, v1v0);
        }
        
        public static bool TriangleCalculate(GRay _ray, GTriangle _triangle, out float _u, out float _v,out float _t) //Möller-Trumbore
        {
            var _rayOrigin = _ray.origin;
            var _rayDir = _ray.direction;
            var _vertex0 = _triangle.V0;
            var _vertex1 = _triangle.V1;
            var _vertex2 = _triangle.V2;
            
            _t = 0;
            _u = 0;
            _v = 0;
            
            var E1 = _vertex1 - _vertex0;
            var E2 = _vertex2 - _vertex0;
            var P = cross(_rayDir, E2);
            var determination = dot(E1, P);
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

            _u = dot(T, P);
            if (_u < 0f || _u > determination)
                return false;
            float3 Q = cross(T, E1);
            _v = dot(_rayDir, Q);
            if (_v < 0f || (_u + _v) > determination)
                return false;

            _t = dot(E2, Q);
            var invDetermination = 1 / determination;
            _u *= invDetermination;
            _v *= invDetermination;
            _t *= invDetermination;
            return true;
        }

        public static void SphereCalculate(GRay _ray, GSphere _sphere, out float _dotOffsetDirection, out float _discriminant)
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
        public static void EllipsoidCalculate(GEllipsoid _ellipsoid, GRay _ray,out float _a,out float _b,out float _c,out float _discriminant)
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
        public static void AABBCalculate(GRay _ray,GBox _box, out float3 _tmin, out float3 _tmax)
        {
            var invRayDir = 1f/(_ray.direction);
            var t0 = (_box.min - _ray.origin)*(invRayDir);
            var t1 = (_box.max - _ray.origin)*(invRayDir);
            _tmin = math.min(t0, t1);
            _tmax = math.max(t0, t1);
        }
        public static void AABBCalculate(G2Ray _ray,G2Box _box, out float2 _tmin, out float2 _tmax)
        {
            var invRayDir = 1f/(_ray.direction);
            var t0 = (_box.min - _ray.origin)*(invRayDir);
            var t1 = (_box.max - _ray.origin)*(invRayDir);
            _tmin = math.min(t0, t1);
            _tmax = math.max(t0, t1);
        }
        public static float2 ConeCalculate(GRay _ray,GConeUnheighted _cone)
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
    }
}