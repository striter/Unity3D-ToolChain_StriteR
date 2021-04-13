using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Rendering
{
    using ImageEffect;
    public class SRF_General : ScriptableRendererFeature
    {
        #region IDs
        static readonly int ID_FrustumCornersRayBL = Shader.PropertyToID("_FrustumCornersRayBL");
        static readonly int ID_FrustumCornersRayBR = Shader.PropertyToID("_FrustumCornersRayBR");
        static readonly int ID_FrustumCornersRayTL = Shader.PropertyToID("_FrustumCornersRayTL");
        static readonly int ID_FrustumCornersRayTR = Shader.PropertyToID("_FrustumCornersRayTR");
        #endregion
        public bool m_FrustumCornersRay;
        public bool m_OpaqueBlurTexture;
        [MFoldout(nameof(m_OpaqueBlurTexture), true)] public ImageEffectParam_Blurs m_BlurParams = USRP.GetDefaultPostProcessData<ImageEffectParam_Blurs>();
        SRP_OpaqueBlurTexture m_OpaqueBlurPass;
        public override void Create()
        {
            m_OpaqueBlurPass = new SRP_OpaqueBlurTexture() { renderPassEvent = RenderPassEvent.AfterRenderingSkybox+1 };
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.isPreviewCamera)
                return;

            bool frustumCornersRay = m_FrustumCornersRay;
            bool opaqueBlurTexture = m_OpaqueBlurTexture;

            if(!renderingData.cameraData.isSceneViewCamera)
            {
                if (!renderingData.cameraData.camera.gameObject.TryGetComponent(out SRD_AddtionalData data))
                    data = renderingData.cameraData.camera.gameObject.AddComponent<SRD_AddtionalData>();
                frustumCornersRay = data.m_FrustumCornersRay.IsEnabled(frustumCornersRay);
                opaqueBlurTexture = data.m_OpaqueBlurTexture.IsEnabled(opaqueBlurTexture);
            }

            if (frustumCornersRay)
                UpdateFrustumCornersRay(renderingData.cameraData.camera);
            if (opaqueBlurTexture)
                renderer.EnqueuePass(m_OpaqueBlurPass.Setup(renderer.cameraColorTarget,m_BlurParams));
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
    }

    public class SRP_OpaqueBlurTexture : ScriptableRenderPass
    {
        #region ID
        static readonly int RT_ID_BlurTexture = Shader.PropertyToID("_OpaqueBlurTexture");
        static readonly RenderTargetIdentifier RT_BlurTexture = new RenderTargetIdentifier(RT_ID_BlurTexture);
        #endregion
        RenderTargetIdentifier m_ColorTexture;
        ImageEffect_Blurs m_Blurs=new ImageEffect_Blurs();
        ImageEffectParam_Blurs m_BlursData;
        public SRP_OpaqueBlurTexture Setup(RenderTargetIdentifier _colorTexture, ImageEffectParam_Blurs _blurs)
        {
            m_ColorTexture = _colorTexture;
            m_BlursData = _blurs;
            m_Blurs.DoValidate(m_BlursData);
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
            m_Blurs.ExecuteBuffer(cmd,renderingData.cameraData.cameraTargetDescriptor,m_ColorTexture,RT_BlurTexture, m_BlursData);
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

}