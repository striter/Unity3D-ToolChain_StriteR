using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.Pipeline
{
    public class SRF_MultiPass : ScriptableRendererFeature
    {
        public string m_PassName;
        public RenderPassEvent m_Event= RenderPassEvent.AfterRenderingTransparents;
        public PerObjectData m_PerObjectData;
        [CullingMask] public int m_Layermask;
        SRP_MultiPass m_MultiPass;
        public override void Create()
        {
            m_MultiPass = new SRP_MultiPass(m_PassName,m_Layermask,m_PerObjectData) {
                renderPassEvent = m_Event,
            };
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.isPreviewCamera)
                return;
            renderer.EnqueuePass(m_MultiPass);
        }
    }
    public class SRP_MultiPass:ScriptableRenderPass
    {
        private string m_PassName;
        private int m_LayerMask;
        private PerObjectData m_PerObjectPerObjectData;

        public SRP_MultiPass(string _passes,int _layerMask,PerObjectData _perObjectData)
        {
            m_PassName = _passes;
            m_LayerMask = _layerMask;
            m_PerObjectPerObjectData = _perObjectData;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            FilteringSettings filterSettings = new FilteringSettings( RenderQueueRange.all,m_LayerMask);
            DrawingSettings drawingSettings = new DrawingSettings  {
                sortingSettings = new SortingSettings(renderingData.cameraData.camera),
                enableDynamicBatching = true,
                enableInstancing = true,
                perObjectData = m_PerObjectPerObjectData,
            };
            drawingSettings.SetShaderPassName(0,new ShaderTagId(m_PassName));
            context.DrawRenderers(renderingData.cullResults,ref drawingSettings,ref filterSettings);
        }
    }
}