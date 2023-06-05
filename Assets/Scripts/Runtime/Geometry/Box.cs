using System;
using System.Collections.Generic;
using System.ComponentModel;
using Procedural;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Geometry
{
    public partial struct G2Box
    {
        public float2 center;
        public float2 extent;
        [NonSerialized] public float2 size;
        [NonSerialized] public float2 min;
        [NonSerialized] public float2 max;
        public G2Box(float2 _center, float2 _extent)
        {
            this = default;
            center = _center;
            extent = _extent;
            Ctor();
        }
        
        void Ctor()
        {
            size = extent * 2f;
            min = center - extent;
            max = center + extent;
        }

        public G2Box Move(float2 _deltaPosition)=> new G2Box(center + _deltaPosition, extent);
        
        public static G2Box Minmax(float2 _min, float2 _max)
        {
            float2 size = _max - _min;
            float2 extend = size / 2;
            return new G2Box(_min+extend,extend);
        }

        public bool Contains(float2 _point,float _bias = float.Epsilon)
        {
            var absOffset = math.abs(center-_point) + _bias;
            return absOffset.x < extent.x && absOffset.y < extent.y;
        }

        public float2 GetPoint(float2 _uv) => min + _uv * size;
    }
    
    public partial struct GBox
    {
        public float3 center;
        public float3 extent;
        [NonSerialized] public float3 size;
        [NonSerialized] public float3 min;
        [NonSerialized] public float3 max;
        public GBox(float3 _center, float3 _extent)
        {
            this = default;
            center = _center;
            extent = _extent;
            Ctor();
        }
        
        void Ctor()
        {
            size = extent * 2f;
            min = center - extent;
            max = center + extent;
        }

        public GBox Move(float3 _deltaPosition)=> new GBox(center + _deltaPosition, extent);
        
        public static GBox Minmax(float3 _min, float3 _max)
        {
            float3 size = _max - _min;
            float3 extend = size / 2;
            return new GBox(_min+extend,extend);
        }

        public GBox Split(float3 _anchor, float3 _sizeRatio)
        {
            var min = this.min + size * _anchor;
            return Minmax(min, min + size * _sizeRatio);
        }
        
        
        public bool Contains(float3 _point,float _bias = float.Epsilon)
        {
            float3 absOffset = math.abs(center-_point) + _bias;
            return absOffset.x < extent.x && absOffset.y < extent.y && absOffset.z < extent.z;
        }
        
    }

    [Serializable]
    public partial struct G2Box : ISerializationCallbackReceiver
    {
        public void OnBeforeSerialize(){  }
        public void OnAfterDeserialize()=>Ctor();
    }
    
    [Serializable]
    public partial struct GBox : ISerializationCallbackReceiver
    {
        public void OnBeforeSerialize(){  }
        public void OnAfterDeserialize()=>Ctor();
        public Bounds ToBounds()=> new Bounds(center, size);
    }
}