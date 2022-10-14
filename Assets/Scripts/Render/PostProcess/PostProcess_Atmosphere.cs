using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rendering.PostProcess
{
    public class PostProcess_Atmosphere : PostProcessBehaviour<PPCore_Atmosphere, PPData_Transparent>
    {
        public override bool m_OpaqueProcess => false;
        public override EPostProcess Event => EPostProcess.Volumetric;
    }
    public class PPCore_Atmosphere : PostProcessCore<PPData_Transparent>
    {
        private enum EPassIndex
        {
            Combine=0,
            Sample=1,
        }
        #region ShaderProperties
        static readonly int RT_ID_Sample = Shader.PropertyToID("_Volumetric_Sample");
        static RenderTargetIdentifier RT_Sample = new RenderTargetIdentifier(RT_ID_Sample);
        static readonly int RT_ID_Blur = Shader.PropertyToID("_Volumetric_Blur");
        static RenderTargetIdentifier RT_Blur = new RenderTargetIdentifier(RT_ID_Blur);

        private const string KW_VolumetricLight = "_VOLUMETRICLIGHT";
        #endregion
        readonly PPCore_Blurs m_VolumetricBlur;
        public PPCore_Atmosphere()
        {
            m_VolumetricBlur = new PPCore_Blurs();
        }
        
        public override void Destroy()
        {
            base.Destroy();
            m_VolumetricBlur.Destroy();
        }

        public override void OnValidate(ref PPData_Transparent _data)
        {
            base.OnValidate(ref _data);
            if(m_Material.EnableKeyword(KW_VolumetricLight, _data.m_VolumetricLight))
                _data.m_VolumetricLightData.Apply(m_Material);
            m_VolumetricBlur.OnValidate(ref _data.m_VolumetricBlur);
        }
        public override void ExecutePostProcessBuffer(CommandBuffer _buffer, RenderTargetIdentifier _src, RenderTargetIdentifier _dst, RenderTextureDescriptor _descriptor,ref PPData_Transparent _data)
        {
            if (!_data.m_VolumetricLight)
            {
                _buffer.Blit(_src,_dst);
                return;
            }
            var sampleDescriptor = _descriptor;
            sampleDescriptor.width /= _data.m_VolumetricDownSample;
            sampleDescriptor.height /= _data.m_VolumetricDownSample;
            sampleDescriptor.colorFormat = RenderTextureFormat.ARGB32;
            sampleDescriptor.depthBufferBits = 0;
        
            _buffer.GetTemporaryRT(RT_ID_Sample, sampleDescriptor,FilterMode.Bilinear);

            if (!_data.m_EnableVolumetricBlur)
            {
                _buffer.Blit(_src, RT_ID_Sample, m_Material, (int)EPassIndex.Sample);
            }
            else
            {
                _buffer.GetTemporaryRT(RT_ID_Blur, sampleDescriptor, FilterMode.Bilinear);
                _buffer.Blit(_src, RT_ID_Blur, m_Material,  (int)EPassIndex.Sample);
                m_VolumetricBlur.ExecutePostProcessBuffer(_buffer, RT_ID_Blur, RT_ID_Sample, sampleDescriptor,ref _data.m_VolumetricBlur); 
                _buffer.ReleaseTemporaryRT(RT_ID_Blur);
            }
            
            _buffer.Blit(_src, _dst, m_Material,  (int)EPassIndex.Combine);
            _buffer.ReleaseTemporaryRT(RT_ID_Sample);

        }
    }
    [Serializable]
    public struct PPData_Transparent:IPostProcessParameter
    {
        public enum EMarchTimes
        {
            _8=8,
            _16=16,
            _32=32,
            _64=64,
            _128=128,
        }

        [MTitle]public bool m_VolumetricLight;
        [MFoldout(nameof(m_VolumetricLight),true)] public Data_VolumetricLight m_VolumetricLightData;
        [Header("Optimize")]
        [Range(1, 4)] public int m_VolumetricDownSample;
        [MTitle] public bool m_EnableVolumetricBlur;
        [MFoldout(nameof(m_EnableVolumetricBlur), true)] public PPData_Blurs m_VolumetricBlur;
        public bool Validate() => m_VolumetricLight;
        public static readonly PPData_Transparent kDefault = new PPData_Transparent()
        {
            m_VolumetricLight = true,
            m_VolumetricLightData = new Data_VolumetricLight()
            {
                m_Strength = 1f,
                m_Pow = 2,
                m_MarchStrength = .3f,
                m_Distance = 20f,
                m_MarchTimes = EMarchTimes._64,
                m_Dither=false,
            },
            m_VolumetricDownSample = 1,
            m_EnableVolumetricBlur=false,
            m_VolumetricBlur=PPData_Blurs.kDefault,
        };

        [Serializable]
        public struct Data_VolumetricLight
        {
            [Range(.1f,5f)]public float m_Strength;
            [Clamp(0f)] public float m_Distance;
            public EMarchTimes m_MarchTimes;
            [Range(0, 2f)] public float m_MarchStrength;
            [Range(0.1f, 2f)] public float m_Pow;
            public bool m_Dither;
            #region Properties
            static readonly int ID_ColorStrength = Shader.PropertyToID("_ColorStrength");
            static readonly int ID_LightPow = Shader.PropertyToID("_LightPow");
            static readonly int ID_LightStrength = Shader.PropertyToID("_LightStrength");
            static readonly int ID_MarchTimes = Shader.PropertyToID("_MarchTimes");
            static readonly int ID_MarchDistance = Shader.PropertyToID("_MarchDistance");
            const string kW_DITHER = "_DITHER";

            public void Apply(Material m_Material)
            {
                m_Material.SetFloat(ID_ColorStrength,m_Strength);
                m_Material.SetInt(ID_MarchTimes, (int)m_MarchTimes);
                m_Material.SetFloat(ID_MarchDistance, m_Distance);
                m_Material.SetFloat(ID_LightPow, m_Pow);
                m_Material.SetFloat(ID_LightStrength, m_MarchStrength);
                m_Material.EnableKeyword(kW_DITHER, m_Dither);
            }
            #endregion
        }
    }
}