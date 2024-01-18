using Unity.Mathematics;

namespace Runtime.Geometry.Validation
{
    using static math;

    public static partial class UGeometry
    {
        public static bool Intersect(this GRay _ray, GTriangle _triangle) =>   RayIntersection.TriangleCalculate(_ray, _triangle, out var u, out var v, out var t) && !(u < 0.0 || v < 0.0 || u + v > 1.0);

        public static bool Intersect(this GRay _ray, GTriangle _triangle, out float _distance,bool _rayDirectionCheck = false,bool _triangleDirectionCheck = false)
        {
            if (!RayIntersection.TriangleCalculate(_ray, _triangle, out var u, out var v, out _distance))
                return false;

            var intersect = !(u < 0.0 || v < 0.0 || u + v > 1.0);
            intersect = intersect && (!_rayDirectionCheck || _distance > 0);
            intersect = intersect && (!_triangleDirectionCheck || dot(_triangle.normal, _ray.direction) < 0);
            return intersect;
        }
        
        public static bool Intersect(this GLine _line,GTriangle _triangle, out float _distance,bool _rayDirectionCheck = false,bool _triangleDirectionCheck = false) =>Intersect(_line.ToRay(), _triangle, out _distance,_rayDirectionCheck,_triangleDirectionCheck) && _distance >= 0 && _distance <= _line.length;

        public static bool Intersect(this GRay _ray, GSphere _sphere)
        {
            RayIntersection.SphereCalculate(_ray, _sphere, out var dotOffsetDirection, out var discriminant);
            return discriminant >= 0;
        }

        public static bool Intersect(this GRay _ray, GEllipsoid _ellipsoid)
        {
            RayIntersection.EllipsoidCalculate(_ellipsoid, _ray, out var a, out var b, out var c, out var discriminant);
            return discriminant >= 0;
        }

        public static bool Intersect(this GRay _ray, GBox _box)
        {
            RayIntersection.AABBCalculate(_ray, _box, out var tmin, out var tmax);
            return tmin.maxElement() <= tmax.minElement();
        }

        public static bool Intersect(this GRay _ray, GPlane _plane, out float3 _hitPoint)
        {
            var distance = Distance(_ray, _plane);
            _hitPoint = _ray.GetPoint(distance);
            return distance != 0;
        }

        #region AABB

        public static bool Intersect(GPlane _plane, GBox _box)
        {
            var c = _box.center;
            var e = abs(_box.extent);

            var n = _plane.normal;
            var d = _plane.distance;
            var r = dot(e, abs(n));
            var s = dot(n, c) - d;
            return s <= r;
        }

        public static bool Intersect(GBox _src, GBox _dst)
        {
            return _src.min.x <= _dst.max.x && _src.max.x >= _dst.min.x &&
                   _src.min.y <= _dst.max.y && _src.max.y >= _dst.min.y &&
                   _src.min.z <= _dst.max.z && _src.max.z >= _dst.min.z;
        }

        public static bool Intersect(this GLine _line,GQuad _quad,out float _distance,bool _directed = false)
        {
            _quad.GetTriangles(out var _triangle1,out var _triangle2);
            var ray = _line.ToRay();
            if (Intersect(ray, _triangle1,out _distance,_directed))
                return true;

            return Intersect(ray, _triangle2,out _distance,_directed);
        }
        
        public static bool Intersect(GFrustumPlanes _frustumPlanes, GBox _bounding)
        {
            for (var i = 0; i < _frustumPlanes.Length; i++)
                if (!Intersect(_frustumPlanes[i], _bounding))
                    return false;

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
            var outside = true;
            for (var i = 0; i < 8; i++) outside &= _frustumPoints[i].x > _box.max.x;
            if (outside) return false;
            outside = true;
            for (var i = 0; i < 8; i++) outside &= _frustumPoints[i].x < _box.min.x;
            if (outside) return false;
            outside = true;
            for (var i = 0; i < 8; i++) outside &= _frustumPoints[i].y > _box.max.y;
            if (outside) return false;
            outside = true;
            for (var i = 0; i < 8; i++) outside &= _frustumPoints[i].y < _box.min.y;
            if (outside) return false;
            outside = true;
            for (var i = 0; i < 8; i++) outside &= _frustumPoints[i].z > _box.max.z;
            if (outside) return false;
            outside = true;
            for (var i = 0; i < 8; i++) outside &= _frustumPoints[i].z < _box.min.z;
            if (outside) return false;
            return true;
        }

        #endregion
    }
}