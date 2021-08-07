using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Linq;
using Rendering.ImageEffect;
using UnityEngine.Serialization;

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
        public bool m_ScreenParams;
        public bool m_OpaqueBlurTexture=false;
        [MFoldout(nameof(m_OpaqueBlurTexture), true)] public PPData_Blurs m_BlurParams = UPipeline.GetDefaultPostProcessData<PPData_Blurs>();
        public bool m_NormalTexture=false;
        [MFoldout(nameof(m_ScreenParams), true)] public bool m_CameraReflectionTexture=false;
        [MFoldout(nameof(m_CameraReflectionTexture), true)] public SRD_PlanarReflectionData m_PlanarReflectionData= SRD_PlanarReflectionData.Default();
        
        SRP_NormalTexture m_NormalPass;
        SRP_ComponentBasedPostProcess m_PostProcesssing_Opaque;
        SRP_ComponentBasedPostProcess m_PostProcesssing_AfterAll;
        SRP_Reflection m_Reflection;
        private bool m_Available => m_Resources;
        public override void Create()
        {
            if (!m_Available)
                return;
            
            m_NormalPass = new SRP_NormalTexture(m_Resources) { renderPassEvent = RenderPassEvent.AfterRenderingSkybox};
            m_PostProcesssing_Opaque = new SRP_ComponentBasedPostProcess() { renderPassEvent = RenderPassEvent.AfterRenderingSkybox };
            m_PostProcesssing_AfterAll = new SRP_ComponentBasedPostProcess() {  renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing };
            m_Reflection = new SRP_Reflection(m_PlanarReflectionData,m_Resources);
        }
        protected override void Dispose(bool disposing)
        {
            if (!m_Available)
                return;
            
            base.Dispose(disposing);
            m_NormalPass.Dispose();
            m_PostProcesssing_Opaque.Dispose();
            m_PostProcesssing_AfterAll.Dispose();
            m_Reflection.Dispose();
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!m_Available)
                return;
            
            if (renderingData.cameraData.isPreviewCamera)
                return;

            bool screenParams = m_ScreenParams;
            bool cameraNormalTexture = m_NormalTexture;
            bool cameraReflectionTexture = m_CameraReflectionTexture;

            if (screenParams)
                UpdateScreenParams(ref renderingData);
            if (cameraNormalTexture)
                renderer.EnqueuePass(m_NormalPass);
            if (cameraReflectionTexture)
                m_Reflection.EnqueuePass(renderer);
            UpdatePostProcess(renderer, ref renderingData);
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
        void UpdatePostProcess(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            var postEffects = renderingData.cameraData.camera.GetComponents<APostProcessBase>().Collect(p => p.enabled);
            if (!postEffects.Any())
                return;

            var groups = postEffects.GroupBy(p=>p.m_IsOpaqueProcess);
            foreach (var group in groups)
            {
                var pass = group.Key ? m_PostProcesssing_Opaque : m_PostProcesssing_AfterAll;
                renderer.EnqueuePass(  pass.Setup(renderer, group));
            }
        }
    }
}