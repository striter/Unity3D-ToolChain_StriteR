using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.PostProcess
{
    public class PostProcess_Blurs : PostProcessComponentBase<PPCore_Blurs, PPData_Blurs>
    {
        public override bool m_OpaqueProcess => false;
        public override EPostProcess Event => EPostProcess.DepthOfField;

        public bool m_Focal;
        [MFoldout(nameof(m_Focal),true)]public PPData_DepthOfField m_FocalData;
        
        public override void OnValidate()
        {
            base.OnValidate();
            m_Effect?.SetFocal(m_Focal,ref m_FocalData);
        }
    }

    public enum EBlurType
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

    enum EBlurPass
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
        [MTitle] public EBlurType m_BlurType;
        [Range(0.05f, 2f)] public float m_BlurSize;
        [MFold(nameof(m_BlurType), EBlurType.Grainy)]
        [Range(1, 13)] public int m_Iteration;
        [MFoldout(nameof(m_BlurType), EBlurType.Kawase, EBlurType.GaussianVHSeperated, EBlurType.AverageVHSeperated, EBlurType.Hexagon, EBlurType.DualFiltering)]
        [Range(1, 4)] public int m_DownSample;
        [MFoldout(nameof(m_BlurType), EBlurType.Hexagon, EBlurType.Bokeh)]
        [Range(-1, 1)] public float m_Angle;
        [MFoldout(nameof(m_BlurType), EBlurType.Directional, EBlurType.Radial)]
        [RangeVector(0, 1)] public Vector2 m_Vector;
        public static readonly PPData_Blurs m_Default = new PPData_Blurs()
        {
            m_BlurSize = 1.3f,
            m_DownSample = 2,
            m_Iteration = 7,
            m_BlurType = EBlurType.DualFiltering,
            m_Angle = 0,
            m_Vector = Vector2.one * .5f,
        };
    }


    [Serializable]
    public struct PPData_DepthOfField
    {
        [Clamp(0)]public float m_Begin;
        [Clamp(0)]public float m_Width;

        public static readonly PPData_DepthOfField m_Default = new PPData_DepthOfField()
        {
            m_Begin = 10,
            m_Width = 5,
        };
    }
    public class PPCore_Blurs : PostProcessCore<PPData_Blurs>
    {
        #region ShaderProperties
        private static readonly int ID_BlurSize = Shader.PropertyToID("_BlurSize");
        private static readonly int ID_Iteration = Shader.PropertyToID("_Iteration");
        private static readonly int ID_Angle = Shader.PropertyToID("_Angle");
        private static readonly int ID_Vector = Shader.PropertyToID("_Vector");

        private const string KW_Focal = "_DOF";
        private static readonly int ID_FocalBegin = Shader.PropertyToID("_FocalStart");
        private static readonly int ID_FocalEnd = Shader.PropertyToID("_FocalEnd");

        public bool SetFocal(bool _focal,ref PPData_DepthOfField focalData)
        {
            if (m_Material.EnableKeyword(KW_Focal, _focal))
            {
                m_Material.SetFloat(ID_FocalBegin,focalData.m_Begin);
                m_Material.SetFloat(ID_FocalEnd,focalData.m_Begin+focalData.m_Width);
            }
            return _focal;
        }
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
                case EBlurType.Kawase:
                case EBlurType.AverageVHSeperated:
                case EBlurType.GaussianVHSeperated:
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
                                case EBlurType.Kawase:
                                    {
                                        int pass = (int)EBlurPass.Kawase;
                                        m_Material.SetFloat(ID_BlurSize, _data.m_BlurSize / _data.m_DownSample * (1 + i));
                                        _buffer.Blit(blitSrc, blitTarget, m_Material, pass);
                                    }
                                    break;
                                case EBlurType.AverageVHSeperated:
                                case EBlurType.GaussianVHSeperated:
                                    {
                                        int horizontalPass = -1;
                                        int verticalPass = -1;
                                        if (_data.m_BlurType == EBlurType.AverageVHSeperated)
                                        {
                                            horizontalPass = (int)EBlurPass.Average_Horizontal;
                                            verticalPass = (int)EBlurPass.Average_Vertical;
                                        }
                                        else if (_data.m_BlurType == EBlurType.GaussianVHSeperated)
                                        {
                                            horizontalPass = (int)EBlurPass.Gaussian_Horizontal;
                                            verticalPass = (int)EBlurPass.Gaussian_Vertical;
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
                case EBlurType.DualFiltering:
                    {
                        int downSamplePass = (int)EBlurPass.DualFiltering_DownSample;
                        int upSamplePass = (int)EBlurPass.DualFiltering_UpSample;

                        int downSampleCount = Mathf.FloorToInt(_data.m_Iteration / 2f);
                        m_Material.SetFloat(ID_BlurSize, _data.m_BlurSize / _data.m_DownSample*4f);

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
                case EBlurType.Grainy:
                    {
                        int grainyPass = (int)EBlurPass.Grainy;
                        m_Material.SetFloat(ID_BlurSize, _data.m_BlurSize * 32);
                        _buffer.Blit(_src, _dst, m_Material, grainyPass);
                    }
                    break;
                case EBlurType.Bokeh:
                    {
                        int bokehPass = (int)EBlurPass.Bokeh;
                        m_Material.SetFloat(ID_BlurSize, _data.m_BlurSize);
                        m_Material.SetInt(ID_Iteration, _data.m_Iteration * 32);
                        m_Material.SetFloat(ID_Angle, _data.m_Angle);
                        _buffer.Blit(_src, _dst, m_Material, bokehPass);
                    }
                    break;
                case EBlurType.Hexagon:
                    {
                        int verticalPass = (int)EBlurPass.Hexagon_Vertical;
                        int diagonalPass = (int)EBlurPass.Hexagon_Diagonal;
                        int rhomboidPass = (int)EBlurPass.Hexagon_Rhomboid;

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
                case EBlurType.Radial:
                    {
                        int pass = (int)EBlurPass.Radial;

                        m_Material.SetFloat(ID_BlurSize, _data.m_BlurSize * 32);
                        m_Material.SetVector(ID_Vector, _data.m_Vector);
                        m_Material.SetInt(ID_Iteration, _data.m_Iteration);
                        _buffer.Blit(_src, _dst, m_Material, pass);
                    }
                    break;
                case EBlurType.Directional:
                    {
                        int pass = (int)EBlurPass.Directional;
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