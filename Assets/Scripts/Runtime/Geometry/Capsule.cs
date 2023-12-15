using System;
using System.Collections.Generic;
using System.ComponentModel;
using Geometry.PointSet;
using Unity.Mathematics;
using UnityEngine;

namespace Geometry
{
    public partial struct Capsule
    {
        public float3 origin;
        public float3 normal;
        public float radius;
        public float height;

        [NonSerialized] public float3 cylinderTop;
        [NonSerialized] public float3 cylinderBottom;
        public Capsule(float3 _origin,float _radius,float3 _normal,float _height)
        {
            this = default;
            origin = _origin;
            radius = _radius;
            normal = _normal;
            height = _height;
            Ctor();
        }

        void Ctor()
        {
            var cylinderExtent = height / 2 * normal;
            cylinderTop = origin + cylinderExtent;
            cylinderBottom = origin - cylinderExtent;
        }
        public float3 Center => origin;
        public static readonly Capsule kDefault = new Capsule(float3.zero, .5f, kfloat3.up, 1f);
    }

    [Serializable]
    public partial struct Capsule : IShape , ISerializationCallbackReceiver
    {
        public Capsule(CapsuleCollider _collider)
        {
            this = default;
            var position = _collider.transform.position;
            var height = _collider.height;
            var normal = kfloat3.up;
            var radius = _collider.radius;
            switch (_collider.direction)
            {
                default: throw new InvalidEnumArgumentException();
                case 0: normal = _collider.transform.right; break;
                case 1: normal = _collider.transform.up; break;
                case 2: normal = _collider.transform.forward; break;
            }
            this = new Capsule(position, radius, normal, height - radius * 2);
        }

        public static Capsule operator *(Matrix4x4 _matrix, Capsule _plane) => new Capsule(_matrix.MultiplyPoint(_plane.origin),_plane.radius, _matrix.MultiplyVector(_plane.normal),_plane.height);

        public float3 GetSupportPoint(float3 _direction)
        {
            throw new System.NotImplementedException();
        }

        public GBox GetBoundingBox()
        {
            var topBounds = new GSphere(cylinderTop, radius).GetBoundingBox();
            var bottomBounds = new GSphere(cylinderBottom, radius).GetBoundingBox();
            return bottomBounds.Encapsulate(topBounds);
        }

        public GSphere GetBoundingSphere()
        {
            var topBounds = new GSphere(cylinderTop, radius);
            var bottomBounds = new GSphere(cylinderBottom, radius);
            return GSphere.Minmax(topBounds,bottomBounds);
        }
        
        public void OnBeforeSerialize(){}
        public void OnAfterDeserialize() => Ctor();
    }
}