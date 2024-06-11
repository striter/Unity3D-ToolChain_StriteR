using System;
using System.Collections.Generic;
using System.ComponentModel;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{

    [Serializable]
    public partial struct G2Circle: IArea , ISDF<float2> 
    {
        public float2 center;
        public float radius;

        public G2Circle(float2 _center,float _radius)
        {
            center = _center;
            radius = _radius;
        }
        
        public static readonly G2Circle kDefault = new G2Circle(float2.zero, .5f);
        public static readonly G2Circle kZero = new G2Circle(float2.zero, 0f);
        public static readonly G2Circle kOne = new G2Circle(float2.zero, 1f);
        public static G2Circle operator +(G2Circle _src, float2 _dst) => new G2Circle(_src.center+_dst,_src.radius);
        public static G2Circle operator -(G2Circle _src, float2 _dst) => new G2Circle(_src.center - _dst, _src.radius);
        public float2 GetSupportPoint(float2 _direction) => center + _direction * radius;
        public float SDF(float2 _position) => math.length(center-_position) - radius;
        public float2 Center => center;
        public override string ToString() => $"G2Circle({center},{radius})";
    }
    
    public static class G2Circle_Extension
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