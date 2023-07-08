using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Geometry.Curves
{
    public interface ICurve<T>
    {
        T Evaluate(float _value);
    }

    public interface ICurveTangents<T> : ICurve<T>
    {
        T EvaluateTangent(float _value);
    }

    public static class UCurve
    {
        public static T[] Output<T>(this ICurve<T> _curve,int _amount)
        {
            T[] outputs = new T[_amount+1];
            var amount = (float)_amount;
            for (int i = 0; i <= _amount; i++)
                outputs[i] = _curve.Evaluate(i/amount);
            return outputs;
        }
        
        public static void DrawGizmos(this ICurve<float3> _curve,int _amount = 64)
        {
            var outputs = _curve.Output(_amount);
            Gizmos.color = Color.white;
            UGizmos.DrawLines(outputs, p => p);
        }

        public static void DrawGizmos_Tangents(this ICurveTangents<float3> _curve,int _amount = 64,float _sphereSize = 0.1f)
        {
            Gizmos.color = KColor.kOrange.SetA(.1f);
            for (int i = 0; i < _amount + 1; i++)
            {
                var value = i / (float) _amount;
                var point = _curve.Evaluate(value);
                Gizmos.DrawLine(point,point + _curve.Evaluate(value)*.1f);
            }
        }
    }
}