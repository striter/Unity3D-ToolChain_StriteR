using System;
using System.Linq;
using System.Runtime.Serialization;
using Rendering.IndirectDiffuse.SphericalHarmonics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rendering
{
    [Serializable]
    public struct LightmapParameter
    {
        public int index;
        public Vector4 scaleOffset;
    }

    [Serializable]
    public struct LightmapTextureExport
    {
        public Texture2D lightMapColor;
        // public Texture2D lightmapDir;
        // public Texture2D shadowMask;
        public LightmapTextureExport(LightmapData _data)
        {
            lightMapColor = _data.lightmapColor;
            // lightmapDir = _data.lightmapDir;
            // shadowMask = _data.shadowMask;
        }
    }
    
    
    //These stuff should be interpolated at the CPU size
    [Serializable]
    public struct EnvironmentParameters
    {
        [ColorUsage(true,true)]public Color gradientSky;
        [ColorUsage(true,true)]public Color gradientEquator;
        [ColorUsage(true,true)]public Color gradientGround;
        [Range(0.1f,2f)] public float reflectionIntensity;
        [HideInInspector] public SHL2Data shData;

        public EnvironmentParameters Ctor()
        {
            shData = SphericalHarmonicsExport.ExportL2Gradient(4096, gradientSky, gradientEquator,gradientGround,"Test");
            return this;
        }

        public static EnvironmentParameters Interpolate(EnvironmentParameters _a, EnvironmentParameters _b,
            float _interpolate)
        {
            return new EnvironmentParameters()
            {
                gradientSky = Color.Lerp(_a.gradientSky,_b.gradientSky,_interpolate),
                gradientEquator = Color.Lerp(_a.gradientEquator,_b.gradientEquator,_interpolate),
                gradientGround = Color.Lerp(_a.gradientGround,_b.gradientGround,_interpolate),
                reflectionIntensity = Mathf.Lerp(_a.reflectionIntensity,_b.reflectionIntensity,_interpolate),
                shData = SHL2Data.Interpolate(_a.shData,_b.shData,_interpolate),
            };
        }

        public static readonly EnvironmentParameters kDefault = new EnvironmentParameters()
        {
            gradientSky = Color.cyan,
            gradientGround = Color.black,
            gradientEquator = Color.white.SetAlpha(.5f),
            reflectionIntensity = 1f,
        }.Ctor();
    }
    
    [Serializable]
    public class EnvironmentCollection:ISerializationCallbackReceiver
    {

        public Cubemap m_EnvironmentReflection;
        public EnvironmentParameters m_Parameters=EnvironmentParameters.kDefault;
        
        // public LightmapParameter[] m_Parameters;
        // public Texture2D[] m_LightmapColors;

        public void OnBeforeSerialize(){}
        public void OnAfterDeserialize() => m_Parameters.Ctor();
        public static EnvironmentCollection ExportRenderSetting(Transform _rootTransform)
        {
            return new EnvironmentCollection()
            {
                // Debug.Assert(LightmapSettings.lightmapsMode== LightmapsMode.NonDirectional,"Only none-directional mode supported");
                // m_Parameters = _rootTransform.GetComponentsInChildren<MeshRenderer>().Select(p => new LightmapParameter()
                //     {index = p.lightmapIndex, scaleOffset = p.lightmapScaleOffset}).ToArray();
                // m_LightmapColors = LightmapSettings.lightmaps.Select(p => p.lightmapColor).ToArray();

                // m_EnvironmentReflection = RenderSettings.customReflection,
                m_Parameters = new EnvironmentParameters()
                {
                    reflectionIntensity = RenderSettings.reflectionIntensity,
                    gradientSky = RenderSettings.ambientSkyColor,
                    gradientEquator = RenderSettings.ambientEquatorColor,
                    gradientGround = RenderSettings.ambientGroundColor
                }.Ctor()
            };
        }

        private const int kMaxLightmapCount = 10;
        private static int[] GetLightmapIDs(string _keyword)
        {
            int[] ids=new int[kMaxLightmapCount];
            for (int i = 0; i < kMaxLightmapCount; i++)
                ids[i] = Shader.PropertyToID($"{_keyword}{i}");
            return ids;
        }

        private static readonly int kEnvironmentInterpolate = Shader.PropertyToID("_EnvironmentInterpolate");

        private static readonly string kLightmapKeyword = "LIGHTMAP_ON";
        private static readonly string[] kEnvironmentKeywords = {"ENVIRONMENT_CUSTOM", "ENVIRONMENT_INTERPOLATE"};
        private static readonly int kLightmapIndex = Shader.PropertyToID("_LightmapIndex");        private static readonly int kLightmapInterpolateIndex = Shader.PropertyToID("_LightmapInterpolateIndex");
        private static readonly int kLightmapSTID = Shader.PropertyToID("_LightmapST");        private static readonly int kLightmapInterpolateSTID = Shader.PropertyToID("_LightmapInterpolateST");
        private static readonly int[] kLightmapIDs = GetLightmapIDs("_Lightmap");        private static readonly int[] kLightmapInterpolateIDs = GetLightmapIDs("_Lightmap_Interpolate");

        private static readonly int kSpecCube0ID = Shader.PropertyToID("_SpecCube0"); private static readonly int kSpecCube0InterpolateID = Shader.PropertyToID("_SpecCube0_Interpolate");
        private static readonly int kSpecCube0IntensityID = Shader.PropertyToID("_SpecCube0_Intensity");

        private static readonly int kSHAr = Shader.PropertyToID("_SHAr"); private static readonly int kSHAg = Shader.PropertyToID("_SHAg"); private static readonly int kSHAb = Shader.PropertyToID("_SHAb");  
        private static readonly int kSHBr = Shader.PropertyToID("_SHBr"); private static readonly int kSHBg = Shader.PropertyToID("_SHBg");  private static readonly int kSHBb = Shader.PropertyToID("_SHBb");
        private static readonly int kSHC = Shader.PropertyToID("_SHC");

        public static void ApplyParameters(EnvironmentParameters _collection)
        {
            _collection.shData.OutputSH(out var _SHAr,out var _SHAg,out var _SHAb,out var _SHBr,out var _SHBg,out var _SHBb,out var _SHC);
            Shader.SetGlobalVector(kSHAr,_SHAr);
            Shader.SetGlobalVector(kSHAg,_SHAg);
            Shader.SetGlobalVector(kSHAb,_SHAb);
            Shader.SetGlobalVector(kSHBr,_SHBr);
            Shader.SetGlobalVector(kSHBg,_SHBg);
            Shader.SetGlobalVector(kSHBb,_SHBb);
            Shader.SetGlobalVector(kSHC,_SHC);
            Shader.SetGlobalFloat(kSpecCube0IntensityID,_collection.reflectionIntensity);
        }
        public void Apply(MeshRenderer[] _renderers)
        {
            if(m_EnvironmentReflection)
                Shader.SetGlobalTexture(kSpecCube0ID,m_EnvironmentReflection);

            // int lightmapCount = m_LightmapColors.Length;
            // Debug.Assert(lightmapCount <= kMaxLightmapCount ,"Lightmap Error: Lightmap Length Greater Than Max!");
            // for (int i = 0; i < lightmapCount; i++)
            //     Shader.SetGlobalTexture(kLightmapIDs[i], m_LightmapColors[i]);
        
            // for (int i = 0; i < _renderers.Length; i++)
            // {
            //     var param = m_Parameters[i];
            //     if(param.index == -1)
            //         continue;
            //
            //     var renderer = _renderers[i];
            //     renderer.material.SetVector(kLightmapSTID,param.scaleOffset);
            //     renderer.material.SetInt(kLightmapIndex,param.index);
            //     renderer.material.EnableKeyword(kLightmapKeyword,true);
            // }
            URender.EnableGlobalKeywords(kEnvironmentKeywords, 1);
            ApplyParameters(m_Parameters);
        }
        public static void Interpolate( MeshRenderer[] _renderers, EnvironmentCollection _collection1,EnvironmentCollection _collection2,float _interpolate)
        {
            if (_interpolate >= 1f - float.Epsilon)
            {
                _collection2.Apply(_renderers);
                return;
            }
            
            _collection1.Apply(_renderers);
            if (_interpolate <= float.Epsilon)
                return;

            ApplyParameters(EnvironmentParameters.Interpolate(_collection1.m_Parameters,_collection2.m_Parameters,_interpolate));
            URender.EnableGlobalKeywords(kEnvironmentKeywords, 2);
            Shader.SetGlobalFloat(kEnvironmentInterpolate,_interpolate);
            
            if(_collection2.m_EnvironmentReflection)
                Shader.SetGlobalTexture(kSpecCube0InterpolateID,_collection2.m_EnvironmentReflection);

            // int lightmapCount = _collection2.m_LightmapColors.Length;
            // for (int i = 0; i < lightmapCount; i++)
            //     Shader.SetGlobalTexture(kLightmapInterpolateIDs[i],_collection2.m_LightmapColors[i]);

            // for (int i = 0; i < _renderers.Length; i++)
            // {
            //     var renderer = _renderers[i];
            //     var parameter2 = _collection2.m_Parameters[i];
            //     if (parameter2.index != -1)
            //     {
            //         renderer.material.SetVector(kLightmapInterpolateSTID,parameter2.scaleOffset);
            //         renderer.material.SetInt(kLightmapInterpolateIndex,parameter2.index);
            //     }
            // }
        }
        public static void Dispose()
        {
            URender.EnableGlobalKeywords(kEnvironmentKeywords, -1);
            for (int i = 0; i < kMaxLightmapCount; i++)
            {
                Shader.SetGlobalTexture(kLightmapIDs[i],null);
                Shader.SetGlobalTexture(kLightmapInterpolateIDs[i],null);
            }
            Shader.SetGlobalTexture(kSpecCube0ID,null);
            Shader.SetGlobalTexture(kSpecCube0InterpolateID,null);
        }

    }
}
