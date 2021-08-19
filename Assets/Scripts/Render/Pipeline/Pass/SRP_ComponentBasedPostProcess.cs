using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Rendering.Pipeline
{
    using PostProcess;
    public class SRP_ComponentBasedPostProcess : ScriptableRenderPass, ISRPBase
    {
        private static readonly int ID_Blit_Temp1 = Shader.PropertyToID("_PostProcessing_Blit_Temp1");
        private static readonly int ID_Blit_Temp2 = Shader.PropertyToID("_PostProcessing_Blit_Temp2");
        private static readonly RenderTargetIdentifier m_BlitTemp1 = new RenderTargetIdentifier(ID_Blit_Temp1);
        private static readonly RenderTargetIdentifier m_BlitTemp2 = new RenderTargetIdentifier(ID_Blit_Temp2);
        private string m_Name;
        private List<APostProcessBase>  m_Effects;
        public void Dispose()
        {

        }

        public SRP_ComponentBasedPostProcess Setup(IEnumerable<APostProcessBase> effect)
        {
            m_Effects = effect.ToList();
            m_Effects.Sort((a, b) => (int) (a.Event - b.Event));
            return this;
        }
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            base.Configure(cmd, cameraTextureDescriptor);
            foreach (var effect in m_Effects)
                effect.Configure(cmd, cameraTextureDescriptor);
            ConfigureTarget(colorAttachment,depthAttachment);
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("Component Based Post Process");
            var descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.msaaSamples = 1;
            cmd.GetTemporaryRT(ID_Blit_Temp1, descriptor);
            cmd.GetTemporaryRT(ID_Blit_Temp2, descriptor);
            var renderer = renderingData.cameraData.renderer;
            int finalIndex = m_Effects.Count() - 1;
            int blitIndex = 0;
            bool blitSwap = false;
            foreach (var effect in m_Effects)
            {
                bool firstBlit = blitIndex == 0;
                bool finalBlit = blitIndex == finalIndex;
                blitIndex++;
            
                RenderTargetIdentifier src = blitSwap ? m_BlitTemp1 : m_BlitTemp2;
                RenderTargetIdentifier dst = blitSwap ? m_BlitTemp2 : m_BlitTemp1;
                if (firstBlit)
                    src = renderer.cameraColorTarget;
                else if (finalBlit)
                    dst = renderer.cameraColorTarget;
                blitSwap = !blitSwap;
            
                string name = effect.GetType().Name;
                cmd.BeginSample(name);
                effect.ExecuteContext(renderer, context, ref renderingData);
                effect.ExecuteBuffer(cmd, src, dst, descriptor);
                cmd.EndSample(name);
            }
            if (blitIndex == 1)
                cmd.Blit(m_BlitTemp1, renderer.cameraColorTarget);
            
            cmd.ReleaseTemporaryRT(ID_Blit_Temp1);
            cmd.ReleaseTemporaryRT(ID_Blit_Temp2);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
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