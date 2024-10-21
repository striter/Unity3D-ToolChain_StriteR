using System.Collections.Generic;
using System.Linq.Extensions;
using UnityEditor.Extensions.EditorPath;
using UnityEditor.Extensions.ScriptableObjectBundle;
using UnityEngine;

namespace UnityEditor.Extensions.AssetPipeline.Model
{
    public class EAssetPostProcessPipeline : AssetPostprocessor
    {
        private IEnumerable<Y> Filter<Y>() where Y:AssetPostProcessRule
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(Y).Name}");
            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var directory = assetPath.GetPathDirectory();
                if (!assetPath.Contains(directory))
                    continue;

                var bundle = AssetDatabase.LoadAssetAtPath<AScriptableObjectBundle>(assetPath);
                foreach (var rule in bundle.m_Objects.CollectAs<ScriptableObject, Y>())
                    yield return rule;
            }
        }

        private void OnPostprocessModel(GameObject _gameObject)
        {
            foreach (var modelProcess in Filter<AModelProcess>())
            {
                modelProcess.Process(_gameObject);
                EditorUtility.SetDirty(_gameObject);
            }
            AssetDatabase.SaveAssetIfDirty(_gameObject);
        }
    }
}