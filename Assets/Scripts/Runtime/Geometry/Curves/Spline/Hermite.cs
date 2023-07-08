using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Geometry.Curves.Spline
{
    [Serializable]
    public struct GHermitePoint
    {
        public float3 position;
        [PostNormalize]public float3 normal;
    }
    
    [Serializable]
    public struct GHermiteSpline : ISpline<float3>
    {
        public GHermitePoint[] positions;
        public float3 Evaluate(float _value)
        {
            var count = positions.Length - 1;
            _value *= count;
            var start = (int)math.floor(_value );
            if (start == count)
                return positions.Last().position;
            var t = _value - start;
            var P0 = positions[start].position;
            var P1 = positions[start + 1].position;
            var T0 = positions[start].normal;
            var T1 = positions[start + 1].normal;
            return new GHermiteCurve(P0, T0, P1, T1).Evaluate(t);
        }

        public IEnumerable<float3> Coordinates => positions.Select(p => p.position);

        public static readonly GHermiteSpline kDefault = new GHermiteSpline()
        {
            positions = new GHermitePoint[]
            {
                new(){position = kfloat3.left,normal = kfloat3.forward},
                new(){position = kfloat3.forward,normal = kfloat3.back},
                new(){position = kfloat3.right,normal = kfloat3.forward},
            }
        };
    }
}
