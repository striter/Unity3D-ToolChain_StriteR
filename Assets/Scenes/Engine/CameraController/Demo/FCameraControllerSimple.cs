using System;
using CameraController.Inputs;
using CameraController.Inputs.Touch;
using Runtime.TouchTracker;
using Unity.Mathematics;
using UnityEngine;

namespace CameraController.Demo
{
    public class FCameraControllerSimple : MonoBehaviour
    {
        public Input m_Input;
        [field : SerializeField] public FCameraControllerCore m_Core { get; private set; }= new ();
        public void LateUpdate()
        {
            var _deltaTime = Time.unscaledDeltaTime;
            var tracks = TouchTracker.Execute(_deltaTime);
            m_Input.PlayerPinch += tracks.CombinedPinch();
            m_Input.PlayerDrag += tracks.CombinedDrag();
            m_Core.Tick(_deltaTime, ref m_Input);
        }

        private void OnDrawGizmos()
        {
            m_Core.DrawGizmos(m_Input);
        }

        [Serializable]
        public class Input : AControllerInput,IFOVOffset,IViewportOffset , IAnchorOffset , IControllerPlayerTouchInput
        {
            public Camera camera;
            public ACameraController controller;
            public Transform anchor;
            public float3 anchorOffset;
            public Transform target;
            public float2 viewPort;
            public float3 euler;
            public float pinch;
            public float fovDelta;

            public override Camera Camera => camera;
            public float2 PlayerDrag { get; set; }
            public float PlayerPinch { get; set; }
            public FPlayerInputMultiplier Sensitive { get; set; } = FPlayerInputMultiplier.kDefaultPixels;
            public override ITransformHandle Anchor => new FTransformHandleDefault(anchor);
            public override Transform Target => target;
            public override ICameraController Controller => controller;
            public float Pitch { get => euler.x; set=> euler.x = value; }
            public float Yaw { get => euler.y; set=> euler.y = value; }
            public float Pinch { get => pinch; set=> pinch = value; }
            public float3 OffsetAnchor => anchorOffset;
            public float OffsetFOV { get => fovDelta; set => fovDelta = value; }
            public float OffsetViewPortX { get => viewPort.x; set => viewPort.x = value; }
            public float OffsetViewPortY { get => viewPort.y; set => viewPort.y = value; }
        }
    }
    
    
}