using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Rendering.CameraCommandBuffer;
using Rendering.ImageEffect;
namespace Rendering
{
    [RequireComponent(typeof(Camera))]
    [ExecuteInEditMode]
    public class CameraRenderEffectManager : MonoBehaviour
    {
        [SerializeField]
        public bool m_DepthTexture = false;
        public bool m_DepthToWorldCalculation = false;
        [ SerializeField]
        public bool m_GeometryCopyTexture = false;
        [SerializeField]
        public bool m_GeometryCopyBlurTexture = false;
        [SerializeField]
        public ImageEffectParam_Blurs m_BlurData;
        public Camera m_Camera { get; private set; }
        GeometryCopyBuffer m_GeometryTextureCopyBuffer = new GeometryCopyBuffer();
        GeometryCopyBlurBuffer m_GeometryTextureCopyBlurBuffer = new GeometryCopyBlurBuffer();

        private void OnValidate()
        {
            InitParams();
        }
        private void OnEnable()
        {
            InitParams();
        }

        private void OnDisable()
        {
            InitParams();
        }


        void InitParams()
        {
            m_Camera = GetComponent<Camera>();

            m_Camera.depthTextureMode = gameObject.activeSelf&&m_DepthTexture ? DepthTextureMode.Depth : DepthTextureMode.None;

            if (gameObject.activeSelf&&m_GeometryCopyTexture)
                m_GeometryTextureCopyBuffer.Play(this);
            else
                m_GeometryTextureCopyBuffer.OnDisable();

            if (gameObject.activeSelf&&m_GeometryCopyBlurTexture)
                m_GeometryTextureCopyBlurBuffer.InitBlur(()=>m_BlurData).Play(this);
            else
                m_GeometryTextureCopyBlurBuffer.OnDisable();
        }


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
    }

}