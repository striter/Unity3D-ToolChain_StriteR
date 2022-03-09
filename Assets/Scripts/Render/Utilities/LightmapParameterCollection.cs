using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rendering
{
    public class LightmapParameterCollection:ScriptableObject
    {
        public LightmapParameter[] m_Parameters;
        #if UNITY_EDITOR
        public void ExportFromScene(Transform _rootTransform)
        {
            var renderers = _rootTransform.GetComponentsInChildren<MeshRenderer>();
            m_Parameters = new LightmapParameter[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
                m_Parameters[i] = new LightmapParameter()
                    {index = renderers[i].lightmapIndex, scaleOffset = renderers[i].lightmapScaleOffset};
        }
        #endif
        public void ApplyUnityParameters(Transform _rootTransform)=> ApplyUnityParameters(_rootTransform.GetComponentsInChildren<MeshRenderer>());
        public void ApplyUnityParameters(MeshRenderer[] _renderers)
        {
            int length = m_Parameters.Length;
            Debug.Assert(_renderers.Length == length,"Renderers Length Not Equals!");
            for (int i = 0; i < length; i++)
            {
                var renderer = _renderers[i];
                ref var param = ref m_Parameters[i];
                renderer.lightmapIndex = param.index;
                renderer.lightmapScaleOffset = param.scaleOffset;
            }
        }
    }

    [Serializable]
    public struct LightmapParameter
    {
        public int index;
        public Vector4 scaleOffset;
    }

    [Serializable]
    public struct EnvironmentCollection
    {
        public Texture2D[] m_LightmapColors;
        public Texture m_EnvironmentReflection;

        public Color m_SkyColor;
        public Color m_EquatorColor;
        public Color m_GroundColor;
        
        public static EnvironmentCollection Export()
        {
            Debug.Assert(LightmapSettings.lightmapsMode== LightmapsMode.NonDirectional,"Only none-directional mode supported");
            return new EnvironmentCollection()
            {
                m_LightmapColors = LightmapSettings.lightmaps.Select(p => p.lightmapColor).ToArray(),
                m_EnvironmentReflection =  RenderSettings.customReflection,
                m_SkyColor = RenderSettings.ambientSkyColor,
                m_EquatorColor = RenderSettings.ambientEquatorColor,
                m_GroundColor = RenderSettings.ambientGroundColor,
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

        private static readonly string[] kLightmapKeywords = {"LIGHTMAP_CUSTOM", "LIGHTMAP_INTERPOLATE"};
        private static readonly int kLightmapIndex = Shader.PropertyToID("_LightmapIndex");
        private static readonly int kLightmapSTID = Shader.PropertyToID("_LightmapST");
        private static readonly int[] kLightmapIDs = GetLightmapIDs("_Lightmap");
            
        private static readonly int kLightmapInterpolate = Shader.PropertyToID("_Lightmap_Interpolation");
        private static readonly int[] kLightmapInterpolateIDs = GetLightmapIDs("_Lightmap_Interpolate");
        

        private static readonly string[] kEnvironmentKeywords = {"ENVIRONMENT_CUSTOM", "ENVIRONMENT_INTERPOLATE"};
        private static readonly int kSpecCube0ID = Shader.PropertyToID("_SpecCube0");
        private static readonly int kSpecCube0InterpolateID = Shader.PropertyToID("_SpecCube0_Interpolate");

        private static readonly int kSHAr = Shader.PropertyToID("_SHAr");
        private static readonly int kSHAg = Shader.PropertyToID("_SHAg");
        private static readonly int kSHAb = Shader.PropertyToID("_SHAb");
        private static readonly int kSHBr = Shader.PropertyToID("_SHBr");
        private static readonly int kSHBg = Shader.PropertyToID("_SHBg");
        private static readonly int kSHBb = Shader.PropertyToID("_SHBb");
        private static readonly int kSHC = Shader.PropertyToID("_SHC");

        private static readonly float kY0 = .5f * Mathf.Sqrt(1f / UMath.PI);
        private static readonly float kY1 = Mathf.Sqrt(3f/(4f*UMath.PI));
        public void Apply(MeshRenderer[] _renderers,LightmapParameterCollection _parameterCollection)
        {
            URender.EnableGlobalKeywords(kEnvironmentKeywords, 1);
            //To do: Spherical Harmonics L2 Calculation
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = m_SkyColor;
            RenderSettings.ambientEquatorColor = m_EquatorColor;
            RenderSettings.ambientGroundColor = m_GroundColor;
            
            if(m_EnvironmentReflection)
                Shader.SetGlobalTexture(kSpecCube0ID,m_EnvironmentReflection);

            if (_parameterCollection == null)
                return;
            int lightmapCount = m_LightmapColors.Length;
              Debug.Assert(lightmapCount <= kMaxLightmapCount ,"Lightmap Error: Length Not Equals!");
            for (int i = 0; i < lightmapCount; i++)
                Shader.SetGlobalTexture(kLightmapIDs[i], m_LightmapColors[i]);
        
            for (int i = 0; i < _renderers.Length; i++)
            {
                var param = _parameterCollection.m_Parameters[i];
                if(param.index == -1)
                    continue;
            
                var renderer = _renderers[i];
                renderer.material.SetVector(kLightmapSTID,param.scaleOffset);
                renderer.material.SetInt(kLightmapIndex,param.index);
                renderer.material.EnableKeywords(kLightmapKeywords, 1);
            }
        }
        public static void Interpolate( MeshRenderer[] _renderers, EnvironmentCollection _collection1,EnvironmentCollection _collection2,float _interpolate,LightmapParameterCollection _parameterCollection)
        {
            if (_interpolate <= float.Epsilon)
            {
                _collection1.Apply(_renderers,_parameterCollection);
                return;
            }

            if (_interpolate >= 1f - float.Epsilon)
            {
                _collection2.Apply(_renderers,_parameterCollection);
                return;
            }
            
            URender.EnableGlobalKeywords(kEnvironmentKeywords, 2);
            if(_collection1.m_EnvironmentReflection)
                Shader.SetGlobalTexture(kSpecCube0ID,_collection1.m_EnvironmentReflection);
            if(_collection2.m_EnvironmentReflection)
                Shader.SetGlobalTexture(kSpecCube0InterpolateID,_collection2.m_EnvironmentReflection);

            int lightmapCount = _collection1.m_LightmapColors.Length;
            Shader.SetGlobalFloat(kLightmapInterpolate,_interpolate);
            for (int i = 0; i < lightmapCount; i++)
            {
                Shader.SetGlobalTexture(kLightmapIDs[i],_collection1.m_LightmapColors[i]);
                Shader.SetGlobalTexture(kLightmapInterpolateIDs[i],_collection2.m_LightmapColors[i]);
            }

            if (_parameterCollection == null)
                return;
            Debug.Assert(_renderers.Length == _parameterCollection.m_Parameters.Length,"Lightmap Error: Length Not Equals!");
            Debug.Assert(_collection1.m_LightmapColors.Length == _collection2.m_LightmapColors.Length,"Lightmap Error: Diff Interpolate Textures Length!");
            
            for (int i = 0; i < _renderers.Length; i++)
            {
                var param = _parameterCollection.m_Parameters[i];
                if (param.index == -1)
                    continue;
                
                var renderer = _renderers[i];
                var index = param.index;
                renderer.material.SetVector(kLightmapSTID,param.scaleOffset);
                renderer.material.SetInt(kLightmapIndex,param.index);
                renderer.material.EnableKeywords(kLightmapKeywords, 2);
            }
        }

        public static void Dispose()
        {
            URender.EnableGlobalKeywords(kLightmapKeywords, -1);
            URender.EnableGlobalKeywords(kEnvironmentKeywords, -1);
            for(int i=0;i<kMaxLightmapCount;i++)
                Shader.SetGlobalTexture(kLightmapIDs[i],null);
            Shader.SetGlobalTexture(kSpecCube0ID,null);
            Shader.SetGlobalTexture(kSpecCube0InterpolateID,null);
        }
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
}
