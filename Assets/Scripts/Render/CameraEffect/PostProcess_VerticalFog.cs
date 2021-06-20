using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Rendering.ImageEffect
{
    public class PostProcess_VerticalFog:PostProcessComponentBase<PPCore_VerticalFog, PPData_VerticalFog>
    {
    }

    [Serializable]
    public struct PPData_VerticalFog
    {
        [ColorUsage(true,true)]public Color m_FogColor;
        public float m_FogDensity;
        public float m_FogVerticalStart;
        public float m_FogVerticalOffset;
        [MTitle] public Texture2D m_NoiseTexure;
        [MFold(nameof(m_NoiseTexure), null)] public float m_NoiseScale;
        [MFold(nameof(m_NoiseTexure), null)] public float m_NoiseSpeedX;
        [MFold(nameof(m_NoiseTexure), null)] public float m_NoiseSpeedY;
        public static readonly PPData_VerticalFog m_Default = new PPData_VerticalFog()
        {
            m_FogColor=Color.grey,
            m_FogDensity = 1,
            m_FogVerticalStart = -2f,
            m_FogVerticalOffset = 2f,
            m_NoiseScale = 15f,
            m_NoiseSpeedX = .1f,
            m_NoiseSpeedY = .1f,
    };
    }
    public class PPCore_VerticalFog:PostProcessCore<PPData_VerticalFog>
    {
        #region ShaderProeprties
        static readonly int ID_FogColor = Shader.PropertyToID("_FogColor");
        static readonly int ID_FogDensity = Shader.PropertyToID("_FogDensity");
        static readonly int ID_FogVerticalStart = Shader.PropertyToID("_FogVerticalStart");
        static readonly int ID_FogVerticalOffset = Shader.PropertyToID("_FogVerticalOffset");
        const string KW_Noise = "_NOISE";
        static readonly int ID_NoiseTexure = Shader.PropertyToID("_NoiseTex");
        static readonly int ID_NoiseScale = Shader.PropertyToID("_NoiseScale");
        static readonly int ID_NoiseSpeedX = Shader.PropertyToID("_NoiseSpeedX");
        static readonly int ID_NoiseSpeedY = Shader.PropertyToID("_NoiseSpeedY");
        #endregion
        public override void OnValidate(PPData_VerticalFog _data)
        {
            base.OnValidate(_data);
            m_Material.SetColor(ID_FogColor, _data.m_FogColor);
            m_Material.SetFloat(ID_FogDensity, _data.m_FogDensity);
            m_Material.SetFloat(ID_FogVerticalStart, _data.m_FogVerticalStart);
            m_Material.SetFloat(ID_FogVerticalOffset, _data.m_FogVerticalOffset);
            m_Material.EnableKeyword(KW_Noise, _data.m_NoiseTexure);
            m_Material.SetTexture(ID_NoiseTexure, _data.m_NoiseTexure);
            m_Material.SetFloat(ID_NoiseScale, _data.m_NoiseScale);
            m_Material.SetFloat(ID_NoiseSpeedX, _data.m_NoiseSpeedX);
            m_Material.SetFloat(ID_NoiseSpeedY, _data.m_NoiseSpeedY);
        }
    }
}