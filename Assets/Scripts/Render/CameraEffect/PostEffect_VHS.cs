using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Rendering.ImageEffect
{

    public class PostEffect_VHS : PostEffectBase<ImageEffect_VHS, ImageEffectParam_VHS>{ }
    
    public enum enum_VHSScreenCut
    {
        None=0,
        Hard=1,
        Scaled=2,
    }
    [Serializable]
    public class ImageEffectParam_VHS:ImageEffectParamBase
    {
        [Header("Screen Cut")] 
        public enum_VHSScreenCut m_ScreenCut;
        [RangeVector(0, 1)] public Vector2 m_ScreenCutDistance=Vector2.one*0.1f;
        [Header("Color Bleed")] 
        public bool m_ColorBleed = true;
        [Range(1, 4)] public int m_ColorBleedIteration = 2;
        [Range(0, 2)] public float m_ColorBleedSize = .8f;
        [RangeVector(-5, 5)] public Vector2 m_ColorBleedR = Vector2.one;
        [RangeVector(-5, 5)] public Vector2 m_ColorBleedG = -Vector2.one;
        [RangeVector(-5, 5)] public Vector2 m_ColorBleedB = Vector2.zero;
        [Header("Pixel Distort")]
        public bool m_PixelDistort = true;
        [RangeVector(0, 1)] public Vector2 m_PixelDistortScale = Vector2.one*.1f;
        [Range(0, 0.5f)] public float m_PixelDistortStrength = .1f;
        [Range(0.5f, 1f)] public float m_PixelDistortClip = .95f;
        [Range(0f, 144f)] public int m_PixelDistortFrequency = 10;

        [Header("Line Distort")]
        public bool m_LineDistort = true;
        [Range(-2f, 2f)] public float m_LineDistortSpeed = 1f;
        [Range(-.1f, .1f)] public float m_LineDistortStrength = 0.005f;
        [Range(0f, 1f)] public float m_LineDistortClip = 0.8f;
        [Range(0, 10f)] public float m_LineDistortFrequency = 0.5f;
        [Header("Grain")] 
        public bool m_Grain = true;
        public Color m_GrainColor = Color.white;
        [RangeVector(0, 1)] public Vector2 m_GrainScale = Vector2.one;
        [Range(0, 1)] public float m_GrainClip = .5f;
        [Range(0, 144)] public int m_GrainFrequency = 10;
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
        }

    }
}