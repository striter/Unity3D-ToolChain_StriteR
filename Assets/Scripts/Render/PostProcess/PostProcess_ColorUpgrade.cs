using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.PostProcess
{
    public class PostProcess_ColorUpgrade : PostProcessBehaviour<PPCore_ColorUpgrade, PPData_ColorUpgrade>
    {
        public override bool m_OpaqueProcess => false;
        public override EPostProcess Event => EPostProcess.ColorUpgrade;
    }

    public enum EFXAA
    {
        None=0,
        SubPixel,
        // EdgeDetect,
        // Both,
    }
    public enum EMixChannel
    {
        None = 0,
        Red = 1,
        Green = 2,
        Blue = 3,
    }
    public enum ELUTCellCount
    {
        _16 = 16,
        _32 = 32,
        _64 = 64,
        _128 = 128,
    }

    public enum EBloomSample
    {
        Luminance,
        Redraw,
    }
    [System.Serializable]
    public struct PPData_ColorUpgrade:IPostProcessParameter
    {
        // [MTitle] public EFXAA m_FXAA;
        // [MFold(nameof(m_FXAA),EFXAA.None)] public bool m_AdditionalSample;
        // [MFold(nameof(m_FXAA),EFXAA.None)] public bool m_UseDepth;
        // [MFold(nameof(m_FXAA),EFXAA.None)] [Range(.01f,1f)] public float m_ContrastSkip;
        // [MFold(nameof(m_FXAA),EFXAA.None)] [Range(.01f,1f)] public float m_RelativeSkip;
        // [MFoldout(nameof(m_FXAA),EFXAA.SubPixel)] [Range(.1f,2f)] public float m_SubPixelBlend;
        
        [MTitle]public bool m_LUT;
        [MFoldout(nameof(m_LUT),true)] public Texture2D m_LUTTex ;
        [MFoldout(nameof(m_LUT),true)] public ELUTCellCount m_LUTCellCount ;
        [MFoldout(nameof(m_LUT),true)] [Range(0,1)] public float m_LUTWeight;
        [MTitle]public bool m_BSC;
        [MFoldout(nameof(m_BSC),true)] [Range(0, 2)]public float m_Brightness ;
        [MFoldout(nameof(m_BSC),true)] [Range(0, 2)] public float m_Saturation ;
        [MFoldout(nameof(m_BSC),true)] [Range(0, 2)]public float m_Contrast ;

        [MTitle]public bool m_ChannelMix;
        [MFoldout(nameof(m_ChannelMix),true)] [RangeVector(-1, 1)] public Vector3 m_MixRed;
        [MFoldout(nameof(m_ChannelMix),true)] [RangeVector(-1, 1)] public Vector3 m_MixGreen;
        [MFoldout(nameof(m_ChannelMix),true)] [RangeVector(-1, 1)] public Vector3 m_MixBlue;

        public bool m_UseMaskTexture;
        
        [MTitle]public bool m_Bloom;
        [MFoldout(nameof(m_Bloom),true)] public Data_Bloom m_BloomData;
        public bool Validate() => //m_FXAA != EFXAA.None  || 
                                  m_LUT  || 
                                  m_BSC  || 
                                  m_ChannelMix  || 
                                  m_Bloom;
        
        public static readonly PPData_ColorUpgrade kDefault = new PPData_ColorUpgrade()
        {
            // m_FXAA =  EFXAA.None,
            // m_ContrastSkip = .1f,
            // m_RelativeSkip = .2f,
            // m_SubPixelBlend = 1f,
            // m_AdditionalSample=true,
            
            m_LUT = false,
            m_LUTCellCount = ELUTCellCount._16,
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
                m_LayerMask = int.MaxValue,
                m_Threshold = 0.25f,
                m_Color = Color.white,
                m_Blur = PPData_Blurs.kDefault,
                m_BloomDebug = false,
            },
        };

        [Serializable]
        public struct Data_Bloom
        {
            public EBloomSample m_SampleMode;
            [MFoldout(nameof(m_SampleMode),EBloomSample.Luminance)] [Range(0.0f, 3f)] public float m_Threshold;
            [MFoldout(nameof(m_SampleMode),EBloomSample.Redraw)][CullingMask] public int m_LayerMask;
            [ColorUsage(true,true)] public Color m_Color;
            public PPData_Blurs m_Blur;
            public bool m_BloomDebug;

            #region Properties
            static readonly int ID_Threshold = Shader.PropertyToID("_BloomThreshold");
            static readonly int ID_Color = Shader.PropertyToID("_BloomColor");

            public void Apply(Material _material, PPCore_Blurs _blur)
            {
                _material.SetFloat(ID_Threshold, m_Threshold);
                _material.SetColor(ID_Color, m_Color);
                _blur.OnValidate(ref m_Blur);
            }
            #endregion
        }
    }

    public class PPCore_ColorUpgrade : PostProcessCore<PPData_ColorUpgrade>,IPostProcessPipelineCallback<PPData_ColorUpgrade>
    {
        #region ShaderProperties
        // const string kFXAA = "_FXAA";
        // const string kFXAA_AdditionalSample="_FXAA_ADDITIONAL_SAMPLE";
        // const string kFXAA_Depth = "_FXAA_DEPTH";
        // const string kFXAA_SubPixel="_FXAA_SUBPIXEL";
        // const string KW_FXAAEdgeBlend="_FXAA_EDGE";
        // readonly int kConstrastSkip=Shader.PropertyToID("_FXAAContrastSkip");
        // readonly int kRelativeSkip=Shader.PropertyToID("_FXAARelativeSkip");
        // readonly int kBlendStrength=Shader.PropertyToID("_FXAABlendStrength");
        
        const string KW_LUT = "_LUT";
        static readonly int ID_LUT = Shader.PropertyToID("_LUTTex");
        readonly int ID_LUTCellCount = Shader.PropertyToID("_LUTCellCount");
        readonly int ID_LUTWeight = Shader.PropertyToID("_LUTWeight");

        const string KW_BSC = "_BSC";
        static readonly int ID_Brightness = Shader.PropertyToID("_Brightness");
        static readonly int ID_Saturation = Shader.PropertyToID("_Saturation");
        static readonly int ID_Contrast = Shader.PropertyToID("_Contrast");

        const string KW_MixChannel = "_CHANNEL_MIXER";
        static readonly int ID_MixRed = Shader.PropertyToID("_MixRed");
        static readonly int ID_MixGreen = Shader.PropertyToID("_MixGreen");
        static readonly int ID_MixBlue = Shader.PropertyToID("_MixBlue");
        
        static readonly int RT_ID_Sample = Shader.PropertyToID("_Bloom_Sample");
        static readonly int RT_ID_Blur = Shader.PropertyToID("_Bloom_Blur");

        static readonly string kUseMaskTexture = "_MASK";
        
        const string KW_BLOOM = "_BLOOM";
        static readonly RenderTargetIdentifier RT_Sample = new RenderTargetIdentifier(RT_ID_Sample);
        static readonly RenderTargetIdentifier RT_Blur = new RenderTargetIdentifier(RT_ID_Blur);
        #endregion
        
        enum EPassIndex
        {
            Process = 0,
            BloomSample = 1,
        }
        PPCore_Blurs m_CoreBlurs;
        public PPCore_ColorUpgrade()
        {
            m_CoreBlurs = new PPCore_Blurs();
        }
        
        public override void Destroy()
        {
            base.Destroy();
            m_CoreBlurs.Destroy();
        }
        
        public override void OnValidate(ref PPData_ColorUpgrade _data)
        {
            base.OnValidate(ref _data);
            // if (m_Material.EnableKeyword(kFXAA, _data.m_FXAA!= EFXAA.None))
            // {
            //     m_Material.SetFloat(kConstrastSkip,_data.m_ContrastSkip);
            //     m_Material.SetFloat(kRelativeSkip,_data.m_RelativeSkip);
            //     m_Material.EnableKeyword(kFXAA_Depth, _data.m_UseDepth);
            //     m_Material.EnableKeyword(kFXAA_AdditionalSample,_data.m_AdditionalSample);
            //
            //     bool subPixel = _data.m_FXAA == EFXAA.SubPixel;//||_data.m_FXAA == EFXAA.Both  ;
            //     if (m_Material.EnableKeyword(kFXAA_SubPixel,subPixel))
            //         m_Material.SetFloat(kBlendStrength,_data.m_SubPixelBlend);
            //     //bool edge = _data.m_FXAA == EFXAA.Both || _data.m_FXAA == EFXAA.EdgeDetect;
            //     //m_Material.EnableKeyword(KW_FXAAEdgeBlend, edge);
            // }
            //
            if (m_Material.EnableKeyword(KW_LUT, _data.m_LUT))
            {
                m_Material.SetTexture(ID_LUT, _data.m_LUTTex);
                m_Material.SetInt(ID_LUTCellCount, (int) _data.m_LUTCellCount);
                m_Material.SetFloat(ID_LUTWeight,_data.m_LUTWeight);
            }

            if ( m_Material.EnableKeyword(KW_BSC, _data.m_BSC))
            {
                m_Material.SetFloat(ID_Brightness, _data.m_Brightness);
                m_Material.SetFloat(ID_Saturation, _data.m_Saturation);
                m_Material.SetFloat(ID_Contrast, _data.m_Contrast);
            }

            if ( m_Material.EnableKeyword(KW_MixChannel, _data.m_ChannelMix))
            {
                m_Material.SetVector(ID_MixRed, _data.m_MixRed + Vector3.right);
                m_Material.SetVector(ID_MixGreen, _data.m_MixGreen + Vector3.up);
                m_Material.SetVector(ID_MixBlue, _data.m_MixBlue + Vector3.forward);
            }

            m_Material.EnableKeyword(kUseMaskTexture, _data.m_UseMaskTexture);

            if (m_Material.EnableKeyword(KW_BLOOM, _data.m_Bloom))
                _data.m_BloomData.Apply(m_Material, m_CoreBlurs);
        }

        
        public override void ExecutePostProcessBuffer(CommandBuffer _buffer, RenderTargetIdentifier _src, RenderTargetIdentifier _dst,
            RenderTextureDescriptor _descriptor, ref PPData_ColorUpgrade _data)
        {
            if (!_data.m_Bloom)
            {
                _buffer.Blit(_src, _dst, m_Material, (int)EPassIndex.Process);
                return;
            }

            ref var bloomData = ref _data.m_BloomData;
            if (bloomData.m_SampleMode == EBloomSample.Luminance)
                _buffer.Blit(_src, RT_Sample, m_Material, (int)EPassIndex.BloomSample);

            m_CoreBlurs.ExecutePostProcessBuffer(_buffer, RT_Sample, RT_Blur, _descriptor, ref bloomData.m_Blur);

            if(bloomData.m_BloomDebug)
                _buffer.Blit(RT_Blur,_dst);
            else
                _buffer.Blit(_src, _dst, m_Material, (int)EPassIndex.Process);
        }

        public void Configure(CommandBuffer _buffer, RenderTextureDescriptor _descriptor, ref PPData_ColorUpgrade _data)
        {
            if (!_data.m_Bloom)
                return;
            ref var data = ref _data.m_BloomData;
            _descriptor.mipCount = 0;

            _buffer.GetTemporaryRT(RT_ID_Sample, _descriptor, FilterMode.Bilinear);
            _buffer.GetTemporaryRT(RT_ID_Blur, _descriptor ,FilterMode.Bilinear);
        }

        public void ExecuteContext(ScriptableRenderer _renderer, ScriptableRenderContext _context, ref RenderingData _renderingData,
            ref PPData_ColorUpgrade _data)
        {
            if (!_data.m_Bloom)
                return;
            
            ref var bloomData = ref _data.m_BloomData;
            if (bloomData.m_SampleMode != EBloomSample.Redraw)
                return;
            
            CommandBuffer buffer = CommandBufferPool.Get("Bloom Redraw Execute");
            buffer.SetRenderTarget(RT_ID_Sample);
            buffer.ClearRenderTarget(true, true, Color.black);
            _context.ExecuteCommandBuffer(buffer);

            DrawingSettings drawingSettings = UPipeline.CreateDrawingSettings(true, _renderingData.cameraData.camera);
            drawingSettings.perObjectData = (PerObjectData)int.MaxValue;
            FilteringSettings filterSettings = new FilteringSettings(RenderQueueRange.all) { layerMask = bloomData.m_LayerMask };
            _context.DrawRenderers(_renderingData.cullResults, ref drawingSettings, ref filterSettings);

            buffer.Clear();
            buffer.SetRenderTarget(_renderer.cameraColorTarget);
            _context.ExecuteCommandBuffer(buffer);
            CommandBufferPool.Release(buffer);
        }

        public void FrameCleanUp(CommandBuffer _buffer, ref PPData_ColorUpgrade _data)
        {
            if (!_data.m_Bloom)
                return;
            _buffer.ReleaseTemporaryRT(RT_ID_Sample);
            _buffer.ReleaseTemporaryRT(RT_ID_Blur);
        }
    }
}
