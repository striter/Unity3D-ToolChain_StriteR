using System;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    [Serializable]
    public struct G2Ray : ISDF2
    {
        public float2 origin;
        public float2 direction;

        public G2Ray(float2 _position, float2 _direction)
        {
            origin = _position;
            direction = _direction;
        }

        public float2 GetPoint(float _distance) => origin + direction * _distance;
        
        public static G2Ray StartEnd(float2 _start, float2 _end) => new G2Ray(_start, math.normalize(_end - _start));
        public float2 Origin => Origin;
        public float SDF(float2 _position)
        {
            var lineDirection = direction;
            var pointToStart = _position - origin;
            return math.length(umath.cross(lineDirection, pointToStart));
        }

        public bool SideSign(float2 _point) => math.dot( umath.Rotate2DCW90(direction),(_point - origin)) > 0;
        public void DrawGizmos() => Gizmos.DrawRay(origin.to3xz(),direction.to3xz());
    }
}