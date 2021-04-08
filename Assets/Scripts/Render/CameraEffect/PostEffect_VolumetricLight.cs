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
        [Range(0.1f, 2f)] public float m_Pow;
        [Header("Optimize"),RangeInt(1, 4)] public int m_DownSample;
        public bool m_Dither;
        [MTitle] public bool m_EnableBlur;
        [MFoldout(nameof(m_EnableBlur), true)] public ImageEffectParam_Blurs m_BlurParam;
        public static readonly CameraEffectParam_VolumetricLight m_Default = new CameraEffectParam_VolumetricLight()
        {
            m_Pow = 2,
            m_Strength = .3f,
            m_Distance = 20f,
            m_MarchTimes = enum_LightMarchTimes._64,
            m_DownSample=2,
            m_Dither=false,
            m_EnableBlur=false,
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
        const string kW_DITHER = "_DITHER";

        static readonly int RT_ID_Sample = Shader.PropertyToID("_VolumetricLight_Sample");
        static RenderTargetIdentifier RT_Sample = new RenderTargetIdentifier(RT_ID_Sample);
        static readonly int RT_ID_Blur = Shader.PropertyToID("_VolumetricLight_Blur");
        static RenderTargetIdentifier RT_Blur = new RenderTargetIdentifier(RT_ID_Blur);
        #endregion
        public ImageEffect_Blurs m_Blur;
        public CameraEffect_VolumetricLight() : base()
        {
            m_Blur = new ImageEffect_Blurs();
        }
        public override void Destroy()
        {
            base.Destroy();
            m_Blur.Destroy();
        }

        protected override void OnValidate(CameraEffectParam_VolumetricLight _params, Material _material)
        {
            base.OnValidate(_params, _material);
            m_Blur.DoValidate(_params.m_BlurParam);
            _material.SetInt(ID_MarchTimes, (int)_params.m_MarchTimes);
            _material.SetFloat(ID_MarchDistance, _params.m_Distance);
            _material.SetFloat(ID_LightPow, _params.m_Pow);
            _material.SetFloat(ID_LightStrength, _params.m_Strength);
            _material.EnableKeyword(kW_DITHER, _params.m_Dither);
        }
        protected override void OnExecuteBuffer(CommandBuffer _buffer, RenderTextureDescriptor _descriptor, RenderTargetIdentifier _src, RenderTargetIdentifier _dst, Material _material, CameraEffectParam_VolumetricLight _param)
        {
            _descriptor.colorFormat = RenderTextureFormat.R8;
            int sampleWidth  = _descriptor.width / _param.m_DownSample;
            int sampleHeight = _descriptor.height / _param.m_DownSample;

            if (!_param.m_EnableBlur)
            {
                _buffer.GetTemporaryRT(RT_ID_Sample, sampleWidth,sampleHeight,0,FilterMode.Bilinear, _descriptor.colorFormat);
                _buffer.Blit(_src, RT_ID_Sample, _material, 0);
                _buffer.ReleaseTemporaryRT(RT_ID_Sample);
            }
            else
            {
                _buffer.GetTemporaryRT(RT_ID_Blur, sampleWidth, sampleHeight, 0, FilterMode.Bilinear, _descriptor.colorFormat);
                _buffer.GetTemporaryRT(RT_ID_Sample, _descriptor.width,_descriptor.height,0,FilterMode.Bilinear,_descriptor.colorFormat);
                _buffer.Blit(_src, RT_ID_Blur, _material, 0);
                m_Blur.ExecuteBuffer(_buffer, _descriptor, RT_ID_Blur, RT_ID_Sample, _param.m_BlurParam);
                _buffer.ReleaseTemporaryRT(RT_ID_Blur);
                _buffer.ReleaseTemporaryRT(RT_ID_Sample);
            }

            _buffer.Blit(_src, _dst, _material, 1);
        }
    }
}