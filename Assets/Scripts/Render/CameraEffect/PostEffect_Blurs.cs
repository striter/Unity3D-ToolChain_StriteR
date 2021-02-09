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
        AverageVHSeperated,
        GaussianVHSeperated,
        DualFiltering ,
        Grainy,
        Hexagon,
        Bokeh,
        Directional,
        Radial,
    }

    enum enum_BlurPass
    {
        //Blur
        Kawase=0,
        Average_Horizontal,
        Average_Vertical,
        Gaussian_Horizontal,
        Gaussian_Vertical,
        DualFiltering_DownSample,
        DualFiltering_UpSample,
        Grainy,

        //Shaped
        Bokeh,
        Hexagon_Vertical,
        Hexagon_Diagonal,
        Hexagon_Rhomboid,

        //Directioned
        Radial,
        Directional,
    }

    [Serializable]
    public struct ImageEffectParam_Blurs
    {
        public enum_BlurType blurType;
        [Header("Common")]
        [Range(0.15f, 2.5f)] public float blurSize;
        [Range(1, 8)] public int iteration;
        [Range(1, 4)] public int downSample;
        [Header("Bokeh | Hexagon")]
        [Range(-1, 1)] public float angle;
        [Header("Directional | Offset")]
        [RangeVector(0, 1)] public Vector2 vector;
        public static readonly ImageEffectParam_Blurs m_Default = new ImageEffectParam_Blurs()
        {
            blurSize = 1.0f,
            downSample = 2,
            iteration = 1,
            blurType = enum_BlurType.Kawase,
            angle = 0,
            vector=Vector2.one*.5f,
        };
    }

    public class ImageEffect_Blurs : ImageEffectBase<ImageEffectParam_Blurs>
    {
        public ImageEffect_Blurs() : base()
        {

        }
        #region ShaderProperties
        static readonly int ID_BlurSize = Shader.PropertyToID("_BlurSize");
        static readonly int ID_Iteration = Shader.PropertyToID("_Iteration");
        static readonly int ID_Angle = Shader.PropertyToID("_Angle");
        static readonly int ID_Vector = Shader.PropertyToID("_Vector");
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
                case enum_BlurType.DualFiltering:
                    {
                        int downSamplePass = (int)enum_BlurPass.DualFiltering_DownSample;
                        int upSamplePass = (int)enum_BlurPass.DualFiltering_UpSample;

                        int downSampleCount = Mathf.FloorToInt(_param.iteration / 2f);
                        _material.SetFloat(ID_BlurSize, _param.blurSize / _param.downSample);

                        RenderTexture[] tempTextures = new RenderTexture[_param.iteration - 1];
                        for (int i = 0; i < _param.iteration - 1; i++)
                        {
                            int filterSample = downSampleCount - Mathf.CeilToInt(Mathf.Abs(_param.iteration / 2f - (i + 1))) + 1 + _param.iteration % 2;
                            int filterWidth = startWidth / filterSample;
                            int filterHeight = startHeight / filterSample;
                            tempTextures[i] = RenderTexture.GetTemporary(filterWidth, filterHeight, 0, _src.format);
                        }
                        for (int i = 0; i < _param.iteration; i++)
                        {
                            int filterPass = i <= downSampleCount ? downSamplePass : upSamplePass;
                            RenderTexture blitSrc = i == 0 ? _src : tempTextures[i - 1];
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
                case enum_BlurType.Grainy:
                    {
                        int grainyPass = (int)enum_BlurPass.Grainy;
                        _material.SetFloat(ID_BlurSize, _param.blurSize * _param.iteration * _param.downSample);
                        _material.SetInt(ID_Iteration, _param.iteration * _param.downSample);
                        Graphics.Blit(_src, _dst, _material, grainyPass);
                    }
                    break;
                case enum_BlurType.Bokeh:
                    {
                        int bokehPass = (int)enum_BlurPass.Bokeh;
                        _material.SetFloat(ID_BlurSize, _param.blurSize);
                        _material.SetInt(ID_Iteration, _param.iteration * 32 / _param.downSample);
                        _material.SetFloat(ID_Angle, _param.angle);
                        Graphics.Blit(_src, _dst, _material, bokehPass);
                    }
                    break;
                case enum_BlurType.Hexagon:
                    {
                        int verticalPass = (int)enum_BlurPass.Hexagon_Vertical;
                        int diagonalPass = (int)enum_BlurPass.Hexagon_Diagonal;
                        int rhomboidPass = (int)enum_BlurPass.Hexagon_Rhomboid;

                        _material.SetFloat(ID_BlurSize, _param.blurSize * 2);
                        _material.SetFloat(ID_Iteration, _param.iteration*2);
                        _material.SetFloat(ID_Angle, _param.angle);

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
                case enum_BlurType.Radial:
                    {
                        int pass = (int)enum_BlurPass.Radial;

                        _material.SetFloat(ID_BlurSize, _param.blurSize );
                        _material.SetVector(ID_Vector, _param.vector);
                        _material.SetInt(ID_Iteration, _param.iteration * _param.downSample);
                        Graphics.Blit(_src, _dst, _material, pass);
                    }
                    break;
                case enum_BlurType.Directional:
                    {
                        int pass = (int) enum_BlurPass.Directional;
                        _material.SetFloat(ID_BlurSize, _param.blurSize);
                        _material.SetVector(ID_Vector, _param.vector*2-Vector2.one);
                        _material.SetInt(ID_Iteration, _param.iteration * _param.downSample);
                        Graphics.Blit(_src, _dst, _material, pass);
                    }
                    break;
            }

        }

    }
}