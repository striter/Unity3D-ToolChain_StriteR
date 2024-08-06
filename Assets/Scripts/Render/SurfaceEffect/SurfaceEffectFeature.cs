using Rendering.Pipeline.Component;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.Pipeline
{
    public class SurfaceEffectFeature : ScriptableRendererFeature
    {
        public RenderPassEvent m_Event = RenderPassEvent.AfterRenderingTransparents;
        SurfaceEffectPass m_Passes;
        public override void Create()
        {
            m_Passes = new SurfaceEffectPass { renderPassEvent = m_Event, };
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.isPreviewCamera)
                return;
            renderer.EnqueuePass(m_Passes);
        }
    }
    public class SurfaceEffectPass:ScriptableRenderPass
    {
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get("SurfaceEffectPass");
            foreach (var obj in SurfaceEffectBehaviour.kBehaviours)
            {
                foreach (var (renderer, material) in obj.GetRenderers())
                {
                    for(int i=0;i<renderer.sharedMaterials.Length;i++)
                        cmd.DrawRenderer(renderer,material,i);
                }
            }
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}