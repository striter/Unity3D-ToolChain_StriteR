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
        [Range(0.0f, 5)] public float intensity;
        public ImageEffectParam_Blurs m_BlurParams;
        public bool debug;
        public static readonly CameraEffectParam_Bloom m_Default = new CameraEffectParam_Bloom()
        {
            threshold = 0.25f,
            intensity = 0.3f,
            m_BlurParams = ImageEffectParam_Blurs.m_Default,
            debug = false,
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
             m_Blur.DoValidate(_params.m_BlurParams);
        }
        protected override void OnImageProcess(RenderTexture _src, RenderTexture _dst, Material _material, CameraEffectParam_Bloom _param)
        {
            _src.filterMode = FilterMode.Bilinear;
            var rtW = _src.width;
            var rtH = _src.height;

            RenderTexture rt1 = RenderTexture.GetTemporary(rtW, rtH, 0, _src.format);
            RenderTexture rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, _src.format);

            Graphics.Blit(_src, rt1, _material, (int)enum_Pass.SampleLight);
            
            m_Blur.DoImageProcess(rt1, rt2,_param.m_BlurParams);

            if (_param.debug)
            {
                Graphics.Blit(rt2, _dst);
            }
            else
            {
                _material.SetTexture("_Bloom", rt2);
                Graphics.Blit(_src, _dst, _material, (int)enum_Pass.AddBloomTex);
            }

            RenderTexture.ReleaseTemporary(rt1);
            RenderTexture.ReleaseTemporary(rt2);
        }
    }
}