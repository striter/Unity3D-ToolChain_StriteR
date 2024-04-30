using System;
using CameraController;
using CameraController.Animation;
using CameraController.Inputs;
using CameraController.Inputs.Touch;
using Dome.Entity;
using Dome.LocalPlayer;
using Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;

namespace Dome
{
    
    public class FDomeCamera : ADomeController
    {
        public FControllerInput m_Input;
        public FCameraControllerOutput m_Ouput;
        public FControllerInterpolate m_Interpolate;
        public Camera m_Camera => m_Input.camera;
        
        private FCameraControllerCore m_Controller = new FCameraControllerCore();
        public override void OnInitialized()
        {
            m_Input.camera = transform.GetComponentInChildren<Camera>();
        }

        public override void Dispose()
        {
        }
        public override void Tick(float _deltaTime)
        {
            m_Controller.Tick(_deltaTime, ref m_Input);
        }

        public GRay ScreenPointToRay(float2 _position) => m_Camera.ScreenPointToRay(_position.to3xy());
        

        public void OnEntityControlChanged(ADomePlayerControl _controller)
        {
            m_Input.anchor = _controller?.GetAnchor();
            m_Controller.Switch(_controller==null?FEmptyController.kDefault:_controller.m_CameraController,ref m_Input);
            m_Controller.AppendModifier(m_Interpolate);
        }
        
        private void OnDrawGizmos()
        {
            m_Controller.DrawGizmos(m_Input);
        }
    }
    
    [Serializable]
    public class FControllerInput : AControllerInput,IFOVOffset,IViewportOffset,IAnchorOffset,IPlayerInput
    {
        public Camera camera;
        public Transform anchor;
        public float3 anchorOffset;
        public Transform target;
        public float2 viewPort;
        public float3 euler;
        public float pinch;
        public float fovDelta;

        public override Camera Camera => camera;
        public override Transform Anchor => anchor;
        public override Transform Target => target;
        public float OffsetFOV { get => fovDelta; set => fovDelta = value; }
        public float OffsetViewPortX { get => viewPort.x; set => viewPort.x = value; }
        public float OffsetViewPortY { get => viewPort.y; set => viewPort.y = value; }
        public float3 OffsetAnchor => anchorOffset;
        public float Pitch { get=> euler.x; set=> euler.x = value; }
        public float Yaw { get=> euler.y; set=> euler.y = value; }
        public float Roll { get=> euler.z; set=> euler.z = value; }
        public float Pinch { get=> pinch; set=> pinch = value; }
    }


}