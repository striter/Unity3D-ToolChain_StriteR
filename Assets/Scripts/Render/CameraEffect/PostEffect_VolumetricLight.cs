using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        [Range(0, .5f)] public float m_Strength;
        [Range(0.1f,20f)]public float m_Pow;
        public static readonly CameraEffectParam_VolumetricLight m_Default = new CameraEffectParam_VolumetricLight()
        {
            m_Pow = 2,
            m_Strength = .3f,
            m_Distance = 20f,
            m_MarchTimes = enum_LightMarchTimes._64,
        };
    }

    public class CameraEffect_VolumetricLight : ImageEffectBase<CameraEffectParam_VolumetricLight>
    {
        #region ShaderProperties
        static readonly int ID_LightPow = Shader.PropertyToID("_LightPow");
        static readonly int ID_LightStrength = Shader.PropertyToID("_LightStrength");
        static readonly int ID_MarchTimes = Shader.PropertyToID("_MarchTimes");
        static readonly int ID_MarchDistance = Shader.PropertyToID("_MarchDistance");
        #endregion
        protected override void OnValidate(CameraEffectParam_VolumetricLight _params, Material _material)
        {
            base.OnValidate(_params, _material);
            _material.SetInt(ID_MarchTimes, (int)_params.m_MarchTimes);
            _material.SetFloat(ID_MarchDistance, _params.m_Distance);
            _material.SetFloat(ID_LightPow, _params.m_Pow);
            _material.SetFloat(ID_LightStrength, _params.m_Strength);
        }
    }
}