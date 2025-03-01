using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Rendering.Pipeline.Mask;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.PostProcess
{
    public class PostProcess_Blurs : APostProcessBehaviour<FBlursCore, DBlurs>
    {
        public override bool m_OpaqueProcess => false;
        public override EPostProcess Event => EPostProcess.DepthOfField;

        public enum_Focal m_Focal;
        [Foldout(nameof(m_Focal),enum_Focal._DOF_DISTANCE,enum_Focal._DOF_DISTANCE_MASK)] public RangeFloat m_FocalData;
        [Foldout(nameof(m_Focal),enum_Focal._DOF_MASK,enum_Focal._DOF_DISTANCE_MASK)] public MaskTextureData m_FocalMaskData;

        private static readonly int kIDFocalBegin = Shader.PropertyToID("_FocalStart");
        private static readonly int kIDFocalEnd = Shader.PropertyToID("_FocalEnd");
        public static readonly int kFocalMaskID = Shader.PropertyToID("_CameraFocalMaskTexture");
        public static readonly string kDOF_Distance = "_DOF_DISTANCE";
        public static readonly string kDOF_Mask = "_DOF_MASK";
        public static readonly RenderTargetIdentifier kFocalMaskRT = new RenderTargetIdentifier(kFocalMaskID);

        public void SetFocalDistance(float _distance)
        {
            m_FocalData = new RangeFloat(_distance, _distance);
            SetDirty();
        }
        
        protected override void ApplyParameters()
        {
            base.ApplyParameters();
            var material = m_Effect.m_Material;
            if (material.EnableKeyword(kDOF_Distance, m_Focal is enum_Focal._DOF_DISTANCE or enum_Focal._DOF_DISTANCE_MASK))
            {
                material.SetFloat(kIDFocalBegin, m_FocalData.start);
                material.SetFloat(kIDFocalEnd, m_FocalData.end);
            }
            material.EnableKeyword(kDOF_Mask, m_Focal is enum_Focal._DOF_MASK or enum_Focal._DOF_DISTANCE_MASK);
        }

        public override void Execute(CommandBuffer _buffer, RenderTargetIdentifier _src, RenderTargetIdentifier _dst,
            RenderTextureDescriptor _executeData,  ScriptableRenderContext _context, ref RenderingData _renderingData)
        {
            if (m_Focal == enum_Focal._DOF_DISTANCE)
            {
                base.Execute(_buffer, _src, _dst, _executeData, _context, ref _renderingData);
                return;
            }

            var descriptor = _executeData;
            descriptor.colorFormat = RenderTextureFormat.R8;
            descriptor.depthBufferBits = 0;
            _buffer.GetTemporaryRT(kFocalMaskID, descriptor);
            _context.ExecuteCommandBuffer(_buffer);
            _buffer.Clear();
            
            MaskTexturePass.DrawMask(kFocalMaskRT,_context,ref _renderingData,m_FocalMaskData);
            base.Execute(_buffer, _src, _dst, _executeData, _context, ref _renderingData);
            _buffer.ReleaseTemporaryRT(kFocalMaskID);
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
        LightStreak,
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

        Blinking_Radial,
        Blinking_Combine,
        
        NextGen_DownSample,
        NextGen_UpSample,
        NextGen_UpSampleFinal,
    }

    public enum enum_Focal
    {
        None = 0,
        _DOF_DISTANCE,
        _DOF_MASK,
        _DOF_DISTANCE_MASK,
    }
    [Serializable]
    public struct DBlurs:IPostProcessParameter
    {
        public EBlurType m_BlurType;
        [Fold(nameof(m_BlurType), EBlurType.None)] [Range(0.05f, 2f)] public float m_BlurSize;
        [Fold(nameof(m_BlurType),  EBlurType.None,EBlurType.Grainy)]
        [Range(1, FBlursCore.kMaxIteration)] public int m_Iteration;
        [Foldout(nameof(m_BlurType), EBlurType.Kawase, EBlurType.GaussianVHSeperated, EBlurType.AverageVHSeperated, EBlurType.Hexagon, EBlurType.DualFiltering,EBlurType.NextGen,EBlurType.LightStreak)]
        [Range(1, 4)] public int m_DownSample;
        [Foldout(nameof(m_BlurType), EBlurType.Hexagon, EBlurType.Bokeh)]
        [Range(-1, 1)] public float m_Angle;
        [Foldout(nameof(m_BlurType), EBlurType.LightStreak)]
        [RangeVector(0, 1)] public Vector2 m_Vector;
        [Foldout(nameof(m_BlurType), EBlurType.LightStreak)]
        [Range(.9f, 1f)] public float m_Attenuation;
        
        public bool Validate() => m_BlurType != EBlurType.None;
        
        public static readonly DBlurs kDefault = new DBlurs() {
            m_BlurSize = 1.3f,
            m_DownSample = 2,
            m_Iteration = 7,
            m_BlurType = EBlurType.DualFiltering,
            m_Angle = 0,
            m_Vector = Vector2.one * .5f,
            m_Attenuation =  1f,
        };

        public static readonly DBlurs kNone = new() {
            m_BlurSize = 1.3f,
            m_DownSample = 2,
            m_Iteration = 7,
            m_BlurType = EBlurType.None,
            m_Angle = 0,
            m_Vector = Vector2.one * .5f,
            m_Attenuation =  1f,
        };
    }


    [Serializable]
    public class FBlursCore : PostProcessCore<DBlurs>
    {
        public static FBlursCore Instance => kInstance;
        private static readonly PassiveInstance<FBlursCore> kInstance = new (()=> new ());
        
        #region ShaderProperties

        private static readonly int kIDBlurSize = Shader.PropertyToID("_BlurSize");
        private static readonly int kIDIteration = Shader.PropertyToID("_Iteration");
        private static readonly int kIDAngle = Shader.PropertyToID("_Angle");
        private static readonly int kIDVector = Shader.PropertyToID("_Vector");
        private static readonly int kIDAttenuation = Shader.PropertyToID("_Attenuation");

        private static readonly string kKWFirstBlur = "_FIRSTBLUR";
        private static readonly string kKWFinalBlur = "_FINALBLUR";
        private static readonly string kKWEncoding = "_ENCODE";

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

        static bool EncodingRequired(EBlurType _type)
        {
            switch (_type)
            {
                case EBlurType.Kawase:
                case EBlurType.AverageVHSeperated:
                case EBlurType.GaussianVHSeperated:
                case EBlurType.DualFiltering:
                case EBlurType.NextGen:
                case EBlurType.LightStreak:
                    return true;
            }

            return false;
        }

        private static readonly Dictionary<EBlurType, string> kBlurSample = UEnum.GetEnums<EBlurType>().ToDictionary(p=>p,p=>p.ToString());
        static readonly int kBlurTempID1 = Shader.PropertyToID("_PostProcessing_Blit_Blur_Temp1");
        static readonly RenderTargetIdentifier kBlurTempRT1 = new RenderTargetIdentifier(kBlurTempID1);
        static readonly int kBlurTempID2 = Shader.PropertyToID("_PostProcessing_Blit_Blur_Temp2");
        static readonly RenderTargetIdentifier kBlurTempRT2 = new RenderTargetIdentifier(kBlurTempID2);
        static readonly int kBlurTempID3 = Shader.PropertyToID("_PostProcessing_Blit_Blur_Temp3");
        static readonly RenderTargetIdentifier kBlurTempRT3 = new RenderTargetIdentifier(kBlurTempID3);

        private static readonly int kBlinkingVerticalID = Shader.PropertyToID("_Blinking_Vertical");
        static readonly RenderTargetIdentifier kBlinkingVerticalRT = new RenderTargetIdentifier(kBlinkingVerticalID);
        private static readonly int kBlinkingHorizontalID = Shader.PropertyToID("_Blinking_Horizontal");
        static readonly RenderTargetIdentifier kBlinkingHorizontalRT = new RenderTargetIdentifier(kBlinkingHorizontalID);
        
        static readonly int kHexagonVerticalID = Shader.PropertyToID("_Hexagon_Vertical");
        static readonly RenderTargetIdentifier kVerticalRT = new RenderTargetIdentifier(kHexagonVerticalID);
        static readonly int kHexagonDiagonalID = Shader.PropertyToID("_Hexagon_Diagonal");
        static readonly RenderTargetIdentifier kDiagonalRT = new RenderTargetIdentifier(kHexagonDiagonalID);

        public override void Execute(RenderTextureDescriptor _descriptor, ref DBlurs _data, CommandBuffer _buffer,
            RenderTargetIdentifier _src, RenderTargetIdentifier _dst, ScriptableRenderContext _context, ref RenderingData _renderingData)
        {
            var sampleName = kBlurSample[_data.m_BlurType];
            _buffer.BeginSample(sampleName);
            var baseDownSample = math.max(_data.m_DownSample, 1);
            var startWidth = _descriptor.width / baseDownSample;
            var startHeight = _descriptor.height / baseDownSample;
            m_Material.EnableKeyword(kKWEncoding,EncodingRequired(_data.m_BlurType) && _descriptor.colorFormat!=RenderTextureFormat.ARGB32);
            
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
                            m_Material.SetFloat(kIDBlurSize, _data.m_BlurSize / _data.m_DownSample * (1 + i));
                            switch (_data.m_BlurType)
                            {
                                case EBlurType.Kawase:
                                    {
                                        int pass = (int)EBlurPass.Kawase;
                                        _buffer.EnableKeyword(kKWFirstBlur, i==0);
                                        _buffer.EnableKeyword(kKWFinalBlur,i==_data.m_Iteration-1);
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
                                        
                                        _buffer.GetTemporaryRT(kBlurTempID3, startWidth, startHeight, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
                                        _buffer.EnableKeyword(kKWFirstBlur, i==0);
                                        _buffer.Blit(blitSrc, kBlurTempRT3, m_Material, horizontalPass);
                                        _buffer.EnableKeyword(kKWFirstBlur, false);
                                        _buffer.EnableKeyword(kKWFinalBlur,i==_data.m_Iteration-1);
                                        _buffer.Blit(kBlurTempRT3, blitTarget, m_Material, verticalPass);
                                        _buffer.EnableKeyword(kKWFinalBlur,false);
                                        _buffer.ReleaseTemporaryRT(kBlurTempID3);
                                    }
                                    break;
                            }
                        }
                        _buffer.ReleaseTemporaryRT(kBlurTempID1);
                        _buffer.ReleaseTemporaryRT(kBlurTempID2);
                        _buffer.EnableKeyword(kKWFinalBlur,false);
                    }
                    break;
                case EBlurType.DualFiltering:
                    {
                        int downSamplePass = (int)EBlurPass.DualFiltering_DownSample;
                        int upSamplePass = (int)EBlurPass.DualFiltering_UpSample;

                        int downSampleCount = Mathf.FloorToInt(_data.m_Iteration / 2f);
                        m_Material.SetFloat(kIDBlurSize, _data.m_BlurSize / _data.m_DownSample*4f);

                        for (int i = 0; i < _data.m_Iteration - 1; i++)
                        {
                            int filterSample = downSampleCount - Mathf.CeilToInt(Mathf.Abs(_data.m_Iteration / 2f - (i + 1))) + 1 + _data.m_Iteration % 2;
                            int filterWidth = startWidth / filterSample;
                            int filterHeight = startHeight / filterSample;
                            _buffer.GetTemporaryRT(kDualFilteringIDs[i], filterWidth, filterHeight, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
                        }
                        for (int i = 0; i < _data.m_Iteration; i++)
                        {
                            _buffer.EnableKeyword(kKWFirstBlur, i==0);
                            _buffer.EnableKeyword(kKWFinalBlur,i==_data.m_Iteration-1);
                            int filterPass = i <= downSampleCount ? downSamplePass : upSamplePass;
                            RenderTargetIdentifier blitSrc = i == 0 ? _src : kDualFilteringRTs[i - 1];
                            RenderTargetIdentifier blitTarget = i == _data.m_Iteration - 1 ? _dst : kDualFilteringRTs[i];
                            _buffer.Blit(blitSrc, blitTarget, m_Material, filterPass);
                        }
                        for (int i = 0; i < _data.m_Iteration - 1; i++)
                            _buffer.ReleaseTemporaryRT(kDualFilteringIDs[i]);
                        _buffer.EnableKeyword(kKWFinalBlur,false);
                    }
                    break;
                case EBlurType.Grainy:
                    {
                        int grainyPass = (int)EBlurPass.Grainy;
                        m_Material.SetFloat(kIDBlurSize, _data.m_BlurSize * 32);
                        _buffer.Blit(_src, _dst, m_Material, grainyPass);
                    }
                    break;
                case EBlurType.Bokeh:
                    {
                        int bokehPass = (int)EBlurPass.Bokeh;
                        m_Material.SetFloat(kIDBlurSize, _data.m_BlurSize);
                        m_Material.SetInt(kIDIteration, _data.m_Iteration * 32);
                        m_Material.SetFloat(kIDAngle, _data.m_Angle);
                        _buffer.Blit(_src, _dst, m_Material, bokehPass);
                    }
                    break;
                case EBlurType.Hexagon:
                    {
                        var verticalPass = (int)EBlurPass.Hexagon_Vertical;
                        var diagonalPass = (int)EBlurPass.Hexagon_Diagonal;
                        var rhomboidPass = (int)EBlurPass.Hexagon_Rhomboid;

                        m_Material.SetFloat(kIDBlurSize, _data.m_BlurSize * 2);
                        m_Material.SetFloat(kIDIteration, _data.m_Iteration * 2);
                        m_Material.SetFloat(kIDAngle, _data.m_Angle);
                        _buffer.GetTemporaryRT(kHexagonVerticalID, startWidth, startHeight, 0, FilterMode.Bilinear, _descriptor.colorFormat);
                        _buffer.GetTemporaryRT(kHexagonDiagonalID, startWidth, startHeight, 0, FilterMode.Bilinear, _descriptor.colorFormat);

                        _buffer.Blit(_src, kVerticalRT, m_Material, verticalPass);
                        _buffer.Blit(_src, kDiagonalRT, m_Material, diagonalPass);
                        _buffer.Blit(_src, _dst, m_Material, rhomboidPass);

                        _buffer.ReleaseTemporaryRT(kHexagonVerticalID);
                        _buffer.ReleaseTemporaryRT(kHexagonDiagonalID);
                    }
                    break;
                case EBlurType.LightStreak:
                {
                    var radialPass = (int)EBlurPass.Blinking_Radial;
                    var combinePass = (int) EBlurPass.Blinking_Combine;
                    
                    _buffer.GetTemporaryRT(kBlurTempID1, startWidth, startHeight, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
                    _buffer.GetTemporaryRT(kBlurTempID2, startWidth, startHeight, 0, FilterMode.Bilinear,  RenderTextureFormat.ARGB32);
                    _buffer.GetTemporaryRT(kBlinkingVerticalID,startWidth,startHeight,0,FilterMode.Bilinear, RenderTextureFormat.ARGB32);
                    _buffer.GetTemporaryRT(kBlinkingHorizontalID,startWidth,startHeight,0,FilterMode.Bilinear, RenderTextureFormat.ARGB32);
                    Action<int, Vector2, float, int, float, RenderTargetIdentifier> DoRadialBlur = (_iteration, _direction, _blurSize, _downSample, _attenuation, _RTid) => {
                        var radialDirection = _direction;
                        m_Material.SetVector(kIDAttenuation, new Vector4(1, _attenuation, umath.sqr(_attenuation), umath.pow3(_attenuation)));
                        for (var i = 0; i < _iteration; i++)
                        {
                            var blitSrc =  i == 0 ? _src : (i % 2 == 0 ? kBlurTempRT1 : kBlurTempRT2);
                            var blitTarget = i == _iteration - 1  ? _RTid  : (i % 2 == 0 ? kBlurTempRT2 : kBlurTempRT1);
                            _buffer.EnableKeyword(kKWFirstBlur, i == 0);
                            _buffer.SetGlobalVector(kIDVector, new Vector4(radialDirection.x, radialDirection.y, -radialDirection.x, -radialDirection.y) * _blurSize / _downSample * (1 + i));
                            _buffer.Blit(blitSrc, blitTarget, m_Material, radialPass);
                        }
                    };

                    DoRadialBlur(_data.m_Iteration,_data.m_Vector,_data.m_BlurSize,_data.m_DownSample,_data.m_Attenuation,kBlinkingVerticalRT);
                    DoRadialBlur(_data.m_Iteration, KRotation.kRotateCW90.mul(_data.m_Vector),_data.m_BlurSize,_data.m_DownSample,_data.m_Attenuation,kBlinkingHorizontalRT);
                    
                    _buffer.EnableKeyword(kKWFirstBlur, false);
                    _buffer.EnableKeyword(kKWFinalBlur,true);
                    _buffer.Blit(_src,_dst,m_Material,combinePass);
                    
                    
                    _buffer.ReleaseTemporaryRT(kBlurTempID1);
                    _buffer.ReleaseTemporaryRT(kBlurTempID2);
                    _buffer.ReleaseTemporaryRT(kBlinkingVerticalID);
                    _buffer.ReleaseTemporaryRT(kBlinkingHorizontalID);
                    _buffer.EnableKeyword(kKWFinalBlur,false);
                }
                    break;
                case EBlurType.NextGen:
                    {
                        var iteration = Mathf.Clamp(_data.m_Iteration,2,(int)Mathf.Log(Mathf.ClosestPowerOfTwo(Mathf.Min(startWidth,startHeight)) -1,2));
                        var downSamplePass = (int)EBlurPass.NextGen_DownSample;
                        var upSamplePass = (int)EBlurPass.NextGen_UpSample;
                        var upSampleFinalPass = (int) EBlurPass.NextGen_UpSampleFinal;

                        m_Material.SetFloat(kIDBlurSize, _data.m_BlurSize / _data.m_DownSample*4f);

                        for (var i = 0; i < iteration; i++)
                        {
                            var downSample = umath.pow( 2,i + 1 );
                            _buffer.GetTemporaryRT(kNextGenDownIDs[i], startWidth /  downSample, startHeight / downSample, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);

                            var upSample = umath.pow(2, iteration - i - 1);
                            if(i<iteration-1)
                                _buffer.GetTemporaryRT(kNextGenUpIDs[i], startWidth / upSample, startHeight / upSample, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
                        }
                        
                        _buffer.BeginSample("Next Gen Down");
                        for (var i = 0 ; i < iteration ; i++)
                        {
                            _buffer.EnableKeyword(kKWFirstBlur, i==0);
                            RenderTargetIdentifier blitSrc = i == 0 ? _src : kNextGenDownIDs[i - 1];
                            RenderTargetIdentifier blitTarget = kNextGenDownIDs[i];
                            _buffer.Blit(blitSrc, blitTarget, m_Material, downSamplePass);
                        }
                        _buffer.EndSample("Next Gen Down");

                        _buffer.BeginSample("Next Gen Up");
                        for (var i = 0 ; i < iteration -1 ; i++)
                        {
                            RenderTargetIdentifier blitSrc = i == 0 ? kNextGenDownIDs[iteration-1] : kNextGenUpIDs[i - 1];
                            RenderTargetIdentifier blitTarget = kNextGenUpIDs[i];
                            _buffer.SetGlobalTexture("_PreDownSample",kNextGenDownIDs[iteration-2-i]);
                            _buffer.Blit(blitSrc, blitTarget, m_Material, upSamplePass);
                        }
                        _buffer.EndSample("Next Gen Up");
                        
                        _buffer.EnableKeyword(kKWFinalBlur,true);
                        _buffer.Blit(kNextGenUpIDs[iteration-2],_dst,m_Material,upSampleFinalPass);
                        
                        for (var i = 0; i < iteration; i++)
                        {
                            _buffer.ReleaseTemporaryRT(kNextGenDownIDs[i]);
                            if(i<iteration-1)
                                _buffer.ReleaseTemporaryRT(kNextGenUpIDs[i]);
                        }
                        _buffer.EnableKeyword(kKWFinalBlur,false);
                    }
                    break;
            }
            _buffer.EndSample(sampleName);
        }
    }
}