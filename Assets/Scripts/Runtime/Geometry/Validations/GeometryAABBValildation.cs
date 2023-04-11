using Unity.Mathematics;

namespace Geometry.Validation
{
    public static partial class UGeometryValidation
    {
        public static class AABB
        {
            public static bool PlaneIntersection(GPlane _plane, GBox _box)
            {
                var c = _box.center;
                var e = math.abs(_box.extent);

                var n = _plane.normal;
                float d = _plane.distance;
                float r = math.dot(e,math.abs(n));
                float s = math.dot(n, c) - d;
                return s <= r;
            }
            
            public static bool AABBIntersection(GBox _src, GBox _dst)
            {
                return (_src.min.x <= _dst.max.x && _src.max.x >= _dst.min.x) &&
                       (_src.min.y <= _dst.max.y && _src.max.y >= _dst.min.y) &&
                       (_src.min.z <= _dst.max.z && _src.max.z >= _dst.min.z);
            }

            public static bool FrustumPlaneIntersection(GFrustumPlanes _frustumPlanes,GBox _bounding)
            {
                for (int i = 0; i < _frustumPlanes.Length; i++)
                {
                    if (!PlaneIntersection(_frustumPlanes[i],_bounding)) 
                        return false;
                }
                return true;
            }
        
            public static bool FrustumPlaneIntersection(GFrustumPlanes _frustumPlanes, GBox _bounding, GFrustumPoints _frustumPoints)
            {
                if (!FrustumPlaneIntersection(_frustumPlanes, _bounding))
                    return false;
            
                if (!AABBIntersection(_frustumPoints.bounding,_bounding))       // if (!FrustumPointsAABBIntersection(_frustumPoints,_bounding)) //More expensive
                    return false;
            
                return true;
            }
            
            public static bool FrustumPointsAABBIntersection(GFrustumPoints _frustumPoints, GBox _box)
            {
                bool outside;
                outside = true; for (int i = 0; i < 8; i++) outside &= _frustumPoints[i].x > _box.max.x; if(outside) return false;
                outside = true; for (int i = 0; i < 8; i++) outside &= _frustumPoints[i].x < _box.min.x; if(outside) return false;
                outside = true; for (int i = 0; i < 8; i++) outside &= _frustumPoints[i].y > _box.max.y; if(outside) return false;
                outside = true; for (int i = 0; i < 8; i++) outside &= _frustumPoints[i].y < _box.min.y; if(outside) return false;
                outside = true; for (int i = 0; i < 8; i++) outside &= _frustumPoints[i].z > _box.max.z; if(outside) return false;
                outside = true; for (int i = 0; i < 8; i++) outside &= _frustumPoints[i].z < _box.min.z; if(outside) return false;
                return true;
            }
        }
    }
}