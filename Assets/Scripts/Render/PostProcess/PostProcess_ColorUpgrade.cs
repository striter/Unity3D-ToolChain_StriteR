using UnityEngine;
using UnityEngine.Rendering;

namespace Rendering.PostProcess
{
    public class PostProcess_ColorUpgrade : PostProcessComponentBase<PPCore_ColorUpgrade, PPData_ColorUpgrade>
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

    [System.Serializable]
    public struct PPData_ColorUpgrade
    {
        [MTitle] public EFXAA m_FXAA;
        [MFold(nameof(m_FXAA),EFXAA.None)] public bool m_AdditionalSample;
        [MFold(nameof(m_FXAA),EFXAA.None)] public bool m_UseDepth;
        [MFold(nameof(m_FXAA),EFXAA.None)] [Range(.01f,1f)] public float m_ContrastSkip;
        [MFold(nameof(m_FXAA),EFXAA.None)] [Range(.01f,1f)] public float m_RelativeSkip;
        [MFoldout(nameof(m_FXAA),EFXAA.SubPixel)] [Range(.1f,2f)] public float m_SubPixelBlend;
        
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

        [MTitle]public bool m_Bloom;
        [MFoldout(nameof(m_Bloom),true)] [Range(0.0f, 2f)] public float m_Threshold;
        [MFoldout(nameof(m_Bloom),true)] [Range(0.0f, 5)] public float m_Intensity;
        [MFoldout(nameof(m_Bloom),true)] public PPData_Blurs m_Blur;
        [MFoldout(nameof(m_Bloom),true)] public bool m_BloomDebug;
        
        public static readonly PPData_ColorUpgrade m_Default = new PPData_ColorUpgrade()
        {
            m_FXAA =  EFXAA.None,
            m_ContrastSkip = .1f,
            m_RelativeSkip = .2f,
            m_SubPixelBlend = 1f,
            m_AdditionalSample=true,
            
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
            
            m_Bloom = false,
            m_Threshold = 0.25f,
            m_Intensity = 0.3f,
            m_Blur = PPData_Blurs.m_Default,
            m_BloomDebug = false,
        };
    }

    public class PPCore_ColorUpgrade : PostProcessCore<PPData_ColorUpgrade>
    {
        #region ShaderProperties
        const string KW_FXAA = "_FXAA";
        const string KW_FXAA_AdditionalSample="_FXAA_ADDITIONAL_SAMPLE";
        const string KW_FXAADepth = "_FXAA_DEPTH";
        const string KW_FXAASubPixelBlend="_FXAA_SUBPIXEL";
        //const string KW_FXAAEdgeBlend="_FXAA_EDGE";
        readonly int ID_ContrastSkip=Shader.PropertyToID("_FXAAContrastSkip");
        readonly int ID_RelativeSkip=Shader.PropertyToID("_FXAARelativeSkip");
        readonly int ID_BlendStrength=Shader.PropertyToID("_FXAABlendStrength");
        
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
        
        const string KW_BLOOM = "_BLOOM";
        static readonly int ID_Threshold = Shader.PropertyToID("_Threshold");
        static readonly int ID_Intensity = Shader.PropertyToID("_Intensity");
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
        
        public override void OnValidate(ref PPData_ColorUpgrade _ssaoData)
        {
            base.OnValidate(ref _ssaoData);
            if (m_Material.EnableKeyword(KW_FXAA, _ssaoData.m_FXAA!= EFXAA.None))
            {
                m_Material.SetFloat(ID_ContrastSkip,_ssaoData.m_ContrastSkip);
                m_Material.SetFloat(ID_RelativeSkip,_ssaoData.m_RelativeSkip);
                m_Material.EnableKeyword(KW_FXAADepth, _ssaoData.m_UseDepth);
                m_Material.EnableKeyword(KW_FXAA_AdditionalSample,_ssaoData.m_AdditionalSample);

                bool subPixel = _ssaoData.m_FXAA == EFXAA.SubPixel;//||_data.m_FXAA == EFXAA.Both  ;
                //bool edge = _data.m_FXAA == EFXAA.Both || _data.m_FXAA == EFXAA.EdgeDetect;
                if (m_Material.EnableKeyword(KW_FXAASubPixelBlend,subPixel))
                    m_Material.SetFloat(ID_BlendStrength,_ssaoData.m_SubPixelBlend);
                //m_Material.EnableKeyword(KW_FXAAEdgeBlend, edge);
            }
            
            if (m_Material.EnableKeyword(KW_LUT, _ssaoData.m_LUT))
            {
                m_Material.SetTexture(ID_LUT, _ssaoData.m_LUTTex);
                m_Material.SetInt(ID_LUTCellCount, (int) _ssaoData.m_LUTCellCount);
                m_Material.SetFloat(ID_LUTWeight,_ssaoData.m_LUTWeight);
            }

            if ( m_Material.EnableKeyword(KW_BSC, _ssaoData.m_BSC))
            {
                m_Material.SetFloat(ID_Brightness, _ssaoData.m_Brightness);
                m_Material.SetFloat(ID_Saturation, _ssaoData.m_Saturation);
                m_Material.SetFloat(ID_Contrast, _ssaoData.m_Contrast);
            }

            if ( m_Material.EnableKeyword(KW_MixChannel, _ssaoData.m_ChannelMix))
            {
                m_Material.SetVector(ID_MixRed, _ssaoData.m_MixRed + Vector3.right);
                m_Material.SetVector(ID_MixGreen, _ssaoData.m_MixGreen + Vector3.up);
                m_Material.SetVector(ID_MixBlue, _ssaoData.m_MixBlue + Vector3.forward);
            }
            
            if(m_Material.EnableKeyword(KW_BLOOM,_ssaoData.m_Bloom))
            {
                m_Material.SetFloat(ID_Threshold, _ssaoData.m_Threshold);
                m_Material.SetFloat(ID_Intensity, _ssaoData.m_Intensity);
                m_CoreBlurs.OnValidate(ref _ssaoData.m_Blur);
            }
        }

        public override void ExecutePostProcessBuffer(CommandBuffer _buffer, RenderTargetIdentifier _src, RenderTargetIdentifier _dst,
            RenderTextureDescriptor _descriptor, ref PPData_ColorUpgrade _data)
        {
            if (!_data.m_Bloom)
            {
                _buffer.Blit(_src, _dst, m_Material, (int)EPassIndex.Process);
                return;
            }

            var rtW = _descriptor.width;
            var rtH = _descriptor.height;

            _buffer.GetTemporaryRT(RT_ID_Blur, rtW, rtH, 0, FilterMode.Bilinear, _descriptor.colorFormat);
            _buffer.GetTemporaryRT(RT_ID_Sample, rtW, rtH, 0, FilterMode.Bilinear, _descriptor.colorFormat);

            _buffer.Blit(_src, RT_Sample, m_Material, (int)EPassIndex.BloomSample);

            m_CoreBlurs.ExecutePostProcessBuffer(_buffer, RT_Sample, RT_Blur, _descriptor, ref _data.m_Blur);

            if(_data.m_BloomDebug)
                _buffer.Blit(RT_Sample,_dst);
            else
                _buffer.Blit(_src, _dst, m_Material, (int)EPassIndex.Process);

            _buffer.ReleaseTemporaryRT(RT_ID_Sample);
            _buffer.ReleaseTemporaryRT(RT_ID_Blur);
        }
    }
}
