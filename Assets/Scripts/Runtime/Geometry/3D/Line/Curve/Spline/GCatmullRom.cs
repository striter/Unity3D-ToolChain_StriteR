using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry.Curves.Spline
{
    [Serializable]
    public struct GCatmullRomSpline:ISpline
    {
        public float3[] positions;
        public GCatmullRomSpline(params float3[] _positions)
        {
            positions = _positions;
        }

        public float3 Evaluate(float _value)
        {
            if (_value == 0)
                return positions[1];
            if (_value >= 0.999f)
                return positions[^2];
            var count = positions.Length - 3;
            _value *= count;
            var start = (int)math.floor(_value) + 1;
            var t = _value %1;
            var P0 = positions[start];
            var P1 = positions[start + 1];
            var T0 =  (P1 - positions[start-1]) / 2;
            var T1 = (positions[start + 2] - P0) / 2;
            return new GHermiteCurve(P0, T0, P1, T1).Evaluate(t);
        }

        public static readonly GCatmullRomSpline kDefault = new GCatmullRomSpline(kfloat3.right,kfloat3.forward,kfloat3.left,kfloat3.back,kfloat3.up,kfloat3.down);
        public IEnumerable<float3> Coordinates => positions;

        public float3 Origin => positions[0];
        public void DrawGizmos() => this.DrawGizmos(64);
    }

}