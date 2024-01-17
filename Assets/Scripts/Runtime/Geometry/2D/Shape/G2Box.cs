using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
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
    }
    
    [Serializable]
    public partial struct G2Box : ISerializationCallbackReceiver,IShape2D
    {
        public void OnBeforeSerialize(){  }
        public void OnAfterDeserialize()=>Ctor();

        public G2Box Move(float2 _deltaPosition)=> new G2Box(center + _deltaPosition, extent);
        
        public static G2Box Minmax(float2 _min, float2 _max)
        {
            var size = _max - _min;
            var extend = size / 2;
            return new G2Box(_min+extend,extend);
        }

        public bool Contains(float2 _point,float _bias = float.Epsilon)
        {
            var absOffset = math.abs(center-_point) + _bias;
            return absOffset.x < extent.x && absOffset.y < extent.y;
        }

        public float2 GetPoint(float2 _uv) => min + _uv * size;
        public static readonly G2Box kDefault = new G2Box(0f,.5f);

        public float2 GetSupportPoint(float2 _direction) => GetPoints().MaxElement(_p => math.dot(_direction, _p));
        
        public float2 Center => center;
        public static G2Box operator /(G2Box _bounds,float2 _div) => new G2Box(_bounds.center/_div,_bounds.extent/_div);
        public static G2Box operator -(G2Box _bounds,float2 _minus) => new G2Box(_bounds.center - _minus,_bounds.extent);

        public GBox To3XZ() => new GBox(center.to3xz(),extent.to3xz());
        public GBox To3XY() => new GBox(center.to3xy(),extent.to3xy());
        public IEnumerable<float2> GetPoints()
        {
            yield return GetPoint(new float2(0, 0));
            yield return GetPoint(new float2(1, 0));
            yield return GetPoint(new float2(1, 1));
            yield return GetPoint(new float2(0, 1));
        }

        public override string ToString() => $"G2Box {center} {extent}";
    }

    
}