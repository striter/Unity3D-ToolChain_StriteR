using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Rendering.Pipeline
{
    public class SRP_NormalTexture : ScriptableRenderPass, ISRPBase
    {
        static readonly int ID_CameraNormalTex = Shader.PropertyToID("_CameraNormalTexture");
        static readonly RenderTargetIdentifier RT_ID_CameraNormalTex = new RenderTargetIdentifier(ID_CameraNormalTex);
        Material m_NormalMaterial;
        public SRP_NormalTexture()
        {
            m_NormalMaterial = new Material(Shader.Find("Hidden/NormalsFromDepth")) { hideFlags = HideFlags.HideAndDontSave };
        }
        public void Dispose()
        {
            GameObject.DestroyImmediate(m_NormalMaterial);
        }
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cmd.GetTemporaryRT(ID_CameraNormalTex, cameraTextureDescriptor.width, cameraTextureDescriptor.height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
            ConfigureTarget(RT_ID_CameraNormalTex);
            base.Configure(cmd, cameraTextureDescriptor);
        }
        public override void FrameCleanup(CommandBuffer cmd)
        {
            base.FrameCleanup(cmd);
            cmd.ReleaseTemporaryRT(ID_CameraNormalTex);
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("Generate Normal Texture");
            cmd.Blit(null, RT_ID_CameraNormalTex, m_NormalMaterial);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }
}