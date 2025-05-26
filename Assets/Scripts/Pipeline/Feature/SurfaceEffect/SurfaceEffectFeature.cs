using Rendering.Pipeline.Component;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.Pipeline
{
    public class SurfaceEffectFeature : AScriptableRendererFeature
    {
        public RenderPassEvent m_Event = RenderPassEvent.AfterRenderingTransparents;
        SurfaceEffectPass m_Passes;
        public override void Create()
        {
            m_Passes = new SurfaceEffectPass { renderPassEvent = m_Event, };
        }
        protected override void EnqueuePass(ScriptableRenderer _renderer, ref RenderingData _renderingData)
        {
            if (_renderingData.cameraData.isPreviewCamera)
                return;
            _renderer.EnqueuePass(m_Passes);
        }
    }
    public class SurfaceEffectPass:ScriptableRenderPass
    {
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get("SurfaceEffectPass");
            foreach (var obj in ISurfaceEffect.kBehaviours)
            {
                foreach (var (renderer, material) in obj.GetSurfaceEffectDrawCalls(renderingData.cameraData.camera))
                {
                    for(var i=0;i<renderer.sharedMaterials.Length;i++)
                        cmd.DrawRenderer(renderer,material,i);
                }
            }
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}