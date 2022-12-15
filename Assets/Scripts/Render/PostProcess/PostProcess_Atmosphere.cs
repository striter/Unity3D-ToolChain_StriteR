using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.PostProcess
{
    public class PostProcess_Atmosphere : PostProcessBehaviour<FAtmosphereCore, DAtmosphere>
    {
        public override bool m_OpaqueProcess => false;
        public override EPostProcess Event => EPostProcess.Volumetric;
    }
    
    [Serializable]
    public struct DAtmosphere:IPostProcessParameter
    {
        public enum EMarchTimes
        {
            _8=8,
            _16=16,
            _32=32,
            _64=64,
            _128=128,
        }

        [MTitle]public bool m_VolumetricLight;
        [MFoldout(nameof(m_VolumetricLight),true)] public DVolumetricLight m_VolumetricLightData;
        [Header("Optimize")]
        [Range(1, 4)] public int m_VolumetricDownSample;
        [MTitle] public bool m_EnableVolumetricBlur;
        [MFoldout(nameof(m_EnableVolumetricBlur), true)] public DBlurs m_VolumetricBlur;
        [MTitle] public bool m_MultiScattering;
        [MFoldout(nameof(m_MultiScattering), true)] public DMultiScattering m_MultiScatteringData;
        public bool Validate() => m_VolumetricLight;
        public static readonly DAtmosphere kDefault = new DAtmosphere()
        {
            m_VolumetricLight = true,
            m_VolumetricLightData = new DVolumetricLight()
            {
                m_Strength = 1f,
                m_Pow = 2,
                m_MarchStrength = .3f,
                m_Distance = 20f,
                m_MarchTimes = EMarchTimes._64,
                m_Dither=false,
            },
            m_VolumetricDownSample = 1,
            m_EnableVolumetricBlur = false,
            m_VolumetricBlur = DBlurs.kDefault,
            m_MultiScattering = false,
            m_MultiScatteringData = DMultiScattering.kDefault,
        };

        [Serializable]
        public struct DVolumetricLight
        {
            [Range(.1f,5f)]public float m_Strength;
            [Clamp(0f)] public float m_Distance;
            public EMarchTimes m_MarchTimes;
            [Range(0, 2f)] public float m_MarchStrength;
            [Range(0.1f, 2f)] public float m_Pow;
            public bool m_Dither;
            #region Properties
            static readonly int ID_ColorStrength = Shader.PropertyToID("_ColorStrength");
            static readonly int ID_LightPow = Shader.PropertyToID("_LightPow");
            static readonly int ID_LightStrength = Shader.PropertyToID("_LightStrength");
            static readonly int ID_MarchTimes = Shader.PropertyToID("_MarchTimes");
            static readonly int ID_MarchDistance = Shader.PropertyToID("_MarchDistance");
            const string kW_DITHER = "_DITHER";

            public void Apply(Material _material)
            {
                _material.SetFloat(ID_ColorStrength,m_Strength);
                _material.SetInt(ID_MarchTimes, (int)m_MarchTimes);
                _material.SetFloat(ID_MarchDistance, m_Distance);
                _material.SetFloat(ID_LightPow, m_Pow);
                _material.SetFloat(ID_LightStrength, m_MarchStrength);
                _material.EnableKeyword(kW_DITHER, m_Dither);
            }
            #endregion
        }

        [Serializable]
        public struct DMultiScattering
        {
            public float distanceScale;
            [Range(0,1)]public float sunItensity;
            [Header("Rayleigh")] 
            [Range(0,10)] public float rayleighScatterCofficients;
            [Range(0,10)] public float rayleighExtinctionCofficients;
            [Header("Mie")]
            [Range(0,10)] public float mieScatterCofficients;
            [Range(0,10)] public float mieExtinctionCofficients;
            [Range(0,0.999f)]public float mieG;
            
            public static readonly DMultiScattering kDefault = new DMultiScattering()
            {
                distanceScale = 1,
                sunItensity = .1f,
                rayleighScatterCofficients = 1,
                rayleighExtinctionCofficients = 1,
                mieScatterCofficients = 1,
                mieExtinctionCofficients = 1,
                mieG = 0.76f,
            };
            
            private static readonly int kDistanceScaleID = Shader.PropertyToID("_DistanceScale");
            private static readonly int kSunIntensity = Shader.PropertyToID("_SunIntensity");
            private static readonly int kScatterRayleighID = Shader.PropertyToID("_ScatterR");
            private static readonly int kScatterMieID = Shader.PropertyToID("_ScatterM");
            private static readonly int kExtinctionRayleighID = Shader.PropertyToID("_ExtinctionR");
            private static readonly int kExtinctionMieID = Shader.PropertyToID("_ExtinctionM");
            private static readonly int kMieG = Shader.PropertyToID("_MieG");

            private static readonly float4 kRayLeighScattering = new float4(5.8f, 13.5f, 33.1f, 0) * 0.00001f;
            private static readonly float4 kMieScattering = new float4(2, 2, 2, 0) * 0.00001f;
            
            public void Apply(Material _material)
            {
                _material.SetFloat(kDistanceScaleID,distanceScale);
                _material.SetFloat(kSunIntensity,sunItensity);
                
                _material.SetVector(kScatterRayleighID,kRayLeighScattering * rayleighScatterCofficients);
                _material.SetVector(kScatterMieID,kMieScattering * mieScatterCofficients);
                
                _material.SetVector(kExtinctionRayleighID,kRayLeighScattering*rayleighExtinctionCofficients);
                _material.SetVector(kExtinctionMieID,kMieScattering * mieExtinctionCofficients);
                _material.SetFloat(kMieG,mieG);
            }
        }
    }
    public class FAtmosphereCore : PostProcessCore<DAtmosphere>
    {
        private enum EPassIndex
        {
            Combine=0,
            Sample=1,
            
            MultiScatterLUT,
            MultiScatterIntegrate,
        }
        
    #region ShaderProperties
        private const string kVolumetricLight = "_VOLUMETRICLIGHT";
        static readonly int kVolumetricID = Shader.PropertyToID("_Volumetric_Sample");
        static RenderTargetIdentifier kVolumetricRT = new RenderTargetIdentifier(kVolumetricID);
        static readonly int kBlurID = Shader.PropertyToID("_Volumetric_Blur");
        static RenderTargetIdentifier kBlurRT = new RenderTargetIdentifier(kBlurID);

        private static int kParticleDensityLUT = Shader.PropertyToID("_AtmosphereDensityLUT");
        private RenderTargetIdentifier kParticleDensityLUTRT = new RenderTargetIdentifier(kParticleDensityLUT);
    #endregion
        
        readonly FBlursCore m_VolumetricBlur;
        public FAtmosphereCore()
        {
            m_VolumetricBlur = new FBlursCore();
        }
        
        public override void Destroy()
        {
            base.Destroy();
            m_VolumetricBlur.Destroy();
        }

        public override void OnValidate(ref DAtmosphere _data)
        {
            base.OnValidate(ref _data);
            if(m_Material.EnableKeyword(kVolumetricLight, _data.m_VolumetricLight))
                _data.m_VolumetricLightData.Apply(m_Material);
            m_VolumetricBlur.OnValidate(ref _data.m_VolumetricBlur);
            
            if(_data.m_MultiScattering)
                _data.m_MultiScatteringData.Apply(m_Material);
        }

        public override void Execute(RenderTextureDescriptor _descriptor, ref DAtmosphere _data, CommandBuffer _buffer,
            RenderTargetIdentifier _src, RenderTargetIdentifier _dst, ScriptableRenderer _renderer,
            ScriptableRenderContext _context, ref RenderingData _renderingData)
        {
            if (_data.m_MultiScattering)
            {
                _buffer.GetTemporaryRT(kParticleDensityLUT,_descriptor.width,_descriptor.height,0,FilterMode.Point,RenderTextureFormat.RGHalf);
                _buffer.Blit(_src,kParticleDensityLUTRT,m_Material,2);
                _buffer.Blit(_src,_dst,m_Material,3);
                _buffer.ReleaseTemporaryRT(kParticleDensityLUT);
                return;
            }
            
            if (!_data.m_VolumetricLight)
            {
                _buffer.Blit(_src,_dst);
                return;
            }
            var sampleDescriptor = _descriptor;
            sampleDescriptor.width /= _data.m_VolumetricDownSample;
            sampleDescriptor.height /= _data.m_VolumetricDownSample;
            sampleDescriptor.colorFormat = RenderTextureFormat.ARGB32;
            sampleDescriptor.depthBufferBits = 0;
        
            _buffer.GetTemporaryRT(kVolumetricID, sampleDescriptor,FilterMode.Bilinear);

            if (!_data.m_EnableVolumetricBlur)
            {
                _buffer.Blit(_src, kVolumetricID, m_Material, (int)EPassIndex.Sample);
            }
            else
            {
                _buffer.GetTemporaryRT(kBlurID, sampleDescriptor, FilterMode.Bilinear);
                _buffer.Blit(_src, kBlurID, m_Material,  (int)EPassIndex.Sample);
                m_VolumetricBlur.Execute(sampleDescriptor,ref _data.m_VolumetricBlur,_buffer, kBlurID, kVolumetricID,_renderer,_context,ref _renderingData ); 
                _buffer.ReleaseTemporaryRT(kBlurID);
            }
            
            _buffer.Blit(_src, _dst, m_Material,  (int)EPassIndex.Combine);
            _buffer.ReleaseTemporaryRT(kVolumetricID);
        }
    }
}