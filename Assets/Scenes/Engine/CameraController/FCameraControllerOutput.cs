using System;
using CameraController.Inputs;
using Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace CameraController
{
    [Serializable]
    public struct FCameraControllerOutput
    {
        public float3 anchor;
        public float3 euler;
        public float fov;
        public float distance;
        public float2 viewPort;
        public quaternion Rotation => quaternion.Euler(euler * kmath.kDeg2Rad);
        
        public void Evaluate(Camera _camera,out GFrustumRays _frustumRays, out GRay _viewportRay)
        {
            _frustumRays = new GFrustumRays(anchor,Rotation ,_camera.fieldOfView,_camera.aspect,_camera.nearClipPlane,_camera.farClipPlane);
            _viewportRay = _frustumRays.GetRay(viewPort + .5f).Inverse().Forward(_camera.nearClipPlane);
        }
        
        public void Apply(Camera _camera)
        {
            Evaluate(_camera, out var frustumRays, out var viewportRay);
            _camera.transform.SetPositionAndRotation( viewportRay.GetPoint(distance), Rotation);
            _camera.fieldOfView = fov;
        }

        public void DrawGizmos(Camera _camera)
        {
            Evaluate(_camera, out var frustumRays, out var viewportRay);
            
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(anchor,.05f);
            Gizmos.DrawRay(viewportRay);
        }
    }
}