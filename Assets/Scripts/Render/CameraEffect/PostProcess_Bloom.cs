using UnityEngine;
using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.ImageEffect
{
    public class PostProcess_Bloom : PostProcessComponentBase<PPCore_Bloom,PPData_Bloom>
    {
    }

    [Serializable]
    public struct PPData_Bloom 
    {
        [Range(0.0f, 2f)] public float threshold;
        [Range(0.0f, 5)] public float intensity;
        public PPData_Blurs m_BlurParams;
        public static readonly PPData_Bloom m_Default = new PPData_Bloom()
        {
            threshold = 0.25f,
            intensity = 0.3f,
            m_BlurParams = PPData_Blurs.m_Default,
        };
    }

    public class PPCore_Bloom : PostProcessCore<PPData_Bloom>
    {
        #region ShaderProperties
        static readonly int RT_ID_Sample = Shader.PropertyToID("_Bloom_Sample");
        static readonly int RT_ID_Blur = Shader.PropertyToID("_Bloom_Blur");
        
        static readonly RenderTargetIdentifier RT_Sample = new RenderTargetIdentifier(RT_ID_Sample);
        static readonly RenderTargetIdentifier RT_Blur = new RenderTargetIdentifier(RT_ID_Blur);

        static readonly int ID_Threshold = Shader.PropertyToID("_Threshold");
        static readonly int ID_Intensity = Shader.PropertyToID("_Intensity");
        #endregion

        public enum enum_Pass
        {
            SampleLight = 0,
            AddBloomTex = 1,
        }

        PPCore_Blurs _mCoreBlur;
        public override void Create()
        {
            base.Create();
            _mCoreBlur = new PPCore_Blurs();
        }
        public override void Destroy()
        {
            base.Destroy();
            _mCoreBlur.Destroy();
        }
        public override void OnValidate(PPData_Bloom _data)
        {
            base.OnValidate(_data);
            m_Material.SetFloat(ID_Threshold, _data.threshold);
            m_Material.SetFloat(ID_Intensity, _data.intensity);
             _mCoreBlur.OnValidate(_data.m_BlurParams);
        }
        public override void ExecutePostProcessBuffer(CommandBuffer _buffer, RenderTargetIdentifier _src, RenderTargetIdentifier _dst, RenderTextureDescriptor _descriptor, PPData_Bloom _data)
        {
            var rtW = _descriptor.width;
            var rtH = _descriptor.height;

            _buffer.GetTemporaryRT(RT_ID_Blur, rtW, rtH, 0, FilterMode.Bilinear, _descriptor.colorFormat);
            _buffer.GetTemporaryRT(RT_ID_Sample, rtW, rtH, 0, FilterMode.Bilinear, _descriptor.colorFormat);

            _buffer.Blit(_src, RT_Sample, m_Material, (int)enum_Pass.SampleLight);

            _mCoreBlur.ExecutePostProcessBuffer(_buffer, RT_Sample, RT_Blur, _descriptor, _data.m_BlurParams);

            _buffer.Blit(_src, _dst, m_Material, (int)enum_Pass.AddBloomTex);

            _buffer.ReleaseTemporaryRT(RT_ID_Sample);
            _buffer.ReleaseTemporaryRT(RT_ID_Blur);
        }
    }
}