using System;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Geometry.Curves
{
    using static math;
    using static umath;
    [Serializable]
    public struct GBezierCurveQuadratic:ISerializationCallbackReceiver
    {
        public float3 source;
        public float3 control;
        public float3 destination;
        
        [HideInInspector] public float3 tangentSource;
        [HideInInspector] public float3 tangentDestination;
        
        public GBezierCurveQuadratic(float3 _src, float3 _dst, float3 _control)
        {
            source = _src;
            destination = _dst;
            control = _control;
            tangentSource = default;
            tangentDestination = default;
            Ctor();
        }

        void Ctor()
        {
            tangentSource = normalize(control - source);
            tangentDestination = normalize(destination - control);
        }

        public Vector3 Evaluate(float _value)
        {
            float value = _value;
            float oneMinusValue = 1 - value;
            return sqr(oneMinusValue) * source + 2 * (oneMinusValue) * value * control + sqr(value) * destination;
        }

        public float3 GetTangent(float _value) => normalize(lerp(tangentSource,tangentDestination,_value));

    #region Implements
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()=> Ctor();
    #endregion
    }

    [Serializable]
    public struct GBezierCurveCubic
    {
        public float3 source;
        public float3 controlSource;
        public float3 destination;
        public float3 controlDestination;

        public float3 Evaluate(float _value)
        {
            float value = _value;
            float oneMinusValue = 1 - value;
            return pow3(oneMinusValue) * source +  3 * sqr(oneMinusValue) * value * controlSource +  3 * oneMinusValue * sqr(value) * controlDestination + pow3(value) * destination;
        }
    }

    public static class UBezierCurve
    {
        public static GBox GetBoundingBox(this GBezierCurveQuadratic _curve)
        {
            var source = _curve.source;
            var destination = _curve.destination;
            var control = _curve.control;
            
            var min = math.min(source,destination);
            var max = math.max(source,destination);
            GBox box = GBox.Minmax(min, max);
            if (!box.Contains(control))
            {
                float3 t =  ((source - control)/(source - 2 * control + destination)).saturate();
                float3 s = 1f - t;
                float3 q = s*s*source + 2*s*t*control+ t*t*destination;
                box = GBox.Minmax(math.min(min,q),Vector3.Max(max,q));
            }
            return box;
        }
        
        public static GBox GetBoundingBox(this GBezierCurveCubic _curve)
        {
            var source = _curve.source;
            var destination = _curve.destination;
            var controlSource = _curve.controlSource;
            var controlDestination = _curve.controlDestination;
            
            var min = math.min(source, destination);
            var max = math.max(source, destination);
            var c = -source + controlSource;
            var b = source - 2 * controlSource + controlDestination;
            var a = -source + 3 * controlSource - 3 * controlDestination + destination;
        
            var h = b*b - a*c;
            if (b.anyGreater(0f))
            {
                var g = sqrt(abs(h));
                var t1 = ((-b - g)/a).saturate();
                var s1 = 1f - t1;
                var t2 = ((-b + g)/a).saturate();
                var s2 = 1f - t2;
                var q1 = s1*s1*s1*source +
                         3.0f * s1*s1*t1*controlSource +
                         3.0f * s1*t1*t1*controlDestination +
                         t1*t1*t1*destination;
                var q2 = s2*s2*s2*source +
                         3.0f * s2*s2*t2*controlSource +
                         3.0f * s2*t2*t2*controlDestination +
                         t2*t2*t2*destination;
                
                if (h.x >= 0)
                {
                    min.x = Mathf.Min(min.x,q1.x, q2.x);
                    max.x = Mathf.Max(max.x, q1.x, q2.x);
                }
                if (h.y >= 0)
                {
                    min.y = Mathf.Min(min.y, q1.y, q2.y);
                    max.y = Mathf.Max(max.y, q1.y, q2.y);
                }
                if (h.z >= 0)
                {
                    min.z = Mathf.Min(min.z, q1.z, q2.z);
                    max.z = Mathf.Max(max.z, q1.z, q2.z);
                }
            }

            return GBox.Minmax(min, max);
        }

#if UNITY_EDITOR
        
        public static void DrawGizmos(this GBezierCurveQuadratic _curve, bool _tangents = false,int _density=64)
        {
            var source = _curve.source;
            var destination = _curve.destination;
            var control = _curve.control;
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(source,.05f);
            Gizmos.DrawLine(source,control);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(destination,.05f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(control,.05f);
            Gizmos.color = Color.white;
            
            Vector3[] points = new Vector3[_density + 1];
            for (int i = 0; i < _density + 1; i++)
            {
                var value = i / (float) _density;
                points[i] = _curve.Evaluate(value);
            }
            Gizmos_Extend.DrawLines(points);

            if (!_tangents)
                return;
            Gizmos.color = KColor.kOrange.SetAlpha(.1f);
            for (int i = 0; i < _density + 1; i++)
            {
                var value = i / (float) _density;
                Gizmos.DrawLine(points[i],points[i]+ (Vector3)_curve.GetTangent(value)*.1f);
            }
        }
        
        public static void DrawGizmos(this GBezierCurveCubic _curve,int _density=64)
        {
            var source = _curve.source;
            var destination = _curve.destination;
            var controlSource = _curve.controlSource;
            var controlDestination = _curve.controlDestination;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(source,.05f);
            Gizmos.DrawLine(source,controlSource);
            Gizmos.DrawLine(destination,controlDestination);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(destination,.05f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(controlSource,.05f);
            Gizmos.DrawWireSphere(controlDestination,.05f);
            Gizmos.color = Color.white;
            
            Vector3[] points = new Vector3[_density + 1];
            for (int i = 0; i < _density+1; i++)
                points[i] = _curve.Evaluate(i / (float)_density);
            Gizmos_Extend.DrawLines(points);
        }
#endif
    }
}