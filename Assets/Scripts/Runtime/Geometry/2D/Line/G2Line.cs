using System;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    [Serializable]
    public struct G2Line : ISDF<float2> , IRayIntersection2 , ISerializationCallbackReceiver
    {
        public float2 start;
        public float2 end;
        [NonSerialized] public float2 direction;
        [NonSerialized] public float length,sqrLength;
        public G2Line(float2 _start,float2 _end)
        {
            this = default;
            start = _start;
            end = _end;
            Ctor();
        }

        void Ctor()
        {
            var delta = end - start;
            sqrLength = delta.sqrmagnitude();
            length = math.sqrt(sqrLength);
            if(length ==0) 
                return;
            direction = delta / length;
        }

        public G2Line(float2 _start, float2 _direction, float _length)
        {
            start = _start;
            end = _start + _direction * _length;
            direction = _direction;
            sqrLength = _length * _length;
            length = _length;
        }
        
        public static implicit operator G2Ray(G2Line _ray) => new G2Ray(_ray.start,_ray.direction);
        
        public float2 Origin => start;
        public void DrawGizmos() => Gizmos.DrawLine(start.to3xz(),end.to3xz());

        public float SDF(float2 _position)
        {
            var lineDirection = direction;
            var pointToStart = _position - start;
            return math.length(umath.cross(lineDirection, pointToStart));
        }

        public bool RayIntersection(G2Ray _ray, out float distance)
        {
            var projection = _ray.Projection(this);
            distance = projection.x;
            return distance > 0 && projection.y >= 0 && projection.y <= length;
        }
        
        public float2 GetPoint(float _distance) => start + direction * _distance;
        public float2 GetPointNormalized(float _normalizedDistance) => start + direction * _normalizedDistance * length;

        public void OnBeforeSerialize(){}
        public void OnAfterDeserialize() => Ctor();
        
        public static G2Line kDefault => new G2Line(float2.zero,kfloat2.right);
        public static G2Line kZero => new G2Line(float2.zero,float2.zero);
        
        public static G2Line operator -(G2Line _line,float2 _position) => new G2Line(_line.start - _position,_line.end - _position);
        public static G2Line operator +(G2Line _line,float2 _position) => new G2Line(_line.start + _position,_line.end + _position);
    }
}