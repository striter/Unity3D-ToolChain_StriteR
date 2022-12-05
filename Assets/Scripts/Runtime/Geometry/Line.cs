using System;
using UnityEngine;

namespace Geometry
{
    [Serializable]
    public struct GLine:ISerializationCallbackReceiver
    {
        public Vector3 origin;
        public Vector3 direction;
        public float length;
        [HideInInspector]public Vector3 end;
        public Vector3 GetPoint(float _distance) => origin + direction * _distance;
        public GLine(Vector3 _position, Vector3 _direction, float _length) { 
            origin = _position; 
            direction = _direction; 
            length = _length;
            end = default;
            Ctor();
        }
        void Ctor(){end = origin + direction * length;}
        public static implicit operator GRay(GLine _line)=>new GRay(_line.origin,_line.direction);
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize() { Ctor(); }
    }

    [Serializable]
    public struct GRay
    {
        public Vector3 origin;
        public Vector3 direction;
        public GRay(Vector3 _position, Vector3 _direction) { origin = _position; direction = _direction; }
        public Vector3 GetPoint(float _distance) => origin + direction * _distance;
        public static implicit operator Ray(GRay _ray)=>new Ray(_ray.origin,_ray.direction);
        public static implicit operator GRay(Ray _ray)=>new GRay(_ray.origin,_ray.direction);
        public GLine ToLine(float _length)=>new GLine(origin,direction,_length);
    }
}