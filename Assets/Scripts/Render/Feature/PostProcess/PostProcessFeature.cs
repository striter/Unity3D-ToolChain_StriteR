using System.Collections.Generic;
using Rendering.PostProcess;
using UnityEngine.Rendering.Universal;

namespace Rendering.Pipeline
{
    public class PostProcessFeature : ScriptableRendererFeature
    {
        public DAntiAliasing m_AntiAliasing = DAntiAliasing.kDefault;
        private PostProcess_AntiAliasing m_AntiAliasingPostProcess;
        private SRP_TAAPass m_TAA;
        private PostProcessPass m_OpaquePostProcess;
        private PostProcessPass m_ScreenPostProcess;
        
        private readonly List<IPostProcessBehaviour> m_PostprocessQueue = new List<IPostProcessBehaviour>();
        private readonly List<IPostProcessBehaviour> m_OpaqueProcessing = new List<IPostProcessBehaviour>();
        private readonly List<IPostProcessBehaviour> m_ScreenProcessing = new List<IPostProcessBehaviour>();
        public override void Create()
        {
            m_TAA = new SRP_TAAPass() { renderPassEvent = RenderPassEvent.BeforeRenderingOpaques - 1 };
            m_OpaquePostProcess = new PostProcessPass() { renderPassEvent = RenderPassEvent.AfterRenderingSkybox + 3 };
            m_ScreenPostProcess = new PostProcessPass() { renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing + 1 };
            m_AntiAliasingPostProcess = new PostProcess_AntiAliasing(m_AntiAliasing, m_TAA);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            m_TAA.Dispose();
            m_OpaquePostProcess.Dispose();
            m_ScreenPostProcess.Dispose();
            m_AntiAliasingPostProcess.Dispose();
        }

        public override void AddRenderPasses(ScriptableRenderer _renderer, ref RenderingData _data)
        {
            if (!RenderResources.Enabled)
                return;
            
            m_PostprocessQueue.Clear();
            if (m_AntiAliasing.mode != EAntiAliasing.None)
                m_PostprocessQueue.Add(m_AntiAliasingPostProcess);
            if(m_AntiAliasing.mode == EAntiAliasing.TAA)
                _renderer.EnqueuePass(m_TAA);
            
            if (_data is { postProcessingEnabled: true, cameraData: { postProcessEnabled: true } })
            {
                m_PostprocessQueue.AddRange(_data.cameraData.camera.GetComponentsInChildren<IPostProcessBehaviour>());

                //Enqueue Global
                if (PostProcessGlobalVolume.HasGlobal)
                {
                    var components = PostProcessGlobalVolume.GlobalVolume.GetComponents<IPostProcessBehaviour>();
                    for (var j = 0; j < components.Length; j++)
                        m_PostprocessQueue.Add(components[j]);
                }
            }
            
            if (m_PostprocessQueue.Count<=0)
                return; 
            //Sort&Enqeuue
            m_OpaqueProcessing.Clear();
            m_ScreenProcessing.Clear();

            var postProcessCount = m_PostprocessQueue.Count;
            for (int i = 0; i < postProcessCount; i++)
            {
                var postProcess = m_PostprocessQueue[i];
                postProcess.ValidateParameters();
                if (!postProcess.m_Enabled)
                    continue;
                
                if(postProcess.m_OpaqueProcess)
                    m_OpaqueProcessing.Add(postProcess);
                else
                    m_ScreenProcessing.Add(postProcess);
            }

            if(m_OpaqueProcessing.Count>0)
                _renderer.EnqueuePass(m_OpaquePostProcess.Setup(m_OpaqueProcessing));
            if(m_ScreenProcessing.Count>0)
                _renderer.EnqueuePass(m_ScreenPostProcess.Setup(m_ScreenProcessing));
        }
    }
}