using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Geometry.PointSet
{
    
    
    public static class UPrincipleComponentAnalysis
    {
        public static void Evaluate(IEnumerable<float3> _points,out float3 _centre, out float3 _right, out float3 _up, out float3 _forward)
        {
            _right = kfloat3.right;
            _up = kfloat3.up;
            _forward = kfloat3.forward;

            var m = _points.Average();
            var a00 = 0f;
            var a11 = 0f;
            var a22 =0f;
            var a01mirror = 0f;
            var a02mirror = 0f;
            var a12mirror = 0f;
            int count = 0;
            foreach (var p in _points)
            {
                count++;
                a00+=umath.pow2(p.x - m.x);
                a11+=umath.pow2(p.y - m.y);
                a22+=umath.pow2(p.z - m.z);

                a01mirror += (p.x - m.x)*(p.y-m.y);
                a02mirror += (p.x - m.x)*(p.z-m.z);
                a12mirror += (p.y - m.y)*(p.z-m.z);
            }
            var matrix =  new float3x3(a00,a01mirror,a02mirror,a01mirror,a11,a12mirror,a02mirror,a12mirror,a22)/count;
            _centre = m;
            matrix.GetEigenVectors(out _right,out _up,out _forward);
        }

        public static void Evaluate(IList<float2> _points,out float2 _centre, out float2 _R, out float2 _S)
        {
            _R = kfloat2.up;
            _S = kfloat2.right;

            _centre = _points.Average();
            var m = _centre;
            var a00 = _points.Average(p => umath.pow2(p.x - m.x));
            var a11 = _points.Average(p => umath.pow2(p.y - m.y));
            var a01mirror = _points.Average(p => (p.x - m.x)*(p.y-m.y));
            new float2x2(a00,a01mirror,a01mirror,a11).GetEigenVectors(out _R,out _S);
        }
    }
    
    
}
