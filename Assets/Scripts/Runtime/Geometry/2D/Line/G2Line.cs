using System;
using Unity.Mathematics;

namespace Runtime.Geometry
{
    [Serializable]
    public struct G2Line
    {
        public float2 start;
        public float2 end;

        public G2Line(float2 _start,float2 _end)
        {
            start = _start;
            end = _end;
        }
    }
}