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
        [Range(0, 1)]
        public float m_ScreenCutDistance;
        [Header("Color Bleed")]
        public bool m_ColorBleed = true;
        [Range(1,4)]
        public int m_ColorBleedIteration = 2;
        [Range(0,2)]
        public float m_ColorBleedSize = .8f;
        public Vector2 m_ColorBleedR = Vector2.one;
        public Vector2 m_ColorBleedG = -Vector2.one;
        public Vector2 m_ColorBleedB = Vector2.zero;
        [Header("Grain")]
        public bool m_Grain = true;
        public Color m_GrainColor = Color.white;
        public float m_GrainScale = 1f;
        public Vector2 m_GrainFlow = Vector2.one;
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

        const string KW_Grain = "_GRAIN";
        static readonly int ID_GrainScale = Shader.PropertyToID("_GrainScale");
        static readonly int ID_GrainColor = Shader.PropertyToID("_GrainColor");
        static readonly int ID_GrainFlow = Shader.PropertyToID("_GrainFlow");
        #endregion

        protected override void OnValidate(ImageEffectParam_VHS _params, Material _material)
        {
            base.OnValidate(_params, _material);
            _material.EnableKeywords(KW_SCREENCUT, (int)_params.m_ScreenCut);
            _material.SetFloat(ID_ScreenCutTarget,(1f+ (_params.m_ScreenCut == enum_VHSScreenCut.Scaled?1:-1)*_params.m_ScreenCutDistance) /2f);

            _material.EnableKeyword(KW_ColorBleed, _params.m_ColorBleed);
            _material.SetInt(ID_ColorBleedIteration, _params.m_ColorBleedIteration);
            _material.SetFloat(ID_ColorBleedSize, _params.m_ColorBleedSize);
            _material.EnableKeyword(KW_ColorBleedR, _params.m_ColorBleedR!=Vector2.zero);
            _material.EnableKeyword(KW_ColorBleedG, _params.m_ColorBleedG != Vector2.zero);
            _material.EnableKeyword(KW_ColorBleedB, _params.m_ColorBleedB != Vector2.zero);
            _material.SetVector(ID_ColorBleedR, _params.m_ColorBleedR);
            _material.SetVector(ID_ColorBleedG, _params.m_ColorBleedG);
            _material.SetVector(ID_ColorBleedB, _params.m_ColorBleedB);

            _material.EnableKeyword(KW_Grain, _params.m_Grain);
            _material.SetFloat(ID_GrainScale, _params.m_GrainScale);
            _material.SetVector(ID_GrainFlow, _params.m_GrainFlow);
            _material.SetColor(ID_GrainColor, _params.m_GrainColor);
        }

    }
}