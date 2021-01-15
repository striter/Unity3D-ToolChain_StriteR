using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rendering.ImageEffect
{
    public enum enum_VolumetricCloud_MarchTimes { _16 = 16, _32 = 32, _64 = 64, _128 = 128 }
    public enum enum_VolumetricCloud_LightMarchTimes { _4 = 4, _8 = 8, _16 = 16 }
    public class PostEffect_VolumetricCloud : PostEffectBase<CameraEffect_VolumetricCloud, CameraEffectParam_VolumetricCloud>
    {
        [ImageEffectOpaque]
        protected new void OnRenderImage(RenderTexture source, RenderTexture destination) => base.OnRenderImage(source, destination);
    }

    [Serializable]
    public struct CameraEffectParam_VolumetricCloud 
    {
        [Header("Cloud Setting")]
        public float m_VerticalStart ;
        public float m_VerticalLength;
        public Texture3D m_Noise;
        [RangeVector(0f,1000f)]public Vector3 m_NoiseScale;
        [RangeVector(0.01f,10f)]public Vector3 m_NoiseFlow;
        [Range(0, 100)] public float m_Density;
        [Range(0, 1)] public float m_DensityClip;
        [Range(0, 1)] public float m_DensitySmooth;
        public float m_Distance;
        public enum_VolumetricCloud_MarchTimes m_MarchTimes ;
        [Range(0, 1)] public float m_Opacity;

        [Header("Light Setting")]
        public Texture2D m_ColorRamp;
        [Range(0, 1)] public float m_LightAbsorption;
        public bool m_LightMarch;
        [Range(0, 1)] public float m_LightMarchClip;
        public enum_VolumetricCloud_LightMarchTimes m_LightMarchTimes;

        [Header("Scatter Setting")]
        public bool m_LightScatter;
        [Range(.5f, 1)] public float m_ScatterRange ;
        [Range(0, 1)] public float m_ScatterStrength;
        public static readonly CameraEffectParam_VolumetricCloud m_Default = new CameraEffectParam_VolumetricCloud()
        {
              m_VerticalStart = 20f,
            m_VerticalLength = 100f,
            m_NoiseScale = Vector3.one * 100f,
            m_NoiseFlow = Vector3.one * 0.1f,
            m_Density = 50f,
            m_DensityClip = .6f,
            m_DensitySmooth = 0.1f,
            m_Distance = 100f,
            m_MarchTimes = enum_VolumetricCloud_MarchTimes._32,
            m_Opacity = .8f,
            
            m_LightAbsorption = 1f,
            m_LightMarch = true,
            m_LightMarchClip = 0.1f,
            m_LightMarchTimes = enum_VolumetricCloud_LightMarchTimes._4,

            m_LightScatter = true,
            m_ScatterRange = .8f,
            m_ScatterStrength = .8f,
        };
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