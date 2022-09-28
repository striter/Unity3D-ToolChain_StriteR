using System;
using Rendering.Pipeline;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.Pipeline
{
    public enum EOutlineVertex
    {
        _NORMALSAMPLE_NORMAL,
        _NORMALSAMPLE_TANGENT,
        _NORMALSAMPLE_UV1,
        _NORMALSAMPLE_UV2,
        _NORMALSAMPLE_UV3,
        _NORMALSAMPLE_UV4,
        _NORMALSAMPLE_UV5,
        _NORMALSAMPLE_UV6,
        _NORMALSAMPLE_UV7
    }

    [Serializable]
    public struct SRD_MaskData
    {
        [CullingMask]public int renderMask;
        [Header("Misc")]
        public Color color;
        [Range(0,1)]public float extendWidth;
        public EOutlineVertex outlineVertex;
        public static readonly SRD_MaskData kDefault = new SRD_MaskData()
        {
            renderMask=int.MaxValue,
            color = Color.white,
            extendWidth = 0.1f,
            outlineVertex  = EOutlineVertex._NORMALSAMPLE_NORMAL,
        };
    }
    
    public class SRP_Mask : ScriptableRenderPass,ISRPBase
    {
        private ScriptableRenderer m_Renderer;
        private SRD_MaskData m_MaskData;

        private readonly PassiveInstance<Material> m_HighlightRender=new PassiveInstance<Material>(() => new Material(RenderResources.FindInclude("Game/Additive/Outline")) { hideFlags = HideFlags.HideAndDontSave },GameObject.DestroyImmediate);

        public SRP_Mask Setup(SRD_MaskData _cullingMask,ScriptableRenderer _renderer)
        {
            m_MaskData = _cullingMask;
            m_Renderer = _renderer;
            m_HighlightRender.m_Value.SetColor("_OutlineColor",m_MaskData.color);
            m_HighlightRender.m_Value.SetFloat("_OutlineWidth",m_MaskData.extendWidth);
            m_HighlightRender.m_Value.EnableKeywords(m_MaskData.outlineVertex);
            m_HighlightRender.m_Value.SetInt(KShaderProperties.kCull,(int)CullMode.Off);
            m_HighlightRender.m_Value.SetInt(KShaderProperties.kColorMask,(int)ColorWriteMask.All);
            m_HighlightRender.m_Value.SetInt(KShaderProperties.kZWrite,1);
            return this;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cameraTextureDescriptor.colorFormat = RenderTextureFormat.R8;
            cameraTextureDescriptor.depthBufferBits = 0;
            cmd.GetTemporaryRT(DRenderTextures.kCameraMaskTexture, cameraTextureDescriptor);
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
            drawingSettings.overrideMaterial = m_HighlightRender;
            FilteringSettings filterSettings = new FilteringSettings(RenderQueueRange.all) { layerMask = m_MaskData.renderMask };
            _context.DrawRenderers(_renderingData.cullResults, ref drawingSettings, ref filterSettings);

            buffer.Clear();
            buffer.SetRenderTarget(m_Renderer.cameraColorTarget);
            _context.ExecuteCommandBuffer(buffer);
            CommandBufferPool.Release(buffer);
        }

    }

}