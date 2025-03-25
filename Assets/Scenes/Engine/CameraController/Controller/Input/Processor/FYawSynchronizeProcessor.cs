using System;
using UnityEngine;

namespace CameraController.Inputs
{
    [CreateAssetMenu(fileName = "InputProcessor", menuName = "Camera/InputProcessor/YawSynchronizer", order = 0)]
    public class FYawSynchronizeProcessor : AControllerInputProcessor
    {
        public float m_FixedPinch = 0;
        public override bool Controllable => false;
        void Init<T>(ref T _input) where T: AControllerInput
        {
            if (_input is not IPlayerInput playerInput)
                return;
            
            playerInput.Pinch = m_FixedPinch;
            playerInput.Pitch = UController.GetPitch(_input.Anchor.transform.forward);
            playerInput.Yaw = UController.GetYaw(_input.Anchor.transform.forward);
        }

        public override void OnEnter<T>(ref T _input) => Init(ref _input);
        public override void OnTick<T>(float _deltaTime, ref T _input) => Init(ref _input);
        public override void OnReset<T>(ref T _input) { }
        public override void OnExit() { }
    }
}