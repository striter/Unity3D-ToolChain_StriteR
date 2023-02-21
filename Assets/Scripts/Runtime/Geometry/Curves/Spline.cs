using System;
using Unity.Mathematics;
using UnityEngine;

namespace Geometry
{
    [Serializable]
    public struct GBezierSpline
    {
        public float3[] coordinates;
        [Clamp(1,12)]public int k;
        
        public static readonly GBezierSpline kDefault = new GBezierSpline()
        {
            coordinates = new float3[]{new float3(-1,0,-1),new float3(0,0,1),new float3(1,0,-1)},
        };

        public float3 Evaluate(float _value)
        {
            var n = coordinates.Length -1;
            var t = math.lerp(k ,n + 2  ,_value);
            float3 result = 0;
            for (int i = 1; i <= n + 1; i++)
            {
                var nik = Basis(i ,k,t);
                result += nik * coordinates[i - 1];
            }
            return result;
        }

        static float Basis(int _i,int _k,float _t)
        {
            if (_k == 1)
                return (_i <= _t && _t < _i + 1)?1:0;

            var nextK = _k - 1;
            
            float coefficient1 = (_t - _i) / nextK;
            float coefficient2 = (_i +_k - _t) / nextK;
            
            return coefficient1 * Basis(_i,nextK, _t) + coefficient2 * Basis(_i + 1,nextK, _t) ;
        }
    }
    
    public static class USpline
    {
        public static float3[] Output(this GBezierSpline _spline,int _amount)
        {
            float3[] outputs = new float3[_amount+1];
            var amount = (float)_amount;
            for (int i = 0; i <= _amount; i++)
                outputs[i] = _spline.Evaluate(i/amount);
            
            return outputs;
        }
        
        public static void DrawGizmos(this GBezierSpline _curve,int _amount = 64)
        {
            var outputs = _curve.Output(_amount);
            Gizmos.color = Color.white;
            Gizmos_Extend.DrawLines(_curve.coordinates,p=>p);
            Gizmos.color = Color.green;
            Gizmos_Extend.DrawLines(outputs, p => p);
        }
    }

}