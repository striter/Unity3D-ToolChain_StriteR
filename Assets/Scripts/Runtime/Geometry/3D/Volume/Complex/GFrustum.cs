using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Extensions;
using Unity.Mathematics;
using UnityEngine;
using Runtime.Geometry.Extension;

namespace Runtime.Geometry
{
    [Serializable]
    public struct GFrustum : IVolume , ISerializationCallbackReceiver
    {
        public float3 origin;
        public quaternion rotation;
        [Clamp(0)]public float fov;
        public float aspect;
        [Clamp(0)]public float zNear;
        [Clamp(0)]public float zFar;

        [NonSerialized] public GFrustumPlanes planes;
        [NonSerialized] public GFrustumRays rays;
        [NonSerialized] public GFrustumPoints points;
        public GFrustum(Camera _camera) : this(_camera.transform.position, _camera.transform.rotation, _camera.fieldOfView, _camera.aspect, _camera.nearClipPlane, _camera.farClipPlane)
        {
        }
        
        public GFrustum(float3 _origin, quaternion _rotation, float _fov, float _aspect, float _zNear, float _zFar)
        {
            this = default;
            origin = _origin;
            rotation = _rotation;
            fov = _fov;
            aspect = _aspect;
            zNear = _zNear;
            zFar = _zFar;
            Ctor();
        }

        void Ctor()
        {
            rays = new GFrustumRays(origin, rotation, fov, aspect, zNear, zFar);
            planes = new GFrustumPlanes(origin, rotation, fov, aspect, zNear, zFar);
            points = rays.GetFrustumPoints();
        }
        public void DrawGizmos() => points.DrawGizmos();
        public float3 Origin => origin;
        public float3 GetSupportPoint(float3 _direction) => points.MinElement(p => math.dot(p, _direction));
        public GBox GetBoundingBox() => UGeometry.GetBoundingBox(points);
        public GSphere GetBoundingSphere() => UGeometry.GetBoundingSphere(points);
        public void OnBeforeSerialize(){}
        public void OnAfterDeserialize() => Ctor();
    }
}