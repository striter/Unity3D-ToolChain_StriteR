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
        public override bool m_IsOpaqueProcess => true;
    }

    [Serializable]
    public struct CameraEffectParam_VolumetricCloud
    {
        [MTitle] public Texture3D m_MainNoise;
        [MFold(nameof(m_MainNoise)), RangeVector(0f, 1000f)] public Vector3 m_MainNoiseScale;
        [MFold(nameof(m_MainNoise)), RangeVector(0f, 10f)] public Vector3 m_MainNoiseFlow;

        public float m_VerticalStart;
        public float m_VerticalLength;
        [Range(0f, 100f)] public float m_Density;
        [Range(0, 1)] public float m_DensityClip;
        [Range(0, 1)] public float m_DensitySmooth;
        public float m_Distance;
        public enum_VolumetricCloud_MarchTimes m_MarchTimes ;
        [Range(0, 1)] public float m_Opacity;

        [MTitle] public Texture2D m_ShapeMask;
        [MFold(nameof(m_ShapeMask)), RangeVector(0f, 1000f)] public Vector2 m_ShapeMaskScale;
        [MFold(nameof(m_ShapeMask)), RangeVector(0f, 10f)] public Vector2 m_ShapeMaskFlow;

        [Header("Light Setting")] 
        public Texture2D m_ColorRamp;
        [Range(0, 1)] public float m_LightAbsorption;
        [MTitle]public bool m_LightMarch;
        [MFoldout(nameof(m_LightMarch),true), Range(0, 1)] public float m_LightMarchClip;
        [MFoldout(nameof(m_LightMarch), true)] public enum_VolumetricCloud_LightMarchTimes m_LightMarchTimes;
        [MTitle] public bool m_LightScatter;
        [MFoldout(nameof(m_LightScatter), true), Range(.5f, 1)] public float m_ScatterRange;
        [MFoldout(nameof(m_LightScatter), true), Range(0, 1)] public float m_ScatterStrength;
        public static readonly CameraEffectParam_VolumetricCloud m_Default = new CameraEffectParam_VolumetricCloud()
        {
            m_VerticalStart = 20f,
            m_VerticalLength = 100f,
            m_MainNoise = TResources.EditorDefaultResources.Noise3D,
            m_MainNoiseScale = Vector3.one * 500f,
            m_MainNoiseFlow = Vector3.one * 0.1f,
            m_ShapeMask = TResources.EditorDefaultResources.Noise2D,
            m_ShapeMaskScale = Vector3.one * 500f,
            m_ShapeMaskFlow = Vector3.one * 0.1f,

            m_Density = 50f,
            m_DensityClip = .6f,
            m_DensitySmooth = 0.1f,
            m_Distance = 100f,
            m_MarchTimes = enum_VolumetricCloud_MarchTimes._32,
            m_Opacity = .8f,

            m_ColorRamp = TResources.EditorDefaultResources.Ramp,
            m_LightAbsorption = .2f,
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

        static readonly int ID_MainNoise = Shader.PropertyToID("_MainNoise");
        static readonly int ID_MainNoiseScale = Shader.PropertyToID("_MainNoiseScale");
        static readonly int ID_MainNoiseFlow = Shader.PropertyToID("_MainNoiseFlow");

        const string KW_ShapeMask = "_SHAPEMASK";
        static readonly int ID_ShapeMask = Shader.PropertyToID("_ShapeMask");
        static readonly int ID_ShapeScale = Shader.PropertyToID("_ShapeMaskScale");
        static readonly int ID_ShapeFlow = Shader.PropertyToID("_ShapeMaskFlow");

        static readonly int ID_LightAbsorption = Shader.PropertyToID("_LightAbsorption");
        const string KW_LightMarch = "_LIGHTMARCH";
        static readonly int ID_LightMarchTimes = Shader.PropertyToID("_LightMarchTimes");
        static readonly int ID_LightMarchMinimalDistance = Shader.PropertyToID("_LightMarchMinimalDistance");

        const string KW_LightScatter = "_LIGHTSCATTER";
        static readonly int ID_ScatterRange = Shader.PropertyToID("_ScatterRange");
        static readonly int ID_ScatterStrength = Shader.PropertyToID("_ScatterStrength");
        #endregion
        public override void OnValidate(CameraEffectParam_VolumetricCloud _data)
        {
            base.OnValidate(_data);
            m_Material.SetFloat(ID_VerticalStart, _data.m_VerticalStart);
            m_Material.SetFloat(ID_VerticalEnd, _data.m_VerticalStart+_data.m_VerticalLength);
            m_Material.SetFloat(ID_Opacity, _data.m_Opacity);
            m_Material.SetFloat(ID_Density, _data.m_Density);
            m_Material.SetFloat(ID_DensityClip, _data.m_DensityClip);
            m_Material.SetFloat(ID_DensitySmooth, _data.m_DensitySmooth/2f*_data.m_Distance);
            m_Material.SetFloat(ID_Distance, _data.m_Distance);
            m_Material.SetInt(ID_MarchTimes, (int)_data.m_MarchTimes);
            m_Material.SetTexture(ID_ColorRamp, _data.m_ColorRamp);
            m_Material.SetTexture(ID_MainNoise, _data.m_MainNoise);
            m_Material.SetVector(ID_MainNoiseScale, _data.m_MainNoiseScale);
            m_Material.SetVector(ID_MainNoiseFlow, _data.m_MainNoiseFlow);
            m_Material.EnableKeyword(KW_ShapeMask, _data.m_ShapeMask != null);
            m_Material.SetTexture(ID_ShapeMask, _data.m_ShapeMask);
            m_Material.SetVector(ID_ShapeScale, _data.m_ShapeMaskScale);
            m_Material.SetVector(ID_ShapeFlow, _data.m_ShapeMaskFlow);
            m_Material.SetFloat(ID_LightAbsorption, _data.m_LightAbsorption);
            m_Material.EnableKeyword(KW_LightMarch,_data.m_LightMarch);
            m_Material.SetInt(ID_LightMarchTimes,(int)_data.m_LightMarchTimes);
            m_Material.EnableKeyword(KW_LightScatter, _data.m_LightScatter);
            m_Material.SetFloat(ID_LightMarchMinimalDistance, _data.m_Distance* _data.m_LightMarchClip);
            m_Material.SetFloat(ID_ScatterRange, _data.m_ScatterRange);
            m_Material.SetFloat(ID_ScatterStrength, _data.m_ScatterStrength);
        }
    }
}