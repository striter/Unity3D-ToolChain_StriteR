using System.Collections.Generic;
using System.Linq.Extensions;
using UnityEditor.Extensions.ScriptableObjectBundle;
using UnityEngine;

namespace UnityEditor.Extensions.AssetPipeline.Model
{
    public class EAssetPostProcessPipeline : AssetPostprocessor
    {
        private AssetProcessRules m_Rules;
        private static readonly string kRulesPath = "Assets/Settings/AssetProcessRules.asset";
        
        public EAssetPostProcessPipeline()
        {
            m_Rules = AssetDatabase.LoadAssetAtPath<AssetProcessRules>(kRulesPath);
            Debug.Assert(m_Rules, "AssetProcessRules Not Found At Path!" + kRulesPath);
        }

        private IEnumerable<Y> Filter<Y>() where Y:AssetPostProcessRule
        {
            if (!m_Rules)
                yield break;
            
            foreach (var rule in m_Rules.m_Objects.CollectAs<ScriptableObject, Y>())
            {
                if (!string.IsNullOrEmpty(rule.pathFilter))
                {
                    if (!assetPath.Contains(rule.pathFilter))
                        continue;
                }
                
                yield return rule;
            }
        }

        private void OnPostprocessModel(GameObject _gameObject)
        {
            foreach (var modelProcess in Filter<AModelProcess>())
            {
                modelProcess.Process(_gameObject);
                EditorUtility.SetDirty(_gameObject);
                AssetDatabase.SaveAssetIfDirty(_gameObject);
            }
        }
    }
}