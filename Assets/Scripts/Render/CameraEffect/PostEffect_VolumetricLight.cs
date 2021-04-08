using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rendering.ImageEffect
{
    public class PostEffect_VolumetricLight : PostEffectBase<CameraEffect_VolumetricLight, CameraEffectParam_VolumetricLight>
    {
        public override bool m_IsOpaqueProcess => true;
    }
    public enum enum_LightMarchTimes
    {
        _8=8,
        _16=16,
        _32=32,
        _64=64,
        _128=128,
    }
    [Serializable]
    public struct CameraEffectParam_VolumetricLight
    {
        [Clamp(0f)] public float m_Distance;
        public enum_LightMarchTimes m_MarchTimes;
        [Range(0, 2f)] public float m_Strength;
        [Range(0.1f,5f)]public float m_Pow;
        [RangeInt(1, 4)] public int m_DownSample;
        [MTitle] public bool m_EnableBlur;
        [MFoldout(nameof(m_EnableBlur), true)] public ImageEffectParam_Blurs m_BlurParam;
        public static readonly CameraEffectParam_VolumetricLight m_Default = new CameraEffectParam_VolumetricLight()
        {
            m_Pow = 2,
            m_Strength = .3f,
            m_Distance = 20f,
            m_MarchTimes = enum_LightMarchTimes._64,
            m_EnableBlur=true,
            m_BlurParam=ImageEffectParam_Blurs.m_Default,
        };
    }

    public class CameraEffect_VolumetricLight : ImageEffectBase<CameraEffectParam_VolumetricLight>
    {
        #region ShaderProperties
        static readonly int ID_LightPow = Shader.PropertyToID("_LightPow");
        static readonly int ID_LightStrength = Shader.PropertyToID("_LightStrength");
        static readonly int ID_MarchTimes = Shader.PropertyToID("_MarchTimes");
        static readonly int ID_MarchDistance = Shader.PropertyToID("_MarchDistance");

        static readonly int RTI
        #endregion
        protected override void OnValidate(CameraEffectParam_VolumetricLight _params, Material _material)
        {
            base.OnValidate(_params, _material);
            _material.SetInt(ID_MarchTimes, (int)_params.m_MarchTimes);
            _material.SetFloat(ID_MarchDistance, _params.m_Distance);
            _material.SetFloat(ID_LightPow, _params.m_Pow);
            _material.SetFloat(ID_LightStrength, _params.m_Strength);
        }
        protected override void OnExecuteBuffer(CommandBuffer _buffer, RenderTextureDescriptor _descriptor, RenderTargetIdentifier _src, RenderTargetIdentifier _dst, Material _material, CameraEffectParam_VolumetricLight _param)
        {
            base.OnExecuteBuffer(_buffer, _descriptor, _src, _dst, _material, _param);
        }
    }
}