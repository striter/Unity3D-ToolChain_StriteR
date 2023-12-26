using System;
using Unity.Mathematics;

namespace Geometry
{
    [Serializable]
    public struct G2Ray
    {
        public float2 origin;
        public float2 direction;

        public G2Ray(float2 _position, float2 _direction)
        {
            origin = _position;
            direction = _direction;
        }

        public float2 GetPoint(float _distance) => origin + direction * _distance;
    }
}