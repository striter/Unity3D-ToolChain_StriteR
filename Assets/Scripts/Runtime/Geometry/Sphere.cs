using System;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Mathematics;

namespace Geometry
{
    [Serializable]
    public struct GSphere : IShape
    {
        public float3 center;
        [Clamp(0)] public float radius;
        public GSphere(float3 _center,float _radius) { center = _center;radius = _radius; }
        public static readonly GSphere kOne = new GSphere(float3.zero, .5f);
        public static readonly GSphere kZero = new GSphere(0,0);

        public const int kMaxBoundsCount = 4;
        public static GSphere Minmax(float3 _a, float3 _b) => new GSphere((_a + _b) / 2,math.length(_b-_a)/2);

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
            float f = .5f / determinant ;
            
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
        
        public bool Contains(float3 _p, float _bias = float.Epsilon) =>math.lengthsq(_p - center) < radius * radius + _bias;
        public float3 GetSupportPoint(float3 _direction) => center + _direction * radius;
        public float3 Center => center;
    }

    [Serializable]
    public struct GEllipsoid
    {
        public float3 center;
        public float3 radius;
        public GEllipsoid(float3 _center,float3 _radius) {  center = _center; radius = _radius;}
        public static readonly GEllipsoid kDefault = new GEllipsoid(float3.zero, new float3(.5f,1f,0.5f));
    }

}