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
        Hexagon=3,
    }

    enum enum_BlurPass
    {
        Average_Simple=0,
        Average_Horizontal,
        Average_Vertical,
        Gaussian_Horizontal,
        Gaussian_Vertical,
        Hexagon_Vertical,
        Hexagon_Diagonal,
        Hexagon_Rhomboid,
    }

    [Serializable]
    public struct ImageEffectParam_Blurs 
    {
        [Range(0.15f, 2.5f)] public float blurSize;
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
        static readonly int ID_BlurSize = Shader.PropertyToID("_BlurSize");
        static readonly int ID_HexagonIteration = Shader.PropertyToID("_HexagonIteration");
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
            switch (_param.blurType)
            {
                case enum_BlurType.Average:
                case enum_BlurType.AverageSinglePass:
                case enum_BlurType.Gaussian:
                    {
                        RenderTexture rtTemp1 = RenderTexture.GetTemporary(rtW, rtH, 0, _src.format);
                        RenderTexture rtTemp2 = RenderTexture.GetTemporary(rtW, rtH, 0, _src.format);
                        for (int i = 0; i < _param.iteration; i++)
                        {
                            RenderTexture blitSrc = i == 0 ? _src : (i % 2 == 0 ? rtTemp1 : rtTemp2);
                            RenderTexture blitTarget = i == _param.iteration - 1 ? _dst : (i % 2 == 0 ? rtTemp2 : rtTemp1);
                            switch (_param.blurType)
                            {
                                case enum_BlurType.AverageSinglePass:
                                    {
                                        int pass = (int)enum_BlurPass.Average_Simple;
                                        _material.SetFloat(ID_BlurSize, _param.blurSize / _param.downSample * (1 + i));
                                        Graphics.Blit(blitSrc, blitTarget, _material, pass);
                                    }
                                    break;
                                case enum_BlurType.Average:
                                case enum_BlurType.Gaussian:
                                    {
                                        int horizontalPass = (int)(_param.blurType - 1) * 2 + 1;
                                        int verticalPass = horizontalPass + 1;
                                        _material.SetFloat(ID_BlurSize, _param.blurSize / _param.downSample * (1 + i));

                                        RenderTexture rtTemp3 = RenderTexture.GetTemporary(rtW, rtH, 0, _src.format);
                                        Graphics.Blit(blitSrc, rtTemp3, _material, horizontalPass);
                                        Graphics.Blit(rtTemp3, blitTarget, _material, verticalPass);
                                        RenderTexture.ReleaseTemporary(rtTemp3);
                                    }
                                    break;
                            }
                        }
                        RenderTexture.ReleaseTemporary(rtTemp1);
                        RenderTexture.ReleaseTemporary(rtTemp2);
                    }
                    break;
                case enum_BlurType.Hexagon:
                    {
                        int verticalPass = (int)enum_BlurPass.Hexagon_Vertical;
                        int diagonalPass = (int)enum_BlurPass.Hexagon_Diagonal;
                        int rhomboidPass = (int)enum_BlurPass.Hexagon_Rhomboid;

                        _material.SetFloat(ID_BlurSize, _param.blurSize * 2);
                        _material.SetFloat(ID_HexagonIteration, _param.iteration*2);

                        RenderTexture verticalRT = RenderTexture.GetTemporary(rtW, rtH, 0, _src.format);
                        RenderTexture diagonalRT = RenderTexture.GetTemporary(rtW, rtH, 0, _src.format);

                        Graphics.Blit(_src, verticalRT, _material, verticalPass);
                        _material.SetTexture("_Hexagon_Vertical", verticalRT);
                        Graphics.Blit(_dst, diagonalRT, _material, diagonalPass);
                        _material.SetTexture("_Hexagon_Diagonal", diagonalRT);
                        Graphics.Blit(_src, _dst, _material, rhomboidPass);

                        RenderTexture.ReleaseTemporary(verticalRT);
                        RenderTexture.ReleaseTemporary(diagonalRT);
                    }
                    break;
            }

        }

    }
}