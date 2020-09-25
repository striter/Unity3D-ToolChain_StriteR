using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rendering
{
    [RequireComponent(typeof(Camera))]
    [ExecuteInEditMode]
    public class CameraRenderEffectManager : MonoBehaviour
    {
        [SerializeField]
        public bool m_DepthTexture = false;
        [ SerializeField]
        public bool m_GeometryCopyTexture = false;
        [SerializeField]
        public bool m_GeometryCopyBlurTexture = false;
        [SerializeField]
        public ImageEffectParams_Blurs m_BlurData;
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

    }

}