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
        static readonly  int ID_Matrix_V=Shader.PropertyToID("_Matrix_V");
        #endregion
        [Tooltip("Screen Space World Position Reconstruction")]
        public bool m_ScreenParams;
        [Header("External Textures")]
        public bool m_OpaqueBlurTexture=false;
        [MFoldout(nameof(m_OpaqueBlurTexture), true)] public PPData_Blurs m_BlurParams = UPipeline.GetDefaultPostProcessData<PPData_Blurs>();
        public bool m_NormalTexture=false;
        [MFoldout(nameof(m_ScreenParams), true)] public bool m_CameraReflectionTexture=false;
        [HideInInspector, SerializeField] ComputeShader m_CameraReflectionComputeShader;

        SRP_OpaqueBlurTexture m_OpaqueBlurPass;
        SRP_NormalTexture m_NormalPass;
        SRP_PlanarReflection[] m_ReflecitonPasses;
        SRP_ComponentBasedPostProcess m_PostProcesssing_Opaque;
        SRP_ComponentBasedPostProcess m_PostProcesssing_AfterAll;
        public override void Create()
        {
            m_OpaqueBlurPass = new SRP_OpaqueBlurTexture() { renderPassEvent = RenderPassEvent.AfterRenderingSkybox };
            m_NormalPass = new SRP_NormalTexture() { renderPassEvent = RenderPassEvent.AfterRenderingSkybox};
            m_PostProcesssing_Opaque = new SRP_ComponentBasedPostProcess() { renderPassEvent = RenderPassEvent.AfterRenderingSkybox };
            m_ReflecitonPasses = new SRP_PlanarReflection[SRP_PlanarReflection.C_MaxReflectionTextureCount];
            for (int i=0;i<SRP_PlanarReflection.C_MaxReflectionTextureCount;i++)
                m_ReflecitonPasses[i] = new SRP_PlanarReflection() { renderPassEvent = RenderPassEvent.AfterRenderingSkybox + 1};
            m_PostProcesssing_AfterAll = new SRP_ComponentBasedPostProcess() {  renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing };
#if UNITY_EDITOR
            m_CameraReflectionComputeShader = UnityEditor.AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Shaders/Compute/PlanarReflection.compute");
#endif
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            m_OpaqueBlurPass.Dispose();
            m_NormalPass.Dispose();
            foreach(var reflectionPass in m_ReflecitonPasses)
                reflectionPass.Dispose();
            m_PostProcesssing_Opaque.Dispose();
            m_PostProcesssing_AfterAll.Dispose();
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.isPreviewCamera)
                return;

            bool screenParams = m_ScreenParams;
            bool opaqueBlurTexture = m_OpaqueBlurTexture;
            bool cameraNormalTexture = m_NormalTexture;
            bool cameraReflectionTexture = m_CameraReflectionTexture;

            if(!renderingData.cameraData.isSceneViewCamera)
            {
                if (!renderingData.cameraData.camera.gameObject.TryGetComponent(out SRD_AdditionalData data))
                    data = renderingData.cameraData.camera.gameObject.AddComponent<SRD_AdditionalData>();

                screenParams = data.m_FrustumCornersRay.IsEnabled(screenParams);
                opaqueBlurTexture = data.m_OpaqueBlurTexture.IsEnabled(opaqueBlurTexture);
                cameraNormalTexture = data.m_NormalTexture.IsEnabled(cameraNormalTexture);
                cameraReflectionTexture = data.m_ReflectionTexture.IsEnabled(cameraReflectionTexture);
            }

            if (screenParams)
                UpdateScreenParams(ref renderingData);
            if (opaqueBlurTexture)
                renderer.EnqueuePass(m_OpaqueBlurPass.Setup(renderer,m_BlurParams));
            if (cameraNormalTexture)
                renderer.EnqueuePass(m_NormalPass);
            if (cameraReflectionTexture)
                UpdateCameraReflectionTexture(renderer,ref renderingData);
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
        void UpdateCameraReflectionTexture(ScriptableRenderer _renderer,ref RenderingData renderingData)
        {
            if (SRD_ReflectionPlane.m_ReflectionPlanes.Count == 0)
                return;

            int index = 0;
            foreach (SRD_ReflectionPlane plane in SRD_ReflectionPlane.m_ReflectionPlanes)
            {
                if (!plane.m_MeshRenderer.isVisible)
                    return;
                if (index >= SRP_PlanarReflection.C_MaxReflectionTextureCount)
                {
                    Debug.LogWarning("Reflection Plane Outta Limit!");
                    break;
                }
                _renderer.EnqueuePass(m_ReflecitonPasses[index].Setup(index, _renderer, m_CameraReflectionComputeShader, plane, renderingData.cameraData.isSceneViewCamera));
                index++;
            }
        }
        void UpdatePostProcess(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            var postEffects = renderingData.cameraData.camera.GetComponents<APostProcessBase>().FindAll(p => p.enabled);
            if (!postEffects.Any())
                return;

            var dictionary = postEffects.ToListDictionary(p => p.m_IsOpaqueProcess);

            foreach (bool key in from key in dictionary.Keys let count = dictionary[key].Count() where count != 0 select key)
                renderer.EnqueuePass(key ? m_PostProcesssing_Opaque.Setup(renderer, dictionary[true]) : m_PostProcesssing_AfterAll.Setup(renderer, dictionary[false]));
        }
    }
}