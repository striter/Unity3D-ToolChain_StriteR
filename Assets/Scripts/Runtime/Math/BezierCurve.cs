using System;
using System.Linq;
using Geometry.Voxel;
using UnityEngine;

namespace Geometry.Bezier
{
    [Serializable]
    public struct FBezierCurveQuadratic:ISerializationCallbackReceiver
    {
        public Vector3 source;
        public Vector3 control;
        public Vector3 destination;
        
        [HideInInspector] public Vector3 tangentSource;
        [HideInInspector] public Vector3 tangentDestination;
        
        public FBezierCurveQuadratic(Vector3 _src, Vector3 _dst, Vector3 _control)
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
            tangentSource = Vector3.Normalize(control - source);
            tangentDestination = Vector3.Normalize(destination - control);
        }

        public Vector3 Evaluate(float _value)
        {
            float value = _value;
            float oneMinusValue = 1 - value;
            return UMath.Pow2(oneMinusValue) * source + 2 * (oneMinusValue) * value * control + UMath.Pow2(value) * destination;
        }

        public Vector3 GetTangent(float _value) => Vector3.Lerp(tangentSource,tangentDestination,_value).normalized;

        public GBox GetBoundingBox()
        {
            Vector3 min = Vector3.Min(source,destination);
            Vector3 max = Vector3.Max(source,destination);
            GBox box = GBox.Create(min, max);
            if (!box.IsPointInside(control))
            {
                Vector3 t =  (source - control).div(source - 2 * control + destination).clamp(RangeFloat.k01);
                Vector3 s = Vector3.one - t;
                Vector3 q = s.mul(s).mul(source) + 2*s.mul(t).mul(control)+ t.mul(t).mul(destination);
                box = GBox.Create(Vector3.Min(min,q),Vector3.Max(max,q));
            }
            return box;
        }
        
    #if UNITY_EDITOR
        public void DrawGizmos(bool _tangents = false,int _density=64)
        {
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
                points[i] =  Evaluate(value);
            }
            Gizmos_Extend.DrawLines(points);

            if (!_tangents)
                return;
            Gizmos.color = KColor.kOrange.SetAlpha(.1f);
            for (int i = 0; i < _density + 1; i++)
            {
                var value = i / (float) _density;
                Gizmos.DrawLine(points[i],points[i]+GetTangent(value)*.1f);
            }
        }
    #endif
        
    #region Implements
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()=> Ctor();
    #endregion
    }

    [Serializable]
    public struct FBezierCurveCubic
    {
        public Vector3 source;
        public Vector3 controlSource;
        public Vector3 destination;
        public Vector3 controlDestination;

        public Vector3 Evaluate(float _value)
        {
            float value = _value;
            float oneMinusValue = 1 - value;
            return UMath.Pow3(oneMinusValue) * source +  3 * UMath.Pow2(oneMinusValue) * value * controlSource +  3 * oneMinusValue * UMath.Pow2(value) * controlDestination + UMath.Pow3(value) * destination;
        }

        public GBox GetBoundingBox()
        {
            Vector3 min = Vector3.Min(source, destination);
            Vector3 max = Vector3.Max(source, destination);
            Vector3 c = -source + controlSource;
            Vector3 b = source - 2 * controlSource + controlDestination;
            Vector3 a = -source + 3 * controlSource - 3 * controlDestination + destination;
        
            Vector3 h = b.mul(b) - a.mul(c);
            if (b.Greater(0f).Any())
            {
                Vector3 g = new Vector3(Mathf.Sqrt(Mathf.Abs(h.x)),Mathf.Sqrt(Mathf.Abs(h.y)),Mathf.Sqrt(Mathf.Abs(h.z)));
                Vector3 t1 = (-b - g).div(a).clamp(RangeFloat.k01);
                Vector3 s1 = Vector3.one - t1;
                Vector3 t2 = (-b + g).div(a).clamp(RangeFloat.k01);
                Vector3 s2 = Vector3.one - t2;
                Vector3 q1 = s1.mul(s1).mul(s1).mul(source) +
                             3.0f * s1.mul(s1).mul(t1).mul(controlSource) +
                             3.0f * s1.mul(t1).mul(t1).mul(controlDestination) +
                             t1.mul(t1).mul(t1).mul(destination);
                Vector3 q2 = s2.mul(s2).mul(s2).mul(source) +
                             3.0f * s2.mul(s2).mul(t2).mul(controlSource) +
                             3.0f * s2.mul(t2).mul(t2).mul(controlDestination) +
                             t2.mul(t2).mul(t2).mul(destination);
                
                if (h.x >= 0)
                {
                    min.x = Mathf.Min(min.x, Mathf.Min(q1.x, q2.x));
                    max.x = Mathf.Max(max.x, Mathf.Max(q1.x, q2.x));
                }
                if (h.y >= 0)
                {
                    min.y = Mathf.Min(min.y, Mathf.Min(q1.y, q2.y));
                    max.y = Mathf.Max(max.y, Mathf.Max(q1.y, q2.y));
                }
                if (h.z >= 0)
                {
                    min.z = Mathf.Min(min.z, Mathf.Min(q1.z, q2.z));
                    max.z = Mathf.Max(max.z, Mathf.Max(q1.z, q2.z));
                }
            }

            return GBox.Create(min, max);
        }
        
#if UNITY_EDITOR
        public void DrawGizmos(int _density=64)
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
            
            Vector3[] points = new Vector3[_density + 1];
            for (int i = 0; i < _density+1; i++)
                points[i] = Evaluate(i / (float)_density);
            Gizmos_Extend.DrawLines(points);
        }
#endif
    }
}