using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Rendering.ImageEffect;
namespace Rendering
{
    [RequireComponent(typeof(Camera)),ExecuteInEditMode]
    public class CameraRenderManager : MonoBehaviour
    {
        public enum enum_DepthMode
        {
            None=0,
            BuiltIn=1,
            Optimize=2,
        }

        public enum_DepthMode m_DepthMode = enum_DepthMode.None;
        public bool m_GeometryCopyTexture = false;
        public bool m_GeometryCopyBlurTexture = false;
        public bool m_DepthToWorldCalculation = false;
        public Camera m_Camera { get; private set; }

        private void Start()=>InitCommandBuffers();
        public void OnValidate()
        {

            InitCommandBuffers();
#if UNITY_EDITOR
            if (!m_EditorRenderManager)
                return;
            m_EditorRenderManager.m_DepthToWorldCalculation = m_DepthToWorldCalculation;
            m_EditorRenderManager.OnValidate();
#endif
        }
        private void OnDestroy()=> RemoveCommandBuffers();
        #region CommandBuffer
        #region ShaderProperties
        static readonly int ID_GlobalDepthTexture = Shader.PropertyToID("_CameraDepthTexture");
        static readonly int ID_GeometryTexture = Shader.PropertyToID("_CameraGeometryTexture");
        static readonly int ID_BlurSize = Shader.PropertyToID("_BlurSize");
        static readonly int ID_GeometryBlurTexture = Shader.PropertyToID("_CameraGeometryBlurTexture");
        #endregion
        public ImageEffectParam_Blurs m_BlurData;
        Material m_OpaqueBlurMaterial;
        RenderTexture m_ColorBuffer, m_DepthBuffer;
        RenderTexture  m_DepthTexture;
        RenderTexture m_OpaqueTexture;
        RenderTexture m_BlurTempTexture1, m_BlurTempTexture2;
        void InitCommandBuffers()
        {
            m_Camera = GetComponent<Camera>();
            RemoveCommandBuffers();
            bool optimized = m_DepthMode == enum_DepthMode.Optimize;
            m_Camera.depthTextureMode = m_DepthMode == enum_DepthMode.BuiltIn ? DepthTextureMode.Depth : DepthTextureMode.None;
            
            if (optimized)
            {
                m_ColorBuffer = RenderTexture.GetTemporary(m_Camera.pixelWidth, m_Camera.pixelHeight, 0, RenderTextureFormat.RGB111110Float);
                m_ColorBuffer.name = "Main Color Buffer";
                m_DepthBuffer = RenderTexture.GetTemporary(m_Camera.pixelWidth, m_Camera.pixelHeight, 1, RenderTextureFormat.Depth);
                m_DepthBuffer.name = "Main Depth Buffer";
                m_Camera.SetTargetBuffers(m_ColorBuffer.colorBuffer, m_DepthBuffer.depthBuffer);

                m_DepthTexture = RenderTexture.GetTemporary(m_Camera.pixelWidth, m_Camera.pixelHeight, 0, RenderTextureFormat.RFloat);
                m_DepthTexture.name = "Opaque Depth Texture";

                CommandBuffer m_DepthTextureBuffer = new CommandBuffer() { name = "Depth Texture Generate" };
                m_DepthTextureBuffer.Blit(m_DepthBuffer.depthBuffer, m_DepthTexture.colorBuffer);
                m_DepthTextureBuffer.SetGlobalTexture(ID_GlobalDepthTexture, m_DepthTexture);
                m_Camera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, m_DepthTextureBuffer);
            }

            if (m_GeometryCopyTexture)
            {
                CommandBuffer geometryTextureBuffer = new CommandBuffer() { name = "Geometry Texture Generate" };
                m_OpaqueTexture = RenderTexture.GetTemporary(m_Camera.pixelWidth, m_Camera.pixelHeight, 0);
                m_OpaqueTexture.filterMode = FilterMode.Point;
                if (optimized)
                    geometryTextureBuffer.Blit(m_ColorBuffer, m_OpaqueTexture);
                else
                    geometryTextureBuffer.Blit(BuiltinRenderTextureType.CurrentActive, m_OpaqueTexture);
                geometryTextureBuffer.SetGlobalTexture(ID_GeometryTexture, m_OpaqueTexture);
                m_Camera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, geometryTextureBuffer);
            }

