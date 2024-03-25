using System;
using System.Collections.Generic;
using UnityEngine;

namespace CameraController.Inputs
{

    public interface IControllerInputProcessor
    {
        public void OnEnter<T>(ref T _input) where T : AControllerInput;
        public void OnTick<T>(float _deltaTime,ref T _input) where T : AControllerInput;
        public void OnReset<T>(ref T _input) where T : AControllerInput;
        public void OnExit<T>(ref T _input) where T : AControllerInput;
    }

    public abstract class AControllerInputProcessor : ScriptableObject, IControllerInputProcessor
    {
        public abstract void OnEnter<T>(ref T _input) where T : AControllerInput;
        public abstract void OnTick<T>(float _deltaTime,ref T _input) where T : AControllerInput;
        public abstract void OnReset<T>(ref T _input) where T : AControllerInput;
        public abstract void OnExit<T>(ref T _input) where T : AControllerInput;
    }
}