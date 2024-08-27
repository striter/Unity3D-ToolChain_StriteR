using System.Collections.Generic;
using System.ComponentModel;
using Unity.Mathematics;

namespace Runtime.Geometry
{
    public partial struct GSphere : IRound<float3>
    {
        public int kMaxBoundsCount => 4;
        public float Radius => radius;
        IRound<float3> IRound<float3>.Create(IList<float3> _positions) => Create(_positions);
        public static GSphere Create(IList<float3> _positions) => _positions.Count switch {
            0 => kZero,
            1 => new GSphere(_positions[0], 0f),
            2 => Minmax(_positions[0], _positions[1]),
            3 => Triangle(_positions[0], _positions[1], _positions[2]),
            4 => Tetrahedron(_positions[0], _positions[1], _positions[2], _positions[3]),
            _ => throw new InvalidEnumArgumentException()
        };
        
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
            var sqLength1 = math.lengthsq(r1);
            var sqLength2 = math.lengthsq(r2);
            var sqLength3 = math.lengthsq(r3);
            var determinant =r1.x * (r2.y * r3.z - r3.y * r2.z)  - r2.x * (r1.y * r3.z - r3.y * r1.z)  + r3.x * (r1.y * r2.z - r2.y * r1.z);
            if (determinant == 0)
                return Triangle(_a,_b,_c);
            
            var f = .5f / (determinant + float.Epsilon);
            
            var offset =  f *new float3(
                (r2.y * r3.z - r3.y * r2.z) * sqLength1 - (r1.y * r3.z - r3.y * r1.z) * sqLength2 +  (r1.y * r2.z - r2.y * r1.z) * sqLength3,
                -(r2.x * r3.z - r3.x * r2.z) * sqLength1 + (r1.x * r3.z - r3.x * r1.z) * sqLength2 - (r1.x * r2.z - r2.x * r1.z) * sqLength3,
                (r2.x * r3.y - r3.x * r2.y) * sqLength1 - (r1.x * r3.y - r3.x * r1.y) * sqLength2 + (r1.x * r2.y - r2.x * r1.y) * sqLength3);

            return new GSphere(_a + offset, math.length(offset));
        }

    }
}