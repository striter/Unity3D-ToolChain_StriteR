using System;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace UnityEditor.Extensions.AssetPipeline
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
        
        private void OnPostprocessModel(GameObject _object)
        {
            if (!m_Rules)
                return;
            
            var meshRules = m_Rules.meshRules;
            if (meshRules == null || meshRules.Length == 0)
                return;
            
            var compatibleRules = meshRules.Collect(p => assetPath.Contains(p.m_Directory)).ToList();
            if (compatibleRules.Count == 0)
                return;
            
            var meshFilters = _object.GetComponentsInChildren<MeshFilter>();
            foreach (var meshFilter in meshFilters)
            {
                foreach (var rules in compatibleRules)
                    rules.ProcessMesh(meshFilter.sharedMesh);
            }
            
            EditorUtility.SetDirty(_object);
            AssetDatabase.SaveAssetIfDirty(_object);
        }
    }
}