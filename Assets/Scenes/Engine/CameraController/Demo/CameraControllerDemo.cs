﻿using System;
using System.Collections.Generic;
using System.Linq.Extensions;
using CameraController.Animation;
using CameraController.Inputs;
using CameraController.Inputs.Touch;
using Runtime.TouchTracker;
using Unity.Mathematics;
using UnityEngine;


namespace CameraController.Demo
{
    [Serializable]
    public class FControllerInput : AControllerInput,IFOVOffset,IViewportOffset , IAnchorOffset , IControllerPlayerTouchInput
    {
        [Header("Controllers")]
        [Foldout(nameof(m_ScriptedControllerOverride),null),Clamp(0,nameof(m_Controllers))]public int m_ControllerIndex;
        [Foldout(nameof(m_ScriptedControllerOverride),null)] public List<ACameraController> m_Controllers;
        public ACameraController m_ScriptedControllerOverride;
        
        public Camera camera;
        public Transform anchor;
        public float3 anchorOffset;
        public Transform target;
        public float2 viewPort;
        public float3 euler;
        public float pinch;
        public float fovDelta;

        public override Camera Camera => camera;
        public float2 PlayerDrag { get; set; }
        public float PlayerPinch { get; set; }
        public FPlayerInputMultiplier Sensitive { get; set; } = FPlayerInputMultiplier.kDefaultPixels;
        public override ITransformHandle Anchor => new FTransformHandleDefault(anchor);
        public override Transform Target => target;
        public override ICameraController Controller
        {
            get
            {
                if (m_ScriptedControllerOverride != null)
                    return m_ScriptedControllerOverride;
                return m_ControllerIndex < 0 || m_ControllerIndex >= m_Controllers.Count ? null : m_Controllers[m_ControllerIndex];
            }
        }
        public float Pitch { get => euler.x; set=> euler.x = value; }
        public float Yaw { get => euler.y; set=> euler.y = value; }
        public float Pinch { get => pinch; set=> pinch = value; }
        public float3 OffsetAnchor => anchorOffset;
        public float OffsetFOV { get => fovDelta; set => fovDelta = value; }
        public float OffsetViewPortX { get => viewPort.x; set => viewPort.x = value; }
        public float OffsetViewPortY { get => viewPort.y; set => viewPort.y = value; }
    }

    
    [ExecuteInEditMode]
    public class CameraControllerDemo : MonoBehaviour
    {
        public FControllerInput m_Input;
        
        [Header("Animation")]
        public FControllerInterpolate m_Interpolate;
        public FControllerAdditionalAnimation m_AdditionalAnimation;

        public FCameraControllerCore m_Core = new FCameraControllerCore();
        private Transform m_Target;
        private Transform m_Character;

        private void OnValidate()
        {
            if (m_Input == null)
                return;
            // m_Index = 0;
            m_Target = transform.Find("Target");
            m_Input.camera = transform.GetComponentInChildren<Camera>();
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.R))
                m_Core.Reset();
            
            if (Input.GetKeyDown(KeyCode.CapsLock))
            {
                m_Input.target = m_Input.target != null ? null : m_Target;
                // m_Controller.Reset(ref m_Input);
            }

            if (Input.GetKeyDown(KeyCode.Space))
                m_Core.AppendModifier(m_AdditionalAnimation);
            
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                m_Input.m_ControllerIndex = m_Input.m_Controllers.NextIndex(m_Input.m_ControllerIndex);
                m_Core.AppendModifier(m_Interpolate);
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                m_Input.m_ControllerIndex = -1;
                m_Core.Apply(m_Input, m_Input.Camera.transform.position, m_Input.Camera.transform.rotation, m_Input.Camera.fieldOfView);
            }
        }

        void InputTick()
        {
            if (!Application.isPlaying) return;
            
            var tracks = TouchTracker.Execute(Time.unscaledDeltaTime);
            m_Input.PlayerDrag = tracks.CombinedDrag();
            m_Input.PlayerPinch = tracks.CombinedPinch();            
        }
        
        
        private void LateUpdate()
        {
            InputTick();
            m_Core.Tick(Time.deltaTime,ref m_Input);
        }
        
        public bool m_DrawGizmos = true;
        public bool m_DrawInputGizmos = true;
        public void OnDrawGizmos()
        {
            if(m_DrawGizmos)
                m_Core.DrawGizmos(m_Input);
                
            if(m_DrawInputGizmos)
                m_Input.DrawGizmos();
        }
    }
}