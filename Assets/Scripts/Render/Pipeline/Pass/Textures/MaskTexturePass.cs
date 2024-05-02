using System;
using Rendering.Pipeline;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

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

        public bool inheritDepth;
        public bool outline;
        [MFoldout(nameof(outline),true)] [Range(0,1)]public float extendWidth;
        [MFoldout(nameof(outline),true)] public EOutlineVertex outlineVertex;
        public static readonly SRD_MaskData kDefault = new SRD_MaskData()
        {
            renderMask=int.MaxValue,
            color = Color.white,
            outline = false,
            extendWidth = 0.1f,
            inheritDepth = true,
            outlineVertex  = EOutlineVertex._NORMALSAMPLE_NORMAL,
        };
    }
    
    public class MaskTexturePass : ScriptableRenderPass,ISRPBase
    {
        private SRD_MaskData m_Data;

        private readonly PassiveInstance<Material> m_OutlineRenderer = new PassiveInstance<Material>(() => new Material(RenderResources.FindInclude("Game/Additive/Outline")) { hideFlags = HideFlags.HideAndDontSave },GameObject.DestroyImmediate);
        private readonly PassiveInstance<Material> m_NormalRenderer = new PassiveInstance<Material>(() => new Material(RenderResources.FindInclude("Game/Unlit/Color")) { hideFlags = HideFlags.HideAndDontSave },GameObject.DestroyImmediate);

        public MaskTexturePass Setup(SRD_MaskData _data)
        {
            m_Data = _data;
            var renderer = _data.outline ? m_OutlineRenderer : m_NormalRenderer;
            if (_data.outline)
            {
                renderer.Value.SetColor("_OutlineColor",m_Data.color);
                renderer.Value.SetFloat("_OutlineWidth",m_Data.extendWidth);
            }
            else
            {
                renderer.Value.SetColor(KShaderProperties.kColor,m_Data.color);
            }
            renderer.Value.EnableKeywords(m_Data.outlineVertex);
            renderer.Value.SetInt(KShaderProperties.kCull,(int)CullMode.Off);
            renderer.Value.SetInt(KShaderProperties.kColorMask,(int)ColorWriteMask.All);
            renderer.Value.SetInt(KShaderProperties.kZWrite,1);
            return this;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cameraTextureDescriptor.colorFormat = RenderTextureFormat.R8;
            cameraTextureDescriptor.depthBufferBits = 0;
            cmd.GetTemporaryRT(KRenderTextures.kCameraMaskTexture, cameraTextureDescriptor);
            ConfigureTarget(RTHandles.Alloc(KRenderTextures.kCameraMaskTexture));
            base.Configure(cmd, cameraTextureDescriptor);
        }
        public override void FrameCleanup(CommandBuffer _cmd)
        {
            base.FrameCleanup(_cmd);
            _cmd.ReleaseTemporaryRT(KRenderTextures.kCameraMaskTexture);
        }


        public void Dispose()
        {
            m_OutlineRenderer.Dispose();
            m_NormalRenderer.Dispose();
        }

        public override void Execute(ScriptableRenderContext _context, ref RenderingData _renderingData)
        {                
            CommandBuffer buffer = CommandBufferPool.Get("Camera Mask Texture");
            if(m_Data.inheritDepth)
                buffer.SetRenderTarget(KRenderTextures.kCameraMaskTextureRT,  _renderingData.cameraData.renderer.cameraDepthTargetHandle);
            else
                buffer.SetRenderTarget(KRenderTextures.kCameraMaskTextureRT);
            buffer.ClearRenderTarget(false, true, Color.black);
            _context.ExecuteCommandBuffer(buffer);

            DrawingSettings drawingSettings = UPipeline.CreateDrawingSettings(true, _renderingData.cameraData.camera);
            drawingSettings.overrideMaterial = m_Data.outline ? m_OutlineRenderer : m_NormalRenderer;
            FilteringSettings filterSettings = new FilteringSettings(RenderQueueRange.all) { layerMask = m_Data.renderMask };
            _context.DrawRenderers(_renderingData.cullResults, ref drawingSettings, ref filterSettings);

            buffer.Clear();
            buffer.SetRenderTarget(_renderingData.cameraData.renderer.cameraColorTargetHandle);
            _context.ExecuteCommandBuffer(buffer);
            CommandBufferPool.Release(buffer);
        }

    }

}