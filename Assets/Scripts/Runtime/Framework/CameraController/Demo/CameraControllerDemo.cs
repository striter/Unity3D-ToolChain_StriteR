using System;
using System.Collections.Generic;
using CameraController.Animation;
using CameraController.Inputs;
using TTouchTracker;
using Unity.Mathematics;
using UnityEngine;


namespace CameraController.Demo
{
    
    [Serializable]
    public class FControllerInput : AControllerInput,IFOVOffset,IViewportOffset , IControllerMobileInput
    {
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
        public override Transform Anchor => anchor;
        public override Transform Target => target;
        public override float Pitch { get => euler.x; set=> euler.x = value; }
        public override float Yaw { get => euler.y; set=> euler.y = value; }
        public override float Pinch { get => pinch; set=> pinch = value; }
        
        public override float3 AnchorOffset => anchorOffset;
        public float OffsetFOV { get => fovDelta; set => fovDelta = value; }
        public float OffsetViewPortX { get => viewPort.x; set => viewPort.x = value; }
        public float OffsetViewPortY { get => viewPort.y; set => viewPort.y = value; }
    }

    
    [ExecuteInEditMode]
    public class CameraControllerDemo : MonoBehaviour
    {
        public FControllerInput m_Input;
        
        
        [Header("Controllers")]
        public List<ACameraController> m_Controllers;
        [Header("Animation")]
        public FControllerInterpolate m_Interpolate;
        public FControllerShake m_Shake;

        private Transform m_Target;
        private CameraControllerCore m_Controller = new CameraControllerCore();
        private Transform m_Character;
        private int m_Index;

        private void OnValidate()
        {
            // m_Index = 0;
            m_Target = transform.Find("Target");
            m_Input.camera = transform.GetComponentInChildren<Camera>();
            m_Controller.Switch(m_Controllers[m_Index], ref m_Input);
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.R))
                m_Controller.Reset(ref m_Input);
            
            if (Input.GetKeyDown(KeyCode.CapsLock))
            {
                m_Input.target = m_Input.target != null ? null : m_Target;
                // m_Controller.Reset(ref m_Input);
            }

            if (Input.GetKeyDown(KeyCode.Space))
                m_Controller.AppendModifier(m_Shake,m_Input);
            
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                m_Index = (m_Index + 1) % m_Controllers.Count;
                m_Controller.Switch(m_Controllers[m_Index],ref m_Input);
                m_Controller.AppendModifier(m_Interpolate,m_Input);
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
            m_Controller.Tick(Time.deltaTime,ref m_Input);
        }
        
        public bool m_DrawGizmos = true;
        public bool m_DrawInputGizmos = true;
        public void OnDrawGizmos()
        {
            if(m_DrawGizmos)
                m_Controller.DrawGizmos(m_Input);
                
            if(m_DrawInputGizmos)
                m_Input.DrawGizmos();
        }
    }
}