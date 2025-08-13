using System;
using System.Collections.Generic;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    [Serializable]
    public struct GLine: ISerializationCallbackReceiver , ILine , ISDF
    {
        public float3 start;
        public float3 end;
        [HideInInspector]public float3 direction;
        [HideInInspector]public float length;
        public float3 GetPoint(float _distance) => start + direction * _distance;
        public float3 GetPointNormalized(float _normalized) => start + direction * _normalized * length;

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
        
        public GLine(IList<float3> _vertices,PLine _src):this(_vertices[_src.start],_vertices[_src.end]) => Ctor();
        public static GLine FromPositions<T>(IList<T> _vertices, PLine _src, Func<T, float3> _selector) =>  new GLine(_selector(_vertices[_src.start]), _selector(_vertices[_src.end]));
        void Ctor()
        {
            var offset = end - start;
            length = offset.magnitude();
            direction = offset.safeNormalize();
        }
        
        public static implicit operator GRay(GLine _line)=>new GRay(_line.start,_line.direction);
        
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize() { Ctor(); }
        
        public static GLine StartEnd(float3 _start, float3 _end) => new GLine(_start, _end);
        public GLine Trim(RangeFloat _range) => new GLine(start + direction * length * _range.start, start + direction * length * _range.end);
        public GRay ToRay()=>new GRay(start,direction);
        
        public static readonly GLine kDefault = new GLine(float3.zero, kfloat3.forward);
        public static readonly GLine kZero = new GLine(float3.zero, float3.zero);
        public void DrawGizmos() => Gizmos.DrawLine(start, end);
        public float3 Origin => start + end / 2;
        public float SDF(float3 _position)
        {
            var lineDirection = direction;
            var pointToStart = _position - start;
            return math.length(math.cross(lineDirection, pointToStart));
        }
    }
    
}