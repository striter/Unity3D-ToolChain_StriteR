using UnityEngine;

namespace Runtime.CameraController.Inputs
{
    public interface IControllerInputProcessor
    {
        public bool Controllable { get; }
        public void OnEnter<T>(ref T _input) where T : AControllerInput;
        public void OnTick<T>(float _deltaTime,ref T _input) where T : AControllerInput;
        public void OnReset<T>(ref T _input) where T : AControllerInput;
        public void OnExit();
    }

    public abstract class AControllerInputProcessor : ScriptableObject, IControllerInputProcessor
    {
        public abstract bool Controllable { get; }
        public abstract void OnEnter<T>(ref T _input) where T : AControllerInput;
        public abstract void OnTick<T>(float _deltaTime,ref T _input) where T : AControllerInput;
        public abstract void OnReset<T>(ref T _input) where T : AControllerInput;
        public abstract void OnExit();
    }
}