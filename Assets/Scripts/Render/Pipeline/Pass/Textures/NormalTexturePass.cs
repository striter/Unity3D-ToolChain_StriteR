using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Rendering.Pipeline
{
    using static KRenderTextures;
    
    public class NormalTexturePass : ScriptableRenderPass, ISRPBase
    {
        PassiveInstance<Material> m_NormalMaterial=new PassiveInstance<Material>(()=>new Material( RenderResources.FindInclude("Hidden/NormalsFromDepth"))  {hideFlags = HideFlags.HideAndDontSave},GameObject.DestroyImmediate);

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cmd.GetTemporaryRT(kCameraNormalTex, cameraTextureDescriptor.width, cameraTextureDescriptor.height, 0, FilterMode.Point, RenderTextureFormat.ARGB32);
            ConfigureTarget(RTHandles.Alloc(kRTCameraNormalTex));
            base.Configure(cmd, cameraTextureDescriptor);
        }
        public override void FrameCleanup(CommandBuffer _cmd)
        {
            base.FrameCleanup(_cmd);
            _cmd.ReleaseTemporaryRT(kCameraNormalTex);
        }
        public override void Execute(ScriptableRenderContext _context, ref RenderingData _renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("Generate Normal Texture");
            cmd.Blit(null, kRTCameraNormalTex, m_NormalMaterial);
            _context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
        public void Dispose()
        {
            m_NormalMaterial.Dispose();
        }
    }
}