using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry.Curves
{
    public interface ICurveDimensions<T>
    {
        T Evaluate(float _value);
    }

    public interface ICurveTangents<T> : ICurveDimensions<T>
    {
        T EvaluateTangent(float _value);
    }

    public interface ISpline<T> : ICurveDimensions<T>
    {
        IEnumerable<T> Coordinates { get; }
    }
    
    public static class UCurve
    {
        public static T[] Output<T>(this ICurveDimensions<T> _curve,int _amount)
        {
            T[] outputs = new T[_amount+1];
            var amount = (float)_amount;
            for (int i = 0; i <= _amount; i++)
                outputs[i] = _curve.Evaluate(i/amount);
            return outputs;
        }
        
        public static void DrawGizmos(this ICurveDimensions<float3> _curve,int _amount = 64)
        {
            var outputs = _curve.Output(_amount);
            Gizmos.color = Color.white;
            UGizmos.DrawLines(outputs, p => p);
        }

        public static void DrawGizmos_Tangents(this ICurveTangents<float3> _curve,int _amount = 64,float _sphereSize = 0.1f)
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