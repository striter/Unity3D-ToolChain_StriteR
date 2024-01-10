using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Runtime.Geometry.Validation;

namespace Runtime.Geometry
{
    [Serializable]
    public struct GFrustum
    {
        public float3 origin;
        public quaternion rotation;
        [Clamp(0)]public float fov;
        public float aspect;
        [Clamp(0)]public float zNear;
        [Clamp(0)]public float zFar;
        public GFrustum(Camera _camera)
        {
            origin = _camera.transform.position;
            rotation = _camera.transform.rotation;
            fov = _camera.fieldOfView;
            aspect = _camera.aspect;
            zNear = _camera.nearClipPlane;
            zFar = _camera.farClipPlane;
        }
        
        public GFrustum(float3 _origin, quaternion _rotation, float _fov, float _aspect, float _zNear, float _zFar)
        {
            origin = _origin;
            rotation = _rotation;
            fov = _fov;
            aspect = _aspect;
            zNear = _zNear;
            zFar = _zFar;
        }
        public GFrustumPlanes GetFrustumPlanes() => new GFrustumPlanes(origin,rotation ,fov,aspect,zNear,zFar);
        public GFrustumRays GetFrustumRays() => new GFrustumRays(origin,rotation ,fov,aspect,zNear,zFar);
    }
}