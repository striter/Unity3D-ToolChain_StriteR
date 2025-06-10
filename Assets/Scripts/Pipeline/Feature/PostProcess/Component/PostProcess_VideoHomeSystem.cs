using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Rendering.PostProcess
{

    public class PostProcess_VideoHomeSystem : APostProcessBehaviour<FVideoHomeSystemCore, DVideoHomeSystem>
    {
        public override bool OpaqueProcess => false;
        public override EPostProcess Event => EPostProcess.VideoHomeSystem;
    }
    public enum EVHSScreenCut
    {
        None=0,
        _SCREENCUT_HARD=1,
        _SCREENCUT_SCALED=2,
    }
    [Serializable]
    public struct DVideoHomeSystem:IPostProcessParameter
    {
        [Header("UVs")]
        [Title] public EVHSScreenCut m_ScreenCut;
        [Fold(nameof(m_ScreenCut), EVHSScreenCut.None), RangeVector(0, 1)] public Vector2 screenCutDistance;

        [Title] public bool pixelDistort;
        [Foldout(nameof(pixelDistort), true)][ RangeVector(0, 1)] public Vector2 pixelDistortScale;
        [Foldout(nameof(pixelDistort), true)][ Range(0, 0.5f)] public float pixelDistortStrength;
        [Foldout(nameof(pixelDistort), true)][ Range(0.5f, 1f)] public float pixelDistortClip;
        [Foldout(nameof(pixelDistort), true)][ Range(0f, 144f)] public int pixelDistortFrequency;

        [Title] public bool lineDistort;
        [Foldout(nameof(lineDistort), true)][ Range(-2f, 2f)] public float lineDistortSpeed;
        [Foldout(nameof(lineDistort), true)][ Range(-.1f, .1f)] public float lineDistortStrength;
        [Foldout(nameof(lineDistort), true)][ Range(0f, 1f)] public float lineDistortClip;
        [Foldout(nameof(lineDistort), true)][ Range(0, 10f)] public float lineDistortFrequency;
        
        [Header("Colors")]
        [Title] public bool colorBleed;
        [Foldout(nameof(colorBleed), true)] [Range(0, 5)] public float colorBleedStrength;
        [Foldout(nameof(colorBleed), true)][ Range(1, 4)] public int colorBleedIteration;
        [Foldout(nameof(colorBleed), true)][ Range(0, 2)] public float colorBleedSize;
        [Foldout(nameof(colorBleed), true)][ RangeVector(-5, 5)] public Vector2 colorBleedR;
        [Foldout(nameof(colorBleed), true)][ RangeVector(-5, 5)] public Vector2 colorBleedG;
        [Foldout(nameof(colorBleed), true)][ RangeVector(-5, 5)] public Vector2 colorBleedB;

        [Title] public bool grain;
        [Foldout(nameof(grain), true)] public Color grainColor;
        [Foldout(nameof(grain), true)] [RangeVector(0, 1)] public Vector2 grainScale;
        [Foldout(nameof(grain), true)] [Range(0, 1)] public float grainClip;
        [Foldout(nameof(grain), true)] [Range(0, 144)] public int grainFrequency;
        [Foldout(nameof(grain), true)] public bool grainCircle;
        [Foldout(nameof(grain), true, nameof(grainCircle), true)] [Range(0, .5f)] public float grainCircleWidth;

        [Title] public bool vignette;
        [Foldout(nameof(vignette), true)] public Color vignetteColor;
        [Foldout(nameof(vignette), true), Range(0, 10)] public float vignetteValue;

        public bool Validate() => m_ScreenCut != EVHSScreenCut.None || pixelDistort || lineDistort ||
                                  colorBleed || grain || vignette;
        public static readonly DVideoHomeSystem kDefault = new DVideoHomeSystem()
        {
            m_ScreenCut = EVHSScreenCut._SCREENCUT_HARD,
            screenCutDistance = Vector2.one * 0.1f,

            pixelDistort = true,
            pixelDistortScale = Vector2.one * .1f,
            pixelDistortStrength = .1f,
            pixelDistortClip = .95f,
            pixelDistortFrequency = 10,

            lineDistort = true,
            lineDistortSpeed = 1f,
            lineDistortStrength = 0.005f,
            lineDistortClip = 0.8f,
            lineDistortFrequency = 0.5f,
            
            colorBleed = true,
            colorBleedStrength = 1f,
            colorBleedIteration = 2,
            colorBleedSize = .8f,
            colorBleedR = Vector2.one,
            colorBleedG = -Vector2.one,
            colorBleedB = Vector2.zero,

            grain = true,
            grainColor = new Color(.5f, .5f, .5f, .5f),
            grainScale = Vector2.one,
            grainClip = .5f,
            grainFrequency = 10,
            grainCircle=true,
            grainCircleWidth=.3f,

            vignette = true,
            vignetteColor = Color.black,
            vignetteValue = 2f,
        };

    }
    public class FVideoHomeSystemCore:PostProcessCore<DVideoHomeSystem>
    {
        #region ShaderProperties
        static readonly int ID_ScreenCutTarget = Shader.PropertyToID("_ScreenCutTarget");

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
        
        const string KW_ColorBleed = "_COLORBLEED";
        static readonly int ID_ColorBleedStrengthen = Shader.PropertyToID("_ColorBleedStrength");
        static readonly int ID_ColorBleedIteration = Shader.PropertyToID("_ColorBleedIteration");
        static readonly int ID_ColorBleedSize = Shader.PropertyToID("_ColorBleedSize");
        static readonly int ID_ColorBleedR = Shader.PropertyToID("_ColorBleedR");
        static readonly int ID_ColorBleedG = Shader.PropertyToID("_ColorBleedG");
        static readonly int ID_ColorBleedB = Shader.PropertyToID("_ColorBleedB");

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

        public override bool Validate(ref RenderingData _renderingData,ref DVideoHomeSystem _data)
        {
            if(m_Material.EnableKeywords(_data.m_ScreenCut))
                m_Material.SetVector(ID_ScreenCutTarget,(Vector2.one+ (_data.m_ScreenCut == EVHSScreenCut._SCREENCUT_SCALED?1:-1)*_data.screenCutDistance) /2f);

            if (m_Material.EnableKeyword(KW_PixelDistort, _data.pixelDistort))
            {
                m_Material.SetVector(ID_PixelDistortSize, _data.pixelDistortScale);
                m_Material.SetFloat(ID_PixelDistortClip, _data.pixelDistortClip);
                m_Material.SetFloat(ID_PixelDistortFrequency, _data.pixelDistortFrequency);
                m_Material.SetFloat(ID_PixelDistortStrength, _data.pixelDistortStrength);
            }

            if (m_Material.EnableKeyword(KW_LineDistort, _data.lineDistort))
            {
                m_Material.SetFloat(ID_LineDistortSpeed, _data.lineDistortSpeed);
                m_Material.SetFloat(ID_LineDistortClip, _data.lineDistortClip);
                m_Material.SetFloat(ID_LineDistortFrequency, _data.lineDistortFrequency);
                m_Material.SetFloat(ID_LineDistortStrength, _data.lineDistortStrength);
            }

            if (m_Material.EnableKeyword(KW_ColorBleed, _data.colorBleed))
            {
                m_Material.SetInt(ID_ColorBleedIteration, _data.colorBleedIteration);
                m_Material.SetFloat(ID_ColorBleedSize, _data.colorBleedSize);
                m_Material.SetFloat(ID_ColorBleedStrengthen, _data.colorBleedStrength);
                m_Material.SetVector(ID_ColorBleedR, _data.colorBleedR);
                m_Material.SetVector(ID_ColorBleedG, _data.colorBleedG);
                m_Material.SetVector(ID_ColorBleedB, _data.colorBleedB);
            }
            
            if (m_Material.EnableKeyword(KW_Grain, _data.grain))
            {
                m_Material.SetVector(ID_GrainScale, _data.grainScale);
                m_Material.SetColor(ID_GrainColor, _data.grainColor);
                m_Material.SetFloat(ID_GrainFrequency, _data.grainFrequency);
                m_Material.SetFloat(ID_GrainClip, _data.grainClip);
                m_Material.EnableKeyword(KW_GrainCircle, _data.grainCircle);
                m_Material.SetFloat(ID_GrainCircleWidth, _data.grainCircleWidth);
            }

            if (m_Material.EnableKeyword(KW_Vignette, _data.vignette))
            {
                m_Material.SetColor(ID_VignetteColor, _data.vignetteColor);
                m_Material.SetFloat(ID_VignetteValue, _data.vignetteValue);
            }

            return base.Validate(ref _renderingData,ref _data);
        }
    }
}