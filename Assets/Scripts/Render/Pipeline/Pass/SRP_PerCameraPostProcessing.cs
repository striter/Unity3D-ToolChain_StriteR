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
        RenderTargetIdentifier m_SrcTarget;
        RenderTargetIdentifier m_BlitTarget;
        IEnumerable<APostEffectBase> m_Effects;
        public SRP_PerCameraPostProcessing(string _name) : base()
        {
            m_Name = _name;
            m_BlitTarget = new RenderTargetIdentifier(ID_Blit_Temp);
        }
        public void Dispose()
        {

        }

        public SRP_PerCameraPostProcessing Setup(RenderTargetIdentifier _srcTarget, IEnumerable<APostEffectBase> _effects)
        {
            m_SrcTarget = _srcTarget;
            m_Effects = _effects;
            return this;
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(m_Name);
            cmd.GetTemporaryRT(ID_Blit_Temp, renderingData.cameraData.cameraTargetDescriptor);
            bool blitSrc = true;
            foreach (var effect in m_Effects)
            {
                RenderTargetIdentifier src = blitSrc ? m_SrcTarget : m_BlitTarget;
                RenderTargetIdentifier dst = blitSrc ? m_BlitTarget : m_SrcTarget;
                effect.ExecutePostProcess(cmd, renderingData.cameraData.cameraTargetDescriptor, src, dst);
                blitSrc = !blitSrc;
            }
            if (!blitSrc)
                cmd.Blit(m_BlitTarget, m_SrcTarget);
            cmd.ReleaseTemporaryRT(ID_Blit_Temp);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }

}