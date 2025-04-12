    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    public static class URenderDebug
    {
        public static ScriptableRendererData[] rendererDataList { get; private set; }
        private static int m_DefaultRendererIndex;
        private static bool m_Initialized = false;
        private static bool Valid
        {
            get
            {
                if (m_Initialized)
                    return rendererDataList is { Length: > 0 };
                
                m_Initialized = true;
                var pipelineAsset = GraphicsSettings.renderPipelineAsset;
                if (pipelineAsset == null || pipelineAsset is not UniversalRenderPipelineAsset urpAsset)
                    return Valid;
            
                rendererDataList = UReflection.GetFieldValue<ScriptableRendererData[]>(urpAsset,"m_RendererDataList",BindingFlags.Instance | BindingFlags.NonPublic);
                m_DefaultRendererIndex = UReflection.GetFieldValue<int>(urpAsset, "m_DefaultRendererIndex");
                return Valid;
            }
        }

        public static ScriptableRenderer GetScriptableRenderer(Camera _camera)
        {
            var cameraData = _camera.GetUniversalAdditionalCameraData();
            return cameraData != null ? cameraData.scriptableRenderer as UniversalRenderer : null;
        }

        public static ScriptableRendererData GetScriptableRendererData(int _index)
        {
            if (!Valid)
                return null;

            var index =  _index == -1 ? m_DefaultRendererIndex : _index;
            if (index < 0 || index >= rendererDataList.Length)
            {
                Debug.LogError($"[URenderDebug] Unable to access Invalid Renderer Index {index}");
                return null;
            }
            
            return rendererDataList[index];
        }

        public static ScriptableRendererData GetScriptableRendererData(Camera _camera)
        {
            if (!Valid)
                return null;
            
            var cameraData = _camera.GetUniversalAdditionalCameraData();
            return GetScriptableRendererData(UReflection.GetFieldValue<int>(cameraData, "m_RendererIndex"));
        }

        static IEnumerable<ScriptableRendererFeature> GetAllFeatures(string _typeName)
        {
            if(!Valid)
                yield break;
            
            foreach (var rendererData in rendererDataList)
            {
                if (rendererData == null || rendererData.rendererFeatures == null) 
                    continue;

                foreach (var feature in GetFeatures(rendererData, _typeName))
                    yield return feature;
            }
        }
        
        static IEnumerable<ScriptableRendererFeature> GetFeatures(ScriptableRendererData rendererData,string _typeName)
        {
            
            for (var i = rendererData.rendererFeatures.Count - 1; i >= 0; i--)
            {
                var feature = rendererData.rendererFeatures[i];
                if (feature == null)
                {
                    Debug.LogError($"Null Feature Found At {rendererData.name} : Index{i}");
                    continue;
                }

                if (feature.GetType().Name != _typeName)
                    continue;
                    
                yield return feature;
            }
        }

        public static bool SetAllFeatureActive(string _featureType, bool _active)
        {
            if (!Valid)
                return false;

            var any = false;
            foreach (var feature in GetAllFeatures(_featureType))
            {
                any = true;
                feature.SetActive(_active);
            }

            return any;
        }
        
        public static bool SetFeatureActive(Camera _camera, string _featureName, bool _active)
        {
            if (!Valid)
                return false;

            var any = false;
            foreach (var feature in GetFeatures(GetScriptableRendererData(_camera), _featureName))
            {
                any = true;
                feature.SetActive(_active);
            }
            
            return any;
        }

        public static bool GetFeatureActive(Camera _camera,string _featureName)
        {
            if (Valid)
                return false;

            foreach (var feature in GetFeatures(GetScriptableRendererData(_camera),_featureName))
                return feature.isActive;
            
            return false;
        }
        
        public static ScriptableRendererFeature GetFeature(Camera _camera,string _featureName)
        {
            if (!Valid)
                return null;
            
            foreach (var feature in GetFeatures(GetScriptableRendererData(_camera),_featureName))
                return feature;
            
            return null;
        }
    }
