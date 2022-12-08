using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Rendering.PostProcess;

namespace Rendering.Pipeline
{
    public class SRF_Additional : ScriptableRendererFeature
    {
        public RenderResources m_Resources;
        [Tooltip("Screen Space World Position Reconstruction")]
        public bool m_NormalTexture=false;

        public bool m_Mask = false;
        [MFoldout(nameof(m_Mask), true)] public SRD_MaskData m_MaskData = SRD_MaskData.kDefault;
        public bool m_CameraReflectionTexture=false;
        [MFoldout(nameof(m_CameraReflectionTexture), true)] public SRD_ReflectionData m_PlanarReflectionData = SRD_ReflectionData.kDefault;
        
        private SRP_GlobalParameters m_GlobalParameters;
        private SRP_NormalTexture m_ScreenSpaceNormal;
        private SRP_MaskTexture m_ScreenSpaceMaskTexture;

        public PPData_AntiAliasing m_AntiAliasing = PPData_AntiAliasing.kDefault;
        private SRP_TAAPass m_TAAPass;
        private PostProcess_AntiAliasing m_AntiAliasingPostProcess;
        private SRP_ComponentBasedPostProcess m_OpaquePostProcess;
        private SRP_ComponentBasedPostProcess m_ScreenPostProcess;
        private SRP_Reflection m_Reflection;

        public override void Create()
        {
            if (!m_Resources)
                return;

            m_GlobalParameters = new SRP_GlobalParameters() { renderPassEvent= RenderPassEvent.BeforeRendering };
            m_TAAPass = new SRP_TAAPass() { renderPassEvent = RenderPassEvent.BeforeRenderingOpaques - 1 };
            m_ScreenSpaceMaskTexture = new SRP_MaskTexture() { renderPassEvent = RenderPassEvent.BeforeRenderingOpaques };
            m_ScreenSpaceNormal = new SRP_NormalTexture() { renderPassEvent = RenderPassEvent.AfterRenderingSkybox };
            m_Reflection = new SRP_Reflection(m_PlanarReflectionData, RenderPassEvent.AfterRenderingSkybox + 1);
            
            m_OpaquePostProcess=new SRP_ComponentBasedPostProcess() { renderPassEvent = RenderPassEvent.AfterRenderingSkybox + 2 };
            m_ScreenPostProcess=new SRP_ComponentBasedPostProcess() { renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing + 1 };
            m_AntiAliasingPostProcess = new PostProcess_AntiAliasing(m_AntiAliasing,m_TAAPass);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            m_TAAPass.Dispose();
            m_AntiAliasingPostProcess.Dispose();
            m_Reflection.Dispose();
            m_ScreenSpaceNormal.Dispose();
            m_ScreenSpaceMaskTexture.Dispose();
            m_OpaquePostProcess.Dispose();
            m_ScreenPostProcess.Dispose();
            m_GlobalParameters.Dispose();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!m_Resources)
                return;
            
            if (renderingData.cameraData.isPreviewCamera)
                return;

            bool cameraNormalTexture = m_NormalTexture;
            bool cameraReflectionTexture = renderingData.cameraData.isSceneViewCamera || m_CameraReflectionTexture;
            if(renderingData.cameraData.camera.TryGetComponent(out SRC_CameraConfig param))
            {
                cameraNormalTexture = param.m_Normal.IsEnabled(cameraNormalTexture);
                cameraReflectionTexture = param.m_Reflection.IsEnabled(cameraReflectionTexture);
            }
            
            renderer.EnqueuePass(m_GlobalParameters);
            EnqueuePostProcess(renderer,ref renderingData,param);

            if(m_Mask)
                renderer.EnqueuePass(m_ScreenSpaceMaskTexture.Setup(m_MaskData,renderer));
            if (cameraNormalTexture)
                renderer.EnqueuePass(m_ScreenSpaceNormal);
            if (cameraReflectionTexture)
                m_Reflection.EnqueuePass(renderer);
        }

        private readonly List<IPostProcessBehaviour> m_PostprocessQueue = new List<IPostProcessBehaviour>();
        private readonly List<IPostProcessBehaviour> m_OpaqueProcessing = new List<IPostProcessBehaviour>();
        private readonly List<IPostProcessBehaviour> m_ScreenProcessing = new List<IPostProcessBehaviour>();
        private SRC_CameraConfig m_PostProcessingPreview;
        void EnqueuePostProcess(ScriptableRenderer _renderer,ref RenderingData _data,SRC_CameraConfig _override)
        {
            m_PostprocessQueue.Clear();
            //Enqueue AntiAliasing
            if (m_AntiAliasing.mode != EAntiAliasing.None)
                m_PostprocessQueue.Add(m_AntiAliasingPostProcess);
            if(m_AntiAliasing.mode == EAntiAliasing.TAA)
                _renderer.EnqueuePass(m_TAAPass);

            //Enqueue Global
            if(PostProcessGlobalVolume.HasGlobal)
            {
                var components = PostProcessGlobalVolume.GlobalVolume.GetComponents<IPostProcessBehaviour>();
                for(int j=0;j<components.Length;j++)
                    m_PostprocessQueue.Add(components[j]);
            }
            
            //Enqueue Camera Preview
            if (_data.cameraData.isSceneViewCamera)
            {
                if(m_PostProcessingPreview !=null && m_PostProcessingPreview.m_PostProcessPreview)
                    m_PostprocessQueue.AddRange(m_PostProcessingPreview.GetComponentsInChildren<IPostProcessBehaviour>());
            }
            else
            {
                if (_data.postProcessingEnabled && _data.cameraData.postProcessEnabled)
                {
                    if (_override != null && _override.m_PostProcessPreview)
                        m_PostProcessingPreview = _override;
                    m_PostprocessQueue.AddRange(_data.cameraData.camera.GetComponentsInChildren<IPostProcessBehaviour>());
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
#if UNITY_EDITOR
                postProcess.ValidateParameters();
#endif
                
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