using Runtime.Geometry.Validation;

namespace Runtime.Geometry
{
    public static class UFrustum
    {
        public static bool AABBIntersection(this GFrustumPlanes _frustumPlanes,GBox _bounding) => UGeometry.Intersect(_frustumPlanes,_bounding);
        public static bool AABBIntersection(this GFrustumPlanes _frustumPlanes, GBox _bounding, GFrustumPoints _frustumPoints)=> UGeometry.Intersect(_frustumPlanes,_bounding,_frustumPoints);
    }
}