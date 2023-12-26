using System;
using System.Collections.Generic;
using System.ComponentModel;
using Procedural;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Geometry
{
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
    public partial struct GBox : ISerializationCallbackReceiver,IShape3D
    {
        public void OnBeforeSerialize(){  }
        public void OnAfterDeserialize()=>Ctor();
        public float3 GetSupportPoint(float3 _direction)
        {
            var ray = new GRay(center, _direction.normalize());
            return ray.GetPoint(Validation.UGeometry.Distance(ray, this).sum());
        }
        public float3 GetPoint(float3 _uvw) => center + _uvw * size;
        public float3 Center => center;

        
        public bool Contains(float3 _point,float _bias = float.Epsilon)
        {
            float3 absOffset = math.abs(center-_point) + _bias;
            return absOffset.x < extent.x && absOffset.y < extent.y && absOffset.z < extent.z;
        }

        
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
            yield return GetPoint(new float3(-.5f,.5f,-.5f));
            yield return GetPoint(new float3(-.5f,-.5f,.5f));
            yield return GetPoint(new float3(-.5f,.5f,.5f));
            yield return GetPoint(new float3(.5f,-.5f,-.5f));
            yield return GetPoint(new float3(.5f,.5f,-.5f));
            yield return GetPoint(new float3(.5f,-.5f,.5f));
            yield return GetPoint(new float3(.5f,.5f,.5f));
        }
        
        public static GBox operator +(GBox _src, float3 _dst) => new GBox(_src.center+_dst,_src.extent);
        public static implicit operator Bounds(GBox _box)=> new Bounds(_box.center, _box.size);
        public static implicit operator GBox(Bounds _bounds) => new GBox(_bounds.center,_bounds.extents);
        
        public static readonly GBox kDefault = new GBox(0f,.5f);
    }
}