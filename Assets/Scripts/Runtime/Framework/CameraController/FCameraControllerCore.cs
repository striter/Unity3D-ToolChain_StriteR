using System;
using System.Collections.Generic;
using System.Linq.Extensions;
using CameraController.Animation;
using CameraController.Inputs;
using Unity.Mathematics;
using UnityEngine;

namespace CameraController
{
    [Serializable]
    public sealed class FCameraControllerCore      //Executor
    {
        public ICameraController m_Controller { get; private set; } = FEmptyController.kDefault;
        private List<IControllerPostModifer> m_Animations = new List<IControllerPostModifer>();
        [Readonly] public FCameraControllerOutput m_Output;
        public bool Switch<T>(ICameraController _controller,ref T _input) where T :AControllerInput
        {
            if( !_input.Available || _controller == m_Controller)
                return false;
            
            if (_controller == null)
            {
                Debug.LogError("Can't switch to null controller");
                _controller = FEmptyController.kDefault;
            }
            
            foreach (var processor in m_Controller.InputProcessor)
                processor.OnExit(ref _input);
            m_Controller.OnExit();
        
            m_Controller = _controller;

            foreach (var processor in m_Controller.InputProcessor)
                processor.OnEnter(ref _input);
            
            m_Controller?.OnEnter(_input);
            return true;
        }
        
        public bool Tick<T>(float _deltaTime,ref T _input) where T:AControllerInput
        {
            if( !_input.Available)
                return false;
            
            foreach (var processor in m_Controller.InputProcessor)
                processor.OnTick(_deltaTime,ref _input);

            if (!m_Controller.Tick(_deltaTime, _input, ref m_Output))
                return false;
            
            IControllerPostModifer.Tick(_deltaTime,ref m_Output, ref m_Animations);
            m_Output.Apply(_input.Camera);
            return true;
        }

        public void Reset<T>(ref T _input) where T:AControllerInput
        {
            ClearAnimations();
            if(!_input.Available)
                return;

            foreach (var processor in m_Controller.InputProcessor)
                processor.OnReset(ref _input);
            
            m_Controller.OnReset(_input);
        }
        
        public void Dispose()
        {
            m_Controller.OnExit();
            m_Controller = null;
            
            m_Animations.Clear();
            m_Controller = null;
        }

        public void AppendModifier(IControllerPostModifer _animation, bool _excludeSame = true) => IControllerPostModifer.Append(this,_animation,ref m_Animations, _excludeSame);
        public void RemoveModifier(IControllerPostModifer _animation)=> IControllerPostModifer.Remove(_animation,ref m_Animations);
        public bool HasModifier(IControllerPostModifer _animation) => m_Animations.Find(p => p == _animation) != null;
        public void ClearAnimations() => m_Animations.Clear();

        public void Apply(AControllerInput _input)
        {
            if(_input.Camera == null)
                return;

            var cameraTransform = _input.Camera.transform;
            Apply(_input, cameraTransform.position, cameraTransform.rotation, _input.Camera.fieldOfView);
        }
        public void Apply(AControllerInput _input,float3 _position, quaternion _rotation, float _fov)
        {
            var anchor = _input.Anchor ? (float3)_input.Anchor.position : 0;
            var forward = math.mul(_rotation , kfloat3.forward);
            var delta = anchor - _position;
            
            var forwardProjection = math.dot(delta, forward);
            var distancedAnchor = anchor - forward * forwardProjection;

            var anchorOffset = (_position - distancedAnchor);
            Apply(_input,anchor + anchorOffset,_rotation,_fov, forwardProjection,0);
        }

        public void Apply(AControllerInput _input,float3 _anchor, quaternion _rotation, float _fov, float _distance, float2 _viewPort)
        {
            var output =  new FCameraControllerOutput()
            {
                anchor = _anchor,
                rotation = _rotation,
                fov = _fov,
                viewPort = _viewPort,
                distance = _distance
            };
            
            if(_input.Camera != null)
                output.Apply(_input.Camera);
            
            m_Output = output;
        }
        
        public void DrawGizmos(AControllerInput _input)
        {
            m_Controller.DrawGizmos(_input);
            m_Output.DrawGizmos(_input.Camera);
        } 
    }

}