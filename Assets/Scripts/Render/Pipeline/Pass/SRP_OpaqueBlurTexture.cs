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
        PPCore_Blurs _mCoreBlurs;
        PPData_Blurs m_BlurParams;
        public SRP_OpaqueBlurTexture()
        {
            _mCoreBlurs = new PPCore_Blurs();
        }
        public SRP_OpaqueBlurTexture Setup(ScriptableRenderer _renderer, PPData_Blurs _params)
        {
            m_Renderer = _renderer;
            m_BlurParams = _params;
            _mCoreBlurs.OnValidate(_params);
            return this;
        }
        public void Dispose()
        {
            _mCoreBlurs.Destroy();
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
            _mCoreBlurs.ExecutePostProcessBuffer(cmd, m_Renderer.cameraColorTarget, RT_BlurTexture, renderingData.cameraData.cameraTargetDescriptor, m_BlurParams);
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