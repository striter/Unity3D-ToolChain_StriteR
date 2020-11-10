using UnityEngine;
using System;
namespace Rendering.ImageEffect
{
    public class PostEffect_Bloom : PostEffectBase<ImageEffect_Bloom>
    {
        [Tooltip("Bloom采样参数")]
        public CameraEffectParam_Bloom m_BloomParams;
        [Tooltip("采样模糊参数")]
        public ImageEffectParam_Blurs m_BlurParams;
        protected override ImageEffect_Bloom OnGenerateRequiredImageEffects() => new ImageEffect_Bloom(()=>m_BloomParams,()=>m_BlurParams);
    }

    [System.Serializable]
    public class CameraEffectParam_Bloom : ImageEffectParamBase
    {
        [Tooltip("LDR 亮度采样阈值"), Range(0.0f, 1f)]
        public float threshold = 0.25f;
        [Tooltip("采样后增强"), Range(0.0f, 2.5f)]
        public float intensity = 0.3f;
        [Tooltip("启动贴图模糊")]
        public bool enableBlur = false;
    }

    public class ImageEffect_Bloom : ImageEffectBase<CameraEffectParam_Bloom>
    {
        #region ShaderProperties
        static int ID_Threshold = Shader.PropertyToID("_Threshold");
        static int ID_Intensity = Shader.PropertyToID("_Intensity");
        #endregion

        public enum enum_Pass
        {
            SampleLight = 0,
            AddBloomTex = 1,
            FastBloom = 2,
        }

        ImageEffect_Blurs m_Blur;
        public ImageEffect_Bloom(Func<CameraEffectParam_Bloom> _GetParams, Func<ImageEffectParam_Blurs> _GetBlurParams) : base(_GetParams)
        {
            m_Blur = new ImageEffect_Blurs(_GetBlurParams);
        }
        public override void DoValidate()
        {
            base.DoValidate();
            m_Blur.DoValidate();
        }
        public override void OnDestory()
        {
            base.OnDestory();
            m_Blur.OnDestory();
        }
        protected override void OnValidate(CameraEffectParam_Bloom _params, Material _material)
        {
            base.OnValidate(_params, _material);
            _material.SetFloat(ID_Threshold, _params.threshold);
            _material.SetFloat(ID_Intensity, _params.intensity);
        }
        protected override void OnImageProcess(RenderTexture _src, RenderTexture _dst, Material _material, CameraEffectParam_Bloom _param)
        {
            if (!_param.enableBlur)
            {
                Graphics.Blit(_src, _dst, _material, (int)enum_Pass.FastBloom);
                return;
            }

            _src.filterMode = FilterMode.Bilinear;
            var rtW = _src.width;
            var rtH = _src.height;

            // downsample
            RenderTexture rt1 = RenderTexture.GetTemporary(rtW, rtH, 0, _src.format);
            rt1.filterMode = FilterMode.Bilinear;

            RenderTexture rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, _src.format);
            rt1.filterMode = FilterMode.Bilinear;


            Graphics.Blit(_src, rt1, _material, (int)enum_Pass.SampleLight);
            m_Blur.DoImageProcess(rt1, rt2);
            _material.SetTexture("_Bloom", rt2);
            Graphics.Blit(_src, _dst, _material, (int)enum_Pass.AddBloomTex);

            RenderTexture.ReleaseTemporary(rt1);
            RenderTexture.ReleaseTemporary(rt2);
        }
    }
}