using System;
using Rendering.Pipeline.Mask;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.PostProcess
{
    public class PostProcess_ColorUpgrade : PostProcessBehaviour<FColorUpgradeCore, DColorUpgrade>
    {
        public override bool m_OpaqueProcess => false;
        public override EPostProcess Event => EPostProcess.ColorUpgrade;

        [InspectorButton]
        void SepiaToneFilter()
        {
            m_Data = DColorUpgrade.kDefault;
            m_Data.m_MixRed = new Vector3(0.393f, 0.349f, 0.272f)-Vector3.right;
            m_Data.m_MixGreen = new Vector3(0.769f, 0.686f, 0.534f)-Vector3.up;
            m_Data.m_MixBlue = new Vector3(0.189f, 0.168f, 0.131f)-Vector3.forward;
            ValidateParameters();
        }
    }

    public enum EBloomSample
    {
        Luminance,
        Mask,
    }

    [Serializable]
    public struct DColorUpgrade:IPostProcessParameter
    {
        [Title] public Texture2D m_LUTTex ;
        [MFold(nameof(m_LUTTex),null)] public bool m_64LUT;
        [MFold(nameof(m_LUTTex),null)] [Range(0,1)] public float m_LUTWeight;
        [Title] public bool m_BSC;
        [Foldout(nameof(m_BSC),true)] [Range(0, 2)]public float m_Brightness ;
        [Foldout(nameof(m_BSC),true)] [Range(0, 2)] public float m_Saturation ;
        [Foldout(nameof(m_BSC),true)] [Range(0, 2)]public float m_Contrast ;

        [Title]public bool m_ChannelMix;
        [Foldout(nameof(m_ChannelMix),true)] [RangeVector(-1, 1)] public Vector3 m_MixRed;
        [Foldout(nameof(m_ChannelMix),true)] [RangeVector(-1, 1)] public Vector3 m_MixGreen;
        [Foldout(nameof(m_ChannelMix),true)] [RangeVector(-1, 1)] public Vector3 m_MixBlue;

        public bool m_UseMaskTexture;
        
        [Title]public bool m_Bloom;
        [Foldout(nameof(m_Bloom),true)] public Data_Bloom m_BloomData;

        [Title] public bool motionBlur;
        [Foldout(nameof(motionBlur), true)] [Clamp(1,8)]public int iteration;
        [Foldout(nameof(motionBlur), true)] [Range(-5,5)] public float intensity;
        public bool Validate() => motionBlur ||
                                  m_LUTTex!=null  || 
                                  m_BSC  || 
                                  m_ChannelMix  || 
                                  m_Bloom;
        
        public static readonly DColorUpgrade kDefault = new DColorUpgrade()
        {
            m_LUTTex = null,
            m_64LUT = false,
            m_LUTWeight = 1f,
            
            m_BSC = false,
            m_Brightness = 1,
            m_Saturation = 1,
            m_Contrast = 1,
            
            m_ChannelMix = false,
            m_MixRed = Vector3.zero,
            m_MixGreen = Vector3.zero,
            m_MixBlue = Vector3.zero,
            
            m_UseMaskTexture = false,
            
            m_Bloom = false,
            m_BloomData = new Data_Bloom()
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
            static readonly int ID_Threshold = Shader.PropertyToID("_BloomThreshold");
            static readonly int ID_Color = Shader.PropertyToID("_BloomColor");

            public void Apply(Material _material, FBlursCore _blur)
            {
                _material.SetFloat(ID_Threshold, m_Threshold);
                _material.SetColor(ID_Color, m_Color);
                _blur.OnValidate(ref m_Blur);
            }
            #endregion
        }
    }

    public class FColorUpgradeCore : PostProcessCore<DColorUpgrade>
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
        
        static readonly string kUseMaskTexture = "_MASK";
        #endregion
        
        enum EPassIndex
        {
            Process = 0,
            BloomSample = 1,
        }
        FBlursCore m_CoreBlurs = new();

        public override void Destroy()
        {
            base.Destroy();
            m_CoreBlurs.Destroy();
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
        
        public override void OnValidate(ref DColorUpgrade _data)
        {
            base.OnValidate(ref _data);

            if (m_Material.EnableKeyword(kLUT, _data.m_LUTTex!=null))
            {
                m_Material.SetTexture(kLUTTex, _data.m_LUTTex);
                m_Material.SetVector(kLUTCellCount, GetLUTParameters(_data.m_64LUT ? 64:32,_data.m_LUTTex.width, _data.m_LUTTex.height));
                m_Material.SetFloat(kLUTWeight,_data.m_LUTWeight);
            }

            if ( m_Material.EnableKeyword(kBSC, _data.m_BSC))
            {
                m_Material.SetFloat(kBrightness, _data.m_Brightness);
                m_Material.SetFloat(kSaturation, _data.m_Saturation);
                m_Material.SetFloat(kContrast, _data.m_Contrast);
            }

            if ( m_Material.EnableKeyword(kMixChannel, _data.m_ChannelMix))
            {
                m_Material.SetVector(kMixRed, _data.m_MixRed + Vector3.right);
                m_Material.SetVector(kMixGreen, _data.m_MixGreen + Vector3.up);
                m_Material.SetVector(kMixBlue, _data.m_MixBlue + Vector3.forward);
            }

            m_Material.EnableKeyword(kUseMaskTexture, _data.m_UseMaskTexture);

            if (m_Material.EnableKeyword(kBloom, _data.m_Bloom))
                _data.m_BloomData.Apply(m_Material, m_CoreBlurs);

            if (m_Material.EnableKeyword(kMotionBlur, _data.motionBlur))
            {
                m_Material.SetInt(kMotionBlurIteration,_data.iteration);
                m_Material.SetFloat(kMotionBlurIntensity,_data.intensity);
            }
        }


        public override void Execute(RenderTextureDescriptor _descriptor, ref DColorUpgrade _data, CommandBuffer _buffer,
            RenderTargetIdentifier _src, RenderTargetIdentifier _dst, ScriptableRenderer _renderer,
            ScriptableRenderContext _context, ref RenderingData _renderingData)
        {
            if (!_data.m_Bloom || !_data.m_BloomData.m_Blur.Validate())
            {
                _buffer.Blit(_src, _dst, m_Material, (int)EPassIndex.Process);
                return;
            }

            ref var bloomData = ref _data.m_BloomData;
            
            if (bloomData.m_SampleMode == EBloomSample.Mask)
                MaskTexturePass.DrawMask(RT_Sample, _context, ref _renderingData, bloomData.m_MaskData);
            else if(bloomData.m_SampleMode == EBloomSample.Luminance)
                _buffer.Blit(_src, RT_Sample, m_Material, (int)EPassIndex.BloomSample);

            m_CoreBlurs.Execute(_descriptor, ref bloomData.m_Blur,_buffer, RT_Sample, RT_Blur, _renderer,_context,ref _renderingData);

            if(bloomData.m_BloomDebug)
                _buffer.Blit(RT_Blur,_dst);
            else
                _buffer.Blit(_src, _dst, m_Material, (int)EPassIndex.Process);
        }

        public override void Configure(CommandBuffer _buffer, RenderTextureDescriptor _descriptor, ref DColorUpgrade _data)
        {
            if (!_data.m_Bloom)
                return;
            ref var data = ref _data.m_BloomData;
            _descriptor.mipCount = 0;
            _descriptor.depthBufferBits = 0;

            _buffer.GetTemporaryRT(kSampleID, _descriptor, FilterMode.Bilinear);
            _buffer.GetTemporaryRT(kBlurID, _descriptor ,FilterMode.Bilinear);
        }


        public override void FrameCleanUp(CommandBuffer _buffer, ref DColorUpgrade _data)
        {
            if (!_data.m_Bloom)
                return;
            _buffer.ReleaseTemporaryRT(kSampleID);
            _buffer.ReleaseTemporaryRT(kBlurID);
        }
    }
}
