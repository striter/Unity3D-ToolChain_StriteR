using System.Collections.Generic;
using Unity.Mathematics;

namespace Runtime.Geometry.Extension
{
    
    public interface IRayArea2Intersection 
    {
        public bool RayIntersection(G2Ray _ray,out float2 distance);
    }

    public static class IArea2Intersection_Extension
    {
        public static bool Intersect(this IRayArea2Intersection intersection, G2Ray _ray) => Intersect(_ray, intersection);
        public static float2 Distance(this G2Ray _ray,IRayArea2Intersection _intersection) => _intersection.RayIntersection(_ray,out var distance) ? distance : new float2(-1,-1);
        public static bool Intersect(this G2Ray _ray, IRayArea2Intersection _intersection) => _intersection.RayIntersection(_ray,out var distance);
    }
}