using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Extensions;
using Runtime.Geometry.Extension;
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
        public static readonly G2Box kDefault = new G2Box(0f,.5f);
        public static readonly G2Box kOne = new G2Box(0f,1f);
    }
    
    [Serializable]
    public partial struct G2Box : ISerializationCallbackReceiver,IGeometry2 , IRayAreaIntersection,IConvex2D
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

        public float2 GetUV(float2 _pos) => (_pos - min) / size;
        public float2 GetPoint(float2 _uv) => min + _uv * size;

        public float2 GetSupportPoint(float2 _direction) => this.MaxElement(_p => math.dot(_direction, _p));
        public bool RayIntersection(G2Ray _ray, out float2 distances)
        {
            distances = -1;
            var invRayDir = 1f/(_ray.direction);
            var t0 = (min - _ray.origin)*(invRayDir);
            var t1 = (max - _ray.origin)*(invRayDir);
            var tmin = math.min(t0, t1);
            var tmax = math.max(t0, t1);
            if (tmin.maxElement() > tmax.minElement())
                return false;
            
            var dstA = math.max(tmin.x, tmin.y);
            var dstB = math.min(tmax.x, tmax.y);
            var dstToBox = math.max(0, dstA);
            var dstInsideBox = math.max(0, dstB - dstToBox);
            distances = new float2(dstToBox, dstInsideBox);
            return true;
        }

        public float2 Origin => center;

        public GBox To3XZ() => new GBox(center.to3xz(),extent.to3xz());
        public GBox To3XY() => new GBox(center.to3xy(),extent.to3xy());

        public IEnumerator<float2> GetEnumerator()
        {
            yield return GetPoint(new float2(0, 0));
            yield return GetPoint(new float2(1, 0));
            yield return GetPoint(new float2(1, 1));
            yield return GetPoint(new float2(0, 1));
        }

        public IEnumerable<G2Line> GetEdges()
        {
            yield return new G2Line(GetPoint(new float2(0, 0)), GetPoint(new float2(1, 0)));
            yield return new G2Line(GetPoint(new float2(1, 0)), GetPoint(new float2(1, 1)));
            yield return new G2Line(GetPoint(new float2(1, 1)), GetPoint(new float2(0, 1)));
            yield return new G2Line(GetPoint(new float2(0, 1)), GetPoint(new float2(0, 0)));
        }

        public static G2Box operator +(G2Box _src, float2 _dst) => new G2Box(_src.center+_dst,_src.extent);
        public static G2Box operator -(G2Box _src, float2 _dst) => new G2Box(_src.center-_dst,_src.extent);
        public static G2Box operator /(G2Box _bounds,float2 _div) => new G2Box(_bounds.center/_div,_bounds.extent/_div);
        public static G2Box operator *(G2Box _bounds,float2 _div) => new G2Box(_bounds.center*_div,_bounds.extent*_div);

        public static implicit operator Rect(G2Box _src) => new Rect(_src.min, _src.size);

        public static implicit operator G2Quad(G2Box _src)
        {
            var b = _src.min;
            var f = _src.max;
            var l = new float2(b.x, f.y);
            var r = new float2(f.x, b.y);
            return new G2Quad(b, l, f, r);
        }
        public float4 ToTexelSize() => new float4( size.x, size.y,min.x, min.y);
        
        public override string ToString() => $"G2Box {center} {extent}";
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public void DrawGizmos() => Gizmos.DrawWireCube(center.to3xz(),size.to3xz());
    }

    
}