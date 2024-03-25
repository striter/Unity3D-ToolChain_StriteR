using System.Collections.Generic;
using CameraController.Inputs;
using UnityEngine;

namespace CameraController
{
    
    public interface ICameraController 
    {
        public IControllerInputProcessor InputProcessor { get; }
        void OnEnter(AControllerInput _input);
        FCameraOutput Tick(float _deltaTime,AControllerInput _input);
        void OnReset(AControllerInput _input);
        void OnExit();
        void DrawGizmos(AControllerInput _input);
    }

    public abstract class ACameraController : ScriptableObject, ICameraController
    {
        public abstract IControllerInputProcessor InputProcessor { get; }
        public abstract void OnEnter(AControllerInput _input);
        public abstract FCameraOutput Tick(float _deltaTime,AControllerInput _input);
        public abstract void OnReset(AControllerInput _input);
        public abstract void OnExit();
        public abstract void DrawGizmos(AControllerInput _input);
    }
}