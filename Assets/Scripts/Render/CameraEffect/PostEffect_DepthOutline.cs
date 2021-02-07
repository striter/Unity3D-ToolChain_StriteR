using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rendering.ImageEffect
{
    public class PostEffect_DepthOutline:PostEffectBase<CameraEffect_DepthOutline, CameraEffectParam_DepthOutline>
    {
        [ImageEffectOpaque]
        protected new void OnRenderImage(RenderTexture source, RenderTexture destination)=> base.OnRenderImage(source, destination);
    }

    [System.Serializable]
    public struct CameraEffectParam_DepthOutline
    {
        [ColorUsage(true,true)] public Color m_OutlineColor;
        [Range(0,3)] public float m_SampleDistance;
        [Range(0,3)] public float m_DepthBias;
        [Header("Color Replace")]
        public bool m_ColorReplace;
        public Color m_ReplaceColor;
        public static readonly CameraEffectParam_DepthOutline m_Default = new CameraEffectParam_DepthOutline()
        {
            m_OutlineColor = Color.white,
            m_SampleDistance = 1,
            m_DepthBias = 0.05f,
            m_ColorReplace=false,
            m_ReplaceColor=Color.black,
        };
    }

    public class CameraEffect_DepthOutline:ImageEffectBase<CameraEffectParam_DepthOutline>
    {
        #region ShaderProperties
        static readonly int ID_EdgeColor = Shader.PropertyToID("_OutlineColor");
        static readonly int ID_SampleDistance = Shader.PropertyToID("_SampleDistance");
        static readonly int ID_DepthBias = Shader.PropertyToID("_DepthBias");
        const string KW_ColorReplace = "REPLACECOLOR";
        static readonly int ID_ReplaceColor = Shader.PropertyToID("_ReplaceColor");
        #endregion

        protected override void OnValidate(CameraEffectParam_DepthOutline _params, Material _material)
        {
            base.OnValidate(_params, _material);
            _material.SetColor(ID_EdgeColor, _params.m_OutlineColor);
            _material.SetFloat(ID_SampleDistance, _params.m_SampleDistance);
            _material.SetFloat(ID_DepthBias, _params.m_DepthBias);
            _material.EnableKeyword(KW_ColorReplace, _params.m_ColorReplace);
            _material.SetColor(ID_ReplaceColor, _params.m_ReplaceColor);
        }
    }

}