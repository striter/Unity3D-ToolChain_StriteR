using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.Pipeline
{
    public class SRF_MultiPass : ScriptableRendererFeature
    {
        public string m_PassName;
        public RenderPassEvent m_Event= RenderPassEvent.AfterRenderingTransparents;
        [CullingMask] public int m_Layermask;
        SRP_MultiPass m_OutlinePass ;
        public override void Create()
        {
            m_OutlinePass = new SRP_MultiPass() {
                m_OutlineTags = new ShaderTagId(m_PassName),
                renderPassEvent = m_Event,
                m_LayerMask = m_Layermask,
            };
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(m_OutlinePass);
        }
    }
    public class SRP_MultiPass:ScriptableRenderPass
    {
        public ShaderTagId m_OutlineTags;
        public int m_LayerMask;
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            DrawingSettings drawingSettings = CreateDrawingSettings(m_OutlineTags, ref renderingData,SortingCriteria.CommonOpaque);
            drawingSettings.enableDynamicBatching = true;
            drawingSettings.perObjectData = PerObjectData.None;

            FilteringSettings filterSettings = new FilteringSettings( RenderQueueRange.all,m_LayerMask);
            context.DrawRenderers(renderingData.cullResults,ref drawingSettings,ref filterSettings);
        }
    }
}