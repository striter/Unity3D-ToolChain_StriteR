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
        public float2 viewport;
        public quaternion Rotation => quaternion.Euler(euler * kmath.kDeg2Rad);
        
        public void Evaluate(Camera _camera,out GFrustumRays _frustumRays, out GRay _viewportRay)
        {
            _frustumRays = new GFrustumRays(anchor,Rotation ,fov,_camera.aspect,_camera.nearClipPlane,_camera.farClipPlane);
            _viewportRay = _frustumRays.GetRay(viewport + .5f).Inverse().Forward(_camera.nearClipPlane);
        }
        
        public void Apply(Camera _camera)
        {
            _camera.fieldOfView = fov;
            Evaluate(_camera, out var frustumRays, out var viewportRay);
            _camera.transform.SetPositionAndRotation( viewportRay.GetPoint(distance), Rotation);
        }

        public void DrawGizmos(Camera _camera)
        {
            if (_camera == null) return;
            
            Evaluate(_camera, out var frustumRays, out var viewportRay);
            
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(anchor,.05f);
            Gizmos.DrawRay(viewportRay);
        }

        public static FCameraControllerOutput operator +(FCameraControllerOutput _a, FCameraControllerOutput _b) => new() {
            anchor = _a.anchor + _b.anchor,
            euler = _a.euler + _b.euler,
            viewport = _a.viewport + _b.viewport,
            distance = _a.distance + _b.distance,
            fov = _a.fov + _b.fov,
        };
        
        public static FCameraControllerOutput operator -(FCameraControllerOutput _a, FCameraControllerOutput _b) => new() {
            anchor = _a.anchor - _b.anchor,
            euler = _a.euler - _b.euler,
            viewport = _a.viewport - _b.viewport,
            distance = _a.distance - _b.distance,
            fov = _a.fov - _b.fov,
        };
        
        public static FCameraControllerOutput operator *(FCameraControllerOutput _a, float _b) => new() {
            anchor = _a.anchor * _b,
            euler = _a.euler * _b,
            viewport = _a.viewport * _b,
            distance = _a.distance * _b,
            fov = _a.fov * _b,
        };
        
        public static FCameraControllerOutput operator /(FCameraControllerOutput _a, float _b) => new() {
            anchor = _a.anchor / _b,
            euler = _a.euler / _b,
            viewport = _a.viewport / _b,
            distance = _a.distance / _b,
            fov = _a.fov / _b,
        };
        
        public static FCameraControllerOutput FormatDelta(AControllerInput _input) => new FCameraControllerOutput()
        {
            anchor = math.mul(_input.Anchor.transform.rotation,_input.InputAnchorOffset),
            euler = _input.InputEuler,
            distance = _input.InputDistance,
            viewport = _input.InputViewPort,
            fov = _input.InputFOV,
        };
        
        public static FCameraControllerOutput Lerp( FCameraControllerOutput _a, FCameraControllerOutput _b, float _t)=>_a + (_b - _a) * _t;

    }
}