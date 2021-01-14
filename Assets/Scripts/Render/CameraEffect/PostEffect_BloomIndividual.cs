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

    public class PostEffect_BloomIndividual:PostEffectBase<CameraEffect_BloomIndividual,CameraEffectParam_BloomInvididual>
    {
        Camera m_RenderCamera;
        protected override void OnDestroy()
        {
            base.OnDestroy();
            ClearRenderCamera();
        }
        void GenerateRenderCamera()
        {
            if (m_RenderCamera)
                return;
            Camera _camera = GetComponent<Camera>();

            m_RenderCamera = new GameObject("Bloom Individual Render Camera").AddComponent<Camera>();
            m_RenderCamera.gameObject.hideFlags = HideFlags.HideAndDontSave;
            m_RenderCamera.backgroundColor = Color.black;
            m_RenderCamera.orthographic = _camera.orthographic;
            m_RenderCamera.orthographicSize = _camera.orthographicSize;
            m_RenderCamera.nearClipPlane = _camera.nearClipPlane;
            m_RenderCamera.farClipPlane = _camera.farClipPlane;
            m_RenderCamera.fieldOfView = _camera.fieldOfView;
            m_RenderCamera.depthTextureMode = DepthTextureMode.None;
            m_RenderCamera.enabled = false;
        }
        void ClearRenderCamera()
        {
            if (!m_RenderCamera)
                return;
            GameObject.DestroyImmediate(m_RenderCamera.gameObject);
        }
        protected override void OnEffectCreate(CameraEffect_BloomIndividual _effect)
        {
            base.OnEffectCreate(_effect);
            GenerateRenderCamera();
            _effect.OnCreate(m_RenderCamera);
        }

        public new void OnRenderImage(RenderTexture src, RenderTexture dst)
        {
            m_RenderCamera.transform.position = transform.position;
            m_RenderCamera.transform.rotation = transform.rotation;
            base.OnRenderImage(src, dst);
        }
    }

    [Serializable]
    public class CameraEffectParam_BloomInvididual:ImageEffectParamBase
    {
        [Range(0,5)]
        public float m_Intensity = 1f;
        public enum_BloomIndividual_Blend m_BlendMode = enum_BloomIndividual_Blend.Additive;
        [CullingMask]
        public int m_CullingMask=-1;
        public ImageEffectParam_Blurs m_BlurParam;
    }

    [SerializeField]
    public class CameraEffect_BloomIndividual:ImageEffectBase<CameraEffectParam_BloomInvididual>
    {
#region ShaderProperties
        static readonly int ID_Intensity = Shader.PropertyToID("_Intensity");
        static readonly string[] KW_Blend = new string[] { "_BLOOMINDIVIDUAL_ADDITIVE", "_BLOOMINDIVIDUAL_ALPHABLEND" };
#endregion
        ImageEffect_Blurs m_Blur;
        Camera m_RenderCamera;
        Shader m_RenderBloomShader;
        public CameraEffect_BloomIndividual():base()
        {
            m_Blur = new ImageEffect_Blurs();

            m_RenderBloomShader = Shader.Find("Hidden/CameraEffect_BloomReceiver_Emitter");
            if (m_RenderBloomShader == null )
                throw new Exception("Null Bloom Individual Shader Found!");
        }
        public void OnCreate(Camera _bindCamera)
        {
            m_RenderCamera = _bindCamera;
            m_RenderCamera.clearFlags = CameraClearFlags.SolidColor;
        }
        public override void Destroy()
        {
            base.Destroy();
            m_RenderCamera = null;
            m_Blur.Destroy();
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
            m_Blur.DoImageProcess(m_RenderTexture, m_RenderTexture,_param.m_BlurParam);     //Blur
            _material.SetTexture("_RenderTex", m_RenderTexture);
            m_RenderCamera.targetTexture = null;

            Graphics.Blit(_src, _dst, _material);        //Mix
            RenderTexture.ReleaseTemporary(m_RenderTexture);
        }
    }
}