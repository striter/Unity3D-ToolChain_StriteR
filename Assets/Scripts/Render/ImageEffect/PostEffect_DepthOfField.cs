using System;
using UnityEngine;
namespace Rendering.ImageEffect
{
    public class PostEffect_DepthOfField : PostEffectBase
    {
        [SerializeField, Tooltip("景深采样参数")]
        public CameraEffectParam_DepthOfField m_DepthOfFieldParams;
        [SerializeField, Tooltip("采样图模糊参数")]
        public ImageEffectParam_Blurs m_BlurParams;
        protected override AImageEffectBase OnGenerateRequiredImageEffects() => new CameraEffect_DepthOfField(() => m_DepthOfFieldParams, () => m_BlurParams);
    }

    [System.Serializable]
    public class CameraEffectParam_DepthOfField : ImageEffectParamBase
    {
        [Tooltip("景深起始深度"), Range(0.01f, 1f)]
        public float m_DOFStart = 0.1f;
        [Tooltip("景深渐淡插值深度"), Range(.01f, .3f)]
        public float m_DOFLerp = .1f;
        [Tooltip("遮罩 1 深度")]
        public bool m_FullDepthClip = true;
        [Tooltip("深度取值模糊")]
        public bool m_UseBlurDepth = true;
        [Tooltip("深度取值模糊像素偏差"), Range(.25f, 1.25f)]
        public float m_BlurSize = .5f;
    }
    public class CameraEffect_DepthOfField : ImageEffectBase<CameraEffectParam_DepthOfField>
    {
        #region ShaderID
        static int ID_FocalStart = Shader.PropertyToID("_FocalStart");
        static int ID_FocalLerp = Shader.PropertyToID("_FocalLerp");
        static int ID_BlurTexture = Shader.PropertyToID("_BlurTex");
        static int ID_BlurSize = Shader.PropertyToID("_BlurSize");
        const string KW_FullDepthClip = "_FullDepthClip";
        const string KW_UseBlurDepth = "_UseBlurDepth";
        #endregion

        ImageEffect_Blurs m_Blur;
        public CameraEffect_DepthOfField(Func<CameraEffectParam_DepthOfField> _GetParams, Func<ImageEffectParam_Blurs> _GetBlurParams) : base(_GetParams) { m_Blur = new ImageEffect_Blurs(_GetBlurParams); }
        protected override void OnValidate(CameraEffectParam_DepthOfField _params)
        {
            base.OnValidate(_params);

            m_Material.SetFloat(ID_FocalStart, _params.m_DOFStart);
            m_Material.SetFloat(ID_FocalLerp, _params.m_DOFLerp);
            m_Material.EnableKeyword(KW_FullDepthClip, _params.m_FullDepthClip);
            m_Material.EnableKeyword(KW_UseBlurDepth, _params.m_UseBlurDepth);
            m_Material.SetFloat(ID_BlurSize, _params.m_BlurSize);
            m_Blur.DoValidate();
        }
        public override void OnImageProcess(RenderTexture src, RenderTexture dst)
        {
            RenderTexture _tempBlurTex = RenderTexture.GetTemporary(src.width, src.height, 0, src.format);
            m_Blur.OnImageProcess(src, _tempBlurTex);
            m_Material.SetTexture(ID_BlurTexture, _tempBlurTex);
            Graphics.Blit(src, dst, m_Material);
            RenderTexture.ReleaseTemporary(_tempBlurTex);
        }
        public override void OnDestory()
        {
            base.OnDestory();
            m_Blur.OnDestory();
        }

    }
}
