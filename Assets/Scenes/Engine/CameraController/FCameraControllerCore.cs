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
    public sealed class FCameraControllerCore : ISerializationCallbackReceiver     //Executor
    {
        public bool m_Initialize = true;
        public bool m_Reset = false;
        public ICameraController m_Controller { get; private set; } = FEmptyController.kDefault;
        private List<IControllerPostModifer> m_Animations = new List<IControllerPostModifer>();
        
        [Header("Debug")]
        [Readonly] public FCameraControllerOutput m_Output;
        public bool m_AnchorDebugRecording = false;
        private List<float3> m_AnchorList = new List<float3>();
        [Fold(nameof(m_SerializedController),null)] public MonoBehaviour m_SerializedController;
        [Fold(nameof(m_ScriptableController),null)] public ScriptableObject m_ScriptableController;
        public bool Switch(ICameraController _controller)
        {
            _controller ??= FEmptyController.kDefault;
            if (_controller == m_Controller)
                return false;
            
            if (!m_Initialize)
            {
                foreach (var processor in m_Controller.InputProcessor)
                    processor.OnExit();
                foreach (var modifier in m_Controller.PostModifier)
                    RemoveModifier(modifier);
                m_Controller.OnExit();
            }

            m_Controller = _controller;
            m_SerializedController = m_Controller as MonoBehaviour;
            m_ScriptableController = m_Controller as ScriptableObject;
            m_Initialize = true;
            
            return true;
        }

        public bool Tick<T>(float _deltaTime,ref T _input) where T:AControllerInput
        {
            if (!_input.Available)
            {             
                if(_input.Camera != null && _input.Camera.gameObject.activeInHierarchy)
                    m_Output.Apply(_input.Camera);
                return false;
            }

            if (m_Initialize)
            {
                foreach (var processor in m_Controller.InputProcessor)
                    processor.OnEnter(ref _input);
                foreach (var modifier in m_Controller.PostModifier)
                    AppendModifier(modifier);
            
                m_Controller.OnEnter(_input);
                m_Initialize = false;
            }
            
            if (m_Reset)
            {
                m_Reset = false;
                
                foreach (var processor in m_Controller.InputProcessor)
                    processor.OnReset(ref _input);
            
                m_Controller.OnReset(_input);
            }
            
            foreach (var processor in m_Controller.InputProcessor)
                processor.OnTick(_deltaTime,ref _input);
            
            if (!m_Controller.Tick(_deltaTime, _input, ref m_Output))
                return false;
            
            IControllerPostModifer.Tick(_deltaTime,_input,ref m_Output, ref m_Animations);
            if(m_AnchorDebugRecording)
                m_AnchorList.Add(m_Output.anchor);

            m_Output.Apply(_input.Camera);
            return true;
        }


        public void Reset()
        {
            for(var i = m_Animations.Count - 1;i>=0;i--)
                if(m_Animations[i].Disposable(true))
                    m_Animations.RemoveAt(i);

            m_Reset = true;
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
        public T GetModifier<T>() where T:class,IControllerPostModifer => m_Animations.Find(p => p is T) as T;
        public bool HasModifier(IControllerPostModifer _animation) => m_Animations.Find(p => p == _animation) != null;

        public void Apply(AControllerInput _input)
        {
            if(_input.Camera == null)
                return;

            var cameraTransform = _input.Camera.transform;
            Apply(_input, cameraTransform.position, cameraTransform.rotation, _input.Camera.fieldOfView);
        }
        public void Apply(AControllerInput _input,float3 _position, quaternion _rotation, float _fov)
        {
            var anchor = _input.Available ?  (float3)_input.Anchor.transform.position : 0;
            var forward = math.mul(_rotation , kfloat3.forward);
            var delta = anchor - _position;
            
            var forwardProjection = math.dot(delta, forward);
            var distancedAnchor = anchor - forward * forwardProjection;

            var anchorOffset = (_position - distancedAnchor);
            Apply(_input,anchor + anchorOffset,_rotation,_fov, forwardProjection,0);
        }

        public void Apply(AControllerInput _input,float3 _anchor, quaternion _rotation, float _fov, float _distance, float2 _viewPort) => m_Output = new FCameraControllerOutput() {
                anchor = _anchor,
                euler = _rotation.toEulerAngles(),
                fov = _fov,
                viewPort = _viewPort,
                distance = _distance
            };
        
        public void DrawGizmos(AControllerInput _input)
        {
            m_Controller.DrawGizmos(_input);
            m_Animations.Traversal(p=>p.DrawGizmos(_input));
            m_Output.DrawGizmos(_input.Camera);

            Gizmos.color = Color.green.SetA(.5f);
            foreach (var anchor in m_AnchorList)
                Gizmos.DrawSphere(anchor,.05f);
            Gizmos.color = Color.green;
            UGizmos.DrawLines(m_AnchorList);
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            if(m_AnchorDebugRecording)
                m_AnchorList.Clear();
            if(m_SerializedController != null && m_SerializedController is ICameraController _controllerSerialized)
                Switch(_controllerSerialized);
            if(m_ScriptableController != null && m_ScriptableController is ICameraController _controllerScriptable)
                Switch(_controllerScriptable);
        }
    }

}