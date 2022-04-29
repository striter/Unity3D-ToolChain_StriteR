using System;
using System.Linq;
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
    
    [Serializable]
    public class EnvironmentCollection
    {
        // public LightmapParameter[] m_Parameters;
        // public Texture2D[] m_LightmapColors;
        public Texture m_EnvironmentReflection;

        public SHL2Data m_SHData;
        public void Export(Transform _rootTransform)
        {
            // Debug.Assert(LightmapSettings.lightmapsMode== LightmapsMode.NonDirectional,"Only none-directional mode supported");
            // m_Parameters = _rootTransform.GetComponentsInChildren<MeshRenderer>().Select(p => new LightmapParameter()
            //     {index = p.lightmapIndex, scaleOffset = p.lightmapScaleOffset}).ToArray();
            // m_LightmapColors = LightmapSettings.lightmaps.Select(p => p.lightmapColor).ToArray();
            m_EnvironmentReflection = RenderSettings.customReflection;
            m_SHData = SphericalHarmonicsExport.ExportL2Gradient(4096,RenderSettings.ambientSkyColor,RenderSettings.ambientEquatorColor,RenderSettings.ambientGroundColor);
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

        private static readonly int kSHAr = Shader.PropertyToID("_SHAr");        private static readonly int kSHArL = Shader.PropertyToID("_SHArL");
        private static readonly int kSHAg = Shader.PropertyToID("_SHAg");        private static readonly int kSHAgL = Shader.PropertyToID("_SHAgL"); 
        private static readonly int kSHAb = Shader.PropertyToID("_SHAb");        private static readonly int kSHAbL = Shader.PropertyToID("_SHAbL");
        private static readonly int kSHBr = Shader.PropertyToID("_SHBr");        private static readonly int kSHBrL = Shader.PropertyToID("_SHBrL");
        private static readonly int kSHBg = Shader.PropertyToID("_SHBg");        private static readonly int kSHBgL = Shader.PropertyToID("_SHBgL");
        private static readonly int kSHBb = Shader.PropertyToID("_SHBb");        private static readonly int kSHBbL = Shader.PropertyToID("_SHBbL");
        private static readonly int kSHC = Shader.PropertyToID("_SHC");          private static readonly int kSHCL = Shader.PropertyToID("_SHCL");

        public void Apply(MeshRenderer[] _renderers)
        {
            //To do: Spherical Harmonics L2 Calculation
            m_SHData.OutputSH(out var _SHAr,out var _SHAg,out var _SHAb,out var _SHBr,out var _SHBg,out var _SHBb,out var _SHC);
            Shader.SetGlobalVector(kSHAr,_SHAr);
            Shader.SetGlobalVector(kSHAg,_SHAg);
            Shader.SetGlobalVector(kSHAb,_SHAb);
            Shader.SetGlobalVector(kSHBr,_SHBr);
            Shader.SetGlobalVector(kSHBg,_SHBg);
            Shader.SetGlobalVector(kSHBb,_SHBb);
            Shader.SetGlobalVector(kSHC,_SHC);

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

            URender.EnableGlobalKeywords(kEnvironmentKeywords, 2);
            Shader.SetGlobalFloat(kEnvironmentInterpolate,_interpolate);
            
            _collection2.m_SHData.OutputSH(out var _SHAr,out var _SHAg,out var _SHAb,out var _SHBr,out var _SHBg,out var _SHBb,out var _SHC);
            Shader.SetGlobalVector(kSHArL,_SHAr);
            Shader.SetGlobalVector(kSHAgL,_SHAg);
            Shader.SetGlobalVector(kSHAbL,_SHAb);
            Shader.SetGlobalVector(kSHBrL,_SHBr);
            Shader.SetGlobalVector(kSHBgL,_SHBg);
            Shader.SetGlobalVector(kSHBbL,_SHBb);
            Shader.SetGlobalVector(kSHCL,_SHC);
            
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
