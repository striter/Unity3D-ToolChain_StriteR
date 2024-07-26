using Unity.Mathematics;

namespace Runtime.Geometry.Extension
{
    //https://iquilezles.org/articles/intersectors/
    public interface IRayAreaIntersection : IArea
    {
        public bool RayIntersection(G2Ray _ray,out float2 distance);
    }

    public interface IRayIntersection
    {
        public bool RayIntersection(GRay _ray,out float distance);
    }
    
    public interface IRayVolumeIntersection
    {
        public bool RayIntersection(GRay _ray,out float2 distances);
    }

    public static class IRayIntersection_Extention
    {
        public static float IntersectDistance(this GRay _ray, IRayIntersection _intersection) => _intersection.RayIntersection(_ray,out var distance) ? distance : -1;
        public static float IntersectDistance(this  IRayIntersection _intersection,GRay _ray) => IntersectDistance(_ray,_intersection);
        public static bool Intersect(this IRayIntersection _intersection, GRay _ray) => Intersect(_intersection, _ray);
        public static bool Intersect(this GRay _ray, IRayIntersection _intersection) => _intersection.RayIntersection(_ray,out var distance);
        public static bool Intersect(this GRay _ray, IRayIntersection _intersection,out float distance) => _intersection.RayIntersection(_ray,out distance);

        public static bool IntersectPoint(this GRay _ray, IRayIntersection _intersection, out float3 hitPoint)
        {
            hitPoint = float3.zero;
            if (!_intersection.RayIntersection(_ray, out var distance)) return false;
            hitPoint = _ray.GetPoint(distance);
            return true;
        }
        
        public static bool Intersect(this GLine _line, IRayIntersection _intersection) => _intersection.RayIntersection(_line.ToRay(),out var distance) && distance <= _line.length;
        public static bool Intersect(this IRayIntersection _intersection, GLine _line) => Intersect( _line,_intersection);
    }
    
    public static class IVolumeDistance_Extention
    {
        public static float2 Distance(this GRay _ray,IRayVolumeIntersection _distance) => _distance.RayIntersection(_ray,out var distances) ? distances : new float2(-1,-1);
        public static bool Intersect(this IRayVolumeIntersection _distance, GRay _ray) => Intersect(_ray, _distance);
        public static bool Intersect(this GRay _ray, IRayVolumeIntersection _distance) => _distance.RayIntersection(_ray,out var distances);
    }

    public static class IAreaDistance_Extension
    {
        public static bool Intersect(this IRayAreaIntersection intersection, G2Ray _ray) => Intersect(_ray, intersection);
        public static float2 Distance(this G2Ray _ray,IRayAreaIntersection _intersection) => _intersection.RayIntersection(_ray,out var distance) ? distance : new float2(-1,-1);
        public static bool Intersect(this G2Ray _ray, IRayAreaIntersection _intersection) => _intersection.RayIntersection(_ray,out var distance);
    }

}