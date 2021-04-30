using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
namespace Rendering
{
    public class SRD_AddtionalData : MonoBehaviour
    {
        public CameraOverrideOption m_FrustumCornersRay= CameraOverrideOption.UsePipelineSettings;
        public CameraOverrideOption m_OpaqueBlurTexture = CameraOverrideOption.UsePipelineSettings;
        public CameraOverrideOption m_ReflectionTexture = CameraOverrideOption.UsePipelineSettings;
        [MFoldout(nameof(m_ReflectionTexture), CameraOverrideOption.On, CameraOverrideOption.UsePipelineSettings)] public Plane m_ReflectionPlane = new Plane() { m_Distance = 0, m_Normal = Vector3.up };
    }
}