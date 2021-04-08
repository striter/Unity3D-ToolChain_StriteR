using UnityEngine;
using System;
using UnityEngine.Rendering;

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
        public ImageEffect_Bloom() : base()
        {
            m_Blur = new ImageEffect_Blurs();
        }
        public override void Destroy()
        {
            base.Destroy();
            m_Blur.Destroy();
        }
        protected override void OnValidate(CameraEffectParam_Bloom _params, Material _material)
        {
            base.OnValidate(_params, _material);
            _material.SetFloat(ID_Threshold, _params.threshold);
            _material.SetFloat(ID_Intensity, _params.intensity);
             m_Blur.DoValidate(_params.m_BlurParams);
        }
        protected override void OnExecuteBuffer(CommandBuffer _buffer, RenderTextureDescriptor _descriptor, RenderTargetIdentifier _src, RenderTargetIdentifier _dst, Material _material, CameraEffectParam_Bloom _param)
        {
            var rtW = _descriptor.width;
            var rtH = _descriptor.height;

            _buffer.GetTemporaryRT(RT_ID_Blur, rtW, rtH, 0, FilterMode.Bilinear, _descriptor.colorFormat);
            _buffer.GetTemporaryRT(RT_ID_Sample, rtW, rtH, 0, FilterMode.Bilinear, _descriptor.colorFormat);

            _buffer.Blit(_src, RT_Sample, _material, (int)enum_Pass.SampleLight);

            m_Blur.ExecuteBuffer(_buffer,_descriptor, RT_Sample, RT_Blur, _param.m_BlurParams);

            _buffer.Blit(_src, _dst, _material, (int)enum_Pass.AddBloomTex);

            _buffer.ReleaseTemporaryRT(RT_ID_Sample);
            _buffer.ReleaseTemporaryRT(RT_ID_Blur);
        }
    }
}