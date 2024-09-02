using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    public interface ICurve : IGeometry {
        float3 Evaluate(float _value);
    }
    
    public interface ICurveTangents : ICurve
    {
        float3 EvaluateTangent(float _value);
    }

    public static class ICurve_Extension
    {
        public static float3[] Output(this ICurve _curve,int _amount)
        {
            var outputs = new float3[_amount+1];
            var amount = (float)_amount;
            for (int i = 0; i <= _amount; i++)
                outputs[i] = _curve.Evaluate(i/amount);
            return outputs;
        }
        
        public static void DrawGizmos(this ICurve _curve,int _amount = 64)
        {
            var outputs = _curve.Output(_amount);
            Gizmos.color = Color.white;
            UGizmos.DrawLines(outputs, p => p);
        }

        public static void DrawGizmos_Tangents(this ICurveTangents _curve,int _amount = 64)
        {
            Gizmos.color = KColor.kDarkOrange.SetA(.1f);
            for (int i = 0; i < _amount + 1; i++)
            {
                var value = i / (float) _amount;
                var point = _curve.Evaluate(value);
                Gizmos.DrawLine(point,point + _curve.EvaluateTangent(value)*.1f);
            }
        }
    }
}