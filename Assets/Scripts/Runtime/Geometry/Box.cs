using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geometry
{
    [Serializable]
    public struct GBox:ISerializationCallbackReceiver
    {
        public Vector3 center;
        public Vector3 extend;
        [NonSerialized] public Vector3 size;
        [NonSerialized] public Vector3 min;
        [NonSerialized] public Vector3 max;
        public GBox(Vector3 _center, Vector3 _extend)
        {
            center = _center;
            extend = _extend;
            size = default;
            min = default;
            max = default;
            Ctor();
        }
        void Ctor()
        {
            size = extend * 2f;
            min = center - extend;
            max = center + extend;
        }
        
        public static GBox Create(Vector3 _min, Vector3 _max)
        {
            Vector3 size = _max - _min;
            Vector3 extend = size / 2;
            return new GBox(_min+extend,extend);
        }
        public static GBox Create(params Vector3[] _points)
        {
            int length = _points.Length;
            Vector3 min = Vector3.positiveInfinity;
            Vector3 max = Vector3.negativeInfinity;
            for (int i = 0; i < length; i++)
            {
                min = Vector3.Min(min,_points[i]);
                max = Vector3.Max(max,_points[i]);
            }
            return Create(min, max);
        }
        
        public Bounds ToBounds()=> new Bounds(center, size);
        
        public bool IsPointInside(Vector3 _point)=> 
            _point.x >= min.x && _point.x <= max.x && 
            _point.y >= min.y && _point.y <= max.y && 
            _point.z >= min.z && _point.z <= max.z;
        
        public void OnBeforeSerialize(){  }
        public void OnAfterDeserialize()=>Ctor();
    }

}