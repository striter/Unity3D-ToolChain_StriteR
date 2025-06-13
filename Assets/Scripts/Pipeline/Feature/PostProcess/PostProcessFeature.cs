using System.Collections.Generic;
using Rendering.PostProcess;
using UnityEngine.Rendering.Universal;

namespace Rendering.Pipeline
{
    public class PostProcessFeature : AScriptableRendererFeature
    {
        public DAntiAliasing m_AntiAliasing = DAntiAliasing.kDefault;
        private PostProcess_AntiAliasing m_AntiAliasingPostProcess;
        private SRP_TAASetupPass m_TAASetup;
        private PostProcessPass m_OpaquePostProcess;
        private PostProcessPass m_ScreenPostProcess;
        
        private readonly List<IPostProcessBehaviour> m_PostprocessQueue = new();
        private readonly List<IPostProcessBehaviour> m_OpaqueProcessing = new();
        private readonly List<IPostProcessBehaviour> m_ScreenProcessing = new();
        public override void Create()
        {
            m_TAASetup = new SRP_TAASetupPass() { renderPassEvent = RenderPassEvent.BeforeRenderingPrePasses };
            m_OpaquePostProcess = new PostProcessPass() { renderPassEvent = RenderPassEvent.AfterRenderingSkybox + 3 };
            m_ScreenPostProcess = new PostProcessPass() { renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing };
            m_AntiAliasingPostProcess = new PostProcess_AntiAliasing(m_AntiAliasing, m_TAASetup);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            m_TAASetup.Dispose();
            m_OpaquePostProcess.Dispose();
            m_ScreenPostProcess.Dispose();
            m_AntiAliasingPostProcess.Dispose();
        }

        private static List<IPostProcessBehaviour> kResults = new();
        protected override void EnqueuePass(ScriptableRenderer _renderer, ref RenderingData _renderingData)
        {
            if (!RenderResources.Enabled)
                return;
            
            m_PostprocessQueue.Clear();
            
            if (_renderingData is { postProcessingEnabled: true, cameraData: { postProcessEnabled: true } })
            {
                var antiAliasing = m_AntiAliasing.mode;
                if(antiAliasing != EAntiAliasing.None)
                    m_PostprocessQueue.Add(m_AntiAliasingPostProcess);
                if(antiAliasing == EAntiAliasing.TAA)
                    _renderer.EnqueuePass(m_TAASetup);

                _renderingData.cameraData.camera.GetComponentsInChildren(kResults);
                m_PostprocessQueue.AddRange(kResults);
                foreach (var volume in PostProcessVolume.kVolumes)
                {
                    if (!CullingMaskAttribute.Enabled(_renderingData.cameraData.volumeLayerMask.value, volume.gameObject.layer))
                        continue;
                    
                    volume.GetComponents(kResults);
                    for (var j = 0; j < kResults.Count; j++)
                        m_PostprocessQueue.Add(kResults[j]);
                }
            }
            
            if (m_PostprocessQueue.Count<=0)
                return;
            //Sort&Enqeuue
            m_OpaqueProcessing.Clear();
            m_ScreenProcessing.Clear();

            var postProcessCount = m_PostprocessQueue.Count;
            for (var i = 0; i < postProcessCount; i++)
            {
                var postProcess = m_PostprocessQueue[i];
                if (!postProcess.Validate(ref _renderingData))
                    continue;
                
                if(postProcess.OpaqueProcess)
                    m_OpaqueProcessing.Add(postProcess);
                else
                    m_ScreenProcessing.Add(postProcess);
            }

            if (m_OpaqueProcessing.Count > 0)
                _renderer.EnqueuePass(m_OpaquePostProcess.Setup(m_OpaqueProcessing));

            if (m_ScreenProcessing.Count > 0)
                _renderer.EnqueuePass(m_ScreenPostProcess.Setup(m_ScreenProcessing));
        }
    }
}