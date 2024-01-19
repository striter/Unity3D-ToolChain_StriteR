using System;
using Unity.Mathematics;

namespace Runtime.Geometry
{
    [Serializable]
    public struct G2Line
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
    }
}