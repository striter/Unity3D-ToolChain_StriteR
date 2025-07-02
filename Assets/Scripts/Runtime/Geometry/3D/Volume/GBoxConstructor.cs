﻿using Unity.Mathematics;

namespace Runtime.Geometry
{
    public partial struct GBox
    {
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
        public GBox Encapsulate(GBox _other) => Minmax(math.min(min, _other.min), math.max(max, _other.max));
        public GBox Encapsulate(float3 _point) => Minmax(math.min(min, _point), math.max(max, _point));
        public GBox Encapsulate(GSphere _sphere) => Minmax(math.min(min, _sphere.center - _sphere.radius), math.max(max, _sphere.center + _sphere.radius));
    }
}