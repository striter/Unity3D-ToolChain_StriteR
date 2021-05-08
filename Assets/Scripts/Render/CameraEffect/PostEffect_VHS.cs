using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Rendering.ImageEffect
{

    public class PostEffect_VHS : PostEffectBase<ImageEffect_VHS, ImageEffectParam_VHS>
    {
    }
    public enum enum_VHSScreenCut
    {
        None=0,
        Hard=1,
        Scaled=2,
    }
    [Serializable]
    public struct ImageEffectParam_VHS
    {
        [MTitle] public enum_VHSScreenCut m_ScreenCut;
        [MFold(nameof(m_ScreenCut), enum_VHSScreenCut.None), RangeVector(0, 1)] public Vector2 m_ScreenCutDistance;

        [MTitle] public bool m_ColorBleed;
        [MFoldout(nameof(m_ColorBleed), true)][ Range(1, 4)] public int m_ColorBleedIteration;
        [MFoldout(nameof(m_ColorBleed), true)][ Range(0, 2)] public float m_ColorBleedSize;
        [MFoldout(nameof(m_ColorBleed), true)][ RangeVector(-5, 5)] public Vector2 m_ColorBleedR;
        [MFoldout(nameof(m_ColorBleed), true)][ RangeVector(-5, 5)] public Vector2 m_ColorBleedG;
        [MFoldout(nameof(m_ColorBleed), true)][ RangeVector(-5, 5)] public Vector2 m_ColorBleedB;

        [MTitle] public bool m_PixelDistort;
        [MFoldout(nameof(m_PixelDistort), true)][ RangeVector(0, 1)] public Vector2 m_PixelDistortScale;
        [MFoldout(nameof(m_PixelDistort), true)][ Range(0, 0.5f)] public float m_PixelDistortStrength;
        [MFoldout(nameof(m_PixelDistort), true)][ Range(0.5f, 1f)] public float m_PixelDistortClip;
        [MFoldout(nameof(m_PixelDistort), true)][ Range(0f, 144f)] public int m_PixelDistortFrequency;

        [MTitle] public bool m_LineDistort;
        [MFoldout(nameof(m_LineDistort), true)][ Range(-2f, 2f)] public float m_LineDistortSpeed;
        [MFoldout(nameof(m_LineDistort), true)][ Range(-.1f, .1f)] public float m_LineDistortStrength;
        [MFoldout(nameof(m_LineDistort), true)][ Range(0f, 1f)] public float m_LineDistortClip;
        [MFoldout(nameof(m_LineDistort), true)][ Range(0, 10f)] public float m_LineDistortFrequency;

        [MTitle] public bool m_Grain;
        [MFoldout(nameof(m_Grain), true)] public Color m_GrainColor;
        [MFoldout(nameof(m_Grain), true)] [RangeVector(0, 1)] public Vector2 m_GrainScale;
        [MFoldout(nameof(m_Grain), true)] [Range(0, 1)] public float m_GrainClip;
        [MFoldout(nameof(m_Grain), true)] [Range(0, 144)] public int m_GrainFrequency;
        [MFoldout(nameof(m_Grain), true)] public bool m_GrainCirlce;
        [MFoldout(nameof(m_Grain), true, nameof(m_GrainCirlce), true)] [Range(0, .5f)] public float m_GrainCircleWidth;

        [MTitle] public bool m_Vignette;
        [MFoldout(nameof(m_Vignette), true)] public Color m_VignetteColor;
        [MFoldout(nameof(m_Vignette), true), Range(0, 10)] public float m_VignetteValue;

        public static readonly ImageEffectParam_VHS m_Default = new ImageEffectParam_VHS()
        {
            m_ScreenCut = enum_VHSScreenCut.Hard,
            m_ScreenCutDistance = Vector2.one * 0.1f,

            m_ColorBleed = true,
            m_ColorBleedIteration = 2,
            m_ColorBleedSize = .8f,
            m_ColorBleedR = Vector2.one,
            m_ColorBleedG = -Vector2.one,
            m_ColorBleedB = Vector2.zero,

            m_PixelDistort = true,
            m_PixelDistortScale = Vector2.one * .1f,
            m_PixelDistortStrength = .1f,
            m_PixelDistortClip = .95f,
            m_PixelDistortFrequency = 10,

            m_LineDistort = true,
            m_LineDistortSpeed = 1f,
            m_LineDistortStrength = 0.005f,
            m_LineDistortClip = 0.8f,

            m_LineDistortFrequency = 0.5f,
            m_Grain = true,
            m_GrainColor = new Color(.5f, .5f, .5f, .5f),
            m_GrainScale = Vector2.one,
            m_GrainClip = .5f,
            m_GrainFrequency = 10,
            m_GrainCirlce=true,
            m_GrainCircleWidth=.3f,

            m_Vignette = true,
            m_VignetteColor = Color.black,
            m_VignetteValue = 2f,
        };
    }
    public class ImageEffect_VHS:ImageEffectBase<ImageEffectParam_VHS>
    {
        #region ShaderProperties
        static readonly string[] KW_SCREENCUT = new string[2] { "_SCREENCUT_HARD", "_SCREENCUT_SCALED" };
        static readonly int ID_ScreenCutTarget = Shader.PropertyToID("_ScreenCutTarget");

        const string KW_ColorBleed = "_COLORBLEED";
        const string KW_ColorBleedR = "_COLORBLEED_R";
        const string KW_ColorBleedG = "_COLORBLEED_G";
        const string KW_ColorBleedB = "_COLORBLEED_B";
        static readonly int ID_ColorBleedIteration = Shader.PropertyToID("_ColorBleedIteration");
        static readonly int ID_ColorBleedSize = Shader.PropertyToID("_ColorBleedSize");
        static readonly int ID_ColorBleedR = Shader.PropertyToID("_ColorBleedR");
        static readonly int ID_ColorBleedG = Shader.PropertyToID("_ColorBleedG");
        static readonly int ID_ColorBleedB = Shader.PropertyToID("_ColorBleedB");

        const string KW_PixelDistort = "_PIXELDISTORT";
        static readonly int ID_PixelDistortSize = Shader.PropertyToID("_PixelDistortScale");
        static readonly int ID_PixelDistortFrequency = Shader.PropertyToID("_PixelDistortFrequency");
        static readonly int ID_PixelDistortClip = Shader.PropertyToID("_PixelDistortClip");
        static readonly int ID_PixelDistortStrength = Shader.PropertyToID("_PixelDistortStrength");

        const string KW_LineDistort="_LINEDISTORT";
        static readonly int ID_LineDistortSpeed = Shader.PropertyToID( "_LineDistortSpeed");
        static readonly int ID_LineDistortStrength = Shader.PropertyToID("_LineDistortStrength");
        static readonly int ID_LineDistortClip = Shader.PropertyToID("_LineDistortClip");
        static readonly int ID_LineDistortFrequency = Shader.PropertyToID("_LineDistortFrequency");

        const string KW_Grain = "_GRAIN";
        static readonly int ID_GrainScale = Shader.PropertyToID("_GrainScale");
        static readonly int ID_GrainColor = Shader.PropertyToID("_GrainColor");
        static readonly int ID_GrainClip = Shader.PropertyToID("_GrainClip");
        static readonly int ID_GrainFrequency = Shader.PropertyToID("_GrainFrequency");
        const string KW_GrainCircle = "_GRAIN_CIRCLE";
        static readonly int ID_GrainCircleWidth = Shader.PropertyToID("_GrainCircleWidth");

        const string KW_Vignette = "_VIGNETTE";
        static readonly int ID_VignetteColor = Shader.PropertyToID("_VignetteColor");
        static readonly int ID_VignetteValue = Shader.PropertyToID("_VignetteValue");
#endregion

        protected override void OnValidate(ImageEffectParam_VHS _params, Material _material)
        {
            base.OnValidate(_params, _material);
            _material.EnableKeywords(KW_SCREENCUT, (int)_params.m_ScreenCut);
            _material.SetVector(ID_ScreenCutTarget,(Vector2.one+ (_params.m_ScreenCut == enum_VHSScreenCut.Scaled?1:-1)*_params.m_ScreenCutDistance) /2f);

            _material.EnableKeyword(KW_ColorBleed, _params.m_ColorBleed);
            _material.SetInt(ID_ColorBleedIteration, _params.m_ColorBleedIteration);
            _material.SetFloat(ID_ColorBleedSize, _params.m_ColorBleedSize);
            _material.EnableKeyword(KW_ColorBleedR, _params.m_ColorBleedR!=Vector2.zero);
            _material.EnableKeyword(KW_ColorBleedG, _params.m_ColorBleedG != Vector2.zero);
            _material.EnableKeyword(KW_ColorBleedB, _params.m_ColorBleedB != Vector2.zero);
            _material.SetVector(ID_ColorBleedR, _params.m_ColorBleedR);
            _material.SetVector(ID_ColorBleedG, _params.m_ColorBleedG);
            _material.SetVector(ID_ColorBleedB, _params.m_ColorBleedB);

            _material.EnableKeyword(KW_PixelDistort, _params.m_PixelDistort);
            _material.SetVector(ID_PixelDistortSize, _params.m_PixelDistortScale);
            _material.SetFloat(ID_PixelDistortClip, _params.m_PixelDistortClip);
            _material.SetFloat(ID_PixelDistortFrequency, _params.m_PixelDistortFrequency);
            _material.SetFloat(ID_PixelDistortStrength, _params.m_PixelDistortStrength);

            _material.EnableKeyword(KW_LineDistort,_params.m_LineDistort);
            _material.SetFloat(ID_LineDistortSpeed, _params.m_LineDistortSpeed);
            _material.SetFloat(ID_LineDistortClip, _params.m_LineDistortClip);
            _material.SetFloat(ID_LineDistortFrequency, _params.m_LineDistortFrequency);
            _material.SetFloat(ID_LineDistortStrength, _params.m_LineDistortStrength);

            _material.EnableKeyword(KW_Grain, _params.m_Grain);
            _material.SetVector(ID_GrainScale, _params.m_GrainScale);
            _material.SetColor(ID_GrainColor, _params.m_GrainColor);
            _material.SetFloat(ID_GrainFrequency, _params.m_GrainFrequency);
            _material.SetFloat(ID_GrainClip, _params.m_GrainClip);
            _material.EnableKeyword(KW_GrainCircle, _params.m_GrainCirlce);
            _material.SetFloat(ID_GrainCircleWidth, _params.m_GrainCircleWidth);

            _material.EnableKeyword(KW_Vignette, _params.m_Vignette);
            _material.SetColor(ID_VignetteColor, _params.m_VignetteColor);
            _material.SetFloat(ID_VignetteValue, _params.m_VignetteValue);
        }
    }
}