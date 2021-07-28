using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.ImageEffect
{
    public class PostProcess_Blurs : PostProcessComponentBase<PPCore_Blurs, PPData_Blurs>
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
    public struct PPData_Blurs
    {
        [MTitle] public enum_BlurType m_BlurType;
        [Range(0.15f, 2.5f)] public float m_BlurSize;
        [MFold(nameof(m_BlurType), enum_BlurType.Grainy)]
        [Range(1, 13)] public int m_Iteration;
        [MFoldout(nameof(m_BlurType), enum_BlurType.Kawase, enum_BlurType.GaussianVHSeperated, enum_BlurType.AverageVHSeperated, enum_BlurType.Hexagon, enum_BlurType.DualFiltering)]
        [Range(1, 4)] public int m_DownSample;
        [MFoldout(nameof(m_BlurType), enum_BlurType.Hexagon, enum_BlurType.Bokeh)]
        [Range(-1, 1)] public float m_Angle;
        [MFoldout(nameof(m_BlurType), enum_BlurType.Directional, enum_BlurType.Radial)]
        [RangeVector(0, 1)] public Vector2 m_Vector;
        public static readonly PPData_Blurs m_Default = new PPData_Blurs()
        {
            m_BlurSize = 1.3f,
            m_DownSample = 2,
            m_Iteration = 7,
            m_BlurType = enum_BlurType.DualFiltering,
            m_Angle = 0,
            m_Vector = Vector2.one * .5f,
        };
    }

    public class PPCore_Blurs : PostProcessCore<PPData_Blurs>
    {
        #region ShaderProperties
        static readonly int ID_BlurSize = Shader.PropertyToID("_BlurSize");
        static readonly int ID_Iteration = Shader.PropertyToID("_Iteration");
        static readonly int ID_Angle = Shader.PropertyToID("_Angle");
        static readonly int ID_Vector = Shader.PropertyToID("_Vector");
        #endregion

        public override void ExecutePostProcessBuffer(CommandBuffer _buffer, RenderTargetIdentifier _src, RenderTargetIdentifier _dst, RenderTextureDescriptor _descriptor,ref PPData_Blurs _data)
        {
            if (_data.m_DownSample <= 0)
            {
                Debug.LogWarning("Invalid Down Sample!");
                _buffer.Blit(_src, _dst);
                return;
            }
            const string C_BlurSample = "Blur";
            _buffer.BeginSample(C_BlurSample);
            int startWidth = _descriptor.width / _data.m_DownSample;
            int startHeight = _descriptor.height / _data.m_DownSample;
            switch (_data.m_BlurType)
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
                        for (int i = 0; i < _data.m_Iteration; i++)
                        {
                            RenderTargetIdentifier blitSrc = i == 0 ? _src : (i % 2 == 0 ? rtTemp1 : rtTemp2);
                            RenderTargetIdentifier blitTarget = i == _data.m_Iteration - 1 ? _dst : (i % 2 == 0 ? rtTemp2 : rtTemp1);
                            switch (_data.m_BlurType)
                            {
                                case enum_BlurType.Kawase:
                                    {
                                        int pass = (int)enum_BlurPass.Kawase;
                                        m_Material.SetFloat(ID_BlurSize, _data.m_BlurSize / _data.m_DownSample * (1 + i));
                                        _buffer.Blit(blitSrc, blitTarget, m_Material, pass);
                                    }
                                    break;
                                case enum_BlurType.AverageVHSeperated:
                                case enum_BlurType.GaussianVHSeperated:
                                    {
                                        int horizontalPass = -1;
                                        int verticalPass = -1;
                                        if (_data.m_BlurType == enum_BlurType.AverageVHSeperated)
                                        {
                                            horizontalPass = (int)enum_BlurPass.Average_Horizontal;
                                            verticalPass = (int)enum_BlurPass.Average_Vertical;
                                        }
                                        else if (_data.m_BlurType == enum_BlurType.GaussianVHSeperated)
                                        {
                                            horizontalPass = (int)enum_BlurPass.Gaussian_Horizontal;
                                            verticalPass = (int)enum_BlurPass.Gaussian_Vertical;
                                        }

                                        m_Material.SetFloat(ID_BlurSize, _data.m_BlurSize / _data.m_DownSample * (1 + i));

                                        int tempID3 = Shader.PropertyToID("_PostProcessing_Blit_Blur_Temp3");
                                        _buffer.GetTemporaryRT(tempID3, startWidth, startHeight, 0, FilterMode.Bilinear, _descriptor.colorFormat);
                                        RenderTargetIdentifier rtTemp3 = new RenderTargetIdentifier(tempID3);
                                        _buffer.Blit(blitSrc, rtTemp3, m_Material, horizontalPass);
                                        _buffer.Blit(rtTemp3, blitTarget, m_Material, verticalPass);
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

                        int downSampleCount = Mathf.FloorToInt(_data.m_Iteration / 2f);
                        m_Material.SetFloat(ID_BlurSize, _data.m_BlurSize / _data.m_DownSample);

                        int[] tempIDs = new int[_data.m_Iteration - 1];
                        RenderTargetIdentifier[] tempTextures = new RenderTargetIdentifier[_data.m_Iteration - 1];
                        for (int i = 0; i < _data.m_Iteration - 1; i++)
                        {
                            int filterSample = downSampleCount - Mathf.CeilToInt(Mathf.Abs(_data.m_Iteration / 2f - (i + 1))) + 1 + _data.m_Iteration % 2;
                            int filterWidth = startWidth / filterSample;
                            int filterHeight = startHeight / filterSample;
                            tempIDs[i] = Shader.PropertyToID("_PostProcessing_Blit_DualFiltering_Temp" + i.ToString());
                            _buffer.GetTemporaryRT(tempIDs[i], filterWidth, filterHeight, 0, FilterMode.Bilinear, _descriptor.colorFormat);
                            tempTextures[i] = new RenderTargetIdentifier(tempIDs[i]);
                        }
                        for (int i = 0; i < _data.m_Iteration; i++)
                        {
                            int filterPass = i <= downSampleCount ? downSamplePass : upSamplePass;
                            RenderTargetIdentifier blitSrc = i == 0 ? _src : tempTextures[i - 1];
                            RenderTargetIdentifier blitTarget = i == _data.m_Iteration - 1 ? _dst : tempTextures[i];
                            _buffer.Blit(blitSrc, blitTarget, m_Material, filterPass);
                        }
                        for (int i = 0; i < _data.m_Iteration - 1; i++)
                        {
                            _buffer.ReleaseTemporaryRT(tempIDs[i]);
                        }
                        tempTextures = null;
                    }
                    break;
                case enum_BlurType.Grainy:
                    {
                        int grainyPass = (int)enum_BlurPass.Grainy;
                        m_Material.SetFloat(ID_BlurSize, _data.m_BlurSize * 32);
                        _buffer.Blit(_src, _dst, m_Material, grainyPass);
                    }
                    break;
                case enum_BlurType.Bokeh:
                    {
                        int bokehPass = (int)enum_BlurPass.Bokeh;
                        m_Material.SetFloat(ID_BlurSize, _data.m_BlurSize);
                        m_Material.SetInt(ID_Iteration, _data.m_Iteration * 32);
                        m_Material.SetFloat(ID_Angle, _data.m_Angle);
                        _buffer.Blit(_src, _dst, m_Material, bokehPass);
                    }
                    break;
                case enum_BlurType.Hexagon:
                    {
                        int verticalPass = (int)enum_BlurPass.Hexagon_Vertical;
                        int diagonalPass = (int)enum_BlurPass.Hexagon_Diagonal;
                        int rhomboidPass = (int)enum_BlurPass.Hexagon_Rhomboid;

                        m_Material.SetFloat(ID_BlurSize, _data.m_BlurSize * 2);
                        m_Material.SetFloat(ID_Iteration, _data.m_Iteration * 2);
                        m_Material.SetFloat(ID_Angle, _data.m_Angle);

                        int verticalTempID = Shader.PropertyToID("_Hexagon_Vertical");
                        int diagonalTempID = Shader.PropertyToID("_Hexagon_Diagonal");

                        _buffer.GetTemporaryRT(verticalTempID, startWidth, startHeight, 0, FilterMode.Bilinear, _descriptor.colorFormat);
                        _buffer.GetTemporaryRT(diagonalTempID, startWidth, startHeight, 0, FilterMode.Bilinear, _descriptor.colorFormat);

                        RenderTargetIdentifier verticalRT = new RenderTargetIdentifier(verticalTempID);
                        RenderTargetIdentifier diagonalRT = new RenderTargetIdentifier(diagonalTempID);

                        _buffer.Blit(_src, verticalRT, m_Material, verticalPass);
                        _buffer.Blit(_src, diagonalRT, m_Material, diagonalPass);
                        _buffer.Blit(_src, _dst, m_Material, rhomboidPass);

                        _buffer.ReleaseTemporaryRT(verticalTempID);
                        _buffer.ReleaseTemporaryRT(diagonalTempID);
                    }
                    break;
                case enum_BlurType.Radial:
                    {
                        int pass = (int)enum_BlurPass.Radial;

                        m_Material.SetFloat(ID_BlurSize, _data.m_BlurSize * 32);
                        m_Material.SetVector(ID_Vector, _data.m_Vector);
                        m_Material.SetInt(ID_Iteration, _data.m_Iteration);
                        _buffer.Blit(_src, _dst, m_Material, pass);
                    }
                    break;
                case enum_BlurType.Directional:
                    {
                        int pass = (int)enum_BlurPass.Directional;
                        m_Material.SetFloat(ID_BlurSize, _data.m_BlurSize * 32);
                        m_Material.SetVector(ID_Vector, _data.m_Vector * 2 - Vector2.one);
                        m_Material.SetInt(ID_Iteration, _data.m_Iteration);
                        _buffer.Blit(_src, _dst, m_Material, pass);
                    }
                    break;
            }
            _buffer.EndSample(C_BlurSample);
        }
    }
}