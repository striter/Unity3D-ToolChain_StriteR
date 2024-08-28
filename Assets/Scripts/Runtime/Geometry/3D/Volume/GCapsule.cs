using System;
using System.ComponentModel;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Runtime.Geometry
{
    public partial struct GCapsule
    {
        public float3 origin;
        public float3 normal;
        public float radius;
        public float height;

        [NonSerialized] public float3 cylinderTop;
        [NonSerialized] public float3 cylinderBottom;
        public GCapsule(float3 _origin,float _radius,float3 _normal,float _height)
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
        public float3 Origin => origin;
        public static readonly GCapsule kDefault = new GCapsule(float3.zero, .5f, kfloat3.up, 1f);
    }

    [Serializable]
    public partial struct GCapsule : IVolume , ISerializationCallbackReceiver , IRayIntersection , ISDF
    {
        public GCapsule(CapsuleCollider _collider)
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
            this = new GCapsule(position, radius, normal, height - radius * 2);
        }
        
        public static GCapsule operator +(GCapsule _src, float3 _dst) => new GCapsule(_src.origin+_dst, _src.radius, _src.normal, _src.height);
        public static GCapsule operator *(Matrix4x4 _matrix, GCapsule _plane) => new GCapsule(_matrix.MultiplyPoint(_plane.origin),_plane.radius, _matrix.MultiplyVector(_plane.normal),_plane.height);

        public float3 GetSupportPoint(float3 _direction)
        {
            var center = Origin;
            float distanceAlongDirection = math.dot(_direction, cylinderTop - center);
        
            float3 supportPoint;
            if (distanceAlongDirection >= 0)
                supportPoint = cylinderTop  + radius * _direction;
            else
                supportPoint = cylinderBottom  + radius * _direction;

            return supportPoint;
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
        public float SDF(float3 p)
        {
            var a = cylinderTop;
            var b = cylinderBottom;
            var pa = p - a;
            var ba = b - a;
            var h = math.clamp( math.dot(pa,ba)/math.dot(ba,ba), 0.0f, 1.0f );
            return math.length( pa - ba*h ) - radius;
        }
        public bool RayIntersection(GRay _ray, out float distance)
        {
            distance = -1;
            var pa = cylinderTop;
            var pb = cylinderBottom;
            var ro = _ray.origin;
            var rd = _ray.direction;
            var ra = radius;
            var  ba = pb - pa;
            var  oa = ro - pa;
            var baba = math.dot(ba,ba);
            var bard = math.dot(ba,rd);
            var baoa = math.dot(ba,oa);
            var rdoa = math.dot(rd,oa);
            var oaoa = math.dot(oa,oa);
            var a = baba      - bard*bard;
            var b = baba*rdoa - baoa*bard;
            var c = baba*oaoa - baoa*baoa - ra*ra*baba;
            var h = b*b - a*c;
            if (!(h >= 0.0)) return false;
            var t = (-b-math.sqrt(h))/a;
            var y = baoa + t*bard;
            // body
            if (y > 0.0 && y < baba)
            {
                distance = t;
                return true;
            }
            // caps
            var oc = (y <= 0.0) ? oa : ro - pb;
            b = math.dot(rd,oc);
            c = math.dot(oc,oc) - ra*ra;
            h = b*b - c;
            if (h > 0.0)
            {
                distance = -b - math.sqrt(h);
                return true;
            }
            return false;
        }


        public void DrawGizmos() => UGizmos.DrawWireCapsule(origin,normal,radius,height);
        #if UNITY_EDITOR
        public void DrawHandles() => UnityEditor.UHandles.DrawWireCapsule(origin,normal,radius,height);
        #endif
    }
}