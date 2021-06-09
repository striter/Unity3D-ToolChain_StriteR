using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Rendering.ImageEffect
{
    public class PostEffect_DepthSSAO:PostEffectBase<CameraEffect_DepthSSAO, CameraEffectParam_DepthSSAO>
    {
        public override bool m_IsOpaqueProcess => true;
    }

    [System.Serializable]
    public struct CameraEffectParam_DepthSSAO
    {
        public Color m_Color;
        public float m_Intensity;
        public float m_SampleRadius;
        [Range(0.01f,0.5f)] public float m_DepthBias;
        [Range(0.01f, 1f)] public float m_DepthBiasRange;
        public int m_SampleCount;
        public Texture2D m_NoiseTex;
        public float m_NoiseScale;
        public static readonly CameraEffectParam_DepthSSAO m_Default = new CameraEffectParam_DepthSSAO()
        {
            m_Color = Color.grey,
            m_Intensity = 1f,
            m_SampleRadius = 10f,
            m_DepthBias = 0.1f,
            m_DepthBiasRange = 0.5f,
            m_SampleCount = 16,
            m_NoiseScale = 1,
        };
    }

    public class CameraEffect_DepthSSAO:ImageEffectBase<CameraEffectParam_DepthSSAO>
    {
        #region ShaderProperties
        static readonly int ID_SampleCount = Shader.PropertyToID("_SampleCount");
        static readonly int ID_SampleSphere = Shader.PropertyToID("_SampleSphere");
        static readonly int ID_Color = Shader.PropertyToID("_AOColor");
        static readonly int ID_Intensity = Shader.PropertyToID("_Intensity");
        static readonly int ID_DepthBias = Shader.PropertyToID("_DepthBias");
        static readonly int ID_DepthBiasMax = Shader.PropertyToID("_DepthBiasMax");
        static readonly int ID_NoiseTex = Shader.PropertyToID("_NoiseTex");
        static readonly int ID_NoiseScale = Shader.PropertyToID("_NoiseScale");
        #endregion
        static readonly Vector4[] m_DepthSampleArray = new Vector4[16] {
            new Vector3( 0.5381f, 0.1856f,-0.4319f),  new Vector3( 0.1379f, 0.2486f, 0.4430f),new Vector3( 0.3371f, 0.5679f,-0.0057f),  new Vector3(-0.6999f,-0.0451f,-0.0019f),
            new Vector3( 0.0689f,-0.1598f,-0.8547f),  new Vector3( 0.0560f, 0.0069f,-0.1843f),new Vector3(-0.0146f, 0.1402f, 0.0762f),  new Vector3( 0.0100f,-0.1924f,-0.0344f),
            new Vector3(-0.3577f,-0.5301f,-0.4358f),  new Vector3(-0.3169f, 0.1063f, 0.0158f),new Vector3( 0.0103f,-0.5869f, 0.0046f),  new Vector3(-0.0897f,-0.4940f, 0.3287f),
            new Vector3( 0.7119f,-0.0154f,-0.0918f),  new Vector3(-0.0533f, 0.0596f,-0.5411f),new Vector3( 0.0352f,-0.0631f, 0.5460f),  new Vector3(-0.4776f, 0.2847f,-0.0271f)};
        public override void OnValidate(CameraEffectParam_DepthSSAO _data)
        {
            base.OnValidate(_data);
            Vector4[] array = new Vector4[m_DepthSampleArray.Length];
            for (int i = 0; i < m_DepthSampleArray.Length; i++)
                array[i] = m_DepthSampleArray[i] * _data.m_SampleRadius;
            m_Material.SetInt(ID_SampleCount, _data.m_SampleCount);
            m_Material.SetVectorArray(ID_SampleSphere, array);
            m_Material.SetColor(ID_Color, _data.m_Color);
            m_Material.SetFloat(ID_Intensity, _data.m_Intensity);
            m_Material.SetFloat(ID_DepthBias, _data.m_DepthBias);
            m_Material.SetFloat(ID_DepthBiasMax, _data.m_DepthBias + _data.m_DepthBiasRange);
            m_Material.SetTexture(ID_NoiseTex, _data.m_NoiseTex);
            m_Material.SetFloat(ID_NoiseScale, _data.m_NoiseScale);
        }
    }

}