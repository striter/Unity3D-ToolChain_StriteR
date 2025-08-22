using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

namespace Rendering.Pipeline
{
    //https://developer.nvidia.com/gpugems/gpugems3/part-iv-image-effects/chapter-23-high-speed-screen-particles
    [Serializable]
    public struct OffScreenParticleData
    {
        public CullingMask layerMask;
        public EDownSample downSample;
        public bool maxDepth;
        public Shader blendShader;
        
        public bool Valid => layerMask != 0 && downSample != EDownSample.None && blendShader != null;

        public static readonly OffScreenParticleData kDefault = new()
        {
            layerMask = -1,
            downSample = EDownSample.Quarter,
            maxDepth = false,
        };
    }

    public class OffScreenParticleFeature : ScriptableRendererFeature
    {
        public OffScreenParticleData m_Data = OffScreenParticleData.kDefault;
        private OffScreenParticlePass m_Pass;

        public override void Create()
        {
            m_Pass = new OffScreenParticlePass() { renderPassEvent = RenderPassEvent.AfterRenderingTransparents };
        }

        public override void OnCameraPreCull(ScriptableRenderer renderer, in CameraData cameraData)
        {
            if (!m_Data.Valid)
                return;

            base.OnCameraPreCull(renderer, in cameraData);
            m_Pass.OnPreCull(renderer, in cameraData);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!m_Data.Valid)
                return;

            renderer.EnqueuePass(m_Pass.Setup(m_Data));
        }

    }

    public class OffScreenParticlePass : ScriptableRenderPass
    {
        private static readonly string kTitle = "Off Screen Particle";
        private OffScreenParticleData m_Data;
        private int m_SrcLayerMask;
        private static readonly string kColorTextureName = "_OffScreenParticleTexture";
        private static readonly int kColorTextureID = Shader.PropertyToID(kColorTextureName);
        private static readonly RenderTargetIdentifier kColorTextureRT = new(kColorTextureID);
        private RTHandle m_ColorHandle;
        private ScriptableCullingParameters m_CullParameters;
        private Material m_BlendMaterial;

        private DrawObjectsPass m_DrawTransparentsPass;
        private FilteringSettings m_SrcFilterSettings;
        private FilteringSettings m_VDMFilterSettings;

        public OffScreenParticlePass Setup(OffScreenParticleData _data)
        {
            m_BlendMaterial = _data.blendShader ? CoreUtils.CreateEngineMaterial(_data.blendShader) : null;
            m_Data = _data;
            return this;
        }

        public void OnPreCull(ScriptableRenderer renderer, in CameraData cameraData)
        {
            m_DrawTransparentsPass = (DrawObjectsPass)UDebug.GetFieldValue(renderer, "m_RenderTransparentForwardPass");
            m_SrcFilterSettings =
                (FilteringSettings)UDebug.GetFieldValue(m_DrawTransparentsPass, "m_FilteringSettings");
            var filterSetting = m_SrcFilterSettings;
            filterSetting.layerMask &= ~m_Data.layerMask;
            UDebug.SetFieldValue(m_DrawTransparentsPass, "m_FilteringSettings", filterSetting);
            m_VDMFilterSettings = filterSetting;
            m_VDMFilterSettings.layerMask = m_Data.layerMask;
            if (cameraData.camera.TryGetCullingParameters(out m_CullParameters))
                m_CullParameters.cullingMask = (uint)(int)m_Data.layerMask;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            base.Configure(cmd, cameraTextureDescriptor);
            var downSize = (int)m_Data.downSample;
            cameraTextureDescriptor.width /= downSize;
            cameraTextureDescriptor.height /= downSize;
            cameraTextureDescriptor.colorFormat = GraphicsFormatUtility.IsHDRFormat(cameraTextureDescriptor.graphicsFormat) ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32;
            cmd.GetTemporaryRT(kColorTextureID, cameraTextureDescriptor);

            m_ColorHandle?.Release();
            m_ColorHandle = RTHandles.Alloc(kColorTextureRT);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            base.FrameCleanup(cmd);
            cmd.ReleaseTemporaryRT(kColorTextureID);
            m_ColorHandle?.Release();
            m_ColorHandle = null;
            UDebug.SetFieldValue(m_DrawTransparentsPass, "m_FilteringSettings", m_SrcFilterSettings);
        }
        
        private static readonly int kDownSample = Shader.PropertyToID("_DownSample");
        private static string kMaxDepthKeyword = "_MAX_DEPTH";
        public override void Execute(ScriptableRenderContext _context, ref RenderingData _renderingData)
        {
            var cmd = CommandBufferPool.Get(kTitle);
            cmd.BeginSample(kColorTextureName);
            cmd.SetRenderTarget(m_ColorHandle, m_ColorHandle);

            m_BlendMaterial.SetInt(kDownSample, (int)m_Data.downSample);
            m_BlendMaterial.EnableKeyword(kMaxDepthKeyword,m_Data.maxDepth);
            
            cmd.DrawMesh(UPipeline.kFullscreenMesh, Matrix4x4.identity, m_BlendMaterial, 0, 0); //Blit Depth

            _context.ExecuteCommandBuffer(cmd);
            var drawingSettings = UPipeline.CreateDrawingSettings(true, _renderingData.cameraData.camera,
                SortingCriteria.CommonTransparent);
            drawingSettings.perObjectData = PerObjectData.None;
            _context.DrawRenderers(_context.Cull(ref m_CullParameters), ref drawingSettings, ref m_VDMFilterSettings);

            cmd.Clear();
            cmd.SetRenderTarget(_renderingData.cameraData.renderer.cameraColorTargetHandle,
                RenderBufferLoadAction.Load, RenderBufferStoreAction.Store,
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
            cmd.DrawMesh(UPipeline.kFullscreenMesh, Matrix4x4.identity, m_BlendMaterial, 0, 1); //Blit Color
            cmd.EndSample(kColorTextureName);
            _context.ExecuteCommandBuffer(cmd);
        }
    }
}