using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.Pipeline
{
    public class LightVolumeFeature : ScriptableRendererFeature
    {
        private LightVolumePass m_LightMaskPass;
        public override void Create()
        {
            m_LightMaskPass = new LightVolumePass(){renderPassEvent=RenderPassEvent.BeforeRenderingOpaques};
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.isPreviewCamera)
                return;
            renderer.EnqueuePass(m_LightMaskPass.Setup(renderer));
        }
    }
    
    public class LightVolumePass : ScriptableRenderPass
    {
        private static readonly string kKeyword = "_LIGHTVOLUME";
        private ScriptableRenderer m_Renderer;
        public LightVolumePass Setup(ScriptableRenderer _renderer)
        {
            m_Renderer = _renderer;
            return this;
        }
        
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cameraTextureDescriptor.colorFormat = RenderTextureFormat.ARGB32;
            cameraTextureDescriptor.depthBufferBits = 0;
            cmd.GetTemporaryRT(KRenderTextures.kCameraLightMask, cameraTextureDescriptor);
            cmd.EnableShaderKeyword(kKeyword);
            ConfigureTarget( RTHandles.Alloc(KRenderTextures.kCameraLightMaskRT));
            base.Configure(cmd, cameraTextureDescriptor);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            base.FrameCleanup(cmd);
            cmd.DisableShaderKeyword(kKeyword);
            cmd.ReleaseTemporaryRT(KRenderTextures.kCameraLightMask);
        }
        
        
        public override void Execute(ScriptableRenderContext _context, ref RenderingData _renderingData)
        {                
            CommandBuffer buffer = CommandBufferPool.Get("Camera Light Mask");
            buffer.SetRenderTarget(KRenderTextures.kCameraLightMaskRT,m_Renderer.cameraDepthTargetHandle);
            buffer.ClearRenderTarget(false, true, Color.black);
            _context.ExecuteCommandBuffer(buffer);

            var tag = new ShaderTagId("LightVolume");
            DrawingSettings drawingSettings = UPipeline.CreateDrawingSettings( false,_renderingData.cameraData.camera);
            drawingSettings.SetShaderPassName(0,tag);
            FilteringSettings filterSettings = new FilteringSettings(RenderQueueRange.all) { layerMask = int.MaxValue };
            _context.DrawRenderers(_renderingData.cullResults, ref drawingSettings, ref filterSettings);

            buffer.Clear();
            buffer.SetRenderTarget(m_Renderer.cameraColorTargetHandle,m_Renderer.cameraDepthTargetHandle);
            _context.ExecuteCommandBuffer(buffer);
            CommandBufferPool.Release(buffer);
        }

    }

}