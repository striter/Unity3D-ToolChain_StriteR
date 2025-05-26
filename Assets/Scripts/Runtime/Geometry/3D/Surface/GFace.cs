using System;
using System.Collections.Generic;
using System.Linq.Extensions;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    public partial struct GFace
    {
        public GCoordinates coordinates;
        public float2 size;
        [NonSerialized] public float2 extent;

        public GFace(float3 _origin, float3 _right, float3 _up, float2 _size)
        {
            this = default;
            coordinates = new GCoordinates(_origin, _right, _up);
            size = _size;
            Ctor();
        }

        void Ctor()
        {
            extent = size / 2f;
        }
    }
    
    public partial struct GFace : ISurface , IVolume , ISerializationCallbackReceiver
    {
        public quaternion GetRotation() => coordinates.GetRotation();
        public IEnumerable<float3> GetPoints()
        {
            yield return coordinates.origin + extent.x * coordinates.right - extent.y * coordinates.forward;
            yield return coordinates.origin - extent.x * coordinates.right - extent.y * coordinates.forward;
            yield return coordinates.origin - extent.x * coordinates.right + extent.y * coordinates.forward;
            yield return coordinates.origin + extent.x * coordinates.right + extent.y * coordinates.forward;
        }
        
        public void DrawGizmos()
        {
            coordinates.DrawGizmos();
            Gizmos.color = Color.white;
            UGizmos.DrawLinesConcat(GetPoints());
        }

        public float3 Origin => coordinates.Origin;
        public float3 Normal => coordinates.Normal;
        public float3 GetSupportPoint(float3 _direction) => this.GetPoints().MaxElement(p=> math.dot(p, _direction));
        public GBox GetBoundingBox() => UGeometry.GetBoundingBox(this.GetPoints());
        public GSphere GetBoundingSphere() => UGeometry.GetBoundingSphere(this.GetPoints());
        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize() => Ctor();
    }
    
    
}