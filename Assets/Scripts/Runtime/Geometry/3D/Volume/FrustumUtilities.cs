using Geometry.Validation;

namespace Geometry
{
    public static class UFrustum
    {
        public static bool AABBIntersection(this GFrustumPlanes _frustumPlanes,GBox _bounding) => Validation.UGeometry.Intersect(_frustumPlanes,_bounding);
        public static bool AABBIntersection(this GFrustumPlanes _frustumPlanes, GBox _bounding, GFrustumPoints _frustumPoints)=> Validation.UGeometry.Intersect(_frustumPlanes,_bounding,_frustumPoints);
        
    }
}