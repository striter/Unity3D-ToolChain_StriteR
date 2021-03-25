using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rendering.ImageEffect
{
    public class PostEffect_Outline:PostEffectBase<CameraEffect_Outline, CameraEffectParam_Outline>
    {
        public override bool m_IsOpaqueProcess => !m_EffectData.m_ColorReplace;
    }

    public enum enum_Convolution
    {
        Prewitt = 1,
        Sobel = 2,
    }

    public enum enum_DetectType
    {
        Depth = 1,
        Color = 2,
        Normal = 3,
    }

    [System.Serializable]
    public struct CameraEffectParam_Outline
    {
        [ColorUsage(true, true)] public Color m_OutlineColor;
        [Range(.1f, 3f)] public float m_OutlineWidth;
        public enum_Convolution m_Convolution;
        public enum_DetectType m_DetectType;
        [Range(0, 10f)] public float m_Strength;
        [Range(0, 3f)] public float m_Bias;
        [Header("Color Replace")]
        public bool m_ColorReplace;
        public Color m_ReplaceColor;
        public static readonly CameraEffectParam_Outline m_Default = new CameraEffectParam_Outline()
        {
            m_OutlineColor = Color.white,
            m_OutlineWidth = 1,
            m_Convolution= enum_Convolution.Prewitt,
            m_DetectType= enum_DetectType.Depth,
            m_Strength=2f,
            m_Bias=.5f,
            m_ColorReplace = false,
            m_ReplaceColor = Color.black,
        };
    }

    public class CameraEffect_Outline:ImageEffectBase<CameraEffectParam_Outline>
    {
        #region ShaderProperties
        static readonly int ID_EdgeColor = Shader.PropertyToID("_OutlineColor");
        static readonly int ID_OutlineWidth = Shader.PropertyToID("_OutlineWidth");
        static readonly string[] KW_Convolution = new string[2] { "_CONVOLUTION_PREWITT", "_CONVOLUTION_SOBEL" };
        static readonly string[] KW_DetectType = new string[3] { "_DETECT_DEPTH", "_DETECT_COLOR", "_DETECT_NORMAL" };
        static readonly int ID_Strength = Shader.PropertyToID("_Strength");
        static readonly int ID_Bias = Shader.PropertyToID("_Bias");
        const string KW_ColorReplace = "_COLORREPLACE";
        static readonly int ID_ReplaceColor = Shader.PropertyToID("_ReplaceColor");
        #endregion

        protected override void OnValidate(CameraEffectParam_Outline _params, Material _material)
        {
            base.OnValidate(_params, _material);
            _material.SetColor(ID_EdgeColor, _params.m_OutlineColor);
            _material.SetFloat(ID_OutlineWidth, _params.m_OutlineWidth);
            _material.EnableKeywords(KW_Convolution, (int)_params.m_Convolution);
            _material.EnableKeywords(KW_DetectType, (int)_params.m_DetectType);
            _material.SetFloat(ID_Strength, _params.m_Strength);
            _material.SetFloat(ID_Bias, _params.m_Bias);
            _material.EnableKeyword(KW_ColorReplace,_params.m_ColorReplace);
            _material.SetColor(ID_ReplaceColor, _params.m_ReplaceColor);
        }
    }

}