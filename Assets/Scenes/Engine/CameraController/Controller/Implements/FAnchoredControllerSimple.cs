using Runtime.CameraController.Inputs;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.CameraController
{
    [CreateAssetMenu(fileName = "AnchoredController", menuName = "Camera/Controller/Simple")]
    public class FAnchoredControllerSimple : AAnchoredController        //Deprecated Version of AnchoredController
    {
        [Header("Position")]
        public string m_ChildAnchorName;
        public bool m_UseTransformPosition = false;
        [Range(0,1f)] public float m_AnchorY = 0f;
        public float3 m_AnchorOffset = 0;
        
        [Header("Rotation")]
        public float m_Pitch = 0f;
        public float m_Yaw = 0f;
        
        [Header("Distance")]
        [Min(0)] public float m_CameraDistance = 0f;
        
        [Header("Screen Space Centering")] 
        [Range(20f, 60f)] public float m_FOV = 50f;
        [Range(-.5f, .5f)] public float m_ViewportX;
        [Range(-.5f, .5f)] public float m_ViewportY;
        
        [Header("Pinch")]  
        [MinMaxRange(-10f,10f)] public RangeFloat m_PinchDistanceRange = default;
        [MinMaxRange(-20f,20f)] public RangeFloat m_PinchFovRange = default;
        protected override AnchoredControllerParameters EvaluateBaseParameters(AControllerInput _input)
        {
            var parameter = new AnchoredControllerInput {
                pitch = m_Pitch,
                yaw = m_Yaw,
                fov = m_FOV + m_PinchFovRange.Evaluate(_input.InputPinch),
                distance = m_CameraDistance + m_PinchDistanceRange.Evaluate(_input.InputPinch),
                viewportX = m_ViewportX,
                viewportY = m_ViewportY,
                anchorY = m_AnchorY,
                childName = m_ChildAnchorName,
                anchorOffset = m_AnchorOffset,
                useBoundingBox = m_AnchorY > 0,
                distanceAABBAdaption = false,
                useTransformPosition = m_UseTransformPosition,
            };

            return parameter.Evaluate(_input.Anchor);
        }
    }
}