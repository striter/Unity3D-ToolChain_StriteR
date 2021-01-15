using UnityEngine;
using System;
namespace Rendering.ImageEffect
{
    public class PostEffect_Bloom : PostEffectBase<ImageEffect_Bloom,CameraEffectParam_Bloom>
    {
    }

    [Serializable]
    public struct CameraEffectParam_Bloom 
    {
        [Range(0.0f, 1f)] public float threshold;
        [Range(0.0f, 2.5f)] public float intensity;
        public bool enableBlur;
        public ImageEffectParam_Blurs m_BlurParams;
        public static readonly CameraEffectParam_Bloom m_Default = new CameraEffectParam_Bloom()
        {
            threshold = 0.25f,
            intensity = 0.3f,
            enableBlur = false,
            m_BlurParams = ImageEffectParam_Blurs.m_Default,
        };
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
        public ImageEffect_Bloom() : base()
        {
            m_Blur = new ImageEffect_Blurs();
        }
        public override void Destroy()
        {
            base.Destroy();
            m_Blur.Destroy();
        }
        protected override void OnValidate(CameraEffectParam_Bloom _params, Material _material)
        {
            base.OnValidate(_params, _material);
            _material.SetFloat(ID_Threshold, _params.threshold);
            _material.SetFloat(ID_Intensity, _params.intensity);
            if(_params.enableBlur) m_Blur.DoValidate(_params.m_BlurParams);
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
            m_Blur.DoImageProcess(rt1, rt2,_param.m_BlurParams);
            _material.SetTexture("_Bloom", rt2);
            Graphics.Blit(_src, _dst, _material, (int)enum_Pass.AddBloomTex);

            RenderTexture.ReleaseTemporary(rt1);
            RenderTexture.ReleaseTemporary(rt2);
        }
    }
}