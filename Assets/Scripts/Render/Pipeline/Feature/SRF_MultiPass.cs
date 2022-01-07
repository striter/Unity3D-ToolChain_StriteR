using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.Pipeline
{
    public class SRF_MultiPass : ScriptableRendererFeature
    {
        public string m_PassName;
        [Tooltip("Used for recursive rendering")]public bool m_Shell;
        [MFoldout(nameof(m_Shell),true)][Range(0,32)] public int m_ShellCount;
        public RenderPassEvent m_Event= RenderPassEvent.AfterRenderingTransparents;
        public PerObjectData m_PerObjectData;
        [CullingMask] public int m_Layermask;
        SRP_MultiPass m_OutlinePass ;
        public override void Create()
        {
            m_OutlinePass = new SRP_MultiPass(m_PassName,m_Shell,m_ShellCount,m_Layermask,m_PerObjectData) {
                renderPassEvent = m_Event,
            };
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(m_OutlinePass);
        }
    }
    public class SRP_MultiPass:ScriptableRenderPass
    {
        private string m_PassName;
        private bool m_Shell;
        private int m_ShellCount;
        private int m_LayerMask;
        private PerObjectData m_PerObjectPerObjectData;

        public SRP_MultiPass(string _passes,bool _shell,int _count,int _layerMask,PerObjectData _perObjectData)
        {
            m_PassName = _passes;
            m_Shell = _shell;
            m_ShellCount = _count;
            m_LayerMask = _layerMask;
            m_PerObjectPerObjectData = _perObjectData;
        }

        private static readonly ShaderTagId kNull = new ShaderTagId("Null");
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            FilteringSettings filterSettings = new FilteringSettings( RenderQueueRange.all,m_LayerMask);
            DrawingSettings drawingSettings = new DrawingSettings  {
                sortingSettings = new SortingSettings(renderingData.cameraData.camera),
                enableDynamicBatching = true,
                enableInstancing = true,
                perObjectData = m_PerObjectPerObjectData,
            };
            if (!m_Shell)
            {
                drawingSettings.SetShaderPassName(0,new ShaderTagId(m_PassName));
                context.DrawRenderers(renderingData.cullResults,ref drawingSettings,ref filterSettings);
            }
            else
            {
                int totalDrawed = 0;
                while (totalDrawed < m_ShellCount)
                {
                    for(int i=0;i<16;i++)
                        drawingSettings.SetShaderPassName(i,kNull);
                    
                    for (int i = 0; i < 16; i++)
                    {
                        if (totalDrawed >= m_ShellCount)
                            break;
                        
                        drawingSettings.SetShaderPassName(i,new ShaderTagId(m_PassName+totalDrawed));
                        totalDrawed++;
                    }
                    context.DrawRenderers(renderingData.cullResults,ref drawingSettings,ref filterSettings);
                }

            }
        }
    }
}