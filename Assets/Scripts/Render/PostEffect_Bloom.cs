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
        protected override void OnValidate(CameraEffectParam_Bloom _params)
        {
            base.OnValidate(_params);

            m_Material.SetFloat(ID_Threshold, _params.threshold);
            m_Material.SetFloat(ID_Intensity, _params.intensity);
        }

        public override void OnImageProcess(RenderTexture src, RenderTexture dst)
        {

            CameraEffectParam_Bloom _params = GetParams();
            if (!_params.enableBlur)
            {
                Graphics.Blit(src, dst, m_Material, (int)enum_Pass.FastBloom);
                return;
            }

            src.filterMode = FilterMode.Bilinear;
            var rtW = src.width;
            var rtH = src.height;

            // downsample
            RenderTexture rt1 = RenderTexture.GetTemporary(rtW, rtH, 0, src.format);
            rt1.filterMode = FilterMode.Bilinear;

            RenderTexture rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, src.format);
            rt1.filterMode = FilterMode.Bilinear;


            Graphics.Blit(src, rt1, m_Material, (int)enum_Pass.SampleLight);
            m_Blur.OnImageProcess(rt1, rt2);
            m_Material.SetTexture("_Bloom", rt2);
            Graphics.Blit(src, dst, m_Material, (int)enum_Pass.AddBloomTex);

            RenderTexture.ReleaseTemporary(rt1);
            RenderTexture.ReleaseTemporary(rt2);
        }
    }
}