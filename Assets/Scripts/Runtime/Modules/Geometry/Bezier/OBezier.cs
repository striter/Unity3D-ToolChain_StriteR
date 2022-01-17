using System;
using UnityEngine;

namespace Geometry.Bezier
{
    [Serializable]
    public struct FBezierCurveQuadratic
    {
        public Vector3 source;
        public Vector3 destination;
        public Vector3 control;

        public FBezierCurveQuadratic(Vector3 _src, Vector3 _dst, Vector3 _control)
        {
            source = _src;
            destination = _dst;
            control = _control;
        }

        public Vector3 Evaluate(float _value)=> UMath.QuadraticBezierCurve(source,destination,control,_value);
        
#if UNITY_EDITOR
        public void DrawGizmos(int _density=64)
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
            for (int i = 0; i < _density+1; i++)
                points[i] = Evaluate(i / (float)_density);
            Gizmos_Extend.DrawLines(points);
        }
#endif
    }

    [Serializable]
    public struct FBezierCurveCubic
    {
        public Vector3 source;
        public Vector3 destination;
        public Vector3 controlSource;
        public Vector3 controlDestination;

        public Vector3 Evaluate(float _value) =>
            UMath.CubicBezierCurve(source, destination, controlSource, controlDestination, _value);

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