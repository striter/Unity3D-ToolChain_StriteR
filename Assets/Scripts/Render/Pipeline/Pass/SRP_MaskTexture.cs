using Rendering.Pipeline;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.Pipeline
{
    public class SRP_MaskTexture : ScriptableRenderPass,ISRPBase
    {
        private ScriptableRenderer m_Renderer;
        private SRD_MaskData m_MaskData;
        
        private readonly PassiveInstance<Material> m_HighlightRender=new PassiveInstance<Material>(() =>
        {
            var m_HighlightRender = new Material(RenderResources.FindInclude("Game/Unlit/Color")) { hideFlags = HideFlags.HideAndDontSave };
            m_HighlightRender.SetColor(DShaderProperties.kColor, Color.white);
            return m_HighlightRender;
        },GameObject.DestroyImmediate);

        public SRP_MaskTexture Setup(SRD_MaskData _cullingMask,ScriptableRenderer _renderer)
        {
            m_MaskData = _cullingMask;
            m_Renderer = _renderer;
            return this;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cmd.GetTemporaryRT(DRenderTextures.kCameraMaskTexture, cameraTextureDescriptor.width, cameraTextureDescriptor.height, 0, FilterMode.Bilinear, RenderTextureFormat.R8);
            ConfigureTarget(DRenderTextures.kCameraMaskTexture);
            base.Configure(cmd, cameraTextureDescriptor);
        }
        public override void FrameCleanup(CommandBuffer cmd)
        {
            base.FrameCleanup(cmd);
            cmd.ReleaseTemporaryRT(DRenderTextures.kCameraMaskTexture);
        }
        
        public void Dispose()
        {
            m_HighlightRender.Dispose();
        }

        public override void Execute(ScriptableRenderContext _context, ref RenderingData _renderingData)
        {                
            CommandBuffer buffer = CommandBufferPool.Get("Camera Mask Texture");
            buffer.SetRenderTarget(DRenderTextures.kCameraMaskTextureRT);
            buffer.ClearRenderTarget(false, true, Color.black);
            _context.ExecuteCommandBuffer(buffer);

            DrawingSettings drawingSettings = UPipeline.CreateDrawingSettings(true, _renderingData.cameraData.camera);
            m_HighlightRender.m_Value.SetInt(DShaderProperties.kColorMask,(int)ColorWriteMask.All);
            m_HighlightRender.m_Value.SetInt(DShaderProperties.kZTest,(int)CompareFunction.Less);
            m_HighlightRender.m_Value.SetInt(DShaderProperties.kCull,(int)CullMode.Back);
            drawingSettings.overrideMaterial = m_HighlightRender;
            FilteringSettings filterSettings = new FilteringSettings(RenderQueueRange.all) { layerMask = m_MaskData.cullingMask };
            _context.DrawRenderers(_renderingData.cullResults, ref drawingSettings, ref filterSettings);

            buffer.Clear();
            buffer.SetRenderTarget(m_Renderer.cameraColorTarget);
            _context.ExecuteCommandBuffer(buffer);
            CommandBufferPool.Release(buffer);
        }

    }

}