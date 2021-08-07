using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Rendering.ImageEffect
{

    public class PostProcess_VHS : PostProcessComponentBase<PPCore_VHS, PPData_VHS>
    {
    }
    public enum EVHSScreenCut
    {
        None=0,
        Hard=1,
        Scaled=2,
    }
    [Serializable]
    public struct PPData_VHS
    {
        [MTitle] public EVHSScreenCut m_ScreenCut;
        [MFold(nameof(m_ScreenCut), EVHSScreenCut.None), RangeVector(0, 1)] public Vector2 m_ScreenCutDistance;

        [MTitle] public bool m_ColorBleed;
        [MFoldout(nameof(m_ColorBleed), true)] [Range(0, 5)] public float m_ColorBleedStrength;
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

        public static readonly PPData_VHS m_Default = new PPData_VHS()
        {
            m_ScreenCut = EVHSScreenCut.Hard,
            m_ScreenCutDistance = Vector2.one * 0.1f,

            m_ColorBleed = true,
            m_ColorBleedStrength = 1f,
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
    public class PPCore_VHS:PostProcessCore<PPData_VHS>
    {
        #region ShaderProperties
        static readonly string[] KW_SCREENCUT = new string[2] { "_SCREENCUT_HARD", "_SCREENCUT_SCALED" };
        static readonly int ID_ScreenCutTarget = Shader.PropertyToID("_ScreenCutTarget");

        const string KW_ColorBleed = "_COLORBLEED";
        static readonly int ID_ColorBleedStrengthen = Shader.PropertyToID("_ColorBleedStrength");
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

        public override void OnValidate(ref PPData_VHS _data)
        {
            base.OnValidate(ref _data);
            m_Material.EnableKeywords(KW_SCREENCUT, (int)_data.m_ScreenCut);
            m_Material.SetVector(ID_ScreenCutTarget,(Vector2.one+ (_data.m_ScreenCut == EVHSScreenCut.Scaled?1:-1)*_data.m_ScreenCutDistance) /2f);

            m_Material.EnableKeyword(KW_ColorBleed, _data.m_ColorBleed);
            m_Material.SetInt(ID_ColorBleedIteration, _data.m_ColorBleedIteration);
            m_Material.SetFloat(ID_ColorBleedSize, _data.m_ColorBleedSize);
            m_Material.SetFloat(ID_ColorBleedStrengthen, _data.m_ColorBleedStrength);
            m_Material.SetVector(ID_ColorBleedR, _data.m_ColorBleedR);
            m_Material.SetVector(ID_ColorBleedG, _data.m_ColorBleedG);
            m_Material.SetVector(ID_ColorBleedB, _data.m_ColorBleedB);

            m_Material.EnableKeyword(KW_PixelDistort, _data.m_PixelDistort);
            m_Material.SetVector(ID_PixelDistortSize, _data.m_PixelDistortScale);
            m_Material.SetFloat(ID_PixelDistortClip, _data.m_PixelDistortClip);
            m_Material.SetFloat(ID_PixelDistortFrequency, _data.m_PixelDistortFrequency);
            m_Material.SetFloat(ID_PixelDistortStrength, _data.m_PixelDistortStrength);

            m_Material.EnableKeyword(KW_LineDistort,_data.m_LineDistort);
            m_Material.SetFloat(ID_LineDistortSpeed, _data.m_LineDistortSpeed);
            m_Material.SetFloat(ID_LineDistortClip, _data.m_LineDistortClip);
            m_Material.SetFloat(ID_LineDistortFrequency, _data.m_LineDistortFrequency);
            m_Material.SetFloat(ID_LineDistortStrength, _data.m_LineDistortStrength);

            m_Material.EnableKeyword(KW_Grain, _data.m_Grain);
            m_Material.SetVector(ID_GrainScale, _data.m_GrainScale);
            m_Material.SetColor(ID_GrainColor, _data.m_GrainColor);
            m_Material.SetFloat(ID_GrainFrequency, _data.m_GrainFrequency);
            m_Material.SetFloat(ID_GrainClip, _data.m_GrainClip);
            m_Material.EnableKeyword(KW_GrainCircle, _data.m_GrainCirlce);
            m_Material.SetFloat(ID_GrainCircleWidth, _data.m_GrainCircleWidth);

            m_Material.EnableKeyword(KW_Vignette, _data.m_Vignette);
            m_Material.SetColor(ID_VignetteColor, _data.m_VignetteColor);
            m_Material.SetFloat(ID_VignetteValue, _data.m_VignetteValue);
        }
    }
}