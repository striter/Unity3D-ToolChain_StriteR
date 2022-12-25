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
                var e = math.abs(_box.extend);

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
            
                if (!AABBIntersection(_frustumPoints.bounding,_bounding))
                    return false;
            
                return true;
            }
        }
    }
}