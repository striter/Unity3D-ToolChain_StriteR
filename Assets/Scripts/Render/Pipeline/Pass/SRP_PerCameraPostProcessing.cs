using System.Collections;
using System.Collections.Generic;
using Rendering.ImageEffect;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Rendering.Pipeline
{
    public class SRP_PerCameraPostProcessing : ScriptableRenderPass, ISRPBase
    {
        static readonly int ID_Blit_Temp = Shader.PropertyToID("_PostProcessing_Blit_Temp");
        string m_Name;
        ScriptableRenderer m_Renderer;
        RenderTargetIdentifier m_BlitTarget;
        IEnumerable<APostProcessBase> m_Effects;
        public SRP_PerCameraPostProcessing(string _name) : base()
        {
            m_Name = _name;
            m_BlitTarget = new RenderTargetIdentifier(ID_Blit_Temp);
        }
        public void Dispose()
        {

        }

        public SRP_PerCameraPostProcessing Setup(ScriptableRenderer _renderer, IEnumerable<APostProcessBase> _effects)
        {
            m_Renderer = _renderer;
            m_Effects = _effects;
            return this;
        }
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            base.Configure(cmd, cameraTextureDescriptor);
            foreach (var effect in m_Effects)
                effect.Configure(m_Renderer, cmd, cameraTextureDescriptor, this);
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(m_Name);
            cmd.GetTemporaryRT(ID_Blit_Temp, renderingData.cameraData.cameraTargetDescriptor);
            bool blitSrc = true;
            foreach (var effect in m_Effects)
            {
                string name = effect.GetType().Name;
                cmd.BeginSample(name);
                RenderTargetIdentifier src = blitSrc ? m_Renderer.cameraColorTarget : m_BlitTarget;
                RenderTargetIdentifier dst = blitSrc ? m_BlitTarget : m_Renderer.cameraColorTarget;
                effect.Execute(m_Renderer, context, ref renderingData);
                effect.ExecuteBuffer(cmd, src, dst,renderingData.cameraData.cameraTargetDescriptor);
                blitSrc = !blitSrc;
                cmd.EndSample(name);
            }
            if (!blitSrc)
                cmd.Blit(m_BlitTarget, m_Renderer.cameraColorTarget);
            cmd.ReleaseTemporaryRT(ID_Blit_Temp);
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