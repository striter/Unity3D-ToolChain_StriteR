using System;
using System.Collections.Generic;
using System.Linq.Extensions;
using UnityEditor.Extensions.AssetPipeline.Process;
using UnityEditor.Extensions.EditorPath;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityEditor.Extensions.AssetPipeline
{
    public class AssetProcessPipeline : AssetPostprocessor
    {
        private void ProcessRulesBundle<Process>(UnityEngine.Object _object) where Process:AAssetProcess
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(Process).Name}");
            foreach (var guid in guids)
            {
                var bundleAssetPath = AssetDatabase.GUIDToAssetPath(guid);
                var directory = bundleAssetPath.GetPathDirectory();
                if (!assetPath.Contains(directory))
                    continue;

                var bundle = AssetDatabase.LoadAssetAtPath<AssetProcessBundle>(bundleAssetPath);
                if(!bundle.m_Enable)
                    continue;
                
                foreach (var process in bundle.m_Objects.CollectAs<ScriptableObject, Process>())
                    if (_object == null)
                    {
                        var importer = AssetImporter.GetAtPath(assetPath);
                        if( process.Preprocess(importer))
                            importer.SaveAndReimport();
                    }
                    else
                    { 
                        if (process.PostProcess(_object))
                            EditorUtility.SetDirty(_object);
                    }
            }
            if (_object != null)
                AssetDatabase.SaveAssetIfDirty(_object);
        }

        private void OnPreprocessModel() => ProcessRulesBundle<AModelProcess>(null);
        private void OnPostprocessModel(GameObject _modelRoot) => ProcessRulesBundle<AModelProcess>(_modelRoot);
        private void OnPreprocessTexture() => ProcessRulesBundle<ATextureProcess>(null);
        private void OnPostprocessTexture(Texture2D _texture)=> ProcessRulesBundle<ATextureProcess>(_texture);

        // private void OnPostprocessAnimation(Animation _animation)
        // {
        //     foreach (var animationProcess in ProcessRulesBundle<AAnimationProcess>())
        //     {
        //         animationProcess.importer = AssetImporter.GetAtPath(assetPath) as AnimationImporter;
        //         animationProcess.Process(_animation);
        //         animationProcess.importer.SaveAndReimport();
        //         EditorUtility.SetDirty(_animation);
        //     }
        //     
        //     AssetDatabase.SaveAssetIfDirty(_animation);
        // }
    }
}