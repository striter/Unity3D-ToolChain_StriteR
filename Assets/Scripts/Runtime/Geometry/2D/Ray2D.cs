using System;
using Unity.Mathematics;

namespace Geometry
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