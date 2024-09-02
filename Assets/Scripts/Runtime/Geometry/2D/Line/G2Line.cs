using System;
using Runtime.Geometry.Extension;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    [Serializable]
    public struct G2Line : ISDF2
    {
        public float2 start;
        public float2 end;
        [NonSerialized] public float2 direction;
        public G2Line(float2 _start,float2 _end)
        {
            start = _start;
            end = _end;

            direction = default;
            Ctor();
        }

        void Ctor()
        {
            direction = (end - start).normalize();
        }

        public float2 Origin => start;
        public void DrawGizmos() => Gizmos.DrawLine(start.to3xz(),end.to3xz());

        public float SDF(float2 _position)
        {
            var lineDirection = direction;
            var pointToStart = _position - start;
            return math.length(umath.cross(lineDirection, pointToStart));
        }
    }
}