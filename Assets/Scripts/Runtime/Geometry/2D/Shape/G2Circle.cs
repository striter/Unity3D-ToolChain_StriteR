using System;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{

    
    [Serializable]
    public struct G2Circle:IShape2D
    {
        public float2 center;
        public float radius;

        public G2Circle(float2 _center,float _radius)
        {
            center = _center;
            radius = _radius;
        }
        
        public static G2Circle Minmax(float2 _a, float2 _b) => new G2Circle((_a + _b) / 2,math.length(_b-_a)/2);

        public static G2Circle TriangleCircumscribed(G2Triangle _triangle) => TriangleCircumscribed(_triangle.V0,_triangle.V1,_triangle.V2);
        public static G2Circle TriangleCircumscribed(float2 _a, float2 _b, float2 _c)
        {
            var ox = (math.min(math.min(_a.x, _b.x), _c.x) + math.max(math.max(_a.x, _b.x), _c.x)) / 2;
            var oy = (math.min(math.min(_a.y, _b.y), _c.y) + math.max(math.max(_a.y, _b.y), _c.y)) / 2;
            var ax = _a.x - ox; var ay = _a.y - oy;
            var bx = _b.x - ox; var by = _b.y - oy;
            var cx = _c.x - ox; var cy = _c.y - oy;
            var d = (ax * (by - cy) + bx * (cy - ay) + cx * (ay - by)) * 2;
            if (d == 0)
                return kZero;
            var x = ((ax*ax + ay*ay) * (by - cy) + (bx*bx + by*by) * (cy - ay) + (cx*cx + cy*cy) * (ay - by)) / d;
            var y = ((ax*ax + ay*ay) * (cx - bx) + (bx*bx + by*by) * (ax - cx) + (cx*cx + cy*cy) * (bx - ax)) / d;
            float2 p = new float2(ox + x, oy + y);
            var sqR = Mathf.Max(math.distancesq(p, _a), math.distancesq(p, _b), math.distancesq(p, _c));
            return new G2Circle(p, math.sqrt(sqR));
        }


        public const int kMaxBoundsCount = 3;
        public static G2Circle Create(IList<float2> _positions)
        {
            switch (_positions.Count)
            {
                case 0: return kZero;
                case 1: return new G2Circle(_positions[0], 0f);
                case 2: return Minmax(_positions[0],_positions[1]);
                case 3: return TriangleCircumscribed(_positions[0], _positions[1], _positions[2]);
                default: throw new InvalidEnumArgumentException();
            }
        }
        
        public bool Contains(float2 _p, float _bias = float.Epsilon) =>math.lengthsq(_p - center) < radius * radius + _bias;
        public static readonly G2Circle kZero = new G2Circle(float2.zero, 0f);
        public static readonly G2Circle kOne = new G2Circle(float2.zero, 1f);
        public float2 GetSupportPoint(float2 _direction) => center + _direction * radius;
        public float2 Center => center;
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