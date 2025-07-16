using System;
using CameraController;
using CameraController.Inputs;
using CameraController.Inputs.Touch;
using Unity.Mathematics;
using UnityEngine;

namespace TechToys.CharacterControl
{
    public class CharacterControl_Camera : SingletonMono<CharacterControl_Camera> , ICharacterControlMgr
    {
        [field: SerializeField] public FCameraControllerCore m_ControllerCore { get; private set; }
        [field: SerializeField] public FControllerInput m_CameraInput;
        public void Initialize()
        {
            
        }

        public void Dispose()
        {
        }

        public void Tick(float _deltaTime)
        {
            var inputMgr = CharacterControl_Input.Instance.m_Input;
            m_CameraInput.PlayerDrag = inputMgr.cameraMove;
            m_CameraInput.PlayerPinch = inputMgr.cameraZoom;
        }

        public void LateTick(float _deltaTime)
        {
            m_ControllerCore.Tick(_deltaTime, ref m_CameraInput);
        }
    }
    [Serializable]
    public class FControllerInput : AControllerInput,IFOVOffset,IViewportOffset , IAnchorOffset , IControllerPlayerTouchInput
    {
        public ACameraController controller;
        public Camera camera;
        public Transform anchor;
        public float3 anchorOffset;
        public Transform target;
        public float2 viewPort;
        public float3 euler;
        public float pinch;
        public float fovDelta;
        public override Camera Camera => camera;
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
        [field : SerializeField] public FPlayerInputMultiplier Sensitive { get; set; } = FPlayerInputMultiplier.kDefaultPixels;
        [field : SerializeField] public float2 PlayerDrag { get; set; }
        [field : SerializeField] public float PlayerPinch { get; set; }
    }
}