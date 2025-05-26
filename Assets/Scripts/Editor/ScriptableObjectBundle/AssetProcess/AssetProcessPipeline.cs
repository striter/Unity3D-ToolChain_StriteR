using System.Linq.Extensions;
using UnityEditor.Extensions.EditorPath;
using UnityEngine;

namespace UnityEditor.Extensions.AssetProcess
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
                
                var importer = AssetImporter.GetAtPath(assetPath);
                foreach (var process in bundle.m_Objects.CollectAs<ScriptableObject, Process>())
                        if(_object == null ? process.Preprocess(importer) : process.Postprocess(_object))
                            importer.SaveAndReimport();
            }
        }

        private void OnPreprocessModel() => ProcessRulesBundle<AModelProcess>(null);
        private void OnPostprocessModel(GameObject _modelRoot) => ProcessRulesBundle<AModelProcess>(_modelRoot);
        private void OnPreprocessTexture() => ProcessRulesBundle<ATextureProcess>(null);
        private void OnPostprocessTexture(Texture2D _texture)=> ProcessRulesBundle<ATextureProcess>(_texture);
        private void OnPreprocessAnimation() => ProcessRulesBundle<AAnimationProcess>(null);
        private void OnPostprocessAnimation(GameObject root, AnimationClip clip) => ProcessRulesBundle<AAnimationProcess>(clip);
        
    }
}