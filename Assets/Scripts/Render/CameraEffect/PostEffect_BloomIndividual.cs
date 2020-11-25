using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Rendering.ImageEffect
{
    public enum enum_BloomIndividual_Blend
    {
        None=0,
        Additive=1,
        AlphaBlend=2,
    }

    public class PostEffect_BloomIndividual:PostEffectBase<CameraEffect_BloomIndividual>
    {
        Camera m_RenderCamera;
        protected override void Awake()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif
            Camera _camera = GetComponent<Camera>();
            GameObject temp = new GameObject("Render Camera");
            temp.transform.SetParentResetTransform(_camera.transform);
            m_RenderCamera = temp.AddComponent<Camera>();
            m_RenderCamera.backgroundColor = Color.black;
            m_RenderCamera.orthographic = _camera.orthographic;
            m_RenderCamera.orthographicSize = _camera.orthographicSize;
            m_RenderCamera.nearClipPlane = _camera.nearClipPlane;
            m_RenderCamera.farClipPlane = _camera.farClipPlane;
            m_RenderCamera.fieldOfView = _camera.fieldOfView;
            m_RenderCamera.depthTextureMode = DepthTextureMode.None;
            m_RenderCamera.enabled = false;
            base.Awake();
        }

        protected override void OnDestroy()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif

            base.OnDestroy();
            GameObject.Destroy(m_RenderCamera.gameObject);
        }

        public override void OnValidate()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying||m_RenderCamera==null)
                return;
#endif
            base.OnValidate();
        }
        public CameraEffectParam_BloomInvididual m_Param;
        public ImageEffectParam_Blurs m_BlurParam;
        protected override CameraEffect_BloomIndividual OnGenerateRequiredImageEffects() => new CameraEffect_BloomIndividual(m_RenderCamera, () => m_Param, () => m_BlurParam);
    }

    [System.Serializable]
    public class CameraEffectParam_BloomInvididual:ImageEffectParamBase
    {
        [Range(0,5)]
        public float m_Intensity = 1f;
        public enum_BloomIndividual_Blend m_BlendMode = enum_BloomIndividual_Blend.Additive;
        [CullingMask]
        public int m_CullingMask=0;
    }


    public class CameraEffect_BloomIndividual:ImageEffectBase<CameraEffectParam_BloomInvididual>
    {
        #region ShaderProperties
        static readonly int ID_Intensity = Shader.PropertyToID("_Intensity");
        static readonly string[] KW_Blend = new string[] { "_BLOOMINDIVIDUAL_ADDITIVE", "_BLOOMINDIVIDUAL_ALPHABLEND" };
        #endregion
        ImageEffect_Blurs m_Blur;
        Camera m_RenderCamera;
        Shader m_RenderBloomShader;
        public CameraEffect_BloomIndividual(Camera _camera, Func<CameraEffectParam_BloomInvididual> _GetParam,Func<ImageEffectParam_Blurs> _GetBlurParam):base(_GetParam)
        {
            m_Blur = new ImageEffect_Blurs(_GetBlurParam);

            m_RenderBloomShader = Shader.Find("Hidden/CameraEffect_BloomReceiver_Emitter");
            if (m_RenderBloomShader == null )
                throw new Exception("Null Bloom Individual Shader Found!");

            m_RenderCamera = _camera;
            m_RenderCamera.clearFlags = CameraClearFlags.SolidColor;
        }
        protected override void OnValidate(CameraEffectParam_BloomInvididual _params, Material _material)
        {
            base.OnValidate(_params, _material);
            _material.SetFloat(ID_Intensity, _params.m_Intensity);
            m_RenderCamera.backgroundColor = _params.m_BlendMode == enum_BloomIndividual_Blend.Additive ? Color.black : Color.clear;
            m_RenderCamera.cullingMask = _params.m_CullingMask;
        }
        protected override void OnImageProcess(RenderTexture _src, RenderTexture _dst, Material _material, CameraEffectParam_BloomInvididual _param)
        {
            TRender.EnableGlobalKeyword(KW_Blend, (int)_param.m_BlendMode);
            RenderTexture m_RenderTexture = RenderTexture.GetTemporary(m_RenderCamera.scaledPixelWidth, m_RenderCamera.scaledPixelHeight, 1);
            m_RenderCamera.targetTexture = m_RenderTexture;
            m_RenderCamera.RenderWithShader(m_RenderBloomShader, "RenderType");
            m_Blur.DoImageProcess(m_RenderTexture, m_RenderTexture);     //Blur
            _material.SetTexture("_RenderTex", m_RenderTexture);
            m_RenderCamera.targetTexture = null;

            Graphics.Blit(_src, _dst, _material);        //Mix
            RenderTexture.ReleaseTemporary(m_RenderTexture);
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            m_RenderCamera = null;
            m_Blur.OnDestroy();
        }
    }
}