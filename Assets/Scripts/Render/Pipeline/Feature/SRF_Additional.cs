using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Linq;
using Rendering.PostProcess;
namespace Rendering.Pipeline
{
    public class SRF_Additional : ScriptableRendererFeature
    {
        #region IDs
        static readonly int ID_FrustumCornersRayBL = Shader.PropertyToID("_FrustumCornersRayBL");
        static readonly int ID_FrustumCornersRayBR = Shader.PropertyToID("_FrustumCornersRayBR");
        static readonly int ID_FrustumCornersRayTL = Shader.PropertyToID("_FrustumCornersRayTL");
        static readonly int ID_FrustumCornersRayTR = Shader.PropertyToID("_FrustumCornersRayTR");
        static readonly int ID_Matrix_VP = Shader.PropertyToID("_Matrix_VP");
        static readonly int ID_Matrix_I_VP=Shader.PropertyToID("_Matrix_I_VP");
        static readonly int ID_Matrix_V = Shader.PropertyToID("_Matrix_V");
        #endregion

        public RenderResources m_Resources;
        [Tooltip("Screen Space World Position Reconstruction")]
        public bool m_NormalTexture=false;
        public bool m_CameraReflectionTexture=false;
        [MFoldout(nameof(m_CameraReflectionTexture), true)] public SRD_PlanarReflectionData m_PlanarReflectionData= SRD_PlanarReflectionData.Default();
        
        SRP_NormalTexture m_NormalPass;
        private SRP_ComponentBasedPostProcess m_OpaquePostProcess;
        private SRP_ComponentBasedPostProcess m_ScreenPostProcess;
        private SRP_Reflection m_Reflection;
        private bool m_Available => m_Resources;
        public override void Create()
        {
            if (!m_Available)
                return;
            
            m_NormalPass = new SRP_NormalTexture(m_Resources) { renderPassEvent = RenderPassEvent.AfterRenderingSkybox};
            m_Reflection = new SRP_Reflection(m_PlanarReflectionData,m_Resources, RenderPassEvent.AfterRenderingSkybox + 1);
            m_OpaquePostProcess=new SRP_ComponentBasedPostProcess(){renderPassEvent = RenderPassEvent.AfterRenderingSkybox + 2};
            m_ScreenPostProcess=new SRP_ComponentBasedPostProcess(){renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing + 1};
        }
        protected override void Dispose(bool disposing)
        {
            if (!m_Available)
                return;
            
            base.Dispose(disposing);
            m_NormalPass.Dispose();
            m_Reflection.Dispose();
            m_OpaquePostProcess.Dispose();
            m_ScreenPostProcess.Dispose();
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!m_Available)
                return;
            
            if (renderingData.cameraData.isPreviewCamera)
                return;

            UpdateScreenParams(ref renderingData);
            foreach (var postProcessPass in UpdatePostProcess(renderingData.cameraData.camera))
                renderer.EnqueuePass(postProcessPass);

            bool cameraNormalTexture = m_NormalTexture;
            bool cameraReflectionTexture = m_CameraReflectionTexture;
            if(renderingData.cameraData.camera.TryGetComponent(out SRC_PipelineAdditionalOverride param))
            {
                cameraNormalTexture = param.m_Normal.IsEnabled(cameraNormalTexture);
                cameraReflectionTexture = param.m_Reflection.IsEnabled(cameraReflectionTexture);
            }
            
            if (cameraNormalTexture)
                renderer.EnqueuePass(m_NormalPass);
            if (cameraReflectionTexture)
                m_Reflection.EnqueuePass(renderer);
        }
        void UpdateScreenParams(ref RenderingData _renderingData)
        {
            var camera = _renderingData.cameraData.camera;
            float fov = camera.fieldOfView;
            float far = camera.farClipPlane;
            float near = camera.nearClipPlane;
            float aspect = camera.aspect;
            Transform cameraTrans = camera.transform;
            float halfHeight = near * Mathf.Tan(fov * .5f * Mathf.Deg2Rad);
            Vector3 forward = cameraTrans.forward;
            Vector3 toRight = cameraTrans.right * halfHeight * aspect;
            Vector3 toTop = cameraTrans.up * halfHeight;
            
            Vector3 topLeft = forward * near + toTop - toRight;
            float scale = topLeft.magnitude / near;
            topLeft.Normalize();
            topLeft *= scale;
            Vector3 topRight = forward * near + toTop + toRight;
            topRight.Normalize();
            topRight *= scale;
            Vector3 bottomLeft = forward * near - toTop - toRight;
            bottomLeft.Normalize();
            bottomLeft *= scale;
            Vector3 bottomRight = forward * near - toTop + toRight;
            bottomRight.Normalize();
            bottomRight *= scale;
            Shader.SetGlobalVector(ID_FrustumCornersRayBL, bottomLeft);
            Shader.SetGlobalVector(ID_FrustumCornersRayBR, bottomRight);
            Shader.SetGlobalVector(ID_FrustumCornersRayTL, topLeft);
            Shader.SetGlobalVector(ID_FrustumCornersRayTR, topRight);
            
            Matrix4x4 projection = GL.GetGPUProjectionMatrix(_renderingData.cameraData.GetProjectionMatrix(),
                _renderingData.cameraData.IsCameraProjectionMatrixFlipped());
            Matrix4x4 view = _renderingData.cameraData.GetViewMatrix();
            Matrix4x4 vp = projection * view;

            Shader.SetGlobalMatrix(ID_Matrix_VP,vp);
            Shader.SetGlobalMatrix(ID_Matrix_I_VP,vp.inverse);
            Shader.SetGlobalMatrix(ID_Matrix_V,view);
        }
        IEnumerable<ScriptableRenderPass> UpdatePostProcess(Camera camera)
        {
            var postEffects = camera.GetComponents<APostProcessBase>().Collect(p => p.enabled);
            var aPostProcessBases = postEffects as APostProcessBase[] ?? postEffects.ToArray();
            if (!aPostProcessBases.Any())
                yield break;

            foreach (var aPostProcessBaseGroup in aPostProcessBases.GroupBy(p => p.m_OpaqueProcess))
                yield return (aPostProcessBaseGroup.Key ? m_OpaquePostProcess : m_ScreenPostProcess).Setup(aPostProcessBaseGroup);
        }
    }
}