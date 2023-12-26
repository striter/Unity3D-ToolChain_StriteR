using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Geometry.Validation
{
    public static partial class UGeometry
    {            
        public static bool Intersect(GTriangle _triangle, GRay _ray, bool _rayDirectionCheck, out float _distance)
        {
            if (!RayIntersection.TriangleCalculate(_ray.origin, _triangle[0], _triangle[1], _triangle[2], _ray.direction,
                    out _distance, out float u, out float v))
                return false;
            return !_rayDirectionCheck || _distance > 0;
        }

        public static bool Intersect(GRay _ray, GSphere _sphere)
        {
            RayIntersection.SphereCalculate(_ray, _sphere, out float dotOffsetDirection, out float discriminant);
            return discriminant >= 0;
        }

        public static bool Intersect(GRay _ray, GEllipsoid _ellipsoid)
        {
            RayIntersection.EllipsoidCalculate(_ellipsoid, _ray, out var a, out var b, out var c, out var discriminant);
            return discriminant >= 0;
        }

        public static bool Intersect(GTriangle _triangle, GRay _ray, bool _rayDirectionCheck, bool _triangleDirectionCheck,
            out float _distance)
        {
            if (!RayIntersection.TriangleCalculate(_triangle[0], _triangle[1], _triangle[2], _ray.origin, _ray.direction,
                    out _distance, out float u, out float v))
                return false;
            bool intersect = true;
            intersect &= !_rayDirectionCheck || _distance > 0;
            intersect &= !_triangleDirectionCheck || math.dot(_triangle.normal, _ray.direction) < 0;
            return intersect;
        }

        public static bool Intersect(GRay _ray, GBox _box)
        {
            RayIntersection.AABBCalculate(_ray, _box, out var tmin, out var tmax);
            return tmin.maxElement() <= tmax.minElement();
        }

        public static bool Intersect(GRay _ray, GPlane _plane, out float3 _hitPoint)
        {
            float distance = Distance(_ray, _plane);
            _hitPoint = _ray.GetPoint(distance);
            return distance != 0;
        }

        #region AABB

        public static bool Intersect(GPlane _plane, GBox _box)
        {
            var c = _box.center;
            var e = math.abs(_box.extent);

            var n = _plane.normal;
            float d = _plane.distance;
            float r = math.dot(e, math.abs(n));
            float s = math.dot(n, c) - d;
            return s <= r;
        }

        public static bool Intersect(GBox _src, GBox _dst)
        {
            return (_src.min.x <= _dst.max.x && _src.max.x >= _dst.min.x) &&
                   (_src.min.y <= _dst.max.y && _src.max.y >= _dst.min.y) &&
                   (_src.min.z <= _dst.max.z && _src.max.z >= _dst.min.z);
        }

        public static bool Intersect(GFrustumPlanes _frustumPlanes, GBox _bounding)
        {
            for (int i = 0; i < _frustumPlanes.Length; i++)
            {
                if (!Intersect(_frustumPlanes[i], _bounding))
                    return false;
            }

            return true;
        }

        public static bool Intersect(GFrustumPlanes _frustumPlanes, GBox _bounding,
            GFrustumPoints _frustumPoints)
        {
            if (!Intersect(_frustumPlanes, _bounding))
                return false;

            if (!Intersect(_frustumPoints.bounding,
                    _bounding)) // if (!FrustumPointsAABBIntersection(_frustumPoints,_bounding)) //More expensive
                return false;

            return true;
        }

        public static bool Intersect(GFrustumPoints _frustumPoints, GBox _box)
        {
            bool outside;
            outside = true;
            for (int i = 0; i < 8; i++) outside &= _frustumPoints[i].x > _box.max.x;
            if (outside) return false;
            outside = true;
            for (int i = 0; i < 8; i++) outside &= _frustumPoints[i].x < _box.min.x;
            if (outside) return false;
            outside = true;
            for (int i = 0; i < 8; i++) outside &= _frustumPoints[i].y > _box.max.y;
            if (outside) return false;
            outside = true;
            for (int i = 0; i < 8; i++) outside &= _frustumPoints[i].y < _box.min.y;
            if (outside) return false;
            outside = true;
            for (int i = 0; i < 8; i++) outside &= _frustumPoints[i].z > _box.max.z;
            if (outside) return false;
            outside = true;
            for (int i = 0; i < 8; i++) outside &= _frustumPoints[i].z < _box.min.z;
            if (outside) return false;
            return true;
        }

        #endregion
    }
}