using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Rendering.Pipeline
{
    using static KRenderTextures;

    public enum ENormalTextureMode
    {
        ReconstructFromDepth,
        ShaderReplacement,
        LightMode,
    }
    
    [Serializable]
    public struct NormalTexturePassData
    {
        public ENormalTextureMode mode;
        public EDownSample downSample;
        public static NormalTexturePassData kDefault => new NormalTexturePassData { mode = ENormalTextureMode.ReconstructFromDepth, downSample = EDownSample.None };
    }

    public class NormalTexturePass : ScriptableRenderPass
    {
        private PassiveInstance<Material> m_NormalFromDepthMaterial = new(()=>new Material( RenderResources.FindInclude("Hidden/NormalsFromDepth"))  {hideFlags = HideFlags.HideAndDontSave},GameObject.DestroyImmediate);
        private PassiveInstance<Shader> m_NormalShader = new(()=>RenderResources.FindInclude("Game/Additive/DepthNormals"));
        
        private NormalTexturePassData m_Data;
        private static readonly string kTitle = nameof(NormalTexturePass);
        private RTHandle m_CameraNormalTex;
        public NormalTexturePass Setup(NormalTexturePassData _data)
        {
            m_Data = _data;
            this.renderPassEvent = m_Data.mode == ENormalTextureMode.ReconstructFromDepth ? RenderPassEvent.AfterRenderingOpaques : RenderPassEvent.AfterRenderingPrePasses;
            return this;
        }
        
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            base.Configure(cmd, cameraTextureDescriptor);
            var downSample = (int)m_Data.downSample;
            cmd.GetTemporaryRT(kCameraNormalID, cameraTextureDescriptor.width / downSample, cameraTextureDescriptor.height / downSample, m_Data.mode == ENormalTextureMode.ReconstructFromDepth ? 0 : 16, FilterMode.Point, RenderTextureFormat.ARGB32);
            m_CameraNormalTex = RTHandles.Alloc(kCameraNormalID);
            ConfigureTarget(m_CameraNormalTex);
        }
        public override void FrameCleanup(CommandBuffer _cmd)
        {
            base.FrameCleanup(_cmd);
            _cmd.ReleaseTemporaryRT(kCameraNormalID);
            m_CameraNormalTex?.Release();
            m_CameraNormalTex = null;
        }
        public override void Execute(ScriptableRenderContext _context, ref RenderingData _renderingData)
        {
            var cmd = CommandBufferPool.Get(kTitle);
            cmd.BeginSample(kCameraNormalTexure);
            cmd.ClearRenderTarget(true, true, Color.black);
            _context.ExecuteCommandBuffer(cmd);

            cmd.Clear();
            switch (m_Data.mode)
            {
                case ENormalTextureMode.ReconstructFromDepth:
                {
                    cmd.DrawMesh(UPipeline.kFullscreenMesh, Matrix4x4.identity, m_NormalFromDepthMaterial);
                    _context.ExecuteCommandBuffer(cmd);
                }
                    break;
                case ENormalTextureMode.ShaderReplacement:
                {
                    var drawingSettings = UPipeline.CreateDrawingSettings(true, _renderingData.cameraData.camera,SortingCriteria.CommonOpaque);
                    drawingSettings.overrideShader = m_NormalShader;
                    drawingSettings.perObjectData = PerObjectData.None;
                    drawingSettings.enableDynamicBatching = true;
                    var filteringSettings = new FilteringSettings(RenderQueueRange.opaque, _renderingData.cameraData.camera.cullingMask);
                    _context.DrawRenderers(_renderingData.cullResults, ref drawingSettings, ref filteringSettings);
                    _context.ExecuteCommandBuffer(cmd);
                }
                    break;
                case ENormalTextureMode.LightMode:
                {
                    var drawingSettings = new DrawingSettings(new ShaderTagId("DepthNormals"), new SortingSettings(_renderingData.cameraData.camera){criteria = SortingCriteria.CommonOpaque});
                    var filteringSettings = new FilteringSettings(RenderQueueRange.opaque, _renderingData.cameraData.camera.cullingMask);
                    _context.DrawRenderers(_renderingData.cullResults, ref drawingSettings, ref filteringSettings);
                    _context.ExecuteCommandBuffer(cmd);
                }
                    break;
            }
            
            cmd.Clear();
            cmd.EndSample(kCameraNormalTexure);
            _context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
        public void Dispose()
        {
            m_NormalFromDepthMaterial.Dispose();
        }
    }
}