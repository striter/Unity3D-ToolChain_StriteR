using Unity.Mathematics;
using UnityEngine;

namespace Geometry.Validation
{
    using static math;
    public static partial class UGeometryValidation
    {
        public static class Point
        {
            public static float Distance(float3 _point, GPlane _plane)
            {
                return math.dot(_plane, _point.to4(1f));
                // float nr = _point.x * _plane.normal.x + _point.y * _plane.normal.y + _point.z * _plane.normal.z +
                //            _plane.distance;
                // return nr / math.length(_plane.normal);
            }
        }
    }
}