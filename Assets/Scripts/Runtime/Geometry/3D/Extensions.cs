using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    public static class UVolume_Extension
    {
        #if UNITY_EDITOR
        public static void DrawHandles(this IVolume _volume)
        {
            switch (_volume)
            {
                default: Debug.LogWarning($"Unknown Handles For Volume {_volume.GetType()}"); break;
                case GBox box: UnityEditor.Handles.DrawWireCube(box.center,box.size); break;
                case GSphere sphere: UnityEditor.UHandles.DrawWireSphere(sphere.center,sphere.radius); break;
            }
        }
        #endif
    }
    
    public static class UCurve
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