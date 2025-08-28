using System;
using System.Collections.Generic;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    public partial struct G2Box
    {
        public static implicit operator float4(G2Box _box) => new(_box.center.x, _box.center.y, _box.extent.x, _box.extent.y);
        public static implicit operator Rect(G2Box _src) => new(_src.min, _src.size);
        public static implicit operator G2Box(Rect _src) => new(_src.center, _src.size / 2);
        public static implicit operator G2Quad(G2Box _src)
        {
            var b = _src.min;
            var f = _src.max;
            var l = new float2(b.x, f.y);
            var r = new float2(f.x, b.y);
            return new G2Quad(b, l, f, r);
        }
        
        public static G2Box Normalize(G2Box _src, G2Box _normalize)
        {
            var min = math.lerp(_src.min, _src.max, _normalize.min);
            var max = math.lerp(_src.min, _src.max, _normalize.max);
            return new G2Box(min, max - min);
        }
        public G2Box Resize(float _factor) => Minmax(center - extent * _factor, center + extent * _factor);
        public G2Box Resize(float2 newSize) => new G2Box(center, newSize / 2);
        public G2Box Collapse(float2 _factor, float2 _center = default) => new(center + _center * size, extent * _factor);
        public G2Box Move(float2 _deltaPosition)=> new G2Box(center + _deltaPosition, extent);
        
        public static G2Box MinSize(float2 _min, float2 _size) => new G2Box(_min + _size / 2, _size / 2);
        public static G2Box Minmax(float2 _min, float2 _max)
        {
            var size = _max - _min;
            var extend = size / 2;
            return new G2Box(_min+extend,extend);
        }

        public IEnumerable<G2Box> Divide(int _division)
        {
            var size = this.size / _division;
            for (var i = 0; i < _division; i++)
            for (var j = 0; j < _division; j++)
                yield return MinSize(min + size * new float2(i, j), size);
        }

        public static G2Box Clamp(G2Box _clamp,G2Box _dst) => Minmax(math.clamp(_dst.min,_clamp.min,_clamp.max), math.clamp(_dst.max,_clamp.min,_clamp.max));

        public static G2Box GetBoundingBox(IEnumerable<float2> _positions)
        {
            UGeometry.Minmax(_positions,out var min,out var max);
            return Minmax(min,max);
        }
        
        public static G2Box GetBoundingBox<T>(IList<T> _elements, Func<T, float2> _convert)
        {
            var min = kfloat2.max;
            var max = kfloat2.min;
            for(var i = _elements.Count - 1; i>=0;i--)
            {
                var position = _convert( _elements[i]);
                min = math.min(position, min);
                max = math.max(position, max);
            }

            return G2Box.Minmax(min,max);
        }
    }
}