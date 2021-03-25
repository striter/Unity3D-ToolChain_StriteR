using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rendering.ImageEffect
{
    public class PostEffect_DepthOfField : PostEffectBase<CameraEffect_DepthOfField, CameraEffectParam_DepthOfField>
    {
    }

    [Serializable]
    public struct CameraEffectParam_DepthOfField 
    {
        [Range(0.01f, 1f)] public float m_DOFStart;
        [Range(.01f, .3f)] public float m_DOFLerp;
        public ImageEffectParam_Blurs m_BlurParams;
        [Header("Depth Blur")]
        public bool m_DepthBlurSample;
        [Range(.25f, 1.25f)] public float m_DepthBlurSize;
        public static readonly CameraEffectParam_DepthOfField m_Default = new CameraEffectParam_DepthOfField()
        {
            m_DOFStart = 0.1f,
            m_DOFLerp = .1f,
            m_BlurParams = ImageEffectParam_Blurs.m_Default,
            m_DepthBlurSample = true,
            m_DepthBlurSize = .5f,
        };
}
    public class CameraEffect_DepthOfField : ImageEffectBase<CameraEffectParam_DepthOfField>
    {
        #region ShaderID
        static int RT_ID_Blur = Shader.PropertyToID("_BlurTex");
        static RenderTargetIdentifier RT_Blur = new RenderTargetIdentifier(RT_ID_Blur);

        static int ID_FocalStart = Shader.PropertyToID("_FocalStart");
        static int ID_FocalLerp = Shader.PropertyToID("_FocalLerp");
        static int ID_BlurSize = Shader.PropertyToID("_BlurSize");
        const string KW_UseBlurDepth = "_UseBlurDepth";
        #endregion

        ImageEffect_Blurs m_Blur;
        public CameraEffect_DepthOfField() : base() { m_Blur = new ImageEffect_Blurs(); }
        public override void Destroy()
        {
            base.Destroy();
            m_Blur.Destroy();
        }
        protected override void OnValidate(CameraEffectParam_DepthOfField _params, Material _material)
        {
            base.OnValidate(_params, _material);
            _material.SetFloat(ID_FocalStart, _params.m_DOFStart);
            _material.SetFloat(ID_FocalLerp, _params.m_DOFLerp);
            _material.EnableKeyword(KW_UseBlurDepth, _params.m_DepthBlurSample);
            _material.SetFloat(ID_BlurSize, _params.m_DepthBlurSize);
            m_Blur.DoValidate(_params.m_BlurParams);
        }
        protected override void OnExecuteBuffer(CommandBuffer _buffer, RenderTextureDescriptor _descriptor, RenderTargetIdentifier _src, RenderTargetIdentifier _dst, Material _material, CameraEffectParam_DepthOfField _param)
        {
            base.OnExecuteBuffer(_buffer, _descriptor, _src, _dst, _material, _param);
            _buffer.GetTemporaryRT(RT_ID_Blur,_descriptor.width,_descriptor.height,0,FilterMode.Bilinear,RenderTextureFormat.ARGB32);
            m_Blur.ExecuteBuffer(_buffer,_descriptor,_src, RT_Blur, _param.m_BlurParams);
            _buffer.Blit(_src, _dst, _material);
            _buffer.ReleaseTemporaryRT(RT_ID_Blur);
        }

    }
}
