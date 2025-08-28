using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

namespace Rendering.Pipeline
{
    //https://developer.nvidia.com/gpugems/gpugems3/part-iv-image-effects/chapter-23-high-speed-screen-particles
    //https://advances.realtimerendering.com/s2013/Tatarchuk-Destiny-SIGGRAPH2013.pdf
    [Serializable]
    public struct OffScreenParticleData
    {
        [DefaultAsset("Hidden/OffScreenParticle")] public Shader blendShader;
        public CullingMask layerMask;
        public EDownSample downSample;
        public bool maxDepth;
        public bool varianceDepthMaps;
        
        public bool Valid => layerMask != 0 && downSample != EDownSample.None && blendShader != null;

        public static readonly OffScreenParticleData kDefault = new()
        {
            layerMask = -1,
            downSample = EDownSample.Quarter,
            maxDepth = false,
            varianceDepthMaps = false,
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

    public enum EOffScreenParticleShaderPass
    {
        _ClearAndDownSample = 0,
        _Blend = 1,
        _VDMParticleRenderer = 2,
    }

    public class OffScreenParticlePass : ScriptableRenderPass
    {
        private static readonly string kTitle = "Off Screen Particle";
        private OffScreenParticleData m_Data;
        private int m_SrcLayerMask;
        private static readonly string kColorTextureName = "_OffScreenParticleTexture";
        private static readonly int kColorTextureID = Shader.PropertyToID(kColorTextureName);
        private static readonly RenderTargetIdentifier kColorTextureRT = new(kColorTextureID);

        private static string kVDMKeyword = "_VDM";
        private static readonly string kVDMDepthTextureName = "_VDMDepthTexture";
        private static readonly int kVDMDepthID = Shader.PropertyToID(kVDMDepthTextureName);
        private RTHandle m_VDMDepthHandle;
        
        private RTHandle m_ColorHandle;
        private ScriptableCullingParameters m_CullParameters;
        private Material m_BlendMaterial;
        
        private DrawObjectsPass m_DrawTransparentsPass;
        private FilteringSettings m_SrcFilteringSettings;
        private FilteringSettings m_FilteringSettings;

        public OffScreenParticlePass Setup(OffScreenParticleData _data)
        {
            m_BlendMaterial = _data.blendShader ? CoreUtils.CreateEngineMaterial(_data.blendShader) : null;
            m_Data = _data;
            return this;
        }

        public void OnPreCull(ScriptableRenderer renderer, in CameraData cameraData)
        {
            m_DrawTransparentsPass = (DrawObjectsPass)UDebug.GetFieldValue(renderer, "m_RenderTransparentForwardPass");
            m_SrcFilteringSettings = (FilteringSettings)UDebug.GetFieldValue(m_DrawTransparentsPass, "m_FilteringSettings");
            var filterSetting = m_SrcFilteringSettings;
            filterSetting.layerMask &= ~m_Data.layerMask;
            UDebug.SetFieldValue(m_DrawTransparentsPass, "m_FilteringSettings", filterSetting);
            m_FilteringSettings = filterSetting;
            m_FilteringSettings.layerMask = m_Data.layerMask;
            if (cameraData.camera.TryGetCullingParameters(out m_CullParameters))
                m_CullParameters.cullingMask = (uint)(int)m_Data.layerMask;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            base.Configure(cmd, cameraTextureDescriptor);
            var downSize = (int)m_Data.downSample;
            cameraTextureDescriptor.msaaSamples = 1;
            cameraTextureDescriptor.width /= downSize;
            cameraTextureDescriptor.height /= downSize;
            cameraTextureDescriptor.colorFormat = GraphicsFormatUtility.IsHDRFormat(cameraTextureDescriptor.graphicsFormat) ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32;
            cmd.GetTemporaryRT(kColorTextureID, cameraTextureDescriptor,FilterMode.Point);

            cameraTextureDescriptor.depthBufferBits = 0;
            cameraTextureDescriptor.colorFormat = RenderTextureFormat.ARGBFloat;
            cmd.GetTemporaryRT(kVDMDepthID, cameraTextureDescriptor,FilterMode.Point);
            
            m_ColorHandle?.Release();
            m_ColorHandle = RTHandles.Alloc(kColorTextureRT);
            m_VDMDepthHandle?.Release();
            m_VDMDepthHandle = RTHandles.Alloc(kVDMDepthID);
            
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            base.FrameCleanup(cmd);
            cmd.ReleaseTemporaryRT(kColorTextureID);
            cmd.ReleaseTemporaryRT(kVDMDepthID);
            m_ColorHandle?.Release();
            m_ColorHandle = null;
            m_VDMDepthHandle?.Release();
            m_VDMDepthHandle = null;
            
            UDebug.SetFieldValue(m_DrawTransparentsPass, "m_FilteringSettings", m_SrcFilteringSettings);
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
            m_BlendMaterial.EnableKeyword(kVDMKeyword, m_Data.varianceDepthMaps);

            cmd.SetGlobalVector(KShaderProperties.kOutputTexelSize,_renderingData.cameraData.cameraTargetDescriptor.GetTexelSize((int)m_Data.downSample));
            cmd.DrawMesh(UPipeline.kFullscreenMesh, Matrix4x4.identity, m_BlendMaterial, 0, (int)EOffScreenParticleShaderPass._ClearAndDownSample); //Blit Depth
            _context.ExecuteCommandBuffer(cmd);
            
            var particlesToRender = _context.Cull(ref m_CullParameters);

            if (m_Data.varianceDepthMaps)
            {
                var vdmCommand = CommandBufferPool.Get(kVDMKeyword);
                vdmCommand.SetRenderTarget(m_VDMDepthHandle);
                vdmCommand.BeginSample(kVDMDepthTextureName);
                vdmCommand.ClearRenderTarget(true,true,Color.black);
                _context.ExecuteCommandBuffer(vdmCommand);
                
                var vdmDrawingSettings = UPipeline.CreateDrawingSettings(true, _renderingData.cameraData.camera, SortingCriteria.CommonTransparent);
                vdmDrawingSettings.overrideMaterial = m_BlendMaterial;
                vdmDrawingSettings.overrideMaterialPassIndex = (int)EOffScreenParticleShaderPass._VDMParticleRenderer;
                _context.DrawRenderers(particlesToRender, ref vdmDrawingSettings, ref m_FilteringSettings);
                
                vdmCommand.Clear();
                vdmCommand.EndSample(kVDMDepthTextureName);
                vdmCommand.SetRenderTarget(m_ColorHandle, m_ColorHandle);
                _context.ExecuteCommandBuffer(vdmCommand);
                CommandBufferPool.Release(vdmCommand);
            }
            
            var drawingSettings = UPipeline.CreateDrawingSettings(true, _renderingData.cameraData.camera, SortingCriteria.CommonTransparent);
            drawingSettings.perObjectData = PerObjectData.None;
            _context.DrawRenderers(particlesToRender, ref drawingSettings, ref m_FilteringSettings);
            
            cmd.Clear();
            cmd.SetRenderTarget(_renderingData.cameraData.renderer.cameraColorTargetHandle,
                RenderBufferLoadAction.Load, RenderBufferStoreAction.Store,
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
            cmd.DrawMesh(UPipeline.kFullscreenMesh, Matrix4x4.identity, m_BlendMaterial, 0,(int)EOffScreenParticleShaderPass._Blend); //Blit Color
            cmd.EndSample(kColorTextureName);
            _context.ExecuteCommandBuffer(cmd);
            
            CommandBufferPool.Release(cmd);
        }
    }
}