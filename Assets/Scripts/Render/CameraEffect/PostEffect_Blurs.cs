using System;
using UnityEngine;
namespace Rendering.ImageEffect
{
    public class PostEffect_Blurs : PostEffectBase<ImageEffect_Blurs>
    {
        [Tooltip("模糊参数")]
        public ImageEffectParam_Blurs m_BlurParam;
        protected override ImageEffect_Blurs OnGenerateRequiredImageEffects() => new ImageEffect_Blurs(() => m_BlurParam);
    }


    [System.Serializable]
    public class ImageEffectParam_Blurs : ImageEffectParamBase
    {
        [Tooltip("模糊像素偏差"), Range(0.25f, 1.5f)]
        public float blurSize = 1.0f;
        [Tooltip("贴图降采样"), Range(1, 4)]
        public int downSample = 2;
        [Tooltip("迭代次数"), Range(1, 8)]
        public int iteration = 1;
        [Tooltip("模糊方式")]
        public ImageEffect_Blurs.enum_BlurType blurType = ImageEffect_Blurs.enum_BlurType.AverageSinglePass;
    }

    public class ImageEffect_Blurs : ImageEffectBase<ImageEffectParam_Blurs>
    {
        public ImageEffect_Blurs(Func<ImageEffectParam_Blurs> _GetParams) : base(_GetParams)
        {

        }
        #region ShaderProperties
        const string KW_ClipAlpha = "CLIP_ZERO_ALPHA";
        static readonly int ID_BlurSize = Shader.PropertyToID("_BlurSize");
        #endregion
        public enum enum_BlurType
        {
            AverageSinglePass = 0,
            Average = 1,
            Gaussian = 2,
        }

        protected override void OnImageProcess(RenderTexture _src, RenderTexture _dst, Material _material, ImageEffectParam_Blurs _param)
        {
            if (_material.passCount <= 1)
            {
                Debug.LogWarning("Invalid Material Pass Found Of Blur!");
                Graphics.Blit(_src, _dst);
                return;
            }

            ImageEffectParam_Blurs m_Params = GetParams();

            int rtW = _src.width / m_Params.downSample;
            int rtH = _src.height / m_Params.downSample;
            RenderTexture rt1 = _src;

            for (int i = 0; i < m_Params.iteration; i++)
            {
                _material.SetFloat(ID_BlurSize, m_Params.blurSize * (1 + i));
                if (m_Params.blurType == enum_BlurType.AverageSinglePass)
                {
                    int pass = (int)m_Params.blurType;
                    if (i != m_Params.iteration - 1)
                    {
                        RenderTexture rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, _src.format);
                        Graphics.Blit(rt1, rt2, _material, pass);
                        if(i!=0)
                            RenderTexture.ReleaseTemporary(rt1);
                        rt1 = rt2;
                        continue;
                    }
                    Graphics.Blit(rt1, _dst, _material, pass);
                }
                else
                {
                    int horizontalPass = (int)(m_Params.blurType - 1) * 2 + 1;
                    int verticalPass = horizontalPass + 1;

                    // vertical blur
                    RenderTexture rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, _src.format);
                    Graphics.Blit(rt1, rt2, _material, horizontalPass);
                    if (i != 0)
                        RenderTexture.ReleaseTemporary(rt1);
                    rt1 = rt2;

                    if (i != m_Params.iteration - 1)
                    {
                        // horizontal blur
                        rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, _src.format);
                        Graphics.Blit(rt1, rt2, _material, verticalPass);
                        RenderTexture.ReleaseTemporary(rt1);
                        rt1 = rt2;
                        continue;
                    }
                    Graphics.Blit(rt1, _dst, _material, horizontalPass);
                }
            }
            RenderTexture.ReleaseTemporary(rt1);
        }
    }
}