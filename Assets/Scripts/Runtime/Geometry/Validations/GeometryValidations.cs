using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Geometry.Validation
{
    public static partial class UGeometryValidation
    {
        public static float3 StereographicProjection(float3 _srcPoint, float3 _origin, GPlane _projectionPlane)
        {
            var ray = new GRay(_origin, _srcPoint - _origin);
            return ray.GetPoint(Ray.Distance(ray, _projectionPlane));
        }

    }
}