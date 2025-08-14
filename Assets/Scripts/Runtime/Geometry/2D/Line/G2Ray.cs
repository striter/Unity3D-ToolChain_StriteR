using System;
using Unity.Mathematics;

namespace Runtime.Geometry
{
    [Serializable]
    public partial struct G2Ray
    {
        public float2 origin;
        public float2 direction;

        public G2Ray(float2 _position, float2 _direction)
        {
            origin = _position;
            direction = _direction;
        }

    }
}