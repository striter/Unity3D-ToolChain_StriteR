using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Rendering.ImageEffect;
namespace Rendering.Pipeline
{
    public class SRP_OpaqueBlurTexture : ScriptableRenderPass, ISRPBase
    {
        #region ID
        static readonly int RT_ID_BlurTexture = Shader.PropertyToID("_OpaqueBlurTexture");
        static readonly RenderTargetIdentifier RT_BlurTexture = new RenderTargetIdentifier(RT_ID_BlurTexture);
        #endregion
        RenderTargetIdentifier m_ColorTexture;
        ImageEffect_Blurs m_Blurs;
        ImageEffectParam_Blurs m_BlurParams;
        public SRP_OpaqueBlurTexture()
        {
            m_Blurs = new ImageEffect_Blurs();
        }
        public SRP_OpaqueBlurTexture Setup(RenderTargetIdentifier _colorTexture, ImageEffectParam_Blurs _params)
        {
            m_ColorTexture = _colorTexture;
            m_BlurParams = _params;
            m_Blurs.DoValidate(_params);
            return this;
        }
        public void Dispose()
        {
            m_Blurs.Destroy();
        }
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            base.Configure(cmd, cameraTextureDescriptor);
            cmd.GetTemporaryRT(RT_ID_BlurTexture, cameraTextureDescriptor.width, cameraTextureDescriptor.height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
            ConfigureTarget(RT_BlurTexture);
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("Generate Opaque Blur Texture");
            m_Blurs.ExecuteBuffer(cmd, renderingData.cameraData.cameraTargetDescriptor, m_ColorTexture, RT_BlurTexture, m_BlurParams);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
        public override void FrameCleanup(CommandBuffer cmd)
        {
            base.FrameCleanup(cmd);
            cmd.ReleaseTemporaryRT(RT_ID_BlurTexture);
        }
    }
}