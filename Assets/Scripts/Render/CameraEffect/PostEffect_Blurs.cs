using System;
using UnityEngine;
using UnityEngine.Rendering;

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
        [MTitle]public enum_BlurType blurType;
        [Range(0.15f, 2.5f)] public float blurSize;
        [Range(1, 8)] public int iteration;
        [Range(1, 4)] public int downSample;
        [MFoldout(nameof(blurType),enum_BlurType.Hexagon,enum_BlurType.Bokeh)]
        [Range(-1, 1)] public float angle;
        [MFoldout(nameof(blurType), enum_BlurType.Directional,enum_BlurType.Radial)]
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
        protected override void OnExecuteBuffer(CommandBuffer _buffer,RenderTextureDescriptor _descriptor,  RenderTargetIdentifier _src, RenderTargetIdentifier _dst, Material _material, ImageEffectParam_Blurs _param)
        {
            if (_material.passCount <= 1)
            {
                Debug.LogWarning("Invalid Material Pass Found Of Blur!");
                _buffer.Blit(_src, _dst);
                return;
            }

            int startWidth = _descriptor.width / _param.downSample;
            int startHeight = _descriptor.height / _param.downSample;
            switch (_param.blurType)
            {
                case enum_BlurType.AverageVHSeperated:
                case enum_BlurType.Kawase:
                case enum_BlurType.GaussianVHSeperated:
                    {
                        int idTemp1= Shader.PropertyToID("_PostProcessing_Blit_Blur_Temp1");
                        int idTemp2= Shader.PropertyToID("_PostProcessing_Blit_Blur_Temp2");
                        _buffer.GetTemporaryRT(idTemp1, startWidth,startHeight,0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
                        RenderTargetIdentifier rtTemp1 = new RenderTargetIdentifier(idTemp1);
                        _buffer.GetTemporaryRT(idTemp2, startWidth, startHeight, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
                        RenderTargetIdentifier rtTemp2 = new RenderTargetIdentifier(idTemp2);
                        for (int i = 0; i < _param.iteration; i++)
                        {
                            RenderTargetIdentifier blitSrc = i == 0 ? _src : (i % 2 == 0 ? rtTemp1 : rtTemp2);
                            RenderTargetIdentifier blitTarget = i == _param.iteration - 1 ? _dst : (i % 2 == 0 ? rtTemp2 : rtTemp1);
                            switch (_param.blurType)
                            {
                                case enum_BlurType.Kawase:
                                    {
                                        int pass = (int)enum_BlurPass.Kawase;
                                        _material.SetFloat(ID_BlurSize, _param.blurSize / _param.downSample * (1 + i));
                                        _buffer.Blit(blitSrc, blitTarget, _material, pass);
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

                                        int tempID3 = Shader.PropertyToID("_PostProcessing_Blit_Blur_Temp3");
                                        _buffer.GetTemporaryRT(tempID3, startWidth, startHeight, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
                                        RenderTargetIdentifier rtTemp3 = new RenderTargetIdentifier(tempID3);
                                        _buffer.Blit(blitSrc, rtTemp3, _material, horizontalPass);
                                        _buffer.Blit(rtTemp3, blitTarget, _material, verticalPass);
                                        _buffer.ReleaseTemporaryRT(tempID3);
                                    }
                                    break;
                            }
                        }
                        _buffer.ReleaseTemporaryRT(idTemp1);
                        _buffer.ReleaseTemporaryRT(idTemp2);
                    }
                    break;
                case enum_BlurType.DualFiltering:
                    {
                        int downSamplePass = (int)enum_BlurPass.DualFiltering_DownSample;
                        int upSamplePass = (int)enum_BlurPass.DualFiltering_UpSample;

                        int downSampleCount = Mathf.FloorToInt(_param.iteration / 2f);
                        _material.SetFloat(ID_BlurSize, _param.blurSize / _param.downSample);

                        int[] tempIDs = new int[_param.iteration - 1];
                        RenderTargetIdentifier[] tempTextures = new RenderTargetIdentifier[_param.iteration - 1];
                        for (int i = 0; i < _param.iteration - 1; i++)
                        {
                            int filterSample = downSampleCount - Mathf.CeilToInt(Mathf.Abs(_param.iteration / 2f - (i + 1))) + 1 + _param.iteration % 2;
                            int filterWidth = startWidth / filterSample;
                            int filterHeight = startHeight / filterSample;
                            tempIDs[i] = Shader.PropertyToID("_PostProcessing_Blit_DualFiltering_Temp"+i.ToString());
                             _buffer.GetTemporaryRT(tempIDs[i], filterWidth, filterHeight, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
                            tempTextures[i] = new RenderTargetIdentifier(tempIDs[i]);
                        }
                        for (int i = 0; i < _param.iteration; i++)
                        {
                            int filterPass = i <= downSampleCount ? downSamplePass : upSamplePass;
                            RenderTargetIdentifier blitSrc = i == 0 ? _src : tempTextures[i - 1];
                            RenderTargetIdentifier blitTarget = i == _param.iteration - 1 ? _dst : tempTextures[i];
                            _buffer.Blit(blitSrc, blitTarget, _material, filterPass);
                        }
                        for (int i = 0; i < _param.iteration - 1; i++)
                        {
                            _buffer.ReleaseTemporaryRT(tempIDs[i]);
                        }
                        tempTextures = null;
                    }
                    break;
                case enum_BlurType.Grainy:
                    {
                        int grainyPass = (int)enum_BlurPass.Grainy;
                        _material.SetFloat(ID_BlurSize, _param.blurSize * _param.iteration * _param.downSample);
                        _material.SetInt(ID_Iteration, _param.iteration * _param.downSample);
                        _buffer.Blit(_src, _dst, _material, grainyPass);
                    }
                    break;
                case enum_BlurType.Bokeh:
                    {
                        int bokehPass = (int)enum_BlurPass.Bokeh;
                        _material.SetFloat(ID_BlurSize, _param.blurSize);
                        _material.SetInt(ID_Iteration, _param.iteration * 32 / _param.downSample);
                        _material.SetFloat(ID_Angle, _param.angle);
                        _buffer.Blit(_src, _dst, _material, bokehPass);
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

                        int verticalTempID = Shader.PropertyToID("_Hexagon_Vertical");
                        int diagonalTempID = Shader.PropertyToID("_Hexagon_Diagonal");
                        
                        _buffer.GetTemporaryRT(verticalTempID,startWidth, startHeight, 0,FilterMode.Bilinear,RenderTextureFormat.ARGB32);
                        _buffer.GetTemporaryRT(diagonalTempID,startWidth, startHeight, 0, FilterMode.Bilinear,RenderTextureFormat.ARGB32);

                        RenderTargetIdentifier verticalRT = new RenderTargetIdentifier(verticalTempID); 
                        RenderTargetIdentifier diagonalRT = new RenderTargetIdentifier(diagonalTempID); 

                        _buffer.Blit(_src, verticalRT, _material, verticalPass);
                        _buffer.Blit(_src, diagonalRT, _material, diagonalPass);
                        _buffer.Blit(_src, _dst, _material, rhomboidPass);

                        _buffer.ReleaseTemporaryRT(verticalTempID);
                        _buffer.ReleaseTemporaryRT(diagonalTempID);
                    }
                    break;
                case enum_BlurType.Radial:
                    {
                        int pass = (int)enum_BlurPass.Radial;

                        _material.SetFloat(ID_BlurSize, _param.blurSize );
                        _material.SetVector(ID_Vector, _param.vector);
                        _material.SetInt(ID_Iteration, _param.iteration * _param.downSample);
                        _buffer.Blit(_src, _dst, _material, pass);
                    }
                    break;
                case enum_BlurType.Directional:
                    {
                        int pass = (int) enum_BlurPass.Directional;
                        _material.SetFloat(ID_BlurSize, _param.blurSize);
                        _material.SetVector(ID_Vector, _param.vector*2-Vector2.one);
                        _material.SetInt(ID_Iteration, _param.iteration * _param.downSample);
                        _buffer.Blit(_src, _dst, _material, pass);
                    }
                    break;
            }

        }

    }
}