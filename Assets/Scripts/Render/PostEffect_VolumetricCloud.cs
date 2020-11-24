using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rendering.ImageEffect
{
    public enum enum_VolumetricCloud_MarchTimes { _16 = 16, _32 = 32, _64 = 64, _128 = 128 }
    public enum enum_VolumetricCloud_LightMarchTimes { _4 = 4, _8 = 8, _16 = 16 }
    public class PostEffect_VolumetricCloud : PostEffectBase<CameraEffect_VolumetricCloud>
    {
        public CameraEffectParam_VolumetricCloud m_Param;
        protected override CameraEffect_VolumetricCloud OnGenerateRequiredImageEffects() => new CameraEffect_VolumetricCloud(()=>m_Param);
    }

    [System.Serializable]
    public class CameraEffectParam_VolumetricCloud : ImageEffectParamBase
    {
        [Header("Cloud Setting")]
        public float m_VerticalStart = 20f;
        public float m_VerticalLength = 100f;
        public Texture3D m_Noise;
        public Vector3 m_NoiseScale;
        public Vector3 m_NoiseFlow;
        [Range(0, 100)] public float m_Density = 50f;
        [Range(0, 1)] public float m_DensityClip = .6f;
        [Range(0, 1)] public float m_DensitySmooth = 0.1f;
        public float m_Distance = 100f;
        public enum_VolumetricCloud_MarchTimes m_MarchTimes = enum_VolumetricCloud_MarchTimes._32;
        [Range(0, 1)] public float m_Opacity = .8f;

        [Header("Light Setting")]
        public Texture2D m_ColorRamp;
        [Range(0, 1)] public float m_LightAbsorption = 1f;
        public bool m_LightMarch = true;
        [Range(0, 1)] public float m_LightMarchClip = 0.1f;
        public enum_VolumetricCloud_LightMarchTimes m_LightMarchTimes = enum_VolumetricCloud_LightMarchTimes._4;

        [Header("Scatter Setting")]
        public bool m_LightScatter = true;
        [Range(.5f, 1)] public float m_ScatterRange = .8f;
        [Range(0, 1)] public float m_ScatterStrength = .8f;
    }

    public class CameraEffect_VolumetricCloud : ImageEffectBase<CameraEffectParam_VolumetricCloud>
    {
        #region ShaderProperties
        static readonly int ID_VerticalStart = Shader.PropertyToID("_VerticalStart");
        static readonly int ID_VerticalEnd = Shader.PropertyToID("_VerticalEnd");

        static readonly int ID_Opacity = Shader.PropertyToID("_Opacity");
        static readonly int ID_Density = Shader.PropertyToID("_Density");
        static readonly int ID_DensityClip = Shader.PropertyToID("_DensityClip");
        static readonly int ID_DensitySmooth = Shader.PropertyToID("_DensitySmooth");
        static readonly int ID_Distance = Shader.PropertyToID("_Distance");
        static readonly int ID_MarchTimes = Shader.PropertyToID("_RayMarchTimes");
        static readonly int ID_ColorRamp = Shader.PropertyToID("_ColorRamp");

        static readonly int ID_Noise = Shader.PropertyToID("_Noise");
        static readonly int ID_NoiseScale = Shader.PropertyToID("_NoiseScale");
        static readonly int ID_NoiseFlow = Shader.PropertyToID("_NoiseFlow");

        static readonly int ID_LightAbsorption = Shader.PropertyToID("_LightAbsorption");
        const string KW_LightMarch = "_LIGHTMARCH";
        static readonly int ID_LightMarchTimes = Shader.PropertyToID("_LightMarchTimes");
        static readonly int ID_LightMarchMinimalDistance = Shader.PropertyToID("_LightMarchMinimalDistance");

        const string KW_LightScatter = "_LIGHTSCATTER";
        static readonly int ID_ScatterRange = Shader.PropertyToID("_ScatterRange");
        static readonly int ID_ScatterStrength = Shader.PropertyToID("_ScatterStrength");
        #endregion
        public CameraEffect_VolumetricCloud(Func<CameraEffectParam_VolumetricCloud> _GetParam) : base(_GetParam) { }
        protected override void OnValidate(CameraEffectParam_VolumetricCloud _params, Material _material)
        {
            base.OnValidate(_params, _material);
            _material.SetFloat(ID_VerticalStart, _params.m_VerticalStart);
            _material.SetFloat(ID_VerticalEnd, _params.m_VerticalStart+_params.m_VerticalLength);
            _material.SetFloat(ID_Opacity, _params.m_Opacity);
            _material.SetFloat(ID_Density, _params.m_Density);
            _material.SetFloat(ID_DensityClip, _params.m_DensityClip);
            _material.SetFloat(ID_DensitySmooth, _params.m_DensitySmooth/2f*_params.m_Distance);
            _material.SetFloat(ID_Distance, _params.m_Distance);
            _material.SetInt(ID_MarchTimes, (int)_params.m_MarchTimes);
            _material.SetTexture(ID_ColorRamp, _params.m_ColorRamp);
            _material.SetTexture(ID_Noise, _params.m_Noise);
            _material.SetVector(ID_NoiseScale, _params.m_NoiseScale);
            _material.SetVector(ID_NoiseFlow, _params.m_NoiseFlow);
            _material.SetFloat(ID_LightAbsorption, _params.m_LightAbsorption);
            _material.EnableKeyword(KW_LightMarch,_params.m_LightMarch);
            _material.SetInt(ID_LightMarchTimes,(int)_params.m_LightMarchTimes);
            _material.EnableKeyword(KW_LightScatter, _params.m_LightScatter);
            _material.SetFloat(ID_LightMarchMinimalDistance, _params.m_Distance* _params.m_LightMarchClip);
            _material.SetFloat(ID_ScatterRange, _params.m_ScatterRange);
            _material.SetFloat(ID_ScatterStrength, _params.m_ScatterStrength);
        }
    }
}