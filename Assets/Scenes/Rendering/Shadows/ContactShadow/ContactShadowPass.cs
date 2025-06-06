using Pipeline;
using Rendering;
using Runtime.Geometry;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Examples.Rendering.Shadows.ScreenspaceShadow
{
    public class ContactShadowPass : ScriptableRenderPass
    {
        private ScreenSpaceShadowData m_Data;
        private Material m_Material;
        private static readonly int kScreenspaceShadowTexture = Shader.PropertyToID("_ContactShadowTexture");
        private static readonly RenderTargetIdentifier kShadowRT = new(kScreenspaceShadowTexture);
        private static readonly string kTitle = "Contact Shadow";
        private static readonly string kKeyword = "CONTACT_SHADOW";
        public ContactShadowPass Setup(ScreenSpaceShadowData _data, Material _material)
        {
            m_Data = _data;
            m_Material = _material;
            renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
            return this;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            base.Configure(cmd, cameraTextureDescriptor);
            cameraTextureDescriptor.colorFormat = RenderTextureFormat.ARGB32;
            cameraTextureDescriptor.depthBufferBits = 0;
            cmd.GetTemporaryRT(kScreenspaceShadowTexture, cameraTextureDescriptor);
            cmd.EnableKeyword(kKeyword,true);
        }

        public override void Execute(ScriptableRenderContext _context, ref RenderingData _renderingData)
        {
            if (!m_Material)
                return;
            
            var light = _renderingData.lightData.mainLightIndex == -1 ? null : _renderingData.lightData.visibleLights[_renderingData.lightData.mainLightIndex].light;
            if (light == null || light.shadows == LightShadows.None)
                return;
            
            var cmd = CommandBufferPool.Get(kTitle);
            _context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            cmd.SetRenderTarget(kShadowRT,_renderingData.cameraData.renderer.cameraDepthTargetHandle);
            cmd.ClearRenderTarget(false,true,Color.black);
            cmd.DrawMesh(UPipeline.kFullscreenMesh, Matrix4x4.identity, m_Material);
            _context.ExecuteCommandBuffer(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            base.FrameCleanup(cmd);
            cmd.EnableKeyword(kKeyword,false);
            cmd.ReleaseTemporaryRT(kScreenspaceShadowTexture);
        }
    }
}