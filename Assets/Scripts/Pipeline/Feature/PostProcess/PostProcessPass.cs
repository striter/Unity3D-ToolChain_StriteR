using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Rendering.Pipeline
{
    using PostProcess;
    public class PostProcessPass : ScriptableRenderPass
    {
        private static readonly int ID_Blit_Temp1 = Shader.PropertyToID("_PostProcessing_Blit_Temp1");
        private static readonly int ID_Blit_Temp2 = Shader.PropertyToID("_PostProcessing_Blit_Temp2");
        private static readonly RenderTargetIdentifier m_BlitTemp1 = new RenderTargetIdentifier(ID_Blit_Temp1);
        private static readonly RenderTargetIdentifier m_BlitTemp2 = new RenderTargetIdentifier(ID_Blit_Temp2);
        private List<IPostProcessBehaviour>  m_Effects;
        
        public PostProcessPass Setup(List<IPostProcessBehaviour> effects)
        {
            m_Effects = effects;
            m_Effects.Sort((a, b) => a.Event - b.Event);
            return this;
        }

        public void Dispose()
        {
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            base.Configure(cmd, cameraTextureDescriptor);
            foreach (var effect in m_Effects)
                effect.Configure(cmd, cameraTextureDescriptor);
            ConfigureTarget(colorAttachmentHandle,depthAttachmentHandle);
        }
        public override void Execute(ScriptableRenderContext _context, ref RenderingData _renderingData)
        {
            var cmd = CommandBufferPool.Get("Post Process");
            var descriptor = _renderingData.cameraData.cameraTargetDescriptor;
            descriptor.msaaSamples = 1;
            descriptor.depthBufferBits = 0;
            var colorTarget = _renderingData.cameraData.renderer.cameraColorTargetHandle;
            var buffer2Required = m_Effects.Count > 2; 
            cmd.GetTemporaryRT(ID_Blit_Temp1, descriptor);
            if(buffer2Required)
                cmd.GetTemporaryRT(ID_Blit_Temp2, descriptor);
            var lastIndex = m_Effects.Count - 1;
            var blitIndex = 0;
            var swap = false;
            foreach (var effect in m_Effects)
            {
                var src = swap ? m_BlitTemp1 : m_BlitTemp2;
                var dst = swap ? m_BlitTemp2 : m_BlitTemp1;
                if (blitIndex == 0)
                    src = colorTarget;
                else if (blitIndex == lastIndex)
                    dst = colorTarget;
                swap = !swap;

                var name = effect.GetType().Name;
                cmd.BeginSample(name);
                effect.Execute(cmd, src, dst, descriptor, _context, ref _renderingData);
                cmd.EndSample(name);
            
                blitIndex++;
            }
            if(lastIndex == 0)
                cmd.Blit(m_BlitTemp1, colorTarget);
            
            cmd.ReleaseTemporaryRT(ID_Blit_Temp2);
            if(buffer2Required)
                cmd.ReleaseTemporaryRT(ID_Blit_Temp1);
            
            _context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
        public override void FrameCleanup(CommandBuffer cmd)
        {
            base.FrameCleanup(cmd);
            foreach (var effect in m_Effects)
                effect.FrameCleanUp(cmd);
        }
    }
}