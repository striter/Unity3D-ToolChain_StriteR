using System;
using System.ComponentModel;
using Procedural;
using Unity.Mathematics;
using UnityEngine;

namespace Geometry
{
    public partial struct G2Box
    {
        public float2 center;
        public float2 extend;
        [NonSerialized] public float2 size;
        [NonSerialized] public float2 min;
        [NonSerialized] public float2 max;
        public G2Box(float2 _center, float2 _extend)
        {
            this = default;
            center = _center;
            extend = _extend;
            Ctor();
        }
        
        void Ctor()
        {
            size = extend * 2f;
            min = center - extend;
            max = center + extend;
        }

        public G2Box Move(float2 _deltaPosition)=> new G2Box(center + _deltaPosition, extend);
        
        public static G2Box Minmax(float2 _min, float2 _max)
        {
            float2 size = _max - _min;
            float2 extend = size / 2;
            return new G2Box(_min+extend,extend);
        }

        public bool Contains(float2 _point,float _bias = float.Epsilon)
        {
            var absOffset = math.abs(center-_point) + _bias;
            return absOffset.x < extend.x && absOffset.y < extend.y;
        }
    }
    
    public partial struct GBox
    {
        public float3 center;
        public float3 extend;
        [NonSerialized] public float3 size;
        [NonSerialized] public float3 min;
        [NonSerialized] public float3 max;
        public GBox(float3 _center, float3 _extend)
        {
            this = default;
            center = _center;
            extend = _extend;
            Ctor();
        }
        
        void Ctor()
        {
            size = extend * 2f;
            min = center - extend;
            max = center + extend;
        }

        public GBox Move(float3 _deltaPosition)=> new GBox(center + _deltaPosition, extend);
        
        public static GBox Minmax(float3 _min, float3 _max)
        {
            float3 size = _max - _min;
            float3 extend = size / 2;
            return new GBox(_min+extend,extend);
        }

        public GBox Split(float3 _anchor, float3 _sizeRatio)
        {
            var min = this.min + size * _anchor;
            return GBox.Minmax(min, min + size * _sizeRatio);
        }
        
        
        public bool Contains(float3 _point,float _bias = float.Epsilon)
        {
            float3 absOffset = math.abs(center-_point) + _bias;
            return absOffset.x < extend.x && absOffset.y < extend.y && absOffset.z < extend.z;
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