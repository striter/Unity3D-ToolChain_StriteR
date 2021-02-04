using System;
using UnityEngine;
namespace Rendering.ImageEffect
{
    public class PostEffect_Blurs : PostEffectBase<ImageEffect_Blurs,ImageEffectParam_Blurs>
    {
    }

    public enum enum_BlurType
    {
        Kawase = 0,
        AverageVHSeperated = 1,
        GaussianVHSeperated = 2,
        Hexagon=3,
        DualFiltering=4,
    }

    enum enum_BlurPass
    {
        Kawase=0,
        Average_Horizontal,
        Average_Vertical,
        Gaussian_Horizontal,
        Gaussian_Vertical,

        Hexagon_Vertical,
        Hexagon_Diagonal,
        Hexagon_Rhomboid,

        DualFiltering_DownSample,
        DualFiltering_UpSample,
    }

    [Serializable]
    public struct ImageEffectParam_Blurs 
    {
        [Header("Common")]
        [Range(0.15f, 2.5f)] public float blurSize;
        [Range(1, 4)] public int downSample;
        [Range(1, 8)] public int iteration;
        public enum_BlurType blurType;
        [Header("Hexagon")]
        [Range(-1, 1)] public float hexagonAngle;
        public static readonly ImageEffectParam_Blurs m_Default = new ImageEffectParam_Blurs()
        {
            blurSize = 1.0f,
            downSample = 2,
            iteration = 1,
            blurType = enum_BlurType.Kawase,
            hexagonAngle = 0,
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
        static readonly int ID_HexagonAngle = Shader.PropertyToID("_HexagonAngle");
        #endregion
        protected override void OnImageProcess(RenderTexture _src, RenderTexture _dst, Material _material, ImageEffectParam_Blurs _param)
        {
            if (_material.passCount <= 1)
            {
                Debug.LogWarning("Invalid Material Pass Found Of Blur!");
                Graphics.Blit(_src, _dst);
                return;
            }


            int startWidth = _src.width / _param.downSample;
            int startHeight = _src.height / _param.downSample;
            switch (_param.blurType)
            {
                case enum_BlurType.AverageVHSeperated:
                case enum_BlurType.Kawase:
                case enum_BlurType.GaussianVHSeperated:
                    {
                        RenderTexture rtTemp1 = RenderTexture.GetTemporary(startWidth, startHeight, 0, _src.format);
                        RenderTexture rtTemp2 = RenderTexture.GetTemporary(startWidth, startHeight, 0, _src.format);
                        for (int i = 0; i < _param.iteration; i++)
                        {
                            RenderTexture blitSrc = i == 0 ? _src : (i % 2 == 0 ? rtTemp1 : rtTemp2);
                            RenderTexture blitTarget = i == _param.iteration - 1 ? _dst : (i % 2 == 0 ? rtTemp2 : rtTemp1);
                            switch (_param.blurType)
                            {
                                case enum_BlurType.Kawase:
                                    {
                                        int pass = (int)enum_BlurPass.Kawase;
                                        _material.SetFloat(ID_BlurSize, _param.blurSize / _param.downSample * (1 + i));
                                        Graphics.Blit(blitSrc, blitTarget, _material, pass);
                                    }
                                    break;
                                case enum_BlurType.AverageVHSeperated:
                                case enum_BlurType.GaussianVHSeperated:
                                    {
                                        int horizontalPass = -1;
                                        int verticalPass = -1;
                                        if(_param.blurType== enum_BlurType.AverageVHSeperated)
                                        {
                                            horizontalPass = (int)enum_BlurPass.Average_Horizontal;
                                            verticalPass = (int)enum_BlurPass.Average_Vertical;
                                        }
                                        else if(_param.blurType== enum_BlurType.GaussianVHSeperated)
                                        {
                                            horizontalPass = (int)enum_BlurPass.Gaussian_Horizontal;
                                            verticalPass = (int)enum_BlurPass.Gaussian_Vertical;
                                        }

                                        _material.SetFloat(ID_BlurSize, _param.blurSize / _param.downSample * (1 + i));

                                        RenderTexture rtTemp3 = RenderTexture.GetTemporary(startWidth, startHeight, 0, _src.format);
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
                        _material.SetFloat(ID_HexagonAngle, _param.hexagonAngle);

                        RenderTexture verticalRT = RenderTexture.GetTemporary(startWidth, startHeight, 0, _src.format);
                        RenderTexture diagonalRT = RenderTexture.GetTemporary(startWidth, startHeight, 0, _src.format);

                        Graphics.Blit(_src, verticalRT, _material, verticalPass);
                        _material.SetTexture("_Hexagon_Vertical", verticalRT);
                        Graphics.Blit(_dst, diagonalRT, _material, diagonalPass);
                        _material.SetTexture("_Hexagon_Diagonal", diagonalRT);
                        Graphics.Blit(_src, _dst, _material, rhomboidPass);

                        RenderTexture.ReleaseTemporary(verticalRT);
                        RenderTexture.ReleaseTemporary(diagonalRT);
                    }
                    break;
                case enum_BlurType.DualFiltering:
                    {
                        int downSamplePass = (int)enum_BlurPass.DualFiltering_DownSample;
                        int upSamplePass = (int)enum_BlurPass.DualFiltering_UpSample;

                        int downSampleCount = Mathf.FloorToInt( _param.iteration / 2f);
                        _material.SetFloat(ID_BlurSize, _param.blurSize/_param.downSample);

                        RenderTexture[] tempTextures = new RenderTexture[_param.iteration-1];
                        for(int i=0;i<_param.iteration-1;i++)
                        {
                            int filterSample = downSampleCount- Mathf.CeilToInt(Mathf.Abs( _param.iteration / 2f - (i+1)))+ 1+_param.iteration%2;
                            int filterWidth = startWidth / filterSample;
                            int filterHeight = startHeight / filterSample;
                            tempTextures[i] = RenderTexture.GetTemporary(filterWidth,filterHeight,0,_src.format);
                        }
                        for(int i=0;i<_param.iteration;i++)
                        {
                            int filterPass = i <= downSampleCount ? downSamplePass : upSamplePass;
                            RenderTexture blitSrc = i == 0 ? _src : tempTextures[i-1];
                            RenderTexture blitTarget = i == _param.iteration - 1 ? _dst : tempTextures[i];
                            Graphics.Blit(blitSrc, blitTarget, _material, filterPass);
                        }
                        for (int i = 0; i < _param.iteration - 1; i++)
                        {
                            RenderTexture.ReleaseTemporary(tempTextures[i]);
                        }
                        tempTextures = null;
                    }
                    break;
            }

        }

    }
}