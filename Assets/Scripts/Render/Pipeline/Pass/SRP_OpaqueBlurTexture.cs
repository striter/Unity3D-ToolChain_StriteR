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
        ScriptableRenderer m_Renderer;
        ImageEffect_Blurs m_Blurs;
        ImageEffectParam_Blurs m_BlurParams;
        public SRP_OpaqueBlurTexture()
        {
            m_Blurs = new ImageEffect_Blurs();
        }
        public SRP_OpaqueBlurTexture Setup(ScriptableRenderer _renderer, ImageEffectParam_Blurs _params)
        {
            m_Renderer = _renderer;
            m_BlurParams = _params;
            m_Blurs.OnValidate(_params);
            return this;
        }
        public void Dispose()
        {
            m_Blurs.Destroy();
        }
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cmd.GetTemporaryRT(RT_ID_BlurTexture, cameraTextureDescriptor);
            ConfigureTarget(RT_BlurTexture);
            base.Configure(cmd, cameraTextureDescriptor);
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("Generate Opaque Blur Texture");
            m_Blurs.ExecutePostProcessBuffer(cmd, m_Renderer.cameraColorTarget, RT_BlurTexture, renderingData.cameraData.cameraTargetDescriptor, m_BlurParams);
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