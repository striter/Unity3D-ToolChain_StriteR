using System;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry.Curves
{
    using static math;
    using static umath;
    
    [Serializable]
    public partial struct GBezierCurveCubic:ICurveDimensions<float3>
    {
        public float3 source;
        public float3 controlSource;
        public float3 destination;
        public float3 controlDestination;

        public GBezierCurveCubic(float3 _src, float3 _dst, float3 _srcCtr, float3 _dstCtr)
        {
            source = _src;
            destination = _dst;
            controlSource = _srcCtr;
            controlDestination = _dstCtr;
        }
        
        public float3 Evaluate(float _value)
        {
            float value = _value;
            float oneMinusValue = 1 - value;
            return pow3(oneMinusValue) * source +  3 * sqr(oneMinusValue) * value * controlSource +  3 * oneMinusValue * sqr(value) * controlDestination + pow3(value) * destination;
        }


        public void DrawGizmos() => DrawGizmos(64);
        public void DrawGizmos(int _density)
        {
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
            UCurve.DrawGizmos(this,_density);
        }
    }

    public static class GBezierCurveCubic_Extension
    {
        
        public static void Split(this GBezierCurveCubic _src, float _value, out GBezierCurveCubic _Q,out GBezierCurveCubic _R)       //de Casteljau
        {
            var P0 = _src.source;
            var P1 = _src.controlSource;
            var P2 = _src.controlDestination;
            var P3 = _src.destination;

            var s = _value;
            var BS = _src.Evaluate(_value);

            var P11 = lerp(P1,P2,s);
        
            var Q1 = lerp(P0,P1,s);
            var Q2 = lerp(Q1, P11, s);

            var R2 = lerp(P2,P3,s);
            var R1 = lerp(P11, R2, s);
        
            _Q = new GBezierCurveCubic(P0, BS, Q1,Q2);
            _R = new GBezierCurveCubic(BS, P3,R1,R2);
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

    }
}