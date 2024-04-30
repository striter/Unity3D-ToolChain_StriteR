using System;
using UnityEngine;

namespace CameraController.Inputs
{
    [CreateAssetMenu(fileName = "InputProcessor", menuName = "Camera/InputProcessor/YawSynchronizer", order = 0)]
    public class FYawSynchronizeProcessor : AControllerInputProcessor
    {
        public override bool Controllable => false;

        public override void OnEnter<T>(ref T _input)
        {
        }

        public override void OnTick<T>(float _deltaTime, ref T _input)
        {
            if (_input is not IPlayerInput playerInput)
                return;
            
            playerInput.Yaw = UController.GetYaw(_input.Anchor.forward);   
        }

        public override void OnReset<T>(ref T _input)
        {
        }

        public override void OnExit<T>(ref T _input)
        {
        }
    }
}