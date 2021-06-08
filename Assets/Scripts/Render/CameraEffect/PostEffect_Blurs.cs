using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rendering.ImageEffect
{
    public class PostEffect_Blurs : PostEffectBase<ImageEffect_Blurs, ImageEffectParam_Blurs>
    {
    }

    public enum enum_BlurType
    {
        Kawase = 0,
        AverageVHSeperated,
        GaussianVHSeperated,
        DualFiltering,
        Grainy,
        Hexagon,
        Bokeh,
        Directional,
        Radial,
    }

    enum enum_BlurPass
    {
        //Blur
        Kawase = 0,
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
        [MTitle] public enum_BlurType m_BlurType;
        [Range(0.15f, 2.5f)] public float m_BlurSize;
        [MFold(nameof(m_BlurType), enum_BlurType.Grainy)]
        [Range(1, 8)] public int m_Iteration;
        [MFoldout(nameof(m_BlurType), enum_BlurType.Kawase, enum_BlurType.GaussianVHSeperated, enum_BlurType.AverageVHSeperated, enum_BlurType.Hexagon, enum_BlurType.DualFiltering)]
        [Range(1, 4)] public int m_DownSample;
        [MFoldout(nameof(m_BlurType), enum_BlurType.Hexagon, enum_BlurType.Bokeh)]
        [Range(-1, 1)] public float m_Angle;
        [MFoldout(nameof(m_BlurType), enum_BlurType.Directional, enum_BlurType.Radial)]
        [RangeVector(0, 1)] public Vector2 m_Vector;
        public static readonly ImageEffectParam_Blurs m_Default = new ImageEffectParam_Blurs()
        {
            m_BlurSize = 1.3f,
            m_DownSample = 2,
            m_Iteration = 7,
            m_BlurType = enum_BlurType.DualFiltering,
            m_Angle = 0,
            m_Vector = Vector2.one * .5f,
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

        protected override void OnExecuteBuffer(CommandBuffer _buffer, RenderTextureDescriptor _descriptor, RenderTargetIdentifier _src, RenderTargetIdentifier _dst, Material _material, ImageEffectParam_Blurs _param)
        {
            if (_param.m_DownSample <= 0)
            {
                Debug.LogWarning("Invalid Down Sample!");
                _buffer.Blit(_src, _dst);
                return;
            }

            int startWidth = _descriptor.width / _param.m_DownSample;
            int startHeight = _descriptor.height / _param.m_DownSample;
            switch (_param.m_BlurType)
            {
                case enum_BlurType.Kawase:
                case enum_BlurType.AverageVHSeperated:
                case enum_BlurType.GaussianVHSeperated:
                    {
                        int idTemp1 = Shader.PropertyToID("_PostProcessing_Blit_Blur_Temp1");
                        int idTemp2 = Shader.PropertyToID("_PostProcessing_Blit_Blur_Temp2");
                        _buffer.GetTemporaryRT(idTemp1, startWidth, startHeight, 0, FilterMode.Bilinear, _descriptor.colorFormat);
                        RenderTargetIdentifier rtTemp1 = new RenderTargetIdentifier(idTemp1);
                        _buffer.GetTemporaryRT(idTemp2, startWidth, startHeight, 0, FilterMode.Bilinear, _descriptor.colorFormat);
                        RenderTargetIdentifier rtTemp2 = new RenderTargetIdentifier(idTemp2);
                        for (int i = 0; i < _param.m_Iteration; i++)
                        {
                            RenderTargetIdentifier blitSrc = i == 0 ? _src : (i % 2 == 0 ? rtTemp1 : rtTemp2);
                            RenderTargetIdentifier blitTarget = i == _param.m_Iteration - 1 ? _dst : (i % 2 == 0 ? rtTemp2 : rtTemp1);
                            switch (_param.m_BlurType)
                            {
                                case enum_BlurType.Kawase:
                                    {
                                        int pass = (int)enum_BlurPass.Kawase;
                                        _material.SetFloat(ID_BlurSize, _param.m_BlurSize / _param.m_DownSample * (1 + i));
                                        _buffer.Blit(blitSrc, blitTarget, _material, pass);
                                    }
                                    break;
                                case enum_BlurType.AverageVHSeperated:
                                case enum_BlurType.GaussianVHSeperated:
                                    {
                                        int horizontalPass = -1;
                                        int verticalPass = -1;
                                        if (_param.m_BlurType == enum_BlurType.AverageVHSeperated)
                                        {
                                            horizontalPass = (int)enum_BlurPass.Average_Horizontal;
                                            verticalPass = (int)enum_BlurPass.Average_Vertical;
                                        }
                                        else if (_param.m_BlurType == enum_BlurType.GaussianVHSeperated)
                                        {
                                            horizontalPass = (int)enum_BlurPass.Gaussian_Horizontal;
                                            verticalPass = (int)enum_BlurPass.Gaussian_Vertical;
                                        }

                                        _material.SetFloat(ID_BlurSize, _param.m_BlurSize / _param.m_DownSample * (1 + i));

                                        int tempID3 = Shader.PropertyToID("_PostProcessing_Blit_Blur_Temp3");
                                        _buffer.GetTemporaryRT(tempID3, startWidth, startHeight, 0, FilterMode.Bilinear, _descriptor.colorFormat);
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

                        int downSampleCount = Mathf.FloorToInt(_param.m_Iteration / 2f);
                        _material.SetFloat(ID_BlurSize, _param.m_BlurSize / _param.m_DownSample);

                        int[] tempIDs = new int[_param.m_Iteration - 1];
                        RenderTargetIdentifier[] tempTextures = new RenderTargetIdentifier[_param.m_Iteration - 1];
                        for (int i = 0; i < _param.m_Iteration - 1; i++)
                        {
                            int filterSample = downSampleCount - Mathf.CeilToInt(Mathf.Abs(_param.m_Iteration / 2f - (i + 1))) + 1 + _param.m_Iteration % 2;
                            int filterWidth = startWidth / filterSample;
                            int filterHeight = startHeight / filterSample;
                            tempIDs[i] = Shader.PropertyToID("_PostProcessing_Blit_DualFiltering_Temp" + i.ToString());
                            _buffer.GetTemporaryRT(tempIDs[i], filterWidth, filterHeight, 0, FilterMode.Bilinear, _descriptor.colorFormat);
                            tempTextures[i] = new RenderTargetIdentifier(tempIDs[i]);
                        }
                        for (int i = 0; i < _param.m_Iteration; i++)
                        {
                            int filterPass = i <= downSampleCount ? downSamplePass : upSamplePass;
                            RenderTargetIdentifier blitSrc = i == 0 ? _src : tempTextures[i - 1];
                            RenderTargetIdentifier blitTarget = i == _param.m_Iteration - 1 ? _dst : tempTextures[i];
                            _buffer.Blit(blitSrc, blitTarget, _material, filterPass);
                        }
                        for (int i = 0; i < _param.m_Iteration - 1; i++)
                        {
                            _buffer.ReleaseTemporaryRT(tempIDs[i]);
                        }
                        tempTextures = null;
                    }
                    break;
                case enum_BlurType.Grainy:
                    {
                        int grainyPass = (int)enum_BlurPass.Grainy;
                        _material.SetFloat(ID_BlurSize, _param.m_BlurSize * 32);
                        _buffer.Blit(_src, _dst, _material, grainyPass);
                    }
                    break;
                case enum_BlurType.Bokeh:
                    {
                        int bokehPass = (int)enum_BlurPass.Bokeh;
                        _material.SetFloat(ID_BlurSize, _param.m_BlurSize);
                        _material.SetInt(ID_Iteration, _param.m_Iteration * 32);
                        _material.SetFloat(ID_Angle, _param.m_Angle);
                        _buffer.Blit(_src, _dst, _material, bokehPass);
                    }
                    break;
                case enum_BlurType.Hexagon:
                    {
                        int verticalPass = (int)enum_BlurPass.Hexagon_Vertical;
                        int diagonalPass = (int)enum_BlurPass.Hexagon_Diagonal;
                        int rhomboidPass = (int)enum_BlurPass.Hexagon_Rhomboid;

                        _material.SetFloat(ID_BlurSize, _param.m_BlurSize * 2);
                        _material.SetFloat(ID_Iteration, _param.m_Iteration * 2);
                        _material.SetFloat(ID_Angle, _param.m_Angle);

                        int verticalTempID = Shader.PropertyToID("_Hexagon_Vertical");
                        int diagonalTempID = Shader.PropertyToID("_Hexagon_Diagonal");

                        _buffer.GetTemporaryRT(verticalTempID, startWidth, startHeight, 0, FilterMode.Bilinear, _descriptor.colorFormat);
                        _buffer.GetTemporaryRT(diagonalTempID, startWidth, startHeight, 0, FilterMode.Bilinear, _descriptor.colorFormat);

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

                        _material.SetFloat(ID_BlurSize, _param.m_BlurSize * 32);
                        _material.SetVector(ID_Vector, _param.m_Vector);
                        _material.SetInt(ID_Iteration, _param.m_Iteration);
                        _buffer.Blit(_src, _dst, _material, pass);
                    }
                    break;
                case enum_BlurType.Directional:
                    {
                        int pass = (int)enum_BlurPass.Directional;
                        _material.SetFloat(ID_BlurSize, _param.m_BlurSize * 32);
                        _material.SetVector(ID_Vector, _param.m_Vector * 2 - Vector2.one);
                        _material.SetInt(ID_Iteration, _param.m_Iteration);
                        _buffer.Blit(_src, _dst, _material, pass);
                    }
                    break;
            }

        }

    }
}