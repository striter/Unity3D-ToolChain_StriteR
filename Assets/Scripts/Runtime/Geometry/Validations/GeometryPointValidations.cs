using Unity.Mathematics;
using UnityEngine;

namespace Geometry.Validation
{
    public static partial class UGeometryValidation
    {
        public static class Point
        {
            public static float PlaneDistance(float3 _point, GPlane _plane)
            {
                float nr = _point.x * _plane.normal.x + _point.y * _plane.normal.y + _point.z * _plane.normal.z +
                           _plane.distance;
                return nr / math.length(_plane.normal);
            }
            
            
            public static float PlaneProjection(float3 _point,GPlane _plane) => math.dot(_plane, _point.to4(1f));
        }
    }
}