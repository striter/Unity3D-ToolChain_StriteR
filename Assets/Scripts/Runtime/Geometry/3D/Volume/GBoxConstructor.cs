using System;
using System.Collections.Generic;
using Runtime.Geometry.Extension;
using Unity.Mathematics;

namespace Runtime.Geometry
{
    public partial struct GBox
    {
        public GBox Move(float3 _deltaPosition)=> new GBox(center + _deltaPosition, extent);
        
        public static GBox Minmax(float3 _min, float3 _max)
        {
            var size = _max - _min;
            var extend = size / 2;
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
        
        public static GBox GetBoundingBox(IEnumerable<float3> _positions)
        {
            UGeometry.Minmax(_positions,out var min,out var max);
            return GBox.Minmax(min,max);
        }
        
        public static GBox GetBoundingBox<T>(IList<T> _elements, Func<T, float3> _convert)
        {
            var min = kfloat3.max;
            var max = kfloat3.min;
            for(var i = _elements.Count - 1; i>=0;i--)
            {
                var position = _convert( _elements[i]);
                min = math.min(position, min);
                max = math.max(position, max);
            }

            return GBox.Minmax(min,max);
        }

        public static GBox GetBoundingBoxOriented(float3 _right,float3 _up,float3 _forward,IEnumerable<float3> _points)
        {
            float3 min = float.MaxValue;
            float3 max = float.MinValue;
            foreach (var point in _points)
            {
                var cur = new float3(math.dot(point,_right),math.dot(point,_up),math.dot(point,_forward));
                min = math.min(cur, min);
                max = math.max(cur, max);
            }

            return GBox.Minmax(min, max);
        }

        public static GBox GetBoundingBox(IEnumerable<GBox> _boxes)
        {
            var min = kfloat3.max;
            var max = kfloat3.min;
            foreach (var box in _boxes)
            {
                min = math.min(box.min, min);
                max = math.max(box.max, max);
            }
            return GBox.Minmax(min, max);
        }

        
    }
}