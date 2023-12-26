using System;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Mathematics;
using UnityEngine;

namespace Geometry
{
    [Serializable]
    public struct GCircle : IShape3D
    {
        public float3 center;
        public float radius;

        public GCircle(float3 _center,float _radius)
        {
            center = _center;
            radius = _radius;
        }

        public float3 GetSupportPoint(float3 _direction) => center + _direction.normalize() * radius;
        public float3 Center => center;
    }
    
    public static class UCircle
    {
        public static G2Triangle GetCircumscribedTriangle(this G2Circle _circle)
        {
            var r = _circle.radius;
            return new G2Triangle(
                _circle.center + new float2(0f,r/kmath.kSin30d) ,
                _circle.center + new float2(r * kmath.kTan60d,-r),
                _circle.center + new float2(-r * kmath.kTan60d,-r));
        }
    }
}