using System;
using System.Collections;
using System.Collections.Generic;
using Rendering.ImageEffect;
using UnityEngine;
namespace Rendering.Pipeline
{
    [Serializable]
    public class SRD_PlanarReflectionData
    {
        public EReflectionSpace m_ReflectionType;
        [MFoldout(nameof(m_ReflectionType), EReflectionSpace.ScreenSpace)] [Range(1, 4)] public int m_Sample;

        [MFoldout(nameof(m_ReflectionType), EReflectionSpace.MirrorSpace)] [Range(0,8)]public int m_LightCount;
        [MFoldout(nameof(m_ReflectionType), EReflectionSpace.MirrorSpace)] public bool m_IncludeTransparent;
        
        [Header("Optimize")]
        [Range(1,4)] public int m_DownSample;
        public bool m_EnableBlur;
        [MFoldout(nameof(m_EnableBlur), true)] public PPData_Blurs m_BlurParam;

        public static SRD_PlanarReflectionData Default()
        {
            return new SRD_PlanarReflectionData()
            {
                m_ReflectionType = EReflectionSpace.ScreenSpace,
                m_IncludeTransparent = false,
                m_DownSample=2,
                m_LightCount=8,
                m_Sample = 1,
                m_EnableBlur = true,
                m_BlurParam = UPipeline.GetDefaultPostProcessData<PPData_Blurs>(),
            };
        }
    }
}