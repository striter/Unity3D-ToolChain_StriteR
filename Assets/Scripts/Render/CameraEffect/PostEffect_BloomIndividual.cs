using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Rendering.ImageEffect
{
    public enum enum_BloomIndividual_Blend
    {
        DEBUG=0,
        Additive=1,
        AlphaBlend=2,
    }

    public class PostEffect_BloomIndividual:PostEffectBase<CameraEffect_BloomIndividual,CameraEffectParam_BloomInvididual>
    {
        Camera m_RenderCamera;
        protected override void OnEffectCreate(CameraEffect_BloomIndividual _effect)
        {
            base.OnEffectCreate(_effect);
            if (!m_RenderCamera)
            {
                m_RenderCamera = new GameObject("Bloom Individual Render Camera").AddComponent<Camera>();
                m_RenderCamera.gameObject.hideFlags = HideFlags.HideAndDontSave;
                m_RenderCamera.depthTextureMode = DepthTextureMode.None;
                m_RenderCamera.enabled = false;
            }
            _effect.OnCreate(m_RenderCamera);
        }
        public override void OnValidate()
        {
            base.OnValidate();
            if(m_Camera&&m_RenderCamera)
                SyncCamera(m_Camera, m_RenderCamera);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (m_RenderCamera) GameObject.DestroyImmediate(m_RenderCamera.gameObject);
        }

        public new void OnRenderImage(RenderTexture src, RenderTexture dst)
        {
            m_RenderCamera.transform.position = m_Camera.transform.position;
            m_RenderCamera.transform.rotation = m_Camera.transform.rotation;
#if UNITY_EDITOR
            SyncCamera(m_Camera, m_RenderCamera);
#endif
            base.OnRenderImage(src, dst);
        }

        void SyncCamera(Camera _sourceCamera,Camera _targetCamera)
        {
            _targetCamera.orthographic = _sourceCamera.orthographic;
            _targetCamera.orthographicSize = _sourceCamera.orthographicSize;
            _targetCamera.nearClipPlane = _sourceCamera.nearClipPlane;
            _targetCamera.farClipPlane = _sourceCamera.farClipPlane;
            _targetCamera.fieldOfView = _sourceCamera.fieldOfView;
            _targetCamera.allowHDR = _sourceCamera.allowHDR;
            _targetCamera.allowMSAA = _sourceCamera.allowMSAA;
            _targetCamera.aspect = _sourceCamera.aspect;
        }
    }

    [Serializable]
    public struct CameraEffectParam_BloomInvididual
    {
        [Range(0, 5)] public float m_Intensity;
        public enum_BloomIndividual_Blend m_BlendMode;
        [CullingMask] public int m_CullingMask;
        public bool m_EnableBlur;
        public ImageEffectParam_Blurs m_BlurParam;
        public static readonly CameraEffectParam_BloomInvididual m_Default = new CameraEffectParam_BloomInvididual()
        {
            m_Intensity = 1f,
            m_BlendMode = enum_BloomIndividual_Blend.Additive,
            m_CullingMask = -1,
            m_BlurParam = ImageEffectParam_Blurs.m_Default,
            m_EnableBlur = true
        };
    }

    [SerializeField]
    public class CameraEffect_BloomIndividual:ImageEffectBase<CameraEffectParam_BloomInvididual>
    {
#region ShaderProperties
        static readonly int ID_Intensity = Shader.PropertyToID("_Intensity");
        static readonly string[] KW_Blend = new string[] { "_BLOOMINDIVIDUAL_ADDITIVE", "_BLOOMINDIVIDUAL_ALPHABLEND" , "_BLOOMINDIVIDUAL_DEBUG" };
        static readonly int ID_TargetTexture = Shader.PropertyToID("_TargetTex");
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
            RenderTexture renderTexture = RenderTexture.GetTemporary(m_RenderCamera.scaledPixelWidth, m_RenderCamera.scaledPixelHeight, 1);
            m_RenderCamera.targetTexture = renderTexture;
            m_RenderCamera.RenderWithShader(m_RenderBloomShader, "RenderType");
            m_RenderCamera.targetTexture = null;

            RenderTexture targetTexture=renderTexture;
            if (_param.m_EnableBlur)
            {
                targetTexture = RenderTexture.GetTemporary(m_RenderCamera.scaledPixelWidth, m_RenderCamera.scaledPixelHeight, 1);
                m_Blur.DoImageProcess(renderTexture, targetTexture, _param.m_BlurParam);     //Blur
                RenderTexture.ReleaseTemporary(renderTexture);
            }

            _material.SetTexture(ID_TargetTexture, targetTexture);
            Graphics.Blit(_src, _dst, _material);        //Mix
            RenderTexture.ReleaseTemporary(targetTexture);
        }
    }
}