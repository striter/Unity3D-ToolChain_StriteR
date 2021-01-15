using System;
using UnityEngine;
namespace Rendering.ImageEffect
{
    public class PostEffect_Blurs : PostEffectBase<ImageEffect_Blurs,ImageEffectParam_Blurs>
    {
    }

    public enum enum_BlurType
    {
        AverageSinglePass = 0,
        Average = 1,
        Gaussian = 2,
    }


    [Serializable]
    public struct ImageEffectParam_Blurs 
    {
        [Range(0.25f, 1.5f)] public float blurSize ;
        [Range(1, 4)] public int downSample;
        [Range(1, 8)] public int iteration;
        public enum_BlurType blurType;
        public static readonly ImageEffectParam_Blurs m_Default = new ImageEffectParam_Blurs()
        {
            blurSize = 1.0f,
            downSample = 2,
            iteration = 1,
            blurType = enum_BlurType.AverageSinglePass,
        };
    }

    public class ImageEffect_Blurs : ImageEffectBase<ImageEffectParam_Blurs>
    {
        public ImageEffect_Blurs() : base()
        {

        }
        #region ShaderProperties
        const string KW_ClipAlpha = "CLIP_ZERO_ALPHA";
        static readonly int ID_BlurSize = Shader.PropertyToID("_BlurSize");
        #endregion
        protected override void OnImageProcess(RenderTexture _src, RenderTexture _dst, Material _material, ImageEffectParam_Blurs _param)
        {
            if (_material.passCount <= 1)
            {
                Debug.LogWarning("Invalid Material Pass Found Of Blur!");
                Graphics.Blit(_src, _dst);
                return;
            }

            int rtW = _src.width / _param.downSample;
            int rtH = _src.height / _param.downSample;
            RenderTexture rt1 = _src;

            for (int i = 0; i < _param.iteration; i++)
            {
                _material.SetFloat(ID_BlurSize, _param.blurSize * (1 + i));
                if (_param.blurType == enum_BlurType.AverageSinglePass)
                {
                    int pass = (int)_param.blurType;
                    if (i != _param.iteration - 1)
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
                    int horizontalPass = (int)(_param.blurType - 1) * 2 + 1;
                    int verticalPass = horizontalPass + 1;

                    // vertical blur
                    RenderTexture rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, _src.format);
                    Graphics.Blit(rt1, rt2, _material, horizontalPass);
                    if (i != 0)
                        RenderTexture.ReleaseTemporary(rt1);
                    rt1 = rt2;

                    if (i != _param.iteration - 1)
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