            if (m_GeometryCopyBlurTexture)
            {
                CommandBuffer opaqueBlurTexture = new CommandBuffer() { name = "Geometry Blur Texture Generate" };
                ImageEffectParam_Blurs _params = m_BlurData;
                m_OpaqueBlurMaterial = AImageEffectBase.CreateMaterial(typeof(ImageEffect_Blurs));
                switch (_params.blurType)
                {
                    default:
                        Debug.LogError("Override This Please");
                        break;
                    case ImageEffect_Blurs.enum_BlurType.Average:
                    case ImageEffect_Blurs.enum_BlurType.Gaussian:
                        int rtW = m_Camera.pixelWidth / _params.downSample;
                        int rtH = m_Camera.pixelHeight / _params.downSample;

                        m_BlurTempTexture1 = RenderTexture.GetTemporary(rtW, rtH, 0, RenderTextureFormat.ARGB32);
                        m_BlurTempTexture1.filterMode = FilterMode.Bilinear;
                        m_BlurTempTexture1.name = "Geometry Blur Copy 1";

                        m_BlurTempTexture2 = RenderTexture.GetTemporary(rtW, rtH, 0, RenderTextureFormat.ARGB32);
                        m_BlurTempTexture2.filterMode = FilterMode.Bilinear;
                        m_BlurTempTexture2.name = "Geometry Blur Copy 2";

                        if (optimized)
                            opaqueBlurTexture.Blit(m_ColorBuffer, m_BlurTempTexture1);
                        else
                            opaqueBlurTexture.Blit(BuiltinRenderTextureType.CurrentActive, m_BlurTempTexture1);

                        m_OpaqueBlurMaterial.SetFloat(ID_BlurSize, _params.blurSize);
                        for (int i = 0; i < _params.iteration; i++)
                        {
                            int passStart = ((int)_params.blurType - 1) * 2 + 1;
                            opaqueBlurTexture.Blit(m_BlurTempTexture1, m_BlurTempTexture2, m_OpaqueBlurMaterial, passStart);
                            opaqueBlurTexture.Blit(m_BlurTempTexture2, m_BlurTempTexture1, m_OpaqueBlurMaterial, passStart + 1);
                        }
                        break;
                }
                opaqueBlurTexture.SetGlobalTexture(ID_GeometryBlurTexture, m_BlurTempTexture1);
                m_Camera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, opaqueBlurTexture);
            }
        }
        void RemoveCommandBuffers()
        {
            m_Camera.targetTexture = null;
            m_Camera.RemoveAllCommandBuffers();
            RenderTexture.ReleaseTemporary(m_ColorBuffer);
            RenderTexture.ReleaseTemporary(m_OpaqueTexture);
            RenderTexture.ReleaseTemporary(m_BlurTempTexture1);
            RenderTexture.ReleaseTemporary(m_BlurTempTexture2);
            RenderTexture.ReleaseTemporary(m_DepthBuffer);
            RenderTexture.ReleaseTemporary(m_DepthTexture);
            if (m_OpaqueBlurMaterial)
                DestroyImmediate(m_OpaqueBlurMaterial);
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (m_DepthMode== enum_DepthMode.Optimize)
                Graphics.Blit(m_ColorBuffer, destination);
            else
                Graphics.Blit(source, destination);
        }
        #endregion

        #region DepthCalculation
        static readonly int ID_VPMatrixInverse = Shader.PropertyToID("_VPMatrixInverse");
        static readonly int ID_FrustumCornersRayBL = Shader.PropertyToID("_FrustumCornersRayBL");
        static readonly int ID_FrustumCornersRayBR = Shader.PropertyToID("_FrustumCornersRayBR");
        static readonly int ID_FrustumCornersRayTL = Shader.PropertyToID("_FrustumCornersRayTL");
        static readonly int ID_FrustumCornersRayTR = Shader.PropertyToID("_FrustumCornersRayTR");

        private void OnPreRender()
        {
            if (!m_DepthToWorldCalculation)
                return;

            Shader.SetGlobalMatrix(ID_VPMatrixInverse, (m_Camera.projectionMatrix * m_Camera.worldToCameraMatrix).inverse);
            float fov = m_Camera.fieldOfView;
            float near = m_Camera.nearClipPlane;
            float aspect = m_Camera.aspect;

            Transform cameraTrans = m_Camera.transform;
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
        #endregion

#if UNITY_EDITOR
        CameraRenderManager m_EditorRenderManager;

        private void OnEnable()
        {
            if (!UnityEditor.SceneView.lastActiveSceneView)
                return;
            if (UnityEditor.SceneView.lastActiveSceneView.camera.gameObject == this.gameObject)
                return;

            m_EditorRenderManager = UnityEditor.SceneView.lastActiveSceneView.camera.gameObject.AddComponent<CameraRenderManager>();
            m_EditorRenderManager.m_DepthToWorldCalculation = m_DepthToWorldCalculation;
        }
        private void OnDisable()
        {
            if (!UnityEditor.SceneView.lastActiveSceneView)
                return;
            if (UnityEditor.SceneView.lastActiveSceneView.camera.gameObject == this.gameObject)
                return;
            if (!m_EditorRenderManager)
                return;

            GameObject.DestroyImmediate(m_EditorRenderManager);
        }
#endif
    }

}