using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.PostProcess
{
    public class PostProcess_Atmosphere : APostProcessBehaviour<FAtmosphereCore, DAtmosphere>
    {
        public override bool OpaqueProcess => false;
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

        [Title]public bool volumetricLight;
        [Foldout(nameof(volumetricLight),true)] public DVolumetricLight volumetricLightData;
        [Header("Optimize")]
        [Range(1, 4)] public int volumetricDownSample;
        [Title] public bool enableVolumetricBlur;
        [Foldout(nameof(enableVolumetricBlur), true)] public DBlurs volumetricBlur;
        [Title] public bool multiScattering;
        [Foldout(nameof(multiScattering), true)] public DMultiScattering multiScatteringData;
        public bool Validate() => (multiScattering || volumetricLight) && volumetricDownSample > 0;
        public static readonly DAtmosphere kDefault = new DAtmosphere()
        {
            volumetricLight = true,
            volumetricLightData = new DVolumetricLight()
            {
                strength = 1f,
                pow = 2,
                marchStrength = .3f,
                distance = 20f,
                marchTimes = EMarchTimes._64,
                dither=false,
            },
            volumetricDownSample = 1,
            enableVolumetricBlur = false,
            volumetricBlur = DBlurs.kDefault,
            multiScattering = false,
            multiScatteringData = DMultiScattering.kDefault,
        };

        [Serializable]
        public struct DVolumetricLight
        {
            [Range(.1f,5f)]public float strength;
            [Clamp(0f)] public float distance;
            public EMarchTimes marchTimes;
            [Range(0, 2f)] public float marchStrength;
            [Range(0.1f, 2f)] public float pow;
            public bool dither;
            #region Properties
            static readonly int ID_ColorStrength = Shader.PropertyToID("_ColorStrength");
            static readonly int ID_LightPow = Shader.PropertyToID("_LightPow");
            static readonly int ID_LightStrength = Shader.PropertyToID("_LightStrength");
            static readonly int ID_MarchTimes = Shader.PropertyToID("_MarchTimes");
            static readonly int ID_MarchDistance = Shader.PropertyToID("_MarchDistance");
            const string kW_DITHER = "_DITHER";

            public void Apply(Material _material)
            {
                _material.SetFloat(ID_ColorStrength,strength);
                _material.SetInt(ID_MarchTimes, (int)marchTimes);
                _material.SetFloat(ID_MarchDistance, distance);
                _material.SetFloat(ID_LightPow, pow);
                _material.SetFloat(ID_LightStrength, marchStrength);
                _material.EnableKeyword(kW_DITHER, dither);
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
        public override bool Validate(ref RenderingData _renderingData,ref DAtmosphere _data)
        {
            if(m_Material.EnableKeyword(kVolumetricLight, _data.volumetricLight))
                _data.volumetricLightData.Apply(m_Material);
            
            if(_data.multiScattering)
                _data.multiScatteringData.Apply(m_Material);
            return base.Validate(ref _renderingData,ref _data);
        }

        public override void Execute(RenderTextureDescriptor _descriptor, ref DAtmosphere _data, CommandBuffer _buffer,
            RenderTargetIdentifier _src, RenderTargetIdentifier _dst, ScriptableRenderContext _context, ref RenderingData _renderingData)
        {
            if (_data.multiScattering)
            {
                _buffer.GetTemporaryRT(kParticleDensityLUT,_descriptor.width,_descriptor.height,0,FilterMode.Point,RenderTextureFormat.RGHalf);
                _buffer.Blit(_src,kParticleDensityLUTRT,m_Material,2);
                _buffer.Blit(_src,_dst,m_Material,3);
                _buffer.ReleaseTemporaryRT(kParticleDensityLUT);
            }

            if (_data.volumetricLight)
            {
                var sampleDescriptor = _descriptor;
                sampleDescriptor.width /= _data.volumetricDownSample;
                sampleDescriptor.height /= _data.volumetricDownSample;
                sampleDescriptor.colorFormat = RenderTextureFormat.ARGB32;
                sampleDescriptor.depthBufferBits = 0;
            
                _buffer.GetTemporaryRT(kVolumetricID, sampleDescriptor,FilterMode.Bilinear);
            
                if (!_data.enableVolumetricBlur)
                {
                    _buffer.Blit(_src, kVolumetricID, m_Material, (int)EPassIndex.Sample);
                }
                else
                {
                    _buffer.GetTemporaryRT(kBlurID, sampleDescriptor, FilterMode.Bilinear);
                    _buffer.Blit(_src, kBlurID, m_Material,  (int)EPassIndex.Sample);
                    FBlursCore.Instance.Execute(sampleDescriptor,ref _data.volumetricBlur,_buffer, kBlurID, kVolumetricID,_context,ref _renderingData ); 
                    _buffer.ReleaseTemporaryRT(kBlurID);
                }
            
                _buffer.Blit(_src, _dst, m_Material,  (int)EPassIndex.Combine);
                _buffer.ReleaseTemporaryRT(kVolumetricID);
            }
        }
    }
}