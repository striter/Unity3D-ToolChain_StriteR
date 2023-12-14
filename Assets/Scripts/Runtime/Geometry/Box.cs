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
    }

    [Serializable]
    public partial struct G2Box : ISerializationCallbackReceiver,I2Shape
    {
        public void OnBeforeSerialize(){  }
        public void OnAfterDeserialize()=>Ctor();

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
        public static readonly G2Box kDefault = new G2Box(0f,.5f);
        public float2 GetSupportPoint(float2 _direction)
        {
            var ray = new G2Ray(center, _direction.normalize());
            return ray.GetPoint(Validation.UGeometry.Distance.Eval(ray, this).sum());
        }
        public float2 Center => center;
        public static G2Box operator /(G2Box _bounds,float2 _div) => new G2Box(_bounds.center/_div,_bounds.extent/_div);
        public static G2Box operator -(G2Box _bounds,float2 _minus) => new G2Box(_bounds.center - _minus,_bounds.extent);

        public GBox To3XZ() => new GBox(center.to3xz(),extent.to3xz());
        public GBox To3XY() => new GBox(center.to3xy(),extent.to3xy());
        public override string ToString() => $"G2Box {center} {extent}";
    }

    [Serializable]
    public partial struct GBox : ISerializationCallbackReceiver,IShape
    {
        public void OnBeforeSerialize(){  }
        public void OnAfterDeserialize()=>Ctor();
        public float3 GetSupportPoint(float3 _direction)
        {
            var ray = new GRay(center, _direction.normalize());
            return ray.GetPoint(Validation.UGeometry.Distance.Eval(ray, this).sum());
        }
        public float3 Center => center;

        public GBox Move(float3 _deltaPosition)=> new GBox(center + _deltaPosition, extent);
        public float3 GetPoint(float3 _uvw) => center + _uvw * size;
        
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

        public GBox Encapsulate(GBox _other) => Minmax(math.min(min, _other.min), math.max(max, _other.max));
        
        public IEnumerable<GPlane> GetPlanes(bool _x,bool _y,bool _z)
        {
            if (_x)
            {
                yield return new GPlane(kfloat3.left,GetPoint(kfloat3.right*.5f));
                yield return new GPlane(kfloat3.right,GetPoint(kfloat3.left*.5f));
            }

            if (_y)
            {
                yield return new GPlane(kfloat3.down,GetPoint(kfloat3.up*.5f));
                yield return new GPlane(kfloat3.up,GetPoint(kfloat3.down*.5f));
            }

            if (_z)
            {
                yield return new GPlane(kfloat3.forward,GetPoint(kfloat3.back*.5f));
                yield return new GPlane(kfloat3.back,GetPoint(kfloat3.forward*.5f));
            }
        }

        public IEnumerable<float3> GetPositions()
        {
            yield return GetPoint(new float3(-.5f,-.5f,-.5f));
            yield return GetPoint(new float3(.5f,-.5f,-.5f));
            yield return GetPoint(new float3(.5f,.5f,-.5f));
            yield return GetPoint(new float3(-.5f,.5f,-.5f));
            yield return GetPoint(new float3(-.5f,-.5f,.5f));
            yield return GetPoint(new float3(.5f,-.5f,.5f));
            yield return GetPoint(new float3(.5f,.5f,.5f));
            yield return GetPoint(new float3(-.5f,.5f,.5f));
        }
        
        public static GBox operator +(GBox _src, float3 _dst) => new GBox(_src.center+_dst,_src.extent);
        public static implicit operator Bounds(GBox _box)=> new Bounds(_box.center, _box.size);
        public static implicit operator GBox(Bounds _bounds) => new GBox(_bounds.center,_bounds.extents);
        
        public static readonly GBox kDefault = new GBox(0f,.5f);
    }
}