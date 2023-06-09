using System;
using Unity.Mathematics;
using UnityEngine;

namespace Geometry.Curves
{
    public interface ICurve<T>
    {
        T[] Coordinates { get; }
        T Evaluate(float _value);
    }

    public static class UCurve
    {
        public static T[] Output<T>(this ICurve<T> _spline,int _amount)
        {
            T[] outputs = new T[_amount+1];
            var amount = (float)_amount;
            for (int i = 0; i <= _amount; i++)
                outputs[i] = _spline.Evaluate(i/amount);
            return outputs;
        }
        
        public static void DrawGizmos(this ICurve<float3> _curve,bool _original = true,float _sphereSize = 0.1f,int _amount = 64)
        {
            Gizmos.color = Color.white.SetAlpha(.5f);
            if (_original)
            {
                foreach (var coord in _curve.Coordinates)
                    Gizmos.DrawSphere(coord,_sphereSize);
                UGizmos.DrawLines(_curve.Coordinates,p=>p);
            }
            var outputs = _curve.Output(_amount);
            Gizmos.color = Color.green;
            UGizmos.DrawLines(outputs, p => p);
        }
        
    }
}