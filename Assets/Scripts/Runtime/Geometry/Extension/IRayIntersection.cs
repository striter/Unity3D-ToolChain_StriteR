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
        public static float Intersection(this GRay _ray, IRayIntersection _intersection) => _intersection.RayIntersection(_ray,out var distance) ? distance : -1;
        public static bool Intersect(this IRayIntersection _intersection, GRay _ray) => Intersect(_intersection, _ray);
        public static bool Intersect(this GRay _ray, IRayIntersection _intersection) => _intersection.RayIntersection(_ray,out var distance);
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