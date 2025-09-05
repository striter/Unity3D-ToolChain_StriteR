using Runtime.Geometry.Extension;

namespace Runtime.Geometry
{
    public static class UFrustum
    {
        public static bool Intersect(this GFrustum _frustum, GBox _bounding)//, GFrustumPoints _frustumPoints)
        {
            var planes = _frustum.planes;
            for (var i = 0; i < planes.Length; i++)
                // if (!Intersect(_frustumPoints,_bounding)) //More expensive
                if (!UGeometry.Intersect(planes[i], _bounding))
                    return false;

            return true;
        }

        public static bool Intersect(this GFrustum _frustum, GSphere _bounding)
        {
            var planes = _frustum.planes;
            for (var i = 0; i < planes.Length; i++)
            {
                if (!UGeometry.Intersect(planes[i], _bounding))
                    return false;
            }

            return true;
        }
        

        public static bool Intersect(this GFrustumPoints _frustumPoints, GBox _box)
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
    }
}