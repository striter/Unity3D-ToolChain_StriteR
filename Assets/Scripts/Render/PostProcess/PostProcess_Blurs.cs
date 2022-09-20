using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.PostProcess
{
    public class PostProcess_Blurs : PostProcessBehaviour<PPCore_Blurs, PPData_Blurs>
    {
        public override bool m_OpaqueProcess => false;
        public override EPostProcess Event => EPostProcess.DepthOfField;

        public enum_Focal m_Focal;
        [MFoldout(nameof(m_Focal),enum_Focal._DOF)]public PPData_DepthOfField m_FocalData;

        protected override void ApplyParameters()
        {
            base.ApplyParameters();
            m_Effect?.SetFocal(m_Focal,ref m_FocalData);
        }
    }

    public enum EBlurType
    {
        None=-1,
        Kawase = 0,
        AverageVHSeperated,
        GaussianVHSeperated,
        DualFiltering,
        Grainy,
        Hexagon,
        Bokeh,
        Directional,
        Radial,
        NextGen,
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
        
        NextGen_DownSample,
        NextGen_UpSample,
        NextGen_UpSampleFinal,
    }

    public enum enum_Focal
    {
        None=0,
        _DOF,
        _DOF_MASK,
    }
    [Serializable]
    public struct PPData_Blurs:IPostProcessParameter
    {
        [MTitle] public EBlurType m_BlurType;
        [MFold(nameof(m_BlurType), EBlurType.None)] [Range(0.05f, 2f)] public float m_BlurSize;
        [MFold(nameof(m_BlurType),  EBlurType.None,EBlurType.Grainy)]
        [Range(1, PPCore_Blurs.kMaxIteration)] public int m_Iteration;
        [MFoldout(nameof(m_BlurType), EBlurType.Kawase, EBlurType.GaussianVHSeperated, EBlurType.AverageVHSeperated, EBlurType.Hexagon, EBlurType.DualFiltering,EBlurType.NextGen)]
        [Range(1, 4)] public int m_DownSample;
        [MFoldout(nameof(m_BlurType), EBlurType.Hexagon, EBlurType.Bokeh)]
        [Range(-1, 1)] public float m_Angle;
        [MFoldout(nameof(m_BlurType), EBlurType.Directional, EBlurType.Radial)]
        [RangeVector(0, 1)] public Vector2 m_Vector;
        public bool Validate() => m_BlurType != EBlurType.None;
        public static readonly PPData_Blurs kDefault = new PPData_Blurs()
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

        public static readonly PPData_DepthOfField kDefault = new PPData_DepthOfField()
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

        private static readonly string[] KW_Focal = {"", ""};
        private static readonly int ID_FocalBegin = Shader.PropertyToID("_FocalStart");
        private static readonly int ID_FocalEnd = Shader.PropertyToID("_FocalEnd");

        private static readonly string KW_FirstBlur = "_FIRSTBLUR";
        private static readonly string KW_FinalBlur = "_FINALBLUR";
        private static readonly string KW_Encoding = "_ENCODE";

        public bool SetFocal(enum_Focal _focal, ref PPData_DepthOfField focalData)
        {
            if (m_Material.EnableKeywords(_focal))
            {
                m_Material.SetFloat(ID_FocalBegin, focalData.m_Begin);
                m_Material.SetFloat(ID_FocalEnd, focalData.m_Begin + focalData.m_Width);
            }

            return _focal != enum_Focal.None;
        }

        #endregion

        public const int kMaxIteration = 12;
        private static readonly int[] kDualFilteringIDs = GetRenderTextureIDs("_Blur_DualFiltering",kMaxIteration);
        private static readonly int[] kNextGenUpIDs = GetRenderTextureIDs("_Blur_NextGen_UP",kMaxIteration);
        private static readonly int[] kNextGenDownIDs = GetRenderTextureIDs("_Blur_NextGen_DOWN",kMaxIteration);

        private static readonly RenderTargetIdentifier[] kDualFilteringRTs = ConvertToRTIdentifier(kDualFilteringIDs);
        static int[] GetRenderTextureIDs(string _id,int _size)
        {
            int[] dualFilteringIDArray = new int[_size];
            for(int i=0;i<_size;i++)
                dualFilteringIDArray[i] = Shader.PropertyToID(_id + i);
            return dualFilteringIDArray;
        }

        static RenderTargetIdentifier[] ConvertToRTIdentifier(int[] _ids)
        {
            RenderTargetIdentifier[] identifiers = new RenderTargetIdentifier[_ids.Length];
            for (int i = 0; i < _ids.Length; i++)
                identifiers[i] = new RenderTargetIdentifier(_ids[i]);
            return identifiers;
        }

        private static readonly Dictionary<EBlurType, string> kBlurSample = UEnum.GetEnums<EBlurType>().ToDictionary(p=>p,p=>p.ToString());
        static readonly int kBlurTempID1 = Shader.PropertyToID("_PostProcessing_Blit_Blur_Temp1");
        static readonly RenderTargetIdentifier kBlurTempRT1 = new RenderTargetIdentifier(kBlurTempID1);
        static readonly int kBlurTempID2 = Shader.PropertyToID("_PostProcessing_Blit_Blur_Temp2");
        static readonly RenderTargetIdentifier kBlurTempRT2 = new RenderTargetIdentifier(kBlurTempID2);
        static readonly int kBlurTempID3 = Shader.PropertyToID("_PostProcessing_Blit_Blur_Temp3");
        static readonly RenderTargetIdentifier kBlurTempRT3 = new RenderTargetIdentifier(kBlurTempID3);

        static readonly int kVerticalID = Shader.PropertyToID("_Hexagon_Vertical");
        static readonly RenderTargetIdentifier kVerticalRT = new RenderTargetIdentifier(kVerticalID);
        static readonly int kDiagonalID = Shader.PropertyToID("_Hexagon_Diagonal");
        static readonly RenderTargetIdentifier kDiagonalRT = new RenderTargetIdentifier(kDiagonalID);

        public override void ExecutePostProcessBuffer(CommandBuffer _buffer, RenderTargetIdentifier _src, RenderTargetIdentifier _dst, RenderTextureDescriptor _descriptor,ref PPData_Blurs _data)
        {
            if (_data.m_DownSample <= 0)
            {
                Debug.LogWarning("Invalid Down Sample!");
                _buffer.Blit(_src, _dst);
                return;
            }

            var sampleName = kBlurSample[_data.m_BlurType];
            _buffer.BeginSample(sampleName);
            int startWidth = _descriptor.width / _data.m_DownSample;
            int startHeight = _descriptor.height / _data.m_DownSample;
            m_Material.EnableKeyword(KW_Encoding,_descriptor.colorFormat!=RenderTextureFormat.ARGB32);
            switch (_data.m_BlurType)
            {
                case EBlurType.Kawase:
                case EBlurType.AverageVHSeperated:
                case EBlurType.GaussianVHSeperated:
                    {
                        _buffer.GetTemporaryRT(kBlurTempID1, startWidth, startHeight, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);

                        _buffer.GetTemporaryRT(kBlurTempID2, startWidth, startHeight, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
                        for (int i = 0; i < _data.m_Iteration; i++)
                        {
                            RenderTargetIdentifier blitSrc = i == 0 ? _src : (i % 2 == 0 ? kBlurTempRT1 : kBlurTempRT2);
                            RenderTargetIdentifier blitTarget = i == _data.m_Iteration - 1 ? _dst : (i % 2 == 0 ? kBlurTempRT2 : kBlurTempRT1);
                            switch (_data.m_BlurType)
                            {
                                case EBlurType.Kawase:
                                    {
                                        int pass = (int)EBlurPass.Kawase;
                                        m_Material.SetInt(ID_BlurSize, (int)(_data.m_BlurSize / _data.m_DownSample * (1 + i)));
                                        _buffer.EnableKeyword(KW_FirstBlur, i==0);
                                        _buffer.EnableKeyword(KW_FinalBlur,i==_data.m_Iteration-1);
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
                                        
                                        _buffer.GetTemporaryRT(kBlurTempID3, startWidth, startHeight, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
                                        _buffer.EnableKeyword(KW_FirstBlur, i==0);
                                        _buffer.Blit(blitSrc, kBlurTempRT3, m_Material, horizontalPass);
                                        _buffer.EnableKeyword(KW_FirstBlur, false);
                                        _buffer.EnableKeyword(KW_FinalBlur,i==_data.m_Iteration-1);
                                        _buffer.Blit(kBlurTempRT3, blitTarget, m_Material, verticalPass);
                                        _buffer.EnableKeyword(KW_FinalBlur,false);
                                        _buffer.ReleaseTemporaryRT(kBlurTempID3);
                                    }
                                    break;
                            }
                        }
                        _buffer.ReleaseTemporaryRT(kBlurTempID1);
                        _buffer.ReleaseTemporaryRT(kBlurTempID2);
                        _buffer.EnableKeyword(KW_FinalBlur,false);
                    }
                    break;
                case EBlurType.DualFiltering:
                    {
                        int downSamplePass = (int)EBlurPass.DualFiltering_DownSample;
                        int upSamplePass = (int)EBlurPass.DualFiltering_UpSample;

                        
                        int downSampleCount = Mathf.FloorToInt(_data.m_Iteration / 2f);
                        m_Material.SetFloat(ID_BlurSize, _data.m_BlurSize / _data.m_DownSample*4f);

                        for (int i = 0; i < _data.m_Iteration - 1; i++)
                        {
                            int filterSample = downSampleCount - Mathf.CeilToInt(Mathf.Abs(_data.m_Iteration / 2f - (i + 1))) + 1 + _data.m_Iteration % 2;
                            int filterWidth = startWidth / filterSample;
                            int filterHeight = startHeight / filterSample;
                            _buffer.GetTemporaryRT(kDualFilteringIDs[i], filterWidth, filterHeight, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
                        }
                        for (int i = 0; i < _data.m_Iteration; i++)
                        {
                            _buffer.EnableKeyword(KW_FirstBlur, i==0);
                            _buffer.EnableKeyword(KW_FinalBlur,i==_data.m_Iteration-1);
                            int filterPass = i <= downSampleCount ? downSamplePass : upSamplePass;
                            RenderTargetIdentifier blitSrc = i == 0 ? _src : kDualFilteringRTs[i - 1];
                            RenderTargetIdentifier blitTarget = i == _data.m_Iteration - 1 ? _dst : kDualFilteringRTs[i];
                            _buffer.Blit(blitSrc, blitTarget, m_Material, filterPass);
                        }
                        for (int i = 0; i < _data.m_Iteration - 1; i++)
                            _buffer.ReleaseTemporaryRT(kDualFilteringIDs[i]);
                        _buffer.EnableKeyword(KW_FinalBlur,false);
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
                        _buffer.GetTemporaryRT(kVerticalID, startWidth, startHeight, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
                        _buffer.GetTemporaryRT(kDiagonalID, startWidth, startHeight, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);

                        _buffer.Blit(_src, kVerticalRT, m_Material, verticalPass);
                        _buffer.Blit(_src, kDiagonalRT, m_Material, diagonalPass);
                        _buffer.Blit(_src, _dst, m_Material, rhomboidPass);

                        _buffer.ReleaseTemporaryRT(kVerticalID);
                        _buffer.ReleaseTemporaryRT(kDiagonalID);
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
                case EBlurType.NextGen:
                    {
                        int iteration = Mathf.Clamp(_data.m_Iteration,2,(int)Mathf.Log(Mathf.ClosestPowerOfTwo(Mathf.Min(startWidth,startHeight)) -1,2));
                        int downSamplePass = (int)EBlurPass.NextGen_DownSample;
                        int upSamplePass = (int)EBlurPass.NextGen_UpSample;
                        int upSampleFinalPass = (int) EBlurPass.NextGen_UpSampleFinal;

                        m_Material.SetFloat(ID_BlurSize, _data.m_BlurSize / _data.m_DownSample*4f);

                        for (int i = 0; i < iteration; i++)
                        {
                            int downSample = UMath.Pow( 2,i + 1 );
                            _buffer.GetTemporaryRT(kNextGenDownIDs[i], startWidth /  downSample, startHeight / downSample, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);

                            int upSample = UMath.Pow(2, iteration - i - 1);
                            if(i<iteration-1)
                                _buffer.GetTemporaryRT(kNextGenUpIDs[i], startWidth / upSample, startHeight / upSample, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
                        }
                        
                        _buffer.BeginSample("Next Gen Down");
                        for (int i = 0 ; i < iteration ; i++)
                        {
                            _buffer.EnableKeyword(KW_FirstBlur, i==0);
                            RenderTargetIdentifier blitSrc = i == 0 ? _src : kNextGenDownIDs[i - 1];
                            RenderTargetIdentifier blitTarget = kNextGenDownIDs[i];
                            _buffer.Blit(blitSrc, blitTarget, m_Material, downSamplePass);
                        }
                        _buffer.EndSample("Next Gen Down");

                        _buffer.BeginSample("Next Gen Up");
                        for (int i = 0 ; i < iteration -1 ; i++)
                        {
                            RenderTargetIdentifier blitSrc = i == 0 ? kNextGenDownIDs[iteration-1] : kNextGenUpIDs[i - 1];
                            RenderTargetIdentifier blitTarget = kNextGenUpIDs[i];
                            _buffer.SetGlobalTexture("_PreDownSample",kNextGenDownIDs[iteration-2-i]);
                            _buffer.Blit(blitSrc, blitTarget, m_Material, upSamplePass);
                        }
                        _buffer.EndSample("Next Gen Up");
                        
                        _buffer.EnableKeyword(KW_FinalBlur,true);
                        _buffer.Blit(kNextGenUpIDs[iteration-2],_dst,m_Material,upSampleFinalPass);
                        
                        
                        for (int i = 0; i < iteration; i++)
                        {
                            _buffer.ReleaseTemporaryRT(kNextGenDownIDs[i]);
                            if(i<iteration-1)
                                _buffer.ReleaseTemporaryRT(kNextGenUpIDs[i]);
                        }
                        _buffer.EnableKeyword(KW_FinalBlur,false);
                    }
                    break;
            }
            _buffer.EndSample(sampleName);
        }
    }
}