using UnityEngine;
using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.ImageEffect
{
    public class PostEffect_Bloom : PostEffectBase<ImageEffect_Bloom,CameraEffectParam_Bloom>
    {
    }

    [Serializable]
    public struct CameraEffectParam_Bloom 
    {
        [Range(0.0f, 2f)] public float threshold;
        [Range(0.0f, 5)] public float intensity;
        public ImageEffectParam_Blurs m_BlurParams;
        public static readonly CameraEffectParam_Bloom m_Default = new CameraEffectParam_Bloom()
        {
            threshold = 0.25f,
            intensity = 0.3f,
            m_BlurParams = ImageEffectParam_Blurs.m_Default,
        };
    }

    public class ImageEffect_Bloom : ImageEffectBase<CameraEffectParam_Bloom>
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

        ImageEffect_Blurs m_Blur;
        public override void Create()
        {
            base.Create();
            m_Blur = new ImageEffect_Blurs();
        }
        public override void Destroy()
        {
            base.Destroy();
            m_Blur.Destroy();
        }
        public override void OnValidate(CameraEffectParam_Bloom _data)
        {
            base.OnValidate(_data);
            m_Material.SetFloat(ID_Threshold, _data.threshold);
            m_Material.SetFloat(ID_Intensity, _data.intensity);
             m_Blur.OnValidate(_data.m_BlurParams);
        }
        public override void ExecutePostProcessBuffer(CommandBuffer _buffer, RenderTargetIdentifier _src, RenderTargetIdentifier _dst, RenderTextureDescriptor _descriptor, CameraEffectParam_Bloom _data)
        {
            var rtW = _descriptor.width;
            var rtH = _descriptor.height;

            _buffer.GetTemporaryRT(RT_ID_Blur, rtW, rtH, 0, FilterMode.Bilinear, _descriptor.colorFormat);
            _buffer.GetTemporaryRT(RT_ID_Sample, rtW, rtH, 0, FilterMode.Bilinear, _descriptor.colorFormat);

            _buffer.Blit(_src, RT_Sample, m_Material, (int)enum_Pass.SampleLight);

            m_Blur.ExecutePostProcessBuffer(_buffer, RT_Sample, RT_Blur, _descriptor, _data.m_BlurParams);

            _buffer.Blit(_src, _dst, m_Material, (int)enum_Pass.AddBloomTex);

            _buffer.ReleaseTemporaryRT(RT_ID_Sample);
            _buffer.ReleaseTemporaryRT(RT_ID_Blur);
        }
    }
}