using Unity.Mathematics;

namespace Runtime.Geometry.Extension
{
    using static math;

    public static partial class UGeometry
    {
        #region AABB

        public static bool Intersect(this GPlane _plane, GBox _box)
        {
            var c = _box.center;
            var e = abs(_box.extent);

            var n = _plane.normal;
            var d = _plane.distance;
            var r = dot(e, abs(n));
            var s = dot(n, c) - d;
            return s <= r;
        }

        public static bool Intersect(this GPlane _plane, GSphere _sphere)
        {
            return _plane.SDF(_sphere.center) <= _sphere.radius;
        }
        
        public static bool Intersect(this GBox _src, GBox _dst)
        {
            return _src.min.x <= _dst.max.x && _src.max.x >= _dst.min.x &&
                   _src.min.y <= _dst.max.y && _src.max.y >= _dst.min.y &&
                   _src.min.z <= _dst.max.z && _src.max.z >= _dst.min.z;
        }

        public static bool Intersect(this GSphere _sphere, GBox _box)
        {
            var sphereCenter = _sphere.center;
            var boxMin = _box.min;
            var boxMax = _box.max;

            var closestPoint = new float3(
                max(boxMin.x, min(sphereCenter.x, boxMax.x)),
                max(boxMin.y, min(sphereCenter.y, boxMax.y)),
                max(boxMin.z, min(sphereCenter.z, boxMax.z))
            );
            return Intersect(_sphere,closestPoint);
        }

        public static bool Intersect(this GSphere _sphere, float3 _position) =>
            (_position - _sphere.center).sqrmagnitude() <= _sphere.radius * _sphere.radius;
        public static bool Intersect(this GBox _box,GSphere _sphere) => Intersect(_sphere, _box);
        public static bool Intersect(this GLine _line,GQuad _quad,out float _distance,bool _directed = false)
        {
            _quad.GetTriangles(out var _triangle1,out var _triangle2);
            var ray = _line.ToRay();
            if (ray.Intersect(_triangle1,out _distance))
                return true;

            return ray.Intersect(_triangle2,out _distance);
        }


        #endregion
    }
}