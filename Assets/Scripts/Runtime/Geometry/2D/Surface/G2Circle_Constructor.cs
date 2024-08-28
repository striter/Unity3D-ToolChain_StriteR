using System.Collections.Generic;
using System.ComponentModel;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    public partial struct G2Circle : IRound<float2>
    {
        public float Radius => radius;
        public int kMaxBoundsCount => 3;
        IRound<float2> IRound<float2>.Create(IList<float2> _positions)
        {
            return Create(_positions);
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
            var p = new float2(ox + x, oy + y);
            var sqR = Mathf.Max(math.distancesq(p, _a), math.distancesq(p, _b), math.distancesq(p, _c));
            return new G2Circle(p, math.sqrt(sqR));
        }

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
    }
}