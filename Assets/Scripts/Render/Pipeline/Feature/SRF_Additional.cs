using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Linq;
using Geometry.Voxel;
using Rendering.PostProcess;
using UnityEngine.Rendering;

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
        
        private SRP_AdditionalParameters m_AdditionalParameters;
        private SRP_NormalTexture m_ScreenSpaceNormal;
        private SRP_MaskTexture m_ScreenSpaceMaskTexture;

        private SRP_ComponentBasedPostProcess m_OpaquePostProcess;
        private SRP_ComponentBasedPostProcess m_ScreenPostProcess;
        private SRP_Reflection m_Reflection;
        private bool m_Available => m_Resources;
        public override void Create()
        {
            if (!m_Available)
                return;

            m_AdditionalParameters = new SRP_AdditionalParameters() { renderPassEvent= RenderPassEvent.BeforeRendering };
            m_ScreenSpaceMaskTexture = new SRP_MaskTexture(){renderPassEvent = RenderPassEvent.BeforeRenderingOpaques};
            m_ScreenSpaceNormal = new SRP_NormalTexture() { renderPassEvent = RenderPassEvent.AfterRenderingSkybox};
            m_Reflection = new SRP_Reflection(m_PlanarReflectionData, RenderPassEvent.AfterRenderingSkybox + 1);
            m_OpaquePostProcess=new SRP_ComponentBasedPostProcess(){renderPassEvent = RenderPassEvent.AfterRenderingSkybox + 2};
            m_ScreenPostProcess=new SRP_ComponentBasedPostProcess(){renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing + 1};
        }
        
        protected override void Dispose(bool disposing)
        {
            if (!m_Available)
                return;
            
            base.Dispose(disposing);
            m_Reflection.Dispose();
            m_ScreenSpaceNormal.Dispose();
            m_ScreenSpaceMaskTexture.Dispose();
            m_OpaquePostProcess.Dispose();
            m_ScreenPostProcess.Dispose();
            m_AdditionalParameters.Dispose();
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!m_Available)
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
            
            EnqueuePostProcess(renderer,renderingData,param);
            renderer.EnqueuePass(m_AdditionalParameters);

            if(m_Mask)
                renderer.EnqueuePass(m_ScreenSpaceMaskTexture.Setup(m_MaskData,renderer));
            if (cameraNormalTexture)
                renderer.EnqueuePass(m_ScreenSpaceNormal);
            if (cameraReflectionTexture)
                m_Reflection.EnqueuePass(renderer);
        }

        private readonly List<IPostProcessBehaviour> m_PostprocessQueue = new List<IPostProcessBehaviour>();
        private SRC_CameraConfig m_PostProcessingPreview;
        void EnqueuePostProcess(ScriptableRenderer _renderer,RenderingData _data,SRC_CameraConfig _override)
        {
            m_PostprocessQueue.Clear();
            if(PostProcessGlobalVolume.HasGlobal)
            {
                var components = PostProcessGlobalVolume.GlobalVolume.GetComponents<IPostProcessBehaviour>();
                for(int j=0;j<components.Length;j++)
                    m_PostprocessQueue.Add(components[j]);
            }
            
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
            EnqueuePostProcesses(_renderer,m_PostprocessQueue);
        }

        private readonly List<IPostProcessBehaviour> m_OpaqueProcessing = new List<IPostProcessBehaviour>();
        private readonly List<IPostProcessBehaviour> m_ScreenProcessing = new List<IPostProcessBehaviour>();
        void EnqueuePostProcesses(ScriptableRenderer _renderer,IList<IPostProcessBehaviour> _postProcesses)
        {
            m_OpaqueProcessing.Clear();
            m_ScreenProcessing.Clear();

            var postProcessCount = _postProcesses.Count;
            for (int i = 0; i < postProcessCount; i++)
            {
                var postProcess = _postProcesses[i];
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
        
        public class SRP_AdditionalParameters : ScriptableRenderPass, ISRPBase
        {
            #region IDs
            private static readonly int ID_FrustumCornersRayBL = Shader.PropertyToID("_FrustumCornersRayBL");
            private static readonly int ID_FrustumCornersRayBR = Shader.PropertyToID("_FrustumCornersRayBR");
            private static readonly int ID_FrustumCornersRayTL = Shader.PropertyToID("_FrustumCornersRayTL");
            private static readonly int ID_FrustumCornersRayTR = Shader.PropertyToID("_FrustumCornersRayTR");

            private static readonly int ID_OrthoCameraDirection = Shader.PropertyToID("_OrthoCameraDirection");
            private static readonly int ID_OrthoCameraPositionBL = Shader.PropertyToID("_OrthoCameraPosBL");
            private static readonly int ID_OrthoCameraPositionBR = Shader.PropertyToID("_OrthoCameraPosBR");
            private static readonly int ID_OrthoCameraPositionTL = Shader.PropertyToID("_OrthoCameraPosTL");
            private static readonly int ID_OrthoCameraPositionTR = Shader.PropertyToID("_OrthoCameraPosTR");

            private static readonly int ID_Matrix_VP = Shader.PropertyToID("_Matrix_VP");
            private static readonly int ID_Matrix_I_VP=Shader.PropertyToID("_Matrix_I_VP");
            private static readonly int ID_Matrix_V = Shader.PropertyToID("_Matrix_V");
            #endregion

            public override void Execute(ScriptableRenderContext context, ref RenderingData _renderingData)
            {
                var camera = _renderingData.cameraData.camera;
                if (_renderingData.cameraData.camera.orthographic)
                {
                    Shader.SetGlobalVector(ID_OrthoCameraDirection,camera.transform.forward);
                    camera.CalculateOrthographicPositions(out var topLeft,out var topRight,out var bottomLeft,out var bottomRight);
                    Shader.SetGlobalVector(ID_OrthoCameraPositionBL, bottomLeft);
                    Shader.SetGlobalVector(ID_OrthoCameraPositionBR, bottomRight);
                    Shader.SetGlobalVector(ID_OrthoCameraPositionTL, topLeft);
                    Shader.SetGlobalVector(ID_OrthoCameraPositionTR, topRight);
                }
                else
                {
                    var rays = new GFrustum(camera).GetFrustumRays();
                    Shader.SetGlobalVector(ID_FrustumCornersRayBL, rays.bottomLeft.direction);
                    Shader.SetGlobalVector(ID_FrustumCornersRayBR, rays.bottomRight.direction);
                    Shader.SetGlobalVector(ID_FrustumCornersRayTL, rays.topLeft.direction);
                    Shader.SetGlobalVector(ID_FrustumCornersRayTR, rays.topRight.direction);
                }
            
                Matrix4x4 projection = GL.GetGPUProjectionMatrix(_renderingData.cameraData.GetProjectionMatrix(),_renderingData.cameraData.IsCameraProjectionMatrixFlipped());
                Matrix4x4 view = _renderingData.cameraData.GetViewMatrix();
                Matrix4x4 vp = projection * view;

                Shader.SetGlobalMatrix(ID_Matrix_VP,vp);
                Shader.SetGlobalMatrix(ID_Matrix_I_VP,vp.inverse);
                Shader.SetGlobalMatrix(ID_Matrix_V,view);
            }
            public void Dispose()
            {
            }
        }
    }
}