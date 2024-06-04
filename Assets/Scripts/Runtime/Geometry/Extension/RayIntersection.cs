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

    }
}