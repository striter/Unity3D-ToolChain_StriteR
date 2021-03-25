using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rendering.ImageEffect;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Rendering
{
    public class SRF_PerCameraPostProcessing : ScriptableRendererFeature
    {
        SRP_PerCameraPostProcessingRenderPass m_OpaquePostProcessingPass;
        SRP_PerCameraPostProcessingRenderPass m_AfterAllPostProcessingPass;
        public override void Create()
        {
            m_OpaquePostProcessingPass = new SRP_PerCameraPostProcessingRenderPass("Opaque Post Process") { renderPassEvent = RenderPassEvent.AfterRenderingSkybox };
            m_AfterAllPostProcessingPass = new SRP_PerCameraPostProcessingRenderPass("After All Post Process") { renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing };
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            var postEffects = renderingData.cameraData.camera.GetComponents<APostEffectBase>().FindAll(p => p.enabled);
            if (postEffects.Count() == 0)
                return;

            var dictionary = postEffects.ToListDictionary(p => p.m_IsOpaqueProcess);

            foreach (var key in dictionary.Keys)
            {
                int count = dictionary[key].Count();
                if (count == 0)
                    continue;

                if (key)
                    renderer.EnqueuePass(m_OpaquePostProcessingPass.Setup(renderer.cameraColorTarget, dictionary[key]));
                else
                    renderer.EnqueuePass(m_AfterAllPostProcessingPass.Setup(renderer.cameraColorTarget, dictionary[key]));
            }
        }
    }

    public class SRP_PerCameraPostProcessingRenderPass : ScriptableRenderPass
    {
        static readonly int ID_Blit_Temp = Shader.PropertyToID("_PostProcessing_Blit_Temp");
        string m_Name;
        RenderTargetIdentifier m_SrcTarget;
        RenderTargetIdentifier m_BlitTarget;
        IEnumerable<APostEffectBase> m_Effects;
        public SRP_PerCameraPostProcessingRenderPass(string _name) : base()
        {
            m_Name = _name;
            m_BlitTarget = new RenderTargetIdentifier(ID_Blit_Temp);
        }
        public SRP_PerCameraPostProcessingRenderPass Setup(RenderTargetIdentifier _srcTarget, IEnumerable<APostEffectBase> _effects)
        {
            m_SrcTarget = _srcTarget;
            m_Effects = _effects;
            return this;
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(m_Name);
            cmd.GetTemporaryRT(ID_Blit_Temp, renderingData.cameraData.cameraTargetDescriptor.width, renderingData.cameraData.cameraTargetDescriptor.height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
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

