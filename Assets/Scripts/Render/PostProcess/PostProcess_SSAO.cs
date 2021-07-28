using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Random = System.Random;

namespace Rendering.ImageEffect
{
    public class PostProcess_SSAO:PostProcessComponentBase<PPCore_SSAO, PPData_SSAO>
    {
        public override bool m_IsOpaqueProcess => true;
    }

    [Serializable]
    public struct PPData_SSAO
    {
        public Color m_Color;
        [Range(0.01f,5f)]public float m_Intensity;
        [Range(0.1f,1f)]public float m_Radius;
        [Range(0.01f, 0.5f)] public float m_Bias;
        [Header("Misc")]
        [IntEnum(8,16,32,64)]public int m_SampleCount;
        public bool m_Dither;
        public string m_RandomVectorKeywords;
        public static readonly PPData_SSAO m_Default = new PPData_SSAO()
        {
            m_Color = Color.grey,
            m_Intensity = 1f,
            m_Radius = .5f,
            m_SampleCount = 16,
            m_Dither=true,
            m_RandomVectorKeywords=DateTime.Now.ToShortTimeString(),
        };
    }

    public class PPCore_SSAO:PostProcessCore<PPData_SSAO>
    {
        #region ShaderProperties
        static readonly int ID_SampleCount = Shader.PropertyToID("_SampleCount");
        static readonly int ID_SampleSphere = Shader.PropertyToID("_SampleSphere");
        static readonly int ID_Color = Shader.PropertyToID("_AOColor");
        static readonly int ID_Intensity = Shader.PropertyToID("_Intensity");
        static readonly int ID_Radius = Shader.PropertyToID("_Radius");
        static  readonly int ID_Bias=Shader.PropertyToID("_Bias");
        private const string KW_Dither = "_DITHER";
        const int m_MaxArraySize = 64;
        #endregion

        public PPCore_SSAO()
        {
        }
        
        public override void OnValidate(ref PPData_SSAO ppDataSsao)
        {
            base.OnValidate(ref ppDataSsao);
            Random random = new Random(ppDataSsao.m_RandomVectorKeywords?.GetHashCode() ?? "AOCodeDefault".GetHashCode());
            Vector4[] randomVectors = new Vector4[m_MaxArraySize];
            for (int i = 0; i < m_MaxArraySize; i++)
                randomVectors[i] = URandom.RandomVector3(random)*Mathf.Lerp( 1f-ppDataSsao.m_Radius,1f,URandom.Random01(random));
            m_Material.SetFloat(ID_Bias,ppDataSsao.m_Radius+ppDataSsao.m_Bias);
            m_Material.SetFloat(ID_Radius, ppDataSsao.m_Radius);
            m_Material.SetInt(ID_SampleCount, ppDataSsao.m_SampleCount);
            m_Material.SetVectorArray(ID_SampleSphere, randomVectors);
            m_Material.SetColor(ID_Color, ppDataSsao.m_Color);
            m_Material.SetFloat(ID_Intensity, ppDataSsao.m_Intensity);
            m_Material.EnableKeyword(KW_Dither,ppDataSsao.m_Dither);
        }
    }

}