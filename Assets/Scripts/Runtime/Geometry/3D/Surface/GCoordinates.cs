﻿using System;
using Runtime.Geometry.Extension;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{
    public partial struct GCoordinates
    {
        public float3 origin;
        [PostNormalize] public float3 right;
        [PostNormalize] public float3 up;
        [NonSerialized] public float3 forward;
        public GCoordinates(float3 _origin, float3 _right, float3 _up)
        {
            this = default;
            origin = _origin;
            right = _right.normalize();
            up = _up.normalize();
            Ctor();
        }
        void Ctor()
        {
            forward = math.cross(right, up);
        }

    }

    [Serializable]
    public partial struct GCoordinates : ISerializationCallbackReceiver , IRayIntersection , ISurface
    {
        public static GCoordinates ForwardBillboard(float3 origin,float3 forward)
        {
            var billboardRotation = Quaternion.LookRotation(forward, Vector3.up);
            var U = math.mul(billboardRotation, kfloat3.up);
            var R = math.mul(billboardRotation, kfloat3.right);
            U = math.cross(R,-forward).normalize();
            R = math.cross(-forward,U).normalize();
            return new GCoordinates(origin, R, U);
        }
        
        public GCoordinates Flip()
        {
            right = -right;
            up = -up;
            forward = -forward;
            return this;
        }
        
        public quaternion GetRotation() => quaternion.LookRotation(forward,up);
        public GLine Right() => new GLine(origin, origin + right);
        public GLine Up() => new GLine(origin, origin + up);
        public GLine Forward() => new GLine(origin, origin + forward);
        public float3 GetPoint(float2 _uv) => origin + _uv.x * right + _uv.y * up;
        public float2 GetUV(float3 _point)
        {
            var v0 = right;
            var v1 = up;
            var v2 = _point - origin;
            var dot00 = math.dot(v0, v0);
            var dot01 = math.dot(v0, v1);
            var dot02 = math.dot(v0, v2);
            var dot11 = math.dot(v1, v1);
            var dot12 = math.dot(v1, v2);

            var denominator = (dot00 * dot11 - dot01 * dot01);
            var u = (dot11 * dot02 - dot01 * dot12) / denominator;
            var v = (dot00 * dot12 - dot01 * dot02) / denominator;
            return new float2(u,v);
        }
        
        public float ProjectRadClockwise(float3 _point) => umath.getRadClockwise(this.GetUV(this.Projection(_point)),kfloat2.up);
        
        public void OnBeforeSerialize(){}
        public void OnAfterDeserialize() => Ctor();

        public static GCoordinates kDefault = new GCoordinates(kfloat3.zero,kfloat3.right,kfloat3.up);
        public static  implicit operator GPlane(GCoordinates _axis) => new GPlane(_axis.forward,_axis.origin);
        public bool RayIntersection(GRay _ray, out float distance) => ((GPlane)this).RayIntersection(_ray,out distance);
        
        public void DrawGizmos()
        {
            Gizmos.color = Color.red;
            Right().DrawGizmos();
            Gizmos.color = Color.green;
            Up().DrawGizmos();
            Gizmos.color = Color.blue;
            Forward().DrawGizmos();
        }

        public float3 Origin => origin;
        public float3 Normal => up;
    }
}