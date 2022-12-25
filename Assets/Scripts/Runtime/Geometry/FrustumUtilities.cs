using Geometry.Validation;

namespace Geometry
{
    public static class UFrustum
    {
        public static bool AABBIntersection(this GFrustumPlanes _frustumPlanes,GBox _bounding) => UGeometryValidation.AABB.FrustumPlaneIntersection(_frustumPlanes,_bounding);
        public static bool AABBIntersection(this GFrustumPlanes _frustumPlanes, GBox _bounding, GFrustumPoints _frustumPoints)=> UGeometryValidation.AABB.FrustumPlaneIntersection(_frustumPlanes,_bounding,_frustumPoints);
    }
}