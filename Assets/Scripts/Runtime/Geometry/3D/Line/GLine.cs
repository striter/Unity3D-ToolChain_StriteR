using System;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    [Serializable]
    public struct GLine:ISerializationCallbackReceiver
    {
        public float3 start;
        public float3 end;
        [HideInInspector]public float3 direction;
        [HideInInspector]public float length;
        public float3 GetPoint(float _distance) => start + direction * _distance;

        public GLine(float3 _start, float3 _end)
        {
            start = _start;
            end = _end;
            direction = default;
            length = default;
            Ctor();
        }

        public GLine(float3 _start, float3 _direction, float _length) { 
            start = _start; 
            direction = _direction; 
            length = _length;
            end = start + direction * length;
        }
        
        void Ctor()
        {
            var offset = end - start;
            direction = offset.normalize();
            length = offset.magnitude();
        }
        
        public static implicit operator GRay(GLine _line)=>new GRay(_line.start,_line.direction);
        
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize() { Ctor(); }
        
        public GRay ToRay()=>new GRay(start,direction);
        
        public static readonly GLine kDefault = new GLine(float3.zero, kfloat3.forward);
    }
    
}