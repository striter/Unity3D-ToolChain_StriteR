using System;
using System.Collections.Generic;
using CameraController;
using CameraController.Animation;
using CameraController.Inputs;
using UnityEngine;

namespace CameraController
{
    [Serializable]
    public class CameraControllerCore      //Executor
    {
        public ICameraController m_Controller { get; private set; } = FEmptyController.kDefault;
        private List<IControllerPostModifer> m_Animations = new List<IControllerPostModifer>();
        [Readonly] public FCameraOutput m_Output;

        public bool Switch<T>(ICameraController _controller,ref T _input) where T :AControllerInput
        {
            if( !_input.Available || _controller == m_Controller)
                return false;

            m_Controller?.InputProcessor?.OnExit(ref _input);
            m_Controller?.OnExit();
            m_Controller = _controller;
            m_Controller?.InputProcessor?.OnEnter(ref _input);
            m_Controller?.OnEnter(_input);
            return true;
        }
        
        public void Tick<T>(float _deltaTime,ref T _input) where T:AControllerInput
        {
            if( !_input.Available)
                return;
            
            m_Controller?.InputProcessor?.OnTick(_deltaTime,ref _input);
            
            m_Output = m_Controller?.Tick(_deltaTime,_input)??default;
            IControllerPostModifer.Tick(_deltaTime,ref m_Output, ref m_Animations);
            m_Output.Apply(_input.Camera);
        }

        public void Reset<T>(ref T _input) where T:AControllerInput
        {
            ClearAnimations();
            if(!_input.Available)
                return;
            
            m_Controller?.InputProcessor?.OnReset(ref _input);
            m_Controller?.OnReset(_input);
        }
        
        public void Dispose()
        {
            m_Controller?.OnExit();
            m_Controller = null;
            
            m_Animations.Clear();
            m_Controller = null;
        }
        

        public void DrawGizmos(AControllerInput _input)
        {
            m_Controller?.DrawGizmos(_input);
        } 
        public void AppendModifier(IControllerPostModifer _animation,AControllerInput _input, bool _excludeSame = true) => IControllerPostModifer.Append(_input,_animation,ref m_Animations, _excludeSame);
        public void ClearAnimations() => m_Animations.Clear();
        public IEnumerable<IControllerPostModifer> GetAnimations() => m_Animations;
    }

}