using System.Collections.Generic;
using CameraController.Inputs;
using UnityEngine;

namespace CameraController
{
    
    public interface ICameraController 
    {
        public IEnumerable<IControllerInputProcessor> InputProcessor { get; }
        void OnEnter(AControllerInput _input);
        bool Tick(float _deltaTime,AControllerInput _input,ref FCameraControllerOutput _output);
        void OnReset(AControllerInput _input);
        void OnExit();
        void DrawGizmos(AControllerInput _input);
    }

    public abstract class ACameraController : ScriptableObject, ICameraController
    {
        public abstract IEnumerable<IControllerInputProcessor> InputProcessor { get; }
        public abstract void OnEnter(AControllerInput _input);
        public abstract bool Tick(float _deltaTime,AControllerInput _input,ref FCameraControllerOutput _output);
        public abstract void OnReset(AControllerInput _input);
        public abstract void OnExit();
        public abstract void DrawGizmos(AControllerInput _input);
    }
}