using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
namespace Rendering.Pipeline
{
    public class SRD_AdditionalData : MonoBehaviour
    {
        [Header("Screen Space Params")]
        public CameraOverrideOption m_FrustumCornersRay = CameraOverrideOption.UsePipelineSettings;
        [Header("External Textures")]
        public CameraOverrideOption m_OpaqueBlurTexture = CameraOverrideOption.UsePipelineSettings;
        public CameraOverrideOption m_NormalTexture = CameraOverrideOption.UsePipelineSettings;
    }
}