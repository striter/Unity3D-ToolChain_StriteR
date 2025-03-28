﻿using Unity.Mathematics;

namespace Runtime.Geometry.Extension
{
    public static partial class UGeometry
    {
        public static float Projection(this G2Ray _ray,float2 _point) => math.dot(_point - _ray.origin, _ray.direction);
        
        public static float2 Projection(this G2Box _box, float2 _direction)
        {
            var ray = new G2Ray(_box.center, _direction.normalize());
            return ray.GetPoint(ray.Distance(_box).sum());
        }
    }
}