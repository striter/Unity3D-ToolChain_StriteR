using System.Collections.Generic;
using System.ComponentModel;
using Geometry.PointSet;
using Unity.Mathematics;
using UnityEngine;

namespace Geometry
{
    public partial struct GSphere
    {
        public const int kMaxBoundsCount = 4;
        public static GSphere Minmax(float3 _a, float3 _b) => new GSphere((_a + _b) / 2,math.length(_b-_a)/2);

        public static GSphere Minmax(GSphere _a, GSphere _b)
        {
            if (_a.Contains(_b))
                return _a;
            if (_b.Contains(_a))
                return _b;
            
            var delta = _b.center - _a.center;
            
            if (delta.sqrmagnitude()<0.005f)
                return _a;
            
            var direction = math.normalize(delta);
            var min = _a.center - direction * _a.radius;
            var max = _b.center + direction * _b.radius;
            return Minmax(min, max);
        }

        public static GSphere Triangle(float3 _a, float3 _b, float3 _c)
        {
            var a = _a - _c;
            var b = _b - _c;
            var aCrossb = math.cross(a, b);
            var numerator = math.cross(b * math.lengthsq(a) - a * math.lengthsq(b), aCrossb);
            var denominator = 2f * math.lengthsq(aCrossb);
            var offset = numerator / denominator;
            return new GSphere(_c + offset , math.length(offset));
        }

        public static GSphere Tetrahedron(float3 _a, float3 _b, float3 _c, float3 _d)
        {      
            var r1 = _b - _a;
            var r2 = _c - _a;
            var r3 = _d - _a;
            float sqLength1 = math.lengthsq(r1);
            float sqLength2 = math.lengthsq(r2);
            float sqLength3 = math.lengthsq(r3);
            var determinant =r1.x * (r2.y * r3.z - r3.y * r2.z)  - r2.x * (r1.y * r3.z - r3.y * r1.z)  + r3.x * (r1.y * r2.z - r2.y * r1.z);
            if (determinant == 0)
            {
                Debug.LogError("Tetrahedron points are coplanar");
                return UBounds.MinimumEnclosingSphere(_a,_b,_c,_d);
            }
            
            float f = .5f / (determinant + float.Epsilon);
            
            float3 offset =  f *new float3(
                (r2.y * r3.z - r3.y * r2.z) * sqLength1 - (r1.y * r3.z - r3.y * r1.z) * sqLength2 +  (r1.y * r2.z - r2.y * r1.z) * sqLength3,
                -(r2.x * r3.z - r3.x * r2.z) * sqLength1 + (r1.x * r3.z - r3.x * r1.z) * sqLength2 - (r1.x * r2.z - r2.x * r1.z) * sqLength3,
                (r2.x * r3.y - r3.x * r2.y) * sqLength1 - (r1.x * r3.y - r3.x * r1.y) * sqLength2 + (r1.x * r2.y - r2.x * r1.y) * sqLength3);

            return new GSphere(_a + offset, math.length(offset));
        }

        public static GSphere Create(IList<float3> _positions)
        {
            switch (_positions.Count)
            {
                case 0: return kZero;
                case 1: return new GSphere(_positions[0], 0f);
                case 2: return Minmax(_positions[0],_positions[1]);
                case 3: return Triangle(_positions[0], _positions[1], _positions[2]);
                case 4: return Tetrahedron(_positions[0], _positions[1], _positions[2],_positions[3]);
                default: throw new InvalidEnumArgumentException();
            }
        }
    }
}