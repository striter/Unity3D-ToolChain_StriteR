using System;
using Rendering.Pipeline.Mask;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.PostProcess
{
    public class PostProcess_ColorGrading : APostProcessBehaviour<FColorGradingCore, DColorGrading>
    {
        public override bool OpaqueProcess => false;
        public override EPostProcess Event => EPostProcess.ColorGrading;

        [InspectorButton]
        void SepiaToneFilter()
        {
            var data = GetData();
            data.mixRed = new Vector3(0.393f, 0.349f, 0.272f) - Vector3.right;
            data.mixGreen = new Vector3(0.769f, 0.686f, 0.534f) - Vector3.up;
            data.mixBlue = new Vector3(0.189f, 0.168f, 0.131f) - Vector3.forward;
            SetEffectData(data);
        }
    }

    public enum EToneMapping
    {
        NONE,
        _TONEMAP_ACES
    }
    
    public enum EBloomSample
    {
        Luminance,
        Mask,
    }

    [Serializable]
    public struct DColorGrading:IPostProcessParameter
    {
        [Title] public Texture2D LUTTex ;
        [Fold(nameof(LUTTex),null)] public bool LUT64;
        [Fold(nameof(LUTTex),null)] [Range(0,1)] public float LUTWeight;
        [Title] public bool m_BSC;
        [Foldout(nameof(m_BSC),true),Range(0, 2)] public float brightness ;
        [Foldout(nameof(m_BSC),true),Range(0, 2)] public float saturation ;
        [Foldout(nameof(m_BSC),true),Range(0, 2)] public float contrast ;

        [Title]public bool channelMix;
        [Foldout(nameof(channelMix),true),RangeVector(-1, 1)] public Vector3 mixRed;
        [Foldout(nameof(channelMix),true),RangeVector(-1, 1)] public Vector3 mixGreen;
        [Foldout(nameof(channelMix),true),RangeVector(-1, 1)] public Vector3 mixBlue;
        
        [Title] public EToneMapping toneMapping;

        [Title] public bool bloom;
        [Foldout(nameof(bloom),true)] public Data_Bloom bloomData;

        [Title] public bool motionBlur;
        [Foldout(nameof(motionBlur), true)] [Clamp(1,8)]public int iteration;
        [Foldout(nameof(motionBlur), true)] [Range(-5,5)] public float intensity;
        public bool Validate() => motionBlur || LUTTex != null || m_BSC || channelMix || (bloom && bloomData.Validate());
        
        public static readonly DColorGrading kDefault = new DColorGrading()
        {
            LUTTex = null,
            LUT64 = false,
            LUTWeight = 1f,
            
            m_BSC = false,
            brightness = 1,
            saturation = 1,
            contrast = 1,
            
            channelMix = false,
            mixRed = Vector3.zero,
            mixGreen = Vector3.zero,
            mixBlue = Vector3.zero,
            
            toneMapping = EToneMapping._TONEMAP_ACES,
            
            bloom = false,
            bloomData = new Data_Bloom()
            {
                m_SampleMode = EBloomSample.Luminance,
                m_Threshold = 0.25f,
                m_Color = Color.white,
                m_MaskData = MaskTextureData.kDefault,
                m_Blur = DBlurs.kDefault,
                m_BloomDebug = false,
            },
            
            
            motionBlur =  false,
            iteration = 2,
            intensity = 1,
        };

        [Serializable]
        public struct Data_Bloom
        {
            public EBloomSample m_SampleMode;
            [Foldout(nameof(m_SampleMode),EBloomSample.Luminance)] [Range(0.0f, 3f)] public float m_Threshold;
            [Foldout(nameof(m_SampleMode),EBloomSample.Mask)] public MaskTextureData m_MaskData;
            [ColorUsage(true,true)] public Color m_Color;
            public DBlurs m_Blur;
            public bool m_BloomDebug;

            #region Properties
            static readonly int kThreshold = Shader.PropertyToID("_BloomThreshold");
            static readonly int kColor = Shader.PropertyToID("_BloomColor");

            public void Apply(Material _material)
            {
                var threshold = UColor.GammaToLinear(m_Threshold);
                _material.SetVector(kThreshold, new Vector4(threshold,threshold * .5f,0,0));
                _material.SetColor(kColor, m_Color);
            }

            public bool Validate() => m_Blur.Validate();

            #endregion
        }
    }

    public class FColorGradingCore : PostProcessCore<DColorGrading>
    {
        #region ShaderProperties
        const string kLUT = "_LUT";
        static readonly int kLUTTex = Shader.PropertyToID("_LUTTex");
        readonly int kLUTCellCount = Shader.PropertyToID("_LUTCellCount");
        readonly int kLUTWeight = Shader.PropertyToID("_LUTWeight");

        const string kBSC = "_BSC";
        static readonly int kBrightness = Shader.PropertyToID("_Brightness");
        static readonly int kSaturation = Shader.PropertyToID("_Saturation");
        static readonly int kContrast = Shader.PropertyToID("_Contrast");

        const string kMixChannel = "_CHANNEL_MIXER";
        static readonly int kMixRed = Shader.PropertyToID("_MixRed");
        static readonly int kMixGreen = Shader.PropertyToID("_MixGreen");
        static readonly int kMixBlue = Shader.PropertyToID("_MixBlue");
        
        static readonly int kSampleID = Shader.PropertyToID("_Bloom_Sample");
        static readonly int kBlurID = Shader.PropertyToID("_Bloom_Blur");

        private const string kMotionBlur = "_MOTIONBLUR";
        private static readonly int kMotionBlurIntensity = Shader.PropertyToID("_Intensity");
        private static readonly int kMotionBlurIteration = Shader.PropertyToID("_Iteration");
        
        const string kBloom = "_BLOOM";
        static readonly RenderTargetIdentifier RT_Sample = new RenderTargetIdentifier(kSampleID);
        static readonly RenderTargetIdentifier RT_Blur = new RenderTargetIdentifier(kBlurID);
        #endregion

        private FBlursCore m_BloomBlur = new();
        enum EPassIndex
        {
            Process = 0,
            BloomSample = 1,
        }

        float4 GetLUTParameters(int _size,int width,int height)
        {
            var cellPixelSize = _size;
            if(cellPixelSize==0)
                return new float4(0,0,0,0);

            var horizontalCellCount = width / cellPixelSize;
            if (width == height)
                return new float4(horizontalCellCount,horizontalCellCount, cellPixelSize,horizontalCellCount * horizontalCellCount);
            return new float4(horizontalCellCount, 1, cellPixelSize, horizontalCellCount);
        }
        
        public override bool Validate(ref RenderingData _renderingData,ref DColorGrading _data)
        {
            if (m_Material.EnableKeyword(kLUT, _data.LUTTex!=null))
            {
                m_Material.SetTexture(kLUTTex, _data.LUTTex);
                m_Material.SetVector(kLUTCellCount, GetLUTParameters(_data.LUT64 ? 64:32,_data.LUTTex.width, _data.LUTTex.height));
                m_Material.SetFloat(kLUTWeight,_data.LUTWeight);
            }

            if ( m_Material.EnableKeyword(kBSC, _data.m_BSC))
            {
                m_Material.SetFloat(kBrightness, _data.brightness);
                m_Material.SetFloat(kSaturation, _data.saturation);
                m_Material.SetFloat(kContrast, _data.contrast);
            }

            if ( m_Material.EnableKeyword(kMixChannel, _data.channelMix))
            {
                m_Material.SetVector(kMixRed, _data.mixRed + Vector3.right);
                m_Material.SetVector(kMixGreen, _data.mixGreen + Vector3.up);
                m_Material.SetVector(kMixBlue, _data.mixBlue + Vector3.forward);
            }

            if (m_Material.EnableKeyword(kBloom, _data.bloom) && m_BloomBlur.Validate(ref _renderingData,ref _data.bloomData.m_Blur))
            {
                _data.bloomData.Apply(m_Material);
                m_BloomBlur.m_Material.EnableKeyword(kBloom, true);
            }

            if (m_Material.EnableKeyword(kMotionBlur, _data.motionBlur))
            {
                m_Material.SetInt(kMotionBlurIteration,_data.iteration);
                m_Material.SetFloat(kMotionBlurIntensity,_data.intensity);
            }

            m_Material.EnableKeywords(_data.toneMapping);
            return base.Validate(ref _renderingData,ref _data);
        }


        public override void Execute(RenderTextureDescriptor _descriptor, ref DColorGrading _data, CommandBuffer _buffer,
            RenderTargetIdentifier _src, RenderTargetIdentifier _dst, ScriptableRenderContext _context, ref RenderingData _renderingData)
        {
            if (!_data.bloom)
            {
                _buffer.Blit(_src, _dst, m_Material, (int)EPassIndex.Process);
                return;
            }

            ref var bloomData = ref _data.bloomData;
            
            if (bloomData.m_SampleMode == EBloomSample.Mask)
                MaskTexturePass.DrawMask(RT_Sample, _context, ref _renderingData, bloomData.m_MaskData);
            else if(bloomData.m_SampleMode == EBloomSample.Luminance)
                _buffer.Blit(_src, RT_Sample, m_Material, (int)EPassIndex.BloomSample);

            m_BloomBlur.Execute(_descriptor, ref bloomData.m_Blur,_buffer, RT_Sample, RT_Blur,_context,ref _renderingData);

            if(bloomData.m_BloomDebug)
                _buffer.Blit(RT_Blur,_dst);
            else
                _buffer.Blit(_src, _dst, m_Material, (int)EPassIndex.Process);
        }

        public override void Configure(CommandBuffer _buffer, RenderTextureDescriptor _descriptor, ref DColorGrading _data)
        {
            if (!_data.bloom)
                return;
            ref var data = ref _data.bloomData;
            _descriptor.mipCount = 0;
            _descriptor.depthBufferBits = 0;

            _buffer.GetTemporaryRT(kSampleID, _descriptor, FilterMode.Bilinear);
            _buffer.GetTemporaryRT(kBlurID, _descriptor ,FilterMode.Bilinear);
        }


        public override void FrameCleanUp(CommandBuffer _buffer, ref DColorGrading _data)
        {
            if (!_data.bloom)
                return;
            _buffer.ReleaseTemporaryRT(kSampleID);
            _buffer.ReleaseTemporaryRT(kBlurID);
        }
    }
}
