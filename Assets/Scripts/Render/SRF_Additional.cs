using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Rendering
{
    using System.Linq;
    using ImageEffect;
    public class SRF_Additional : ScriptableRendererFeature
    {
        #region IDs
        static readonly int ID_FrustumCornersRayBL = Shader.PropertyToID("_FrustumCornersRayBL");
        static readonly int ID_FrustumCornersRayBR = Shader.PropertyToID("_FrustumCornersRayBR");
        static readonly int ID_FrustumCornersRayTL = Shader.PropertyToID("_FrustumCornersRayTL");
        static readonly int ID_FrustumCornersRayTR = Shader.PropertyToID("_FrustumCornersRayTR");
        #endregion
        public bool m_FrustumCornersRay;
        [MFoldout(nameof(m_FrustumCornersRay), true)] public bool m_CameraReflectionTexture;
        [MFoldout(nameof(m_FrustumCornersRay), true, nameof(m_CameraReflectionTexture), true)] public Plane m_CameraReflectionPlane=new Plane() { m_Distance=0,m_Normal=Vector3.up};
        [HideInInspector,SerializeField] ComputeShader m_CameraReflectionComputeShader;

        public bool m_OpaqueBlurTexture;
        ImageEffect_Blurs m_Blurs;
        [MFoldout(nameof(m_OpaqueBlurTexture), true)] public ImageEffectParam_Blurs m_BlurParams = USRP.GetDefaultPostProcessData<ImageEffectParam_Blurs>();

        SRP_OpaqueBlurTexture m_OpaqueBlurPass;
        SRP_CameraReflectionTexture m_ReflecitonPass;

        SRP_PerCameraPostProcessing m_PostProcesssing_Opaque;
        SRP_PerCameraPostProcessing m_PostProcesssing_AfterAll;
        public override void Create()
        {
            m_OpaqueBlurPass = new SRP_OpaqueBlurTexture() { renderPassEvent = RenderPassEvent.AfterRenderingSkybox + 1 };
            m_ReflecitonPass = new SRP_CameraReflectionTexture() { renderPassEvent = RenderPassEvent.AfterRenderingSkybox + 1 };
            m_Blurs = new ImageEffect_Blurs();
#if UNITY_EDITOR
            m_CameraReflectionComputeShader = UnityEditor.AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Shaders/Compute/PlanarReflection.compute");
#endif
            m_PostProcesssing_Opaque = new SRP_PerCameraPostProcessing("Opaque Post Process") { renderPassEvent = RenderPassEvent.AfterRenderingSkybox };
            m_PostProcesssing_AfterAll = new SRP_PerCameraPostProcessing("After All Post Process") { renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing };
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.isPreviewCamera)
                return;

            bool frustumCornersRay = m_FrustumCornersRay;
            bool perCameraPostProcess = true;
            bool opaqueBlurTexture = m_OpaqueBlurTexture;
            bool cameraReflectionTexture = m_CameraReflectionTexture;
            Plane cameraRefelctionPlane = m_CameraReflectionPlane;

            if(!renderingData.cameraData.isSceneViewCamera)
            {
                if (!renderingData.cameraData.camera.gameObject.TryGetComponent(out SRD_AddtionalData data))
                    data = renderingData.cameraData.camera.gameObject.AddComponent<SRD_AddtionalData>();
                perCameraPostProcess = renderingData.cameraData.postProcessEnabled;
                frustumCornersRay = data.m_FrustumCornersRay.IsEnabled(frustumCornersRay);
                opaqueBlurTexture = data.m_OpaqueBlurTexture.IsEnabled(opaqueBlurTexture);
                cameraReflectionTexture = data.m_ReflectionTexture.IsEnabled(cameraReflectionTexture);
                cameraRefelctionPlane = data.m_ReflectionPlane;
            }

            if (frustumCornersRay)
                UpdateFrustumCornersRay(renderingData.cameraData.camera);
            if (perCameraPostProcess)
                UpdatePostProcess(renderer, ref renderingData);
            if (opaqueBlurTexture)
                renderer.EnqueuePass(m_OpaqueBlurPass.Setup(renderer.cameraColorTarget,m_Blurs,m_BlurParams));
            if (cameraReflectionTexture)
                renderer.EnqueuePass(m_ReflecitonPass.Setup(renderer.cameraColorTarget, m_CameraReflectionComputeShader, cameraRefelctionPlane));
        }
        void UpdateFrustumCornersRay(Camera _camera)
        {
            float fov = _camera.fieldOfView;
            float near = _camera.nearClipPlane;
            float aspect = _camera.aspect;
            Transform cameraTrans = _camera.transform;
            float halfHeight = near * Mathf.Tan(fov * .5f * Mathf.Deg2Rad);
            Vector3 toRight = cameraTrans.right * halfHeight * aspect;
            Vector3 toTop = cameraTrans.up * halfHeight;
            Vector3 topLeft = cameraTrans.forward * near + toTop - toRight;
            float scale = topLeft.magnitude / near;
            topLeft.Normalize();
            topLeft *= scale;
            Vector3 topRight = cameraTrans.forward * near + toTop + toRight;
            topRight.Normalize();
            topRight *= scale;
            Vector3 bottomLeft = cameraTrans.forward * near - toTop - toRight;
            bottomLeft.Normalize();
            bottomLeft *= scale;
            Vector3 bottomRight = cameraTrans.forward * near - toTop + toRight;
            bottomRight.Normalize();
            bottomRight *= scale;
            Shader.SetGlobalVector(ID_FrustumCornersRayBL, bottomLeft);
            Shader.SetGlobalVector(ID_FrustumCornersRayBR, bottomRight);
            Shader.SetGlobalVector(ID_FrustumCornersRayTL, topLeft);
            Shader.SetGlobalVector(ID_FrustumCornersRayTR, topRight);
        }
        void UpdatePostProcess(ScriptableRenderer renderer, ref RenderingData renderingData)
        {

            var postEffects = renderingData.cameraData.camera.GetComponents<APostEffectBase>().FindAll(p => p.enabled);
            if (postEffects.Count() == 0)
                return;

            var dictionary = postEffects.ToListDictionary(p => p.m_IsOpaqueProcess);

            foreach (var key in dictionary.Keys)
            {
                int count = dictionary[key].Count();
                if (count == 0)
                    continue;

                if (key)
                    renderer.EnqueuePass(m_PostProcesssing_Opaque.Setup(renderer.cameraColorTarget, dictionary[key]));
                else
                    renderer.EnqueuePass(m_PostProcesssing_AfterAll.Setup(renderer.cameraColorTarget, dictionary[key]));
            }
        }
    }
    public class SRP_PerCameraPostProcessing : ScriptableRenderPass
    {
        static readonly int ID_Blit_Temp = Shader.PropertyToID("_PostProcessing_Blit_Temp");
        string m_Name;
        RenderTargetIdentifier m_SrcTarget;
        RenderTargetIdentifier m_BlitTarget;
        IEnumerable<APostEffectBase> m_Effects;
        public SRP_PerCameraPostProcessing(string _name) : base()
        {
            m_Name = _name;
            m_BlitTarget = new RenderTargetIdentifier(ID_Blit_Temp);
        }
        public SRP_PerCameraPostProcessing Setup(RenderTargetIdentifier _srcTarget, IEnumerable<APostEffectBase> _effects)
        {
            m_SrcTarget = _srcTarget;
            m_Effects = _effects;
            return this;
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(m_Name);
            cmd.GetTemporaryRT(ID_Blit_Temp, renderingData.cameraData.cameraTargetDescriptor);
            bool blitSrc = true;
            foreach (var effect in m_Effects)
            {
                RenderTargetIdentifier src = blitSrc ? m_SrcTarget : m_BlitTarget;
                RenderTargetIdentifier dst = blitSrc ? m_BlitTarget : m_SrcTarget;
                effect.ExecutePostProcess(cmd, renderingData.cameraData.cameraTargetDescriptor, src, dst);
                blitSrc = !blitSrc;
            }
            if (!blitSrc)
                cmd.Blit(m_BlitTarget, m_SrcTarget);
            cmd.ReleaseTemporaryRT(ID_Blit_Temp);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }

    public class SRP_OpaqueBlurTexture : ScriptableRenderPass
    {
        #region ID
        static readonly int RT_ID_BlurTexture = Shader.PropertyToID("_OpaqueBlurTexture");
        static readonly RenderTargetIdentifier RT_BlurTexture = new RenderTargetIdentifier(RT_ID_BlurTexture);
        #endregion
        RenderTargetIdentifier m_ColorTexture;
        ImageEffect_Blurs m_Blurs;
        ImageEffectParam_Blurs m_BlurParams;
        public SRP_OpaqueBlurTexture Setup(RenderTargetIdentifier _colorTexture, ImageEffect_Blurs _blurs,ImageEffectParam_Blurs _params)
        {
            m_ColorTexture = _colorTexture;
            m_Blurs = _blurs;
            m_BlurParams = _params;
            return this;
        }
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            base.Configure(cmd, cameraTextureDescriptor);
            cmd.GetTemporaryRT(RT_ID_BlurTexture, cameraTextureDescriptor.width,cameraTextureDescriptor.height,0,FilterMode.Bilinear,RenderTextureFormat.ARGB32);
            ConfigureTarget(RT_BlurTexture);
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("Generate Opaque Blur Texture");
            m_Blurs.ExecuteBuffer(cmd,renderingData.cameraData.cameraTargetDescriptor,m_ColorTexture,RT_BlurTexture, m_BlurParams);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
        public override void FrameCleanup(CommandBuffer cmd)
        {
            base.FrameCleanup(cmd);
            cmd.ReleaseTemporaryRT(RT_ID_BlurTexture);
        }
    }

    public class SRP_CameraReflectionTexture : ScriptableRenderPass
    {
        static readonly int ID_CameraReflectionTex = Shader.PropertyToID("_CameraReflectionTex");
        static readonly RenderTargetIdentifier RT_ID_CameraReflectionTex = new RenderTargetIdentifier(ID_CameraReflectionTex);
        static readonly int ID_MainTex = Shader.PropertyToID("_MainTex");
        static readonly int ID_Result = Shader.PropertyToID("_Result");
        const int C_KernalGroupCount = 8;
        Plane m_Plane;
        ComputeShader m_ComputeShader;
        RenderTargetIdentifier m_ColorTarget;
        int m_KernalClear;
        int m_KernalGenerate;
        int m_GroupX;
        int m_GroupY;
        public SRP_CameraReflectionTexture Setup(RenderTargetIdentifier _color,ComputeShader _shader,Plane _plane)
        {
            m_ColorTarget = _color;
            m_Plane = _plane;
            m_ComputeShader = _shader;
            m_KernalClear = m_ComputeShader.FindKernel("Clear");
            m_KernalGenerate = m_ComputeShader.FindKernel("Generate");
            return this;
        }
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            base.Configure(cmd, cameraTextureDescriptor);
            RenderTextureDescriptor textureDescriptor = new RenderTextureDescriptor(cameraTextureDescriptor.width, cameraTextureDescriptor.height, RenderTextureFormat.ARGB32, -1) { enableRandomWrite=true};
            cmd.GetTemporaryRT(ID_CameraReflectionTex, textureDescriptor);

            m_GroupX = cameraTextureDescriptor.width / C_KernalGroupCount;
            m_GroupY = cameraTextureDescriptor.height / C_KernalGroupCount;
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            base.FrameCleanup(cmd);
            cmd.ReleaseTemporaryRT(ID_CameraReflectionTex);
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("Generate Reflection Texture");
            cmd.SetComputeTextureParam(m_ComputeShader, m_KernalClear, ID_Result, RT_ID_CameraReflectionTex);
            cmd.SetComputeTextureParam(m_ComputeShader,m_KernalClear,ID_MainTex,m_ColorTarget);
            cmd.DispatchCompute(m_ComputeShader, m_KernalClear, m_GroupX, m_GroupY, 1);
            cmd.SetComputeTextureParam(m_ComputeShader, m_KernalGenerate, ID_Result, RT_ID_CameraReflectionTex);
            cmd.SetComputeTextureParam(m_ComputeShader, m_KernalGenerate, ID_MainTex, m_ColorTarget);
            cmd.DispatchCompute(m_ComputeShader, m_KernalGenerate, m_GroupX, m_GroupY, 1);
            cmd.Blit(RT_ID_CameraReflectionTex,m_ColorTarget);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }
}