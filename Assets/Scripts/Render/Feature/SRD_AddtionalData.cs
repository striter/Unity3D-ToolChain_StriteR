using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
namespace Rendering
{
    public class SRD_AddtionalData : MonoBehaviour
    {
        public CameraOverrideOption m_FrustumCornersRay= CameraOverrideOption.UsePipelineSettings;
        public CameraOverrideOption m_OpaqueBlurTexture= CameraOverrideOption.UsePipelineSettings;
    }
}