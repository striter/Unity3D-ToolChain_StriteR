using System;
using System.Drawing.Drawing2D;
using Rendering.PostProcess;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.Pipeline
{
    
    class ScreenSpaceReflectionPass : ScriptableRenderPass
    {
        private readonly PassiveInstance<Shader> m_ReflectionBlit = new PassiveInstance<Shader>(()=>RenderResources.FindInclude("Hidden/ScreenSpaceReflection"));
        private readonly Material m_Material;
        private ScreenSpaceReflectionData m_Data;
        private RTHandle kSSRHandle;

        public ScreenSpaceReflectionPass(ScreenSpaceReflectionData _data)
        {
            m_Data = _data;
            m_Material = new Material(m_ReflectionBlit){hideFlags = HideFlags.HideAndDontSave};
        }

        public override void Configure(CommandBuffer _cmd, RenderTextureDescriptor _cameraTextureDescriptor)
        {
            _cameraTextureDescriptor.width /= m_Data.downSample;
            _cameraTextureDescriptor.width /= m_Data.downSample;
            kSSRHandle = RTHandles.Alloc(_cameraTextureDescriptor,FilterMode.Bilinear,TextureWrapMode.Clamp,false,1,0f,"_ScreenSpaceReflection");
            ConfigureTarget(kSSRHandle);
            base.Configure(_cmd, _cameraTextureDescriptor);
        }

        public override void OnCameraCleanup(CommandBuffer _cmd)
        {
            base.OnCameraCleanup(_cmd);
            RTHandles.Release(kSSRHandle);
        }

        public override void Execute(ScriptableRenderContext _context, ref RenderingData _renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("Screen Space Reflection Texture");
            cmd.Blit(_renderingData.cameraData.renderer.cameraColorTargetHandle,kSSRHandle,m_Material);
            
            _context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
        
    }
}