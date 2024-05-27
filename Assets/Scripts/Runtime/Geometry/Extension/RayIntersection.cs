using Unity.Mathematics;
using static Unity.Mathematics.math;
namespace Runtime.Geometry.Extension
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

    }
}