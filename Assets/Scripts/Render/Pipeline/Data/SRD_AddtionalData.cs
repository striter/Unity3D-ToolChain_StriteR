using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
namespace Rendering.Pipeline
{
    public class SRD_AddtionalData : MonoBehaviour
    {
        [Header("Screen Space Params")]
        public CameraOverrideOption m_FrustumCornersRay = CameraOverrideOption.UsePipelineSettings;
        public CameraOverrideOption m_CameraViewProjectionMatrix = CameraOverrideOption.UsePipelineSettings;
        [Header("External Textures")]
        public CameraOverrideOption m_OpaqueBlurTexture = CameraOverrideOption.UsePipelineSettings;
        public CameraOverrideOption m_NormalTexture = CameraOverrideOption.UsePipelineSettings;
        [MFoldout(nameof(m_CameraViewProjectionMatrix), CameraOverrideOption.On, CameraOverrideOption.UsePipelineSettings)] public CameraOverrideOption m_ReflectionTexture = CameraOverrideOption.UsePipelineSettings;
    }
}