using System.Collections.Generic;
using System.Linq.Extensions;
using Unity.Mathematics;

namespace Runtime.Geometry
{
    public static class PCA //Principle Component Analysis
    {
        public static void Evaluate(IEnumerable<float3> _points, out float3 _centre, out float3 _right, out float3 _up,
            out float3 _forward)
        {
            _right = kfloat3.right;
            _up = kfloat3.up;
            _forward = kfloat3.forward;

            var m = _points.Average();
            var a00 = 0f;
            var a11 = 0f;
            var a22 = 0f;
            var a01mirror = 0f;
            var a02mirror = 0f;
            var a12mirror = 0f;
            int count = 0;
            foreach (var p in _points)
            {
                count++;
                a00 += umath.pow2(p.x - m.x);
                a11 += umath.pow2(p.y - m.y);
                a22 += umath.pow2(p.z - m.z);

                a01mirror += (p.x - m.x) * (p.y - m.y);
                a02mirror += (p.x - m.x) * (p.z - m.z);
                a12mirror += (p.y - m.y) * (p.z - m.z);
            }

            var matrix = new float3x3(a00, a01mirror, a02mirror,
                a01mirror, a11, a12mirror,
                a02mirror, a12mirror, a22) / count;
            _centre = m;
            matrix.GetEigenVectors(out _right, out _up, out _forward);

        }
    }
}