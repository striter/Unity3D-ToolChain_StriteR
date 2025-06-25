using System;
using Rendering.Pipeline.Mask;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.PostProcess
{
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
        public EBlurType blurType;
        [Fold(nameof(blurType), EBlurType.None)]
        [Range(0.05f, 2f)] public float blurSize;
        [Fold(nameof(blurType),  EBlurType.None,EBlurType.Grainy)]
        [Range(1, FBlursCore.kMaxIteration)] public int iteration;
        [Foldout(nameof(blurType), EBlurType.Kawase, EBlurType.GaussianVHSeperated, EBlurType.AverageVHSeperated, EBlurType.Hexagon, EBlurType.DualFiltering,EBlurType.NextGen,EBlurType.LightStreak)]
        [Range(1, 4)] public int downSample;
        [Foldout(nameof(blurType), EBlurType.Hexagon, EBlurType.Bokeh,EBlurType.LightStreak)]
        [Range(-1, 1)] public float angle;
        [Foldout(nameof(blurType), EBlurType.LightStreak)]
        [Range(.9f, 1f)] public float attenuation;
        
        public bool Validate() => blurType != EBlurType.None && blurSize > 0 && iteration > 0 && downSample > 0;
        
        public static readonly DBlurs kDefault = new DBlurs() {
            blurSize = 1.3f,
            downSample = 2,
            iteration = 7,
            blurType = EBlurType.DualFiltering,
            angle = 0,
            attenuation =  1f,
        };

        public static readonly DBlurs kNone = new() {
            blurSize = 1.3f,
            downSample = 2,
            iteration = 7,
            blurType = EBlurType.None,
            angle = 0,
            attenuation =  1f,
        };
    }
    
    public class PostProcess_Blurs : APostProcessBehaviour<FBlursCore, DBlurs>
    {
        public override bool OpaqueProcess => false;
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
        public override bool Validate(ref RenderingData _renderingData)
        {
            var material = m_Effect.m_Material;
            if (material.EnableKeyword(kDOF_Distance, m_Focal is enum_Focal._DOF_DISTANCE or enum_Focal._DOF_DISTANCE_MASK))
            {
                material.SetFloat(kIDFocalBegin, m_FocalData.start);
                material.SetFloat(kIDFocalEnd, m_FocalData.end);
            }
            material.EnableKeyword(kDOF_Mask, m_Focal is enum_Focal._DOF_MASK or enum_Focal._DOF_DISTANCE_MASK);
            return base.Validate(ref _renderingData);
        }
        
        public override void Configure(CommandBuffer _buffer, RenderTextureDescriptor _descriptor)
        {
            base.Configure(_buffer, _descriptor);
            switch (m_Focal)
            {
                case enum_Focal._DOF_MASK:
                case enum_Focal._DOF_DISTANCE_MASK:
                {
                    _descriptor.colorFormat = RenderTextureFormat.R8;
                    _descriptor.depthBufferBits = 0;
                    _buffer.GetTemporaryRT(kFocalMaskID, _descriptor);
                    
                }
                    break;
            }
        }

        public override void Execute(CommandBuffer _buffer, RenderTargetIdentifier _src, RenderTargetIdentifier _dst,
            RenderTextureDescriptor _executeData,  ScriptableRenderContext _context, ref RenderingData _renderingData)
        {
            switch (m_Focal)
            {
                case enum_Focal.None:
                case enum_Focal._DOF_DISTANCE:
                {
                    base.Execute(_buffer, _src, _dst, _executeData, _context, ref _renderingData);
                }
                    break;
                case enum_Focal._DOF_MASK:
                case enum_Focal._DOF_DISTANCE_MASK:
                {
                    MaskTexturePass.DrawMask(_buffer,kFocalMaskRT,_context,ref _renderingData,m_FocalMaskData);
                    base.Execute(_buffer, _src, _dst, _executeData, _context, ref _renderingData);
                }
                    break;
                default:
                {
                    Debug.LogError($"Unknown Focal {m_Focal}");
                    _buffer.Blit(_src, _dst);
                }
                    break;
            }
        }

        public override void FrameCleanUp(CommandBuffer _buffer)
        {
            base.FrameCleanUp(_buffer);
            switch (m_Focal)
            {
                case enum_Focal._DOF_MASK:
                case enum_Focal._DOF_DISTANCE_MASK:
                {
                    _buffer.ReleaseTemporaryRT(kFocalMaskID);
                }
                    break;
            }
        }
    }

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

        private static readonly int kEncodeID = Shader.PropertyToID("_Encode");
        private static readonly int kDecodeID = Shader.PropertyToID("_Decode");
        #endregion

        public const int kMaxIteration = 12;
        private static readonly int[] kDualFilteringIDs = GetRenderTextureIDs("_Blur_DualFiltering",kMaxIteration);
        private static readonly int[] kNextGenUpIDs = GetRenderTextureIDs("_Blur_NextGen_UP",kMaxIteration);
        private static readonly int[] kNextGenDownIDs = GetRenderTextureIDs("_Blur_NextGen_DOWN",kMaxIteration);
        private static readonly RenderTargetIdentifier[] kDualFilteringRTs = ConvertToRTIdentifier(kDualFilteringIDs);
        
        static int[] GetRenderTextureIDs(string _id,int _size)
        {
            var dualFilteringIDArray = new int[_size];
            for(var i=0;i<_size;i++)
                dualFilteringIDArray[i] = Shader.PropertyToID(_id + i);
            return dualFilteringIDArray;
        }

        static RenderTargetIdentifier[] ConvertToRTIdentifier(int[] _ids)
        {
            var identifiers = new RenderTargetIdentifier[_ids.Length];
            for (var i = 0; i < _ids.Length; i++)
                identifiers[i] = new RenderTargetIdentifier(_ids[i]);
            return identifiers;
        }
        
        static readonly int kBlurTempID1 = Shader.PropertyToID("_PostProcessing_Blit_Blur_Temp1");
        static readonly RenderTargetIdentifier kBlurTempRT1 = new (kBlurTempID1);
        static readonly int kBlurTempID2 = Shader.PropertyToID("_PostProcessing_Blit_Blur_Temp2");
        static readonly RenderTargetIdentifier kBlurTempRT2 = new (kBlurTempID2);

        private static readonly int kBlinkingVerticalID = Shader.PropertyToID("_Blinking_Vertical");
        static readonly RenderTargetIdentifier kBlinkingVerticalRT = new (kBlinkingVerticalID);
        private static readonly int kBlinkingHorizontalID = Shader.PropertyToID("_Blinking_Horizontal");
        static readonly RenderTargetIdentifier kBlinkingHorizontalRT = new (kBlinkingHorizontalID);
        
        static readonly int kHexagonVerticalID = Shader.PropertyToID("_Hexagon_Vertical");
        static readonly RenderTargetIdentifier kVerticalRT = new (kHexagonVerticalID);
        static readonly int kHexagonDiagonalID = Shader.PropertyToID("_Hexagon_Diagonal");
        static readonly RenderTargetIdentifier kDiagonalRT = new (kHexagonDiagonalID);

        public override void Execute(RenderTextureDescriptor _descriptor, ref DBlurs _data, CommandBuffer _buffer,
            RenderTargetIdentifier _src, RenderTargetIdentifier _dst, ScriptableRenderContext _context, ref RenderingData _renderingData)
        {
            if(_descriptor.width <= 16 || _descriptor.height <= 16) 
                return;
            
            var sampleName = _data.blurType.ToString();
            _buffer.BeginSample(sampleName);
            var baseDownSample = math.max(_data.downSample, 1);
            var startWidth = _descriptor.width / baseDownSample;
            var startHeight = _descriptor.height / baseDownSample;
            
            switch (_data.blurType)
            {
                case EBlurType.Kawase:
                case EBlurType.AverageVHSeperated:
                case EBlurType.GaussianVHSeperated:
                    {
                        _buffer.GetTemporaryRT(kBlurTempID1, startWidth, startHeight, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
                        _buffer.GetTemporaryRT(kBlurTempID2, startWidth, startHeight, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
                        for (var i = 0; i < _data.iteration; i++)
                        {
                            var blitSrc = i == 0 ? _src : (i % 2 == 0 ? kBlurTempRT1 : kBlurTempRT2);
                            var blitTarget = i == _data.iteration - 1 ? _dst : (i % 2 == 0 ? kBlurTempRT2 : kBlurTempRT1);
                            m_Material.SetFloat(kIDBlurSize, _data.blurSize / _data.downSample * (1 + i));
                            switch (_data.blurType)
                            {
                                case EBlurType.Kawase:
                                    {
                                        var pass = (int)EBlurPass.Kawase;
                                        Blit(_buffer, m_Material, pass, blitSrc, blitTarget, _descriptor.colorFormat, i, _data.iteration);
                                    }
                                    break;
                                case EBlurType.AverageVHSeperated:
                                case EBlurType.GaussianVHSeperated:
                                    {
                                        var horizontalPass = -1;
                                        var verticalPass = -1;
                                        if (_data.blurType == EBlurType.AverageVHSeperated)
                                        {
                                            horizontalPass = (int)EBlurPass.Average_Horizontal;
                                            verticalPass = (int)EBlurPass.Average_Vertical;
                                        }
                                        else if (_data.blurType == EBlurType.GaussianVHSeperated)
                                        {
                                            horizontalPass = (int)EBlurPass.Gaussian_Horizontal;
                                            verticalPass = (int)EBlurPass.Gaussian_Vertical;
                                        }

                                        var maxIteration = _data.iteration * 2;
                                        var iteration = i * 2;

                                        var srcRT = kBlurTempRT2;
                                        var targetRT = kBlurTempRT1;
                                        if (iteration == 0)
                                            srcRT = blitSrc;
                                        Blit(_buffer, m_Material, horizontalPass, srcRT, targetRT, _descriptor.colorFormat, iteration, maxIteration);

                                        srcRT = kBlurTempRT2;
                                        if (iteration + 1 == maxIteration - 1)
                                            srcRT = blitTarget;
                                        Blit(_buffer, m_Material, verticalPass, targetRT, srcRT, _descriptor.colorFormat, iteration + 1, maxIteration);
                                    }
                                    break;
                            }
                        }
                        _buffer.ReleaseTemporaryRT(kBlurTempID1);
                        _buffer.ReleaseTemporaryRT(kBlurTempID2);
                    }
                    break;
                case EBlurType.Grainy:
                    {
                        var grainyPass = (int)EBlurPass.Grainy;
                        m_Material.SetFloat(kIDBlurSize, _data.blurSize * 32);
                        Blit(_buffer,m_Material, grainyPass,_src, _dst, RenderTextureFormat.ARGB32, 0, 1);
                    }
                    break;
                case EBlurType.Bokeh:
                    {
                        var bokehPass = (int)EBlurPass.Bokeh;
                        m_Material.SetFloat(kIDBlurSize, _data.blurSize);
                        m_Material.SetInt(kIDIteration, _data.iteration * 32);
                        m_Material.SetFloat(kIDAngle, _data.angle);
                        Blit(_buffer,m_Material, bokehPass,_src, _dst, RenderTextureFormat.ARGB32, 0, 1);
                    }
                    break;
                case EBlurType.Hexagon:
                    {
                        var verticalPass = (int)EBlurPass.Hexagon_Vertical;
                        var diagonalPass = (int)EBlurPass.Hexagon_Diagonal;
                        var rhomboidPass = (int)EBlurPass.Hexagon_Rhomboid;

                        m_Material.SetFloat(kIDBlurSize, _data.blurSize * 2);
                        m_Material.SetFloat(kIDIteration, _data.iteration * 2);
                        m_Material.SetFloat(kIDAngle, _data.angle);
                        _buffer.GetTemporaryRT(kHexagonVerticalID, startWidth, startHeight, 0, FilterMode.Bilinear, _descriptor.colorFormat);
                        _buffer.GetTemporaryRT(kHexagonDiagonalID, startWidth, startHeight, 0, FilterMode.Bilinear, _descriptor.colorFormat);

                        Blit(_buffer,m_Material, verticalPass,_src, kVerticalRT, RenderTextureFormat.ARGB32, 0, 3);
                        Blit(_buffer,m_Material, diagonalPass,_src, kDiagonalRT, RenderTextureFormat.ARGB32, 1, 3);
                        Blit(_buffer,m_Material, rhomboidPass,_src, _dst, RenderTextureFormat.ARGB32, 2, 3);

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
                            _buffer.SetGlobalVector(kIDVector, new Vector4(radialDirection.x, radialDirection.y, -radialDirection.x, -radialDirection.y) * _blurSize / _downSample * (1 + i));
                            Blit(_buffer, m_Material, radialPass, blitSrc, blitTarget, _descriptor.colorFormat, i, _iteration);
                        }
                    };

                    var angle = _data.angle;
                    var vector = math.mul(umath.Rotate2D(angle * kmath.kPI) , kfloat2.up);
                    DoRadialBlur(_data.iteration,vector,_data.blurSize,_data.downSample,_data.attenuation,kBlinkingVerticalRT);
                    DoRadialBlur(_data.iteration, KRotation.kRotateCW90.mul(vector),_data.blurSize,_data.downSample,_data.attenuation,kBlinkingHorizontalRT);
                    Blit(_buffer, m_Material, combinePass, _src, _dst, _descriptor.colorFormat, 0,1);
                    
                    _buffer.ReleaseTemporaryRT(kBlurTempID1);
                    _buffer.ReleaseTemporaryRT(kBlurTempID2);
                    _buffer.ReleaseTemporaryRT(kBlinkingVerticalID);
                    _buffer.ReleaseTemporaryRT(kBlinkingHorizontalID);
                }
                    break;
                case EBlurType.DualFiltering:
                {
                    var downSamplePass = (int)EBlurPass.DualFiltering_DownSample;
                    var upSamplePass = (int)EBlurPass.DualFiltering_UpSample;

                    var downSampleCount = Mathf.FloorToInt(_data.iteration / 2f);
                    m_Material.SetFloat(kIDBlurSize, _data.blurSize / _data.downSample*4f);

                    for (var i = 0; i < _data.iteration - 1; i++)
                    {
                        var filterSample = downSampleCount - Mathf.CeilToInt(Mathf.Abs(_data.iteration / 2f - (i + 1))) + 1 + _data.iteration % 2;
                        var filterWidth = startWidth / filterSample;
                        var filterHeight = startHeight / filterSample;
                        _buffer.GetTemporaryRT(kDualFilteringIDs[i], filterWidth, filterHeight, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
                    }
                    for (var i = 0; i < _data.iteration; i++)
                    {
                        var filterPass = i <= downSampleCount ? downSamplePass : upSamplePass;
                        var blitSrc = i == 0 ? _src : kDualFilteringRTs[i - 1];
                        var blitTarget = i == _data.iteration - 1 ? _dst : kDualFilteringRTs[i];
                        Blit(_buffer, m_Material, filterPass, blitSrc, blitTarget, _descriptor.colorFormat, i, _data.iteration);
                    }
                    for (var i = 0; i < _data.iteration - 1; i++)
                        _buffer.ReleaseTemporaryRT(kDualFilteringIDs[i]);
                }
                    break;
                case EBlurType.NextGen:
                    {
                        var iteration = Mathf.Clamp(_data.iteration,2,(int)Mathf.Log(Mathf.ClosestPowerOfTwo(Mathf.Min(startWidth,startHeight)) -1,2));
                        var downSamplePass = (int)EBlurPass.NextGen_DownSample;
                        var upSamplePass = (int)EBlurPass.NextGen_UpSample;
                        var upSampleFinalPass = (int) EBlurPass.NextGen_UpSampleFinal;

                        m_Material.SetFloat(kIDBlurSize, _data.blurSize / _data.downSample*4f);

                        for (var i = 0; i < iteration; i++)
                        {
                            var downSample = umath.pow( 2,i + 1 );
                            _buffer.GetTemporaryRT(kNextGenDownIDs[i], startWidth /  downSample, startHeight / downSample, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);

                            var upSample = umath.pow(2, iteration - i - 1);
                            if(i<iteration-1)
                                _buffer.GetTemporaryRT(kNextGenUpIDs[i], startWidth / upSample, startHeight / upSample, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
                        }
                        
                        var totalIteration = iteration * 2;
                        _buffer.BeginSample("Next Gen Down");
                        for (var i = 0 ; i < iteration ; i++)
                        {
                            var blitSrc = i == 0 ? _src : kNextGenDownIDs[i - 1];
                            var blitTarget = kNextGenDownIDs[i];
                            Blit(_buffer, m_Material, downSamplePass, blitSrc, blitTarget, _descriptor.colorFormat, i, totalIteration);
                        }
                        _buffer.EndSample("Next Gen Down");

                        _buffer.BeginSample("Next Gen Up");
                        for (var i = 0 ; i < iteration -1 ; i++)
                        {
                            var blitSrc = i == 0 ? kNextGenDownIDs[iteration-1] : kNextGenUpIDs[i - 1];
                            var blitTarget = kNextGenUpIDs[i];
                            _buffer.SetGlobalTexture("_PreDownSample",kNextGenDownIDs[iteration-2-i]);
                            Blit(_buffer, m_Material, upSamplePass, blitSrc, blitTarget,  _descriptor.colorFormat, i + iteration, totalIteration);
                        }
                        _buffer.EndSample("Next Gen Up");
                        
                        Blit(_buffer, m_Material, upSampleFinalPass, kNextGenUpIDs[iteration-2], _dst,  _descriptor.colorFormat, iteration - 1, iteration);
                        
                        for (var i = 0; i < iteration; i++)
                        {
                            _buffer.ReleaseTemporaryRT(kNextGenDownIDs[i]);
                            if(i<iteration-1)
                                _buffer.ReleaseTemporaryRT(kNextGenUpIDs[i]);
                        }
                    }
                    break;
            }
            _buffer.EndSample(sampleName);
        }

        void Blit(CommandBuffer _buffer,Material _material,int _passIndex, RenderTargetIdentifier _src, RenderTargetIdentifier _dst, RenderTextureFormat _format, int _iterationIndex, int _totalIteration)
        {
            var compress = _format != RenderTextureFormat.ARGB32;
            _buffer.SetGlobalInt(kDecodeID, compress && _iterationIndex != 0 ? 1 : 0);
            _buffer.SetGlobalInt(kEncodeID,compress && _iterationIndex != _totalIteration - 1 ? 1 : 0);
            // _buffer.Blit(_src, _dst,_material,_passIndex);
            _buffer.BlitFullScreenMesh(_src, _dst, _material, _passIndex);
        }
    }
}