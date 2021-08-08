using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rendering.ImageEffect
{

    public class PostProcess_UberAfterAll : PostProcessComponentBase<PPCore_UberAfterAll,PPData_UberAfterAllData>
    {
    }
    
    public class PPCore_UberAfterAll : PostProcessCore<PPData_UberAfterAllData>
    {
        #region ShaderProperties
        static readonly int ID_Weight = Shader.PropertyToID("_Weight");

        const string KW_LUT = "_LUT";
        static readonly int ID_LUT = Shader.PropertyToID("_LUTTex");
        readonly int ID_LUTCellCount = Shader.PropertyToID("_LUTCellCount");

        const string KW_BSC = "_BSC";
        static readonly int ID_Brightness = Shader.PropertyToID("_Brightness");
        static readonly int ID_Saturation = Shader.PropertyToID("_Saturation");
        static readonly int ID_Contrast = Shader.PropertyToID("_Contrast");

        const string KW_MixChannel = "_CHANNEL_MIXER";
        static readonly int ID_MixRed = Shader.PropertyToID("_MixRed");
        static readonly int ID_MixGreen = Shader.PropertyToID("_MixGreen");
        static readonly int ID_MixBlue = Shader.PropertyToID("_MixBlue");
        
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
        public override void OnValidate(ref PPData_UberAfterAllData _data)
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
            
            m_Material.EnableKeyword(KW_LUT, _data.m_LUT);
            m_Material.SetTexture(ID_LUT, _data.m_LUTTex);
            m_Material.SetInt(ID_LUTCellCount, (int)_data.m_LUTCellCount);

            m_Material.EnableKeyword(KW_BSC, _data.m_BSC);
            m_Material.SetFloat(ID_Brightness, _data.m_brightness);
            m_Material.SetFloat(ID_Saturation, _data.m_saturation);
            m_Material.SetFloat(ID_Contrast, _data.m_contrast);

            m_Material.EnableKeyword(KW_MixChannel, _data.m_ChannelMixing);
            m_Material.SetVector(ID_MixRed, _data.m_MixRed+Vector3.right);
            m_Material.SetVector(ID_MixGreen, _data.m_MixGreen+Vector3.up);
            m_Material.SetVector(ID_MixBlue, _data.m_MixBlue+Vector3.forward);

        }
    }
    [Serializable]
    public struct PPData_UberAfterAllData
    {
        [Header("Color grading")] 
        [MTitle] public bool m_LUT;
        [MFoldout(nameof(m_LUT), true)] public Texture2D m_LUTTex ;
        [MFoldout(nameof(m_LUT), true)] public ELUTCellCount m_LUTCellCount ;

        [MTitle] public bool m_BSC;
        [MFoldout(nameof(m_BSC), true)] [Range(0, 2)]public float m_brightness ;
        [MFoldout(nameof(m_BSC), true)] [Range(0, 2)] public float m_saturation ;
        [MFoldout(nameof(m_BSC), true)] [Range(0, 2)]public float m_contrast ;

        [MTitle] public bool m_ChannelMixing;
        [MFoldout(nameof(m_ChannelMixing), true)] [RangeVector(-1, 1)] public Vector3 m_MixRed;
        [MFoldout(nameof(m_ChannelMixing), true)] [RangeVector(-1, 1)] public Vector3 m_MixGreen;
        [MFoldout(nameof(m_ChannelMixing), true)] [RangeVector(-1, 1)] public Vector3 m_MixBlue;

        [Header("VHS")]
        [MTitle] public EVHSScreenCut m_ScreenCut;
        [MFold(nameof(m_ScreenCut), EVHSScreenCut.None), RangeVector(0, 1)] public Vector2 m_ScreenCutDistance;

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

        [MTitle] public bool m_ColorBleed;
        [MFoldout(nameof(m_ColorBleed), true)] [Range(0, 5)] public float m_ColorBleedStrength;
        [MFoldout(nameof(m_ColorBleed), true)][ Range(1, 4)] public int m_ColorBleedIteration;
        [MFoldout(nameof(m_ColorBleed), true)][ Range(0, 2)] public float m_ColorBleedSize;
        [MFoldout(nameof(m_ColorBleed), true)][ RangeVector(-5, 5)] public Vector2 m_ColorBleedR;
        [MFoldout(nameof(m_ColorBleed), true)][ RangeVector(-5, 5)] public Vector2 m_ColorBleedG;
        [MFoldout(nameof(m_ColorBleed), true)][ RangeVector(-5, 5)] public Vector2 m_ColorBleedB;

        [MTitle] public bool m_Vignette;
        [MFoldout(nameof(m_Vignette), true)] public Color m_VignetteColor;
        [MFoldout(nameof(m_Vignette), true), Range(0, 10)] public float m_VignetteValue;

        public static readonly PPData_UberAfterAllData m_Default = new PPData_UberAfterAllData()
        {
            m_ScreenCut = EVHSScreenCut.Hard,
            m_ScreenCutDistance = Vector2.one * 0.1f,

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

            m_ColorBleed = true,
            m_ColorBleedStrength = 1f,
            m_ColorBleedIteration = 2,
            m_ColorBleedSize = .8f,
            m_ColorBleedR = Vector2.one,
            m_ColorBleedG = -Vector2.one,
            m_ColorBleedB = Vector2.zero,

            m_Vignette = true,
            m_VignetteColor = Color.black,
            m_VignetteValue = 2f,
            
            m_LUT=false,
            m_LUTCellCount = ELUTCellCount._16,
            
            m_BSC = false,
            m_brightness = 1,
            m_saturation = 1,
            m_contrast = 1,
            
            m_ChannelMixing = false,
            m_MixRed = Vector3.zero,
            m_MixGreen = Vector3.zero,
            m_MixBlue = Vector3.zero,
        };
    }
}
