using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Rendering.Pipeline
{
    public class CameraOverride : MonoBehaviour
    {
        public CameraOverrideOption m_Normal = CameraOverrideOption.UsePipelineSettings;
        public CameraOverrideOption m_Reflection = CameraOverrideOption.UsePipelineSettings;
    }
}