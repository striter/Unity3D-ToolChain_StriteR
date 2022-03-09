using System.Collections;
using System.Collections.Generic;
using Rendering;
using UnityEngine;

namespace ExampleScenes.Rendering.GI
{
    public class GIExport : MonoBehaviour
    {
#if UNITY_EDITOR
            [ExtendButton("Export",nameof(ExportEnvironment))]
#endif
        public GIPersistent m_Persistent;
#if UNITY_EDITOR
        [ExtendButton("Export",nameof(ExportLightmapParameters))]
#endif
        public LightmapParameterCollection m_Lightmaps;

#if UNITY_EDITOR
        void ExportLightmapParameters()
        { 
                if (!TEditor.UEAsset.SaveFilePath(out var filePath, "asset", "LightmapParameters"))
                        return;
            
                var collection = UnityEditor.Editor.CreateInstance<LightmapParameterCollection>();
                collection.ExportFromScene(transform);
                m_Lightmaps = TEditor.UEAsset.CreateOrReplaceMainAsset(collection,TEditor.UEPath.FileToAssetPath(filePath));
        }

        void ExportEnvironment()
        {
                if (!TEditor.UEAsset.SaveFilePath(out string filePath, "asset", "Environment"))
                        return;
                GIPersistent renderData = ScriptableObject.CreateInstance<GIPersistent>();

                renderData.m_Environment = EnvironmentCollection.Export();
                var assetPath = TEditor.UEPath.FileToAssetPath(filePath);
                m_Persistent= TEditor.UEAsset.CreateOrReplaceMainAsset(renderData, assetPath);
        }
#endif
    }
}