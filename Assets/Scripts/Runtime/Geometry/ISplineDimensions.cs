using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry.Curves.Spline
{
    public interface ISplineDimensions<T> : ICurveDimensions<T>
    {
        IEnumerable<T> Coordinates { get; }
    }

    public static class USpline
    {
        public static void DrawGizmos(this ISplineDimensions<float3> _curve,int _amount = 64,float _sphereSize = .05f)
        {
            var outputs = _curve.Output(_amount);
            Gizmos.color = Color.white;
            UGizmos.DrawLines(outputs, p => p);
            
            Gizmos.color = Color.green;
            foreach (var coord in _curve.Coordinates)
                Gizmos.DrawSphere(coord,_sphereSize);
            Gizmos.color = Color.white.SetA(.5f);
            UGizmos.DrawLines(_curve.Coordinates,p=>p);
        }
    }
}