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
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {            
            CommandBuffer cmd = CommandBufferPool.Get("Component Based Post Process");
            var descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.msaaSamples = 1;
            cmd.GetTemporaryRT(ID_Blit_Temp1, descriptor);
            cmd.GetTemporaryRT(ID_Blit_Temp2, descriptor);
            var renderer = renderingData.cameraData.renderer;
            int lastIndex = m_Effects.Count - 1;
            int blitIndex = 0;
            bool blitSwap = true;
            foreach (var effect in m_Effects)
            {
                RenderTargetIdentifier src = blitSwap ? m_BlitTemp1 : m_BlitTemp2;
                RenderTargetIdentifier dst = blitSwap ? m_BlitTemp2 : m_BlitTemp1;
                if (blitIndex == 0)
                    src = renderer.cameraColorTargetHandle;
                else if (blitIndex == lastIndex)
                    dst = renderer.cameraColorTargetHandle;
                blitSwap = !blitSwap;

                string name = effect.m_Name;
                cmd.BeginSample(name);
                effect.Execute(cmd, src, dst, descriptor,renderer, context, ref renderingData);
                cmd.EndSample(name);
                
                blitIndex++;
            }
            if (lastIndex == 0)
                cmd.Blit(m_BlitTemp2, renderer.cameraColorTargetHandle);
            
            cmd.ReleaseTemporaryRT(ID_Blit_Temp1);
            cmd.ReleaseTemporaryRT(ID_Blit_Temp2);
            context.ExecuteCommandBuffer(cmd);
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